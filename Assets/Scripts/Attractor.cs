using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attractor : MonoBehaviour
{
    [Header("Hookup")] public SphereCollider myCollider;
    private GameState _gameState;

    [Header("Params")] private bool _active;
    public bool activeOnStart;

    public float pullStrength = 1.0f;
    private float _pullStrengthCurrent = 0;
    public float pullStrengthGain = 1.0f;
    public float radius = 5f;

    private List<Attractable> knownAttractables;

    private void Awake()
    {
        knownAttractables = new List<Attractable>();
        _gameState = FindObjectOfType<GameState>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (activeOnStart)
        {
            _pullStrengthCurrent = pullStrength;
            TurnOn();
        }
        else
        {
            _pullStrengthCurrent = 0;
            TurnOff();
        }
    }

    public void TurnOn()
    {
        _active = true;
    }

    public void TurnOff()
    {
        _active = false;
    }

    private void FixedUpdate()
    {
        foreach (Attractable knownAttractable in knownAttractables)
        {
            Vector3 direction = transform.position - knownAttractable.transform.position;
            var distnace = Vector3.Distance(transform.position, knownAttractable.transform.position);
            var strengthMult = Mathf.Min(1, 1 / Mathf.Pow(distnace, 2));

            direction.Normalize(); // Normalizing the vector so it has a magnitude of 1
            knownAttractable.rb.AddForce(direction * _pullStrengthCurrent * strengthMult * Time.fixedTime,
                ForceMode.Impulse);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        RobotAI robotAI = other.GetComponent<RobotAI>();
        if (robotAI == null)
        {
            robotAI = other.GetComponentInChildren<RobotAI>();
        }

        if (robotAI == null)
        {
            robotAI = other.GetComponentInParent<RobotAI>();
        }

        if (robotAI == null)
        {
            return;
        }

        Attractable attractable = robotAI.myAttractable;

        if (attractable != null && !knownAttractables.Contains(attractable))
        {
            print("Enter: " + other.gameObject.name);
            knownAttractables.Add(attractable);
            attractable.AddAttractor(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        RobotAI robotAI = other.GetComponent<RobotAI>();
        if (robotAI == null)
        {
            robotAI = other.GetComponentInChildren<RobotAI>();
        }

        if (robotAI == null)
        {
            robotAI = other.GetComponentInParent<RobotAI>();
        }

        if (robotAI == null)
        {
            return;
        }

        Attractable attractable = robotAI.myAttractable;

        if (attractable != null)
        {
            print("Leave: " + other.gameObject.name);
            knownAttractables.Remove(attractable);
            attractable.RemoveAttractor(this);
        }
    }


    // Update is called once per frame
    void Update()
    {
        myCollider.radius = radius;

        float pullStrengthTarget = 0;
        if (_active)
        {
            pullStrengthTarget = pullStrength;
        }

        _pullStrengthCurrent =
            Mathf.MoveTowards(_pullStrengthCurrent, pullStrengthTarget, pullStrengthGain * Time.deltaTime);
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

#endif
}