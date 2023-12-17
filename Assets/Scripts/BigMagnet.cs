using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

public class BigMagnet : MonoBehaviour
{
    public SplineContainer rail;
    public float railMove = 0f; // -1 to move backwards at railMoveSpeed

    public float railMoveSpeed = 1f;
    public float railPos;
    private float _railLength;

    private float _rotationSpeed = 0.5f;

    public List<Transform> railStops = new();
    private float[] _railStopPositions;
    public int currentStop = 0;

    private GameState _gameState;
    private GunLCDLogic _gunLcdLogic;

    public int moveEnergy = 6;
    public UnityEvent startMoving;
    public UnityEvent stopMoving;
    public UnityEvent noEnergy;

    public List<Powerable> buttons;
    public Material buttonReadyMaterial;
    public Material buttonDisabledMaterial;
    public Material buttonMovingMaterial;

    private void Awake()
    {
        _gunLcdLogic = FindObjectOfType<GunLCDLogic>();
        _gameState = FindObjectOfType<GameState>();
        _railLength = rail.CalculateLength();
    }

    // Start is called before the first frame update
    void Start()
    {
        _railStopPositions = new float[railStops.Count];
        for (var index = 0; index < railStops.Count; index++)
        {
            var railStop = railStops[index];
            SplineUtility.GetNearestPoint(rail.Spline, railStop.localPosition, out var nearest, out float f);
            _railStopPositions[index] = f * _railLength;
        }

        stopMoving.AddListener(UpdateButtonState);
        startMoving.AddListener(UpdateButtonState);
        _gameState.player.OnEnergyChanged.AddListener(_ => UpdateButtonState());
    }

    // Update is called once per frame
    void Update()
    {
        var targetRailPos = _railStopPositions[currentStop];
        if (Mathf.Approximately(railPos, targetRailPos))
        {
            if (railMove != 0)
            {
                stopMoving.Invoke();
            }

            // We are at the current stop
            railMove = 0;
        }
        else if (railPos < targetRailPos)
        {
            // Moving forward
            railMove = 1;
            railPos += railMoveSpeed * Time.deltaTime * railMove;
            if (railPos > targetRailPos)
                railPos = targetRailPos;
        }
        else
        {
            // Moving backward
            railMove = -1;
            railPos += railMoveSpeed * Time.deltaTime * railMove;
            if (railPos < targetRailPos)
                railPos = targetRailPos;
        }

        rail.Evaluate(railPos / _railLength, out var pos, out var tangent, out _);
        transform.position = pos;
        var tangentVec = new Vector3(tangent.x, 0, tangent.z);
        var newDirection = Vector3.RotateTowards(transform.forward, tangentVec, _rotationSpeed * Time.deltaTime, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDirection);
    }

    public void MoveForward() {
        if (IsStopped()) {
            if (_gameState.player.CurrentEnergy >= moveEnergy) {
                currentStop++;
                currentStop = Mathf.Clamp(currentStop, 0, _railStopPositions.Length);
                _gameState.player.ModEnergy(-moveEnergy);
                startMoving.Invoke();
            } else {
                OnNoEnergy();
            }
        }
    }

    public void OnNoEnergy()
    {
        noEnergy.Invoke();
        _gunLcdLogic.ShowError();
    }

    public void MoveBackward()
    {
        if (IsStopped())
        {
            currentStop--;
            currentStop = Mathf.Clamp(currentStop, 0, _railStopPositions.Length);
        }
    }

    public bool IsStopped()
    {
        var targetRailPos = _railStopPositions[currentStop];
        return Mathf.Approximately(railPos, targetRailPos);
    }

    public void Stop()
    {
        //railMove = 0f;
    }

    private void UpdateButtonState() {
        Material mat;
        if (!IsStopped()) {
            mat = buttonMovingMaterial;
        } else {
            if (_gameState.player.CurrentEnergy < moveEnergy) {
                mat = buttonDisabledMaterial;
            } else {
                mat = buttonReadyMaterial;
            }
        }
        foreach (var btn in buttons) {
            foreach (var rend in btn.GetComponentsInChildren<MeshRenderer>()) {
                rend.material = mat;
            }
        }
    }
}
