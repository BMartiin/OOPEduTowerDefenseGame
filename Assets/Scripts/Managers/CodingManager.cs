using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Jint;
using System;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using Unity.VisualScripting;

public class CodingManager : MonoBehaviour
{
    [Header("UI hivatkozasok")]
    public GameObject puzzlePanel;
    public TMP_Text taskDescriptionText;
    public TMP_InputField inputField;
    public TMP_Text testStatusLabel;
    public TMP_Text goldCountText;

    [Header("Adatok")]
    public GameObject knightPrefab;
    public GameObject archerPrefab;
    public GameObject enemyPrefab;
    public GameObject healthBar;
    public List<Transform> knightSpawnPoints;
    public List<Transform> enemySpawnPoints;

    private List<GameObject> playerArmy = new List<GameObject>();

    [Header("Szint Adatok")]
    public LevelData currentLevel;

    [Header("Managers")]
    public ScoreManager scoreManager;
    public GameOverManager gameOverManager;
    public TutorialManager tutorialManager;

    private Engine engine;
    private int currentQuestionIndex = 0;
    private int internalKnightCount = 0;
    private int internalArcherCount = 0;
    private int lastPowerValue = 0;
    private int goldCount = 0;
    private bool[] completedQuestions;
    private string[] rightAnswers;

