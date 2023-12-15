using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameState : MonoBehaviour
{
    private PlayerData _player;
    private PowerGun _powerGun;
    private CharacterMovement _movement;

    public enum PlayerState
    {
        PLAYING,
        DEAD,
        WIN
    }

    [Header("Creation templates")] public GameObject stickyFlechettePrefab;

    [Header("Gameplay constants")] public float magnetDPS = 35f;
    public int FPS = 60;
    public bool restartEnabled = false;

    [Header("Hookups")] public List<RobotBase> allRobots;
    public MusicManager musicManager;
    public Canvas uiCanvas;
    public UI ui;
    public GameObject uiHurtIndicatorPrefab;

    [Header("Player info")] public PlayerState playerState;

    public PlayerData player => _player;
    public PowerGun PowerGun => _powerGun;
    public CharacterMovement CharacterMovement => _movement;

    private void Awake()
    {
        _player = FindObjectOfType<PlayerData>();
        _powerGun = FindObjectOfType<PowerGun>();
        _movement = FindObjectOfType<CharacterMovement>();
    }

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating(nameof(UpdateMusic), 0, 1.337f);
        // Application.targetFrameRate = FPS;
        restartEnabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        _movement.enabled = playerState == PlayerState.PLAYING;
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene("PHScene");
    }

    public void UpdateMusic()
    {
        switch (playerState)
        {
            case PlayerState.WIN:
                musicManager.Play(1);
                break;
            case PlayerState.PLAYING:
                if (IsPlayerInDanger())
                {
                    musicManager.Play(0);
                }
                else
                {
                    musicManager.Play(1);
                }

                break;
            case PlayerState.DEAD:
                musicManager.Play(2);
                break;
            default:
                musicManager.Play(1);
                break;
        }
    }

    public bool IsPlayerInDanger()
    {
        foreach (RobotBase robot in allRobots)
        {
            if (robot.robotAIState == RobotBase.RobotAIState.ATTACKING
                || robot.robotAIState == RobotBase.RobotAIState.FLECHETTESTUNNED
                || robot.robotAIState == RobotBase.RobotAIState.POSITIONING
                || robot.robotAIState == RobotBase.RobotAIState.RAGDOLL
                || robot.robotAIState == RobotBase.RobotAIState.MAGNETIZED
               )
            {
                return true;
            }
        }

        return false;
    }

    public void DisplayDamageIndicator(Transform damageSource)
    {
        RectTransform rectTransform = uiCanvas.GetComponent<RectTransform>();
        GameObject indicatorGameObject = Instantiate(uiHurtIndicatorPrefab, rectTransform);

        UIDamageIndicator indicator = indicatorGameObject.GetComponent<UIDamageIndicator>();
        indicator.damageSourcePos = damageSource;

        ui.StartDamageOverlay();
    }

    public void Win()
    {
        playerState = PlayerState.WIN;
    }

    public void Loose()
    {
        playerState = PlayerState.DEAD;
        ui.DeathUI();
    }
}