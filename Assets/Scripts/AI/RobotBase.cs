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
    public Rigidbody rb;

    [Header("AI Config")] public Transform head;
    public float playerDetectionDistance = 50f;
    public float ragDollTime = 1f;
    private float _ragDollTime;
    private bool _pullNextFrame;

    [Header("The machine spirit")] public float health;
    public float healthRegen = 5;
    private float _healthCurrent;
    private float _damageBuffer = 0f;
    private Vector3 _positionCache = Vector3.zero;
    private Vector3 _rotationCache = Vector3.zero;

    public int flechetteStunThreshold = 5;
    public int FlechetteCount => myMarkable.myFlechettes.Count;
    public float FlechetteProgress => MathF.Min((float)(FlechetteCount) / flechetteStunThreshold, 1);
    public bool FlechetteProgressReached => FlechetteProgress >= 1f;

    [Header("Callbacks")] public UnityEvent onDeath;

    public enum RobotAIState
    {
        UNKNOWN,
        IDLE,
        ATTACKING,
        POSITIONING,
        FLECHETTESTUNNED,
        RAGDOLL,
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

        _gameState.allRobots.Add(this);
        _ragDollTime = 0;
    }

    private void Update()
    {
        if (_robotAIState != robotAIState)
        {
            var tmpState = _robotAIState;
            _robotAIState = robotAIState;
            OnAIStateChanged(tmpState, robotAIState);
        }

        if (robotAIState == RobotAIState.RAGDOLL)
        {
            _ragDollTime += Time.deltaTime;
            if (_ragDollTime >= ragDollTime)
            {
                robotAIState = RobotAIState.IDLE;
            }
        }
        else
        {
            _ragDollTime = 0;
        }

        if (robotAIState == RobotAIState.FLECHETTESTUNNED && !FlechetteProgressReached)
        {
            robotAIState = RobotAIState.IDLE;
        }

        string text = name + "\n" +
                      robotAIState + "\n" +
                      "HP:" + (int)_healthCurrent + "/" + (int)health + "\n" +
                      "F:" + FlechetteCount + "/" + flechetteStunThreshold + " -> " + (int)(FlechetteProgress * 100f) +
                      "%";
        if (robotAIState == RobotAIState.RAGDOLL)
        {
            text = text + "\nR: " + _ragDollTime + "/" + ragDollTime + "\n" +
                   "Pos: " + RigidBodyChangeRate();
        }

        debugTF.text = text;

        if (robotAIState == RobotAIState.UNKNOWN)
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

        if (_healthCurrent > 0 && _pullNextFrame && !rb.isKinematic)
        {
            PullToPlayer();
            _pullNextFrame = false;
        }
    }

    private void FixedUpdate()
    {
        _positionCache = transform.position;
        _rotationCache = transform.rotation.eulerAngles;
    }

    private void OnAIStateChanged(RobotAIState oldState, RobotAIState newState)
    {
        switch (oldState)
        {
            case RobotAIState.MAGNETIZED:
                myMarkable.RemoveAllFlechettes();
                break;
            case RobotAIState.RAGDOLL:
                myMarkable.RemoveAllFlechettes();
                break;
            case RobotAIState.FLECHETTESTUNNED:
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

    private void OnCollisionEnter(Collision collision)
    {
        var other = collision.gameObject;
        RobotBase otherBase = other.GetComponent<RobotBase>();

        if (otherBase != null)
        {
            if (robotAIState == RobotAIState.MAGNETIZED && otherBase.robotAIState == RobotAIState.MAGNETIZED)
            {
                otherBase.DealDamage(1000000f);
                DealDamage(1000000f);
            }
        }
    }

    public void Kill()
    {
        Debug.LogWarning("Robot " + name + " has died. RIP in pieces.");
        myMarkable.RemoveAllFlechettes();
        myAttractable.OnRobotDeath();

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        _gameState.allRobots.Remove(this);
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

    public void RequestPullToPlayer()
    {
        if (FlechetteCount > 0
            && robotAIState != RobotAIState.UNKNOWN
            && robotAIState != RobotAIState.RAGDOLL
            && robotAIState != RobotAIState.MAGNETIZED)
        {
            robotAIState = RobotAIState.RAGDOLL;
            _pullNextFrame = true;
        }
    }

    private float RigidBodyChangeRate()
    {
        return (_positionCache.magnitude - transform.position.magnitude)
               + (_rotationCache.magnitude - transform.rotation.eulerAngles.magnitude);
    }

    public void PullToPlayer()
    {
        Debug.Log("Pulling " + name + " to player.");
        if (robotAIState != RobotAIState.RAGDOLL)
        {
            Debug.LogError("Failed to pull. Robot is no ragdoll!");
            return;
        }

        _pullNextFrame = false;
        int flechetteCount = FlechetteCount;
        myMarkable.RemoveAllFlechettes();

        
        var playerPos = _gameState.player.transform.position + Vector3.up;
        var myPos = transform.position;

        var b = 30f * Mathf.Deg2Rad; // Angle of the shot
        var toPlayerVec = Vector3.ProjectOnPlane(playerPos - myPos, Vector3.up);
        var d = toPlayerVec.magnitude; // Distance to the player on the y plane
        var y = (playerPos - myPos).y; // Height difference to the player
        var g = 9.8f; // Gravity

        // Calculate necessary velocity
        float v = Mathf.Sqrt(-0.5f * g * d * d / (Mathf.Pow(Mathf.Cos(b), 2) * (y - d * Mathf.Tan(b))));

        Vector3 direction = Quaternion.AngleAxis(b * Mathf.Rad2Deg, Vector3.Cross(toPlayerVec, Vector3.up)) * toPlayerVec;
        v = Mathf.Clamp(v, 5f, 30f);
        
        rb.AddForce(direction * v * FlechetteProgress, ForceMode.VelocityChange);
    }
}