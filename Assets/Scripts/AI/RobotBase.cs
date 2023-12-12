using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class RobotBase : MonoBehaviour
{
    [Header("Hookup")] public Attractable myAttractable;
    public Markable myMarkable;
    public RobotAIState robotAIState = RobotBase.RobotAIState.UNKNOWN;
    private RobotAIState _robotAIState;
    private GameState _gameState;
    public TMP_Text debugTF;

    [Header("AI Config")] public Transform head;
    public float playerDetectionDistance = 50f;

    [Header("The machine spirit")] public float health;
    public float healthRegen = 5;
    private float _healthCurrent;
    private float _damageBuffer = 0f;

    [Header("Callbacks")] public UnityEvent onDeath;

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
        _robotAIState = robotAIState;
        _gameState = FindObjectOfType<GameState>();
        _healthCurrent = health;
        _damageBuffer = 0;
    }

    private void Start()
    {
        if (onDeath == null)
        {
            onDeath = new UnityEvent();
        }
    }

    private void Update()
    {
        if (_robotAIState != robotAIState)
        {
            OnAIStateChanged(_robotAIState, robotAIState);
            _robotAIState = robotAIState;
        }

        debugTF.text = name + "\n" +
                       robotAIState + "\n" +
                       "HP:" + _healthCurrent + "/" + health;

        if (robotAIState == RobotBase.RobotAIState.UNKNOWN)
        {
            Debug.LogError("Robot has an unknown state!", gameObject);
            _healthCurrent = health;
        }

        if (_healthCurrent <= 0f)
        {
            Kill();
        }
    }

    private void LateUpdate()
    {
        if (robotAIState != RobotAIState.MAGNETIZED && _damageBuffer == 0f)
        {
            _healthCurrent += healthRegen * Time.deltaTime;
        }

        if (_damageBuffer > 0)
        {
            _healthCurrent -= _damageBuffer;
            _damageBuffer = 0;
        }

        _healthCurrent = MathF.Min(_healthCurrent, health);
    }


    private void OnAIStateChanged(RobotAIState oldState, RobotAIState newState)
    {
        Debug.Log("Robot '" + name + "' new state: " + newState);

        switch (oldState)
        {
            case RobotAIState.MAGNETIZED:
                myMarkable.RemoveAllFlechettes();
                break;
            default:
                break;
        }

        switch (newState)
        {
            default:
                break;
        }
    }

    public void DealDamage(float damage)
    {
        _damageBuffer = MathF.Max(_damageBuffer, damage);
    }

    public void Kill()
    {
        Debug.LogWarning("Robot " + name + " has died. RIP in pieces.");
        myMarkable.RemoveAllFlechettes();
        myAttractable.OnRobotDeath();

        Destroy(gameObject);
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