using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    private PlayerData _player;
    private PowerGun _powerGun;

    [Header("Creation templates")] public GameObject stickyFlechettePrefab;

    [Header("Gameplay constants")] public float magnetDPS = 35f;

    [Header("Hookups")] public List<RobotBase> allRobots;
    public MusicManager musicManager;

    public PlayerData player => _player;
    public PowerGun PowerGun => _powerGun;

    private void Awake()
    {
        _player = FindObjectOfType<PlayerData>();
        _powerGun = FindObjectOfType<PowerGun>();
    }

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating(nameof(UpdateMusic), 0, 1.337f);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void UpdateMusic()
    {
        if (IsPlayerInDanger())
        {
            musicManager.Play(0);
        }
        else
        {
            musicManager.Play(1);
        }
    }

    public bool IsPlayerInDanger()
    {
        foreach (RobotBase robot in allRobots)
        {
            if (robot.robotAIState == RobotBase.RobotAIState.ATTACKING
                || robot.robotAIState == RobotBase.RobotAIState.FLECHETTESTUNNED
                || robot.robotAIState == RobotBase.RobotAIState.POSITIONING
                || robot.robotAIState == RobotBase.RobotAIState.RAGDOLL
                || robot.robotAIState == RobotBase.RobotAIState.MAGNETIZED
               )
            {
                return true;
            }
        }

        return false;
    }
}