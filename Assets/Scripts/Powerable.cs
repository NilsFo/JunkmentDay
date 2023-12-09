using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Powerable : MonoBehaviour {
    public UnityEvent OnPowered;
    public UnityEvent OnUnpowered;

    public bool powered;
    private bool _powered;
    
    public float powerTime = 0f;
    private float _powerTimer = 0f;
    
    
    
    // Start is called before the first frame update
    void Start() {
        if(powered) 
            Power();
        else
            Unpower();
    }
    // Update is called once per frame
    void Update()
    {
        if (_powered != powered) {
            if(powered) 
                Power();
            else
                Unpower();
        }
        
        if (_powerTimer > 0) {
            _powerTimer -= Time.deltaTime;
            if (_powerTimer <= 0) {
                Unpower();
            }
        }
    }

    public void Power() {
        _powered = true;
        powered = true;
        
        OnPowered.Invoke();
        if (powerTime > 0) {
            _powerTimer = powerTime;
        }
    }

    public void Unpower() {
        _powered = false;
        powered = false;
        _powerTimer = 0;
        OnUnpowered.Invoke();
    }
    
}
