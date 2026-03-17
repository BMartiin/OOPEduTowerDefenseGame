using UnityEngine;
using TMPro; //need this for TextMeshPro things
using System.Collections.Generic;

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

    [Header("Adatok")]
    public List<QuestionData> questions;
    public GameObject knightPrefab;
    public Transform spawnPoint;

    private int currentQuestionIndex = 0;
    private bool canSpawn = false;

    private void Start()
    {
        Time.timeScale = 0.0f;

        puzzlePanel.SetActive(true);

        SelectQuestion(0);
    }

    public void SelectQuestion(int index)
    {
        if (index >= 0 && index < questions.Count)
        {
            currentQuestionIndex = index;

            taskDescriptionText.text = questions[index].Description;

            inputField.text = "";

            testStatusLabel.text = "Várakozás a kódra...";
            testStatusLabel.color = Color.white;
            canSpawn = false;
        }
    }

    public void CheckCode()
    {
        string playerInput = inputField.text.Trim();

        bool isCorrect = false;

        foreach (string answer in questions[currentQuestionIndex].Answers)
        {
            if (playerInput == answer.Trim())
            {
                isCorrect = true;
                break;
            }
        }

        if (isCorrect)
        {
            testStatusLabel.text = "HELYES!";
            testStatusLabel.color = Color.green;
            canSpawn = true;
        }
        else
        {
            testStatusLabel.text = "HIBA!";
            testStatusLabel.color = Color.red;
            canSpawn = false;
        }
    }

    public void StartBattle()
    {
        if (canSpawn)
        {
            //need this so the knight doesn't moonwalk
            Quaternion rightRotation = Quaternion.Euler(0, 180, 0);
            Instantiate(knightPrefab, spawnPoint.position, rightRotation);
            Debug.Log("Knight Spawned");
        }

        Time.timeScale = 1.0f;

        puzzlePanel.SetActive(false);
    }

    public void SkipAndStart()
    {
        Time.timeScale = 1.0f;
        puzzlePanel.SetActive(false);
    }

}
