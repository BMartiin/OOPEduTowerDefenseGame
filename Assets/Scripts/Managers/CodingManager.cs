using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Jint;
using System;
using UnityEngine.UI;

public class CodingManager : MonoBehaviour
{
    [Serializable]
    public struct QuestionData
    {
        public string Description;
    }

    [Header("UI hivatkozasok")]
    public GameObject puzzlePanel;
    public TMP_Text taskDescriptionText;
    public TMP_InputField inputField;
    public TMP_Text testStatusLabel;
    public TMP_Text goldCountText;

    [Header("Adatok")]
    public List<QuestionData> questions;
    public GameObject knightPrefab;
    public GameObject enemyPrefab;
    public GameObject healthBar;
    public List<Transform> knightSpawnPoints;
    public List<Transform> enemySpawnPoints;

    [Header("Managers")]
    public ScoreManager scoreManager;
    public GameOverManager gameOverManager;
    //private float battleStartTime;
    //private List<Unit> spawnedKnights = new List<Unit>();

    private Engine engine;
    private int currentQuestionIndex = 0;
    private int internalCodeActionCount = 0;
    private int lastPowerValue = 0;
    private int goldCount = 0;
    private bool[] completedQuestions;
    private string[] rightAnswers;

    private void Start()
    {
        Time.timeScale = 0.0f;
        puzzlePanel.SetActive(true);
        gameOverManager.panel.SetActive(false);

        RefreshAnswers();
        InitEngine();
        UpdateGoldCount();
    }

    private void InitEngine()
    {
        engine = new Engine();

        engine.SetValue("lovag", new Action<object>((p) => {
            internalCodeActionCount++;
            int power = 1;
            if (p != null) int.TryParse(p.ToString(), out power);
            lastPowerValue = power;
        }));

        engine.SetValue("arany", 100);
        SelectQuestion(0);
    }

    public void SelectQuestion(int index)
    {
        if (index >= 0 && index < questions.Count)
        {

            currentQuestionIndex = index;
            taskDescriptionText.text = questions[index].Description;

            if (completedQuestions[index])
            {
                inputField.text = rightAnswers[index];
                inputField.interactable = false;
                testStatusLabel.text = "SIKER!";
                testStatusLabel.color = Color.green;
            }
            else
            {
                inputField.text = "";
                inputField.interactable = true;
                testStatusLabel.text = "H/N";
                testStatusLabel.color = Color.white;
            }
        }
    }

    public void CheckCode()
    {
        if (completedQuestions[currentQuestionIndex]) return;

        internalCodeActionCount = 0;
        lastPowerValue = 0;

        try
        {
            engine.Execute(inputField.text);
            ValidateResult();
        }
        catch (Exception ex)
        {
            testStatusLabel.text = "HIBA: " + ex.Message;
            testStatusLabel.color = Color.red;
        }
    }

    private void ValidateResult()
    {
        bool success = false;

        switch (currentQuestionIndex)
        {
            case 0: success = (internalCodeActionCount == 1); break;
            case 1: success = (internalCodeActionCount == 3); break;
            case 2: success = (internalCodeActionCount == 1); break;
            case 3: success = (lastPowerValue == 10); break;
            case 4: success = (internalCodeActionCount == 2); break;
        }

        if (success)
        {
            completedQuestions[currentQuestionIndex] = true;
            rightAnswers[currentQuestionIndex] = inputField.text;
            inputField.interactable = false;

            goldCount++;
            UpdateGoldCount();

            testStatusLabel.text = "SIKER!";
            testStatusLabel.color = Color.green;
        }
        else
        {
            testStatusLabel.text = "HIBA";
            testStatusLabel.color = Color.red;
        }
    }

    public void StartBattle()
    {
        SpawnEnemyWithHealthBar();
        SpawnKnightsWithHealthBar();
        
        Time.timeScale = 1.0f;
        puzzlePanel.SetActive(false);
    }

    public void SkipAndStart()
    {
        Time.timeScale = 1.0f;
        puzzlePanel.SetActive(false);
    }

    private void UpdateGoldCount()
    {
        goldCountText.text = goldCount.ToString();
    }

    private void RefreshAnswers()
    {
        completedQuestions = new bool[questions.Count];
        rightAnswers = new string[questions.Count];
    }

    public void EndGame(byte losingTeamID)
    {
        Time.timeScale = 0f;

        int finalScore = scoreManager.CalculateFinalScore(goldCount);

        NetworkManager netManager = UnityEngine.Object.FindFirstObjectByType<NetworkManager>();
        if (netManager != null)
        {
            BattleResultData resultData = new BattleResultData
            {
                userId = netManager.editorTestUserId,
                token = netManager.editorTestToken,
                finalScore = finalScore
            };
            netManager.SendResultsMock(resultData);
        }
        else
        {
            Debug.LogWarning("Nincs network manager a pályán dilo");
        }

        string status = (losingTeamID == 0) ? "VESZTETTÉL!" : "GYÕZELEM!";
        Color statusColor = (losingTeamID == 0) ? Color.red : Color.green;

        gameOverManager.Setup(status, goldCount, finalScore, statusColor);
    }

    private void SpawnKnightsWithHealthBar()
    {
        for (int i = 0; i < goldCount; i++)
        {
            if (i < knightSpawnPoints.Count)
            {
                GameObject newKnight = Instantiate(knightPrefab, knightSpawnPoints[i].position, Quaternion.Euler(0,180,0));
                Vector3 healthBarPosition = newKnight.transform.position + new Vector3(0, 1.4f, 0);
                GameObject newBar = Instantiate(healthBar, healthBarPosition, Quaternion.identity);

                newBar.transform.SetParent(newKnight.transform);

                Unit unitScript = newKnight.GetComponent<Unit>();
                unitScript.healthFillImage = newBar.transform.Find("Fill").GetComponent<Image>();
            }
        }
    }

    private void SpawnEnemyWithHealthBar()
    {
        GameObject newEnemy = Instantiate(enemyPrefab, enemySpawnPoints[0].position, Quaternion.identity);
        Vector3 healthBarPosition = newEnemy.transform.position + new Vector3(0, 1.4f, 0);
        GameObject newBar = Instantiate(healthBar, healthBarPosition, Quaternion.identity);

        newBar.transform.SetParent(newEnemy.transform);

        Unit unitScript = newEnemy.GetComponent<Unit>();
        unitScript.healthFillImage = newBar.transform.Find("Fill").GetComponent<Image>();
    }
}