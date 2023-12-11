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
    public float shootDistance = 15f;
    public float fleeDistance = 3f;

    public float shootPeriod = 3f;
    private float _shootTimer;

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
    }
    
    
    private void NavigationUpdate() {
        if (aiState == CatapultRobotState.IDLE) {
            if(_gameState.player.GetDistanceToPlayer(head.position) < playerDetectionDistance && _gameState.player.PlayerInView(head.position))
                aiState = CatapultRobotState.POSITIONING;
        }
        if (aiState == CatapultRobotState.POSITIONING) {
            myNavMeshAgent.SetDestination(_gameState.player.transform.position);
            var playerDistance = _gameState.player.GetDistanceToPlayer(head.position);
            if (playerDistance < shootDistance) {
                aiState = CatapultRobotState.ATTACKING;
            }
        }
        if (aiState == CatapultRobotState.ATTACKING) {
            // Stop
            myNavMeshAgent.SetDestination(myNavMeshAgent.transform.position);
        }
    }

    public void ShootCatapult() {
        
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
