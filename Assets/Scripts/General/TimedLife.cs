using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class TimedLife : MonoBehaviour
{
    public float aliveTime = 4.0f;
    private float _timer = 0;
    public bool timerActive = true;

    [FormerlySerializedAs("OnEndOfLife")] public UnityEvent onEndOfLife;

    // Start is called before the first frame update
    void Start()
    {
        if (onEndOfLife == null)
        {
            onEndOfLife = new UnityEvent();
        }

        _timer = 0;
        ResetTimer();
    }

    // Update is called once per frame
    void Update()
    {
        if (timerActive)
        {
            _timer += Time.deltaTime;
            if (_timer >= aliveTime)
            {
                DestroySelf();
            }
        }
    }

    public void ResetTimer()
    {
        _timer = 0;
    }

    [ContextMenu("Destroy Self")]
    public void DestroySelf()
    {
        onEndOfLife.Invoke();
        Destroy(gameObject);
    }
}