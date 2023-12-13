using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerTrigger : MonoBehaviour {
    public bool once;
    public UnityEvent OnPlayerEnter;

    private void OnTriggerEnter(Collider other) {
        var player = other.GetComponent<PlayerData>();
        if (player != null) {
            OnPlayerEnter.Invoke();
            if (once) {
                Destroy(gameObject);
            }
        }
    }
}
