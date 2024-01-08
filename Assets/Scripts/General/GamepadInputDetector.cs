using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class GamepadInputDetector : MonoBehaviour
{
    [Header("Is GamePad Mode?")] public bool isGamePad;
    private bool _isGamePad;

    private Vector2 _dpadThisFrame = new Vector2();
    private Vector2 _dpadLastFrame = new Vector2();

    [Header("Listener")] public UnityEvent onSwitchToKeyBoard, onSwitchToGamePad;

    void Start()
    {
        if (onSwitchToKeyBoard == null)
        {
            onSwitchToKeyBoard = new UnityEvent();
        }

        if (onSwitchToGamePad == null)
        {
            onSwitchToGamePad = new UnityEvent();
        }

        _isGamePad = isGamePad;
        NotifyListeners();
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;
        Gamepad gamepad = Gamepad.current;

        // ##################################
        //  Checking if device is plugged in
        // ##################################

        if (gamepad == null)
        {
            isGamePad = false;
        }

        if (mouse == null || keyboard == null)
        {
            isGamePad = true;
        }

        // ##################################
        //  Listening for inputs
        // ##################################
        if (mouse != null && keyboard != null)
        {
            bool isMouseClicked = mouse.leftButton.isPressed ||
                                  mouse.rightButton.isPressed ||
                                  mouse.middleButton.isPressed ||
                                  mouse.delta.ReadValue() != Vector2.zero ||
                                  mouse.scroll.ReadValue() != Vector2.zero;
            bool isKeyboardPressed = keyboard.anyKey.isPressed;

            if (isMouseClicked || isKeyboardPressed)
            {
                isGamePad = false;
            }
        }

        if (gamepad != null)
        {
            bool isGamepadInput =
                gamepad.allControls.Any(control => control is ButtonControl && ((ButtonControl)control).isPressed) ||
                (gamepad.leftStick.ReadValue() != Vector2.zero || gamepad.rightStick.ReadValue() != Vector2.zero ||
                 gamepad.leftTrigger.isPressed || gamepad.rightTrigger.isPressed);
            // Check for any button press

            if (isGamepadInput)
            {
                isGamePad = true;
            }
        }

        // ##################################
        //  Updating D-Pad
        // ##################################
        _dpadLastFrame = _dpadThisFrame;
        if (isGamePad)
        {
            _dpadThisFrame = Gamepad.current.dpad.ReadValue();
        }

        // ##################################
        //  Notifying observers
        // ##################################
        if (_isGamePad != isGamePad)
        {
            _isGamePad = isGamePad;
            NotifyListeners();
        }
    }

    public bool IsDpadPressed()
    {
        if (!isGamePad)
        {
            return false;
        }

        return CollapseVector2(_dpadThisFrame) > 0;
    }

    public bool WasDpadPressedThisFrame()
    {
        if (!isGamePad)
        {
            return false;
        }

        return IsDpadPressed() &&
               (_dpadThisFrame.x != _dpadLastFrame.x || _dpadThisFrame.y != _dpadLastFrame.y)
               && _dpadLastFrame.x == 0 && _dpadLastFrame.y == 0;
    }

    private float CollapseVector2(Vector2 v)
    {
        return Mathf.Abs(v.x) + Mathf.Abs(v.y);
    }

    private void NotifyListeners()
    {
        if (isGamePad)
        {
            onSwitchToGamePad.Invoke();
        }
        else
        {
            onSwitchToKeyBoard.Invoke();
        }
    }
}