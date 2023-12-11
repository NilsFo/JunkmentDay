using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DoorAI : MonoBehaviour
{
    [Header("Open / Close config")] public GameObject doorObject;
    public float closedHeight = 0f;
    public float openedHeight = 4f;
    public Collider myCollider;
    public float transitionSpeed = 2f;
    public NavMeshObstacle navMeshObstacle;

    [Header("Anim")] public AnimationCurve animSpeedCurve;
    private float _stateTimer = 0f;

    private bool _opened;
    private float _heightCurrent;

    // Start is called before the first frame update
    void Start()
    {
        _heightCurrent = closedHeight;
        Close();
    }

    // Update is called once per frame
    void Update()
    {
        _stateTimer += Time.deltaTime;
        float animMult = animSpeedCurve.Evaluate(_stateTimer);

        float heightDesired = closedHeight;
        myCollider.enabled = true;
        if (_opened)
        {
            heightDesired = openedHeight;
            myCollider.enabled = false;
        }

        _heightCurrent = Mathf.MoveTowards(_heightCurrent, heightDesired, Time.deltaTime * transitionSpeed * animMult);
        Vector3 pos = doorObject.transform.localPosition;
        pos.y = _heightCurrent;
        doorObject.transform.localPosition = pos;
    }

    public void Open()
    {
        _stateTimer = 0;
        _opened = true;
        navMeshObstacle.enabled = false;
    }

    public void Close()
    {
        _stateTimer = 0;
        _opened = false;
        navMeshObstacle.enabled = true;
    }
}