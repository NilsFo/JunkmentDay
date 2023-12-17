using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CatapultShot : MonoBehaviour
{
    private GameState _gameState;
    public CatapultRobotAI shooter;
    public int damage = 10;

    public List<AudioClip> creationClips;
    public List<AudioClip> impactClips;

    private void Awake()
    {
        _gameState = FindObjectOfType<GameState>();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer.Equals(LayerMask.NameToLayer("Player")))
        {
            var playerData = other.collider.GetComponentInParent<PlayerData>();
            if (playerData == null)
                return;
            Debug.Log("Catapult Shot is hurting the Player for " + damage + " damage", this);
            playerData.Damage(damage, shooter.transform);
        }

        _gameState.musicManager.CreateAudioClip(
            impactClips[Random.Range(0, impactClips.Count)],
            transform.position,
            pitchRange: 0.1f,
            soundInstanceVolumeMult: 0.69f,
            respectBinning: true
        );

        Destroy(gameObject);
    }

    private void Start()
    {
        _gameState.musicManager.CreateAudioClip(
            creationClips[Random.Range(0, creationClips.Count)],
            transform.position,
            pitchRange: 0.1f,
            soundInstanceVolumeMult: 0.69f,
            respectBinning: true
        );
    }
}