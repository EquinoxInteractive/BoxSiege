using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyMusic : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [Header("Audio Clip")]
    public AudioClip backgroundMusic;
    public AudioClip lobbyMusic;
    public AudioClip jump;
    public AudioClip hit;
    public AudioClip shot;
    public AudioClip gameOver;

    private void Start()
    {
        musicSource.clip = lobbyMusic;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }

    public void StopMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }
}