using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Panelek")]
    public GameObject menuPanel;
    public GameObject infoPanel;
    public GameObject settingsPanel;
    public GameObject levelsPanel;

    private void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        menuPanel.SetActive(true);
        infoPanel.SetActive(false);
        settingsPanel.SetActive(false);
        levelsPanel.SetActive(false);
    }

    public void ShowInfo()
    {
        infoPanel.SetActive(true);
    }

    public void ShowSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void ShowLevels()
    {
        levelsPanel.SetActive(true);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
