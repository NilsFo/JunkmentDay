using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class RobotSawAI : MonoBehaviour
{
    public GameObject sawGameObject;

    private bool _spinning;
    public float rotationSpeed;
    public float rotationSpeedChangeRate = 10;
    private float _rotationSpeedCurrent;


    public void TurnOn()
    {
        _spinning = true;
    }

    public void TurnOff()
    {
        _spinning = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        TurnOn();
    }

    private void FixedUpdate()
    {
        Vector3 rotationEuler = sawGameObject.transform.rotation.eulerAngles;
        rotationEuler.y = rotationEuler.y += _rotationSpeedCurrent * Time.fixedDeltaTime;
        //sawGameObject.transform.rotation = Quaternion.Euler(rotationEuler);
        sawGameObject.transform.Rotate(Vector3.up * _rotationSpeedCurrent * Time.fixedDeltaTime, Space.Self);
    }

    // Update is called once per frame
    void Update()
    {
        float targetRotationSpeed = 0;
        if (_spinning)
        {
            targetRotationSpeed = rotationSpeed;
        }

        _rotationSpeedCurrent = Mathf.MoveTowards(_rotationSpeedCurrent, targetRotationSpeed,
            rotationSpeedChangeRate * Time.deltaTime);
    }
}