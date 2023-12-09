using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attractable : MonoBehaviour
{
    private List<Attractor> _attractors;
    public bool HasAttractors => _attractors.Count > 0;
    public Rigidbody rb;

    private float _originalGravityScale;

    private void Awake()
    {
        _attractors = new List<Attractor>();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // rb.useGravity = !HasAttractors;
    }

    public void AddAttractor(Attractor attractor)
    {
        if (!_attractors.Contains(attractor))
        {
            _attractors.Add(attractor);
        }
    }

    public void RemoveAttractor(Attractor attractor)
    {
        _attractors.Remove(attractor);
    }
}