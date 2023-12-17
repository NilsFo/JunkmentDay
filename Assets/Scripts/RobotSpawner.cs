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

    [Header("Spawn settings")] public bool spawnInRagDollMode = false;
    public int maxSpawnCount = 1;
    public bool autoSpawn = false;
    private List<RobotBase> _allAliveRobots;

    [Header("World hookup")] public DoorAI myDoor;
    public Transform robotSpawnLocation;
    private RobotBase _lastCreatedRobot;

    private void Awake()
    {
        _robotRequested = false;
        _gameState = FindObjectOfType<GameState>();
        maxSpawnCount = Math.Max(maxSpawnCount, 1);
        _allAliveRobots = new List<RobotBase>();
    }

    // Start is called before the first frame update
    void Start()
    {
        myDoor.Close();
        _gameState.allSpawners.Add(this);
        autoSpawn = false;
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

        if (autoSpawn && IsReadyForRobot())
        {
            RequestRobot();
        }

        _allAliveRobots.RemoveAll(@base => @base.IsDestroyed());
    }

    private bool Match(RobotBase obj)
    {
        return obj.IsDestroyed();
    }

    public bool IsReadyForRobot()
    {
        return _lastCreatedRobot == null
               && myDoor.FullyClosed()
               && _allAliveRobots.Count < maxSpawnCount;
    }

    public void RequestRobot()
    {
        _robotRequested = true;
    }

    private void SpawnRobot()
    {
        myDoor.Open();

        GameObject robotGO = Instantiate(robotPrefab, robotSpawnLocation.position, UnityEngine.Quaternion.identity);
        RobotBase robotBase = robotGO.GetComponent<RobotBase>();
        if (robotBase != null)
        {
            robotBase.robotAIState = RobotBase.RobotAIState.POSITIONING;
            if (spawnInRagDollMode)
            {
                robotBase.robotAIState = RobotBase.RobotAIState.RAGDOLL;
            }

            _lastCreatedRobot = robotBase;
            _allAliveRobots.Add(robotBase);
        }
        else
        {
            Destroy(robotBase);
        }
    }
}