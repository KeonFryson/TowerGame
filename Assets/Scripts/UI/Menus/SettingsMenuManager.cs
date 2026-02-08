using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsMenuManager : MonoBehaviour
{
    public TMP_Dropdown graphicsDeropdown;
    public Slider MasterVol, MusicVol, SFXVol;
    public AudioMixer audioMixer;

    private const string MasterVolKey = "MasterVol";
    private const string MusicVolKey = "MusicVol";
    private const string SFXVolKey = "SFXVol";

    void Start()
    {
        if (graphicsDeropdown != null)
        {
            graphicsDeropdown.value = QualitySettings.GetQualityLevel();
        }

        // Load saved volume settings or use defaults
        float masterVol = PlayerPrefs.HasKey(MasterVolKey) ? PlayerPrefs.GetFloat(MasterVolKey) : 0f;
        float musicVol = PlayerPrefs.HasKey(MusicVolKey) ? PlayerPrefs.GetFloat(MusicVolKey) : 0f;
        float sfxVol = PlayerPrefs.HasKey(SFXVolKey) ? PlayerPrefs.GetFloat(SFXVolKey) : 0f;

        if (audioMixer != null)
        {
            audioMixer.SetFloat(MasterVolKey, masterVol);
            if (MasterVol != null) MasterVol.value = masterVol;

            audioMixer.SetFloat(MusicVolKey, musicVol);
            if (MusicVol != null) MusicVol.value = musicVol;

            audioMixer.SetFloat(SFXVolKey, sfxVol);
            if (SFXVol != null) SFXVol.value = sfxVol;
        }

        // Add listeners to sliders to update volume when changed
        if (MasterVol != null)
            MasterVol.onValueChanged.AddListener((value) => SetMasterVolume());
        if (MusicVol != null)
            MusicVol.onValueChanged.AddListener((value) => SetMusicVolume());
        if (SFXVol != null)
            SFXVol.onValueChanged.AddListener((value) => SetSFXVolume());
    }

    public void ChangeGraphicsQuality()
    {
        QualitySettings.SetQualityLevel(graphicsDeropdown.value);
        AudioManager.Instance.PlayButtonClick();
    }

    public void SetMasterVolume()
    {
        if (audioMixer != null && MasterVol != null)
        {
            audioMixer.SetFloat(MasterVolKey, MasterVol.value);
            PlayerPrefs.SetFloat(MasterVolKey, MasterVol.value);
            PlayerPrefs.Save();
        }
    }

    public void SetMusicVolume()
    {
        if (audioMixer != null && MusicVol != null)
        {
            audioMixer.SetFloat(MusicVolKey, MusicVol.value);
            PlayerPrefs.SetFloat(MusicVolKey, MusicVol.value);
            PlayerPrefs.Save();
        }
    }

    public void SetSFXVolume()
    {
        if (audioMixer != null && SFXVol != null)
        {
            audioMixer.SetFloat(SFXVolKey, SFXVol.value);
            PlayerPrefs.SetFloat(SFXVolKey, SFXVol.value);
            PlayerPrefs.Save();
        }
    }
}