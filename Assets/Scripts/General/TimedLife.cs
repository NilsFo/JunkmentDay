using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimedLife : MonoBehaviour
{
    public float aliveTime = 4.0f;
    private float _timer = 0;
    public bool timerActive = true;

    public UnityEvent OnEndOfLife;

    // Start is called before the first frame update
    void Start()
    {
        if (OnEndOfLife == null)
        {
            OnEndOfLife = new UnityEvent();
        }

        _timer = 0;
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

    [ContextMenu("Destroy Self")]
    public void DestroySelf()
    {
        OnEndOfLife.Invoke();
        Destroy(gameObject);
    }
}