using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class BigMagnet : MonoBehaviour {
    public SplineContainer rail;
    public float railMove = 0f;  // -1 to move backwards at railMoveSpeed
    
    public float railMoveSpeed = 1f;
    public float railPos;
    private float _railLength;

    private float _rotationSpeed = 0.5f; 
    
    // Start is called before the first frame update
    void Start() {
        _railLength = rail.CalculateLength();
    }

    // Update is called once per frame
    void Update()
    {
        railPos += railMoveSpeed * Time.deltaTime * railMove;
        transform.position = rail.EvaluatePosition(railPos/_railLength);

        var tangent = rail.EvaluateTangent(railPos / _railLength);
        var tangentVec = new Vector3(tangent.x, 0, tangent.z);
        var newDirection = Vector3.RotateTowards(transform.forward, tangentVec, _rotationSpeed * Time.deltaTime, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDirection);
    }

    public void MoveForward() {
        railMove = 1f;
    }

    public void MoveBackward() {
        railMove = -1f;
    }

    public void Stop() {
        railMove = 0f;
    }
}
