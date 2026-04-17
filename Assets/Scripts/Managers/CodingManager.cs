using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Jint;
using System;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class CodingManager : MonoBehaviour
{
    [Serializable]
    public struct QuestionData
    {
        public string Description;
        [TextArea(3, 10)]
        public string Theory;
        public int RequiredActionCount;
        public int RequiredPower;
        public string RequiredVarName;
        public int ExpectedVarValue;
        [Header("Regex C# Szintaktika")]
        public string RequiredRegexPattern;

        public bool RequiresJint
        {
            get
            {
                return RequiredActionCount > 0
                    || RequiredPower > 0
                    || !string.IsNullOrEmpty(RequiredVarName);
            }
        }
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
    public TutorialManager tutorialManager;

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
                tutorialManager.SwimOut();
            }
            else
            {
                inputField.text = "";
                inputField.interactable = true;
                testStatusLabel.text = "H/N";
                testStatusLabel.color = Color.white;
                tutorialManager.ShowTutorial(questions[index].Theory);
            }
        }
    }

    public void CheckCode()
    {
        if (completedQuestions[currentQuestionIndex]) 
        {
            return;
        } 

        internalCodeActionCount = 0;
        lastPowerValue = 0;
        var currentQ = questions[currentQuestionIndex];
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
        var currentQ = questions[currentQuestionIndex];

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
        if (rawMessage.Contains("is not defined")) return "Olyan nevet használtál, amit nem tanultunk meg! " + "(" + rawMessage + ")";
        else if (rawMessage.Contains("Unexpected token")) return "Úgy látom, elfelejtettél egy írásjelet (meg van minden ; és zárójel?) " + "(" + rawMessage + ")";
        return "Sajnos hibás a kód: " + "(" + rawMessage + ")";
    }

    private bool TestLogic(QuestionData q)
    {
        if (!q.RequiresJint)
        {
            return true;
        }
        
        if (q.RequiredActionCount > 0 && internalCodeActionCount != q.RequiredActionCount)
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
        if (string.IsNullOrEmpty(q.RequiredRegexPattern)) return true;

        return Regex.IsMatch(inputField.text, q.RequiredRegexPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
    }

    private void HandleSuccess()
    {
        completedQuestions[currentQuestionIndex] = true;
        rightAnswers[currentQuestionIndex] = inputField.text;
        inputField.interactable = false;

        goldCount++;
        UpdateGoldCount();

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
            netManager.SendResults(resultData);
        }
        else
        {
            Debug.LogWarning("Nincs network manager a pályán");
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

        for(int i = 0; i<= enemySpawnPoints.Count; i++)
        {
            if(i < enemySpawnPoints.Count)
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
}