using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomInitialRotation : MonoBehaviour
{
    public bool rotateX, rotateY, rotateZ;

    private void Awake()
    {
        Vector3 rot = transform.rotation.eulerAngles;
        float z = rot.z;
        float y = rot.y;
        float x = rot.x;

        if (rotateX)
        {
            rot.x = Random.rotation.eulerAngles.x;
        }

        if (rotateY)
        {
            rot.y = Random.rotation.eulerAngles.y;
        }

        if (rotateZ)
        {
            rot.z = Random.rotation.eulerAngles.z;
        }

        transform.rotation = Quaternion.Euler(rot);
    }
}