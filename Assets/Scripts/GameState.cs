using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    private PlayerData _player;
    public GameObject stickyFlechettePrefab;

    public PlayerData player => _player;

    private void Awake()
    {
        _player = FindObjectOfType<PlayerData>();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
}