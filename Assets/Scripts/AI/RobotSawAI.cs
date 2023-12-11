using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class RobotSawAI : MonoBehaviour
{
    [Header("Hookup")] public NavMeshAgent myNavMeshAgent;
    public RobotBase robotBase;
    public Rigidbody rb;

    public RobotBase.RobotAIState RobotAIStateCurrent
    {
        get => robotBase.robotAIState;
        set => robotBase.robotAIState = value;
    }

    private RobotBase.RobotAIState _robotAIStateLastKnown;
    public RobotSawBladeRotor sawBladeRotor;

    private GameState _gameState;

    // Start is called before the first frame update
    void Awake()
    {
        _gameState = FindObjectOfType<GameState>();
        _robotAIStateLastKnown = robotBase.robotAIState;
        InvokeRepeating(nameof(NavigationUpdate), 0f, 0.5f);
    }

    private void Start()
    {
        _robotAIStateLastKnown = RobotBase.RobotAIState.UNKNOWN;
    }

    // Update is called once per frame
    void Update()
    {
        if (_robotAIStateLastKnown != RobotAIStateCurrent)
        {
            OnAIStateChanged(_robotAIStateLastKnown, RobotAIStateCurrent);
            _robotAIStateLastKnown = RobotAIStateCurrent;
        }

        bool attractableMagnetized = robotBase.myAttractable.IsMagnetized();
        if (RobotAIStateCurrent == RobotBase.RobotAIState.MAGNETIZED
            && !attractableMagnetized)
        {
            RobotAIStateCurrent = RobotBase.RobotAIState.IDLE;
        }

        if ((RobotAIStateCurrent == RobotBase.RobotAIState.IDLE
             || RobotAIStateCurrent == RobotBase.RobotAIState.ATTACKING)
            && attractableMagnetized)
        {
            RobotAIStateCurrent = RobotBase.RobotAIState.MAGNETIZED;
        }
    }

    private void OnAIStateChanged(RobotBase.RobotAIState oldState, RobotBase.RobotAIState newState)
    {
        Debug.Log("Robot '" + name + "' new state: " + newState);

        switch (oldState)
        {
            case RobotBase.RobotAIState.IDLE:
                break;
            case RobotBase.RobotAIState.MAGNETIZED:
                myNavMeshAgent.enabled = true;
                rb.isKinematic = true;
                break;
            case RobotBase.RobotAIState.ATTACKING:
                sawBladeRotor.TurnOff();
                break;
            case RobotBase.RobotAIState.POSITIONING:
                RobotAIStateCurrent = RobotBase.RobotAIState.ATTACKING;
                break;
            case RobotBase.RobotAIState.UNKNOWN:
                break;
            default:
                RobotAIStateCurrent = RobotBase.RobotAIState.UNKNOWN;
                break;
        }

        switch (newState)
        {
            case RobotBase.RobotAIState.IDLE:
                break;
            case RobotBase.RobotAIState.MAGNETIZED:
                myNavMeshAgent.enabled = false;
                rb.isKinematic = false;
                break;
            case RobotBase.RobotAIState.ATTACKING:
                sawBladeRotor.TurnOn();
                break;
            case RobotBase.RobotAIState.POSITIONING:
                RobotAIStateCurrent = RobotBase.RobotAIState.ATTACKING;
                break;
            default:
                RobotAIStateCurrent = RobotBase.RobotAIState.UNKNOWN;
                break;
        }
    }

    private void NavigationUpdate()
    {
        if (RobotAIStateCurrent == RobotBase.RobotAIState.MAGNETIZED || !myNavMeshAgent.enabled)
        {
            return;
        }

        if (RobotAIStateCurrent == RobotBase.RobotAIState.IDLE)
        {
            if (robotBase.PlayerDetected())
                RobotAIStateCurrent = RobotBase.RobotAIState.ATTACKING;
        }

        if (RobotAIStateCurrent == RobotBase.RobotAIState.ATTACKING)
        {
            if (robotBase.PlayerInView())
            {
                myNavMeshAgent.SetDestination(_gameState.player.transform.position);
            }
            else
            {
                RobotAIStateCurrent = RobotBase.RobotAIState.IDLE;
            }
        }
    }
}