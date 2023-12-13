using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public Image hurtOverlayImage;
    public AnimationCurve hurtOverlayIntensityCurve;

    private float _hurtOverlayIntensityProgress;

    // Start is called before the first frame update
    void Start()
    {
        _hurtOverlayIntensityProgress = 10f;
    }

    // Update is called once per frame
    void Update()
    {
        _hurtOverlayIntensityProgress += Time.deltaTime;
        float a = hurtOverlayIntensityCurve.Evaluate(_hurtOverlayIntensityProgress);

        Color c = hurtOverlayImage.color;
        c.a = a * 0.75f;
        hurtOverlayImage.color = c;
    }

    public void StartDamageOverlay()
    {
        _hurtOverlayIntensityProgress = 0;
    }
}