using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text resultText;
    public TMP_Text goldText;
    public TMP_Text scoreText;

    public void Setup(string result, int gold, int score, Color statusColor)
    {
        panel.SetActive(true);
        resultText.text = result;
        resultText.color = statusColor;
        goldText.text = gold + "G";
        scoreText.text = score + "XP";
    }

    public void RestartGame()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("Menu");
    }
}