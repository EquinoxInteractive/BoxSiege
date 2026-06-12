using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioMixer myMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider SFXSlider;

    // Constants for PlayerPrefs keys
    private const string MUSIC_VOLUME_KEY = "musicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    private void Start()
    {
        LoadVolume();
    }

    public void SetMusicVolume()
    {
        if (musicSlider != null && myMixer != null)
        {
            float volume = musicSlider.value;
            // Convert slider value (0 to 1) to logarithmic scale for better audio perception
            myMixer.SetFloat("music", Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20);

            // Save to PlayerPrefs
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, volume);
            PlayerPrefs.Save(); // Force save immediately
        }
    }

    public void SetSFXVolume()
    {
        if (SFXSlider != null && myMixer != null)
        {
            float volume = SFXSlider.value;
            // Add small value to prevent log(0) error
            myMixer.SetFloat("SFX", Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20);

            // Save to PlayerPrefs
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume);
            PlayerPrefs.Save(); // Force save immediately
        }
    }

    private void LoadVolume()
    {
        // Load music volume
        if (musicSlider != null)
        {
            // Get saved value or default to 1 (100%)
            float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
            musicSlider.value = musicVolume;

            // Apply the value to the mixer
            if (myMixer != null)
            {
                myMixer.SetFloat("music", Mathf.Log10(Mathf.Max(0.0001f, musicVolume)) * 20);
            }
        }

        // Load SFX volume
        if (SFXSlider != null)
        {
            // Get saved value or default to 1 (100%)
            float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
            SFXSlider.value = sfxVolume;

            // Apply the value to the mixer
            if (myMixer != null)
            {
                myMixer.SetFloat("SFX", Mathf.Log10(Mathf.Max(0.0001f, sfxVolume)) * 20);
            }
        }
    }

    // Method to reset volume to 100%
    public void ResetToDefaultVolume()
    {
        if (musicSlider != null)
        {
            musicSlider.value = 1f;
            SetMusicVolume();
        }

        if (SFXSlider != null)
        {
            SFXSlider.value = 1f;
            SetSFXVolume();
        }
    }
}