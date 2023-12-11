using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FlechetteProjectile : MonoBehaviour
{
    public GameObject flechetteClutterPrefab;
    public LayerMask stickToLayers;
    public LayerMask becomeClutterLayers;
    private GameState _gameState;

    private Vector3 _cachePosition;
    private Vector3 _originPoint;

    private void Awake()
    {
        _cachePosition = transform.position;
        _gameState = FindObjectOfType<GameState>();
        _originPoint = transform.position;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void FixedUpdate()
    {
        _cachePosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        var layer = other.gameObject.layer;
        Vector3 impactPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);

        if (stickToLayers == (stickToLayers | (1 << layer)))
        {
            Debug.Log("stick to layer", other.gameObject);
            var markable = other.GetComponent<Markable>();
            if (markable != null)
            {
                // hit a markable
                Debug.Log("Shot marker gun, hit a markable entity", markable.gameObject);

                // Creating sticky flechete
                GameObject flechetteObj = Instantiate(_gameState.stickyFlechettePrefab, impactPoint,
                    Quaternion.identity);
                flechetteObj.transform.rotation = transform.rotation;
                flechetteObj.transform.parent = markable.transform;

                StickyFlechette flechette = flechetteObj.GetComponent<StickyFlechette>();
                flechette.myMark = markable;
                _gameState.PowerGun.ReportFlechetteHit(flechette);
                markable.Mark();

                Destroy(gameObject);
            }
        }

        if (becomeClutterLayers == (becomeClutterLayers | (1 << layer)))
        {
            Debug.Log("become clutter layer", other.gameObject);
            CreateClutterReplacement(impactPoint);
            Destroy(gameObject);
        }
    }

    private void CreateClutterReplacement(Vector3 point)
    {
        Quaternion rot = transform.rotation;
        var clutter = Instantiate(flechetteClutterPrefab, point, rot);

        Rigidbody rb = clutter.GetComponent<Rigidbody>();
        var direction = _originPoint-clutter.transform.position;
        direction = direction.normalized;

        float force = Random.Range(3f, 5f);
        rb.AddForce(direction * force, ForceMode.Impulse);
    }
}