using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Quaternion = System.Numerics.Quaternion;

public class RobotSpawner : MonoBehaviour
{
    private GameState _gameState;

    [Header("Robot to spawn")] public GameObject robotPrefab;
    public float closeDistance;
    private bool _robotRequested = false;

    [Header("World hookup")] public DoorAI myDoor;
    public Transform robotSpawnLocation;
    private RobotBase _lastCreatedRobot;

    private void Awake()
    {
        _robotRequested = false;
        _gameState = FindObjectOfType<GameState>();
    }

    // Start is called before the first frame update
    void Start()
    {
        myDoor.Close();
    }

    // Update is called once per frame
    void Update()
    {
        if (_lastCreatedRobot != null)
        {
            if (_lastCreatedRobot.IsDestroyed())
            {
                _lastCreatedRobot = null;
                myDoor.Close();
            }
        }

        if (_robotRequested && IsReadyForRobot())
        {
            _robotRequested = false;
            SpawnRobot();
        }

        if (_lastCreatedRobot != null)
        {
            var dist = Vector3.Distance(_lastCreatedRobot.transform.position, robotSpawnLocation.position);
            if (dist >= closeDistance)
            {
                _lastCreatedRobot = null;
                myDoor.Close();
            }
        }
    }

    public bool IsReadyForRobot()
    {
        return _lastCreatedRobot == null && myDoor.FullyClosed();
    }

    public void RequestRobot()
    {
        _robotRequested = true;
    }

    private void SpawnRobot()
    {
        myDoor.Open();

        var robotGO = Instantiate(robotPrefab, robotSpawnLocation.position, UnityEngine.Quaternion.identity);
        RobotBase robotBase = robotGO.GetComponent<RobotBase>();
        if (robotBase != null)
        {
            robotBase.robotAIState = RobotBase.RobotAIState.POSITIONING;
            _lastCreatedRobot = robotBase;
        }
        else
        {
            Destroy(robotBase);
        }
    }
}