    private void Start()
    {
        LevelData loadedLevel = Resources.Load<LevelData>("Levels/" + MenuManager.LevelToLoad);

        if (loadedLevel != null)
        {
            currentLevel = loadedLevel;
        }
        else
        {
            Debug.LogError("Nincs ilyen nevu szint: " + MenuManager.LevelToLoad);
            return;
        }

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
            internalKnightCount++;
            int power = 1;
            if (p != null) int.TryParse(p.ToString(), out power);
            lastPowerValue = power;
        }));

        engine.SetValue("ijasz", new Action<object>((p) => {
            internalArcherCount++;
            int power = 1;
            if (p != null) int.TryParse(p.ToString(), out power);
            lastPowerValue = power;
        }));

        engine.SetValue("arany", 100);
        SelectQuestion(0);
    }

    public void SelectQuestion(int index)
    {
        if (index >= 0 && index < currentLevel.Questions.Count)
        {

            currentQuestionIndex = index;
            var q = currentLevel.Questions[index];
            taskDescriptionText.text = q.Description;

            if (completedQuestions[index])
            {
                inputField.text = rightAnswers[index];
                inputField.interactable = false;
                testStatusLabel.text = "SIKER!";
                testStatusLabel.color = Color.green;
                tutorialManager.SwimOut();
            }
            else
            {
                inputField.text = "";
                inputField.interactable = true;
                testStatusLabel.text = "H/N";
                testStatusLabel.color = Color.white;
                tutorialManager.ShowTutorial(q.Theory);
            }
        }
    }

    public void CheckCode()
    {
        if (completedQuestions[currentQuestionIndex]) 
        {
            return;
        } 

        internalKnightCount = 0;
        internalArcherCount = 0;
        lastPowerValue = 0;
        var currentQ = currentLevel.Questions[currentQuestionIndex];
        bool requiresJint = currentQ.RequiresJint;

        if (requiresJint) {
            try
            {
                string codeToExecute = PreprocessCodeForJint(inputField.text);
                engine.Execute(codeToExecute);
            }
            catch (Exception ex)
            {
                string errorMessage = "Ifjú tanítványom, hibát ejtettél a kódodban!: \n\n" + TranslateErrorMessage(ex.Message);
                tutorialManager.ShowTutorial(errorMessage);

                testStatusLabel.text = "HIBA";
                testStatusLabel.color = Color.red;

                return;
            }
        }
        
        ValidateResult();
    }

    private string PreprocessCodeForJint(string rawCode)
    {
        return Regex.Replace(rawCode, @"\b(int|float|double|string|bool|char|decimal|long|short|byte)\b", "var");
    }

    private void ValidateResult()
    {
        var currentQ = currentLevel.Questions[currentQuestionIndex];

        bool logicOk = TestLogic(currentQ);
        bool syntaxOk = TestSyntax(currentQ);

        if (logicOk && syntaxOk)
        {
            HandleSuccess();
        }
        else
        {
            HandleFailure(logicOk, syntaxOk);
        }
    }

    private string TranslateErrorMessage(string rawMessage)
    {
        if (rawMessage.Contains("is not defined"))
        {
            return "Olyan nevet használtál, amit nem tanultunk meg! " + "(" + rawMessage + ")";
        }
        else if (rawMessage.Contains("Unexpected token"))
        {
            return "Úgy látom, elfelejtettél egy írásjelet (meg van minden ; és zárójel?) " + "(" + rawMessage + ")";
        }
        return "Sajnos hibás a kód: " + "(" + rawMessage + ")";
    }

    private bool TestLogic(QuestionData q)
    {
        if (!q.RequiresJint)
        {
            return true;
        }

        if (internalKnightCount != q.RequiredKnightCount)
        {
            return false;
        }

        if (internalArcherCount != q.RequiredArcherCount)
        {
            return false;
        }

        if (q.RequiredPower > 0 && lastPowerValue != q.RequiredPower)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(q.RequiredVarName))
        {
            var jsValue = engine.GetValue(q.RequiredVarName);

            if (jsValue == Jint.Native.JsValue.Undefined)
            {
                return false;
            }

            if (jsValue.AsNumber() != q.ExpectedVarValue)
            {
                return false;
            }
        }

        return true;
    }

    private bool TestSyntax(QuestionData q)
    {
        if (string.IsNullOrEmpty(q.RequiredRegexPattern)) 
            return true;

        return Regex.IsMatch(inputField.text, q.RequiredRegexPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
    }

    private void HandleSuccess()
    {
        completedQuestions[currentQuestionIndex] = true;
        rightAnswers[currentQuestionIndex] = inputField.text;
        inputField.interactable = false;

        goldCount++;
        UpdateGoldCount();

        var currentQ = currentLevel.Questions[currentQuestionIndex];
        if (currentQ.RewardUnit == UnitReward.Lovag) playerArmy.Add(knightPrefab);
        else if (currentQ.RewardUnit == UnitReward.Ijasz) playerArmy.Add(archerPrefab);

        testStatusLabel.text = "SIKER!";
        testStatusLabel.color = Color.green;

        tutorialManager.ShowTutorial("Szép munka! Újabb katonát szereztél! Így tovább ifjú tanítványom!");
    }

    private void HandleFailure(bool logicOk, bool syntaxOk)
    {
        if (!syntaxOk)
        {
            tutorialManager.ShowTutorial("A kódod formailag nem megfelelõ ifjú tanítványom. Figyelj a kulcsszavakra, szóközökre és a pontos szintaktikára!");
        }
        else if (!logicOk)
        {
            tutorialManager.ShowTutorial("A kódod lefutott, de logikailag nem azt csinálja, amit kértem. Ellenõrizd a változókat és a hívásokat!");
        }

        testStatusLabel.text = "HIBA";
        testStatusLabel.color = Color.red;
    }

    public void StartBattle()
    {
        SpawnEnemyWithHealthBar();
        SpawnPlayerArmyWithHealthBar();

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
        completedQuestions = new bool[currentLevel.Questions.Count];
        rightAnswers = new string[currentLevel.Questions.Count];
    }

    public void EndGame(byte losingTeamID)
    {
        Time.timeScale = 0f;

        int finalScore = scoreManager.CalculateFinalScore(goldCount);

        Debug.Log($"[EndGame] Lefutott! Gyûjtött arany: {goldCount} -> Kalkulált végsõ pont: {finalScore}");

        NetworkManager netManager = UnityEngine.Object.FindFirstObjectByType<NetworkManager>();
        if (netManager != null)
        {
            string currentUserId = netManager.GetActiveUserId();
            string currentToken = netManager.GetActiveToken();

            Debug.Log($"[EndGame] Azonosítók lekérve. UserID: '{currentUserId}', Token: '{currentToken}'");

            BattleResultData resultData = new BattleResultData
            {
                userId = currentUserId,
                token = currentToken,
                finalScore = finalScore
            };

            Debug.Log("[EndGame] Adatok csomagolása kész. Átadás a NetworkManager-nek");
            netManager.SendResults(resultData);
        }
        else
        {
            Debug.LogWarning("[EndGame HIBA] Nincs NetworkManager a pályán!");
        }

        string status;
        Color statusColor;

        if (losingTeamID == 0)
        {
            status = "VESZTETTÉL!";
            statusColor = Color.red;
            AudioManager.Instance.PlaySFX(AudioManager.Instance.errorSound);
        }
        else
        {
            status = "GYÕZELEM!";
            statusColor = Color.green;
            AudioManager.Instance.PlaySFX(AudioManager.Instance.successSound);
        }

        gameOverManager.Setup(status, goldCount, finalScore, statusColor);
    }
    private void SpawnPlayerArmyWithHealthBar()
    {
        int unitsToSpawn = Mathf.Min(playerArmy.Count, knightSpawnPoints.Count);

        for (int i = 0; i < unitsToSpawn; i++)
        {
            GameObject prefabToSpawn = playerArmy[i];

            GameObject newUnit = Instantiate(prefabToSpawn, knightSpawnPoints[i].position, Quaternion.Euler(0, 180, 0));
            Vector3 healthBarPosition = newUnit.transform.position + new Vector3(0, 1.4f, 0);
            GameObject newBar = Instantiate(healthBar, healthBarPosition, Quaternion.identity);

            newBar.transform.SetParent(newUnit.transform);

            Unit unitScript = newUnit.GetComponent<Unit>();
            unitScript.healthFillImage = newBar.transform.Find("Fill").GetComponent<Image>();
        }
    }

    private void SpawnEnemyWithHealthBar()
    {
        int enemiesToSpawn = Mathf.Min(currentLevel.EnemyCount, enemySpawnPoints.Count);

        for (int i = 0; i< currentLevel.EnemyCount; i++)
        {
            GameObject newEnemy = Instantiate(enemyPrefab, enemySpawnPoints[i].position, Quaternion.identity);
            Vector3 healthBarPosition = newEnemy.transform.position + new Vector3(0, 1.4f, 0);
            GameObject newBar = Instantiate(healthBar, healthBarPosition, Quaternion.identity);

            newBar.transform.SetParent(newEnemy.transform);

            Unit unitScript = newEnemy.GetComponent<Unit>();
            unitScript.healthFillImage = newBar.transform.Find("Fill").GetComponent<Image>();
        
        }
    }
}