using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class CatapultRobotAI : MonoBehaviour
{
    public NavMeshAgent myNavMeshAgent;

    public float shootDistance = 20f;
    public float fleeDistance = 3f;
    private float _movementSpeed;

    public float shootPeriod = 3f;
    private float _shootTimer;
    public Transform shootOrigin;

    public Rigidbody projectilePrefab;
    public Rigidbody rb;

    public RobotBase robotBase;

    public RobotBase.RobotAIState RobotAIStateCurrent
    {
        get => robotBase.robotAIState;
        set => robotBase.robotAIState = value;
    }

    private RobotBase.RobotAIState _robotAIStateLastKnown = RobotBase.RobotAIState.UNKNOWN;

    private GameState _gameState;

    void Awake()
    {
        _gameState = FindObjectOfType<GameState>();
    }

    private void Start()
    {
        _movementSpeed = myNavMeshAgent.speed;
        _robotAIStateLastKnown = RobotBase.RobotAIState.UNKNOWN;
    }

    private void OnEnable() {
        InvokeRepeating(nameof(NavigationUpdate), 0f, 0.5f);
    }

    private void OnDisable() {
        CancelInvoke(nameof(NavigationUpdate));
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

        if (RobotAIStateCurrent == RobotBase.RobotAIState.ATTACKING
            || RobotAIStateCurrent == RobotBase.RobotAIState.POSITIONING)
        {
            if (_shootTimer > 0)
            {
                _shootTimer -= Time.deltaTime;
                if (_shootTimer <= 0)
                {
                    _shootTimer = 0;
                }
            }
        }

        if (RobotAIStateCurrent == RobotBase.RobotAIState.ATTACKING)
        {
            // Turn towards player
            var delta = RotateTowards(_gameState.player.transform.position);
            if (Mathf.Abs(delta) < 0.01f && _shootTimer <= 0)
            {
                // Shoot
                _shootTimer = shootPeriod;
                ShootCatapult();
            }
        }

        if (RobotAIStateCurrent == RobotBase.RobotAIState.UNKNOWN)
        {
            Debug.LogError("Robot has an unknown state!", gameObject);
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
                break;
            case RobotBase.RobotAIState.POSITIONING:
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
                break;
            case RobotBase.RobotAIState.POSITIONING:
                myNavMeshAgent.enabled = true;
                rb.isKinematic = true;
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
        if (!myNavMeshAgent.enabled)
        {
            return;
        }

        if (RobotAIStateCurrent == RobotBase.RobotAIState.IDLE)
        {
            if (robotBase.PlayerDetected())
                RobotAIStateCurrent = RobotBase.RobotAIState.POSITIONING;
        }

        if (RobotAIStateCurrent == RobotBase.RobotAIState.POSITIONING)
        {
            myNavMeshAgent.SetDestination(_gameState.player.transform.position);
            var playerDistance = robotBase.GetDistanceToPlayer();
            if (playerDistance < shootDistance && robotBase.PlayerInView())
            {
                RobotAIStateCurrent = RobotBase.RobotAIState.ATTACKING;
            }
        }

        if (RobotAIStateCurrent == RobotBase.RobotAIState.ATTACKING)
        {
            // Stop
            myNavMeshAgent.SetDestination(myNavMeshAgent.transform.position);
            var playerDistance = robotBase.GetDistanceToPlayer();
            if (playerDistance > shootDistance + 1)
            {
                RobotAIStateCurrent = RobotBase.RobotAIState.POSITIONING;
            }
            else if (!robotBase.PlayerInView())
            {
                RobotAIStateCurrent = RobotBase.RobotAIState.POSITIONING;
            }
        }

        if (_gameState.playerState != GameState.PlayerState.PLAYING)
        {
            myNavMeshAgent.SetDestination(transform.position);
        }
    }

    private float CalculateTrajectoryForce(Vector3 srcPos, Vector3 targetPos, float angle) {
        float b = angle * Mathf.Deg2Rad; // Angle of the shot
        float d = Vector3.ProjectOnPlane(targetPos - srcPos, Vector3.up)
            .magnitude; // Distance to the player on the y plane
        float y = (targetPos - srcPos).y; // Height difference to the player
        float g = 9.8f; // Gravity

        // Calculate necessary force
        float v = Mathf.Sqrt(-0.5f * g * d * d / (Mathf.Pow(Mathf.Cos(b), 2) * (y - d * Mathf.Tan(b))));
        return v;
    }

    public void ShootCatapult()
    {
        Vector3 playerPos = _gameState.player.transform.position + Vector3.up;
        Vector3 myPos = shootOrigin.position;

        float angle = 20f;
        Vector3 velocity = Vector3.zero;
        do {
            float v = CalculateTrajectoryForce(myPos, playerPos, angle);
            Vector3 direction = Quaternion.AngleAxis(angle, -shootOrigin.right) * shootOrigin.forward;
            v = Mathf.Clamp(v, 5f, 30f);
            velocity = direction.normalized * v;

            if (float.IsNaN(direction.x) || float.IsNaN(direction.y) || float.IsNaN(direction.z) ||
                float.IsNaN(velocity.x) || float.IsNaN(velocity.y) || float.IsNaN(velocity.z)) {
                angle += 10f;
                if (angle >= 90) {
                    Debug.LogWarning("Failed to calculate path for projectile!", gameObject);
                    return;
                }
            } else {
                break;
            }
        } while (true);

        Rigidbody projectile = Instantiate(projectilePrefab, shootOrigin.position, shootOrigin.rotation);
        projectile.velocity = velocity;
        projectile.angularVelocity = Vector3.Scale(Random.insideUnitSphere, (new Vector3(10f, 1f, 10f)));
        projectile.GetComponent<CatapultShot>().shooter = this;
    }

    private float RotateTowards(Vector3 target)
    {
        var turnSpeed = myNavMeshAgent.angularSpeed;
        var alpha = turnSpeed * Time.deltaTime;
        var d = target - transform.position;

        // Turn body
        d.y = 0;
        d.Normalize();
        var lookVector = transform.rotation * Vector3.forward;
        var lookDelta = Vector3.Cross(lookVector, d).y;

        if (lookDelta < 0)
        {
            alpha *= -1;
        }

        Quaternion rot;
        if (Mathf.Abs(lookDelta) <= Mathf.Abs(alpha))
        {
            rot = Quaternion.Euler(0, Mathf.Asin(lookDelta) * Mathf.Rad2Deg, 0);
        }
        else
        {
            rot = Quaternion.Euler(0, alpha, 0);
        }

        transform.rotation *= rot;
        return lookDelta;
    }
}