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

    private void OnCollisionEnter(Collision collision)
    {
        var other = collision.gameObject;
        var layer = other.layer;
        var contact = collision.GetContact(0);
        Vector3 impactPoint = contact.point;

        if (stickToLayers == (stickToLayers | (1 << layer)))
        {
            Markable markable = other.GetComponent<Markable>();

            if (markable == null)
            {
                CreateClutterReplacement(impactPoint);
                Destroy(gameObject);
            }
            else
            {
                // Creating sticky flechete
                GameObject flechetteObj = Instantiate(_gameState.stickyFlechettePrefab, impactPoint,
                    Quaternion.identity);
                flechetteObj.transform.rotation = Quaternion.LookRotation(contact.normal, Vector3.up);
                flechetteObj.transform.parent = markable.transform;

                StickyFlechette flechette = flechetteObj.GetComponent<StickyFlechette>();
                flechette.myMark = markable;
                _gameState.PowerGun.ReportFlechetteHit(flechette);
                markable.Mark();

                Destroy(gameObject);
            }

            var powerable = other.GetComponent<Powerable>();
            if (powerable != null)
            {
                powerable.Power();
            }
        }
        else
        {
            if (becomeClutterLayers == (becomeClutterLayers | (1 << layer)))
            {
                CreateClutterReplacement(impactPoint);
                Destroy(gameObject);
            }
            else
            {
                Debug.LogError("Collided with something i dont know", other.gameObject);
            }
        }
    }

    private void CreateClutterReplacement(Vector3 point)
    {
        Quaternion rot = transform.rotation;
        var clutter = Instantiate(flechetteClutterPrefab, point, rot);

        Rigidbody rb = clutter.GetComponent<Rigidbody>();
        var direction = _originPoint - clutter.transform.position;
        direction = direction.normalized;
        
        rb.AddRelativeTorque(new Vector3(
            Random.Range(-20f, 20f),
            Random.Range(-20f, 20f),
            Random.Range(-20f, 20f)
        ));

        float force = Random.Range(3f, 5f);
        rb.AddForce(direction * force, ForceMode.Impulse);
    }
}