using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    private PlayerData _player;
    private PowerGun _powerGun;
    public GameObject stickyFlechettePrefab;

    public PlayerData player => _player;
    public PowerGun PowerGun => _powerGun;

    private void Awake()
    {
        _player = FindObjectOfType<PlayerData>();
        _powerGun = FindObjectOfType<PowerGun>();
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