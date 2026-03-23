using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Jint;
using System;

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

    [Header("Adatok")]
    public List<QuestionData> questions;
    public GameObject knightPrefab;
    public Transform spawnPoint;

    private Engine engine;
    private int currentQuestionIndex = 0;
    private int spawnedCount = 0;
    private int lastPowerValue = 0;
    private int gold = 150;

    private void Start()
    {
        Time.timeScale = 0.0f;
        puzzlePanel.SetActive(true);
        InitEngine();
    }

    private void InitEngine()
    {
        engine = new Engine();

        engine.SetValue("lovag", new Action<object>((p) => {
            int power = 1;
            if (p != null) int.TryParse(p.ToString(), out power);
            SpawnKnight(power);
        }));

        engine.SetValue("arany", gold);
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
        }
    }

    public void SpawnKnight(int power)
    {
        spawnedCount++;
        lastPowerValue = power;

        Quaternion rightRotation = Quaternion.Euler(0, 180, 0);
        Instantiate(knightPrefab, spawnPoint.position, rightRotation);
    }

    public void CheckCode()
    {
        spawnedCount = 0;
        lastPowerValue = 0;
        engine.SetValue("arany", gold);

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
            case 0: success = (spawnedCount == 1); break;
            case 1: success = (spawnedCount == 3); break;
            case 2: success = (spawnedCount == 1); break;
            case 3: success = (lastPowerValue == 10); break;
            case 4: success = (spawnedCount == 2); break;
        }

        if (success)
        {
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
        Time.timeScale = 1.0f;
        puzzlePanel.SetActive(false);
    }

    public void SkipAndStart()
    {
        Time.timeScale = 1.0f;
        puzzlePanel.SetActive(false);
    }
}