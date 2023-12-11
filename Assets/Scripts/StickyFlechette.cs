using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class StickyFlechette : MonoBehaviour
{
    public GameObject flechetteClutterPrefab;
    public Markable myMark;

    private void Start()
    {
        myMark.myFlechettes.Add(this);
    }

    public void DestroyFlechette()
    {
        myMark.myFlechettes.Remove(this);
        CreateClutterReplacement();
        Destroy(gameObject);
    }

    private void CreateClutterReplacement()
    {
        Quaternion rot = transform.rotation;
        var clutter = Instantiate(flechetteClutterPrefab, transform.position, rot);
        Rigidbody rb = clutter.GetComponent<Rigidbody>();

        var direction = clutter.transform.position - myMark.transform.position;
        direction = direction.normalized;

        float force = Random.Range(5f, 8f);
        rb.AddForce(direction * force, ForceMode.Impulse);
    }
}