using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RobotAI : MonoBehaviour
{

    public Attractable myAttractable;
    public Markable myMarkable;
    public NavMeshAgent myNavMeshAgent;

    public enum RobotState {
        IDLE,
        ATTACKING,
        MAGNETIZED
    }

    public RobotState aiState = RobotState.IDLE;
    
    

    private GameState _gameState;
    
    // Start is called before the first frame update
    void Awake() {
        _gameState = FindObjectOfType<GameState>();
        InvokeRepeating(nameof(NavigationUpdate), 0f, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate() {
        if (myAttractable.isMagnetized()) {
            aiState = RobotState.MAGNETIZED;
        }
        if (aiState == RobotState.MAGNETIZED) {
            myNavMeshAgent.enabled = false;
        }
    }

    private void NavigationUpdate() {
        if (aiState == RobotState.IDLE) {
            aiState = RobotState.ATTACKING;
        }
        if(aiState == RobotState.ATTACKING)
            myNavMeshAgent.SetDestination(_gameState.player.transform.position);
    }
}
