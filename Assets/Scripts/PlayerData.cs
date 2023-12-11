using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerData : MonoBehaviour
{

    public GameObject head;

    public int maxHealth = 100;
    private int _currentHealth;

    // Damage binning only applies the maximum damage value over the damage binning time to avoid accumulated damage from multiple sources
    public float damageBinningTime = 0.5f;
    private int _damageBin;
    private float _damageBinTimer;
    public int CurrentHealth {
        get => _currentHealth;
        set => SetHealth(value);
    }

    // Events

    public UnityEvent<int> OnHealthChanged;
    public UnityEvent OnPlayerDeath;
    
    // Start is called before the first frame update
    void Start()
    {
        SetHealth(maxHealth);
    }

    // Update is called once per frame
    void Update()
    {
        if (_damageBinTimer > 0) {
            _damageBinTimer -= Time.deltaTime;
            if (_damageBinTimer <= 0) {
                _damageBinTimer = 0;
                _damageBin = 0;
            }
        }
    }

    // Hurt the player while respecting damage binning
    // Only the maximum Damage value over the last half second is applied if damageBinning = true
    public void Damage(int damage, bool damageBinning = true) {
        if (damageBinning) {
            if (_damageBinTimer > 0) {
                if (damage > _damageBin) {
                    ModHealth(-(damage - _damageBin));
                    _damageBin = damage;
                }
            } else {
                _damageBinTimer = damageBinningTime;
                _damageBin = damage;
                ModHealth(-damage);
            }
        } else {
            ModHealth(-damage);
        }
    }

    // Modify the player's health
    // No damage binning is applied
    public void ModHealth(int change) {
        SetHealth(_currentHealth + change);
    }

    private void SetHealth(int newHealth) {
        newHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        if (newHealth != _currentHealth) {
            _currentHealth = newHealth;
            OnHealthChanged.Invoke(_currentHealth);
            Debug.Log("Player health now " + _currentHealth);
            if (_currentHealth <= 0) {
                Debug.Log("Player is dead");
                OnPlayerDeath.Invoke();
            }
        }
    }

    public float GetDistanceToPlayer(Vector3 fromPosition) {
        return Vector3.Distance(head.transform.position, fromPosition);
    }

    public bool PlayerInView(Vector3 fromPosition) {
        var hit = Physics.Linecast(head.transform.position, fromPosition, out RaycastHit hitInfo, LayerMask.GetMask("World"));
        return !hit;
    }
}
