using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class MusicManager : MonoBehaviour
{
    public GameObject temporalAudioPlayerPrefab;
    public static float userDesiredMusicVolume = 0.5f;
    public static float userDesiredSoundVolume = 0.5f;
    public readonly float GLOBAL_MUSIC_VOLUME_MULT = 0.69f;
    public readonly float GLOBAL_SOUND_VOLUME_MULT = 1.337f;

    [Range(0, 1)] public float levelVolumeMult = 1.0f;

    [Header("Config")] public float binningVolumeMult = 0.25f;
    public float musicChangeSpeed = 1;

    [Header("Playlist")] public List<AudioSource> initiallyKnownSongs;
    public List<AudioSource> initiallyKnownSoundEffects;
    private AudioListener _listener;
    private Dictionary<AudioSource, float> _registeredSoundEffects;
    private List<AudioSource> _registeredAudioSourcesSFX; // Exposed for debug

    private List<AudioSource> _playList;
    private List<int> _desiredMixingVolumes;

    // Audio Binning
    private Dictionary<string, float> _audioJail;

    private void Awake()
    {
        _playList = new List<AudioSource>();
        _desiredMixingVolumes = new List<int>();
        _registeredSoundEffects = new Dictionary<AudioSource, float>();
        _registeredAudioSourcesSFX = new List<AudioSource>();

        foreach (AudioSource song in initiallyKnownSongs)
        {
            _playList.Add(song);
            song.Play();
            song.volume = 0;
            _desiredMixingVolumes.Add(0);
        }

        SkipFade();

        _listener = FindObjectOfType<AudioListener>();
        _audioJail = new Dictionary<string, float>();
    }

    // Start is called before the first frame update
    void Start()
    {
        foreach (AudioSource soundEffect in initiallyKnownSoundEffects)
        {
            _registeredSoundEffects.Add(soundEffect, soundEffect.volume);
            _registeredAudioSourcesSFX.Add(soundEffect);
        }
    }

    public void Play(int index, bool fromBeginning = false)
    {
        for (var i = 0; i < _playList.Count; i++)
        {
            _desiredMixingVolumes[i] = 0;
        }

        if (fromBeginning)
        {
            _playList[index].time = 0;
        }

        _desiredMixingVolumes[index] = 1;

        if (!_playList[index].isPlaying)
        {
            _playList[index].Play();
        }

        // print("Playing: " + _playList[index].gameObject.name);
    }

    public void SkipFade()
    {
        for (var i = 0; i < _playList.Count; i++)
        {
            _playList[i].volume = _desiredMixingVolumes[i] * GetVolumeMusic();
        }
    }

    // Update is called once per frame
    void Update()
    {
        List<AudioSource> deleteListSFX = new List<AudioSource>();
        foreach (AudioSource source in _registeredSoundEffects.Keys)
        {
            if (source.IsDestroyed())
            {
                deleteListSFX.Add(source);
            }
            else
            {
                source.volume = _registeredSoundEffects[source] * GetVolumeSound();
            }
        }

        foreach (AudioSource source in deleteListSFX)
        {
            _registeredSoundEffects.Remove(source);
        }

        if (_registeredAudioSourcesSFX.Count != _registeredSoundEffects.Keys.Count)
        {
            _registeredAudioSourcesSFX.Clear();
            _registeredAudioSourcesSFX.AddRange(_registeredSoundEffects.Keys);
        }

        if (_audioJail == null) return;

        transform.position = _listener.transform.position;
        userDesiredSoundVolume = MathF.Min(userDesiredMusicVolume * 1.0f, 1.0f);

        for (var i = 0; i < _playList.Count; i++)
        {
            var audioSource = _playList[i];
            var volumeMixing = _desiredMixingVolumes[i];

            var trueVolume = Mathf.Lerp(audioSource.volume,
                volumeMixing * GetVolumeMusic(),
                Time.deltaTime * musicChangeSpeed);

            if (trueVolume - Time.deltaTime * musicChangeSpeed <= 0 && volumeMixing == 0)
            {
                trueVolume = 0;
            }

            audioSource.volume = trueVolume;
        }

        var keys = _audioJail.Keys.ToArrayPooled().ToList();
        List<string> releaseKeys = new List<string>();
        if (keys.Count > 0)
        {
            for (var i = 0; i < keys.Count; i++)
            {
                string key = keys[i];
                float timeout = _audioJail[key];
                timeout -= Time.deltaTime;
                _audioJail[key] = timeout;

                if (timeout < 0)
                {
                    releaseKeys.Add(key);
                }
            }
        }

        foreach (var releaseKey in releaseKeys)
        {
            _audioJail.Remove(releaseKey);
        }

        string pg = "";
        foreach (var audioSource in _playList)
        {
            pg += " - " + audioSource.time;
        }
    }

    public float GetVolumeMusic()
    {
        return userDesiredMusicVolume * GLOBAL_MUSIC_VOLUME_MULT * levelVolumeMult;
    }

    public float GetVolumeSound()
    {
        return userDesiredSoundVolume * GLOBAL_SOUND_VOLUME_MULT * levelVolumeMult;
    }

    public GameObject CreateAudioClip(AudioClip audioClip,
        Vector3 position,
        float pitchRange = 0.0f,
        float soundInstanceVolumeMult = 1.0f,
        bool threeDimensional = true,
        bool respectBinning = false)
    {
        // Registering in the jail
        string clipName = audioClip.name;
        float jailTime = audioClip.length * 0.42f;
        float binningMult = 1.0f;

        if (_audioJail.ContainsKey(clipName))
        {
            _audioJail[clipName] = jailTime;
            if (respectBinning)
            {
                binningMult = binningVolumeMult;
                // return;
            }
        }
        else
        {
            _audioJail.Add(clipName, jailTime);
        }

        // Instancing the sound
        GameObject soundInstance = Instantiate(temporalAudioPlayerPrefab);
        soundInstance.transform.position = position;
        AudioSource source = soundInstance.GetComponent<AudioSource>();
        TimedLife life = soundInstance.GetComponent<TimedLife>();
        life.aliveTime = audioClip.length * 2;

        if (threeDimensional)
        {
            source.spatialBlend = 1;
        }
        else
        {
            source.spatialBlend = 0;
        }

        source.clip = audioClip;
        source.pitch = 1.0f + Random.Range(-pitchRange, pitchRange);
        source.volume = MathF.Min(GetVolumeSound() * soundInstanceVolumeMult * binningMult, 1.0f);
        source.Play();

        return soundInstance;
    }

    public void RegisterSoundScaling(AudioSource audioSource)
    {
        if (!_registeredSoundEffects.Keys.Contains(audioSource))
        {
            _registeredSoundEffects.Add(audioSource, audioSource.volume);
        }
    }

    public float AudioBinExternalSoundMult(AudioClip audioClip)
    {
        string clipName = audioClip.name;
        float jailTime = audioClip.length * 0.42f;
        if (_audioJail.ContainsKey(clipName))
        {
            return binningVolumeMult;
        }
        else
        {
            _audioJail.Add(clipName, jailTime);
            return 1.0f;
        }
    }
}