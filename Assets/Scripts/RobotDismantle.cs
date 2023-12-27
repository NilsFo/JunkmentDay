using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotDismantle : MonoBehaviour
{
    public List<GameObject> robotParts;
    [Range(0, 1)] public float dismantleChance = 0.69f;
    public LayerMask newLayer;


    public void Dismantle()
    {
        foreach (GameObject robotPart in robotParts)
        {
            if (Random.Range(0.0f, 1.0f) <= dismantleChance)
            {
                robotPart.transform.parent = null;

                Collider c = robotPart.GetComponent<Collider>();
                c.enabled = true;
                robotPart.AddComponent<Rigidbody>();

                TimedLifeAnimated lifeAnimated = robotPart.AddComponent<TimedLifeAnimated>();
                lifeAnimated.aliveTime = 12;

                robotPart.layer = LayerMask.NameToLayer("Clutter");
            }
        }
    }
}