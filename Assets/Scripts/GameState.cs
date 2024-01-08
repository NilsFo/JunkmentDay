using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameState : MonoBehaviour
{
    private PlayerData _player;
    private PowerGun _powerGun;
    private BigMagnet _bigMagnet;
    private CharacterMovement _movement;
    private MusicManager _musicManager;
    private MouseLook _mouseLook;
    private GamepadInputDetector _gamepadInputDetector;

    public enum PlayerState
    {
        MAINMENU,
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
    public List<EncounterGroup> allEncounterGroups;

    [Header("World Hookup")] public MusicManager musicManager;
    public Canvas uiCanvas;
    public UI ui;
    public GameObject uiHurtIndicatorPrefab;

    [Header("Player info")] public PlayerState playerState;
    private bool _finaleMusic;

    [Header("Main Menu")] public GameObject menuUI;
    public GameObject ingameUI;
    public Button playBT;
    public Slider sliderMouseSensitivity;
    public Slider sliderVolume;
    public Camera mainMenuCamera;

    [Header("Encounters")] public int blockadesCount;
    public int blockadesDestroyed;

    [Header("Settings adjustments")] [Range(0f, 1f)]
    public float volumeBumpMagnitude = 0.1f;

    [Range(0f, 1f)] public float sensitivityBumpMagnitude = 0.1f;

    public PlayerData player => _player;
    public PowerGun PowerGun => _powerGun;
    public BigMagnet BigMagnet => _bigMagnet;
    public MouseLook MouseLook => _mouseLook;
    public CharacterMovement CharacterMovement => _movement;
    public GamepadInputDetector GamepadInputDetector => _gamepadInputDetector;
    public bool IsGamePad => GamepadInputDetector.isGamePad;

    private void Awake()
    {
        _player = FindObjectOfType<PlayerData>();
        _powerGun = FindObjectOfType<PowerGun>();
        _movement = FindObjectOfType<CharacterMovement>();
        _bigMagnet = FindObjectOfType<BigMagnet>();
        _mouseLook = FindObjectOfType<MouseLook>();
        _gamepadInputDetector = FindObjectOfType<GamepadInputDetector>();

        allSpawners = new List<RobotSpawner>();
        allRobots = new List<RobotBase>();
        allEncounterGroups = new List<EncounterGroup>();
        allFlechettes = new List<StickyFlechette>();
    }

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating(nameof(UpdateMusic), 0, 1.337f);
        // Application.targetFrameRate = FPS;
        restartEnabled = false;
        _finaleMusic = false;
        StopAllRobotSpawns();

        // Setting up main menu
        sliderVolume.value = MusicManager.userDesiredMasterVolume;
        sliderMouseSensitivity.value = _mouseLook.sensitivitySettings;
        // musicManager.audioMixer.SetFloat(musicManager.masterTrackName, MusicManager.userDesiredMusicVolumeDB);
        playBT.onClick.AddListener(Play);

        // Notifying if there are roaming robots. For debug reasons.
        Invoke(nameof(CheckEncounterGroups), 0.69f);
    }

    private void Play()
    {
        playerState = PlayerState.PLAYING;
    }

    // Update is called once per frame
    void Update()
    {
        _movement.inputDisabled = playerState == PlayerState.DEAD;

        // Menu state
        _mouseLook.lockCursor = playerState != PlayerState.MAINMENU;
        menuUI.SetActive(playerState == PlayerState.MAINMENU);
        mainMenuCamera.transform.parent.gameObject.SetActive(playerState == PlayerState.MAINMENU);
        ingameUI.SetActive(playerState != PlayerState.MAINMENU);
        player.gameObject.SetActive(playerState != PlayerState.MAINMENU);

        if (playerState == PlayerState.MAINMENU)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (_gamepadInputDetector.isGamePad)
            {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = false;
            }

            musicManager.SkipFade();
        }

        if (playerState == PlayerState.PLAYING)
        {
            playTime += Time.deltaTime;
            HandleInput();
        }

        // Applying settings
        MusicManager.userDesiredMasterVolume = sliderVolume.value;
        _mouseLook.sensitivitySettings = sliderMouseSensitivity.value;
    }

    public void HandleInput()
    {
        Keyboard keyboard = Keyboard.current;
        Gamepad gamepad = Gamepad.current;

        bool backToMenuClicked = false;
        bool increaseVolumeClicked = false;
        bool decreaseVolumeClicked = false;
        bool increaseSensitivityClicked = false;
        bool decreaseSensitivityClicked = false;

        // Restart level
        if (IsGamePad)
        {
            if (gamepad != null)
            {
                backToMenuClicked = gamepad.selectButton.wasPressedThisFrame;
                Vector2 dpad = gamepad.dpad.ReadValue();

                increaseVolumeClicked = dpad.y > 0f && GamepadInputDetector.WasDpadPressedThisFrame();
                decreaseVolumeClicked = dpad.y < 0f && GamepadInputDetector.WasDpadPressedThisFrame();
                increaseSensitivityClicked = dpad.x > 0f && GamepadInputDetector.WasDpadPressedThisFrame();
                decreaseSensitivityClicked = dpad.x < 0f && GamepadInputDetector.WasDpadPressedThisFrame();
            }
        }
        else
        {
            if (keyboard != null)
            {
                backToMenuClicked = keyboard.backspaceKey.wasPressedThisFrame;
                increaseVolumeClicked = keyboard.upArrowKey.wasPressedThisFrame;
                decreaseVolumeClicked = keyboard.downArrowKey.wasPressedThisFrame;
                increaseSensitivityClicked = keyboard.rightArrowKey.wasPressedThisFrame;
                decreaseSensitivityClicked = keyboard.leftArrowKey.wasPressedThisFrame;
            }
        }

        if (backToMenuClicked && Time.timeScale != 0f)
        {
            RestartLevel();
        }

        if (increaseVolumeClicked)
        {
            sliderVolume.value += volumeBumpMagnitude;
        }

        if (decreaseVolumeClicked)
        {
            sliderVolume.value -= volumeBumpMagnitude;
        }

        if (increaseSensitivityClicked)
        {
            sliderMouseSensitivity.value += volumeBumpMagnitude;
        }

        if (decreaseSensitivityClicked)
        {
            sliderMouseSensitivity.value -= volumeBumpMagnitude;
        }
    }

    private void CheckEncounterGroups()
    {
        foreach (RobotBase robot in allRobots)
        {
            bool found = false;
            foreach (EncounterGroup encounterGroup in allEncounterGroups)
            {
                if (found)
                {
                    continue;
                }

                if (encounterGroup.roamingRobots.Contains(robot))
                {
                    found = true;
                }
            }

            if (!found)
            {
                Debug.LogWarning("Roaming robot not assigned to an encounter Group: " + robot.name, robot.gameObject);
            }
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene("GameplayScene");
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
                    if (_finaleMusic)
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
            case PlayerState.MAINMENU:
                musicManager.Play(1);
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

        Win();
    }

    public void Win()
    {
        Debug.LogWarning("A winner is you!");
        StopAllRobotSpawns();
        foreach (RobotBase robot in allRobots)
        {
            robot.Kill();
        }

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

    public void RequestFinaleMusic()
    {
        _finaleMusic = true;
    }
}