using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


public class EncounterGroup : MonoBehaviour
{
    public List<RobotBase> roamingRobots;
    public List<RobotSpawner> spawners;

    private GameState _gameState;
    private bool _setupDone;
    private bool _encountered = false;

    private void Awake()
    {
        _gameState = FindObjectOfType<GameState>();
    }

    private void Start()
    {
        _encountered = false;
    }

    private void Update()
    {
    }

    private void LateUpdate()
    {
        if (!_setupDone)
        {
            foreach (RobotBase robot in roamingRobots)
            {
                robot.gameObject.SetActive(false);
            }

            _setupDone = true;
        }
    }

    public void ActivateEncounter()
    {
        ActivateEncounter(0f, 0f);
    }

    public void ActivateEncounter(float robotDelay, float spawnerDelay)
    {
        if (!_encountered)
        {
            transform.parent = null;
            Invoke(nameof(ActivateSpawners), spawnerDelay);
            Invoke(nameof(ActivateRoamingRobots), robotDelay);
            _encountered = true;
        }
    }

    private void ActivateRoamingRobots()
    {
        foreach (RobotBase robot in roamingRobots)
        {
            robot.gameObject.SetActive(true);
        }
    }

    private void ActivateSpawners()
    {
        foreach (RobotSpawner spawner in spawners)
        {
            spawner.autoSpawn = true;
        }
    }
}