using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CatapultRobotAI : MonoBehaviour
{
    
    public Attractable myAttractable;
    public Markable myMarkable;
    public NavMeshAgent myNavMeshAgent;
    public Transform head;

    public float playerDetectionDistance = 50f;
    public float shootDistance = 20f;
    public float fleeDistance = 3f;

    public float shootPeriod = 3f;
    private float _shootTimer;
    public Transform shootOrigin;

    public Rigidbody projectilePrefab;

    public enum CatapultRobotState {
        IDLE,
        ATTACKING,
        POSITIONING,
        MAGNETIZED
    }
    public CatapultRobotState aiState;
    
    private GameState _gameState;

    void Awake() {
        _gameState = FindObjectOfType<GameState>();
        InvokeRepeating(nameof(NavigationUpdate), 0f, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        if (_shootTimer > 0) {
            _shootTimer -= Time.deltaTime;
            if (_shootTimer <= 0) {
                _shootTimer = 0;
            }
        }
        if (aiState == CatapultRobotState.ATTACKING) {
            // Turn towards player
            var delta = RotateTowards(_gameState.player.transform.position);
            if (Mathf.Abs(delta) < 0.01f && _shootTimer <= 0) {
                // Shoot
                _shootTimer = shootPeriod;
                ShootCatapult();
            }
        }
        
        if (myAttractable.isMagnetized()) {
            aiState = CatapultRobotState.MAGNETIZED;
        }
        if (aiState == CatapultRobotState.MAGNETIZED) {
            myNavMeshAgent.enabled = false;
        }
    }
    
    
    private void NavigationUpdate() {
        if (aiState == CatapultRobotState.IDLE) {
            if(_gameState.player.GetDistanceToPlayer(head.position) < playerDetectionDistance && PlayerInView())
                aiState = CatapultRobotState.POSITIONING;
        }
        if (aiState == CatapultRobotState.POSITIONING) {
            myNavMeshAgent.SetDestination(_gameState.player.transform.position);
            var playerDistance = GetDistanceToPlayer();
            if (playerDistance < shootDistance && PlayerInView()) {
                aiState = CatapultRobotState.ATTACKING;
            }
        }
        if (aiState == CatapultRobotState.ATTACKING) {
            // Stop
            myNavMeshAgent.SetDestination(myNavMeshAgent.transform.position);
            var playerDistance = GetDistanceToPlayer();
            if (playerDistance > shootDistance + 1) {
                aiState = CatapultRobotState.POSITIONING;
            }
            else if (!PlayerInView()) {
                aiState = CatapultRobotState.POSITIONING;
            }
        }
    }
    
    private float GetDistanceToPlayer() {

        return _gameState.player.GetDistanceToPlayer(head.position);
    }
    private bool PlayerInView() {

        return _gameState.player.PlayerInView(head.position);
    }

    public void ShootCatapult() {
        var playerPos = _gameState.player.transform.position + Vector3.up;
        var myPos = shootOrigin.position;
        
        var b = 30f * Mathf.Deg2Rad;  // Angle of the shot
        var d = Vector3.ProjectOnPlane(playerPos - myPos, Vector3.up).magnitude;  // Distance to the player on the y plane
        var y = (playerPos - myPos).y;  // Height difference to the player
        var g = 9.8f;  // Gravity

        // Calculate necessary force
        float v = Mathf.Sqrt(-0.5f * g * d * d / (Mathf.Pow(Mathf.Cos(b), 2) * (y - d * Mathf.Tan(b))));

        var projectile = Instantiate(projectilePrefab, shootOrigin.position, shootOrigin.rotation);
        Vector3 direction = Quaternion.AngleAxis(b * Mathf.Rad2Deg, -shootOrigin.right) * shootOrigin.forward;
        v = Mathf.Clamp(v, 5f, 30f);
        projectile.velocity = direction.normalized * v;
    }
    
    private float RotateTowards(Vector3 target) {
        var turnSpeed = myNavMeshAgent.angularSpeed;
        var alpha = turnSpeed * Time.deltaTime;
        var d = target - transform.position;

        // Turn body
        d.y = 0;
        d.Normalize();
        var lookVector = transform.rotation * Vector3.forward;
        var lookDelta = Vector3.Cross(lookVector, d).y;

        if (lookDelta < 0) {
            alpha *= -1;
        }
        Quaternion rot;
        if (Mathf.Abs(lookDelta) <= Mathf.Abs(alpha)) {
            rot = Quaternion.Euler(0, Mathf.Asin(lookDelta), 0);
        } else {
            rot = Quaternion.Euler(0, alpha, 0);
        }
        transform.rotation *= rot;
        return lookDelta;

    }
}
