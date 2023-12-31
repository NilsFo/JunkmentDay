using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotKillZone : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        RobotBase robotBase = other.GetComponent<RobotBase>();
        if (robotBase != null)
        {
            robotBase.MarkForMagnetDeath();
            // Debug.Log("Robot marked for death!");
        }
    }
}