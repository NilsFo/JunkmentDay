using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector3 = System.Numerics.Vector3;

public class ClutterScrapCollapser : MonoBehaviour
{
    public List<GameObject> scrapStates;
    private int _collapsedState = 0;

    private void Awake()
    {
        foreach (GameObject scrap in scrapStates)
        {
            scrap.SetActive(false);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject scrap in scrapStates)
        {
            scrap.SetActive(false);
        }
        
        _collapsedState = Random.Range(0, scrapStates.Count);
        scrapStates[_collapsedState].SetActive(true);

        transform.rotation = Random.rotation;
    }

    private void OnEnable()
    {
        scrapStates[_collapsedState].SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
    }
}