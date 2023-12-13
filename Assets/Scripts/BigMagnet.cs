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

    public List<Transform> railStops = new();
    private float[] _railStopPositions;
    public int currentStop = 0;

    private GameState _gameState;

    public int moveEnergy = 6;
    
    // Start is called before the first frame update
    void Start() {
        _gameState = FindObjectOfType<GameState>();
        _railLength = rail.CalculateLength();
        _railStopPositions = new float[railStops.Count];
        for (var index = 0; index < railStops.Count; index++) {
            var railStop = railStops [index];
            SplineUtility.GetNearestPoint(rail.Spline, railStop.localPosition, out var nearest, out float f);
            _railStopPositions[index] = f * _railLength;
        }
    }

    // Update is called once per frame
    void Update() {
        var targetRailPos = _railStopPositions [currentStop];
        if (Mathf.Approximately(railPos, targetRailPos)) {
            // We are at the current stop
            railMove = 0;
        } else if (railPos < targetRailPos) {
            // Moving forward
            railMove = 1;
            railPos += railMoveSpeed * Time.deltaTime * railMove;
            if (railPos > targetRailPos)
                railPos = targetRailPos;
        } else {
            // Moving backward
            railMove = -1;
            railPos += railMoveSpeed * Time.deltaTime * railMove;
            if (railPos < targetRailPos)
                railPos = targetRailPos;
        }
        rail.Evaluate(railPos/_railLength, out var pos, out var tangent, out _);
        transform.position = pos;
        var tangentVec = new Vector3(tangent.x, 0, tangent.z);
        var newDirection = Vector3.RotateTowards(transform.forward, tangentVec, _rotationSpeed * Time.deltaTime, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDirection);
    }

    public void MoveForward() {
        if (IsStopped()) {
            if (_gameState.player.CurrentEnergy >= moveEnergy) {
                _gameState.player.ModEnergy(-moveEnergy);
                currentStop++;
                currentStop = Mathf.Clamp(currentStop, 0, _railStopPositions.Length);
            }
        }
    }

    public void MoveBackward() {
        if (IsStopped()) {
            currentStop--;
            currentStop = Mathf.Clamp(currentStop, 0, _railStopPositions.Length);
        }
    }

    public bool IsStopped() {
        var targetRailPos = _railStopPositions [currentStop];
        return Mathf.Approximately(railPos, targetRailPos);
    }

    public void Stop() {
        //railMove = 0f;
    }
}
