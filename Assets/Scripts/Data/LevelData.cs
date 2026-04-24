using UnityEngine;
using System.Collections.Generic;
using System;

public enum UnitReward
{
    Lovag,
    Ijasz,
    None
}

[Serializable]
public struct QuestionData
{
    [Header("Feladat adatai")]
    public string Description;
    [TextArea(3, 10)]
    public string Theory;

    [Header("Logika")]
    public int RequiredKnightCount;
    public int RequiredArcherCount;
    public int RequiredPower;
    public string RequiredVarName;
    public int ExpectedVarValue;

    [Header("Regex - Szintaktika")]
    public string RequiredRegexPattern;

    [Header("Jutalom a feladatert")]
    public UnitReward RewardUnit;

    public bool RequiresJint
    {
        get
        {
            return RequiredKnightCount > 0
                || RequiredArcherCount > 0
                || RequiredPower > 0
                || !string.IsNullOrEmpty(RequiredVarName);
        }
    }
}

[CreateAssetMenu(fileName = "NewLevelData", menuName = "ScriptableObjects/LevelData")]
public class LevelData : ScriptableObject
{
    public string LevelName;
    [Header("Pálya beállítások")]
    public int EnemyCount = 2;
    public List<QuestionData> Questions;
}
