using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class MenuManager : MonoBehaviour
{
    [Header("Panelek")]
    public GameObject menuPanel;
    public GameObject infoPanel;
    public GameObject settingsPanel;

    [Header("UI Elemek")]
    public TMP_Text levelDisplayLabel;

    public static string LevelToLoad = "Level_1";

    private void Start()
    {
        string currentUrl = Application.absoluteURL;

        if (!string.IsNullOrEmpty(currentUrl) && currentUrl.Contains("?level="))
        {
            try
            {
                string[] urlParts = currentUrl.Split(new string[] { "?level=" }, StringSplitOptions.None);
                string levelString = urlParts[1].Split('&')[0];

                if (int.TryParse(levelString, out int parsedLevel))
                {
                    LevelToLoad = "Level_" + parsedLevel;
                    if (levelDisplayLabel != null)
                    {
                        levelDisplayLabel.text = "Szint " + parsedLevel;
                    }
                }
                else
                {
                    LoadDefault();
                }
            }
            catch
            {
                LoadDefault();
            }
        }
        else
        {
            LoadDefault();
        }
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM(AudioManager.Instance.menuBGM);
        }
        ShowMainMenu();
    }

    private void LoadDefault()
    {
        LevelToLoad = "Level_1";
        if (levelDisplayLabel != null)
        {
            levelDisplayLabel.text = "Szint 1";
        }
    }

    public void ShowMainMenu()
    {
        menuPanel.SetActive(true);
        infoPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    public void ShowInfo()
    {
        infoPanel.SetActive(true);
    }

    public void ShowSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void StartGame()
    {
        AudioManager.Instance.StopBGM();
        SceneManager.LoadScene("SampleScene");
    }
}