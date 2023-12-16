using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blockade : MonoBehaviour
{

    private GameState _gameState;
    public float destructionDelay = 3f;
    // Start is called before the first frame update

    private void Awake()
    {
        _gameState = FindObjectOfType<GameState>();
        _gameState.blockadesCount++;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void DestroyBlockade() {
        Invoke(nameof(DestroyBlockadeNow), destructionDelay);
    }

    private void DestroyBlockadeNow()
    {
        _gameState.blockadesDestroyed++;
        
        var magnet = FindObjectOfType<BigMagnet>();
        foreach (var debris in GetComponentsInChildren<MoveToAndDestroy>()) {
            debris.target = magnet.GetComponentInChildren<Attractor>().transform;
            debris.enabled = true;
            debris.transform.parent = null;
        }
        Destroy(gameObject);
    }
}
