using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIDamageIndicator : MonoBehaviour
{
    private GameState _gameState;
    public Transform damageSourcePos;
    public TimedLife timedLife;
    public RectTransform myTransform;

    private void Awake()
    {
        _gameState = FindObjectOfType<GameState>();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (_gameState.playerState != GameState.PlayerState.PLAYING)
        {
            Destroy(gameObject);
            return;
        }

        if (damageSourcePos.IsDestroyed())
        {
            Destroy(gameObject);
            return;
        }

        Vector3 playerPos = _gameState.player.transform.position;
        Vector3 otherPos = damageSourcePos.position;

        // playerPos.y = 0;
        // otherPos.y = 0;
        Vector3 direction = playerPos - otherPos;
        var lookRotation = Quaternion.LookRotation(direction);
        lookRotation.z = -lookRotation.y;
        lookRotation.x = 0;
        lookRotation.y = 0;

        Vector3 northDirection = new Vector3(0, 0, _gameState.player.transform.eulerAngles.y);
        myTransform.localRotation = lookRotation * Quaternion.Euler(northDirection);
    }
}