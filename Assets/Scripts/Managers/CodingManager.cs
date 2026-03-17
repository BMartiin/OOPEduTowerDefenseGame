using UnityEngine;
using TMPro; //need this for TextMeshPro things
using System.Collections.Generic;
using DynamicExpresso;
using DynamicExpresso.Exceptions;

public class CodingManager : MonoBehaviour
{
    [System.Serializable]
    public struct QuestionData
    {
        public string Description;
        public List<string> Answers;
    }

    [Header ("UI hivatkozasok")]
    public GameObject puzzlePanel;
    public TMP_Text taskDescriptionText;
    public TMP_InputField inputField;
    public TMP_Text testStatusLabel;
    public int currentGoldCount = 0; //needed for later

    [Header("Adatok")]
    public List<QuestionData> questions;
    public GameObject knightPrefab;
    public Transform spawnPoint;

    private int currentQuestionIndex = 0;
    private bool canSpawn = false;

    //for code int.
    private Interpreter interpreter;

    private void Start()
    {
        Time.timeScale = 0.0f;
        puzzlePanel.SetActive(true);

        interpreter = new Interpreter();

        interpreter.SetFunction("lovag", new System.Action(SpawnKnight));

        SelectQuestion(0);
    }

    public void SelectQuestion(int index)
    {
        if (index >= 0 && index < questions.Count)
        {
            currentQuestionIndex = index;

            taskDescriptionText.text = questions[index].Description;

            inputField.text = "";

            testStatusLabel.text = "H/N";
            testStatusLabel.color = Color.white;
            canSpawn = false;
        }
    }

    public void CheckCode()
    {
        string playerInput = inputField.text;

        try
        {
            interpreter.Eval(playerInput);
            testStatusLabel.text = "SIKER!";
            testStatusLabel.color = Color.green;

            canSpawn = true;
        }
        catch(ParseException ex)
        {
            testStatusLabel.text = "HIBA: " + ex.Message;
            testStatusLabel.color = Color.red;
        }

    }

    public void SpawnKnight()
    {
        if (canSpawn)
        {
            //need this so the knight doesn't moonwalk
            Quaternion rightRotation = Quaternion.Euler(0, 180, 0);
            Instantiate(knightPrefab, spawnPoint.position, rightRotation);
            Debug.Log("Knight Spawned");
        }
    }

    public void StartBattle()
    {
        Time.timeScale = 1.0f;
        puzzlePanel.SetActive(false);
    }

    public void SkipAndStart()
    {
        Time.timeScale = 1.0f;
        puzzlePanel.SetActive(false);
    }

}
