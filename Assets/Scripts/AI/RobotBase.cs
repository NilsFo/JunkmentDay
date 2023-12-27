using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class RobotBase : MonoBehaviour
{
    [Header("Hookup")] public Attractable myAttractable;
    public Markable myMarkable;
    public RobotAIState robotAIState = RobotBase.RobotAIState.UNKNOWN;
    private RobotAIState _robotAIState;
    private GameState _gameState;
    public TMP_Text debugTF;
    public Rigidbody rb;
    public List<ParticleSystem> stunnedParticles;

    [Header("AI Config")] public Transform head;
    public float playerDetectionDistance = 50f;
    public float getUpTimer = 1.337f;
    public float getUpDeathTimer = 8f;
    private float _getUpTimer;
    private float _getUpDeathTimer;
    private bool _pullNextFrame;
    private int _flechetteCount;
    public bool blinded;

    [Header("Sounds")] public RobotAudioCollection robotAudioCollection;
    public float barkTimer = 8f;
    private float _barkTimer;
    public LayerMask bumpLayerMask;

    [Header("The machine spirit")] public float health;
    public float healthRegen = 5;
    private float _healthCurrent;
    private float _damageBuffer = 0f;
    private Vector3 _positionCache = Vector3.zero;
    private Vector3 _rotationCache = Vector3.zero;
    private float _originalDampening;

    private bool _markedForMagnetDeath;
    private Vector3 _markForDeathLocation;

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
        _barkTimer = 0;
    }

    private void Start()
    {
        if (onDeath == null)
        {
            onDeath = new UnityEvent();
        }

        _gameState.allRobots.Add(this);
        _getUpTimer = 0;
        _getUpDeathTimer = 0;
        _originalDampening = rb.drag;
        _markedForMagnetDeath = false;
        _markForDeathLocation = new Vector3();
    }

    private void Update()
    {
        _barkTimer -= Time.deltaTime;

        if (_robotAIState != robotAIState)
        {
            var tmpState = _robotAIState;
            _robotAIState = robotAIState;
            OnAIStateChanged(tmpState, robotAIState);
        }

        if (_flechetteCount != FlechetteCount)
        {
            _getUpDeathTimer = 0;
        }

        if (robotAIState == RobotAIState.RAGDOLL)
        {
            _getUpDeathTimer += Time.deltaTime;
            if (RigidBodyChangeRate() <= 0.01f)
            {
                _getUpTimer += Time.deltaTime;
                if (_getUpTimer >= getUpTimer)
                {
                    robotAIState = RobotAIState.IDLE;
                    // Debug.Log("Enough of this ragdoll.");
                }
            }
            else
            {
                _getUpTimer = 0;
            }

            if (_getUpDeathTimer >= getUpDeathTimer)
            {
                DealDamage(1000f);
            }
        }
        else
        {
            _getUpTimer = 0;
            _getUpDeathTimer = 0;
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
            text = text + "\nR: " + _getUpTimer + "/" + getUpTimer + "\n" +
                   "Pos: " + RigidBodyChangeRate();
        }

        debugTF.text = text;

        foreach (ParticleSystem particle in stunnedParticles)
        {
            ParticleSystem.EmissionModule particleEmission = particle.emission;
            particleEmission.enabled = robotAIState == RobotAIState.FLECHETTESTUNNED;
        }

        if (robotAIState == RobotAIState.UNKNOWN)
        {
            Debug.LogError("Robot has an unknown state!", gameObject);
            _healthCurrent = health;
        }

        // kill
        if (_healthCurrent <= 0f)
        {
            Kill();
        }

        // Kill Plane
        if (transform.position.y <= -100)
        {
            DealDamage(100000f);
        }

        if (_markedForMagnetDeath)
        {
            DealDamage(80f * Time.deltaTime);
        }

        // GameOver
        if (_gameState.playerState != GameState.PlayerState.PLAYING
            && robotAIState != RobotAIState.FLECHETTESTUNNED
            && robotAIState != RobotAIState.RAGDOLL
           )
        {
            robotAIState = RobotAIState.IDLE;
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
        // Debug.Log(name + " - Robot state changed: " + oldState + " -> " + newState, gameObject);

        switch (oldState)
        {
            case RobotAIState.MAGNETIZED:
                // myMarkable.RemoveAllFlechettes();
                rb.drag = _originalDampening;
                rb.excludeLayers = 0;
                break;
            case RobotAIState.RAGDOLL:
                // myMarkable.RemoveAllFlechettes();
                rb.excludeLayers = 0;
                break;
            case RobotAIState.FLECHETTESTUNNED:
                break;
            default:
                break;
        }

        switch (newState)
        {
            case RobotAIState.ATTACKING:
                HelloSound();
                break;
            case RobotAIState.POSITIONING:
                HelloSound();
                break;
            case RobotAIState.MAGNETIZED:
                rb.drag = 2f;
                rb.excludeLayers = 1 << LayerMask.NameToLayer("Enemies");
                break;
            case RobotAIState.RAGDOLL:
                rb.excludeLayers = 1 << LayerMask.NameToLayer("Enemies");
                break;
            default:
                break;
        }
    }

    public void HelloSound()
    {
        if (_barkTimer <= 0)
        {
            // Debug.Log("Playing 'hello' sound from: " + name, gameObject);
            robotAudioCollection.Play(robotAudioCollection.NextHelloSound(), true);
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

        if ((bumpLayerMask.value & (1 << collision.transform.gameObject.layer)) > 0)
        {
            _gameState.musicManager.CreateAudioClip(
                robotAudioCollection.NetxtBumpSound(),
                transform.position,
                pitchRange: 0.1f,
                soundVolume: 0.95f,
                respectBinning: true
            );
        }
    }

    public void Kill()
    {
        Debug.LogWarning("Robot " + name + " has died. RIP in pieces.");
        myMarkable.RemoveAllFlechettes();
        myAttractable.OnRobotDeath();
        onDeath.Invoke();

        _gameState.player.killCount++;

        robotAudioCollection.transform.parent = null;
        robotAudioCollection.timedLife.timerActive = true;
        robotAudioCollection.Play(robotAudioCollection.NextDeathSound(), false);

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        _gameState.allRobots.Remove(this);
    }

    public bool PlayerDetected()
    {
        return GetDistanceToPlayer() <= playerDetectionDistance && PlayerInView() && !blinded;
    }

    public float GetDistanceToPlayer()
    {
        return _gameState.player.GetDistanceToPlayer(head.position);
    }

    public bool PlayerInView()
    {
        return _gameState.player.PlayerInView(head.position) && !blinded;
    }

    public void ResetVisionRange()
    {
        playerDetectionDistance = 50f;
    }

    public void RequestPullToPlayer()
    {
        if (FlechetteCount > 0
            && robotAIState != RobotAIState.UNKNOWN
            // && robotAIState != RobotAIState.RAGDOLL
            && robotAIState != RobotAIState.MAGNETIZED)
        {
            robotAIState = RobotAIState.RAGDOLL;
            _pullNextFrame = true;
        }
    }

    private float RigidBodyChangeRate()
    {
        float m = Mathf.Abs(_positionCache.magnitude - transform.position.magnitude)
                  + Mathf.Abs(_rotationCache.magnitude - transform.rotation.eulerAngles.magnitude);
        return Mathf.Abs(m);
    }

    public void PullToPlayer()
    {
        // Debug.Log("Pulling " + name + " to player.");
        if (robotAIState != RobotAIState.RAGDOLL)
        {
            Debug.LogError("Failed to pull. Robot is no ragdoll!");
            return;
        }

        _pullNextFrame = false;
        _getUpDeathTimer = 0;
        _getUpTimer = 0;

        int flechetteCount = FlechetteCount;
        float flechetteProgress = FlechetteProgress;

        Vector3 playerPos = _gameState.player.transform.position;
        Vector3 myPos = transform.position;

        float b = 30f * Mathf.Deg2Rad; // Angle of the shot
        Vector3 toPlayerVec = Vector3.ProjectOnPlane(playerPos - myPos, Vector3.up);
        float d = toPlayerVec.magnitude; // Distance to the player on the y plane
        float y = (playerPos - myPos).y; // Height difference to the player
        float g = 9.8f; // Gravity

        // Calculate necessary velocity
        float v = Mathf.Sqrt(-0.5f * g * d * d / (Mathf.Pow(Mathf.Cos(b), 2) * (y - d * Mathf.Tan(b))));

        Vector3 direction = Quaternion.AngleAxis(b * Mathf.Rad2Deg,
            Vector3.Cross(toPlayerVec, Vector3.up)) * toPlayerVec;
        v = Mathf.Clamp(v, 5f, 30f);
        Vector3 force = direction.normalized * v * flechetteProgress * 0.9f;

        if (float.IsNaN(direction.x) || float.IsNaN(direction.y) || float.IsNaN(direction.z) ||
            float.IsNaN(force.x) || float.IsNaN(force.y) || float.IsNaN(force.z))
        {
            Debug.LogError("Failed to calculate path for pulling robot!", gameObject);
            return;
        }

        rb.AddForce(force, ForceMode.VelocityChange);
        myMarkable.RemoveAllFlechettes();
    }

    public void MarkForMagnetDeath()
    {
        if (!_markedForMagnetDeath)
        {
            _markedForMagnetDeath = true;
            _markForDeathLocation = transform.position;
            rb.drag = 8f;
        }
    }
}