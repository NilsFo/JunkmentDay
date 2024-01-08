using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIGamepadSliderRelay : MonoBehaviour
{
    private GameState _gameState;

    [Header("Hookup")] public Slider mySlider;
    private EventSystem _eventSystem;

    [Header("Gamepad Button Pairs to listen for")]
    public bool triggers;
    public bool shoulderButtons;

    [Header("Config")] public float gamepadMagnitudeTap;
    public float gamepadMagnitudeHold;
    public float holdRegisterDelay = 0.25f;
    private float _holdRegisterDelay = 0f;
    public bool inverted;

    private void Awake()
    {
        _gameState = FindObjectOfType<GameState>();
    }


    // Update is called once per frame
    void Update()
    {
        _holdRegisterDelay -= Time.deltaTime;

        if (_gameState.IsGamePad)
        {
            Gamepad gamepad = Gamepad.current;

            if (gamepad != null)
            {
                var dpad = gamepad.dpad.ReadValue();
                var dpadX = dpad.x;
                var dpadY = dpad.y;

                if (triggers)
                {
                    if (gamepad.leftTrigger.wasPressedThisFrame)
                    {
                        Tap(true);
                    }
                    else if (gamepad.leftTrigger.isPressed)
                    {
                        Hold(true);
                    }

                    if (gamepad.rightTrigger.wasPressedThisFrame)
                    {
                        Tap(false);
                    }
                    else if (gamepad.rightTrigger.isPressed)
                    {
                        Hold(false);
                    }
                }

                if (shoulderButtons)
                {
                    if (gamepad.leftShoulder.wasPressedThisFrame)
                    {
                        Tap(true);
                    }
                    else if (gamepad.leftShoulder.isPressed)
                    {
                        Hold(true);
                    }

                    if (gamepad.rightShoulder.wasPressedThisFrame)
                    {
                        Tap(false);
                    }
                    else if (gamepad.rightShoulder.isPressed)
                    {
                        Hold(false);
                    }
                }
            }
        }
    }

    private void Tap(bool reduce)
    {
        _holdRegisterDelay = holdRegisterDelay;
        float change = gamepadMagnitudeTap;
        ApplyChange(change, reduce);
    }

    private void Hold(bool reduce)
    {
        if (_holdRegisterDelay >= 0)
        {
            return;
        }

        float change = gamepadMagnitudeHold * Time.deltaTime;
        ApplyChange(change, reduce);
    }

    private void ApplyChange(float change, bool reduce)
    {
        change = Mathf.Abs(change);
        if (reduce)
        {
            change *= -1;
        }

        if (inverted)
        {
            change *= -1;
        }

        float newValue = mySlider.value + change;
        newValue = Mathf.Clamp(newValue, mySlider.minValue, mySlider.maxValue);
        mySlider.value = newValue;
    }
}