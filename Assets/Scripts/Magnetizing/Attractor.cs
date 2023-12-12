using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Attractor : MonoBehaviour
{
    [Header("Hookup")] public Collider myCollider;
    private GameState _gameState;

    [Header("Params")] private bool _active;
    public bool Active => _active;
    public bool activeOnStart;
    public float falloffDistanceMultiplier = 1.0f;

    public float pullStrength = 1.0f;
    private float _pullStrengthCurrent = 0;
    public float radius = 5f;
    public Transform pullCenter;

    private List<Attractable> _knownAttractables;

    private void Awake()
    {
        _knownAttractables = new List<Attractable>();
        _gameState = FindObjectOfType<GameState>();
        if (pullCenter == null)
            pullCenter = transform;
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
                    var position = pullCenter.position;
                    Vector3 attachmentPoint = knownAttractable.GetAttachmentPoint(position);
                    Vector3 direction = position - attachmentPoint;
                    float distance = Vector3.Distance(position, knownAttractable.transform.position);
                    float distanceMult = Mathf.Min(1f, 1 / Mathf.Pow(distance, 2) * falloffDistanceMultiplier);

                    direction.Normalize(); // Normalizing the vector so it has a magnitude of 1
                    knownAttractable.rb.AddForce(direction * _pullStrengthCurrent * distanceMult * Time.fixedDeltaTime,
                        ForceMode.Impulse);

                    float damage = _gameState.magnetDPS * distanceMult;
                    RobotBase robotBase = knownAttractable.robotBase;
                    if (robotBase != null)
                    {
                        robotBase.DealDamage(damage*Time.fixedDeltaTime);
                    }
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
        } else {
            var blockade = other.attachedRigidbody.GetComponent<Blockade>();
            if (blockade != null)
                blockade.DestroyBlockade();
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
        //myCollider.radius = radius;

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

    public void RemoveAttractable(Attractable attractable)
    {
        _knownAttractables.Remove(attractable);
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

#endif
}