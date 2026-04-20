using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public Slider bgmSlider;
    public Slider sfxSlider;

    private void Start()
    {
        float bgmVol = PlayerPrefs.GetFloat("BGMVolume", 0.7f);
        if (bgmVol <= 0.01f) bgmVol = 0.7f;

        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 0.9f);
        if (sfxVol <= 0.01f) sfxVol = 0.9f;

        if (bgmSlider != null)
            bgmSlider.SetValueWithoutNotify(bgmVol);

        if (sfxSlider != null)
            sfxSlider.SetValueWithoutNotify(sfxVol);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBGMVolume(bgmVol);
            AudioManager.Instance.SetSFXVolume(sfxVol);
        }
    }

    public void OnBGMChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetBGMVolume(value);
    }

    public void OnSFXChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);
    }
}