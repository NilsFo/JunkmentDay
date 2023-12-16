using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healthpack : MonoBehaviour {
    public bool collected;
    public int healAmount = 25;
    private GameState _gameState;
    // Start is called before the first frame update
    void Start() {
        _gameState = FindObjectOfType<GameState>();
    }

    // Update is called once per frame
    void Update()
    {
        if (collected) {
            var playerPos = _gameState.player.transform.position + Vector3.up;
            transform.position = Vector3.MoveTowards(transform.position, playerPos, 10f * Time.deltaTime);
            if (Vector3.Distance(transform.position, playerPos) < 0.1f) {
                _gameState.player.ModHealth(healAmount);
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerStay(Collider other) {
        var player = other.GetComponent<PlayerData>();
        if (player != null) {
            if (player.PlayerInView(transform.position)) {
                if (player.CurrentHealth == player.maxHealth)
                    return;
                collected = true;
                Destroy(GetComponent<Rigidbody>());
                Destroy(GetComponent<BoxCollider>());
                Destroy(GetComponent<SphereCollider>());
            }
        }
    }
}
