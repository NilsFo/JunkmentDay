using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatapultShot : MonoBehaviour
{
    public int damage = 10;

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer.Equals(LayerMask.NameToLayer("Player")))
        {
            var playerData = other.collider.GetComponentInParent<PlayerData>();
            if (playerData == null)
                return;
            Debug.Log("Catapult Shot is hurting the Player for " + damage + " damage", this);
            playerData.Damage(damage);
        }

        Destroy(gameObject);
    }
}