using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GunLCDLogic : MonoBehaviour
{
    private GameState _gameState;
    public TMP_Text displayText;
    public Image lcdPrompt;
    public Image lcdError;

    public float lcdErrorTime = 2f;
    public float lcdErrorPulseRate = 0.5f;
    private float _lcdErrorTime;
    private bool _lcdErrorPulse;
    public AudioClip errorSFX;

    private void Awake()
    {
        _gameState = FindObjectOfType<GameState>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _lcdErrorPulse = false;
        CancelInvoke(nameof(Pulse));
    }

    private void Pulse()
    {
        _lcdErrorPulse = !_lcdErrorPulse;
        if (_lcdErrorPulse)
        {
            PlayErrorSound();
        }
    }

    public void PlayErrorSound()
    {
        GameObject clip = _gameState.musicManager.CreateAudioClip(
            errorSFX, transform.position,
            threeDimensional: false);
        clip.transform.parent = _gameState.player.transform;
    }

    // Update is called once per frame
    void Update()
    {
        _lcdErrorTime -= Time.deltaTime;

        string text = "";
        int count = _gameState.allFlechettes.Count;
        if (count < 10)
        {
            text += "0";
        }

        text += count;

        if (count > 99)
        {
            text = "99";
        }

        displayText.text = text;

        lcdPrompt.enabled = _lcdErrorTime < 0;
        displayText.enabled = _lcdErrorTime < 0;
        lcdError.enabled = _lcdErrorTime > 0 && _lcdErrorPulse;

        if (_lcdErrorTime < 0)
        {
            CancelInvoke(nameof(Pulse));
        }
    }

    public void ShowError()
    {
        _lcdErrorTime = lcdErrorTime;
        _lcdErrorPulse = false;
        
        Pulse();
        CancelInvoke(nameof(Pulse));
        InvokeRepeating(nameof(Pulse), lcdErrorPulseRate, lcdErrorPulseRate);
    }
}