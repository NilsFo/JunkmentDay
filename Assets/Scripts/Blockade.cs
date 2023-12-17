using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blockade : MonoBehaviour
{
    [Header("Config")] public float destructionDelay = 3f;
    private GameState _gameState;

    [Header("Encounter Group")] public EncounterGroup myEncounterGroup;

    private void Awake()
    {
        _gameState = FindObjectOfType<GameState>();
        _gameState.blockadesCount++;
    }

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void DestroyBlockade()
    {
        ActivateEncounter();
        Invoke(nameof(DestroyBlockadeNow), destructionDelay);
    }

    private void DestroyBlockadeNow()
    {
        _gameState.blockadesDestroyed++;

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