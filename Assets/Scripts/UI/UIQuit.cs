using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class UIQuit : MonoBehaviour
{
    private GameState _gameState;
    public GameObject quitConfirmUI;
    public bool paused = false;

    private void Awake()
    {
        _gameState = FindObjectOfType<GameState>();
    }

    private void Start()
    {
        {
            UpdateUI();
        }
    }

    void Update()
    {
#if !UNITY_WEBGL
        bool pauseInput = false;
        bool quitInput = false;
        Keyboard keyboard = Keyboard.current;
        Gamepad gamepad = Gamepad.current;

        if (_gameState.IsGamePad)
        {
            if (gamepad != null)
            {
                pauseInput = gamepad.startButton.wasPressedThisFrame;
                quitInput = gamepad.buttonSouth.wasPressedThisFrame;
            }
        }
        else
        {
            if (keyboard != null)
            {
                pauseInput = keyboard.escapeKey.wasPressedThisFrame;
                quitInput = keyboard.enterKey.wasPressedThisFrame;
            }
        }

        if (pauseInput)
        {
            if (!paused)
            {
                paused = true;
                Time.timeScale = 0;
                UpdateUI();
            }
            else
            {
                paused = false;
                Time.timeScale = 1;
                UpdateUI();
            }
        }

        if (quitInput && paused)
        {
            Application.Quit();
        }
#endif
    }

    private void UpdateUI()
    {
        quitConfirmUI.SetActive(paused);
    }
}