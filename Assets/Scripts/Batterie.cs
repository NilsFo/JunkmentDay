using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Batterie : MonoBehaviour
{
    public bool collected;

    private GameState _gameState;

    // Start is called before the first frame update
    void Start()
    {
        _gameState = FindObjectOfType<GameState>();
    }

    // Update is called once per frame
    void Update()
    {
        if (collected)
        {
            var playerPos = _gameState.player.transform.position + Vector3.up;
            transform.position = Vector3.MoveTowards(transform.position, playerPos, 10f * Time.deltaTime);
            if (Vector3.Distance(transform.position, playerPos) < 0.1f)
            {
                _gameState.player.ModEnergy(1);
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        var player = other.GetComponent<PlayerData>();
        if (player != null)
        {
            if (player.PlayerInView(transform.position))
            {
                collected = true;
                _gameState.ui.StartBatteryOverlay();
                Destroy(GetComponent<Rigidbody>());
                Destroy(GetComponent<BoxCollider>());
                Destroy(GetComponent<SphereCollider>());
            }
        }
    }
}