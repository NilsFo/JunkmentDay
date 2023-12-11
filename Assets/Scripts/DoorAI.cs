using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DoorAI : MonoBehaviour
{
    [Header("Open / Close config")] public GameObject doorObject;
    public float closedHeightX = 0f;
    public float closedHeightY = 0f;
    public float openedHeightY = 4f;
    public float openedHeightX = 0f;
    public Collider myCollider;
    public float transitionSpeed = 2f;
    public NavMeshObstacle navMeshObstacle;

    [Header("Anim")] public AnimationCurve animSpeedCurve;
    private float _stateTimer = 0f;

    private bool _opened;
    private float _heightCurrentX;
    private float _heightCurrentY;

    // Start is called before the first frame update
    void Start()
    {
        _heightCurrentX = closedHeightX;
        _heightCurrentY = closedHeightY;
        Close();
    }

    // Update is called once per frame
    void Update()
    {
        _stateTimer += Time.deltaTime;
        float animMult = animSpeedCurve.Evaluate(_stateTimer);

        float heightDesiredX = closedHeightX;
        float heightDesiredY = closedHeightY;
        myCollider.enabled = true;
        if (_opened)
        {
            heightDesiredX = openedHeightX;
            heightDesiredY = openedHeightY;
            myCollider.enabled = false;
        }

        _heightCurrentX =
            Mathf.MoveTowards(_heightCurrentX, heightDesiredX, Time.deltaTime * transitionSpeed * animMult);
        _heightCurrentY =
            Mathf.MoveTowards(_heightCurrentY, heightDesiredY, Time.deltaTime * transitionSpeed * animMult);
        Vector3 pos = doorObject.transform.localPosition;
        pos.y = _heightCurrentY;
        pos.x = _heightCurrentX;
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