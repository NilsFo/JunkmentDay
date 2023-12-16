using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BatteryBar : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI textfield;
    public Image fillImage;

    private GameState _gameState;

    public float fillSpeed = 1.1337f;
    public float fillDesired = 0f;
    public float fillCurrent = 0f;

    public Gradient chargingGradient;
    public Gradient fullyChargedGradient;
    public float colorChangeSpeed;
    private float _colorGradientProgress;

    private void Awake()
    {
        _gameState = FindObjectOfType<GameState>();
    }

    // Start is called before the first frame update
    void Start()
    {
        fillDesired = 0f;
        fillCurrent = 0f;
        _colorGradientProgress = 0;
    }

    // Update is called once per frame
    void Update()
    {
        _colorGradientProgress += Time.deltaTime * colorChangeSpeed;
        if (_colorGradientProgress > 1)
        {
            _colorGradientProgress = _colorGradientProgress - 1;
        }

        fillDesired = GetPercentage(_gameState.player.CurrentEnergy, _gameState.player.maxEnergy);
        fillDesired = Math.Clamp(fillDesired, 0.0f, 1.0f);

        Color color = fullyChargedGradient.Evaluate(_colorGradientProgress);
        if (fillCurrent < 1)
        {
            textfield.text = "";
            color = chargingGradient.Evaluate(fillCurrent);
        }
        else
        {
            textfield.text = "FULLY CHARGED";
        }

        fillCurrent = Mathf.MoveTowards(fillCurrent, fillDesired, fillSpeed * Time.deltaTime);
        fillCurrent = Math.Clamp(fillCurrent, 0.0f, 1.0f);
        
        fillImage.color = color;
        slider.value = fillCurrent;
    }

    private float GetPercentage(int a, int b)
    {
        float af = a;
        float bf = b;
        return GetPercentage(af, bf);
    }

    private float GetPercentage(float a, float b)
    {
        return a / b;
    }
}