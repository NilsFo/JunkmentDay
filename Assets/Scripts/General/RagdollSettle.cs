using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;


public class RagdollSettle : MonoBehaviour
{
    private Vector3 _positionCache = Vector3.zero;
    private Vector3 _rotationCache = Vector3.zero;

    [Header("Parameters")] public float settleTimer = 8f;
    private float _settleTimer;

    private void Start()
    {
        _settleTimer = 0;
    }

    private void FixedUpdate()
    {
        _positionCache = transform.position;
        _rotationCache = transform.rotation.eulerAngles;
    }

    private void Update()
    {
        if (RigidBodyChangeRate() <= 0.01f)
        {
            _settleTimer += Time.deltaTime;
            if (_settleTimer >= settleTimer)
            {
                Settle();
            }
        }
        else
        {
            _settleTimer = 0;
        }
    }

    public void Settle()
    {
        Destroy(GetComponent<Rigidbody>());
        Destroy(this);

        List<Collider> targetColliders = new List<Collider>();
        targetColliders.AddRange(GetComponentsInChildren<Collider>());
        targetColliders.AddRange(GetComponents<Collider>());

        foreach (Collider col in targetColliders)
        {
            Destroy(col);
        }
    }

    private float RigidBodyChangeRate()
    {
        float m = Mathf.Abs(_positionCache.magnitude - transform.position.magnitude)
                  + Mathf.Abs(_rotationCache.magnitude - transform.rotation.eulerAngles.magnitude);
        return Mathf.Abs(m);
    }
}