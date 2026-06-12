using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clip")]
    public AudioClip backgroundMusic;
    public AudioClip lobbyMusic;
    public AudioClip jump;
    public AudioClip hit;
    public AudioClip shot;
    public AudioClip gameOver;
    public AudioClip winGame;
    public AudioClip healthPowerUp;
    public AudioClip shieldPowerUp;
    public AudioClip jumpBoostPowerUp;
    public AudioClip speedBoostPowerUp;
    public AudioClip fight;
    public AudioClip round1;
    public AudioClip round2;
    public AudioClip round3;
    public AudioClip round4;
    public AudioClip round5;
    public AudioClip round6;
    public AudioClip round7;
    public AudioClip round8;
    public AudioClip suddenDeath;
    public AudioClip finalRound;

    private void Awake()
    {
        // Pastikan AudioSource untuk SFX ada
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            Debug.LogWarning("SFXSource tidak diassign di Inspector, menambahkan AudioSource secara otomatis.");
        }
    }

    private void Start()
    {
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning("MusicSource atau backgroundMusic tidak diassign di AudioManager!");
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioClip untuk SFX kosong!");
            return;
        }

        if (sfxSource == null)
        {
            Debug.LogError("SFXSource tidak ditemukan di AudioManager!");
            return;
        }

        sfxSource.PlayOneShot(clip);
        Debug.Log($"Memutar SFX: {clip.name}");
    }

    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }
}