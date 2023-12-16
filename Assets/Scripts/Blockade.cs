using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blockade : MonoBehaviour
{
    public float destructionDelay = 3f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void DestroyBlockade() {
        Invoke(nameof(DestroyBlockadeNow), destructionDelay);
    }

    private void DestroyBlockadeNow() {
        var magnet = FindObjectOfType<BigMagnet>();
        foreach (var debris in GetComponentsInChildren<MoveToAndDestroy>()) {
            debris.target = magnet.GetComponentInChildren<Attractor>().transform;
            debris.enabled = true;
            debris.transform.parent = null;
        }
        Destroy(gameObject);
    }
}
