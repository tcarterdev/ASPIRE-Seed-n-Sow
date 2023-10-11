using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Settings : MonoBehaviour
{
    [SerializeField] private GameObject settingsMenu;
    [Space]
    [SerializeField] private AudioMixer gameVolumeMixer;
    [SerializeField] private AudioMixer musicVolumeMixer;

    private void Start()
    {
        gameVolumeMixer.SetFloat("Master Volume", PlayerPrefs.GetFloat("GameVolume"));
        musicVolumeMixer.SetFloat("Music Volume", PlayerPrefs.GetFloat("MusicVolume"));
    }

    public void ToggleSettingsMenu()
    {
        if (settingsMenu.activeInHierarchy)
        {
            settingsMenu.SetActive(false);
        }
        else
        {
            settingsMenu.SetActive(true);
        }
    }

    public void SetGameVolumeLevel (float sliderValue)
    {
        gameVolumeMixer.SetFloat("Master Volume", Mathf.Log10(sliderValue) * 20f);
        PlayerPrefs.SetFloat("GameVolume", Mathf.Log10(sliderValue) * 20f);
    }

    public void SetMusicVolumeLevel (float sliderValue)
    {
        musicVolumeMixer.SetFloat("Music Volume", Mathf.Log10(sliderValue) * 20f);
        PlayerPrefs.SetFloat("MusicVolume", Mathf.Log10(sliderValue) * 20f);
    }
}
