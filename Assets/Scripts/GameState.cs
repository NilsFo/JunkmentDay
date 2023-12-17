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
    private BigMagnet _bigMagnet;
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
    public float playTime;

    [Header("Lists")] public List<RobotBase> allRobots;
    public List<StickyFlechette> allFlechettes;
    public List<RobotSpawner> allSpawners;

    [Header("World Hookup")] public MusicManager musicManager;
    public Canvas uiCanvas;
    public UI ui;
    public GameObject uiHurtIndicatorPrefab;

    [Header("Player info")] public PlayerState playerState;

    [Header("Encounters")] public int blockadesCount;
    public int blockadesDestroyed;

    public PlayerData player => _player;
    public PowerGun PowerGun => _powerGun;
    public BigMagnet BigMagnet => _bigMagnet;
    public CharacterMovement CharacterMovement => _movement;

    private void Awake()
    {
        _player = FindObjectOfType<PlayerData>();
        _powerGun = FindObjectOfType<PowerGun>();
        _movement = FindObjectOfType<CharacterMovement>();
        _bigMagnet = FindObjectOfType<BigMagnet>();

        allSpawners = new List<RobotSpawner>();
        allRobots = new List<RobotBase>();
        allFlechettes = new List<StickyFlechette>();
    }

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating(nameof(UpdateMusic), 0, 1.337f);
        // Application.targetFrameRate = FPS;
        restartEnabled = false;
        StopAllRobotSpawns();
    }

    // Update is called once per frame
    void Update()
    {
        _movement.enabled = playerState == PlayerState.PLAYING;

        if (playerState == PlayerState.PLAYING)
        {
            playTime += Time.deltaTime;
        }
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
                    if (blockadesDestroyed >= 6)
                    {
                        musicManager.Play(3);
                    }
                    else
                    {
                        musicManager.Play(0);
                    }
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

    public void RequestWin()
    {
        if (playerState == PlayerState.WIN
            || playerState == PlayerState.DEAD
           )
        {
            return;
        }

        if (blockadesCount == blockadesDestroyed)
        {
            Win();
        }
    }

    public void Win()
    {
        Debug.LogWarning("A winner is you!");
        playerState = PlayerState.WIN;
        ui.WinUI();
    }

    public void Loose()
    {
        playerState = PlayerState.DEAD;
        ui.DeathUI();
    }

    public void StopAllRobotSpawns()
    {
        foreach (RobotSpawner spawner in allSpawners)
        {
            spawner.autoSpawn = false;
        }
    }
}