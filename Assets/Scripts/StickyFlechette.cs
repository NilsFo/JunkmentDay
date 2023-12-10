using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyFlechette : MonoBehaviour
{

    public Markable myMark;

    private void Start()
    {
        myMark.myFlechettes.Add(this);
    }

    private void OnDestroy()
    {
        myMark.myFlechettes.Remove(this);
    }
}
