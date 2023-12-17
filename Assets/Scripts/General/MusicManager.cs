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
    public readonly float GLOBAL_MUSIC_VOLUME_MULT = 0.69f / 2;
    public readonly float GLOBAL_SOUND_VOLUME_MULT = 1.337f / 2;

    [Range(0, 1)] public float levelVolumeMult = 1.0f;

    [Header("Config")] public float binningVolumeMult = 0.25f;

    [Header("Playlist")]
    public List<AudioSource> initiallyKnownSongs;
    private AudioListener _listener;

    private List<AudioSource> _playList;
    public List<int> _desiredMixingVolumes;
    public float musicChangeSpeed = 1;

    private Dictionary<string, float> _audioJail;

    private void Awake()
    {
        _playList = new List<AudioSource>();
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
            _playList[i].volume = _desiredMixingVolumes[i] * GetVolumeMusic() * levelVolumeMult;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_audioJail == null) return;

        transform.position = _listener.transform.position;
        userDesiredSoundVolume = MathF.Min(userDesiredMusicVolume * 1.0f, 1.0f);

        for (var i = 0; i < _playList.Count; i++)
        {
            var audioSource = _playList[i];
            var volumeMixing = _desiredMixingVolumes[i];

            var trueVolume = Mathf.Lerp(audioSource.volume,
                volumeMixing * GetVolumeMusic() * levelVolumeMult,
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
        return userDesiredMusicVolume * GLOBAL_MUSIC_VOLUME_MULT;
    }

    public float GetVolumeSound()
    {
        return userDesiredSoundVolume * GLOBAL_SOUND_VOLUME_MULT;
    }

    public void CreateAudioClip(AudioClip audioClip,
        Vector3 position,
        float pitchRange = 0.0f,
        float soundInstanceVolumeMult = 1.0f,
        bool threeDimensional = true,
        bool respectBinning = false)
    {
        // Registering in the jail
        string clipName = audioClip.name;
        float jailTime = audioClip.length * 0.42f;
        float binningMult=1.0f;
        
        if (_audioJail.ContainsKey(clipName))
        {
            _audioJail[clipName] = jailTime;
            if (respectBinning)
            {
                binningMult = binningVolumeMult;
                return;
            }
        }
        else
        {
            _audioJail.Add(clipName, jailTime);
        }

        // Instancing the sound
        GameObject adp = Instantiate(temporalAudioPlayerPrefab);
        adp.transform.position = position;
        AudioSource source = adp.GetComponent<AudioSource>();
        TimedLife life = adp.GetComponent<TimedLife>();
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
        source.volume = MathF.Min(GetVolumeSound() * soundInstanceVolumeMult * levelVolumeMult*binningMult, 1.0f);
        source.Play();
    }

    public float AudioBinExternalSound(AudioClip audioClip)
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