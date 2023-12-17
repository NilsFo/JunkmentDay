using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RobotAudioCollection : MonoBehaviour
{
    public List<AudioClip> helloSounds;
    public List<AudioClip> deathSounds;
    public List<AudioClip> bumpSounds;

    public AudioSource robotAudioSource;
    public TimedLife timedLife;

    private GameState _gameState;

    public AudioClip NextHelloSound()
    {
        AudioClip sound = helloSounds[Random.Range(0, helloSounds.Count)];
        return sound;
    }

    public AudioClip NextDeathSound()
    {
        AudioClip sound = deathSounds[Random.Range(0, deathSounds.Count)];
        return sound;
    }
    
    public AudioClip NetxtBumpSound()
    {
        AudioClip sound = bumpSounds[Random.Range(0, bumpSounds.Count)];
        return sound;
    }

    private void Awake()
    {
        _gameState = FindObjectOfType<GameState>();
    }

    private void Start()
    {
        timedLife.timerActive = false;
    }

    public void Play(AudioClip clip)
    {
        if (robotAudioSource.isPlaying)
        {
            return;
        }

        robotAudioSource.pitch += Random.Range(-0.1f, 0.1f);
        robotAudioSource.clip = clip;
        robotAudioSource.volume *= _gameState.musicManager.AudioBinExternalSound(clip);
        robotAudioSource.Play();
    }
}