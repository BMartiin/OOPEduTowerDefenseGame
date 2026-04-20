using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources (Lejátszók)")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Hangeffektek (Klipek)")]
    public AudioClip menuBGM;
    public AudioClip swordSwing;
    public AudioClip arrowShoot;
    public AudioClip tutorPop;
    public AudioClip successSound;
    public AudioClip errorSound;

    [Header("UI Hangok")]
    public AudioClip buttonClickSound;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        float savedBGM = PlayerPrefs.GetFloat("BGMVolume", 0.7f);
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 0.9f);

        SetBGMVolume(savedBGM);
        SetSFXVolume(savedSFX);

        AssignClickSoundsToAllButtons();
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip != null)
        {
            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void StopBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }

    private void AssignClickSoundsToAllButtons()
    {
        Button[] allButtons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);

        foreach (Button btn in allButtons)
        {
            btn.onClick.RemoveListener(PlayButtonSound);
            btn.onClick.AddListener(PlayButtonSound);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AssignClickSoundsToAllButtons();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void PlayButtonSound()
    {
        PlaySFX(buttonClickSound);
    }

    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = volume;
        PlayerPrefs.SetFloat("BGMVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
}