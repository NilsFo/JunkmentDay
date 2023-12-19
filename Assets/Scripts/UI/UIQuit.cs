using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class UIQuit : MonoBehaviour {
    public GameObject quitConfirmUI;
    public bool paused = false;
    private void Start() {
        {
            UpdateUI();
        }
    }
    void Update() {
        #if !UNITY_WEBGL
        if (Keyboard.current.escapeKey.wasPressedThisFrame) {
            if (!paused) {
                paused = true;
                Time.timeScale = 0;
                UpdateUI();
            } else {
                paused = false;
                Time.timeScale = 1;
                UpdateUI();
            }
        }
        if (Keyboard.current.enterKey.wasPressedThisFrame && paused) {
            Application.Quit();
        }
        #endif
    }

    private void UpdateUI() {
        quitConfirmUI.SetActive(paused);
    }
    
}
