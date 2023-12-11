using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attractor : MonoBehaviour
{
    [Header("Hookup")] public SphereCollider myCollider;
    private GameState _gameState;

    [Header("Params")] private bool _active;
    public bool Active => _active;
    public bool activeOnStart;
    public float falloffDistanceMultiplier = 1.0f;

    public float pullStrength = 1.0f;
    private float _pullStrengthCurrent = 0;
    public float radius = 5f;

    private List<Attractable> _knownAttractables;

    private void Awake()
    {
        _knownAttractables = new List<Attractable>();
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
        if (_active)
        {
            foreach (Attractable knownAttractable in _knownAttractables)
            {
                if (knownAttractable.IsAttractable())
                {
                    var position = transform.position;
                    Vector3 attachmentPoint = knownAttractable.GetAttachmentPoint(position);
                    Vector3 direction = position - attachmentPoint;
                    float distance = Vector3.Distance(position, knownAttractable.transform.position);
                    float strengthMult = Mathf.Min(1f, 1 / Mathf.Pow(distance, 2) * falloffDistanceMultiplier);

                    direction.Normalize(); // Normalizing the vector so it has a magnitude of 1
                    knownAttractable.rb.AddForce(direction * _pullStrengthCurrent * strengthMult * Time.fixedDeltaTime,
                        ForceMode.Impulse);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Attractable attractable = ExtractAttractableFromEntity(other.gameObject);
        if (attractable != null && !_knownAttractables.Contains(attractable))
        {
            // print("Enter: " + other.gameObject.name);
            _knownAttractables.Add(attractable);
            attractable.AddAttractor(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Attractable attractable = ExtractAttractableFromEntity(other.gameObject);
        if (attractable != null)
        {
            // print("Leave: " + other.gameObject.name);
            _knownAttractables.Remove(attractable);
            attractable.RemoveAttractor(this);
        }
    }

    private Attractable ExtractAttractableFromEntity(GameObject entity)
    {
        RobotBase robotSawAI = entity.GetComponent<RobotBase>();

        if (robotSawAI == null)
        {
            robotSawAI = entity.GetComponentInChildren<RobotBase>();
        }

        if (robotSawAI == null)
        {
            robotSawAI = entity.GetComponentInParent<RobotBase>();
        }

        if (robotSawAI != null)
        {
            return robotSawAI.myAttractable;
        }

        return null;
    }


    // Update is called once per frame
    void Update()
    {
        // updating radius
        myCollider.radius = radius;

        // updating pullstrength
        _pullStrengthCurrent = 0;
        if (_active)
        {
            _pullStrengthCurrent = pullStrength;
        }

        // updating attractors based on flechettes
        foreach (Attractable attractable in _knownAttractables)
        {
            var position = transform.position;

            Color color = Color.red;
            if (attractable.IsAttractable())
            {
                color = Color.blue;
            }

            Debug.DrawLine(position, attractable.GetAttachmentPoint(position), color);
        }
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

#endif
}