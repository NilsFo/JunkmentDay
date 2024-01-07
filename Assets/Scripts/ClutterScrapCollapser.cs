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
    private GameState _gameState;


    private void Awake()
    {
        foreach (GameObject scrap in scrapStates)
        {
            scrap.SetActive(false);
        }

        _gameState = FindObjectOfType<GameState>();
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

        Invoke(nameof(StartOptimizedCleanup), Random.Range(1f, 5f));
    }

    private void OnEnable()
    {
        scrapStates[_collapsedState].SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void StartOptimizedCleanup()
    {
        StartCoroutine(OptimizedCleanup());
    }

    IEnumerator OptimizedCleanup()
    {
        for (var i = scrapStates.Count - 1; i >= 0; i--)
        {
            GameObject scrap = scrapStates[i];
            if (i!=_collapsedState)
            {
                scrapStates.RemoveAt(i);
                Destroy(scrap);
            }

            yield return new WaitForSeconds(.1f);
        }
    }
}