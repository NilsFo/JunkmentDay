using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class RobotBase : MonoBehaviour
{
    [Header("Hookup")] public Attractable myAttractable;
    public Markable myMarkable;
    public RobotAIState robotAIState = RobotBase.RobotAIState.UNKNOWN;
    private GameState _gameState;

    [Header("AI Config")] public Transform head;
    public float playerDetectionDistance = 50f;

    public enum RobotAIState
    {
        UNKNOWN,
        IDLE,
        ATTACKING,
        POSITIONING,
        MAGNETIZED
    }

    private void Awake()
    {
        _gameState = FindObjectOfType<GameState>();
    }

    private void Update()
    {
        if (robotAIState == RobotBase.RobotAIState.UNKNOWN)
        {
            Debug.LogError("Robot has an unknown state!", gameObject);
        }
    }

    public bool PlayerDetected()
    {
        return GetDistanceToPlayer() <= playerDetectionDistance && PlayerInView();
    }

    public float GetDistanceToPlayer()
    {
        return _gameState.player.GetDistanceToPlayer(head.position);
    }

    public bool PlayerInView()
    {
        return _gameState.player.PlayerInView(head.position);
    }
}