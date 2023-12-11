using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtBox : MonoBehaviour
{
    public int damage = 10;

    public bool once = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            var playerData = other.GetComponentInParent<PlayerData>();
            if (playerData == null)
                return;
            Debug.Log("Hurting the Player for " + damage + " damage", this);
            playerData.Damage(damage);
            if (once)
            {
                Destroy(gameObject);
            }
        }
    }
}