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

    public TMP_Text deathTextOne;
    public TMP_Text deathTextTwo;

    // Start is called before the first frame update
    void Start()
    {
        _gameState = FindObjectOfType<GameState>();
        _hurtOverlayIntensityProgress = 10f;

        deathTextOne.enabled = false;
        deathTextTwo.enabled = false;
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
    }

    public void StartDamageOverlay()
    {
        _hurtOverlayIntensityProgress = 0;
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