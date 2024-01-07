using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class TimedLifeAnimated : MonoBehaviour
{
    public float aliveTime = 4.0f;
    public float timeJitter = 0.69f;
    public float animationSpeed = 0.8f;
    public bool timerActive = true;
    private float _timer = 0;
    private bool _animating = false;

    [FormerlySerializedAs("OnEndOfLife")] public UnityEvent onEndOfLife;

    // Start is called before the first frame update
    void Start()
    {
        _timer = 0;
        _animating = false;

        if (timeJitter > 0)
        {
            aliveTime += Random.Range(-timeJitter, timeJitter);
        }

        if (onEndOfLife == null)
        {
            onEndOfLife = new UnityEvent();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (timerActive)
        {
            if (_animating)
            {
                Vector3 scale = transform.localScale;
                scale.x = scale.x - animationSpeed * Time.deltaTime;
                scale.y = scale.y - animationSpeed * Time.deltaTime;
                scale.z = scale.z - animationSpeed * Time.deltaTime;

                if (scale.x <= 0 || scale.y <= 0 || scale.z <= 0)
                {
                    DestroySelf();
                }
                else
                {
                    transform.localScale = scale;
                }
            }
            else
            {
                _timer += Time.deltaTime;
                if (_timer >= aliveTime)
                {
                    _animating = true;
                }
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