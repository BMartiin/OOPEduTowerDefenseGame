using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private float levelStartTime;

    private void Start()
    {
        levelStartTime = Time.time;
    }

    public int CalculateFinalScore(int goldCount)
    {
        int basePoints = goldCount * 15;

        float elapsedTime = Time.time - levelStartTime;
        int timeBonus = Mathf.Max(0, 40 - Mathf.RoundToInt(elapsedTime * 0.5f));

        int finalScore = basePoints + timeBonus;

        return Mathf.Min(finalScore, 100);
    }
}