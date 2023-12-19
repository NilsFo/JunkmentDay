using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class Powerable : MonoBehaviour
{
    public UnityEvent OnPowered;
    public UnityEvent OnUnpowered;
    private GameState _gameState;

    public bool powered;
    private bool _powered;

    public float powerTime = 0f;
    private float _powerTimer = 0f;

    public List<AudioClip> onPowerSounds;

    private void Awake()
    {
        _gameState=FindObjectOfType<GameState>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (powered)
            Power();
        else
            Unpower();
    }

    // Update is called once per frame
    void Update()
    {
        if (_powered != powered)
        {
            if (powered)
                Power();
            else
                Unpower();
        }

        if (_powerTimer > 0)
        {
            _powerTimer -= Time.deltaTime;
            if (_powerTimer <= 0)
            {
                Unpower();
            }
        }
    }

    public void Power()
    {
        _powered = true;
        powered = true;

        OnPowered.Invoke();
        if (powerTime > 0)
        {
            _powerTimer = powerTime;
        }

        if (onPowerSounds.Count == 0)
        {
            Debug.LogWarning("This powerable has no sound associated with it!",gameObject);
        }
        else
        {
            _gameState.musicManager.CreateAudioClip(
                onPowerSounds[Random.Range(0, onPowerSounds.Count)],
                transform.position,
                pitchRange: 0.1f,
                soundVolume: 0.69f,
                respectBinning: true
            );
        }
    }

    public void Unpower()
    {
        _powered = false;
        powered = false;
        _powerTimer = 0;
        OnUnpowered.Invoke();
    }
}