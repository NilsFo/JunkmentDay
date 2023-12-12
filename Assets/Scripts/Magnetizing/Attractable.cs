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
    public Markable myMarkable;
    public RobotBase robotBase;

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
    }

    public Vector3 GetAttachmentPoint(Vector3 source)
    {
        if (myMarkable == null)
        {
            return transform.position;
        }

        StickyFlechette closest = myMarkable.ClosestFlechette(source);
        if (closest == null)
        {
            return transform.position;
        }

        return closest.transform.position;
    }

    public bool IsAttractable()
    {
        if (myMarkable == null)
        {
            return true;
        }

        return myMarkable.myFlechettes.Count > 0;
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

    public bool IsMagnetized()
    {
        foreach (var attractor in _attractors)
        {
            if (attractor.Active)
            {
                if (myMarkable == null)
                {
                    return true;
                }

                return myMarkable.marked;
            }
        }

        return false;
    }

    public void OnRobotDeath()
    {
        foreach (Attractor attractor in _attractors)
        {
            attractor.RemoveAttractable(this);
        }
    }
}