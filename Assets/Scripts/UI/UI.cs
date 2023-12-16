using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public Image hurtOverlayImage;
    public AnimationCurve hurtOverlayIntensityCurve;
    public Image targetingRect;

    private float _hurtOverlayIntensityProgress;
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

        deathTextOne.enabled = false;
        deathTextTwo.enabled = false;
        winTextOne.enabled = false;
        winTextTwo.enabled = false;
        _fadeOut = 0;
    }

    // Update is called once per frame
    void Update()
    {
        _hurtOverlayIntensityProgress += Time.deltaTime;
        float a = hurtOverlayIntensityCurve.Evaluate(_hurtOverlayIntensityProgress);

        if (_gameState.playerState == GameState.PlayerState.DEAD)
        {
            a = 1.1337f;
        }

        Color c = hurtOverlayImage.color;
        c.a = a * 0.5f;
        hurtOverlayImage.color = c;

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
        winTextTwo.text = "Robots dispatched: " + _gameState.player.killCount + "\n" +
                          "Time taken: " + time + " seconds.";
    }

    public void StartDamageOverlay()
    {
        _hurtOverlayIntensityProgress = 0;
    }

    public void WinUI()
    {
        winTextOne.enabled = true;
        fadeoutEnabled = true;

        Invoke(nameof(WinUITwo), 3f);
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