using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Markable : MonoBehaviour
{
    public UnityEvent OnMarked;
    public UnityEvent OnUnmarked;

    public bool marked;
    private bool _marked;
    private GameState _gameState;
    public List<StickyFlechette> myFlechettes;

    private void Awake()
    {
        myFlechettes = new List<StickyFlechette>();
        _gameState = FindObjectOfType<GameState>();
    }

    public void RemoveAllFlechettes()
    {
        foreach (StickyFlechette flechette in myFlechettes)
        {
            flechette.DestroyFlechette();
        }
    }

    public StickyFlechette ClosestFlechette(Vector3 position)
    {
        if (myFlechettes.Count == 0)
        {
            return null;
        }

        StickyFlechette closest = myFlechettes[0];
        float closestDistance = Vector3.Distance(position, closest.transform.position);
        foreach (StickyFlechette flechette in myFlechettes)
        {
            float dist = Vector3.Distance(position, flechette.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closest = flechette;
            }
        }

        return closest;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (marked)
            Mark();
        else
            Unmark();
    }

    // Update is called once per frame
    void Update()
    {
        if (_marked != marked)
        {
            if (marked)
                Mark();
            else
                Unmark();
        }
    }

    public void Mark()
    {
        _marked = true;
        marked = true;

        OnMarked.Invoke();
    }

    public void Unmark()
    {
        _marked = false;
        marked = false;
        OnUnmarked.Invoke();
    }
}