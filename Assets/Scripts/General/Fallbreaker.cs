using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fallbreaker : MonoBehaviour
{
    public int falloffYPos = -300;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y <= falloffYPos)
        {
            Destroy(gameObject);
        }
    }
}