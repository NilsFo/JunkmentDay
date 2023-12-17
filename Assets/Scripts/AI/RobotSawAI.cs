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

    private float _movementSpeed;

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
        _movementSpeed = myNavMeshAgent.speed;
        _robotAIStateLastKnown = RobotBase.RobotAIState.UNKNOWN;
    }

    // Update is called once per frame
    void Update()
    {
        myNavMeshAgent.speed = _movementSpeed * (1 - robotBase.FlechetteProgress);

        if (_robotAIStateLastKnown != RobotAIStateCurrent)
        {
            var tmpState = _robotAIStateLastKnown;
            _robotAIStateLastKnown = RobotAIStateCurrent;
            OnAIStateChanged(tmpState, RobotAIStateCurrent);
        }

        bool attractableMagnetized = robotBase.myAttractable.IsMagnetized() && robotBase.FlechetteProgressReached;
        if (RobotAIStateCurrent == RobotBase.RobotAIState.MAGNETIZED
            && !attractableMagnetized)
        {
            RobotAIStateCurrent = RobotBase.RobotAIState.RAGDOLL;
        }

        if (RobotAIStateCurrent == RobotBase.RobotAIState.IDLE
            || RobotAIStateCurrent == RobotBase.RobotAIState.RAGDOLL
            || RobotAIStateCurrent == RobotBase.RobotAIState.FLECHETTESTUNNED
            || RobotAIStateCurrent == RobotBase.RobotAIState.POSITIONING
            || RobotAIStateCurrent == RobotBase.RobotAIState.ATTACKING
           )
        {
            if (attractableMagnetized)
            {
                RobotAIStateCurrent = RobotBase.RobotAIState.MAGNETIZED;
            }
            else if (robotBase.FlechetteProgressReached && RobotAIStateCurrent != RobotBase.RobotAIState.RAGDOLL)
            {
                RobotAIStateCurrent = RobotBase.RobotAIState.FLECHETTESTUNNED;
            }
        }
    }

    private void OnAIStateChanged(RobotBase.RobotAIState oldState, RobotBase.RobotAIState newState)
    {
        switch (oldState)
        {
            case RobotBase.RobotAIState.IDLE:
                break;
            case RobotBase.RobotAIState.MAGNETIZED:
                break;
            case RobotBase.RobotAIState.ATTACKING:
                sawBladeRotor.TurnOff();
                break;
            case RobotBase.RobotAIState.POSITIONING:
                RobotAIStateCurrent = RobotBase.RobotAIState.ATTACKING;
                break;
            case RobotBase.RobotAIState.UNKNOWN:
                break;
            case RobotBase.RobotAIState.FLECHETTESTUNNED:
                break;
            case RobotBase.RobotAIState.RAGDOLL:
                break;
            default:
                RobotAIStateCurrent = RobotBase.RobotAIState.UNKNOWN;
                break;
        }

        switch (newState)
        {
            case RobotBase.RobotAIState.IDLE:
                myNavMeshAgent.enabled = true;
                rb.isKinematic = true;
                break;
            case RobotBase.RobotAIState.MAGNETIZED:
                myNavMeshAgent.enabled = false;
                rb.isKinematic = false;
                break;
            case RobotBase.RobotAIState.ATTACKING:
                myNavMeshAgent.enabled = true;
                rb.isKinematic = true;
                sawBladeRotor.TurnOn();
                break;
            case RobotBase.RobotAIState.POSITIONING:
                myNavMeshAgent.enabled = true;
                rb.isKinematic = true;
                RobotAIStateCurrent = RobotBase.RobotAIState.ATTACKING;
                break;
            case RobotBase.RobotAIState.RAGDOLL:
                myNavMeshAgent.enabled = false;
                rb.isKinematic = false;
                break;
            case RobotBase.RobotAIState.FLECHETTESTUNNED:
                myNavMeshAgent.enabled = false;
                rb.isKinematic = true;
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

        if (myNavMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            RobotAIStateCurrent = RobotBase.RobotAIState.IDLE;
            //    myNavMeshAgent.SetDestination(transform.position);
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

        if (_gameState.playerState != GameState.PlayerState.PLAYING)
        {
            myNavMeshAgent.SetDestination(transform.position);
        }
    }
}