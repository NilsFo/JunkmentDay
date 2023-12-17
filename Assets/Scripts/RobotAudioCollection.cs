using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RobotAudioCollection : MonoBehaviour
{
    public List<AudioClip> helloSounds;
    public List<AudioClip> deathSounds;

    public AudioSource robotAudioSource;
    public TimedLife timedLife;

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
        robotAudioSource.Play();
    }
    
}