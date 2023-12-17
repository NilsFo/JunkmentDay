using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public Image hurtOverlayImage;
    public Image healthOverlayImage;
    public Image batteryOverlayImage;
    public AnimationCurve hurtOverlayIntensityCurve;
    public Image targetingRect;

    private float _hurtOverlayIntensityProgress;
    private float _healthOverlayIntensityProgress;
    private float _batteryOverlayIntensityProgress;
    private GameState _gameState;

    private bool fadeoutEnabled;
    public float fadeOutSpeed = 0.69f;
    private float _fadeOut = 0f;

    public TMP_Text deathTextOne;
    public TMP_Text deathTextTwo;

    public TMP_Text winTextOne;
    public TMP_Text winTextTwo;
    public Image winOverlay;

    // Start is called before the first frame update
    void Start()
    {
        _gameState = FindObjectOfType<GameState>();
        _hurtOverlayIntensityProgress = 10f;
        _healthOverlayIntensityProgress = 10f;
        _batteryOverlayIntensityProgress = 10f;

        deathTextOne.enabled = false;
        deathTextTwo.enabled = false;
        winTextOne.enabled = false;
        winTextTwo.enabled = false;
        _fadeOut = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // HURT OVERLAY
        _hurtOverlayIntensityProgress += Time.deltaTime;
        _healthOverlayIntensityProgress += Time.deltaTime;
        _batteryOverlayIntensityProgress += Time.deltaTime;
        float a = hurtOverlayIntensityCurve.Evaluate(_hurtOverlayIntensityProgress);

        if (_gameState.playerState == GameState.PlayerState.DEAD)
        {
            a = 1.1337f;
        }

        Color c = hurtOverlayImage.color;
        c.a = a * 0.5f;
        hurtOverlayImage.color = c;

        // Health Overlay
        a = hurtOverlayIntensityCurve.Evaluate(_healthOverlayIntensityProgress);
        c = healthOverlayImage.color;
        c.a = a * 0.15f;
        healthOverlayImage.color = c;

        // Battery Overlay
        a = hurtOverlayIntensityCurve.Evaluate(_batteryOverlayIntensityProgress);
        c = batteryOverlayImage.color;
        c.a = a * 0.15f;
        batteryOverlayImage.color = c;

        // DEATH
        targetingRect.enabled = false;
        if (_gameState.playerState == GameState.PlayerState.PLAYING)
        {
            targetingRect.enabled = true;
        }

        if (fadeoutEnabled)
        {
            _fadeOut += fadeOutSpeed * Time.deltaTime;
            a = MathF.Min(_fadeOut, 1);

            c = winOverlay.color;
            c.a = a;
            winOverlay.color = c;
        }

        int time = (int)_gameState.playTime;
        int minutes = time / 60;
        int seconds = time % 60;
        string formattedSeconds = $"{seconds:D2}";

        winTextTwo.text = "Robots dispatched: " + _gameState.player.killCount + "\n" +
                          "Time taken: " + minutes + ":" + formattedSeconds + ".";
    }

    public void StartDamageOverlay()
    {
        _hurtOverlayIntensityProgress = 0;
    }

    public void StartHealingOverlay()
    {
        if (!(_healthOverlayIntensityProgress < 1))
        {
            _healthOverlayIntensityProgress = 0;
        }
    }

    public void StartBatteryOverlay()
    {
        if (!(_batteryOverlayIntensityProgress < 1))
        {
            _batteryOverlayIntensityProgress = 0;
        }
    }

    public void WinUI()
    {
        Invoke(nameof(WinUIFadeout), 5f);
        Invoke(nameof(WinUIOne), 7f);
        Invoke(nameof(WinUITwo), 10f);
    }

    private void WinUIFadeout()
    {
        fadeoutEnabled = true;
    }

    private void WinUIOne()
    {
        winTextOne.enabled = true;
    }

    private void WinUITwo()
    {
        winTextTwo.enabled = true;
    }

    public void DeathUI()
    {
        deathTextOne.enabled = true;
        Invoke(nameof(DeathUITwo), 2f);
    }

    private void DeathUITwo()
    {
        deathTextTwo.enabled = true;
        _gameState.restartEnabled = true;
    }
}