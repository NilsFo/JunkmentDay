using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class MusicManager : MonoBehaviour
{
    public GameObject temporalAudioPlayerPrefab;
    public static float userDesiredMusicVolume = 0.5f;
    public static float userDesiredSoundVolume = 0.5f;
    public static float userDesiredMasterVolume = 0.5f;

    [Header("Custom sound level balance")] [Range(0, 1)]
    public float baselineMusicVolume = 1.0f;

    [Range(0, 1)] public float baselineSoundVolume = 1.0f;
    [Range(0, 1)] public float baselineMasterVolume = 1.0f;

    public float userDesiredMusicVolumeDB =>
        Mathf.Log10(Mathf.Clamp(userDesiredMusicVolume * baselineMusicVolume, 0.0001f, 1.0f)) * 20;

    public float userDesiredSoundVolumeDB =>
        Mathf.Log10(Mathf.Clamp(userDesiredSoundVolume * baselineSoundVolume, 0.0001f, 1.0f)) * 20;

    public float userDesiredMasterVolumeDB =>
        Mathf.Log10(Mathf.Clamp(userDesiredMasterVolume * baselineMasterVolume, 0.0001f, 1.0f)) * 20;

    [Header("Mixer")] public AudioMixer audioMixer;
    public string masterTrackName = "master";
    public string musicTrackName = "music";
    public string soundEffectsTrackName = "sfx";

    [Header("Config")] public float binningVolumeMult = 0.15f;
    public float musicFadeSpeed = 1;

    [Header("Playlist")] public List<AudioSource> initiallyKnownSongs;
    private AudioListener _listener;

    private List<AudioSource> _playList;
    private List<int> _desiredMixingVolumes;

    // Audio Binning
    private Dictionary<string, float> _audioJail;

    private void Awake()
    {
        _playList = new List<AudioSource>();
        _desiredMixingVolumes = new List<int>();

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
            _playList[i].volume = _desiredMixingVolumes[i];
        }
    }

    // Update is called once per frame
    void Update()
    {
        audioMixer.SetFloat(musicTrackName, userDesiredMusicVolumeDB);
        audioMixer.SetFloat(soundEffectsTrackName, userDesiredSoundVolumeDB);
        audioMixer.SetFloat(masterTrackName, userDesiredMasterVolumeDB);

        if (_audioJail == null) return;

        transform.position = _listener.transform.position;
        userDesiredSoundVolume = MathF.Min(userDesiredMusicVolume * 1.0f, 1.0f);

        for (var i = 0; i < _playList.Count; i++)
        {
            var audioSource = _playList[i];
            var volumeMixing = _desiredMixingVolumes[i];

            var trueVolume = Mathf.Lerp(audioSource.volume,
                volumeMixing,
                Time.deltaTime * musicFadeSpeed);

            if (trueVolume - Time.deltaTime * musicFadeSpeed <= 0 && volumeMixing == 0)
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

    private void LateUpdate()
    {
        userDesiredMusicVolume = Mathf.Clamp(userDesiredMusicVolume, 0f, 1f);
        userDesiredSoundVolume = Mathf.Clamp(userDesiredSoundVolume, 0f, 1f);
        userDesiredMasterVolume = Mathf.Clamp(userDesiredMasterVolume, 0f, 1f);
        baselineMusicVolume = Mathf.Clamp(baselineMusicVolume, 0f, 1f);
        baselineSoundVolume = Mathf.Clamp(baselineSoundVolume, 0f, 1f);
        baselineMasterVolume = Mathf.Clamp(baselineMasterVolume, 0f, 1f);
    }

    public float GetVolumeMusic()
    {
        audioMixer.GetFloat(musicTrackName, out float volume);
        return volume;
    }

    public float GetVolumeSound()
    {
        audioMixer.GetFloat(soundEffectsTrackName, out float volume);
        return volume;
    }

    public float GetMasterSound()
    {
        audioMixer.GetFloat(masterTrackName, out float volume);
        return volume;
    }

    public GameObject CreateAudioClip(AudioClip audioClip,
        Vector3 position,
        float pitchRange = 0.0f,
        float soundVolume = 1.0f,
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
        source.volume = MathF.Min(soundVolume * binningMult, 1.0f);
        source.Play();

        return soundInstance;
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