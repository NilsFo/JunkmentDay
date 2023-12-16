using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MoveToAndDestroy : MonoBehaviour {
    public Transform target;
    public Vector3 angularVelocity;
    public float speed = 10f;
    public float acceleration = 1f;
    
    public UnityEvent OnTargetReached;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update() {
        speed += acceleration * Time.deltaTime;
        transform.rotation *= Quaternion.Euler(angularVelocity * Time.deltaTime);
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        if (Mathf.Approximately(Vector3.Distance(transform.position, target.position), 0)) {
            OnTargetReached.Invoke();
            Destroy(gameObject);
        }
    }
}
