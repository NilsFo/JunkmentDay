using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Blockade : MonoBehaviour
{
    [Header("Config")] public float destructionDelay = 3f;
    private GameState _gameState;
    private bool _destroyCalled = false;

    [Header("Listeners")] public UnityEvent onMagnet;

    [Header("Encounter Group")] public EncounterGroup myEncounterGroup;

    private void Awake()
    {
        _gameState = FindObjectOfType<GameState>();
    }

    void Start()
    {
        _destroyCalled = false;
        if (onMagnet == null)
        {
            onMagnet = new UnityEvent();
        }

        _gameState.blockadesCount++;
    }

    // Update is called once per frame
    void Update()
    {
    }

    [ContextMenu("Destroy Now")]
    public void DestroyBlockade()
    {
        if (!_destroyCalled)
        {
            _destroyCalled = true;
            ActivateEncounter();
            Invoke(nameof(DestroyBlockadeNow), destructionDelay);
        }
    }

    private void DestroyBlockadeNow()
    {
        _gameState.blockadesDestroyed++;
        onMagnet.Invoke();

        var magnet = FindObjectOfType<BigMagnet>();
        foreach (var debris in GetComponentsInChildren<MoveToAndDestroy>())
        {
            debris.target = magnet.GetComponentInChildren<Attractor>().transform;
            debris.enabled = true;
            debris.transform.parent = null;
        }

        Destroy(gameObject);
    }

    public void ActivateEncounter()
    {
        myEncounterGroup.ActivateEncounter(robotDelay: 0, spawnerDelay: destructionDelay * 2f);
    }
}