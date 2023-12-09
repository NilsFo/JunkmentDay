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
    
    
    
    // Start is called before the first frame update
    void Start() {
        if(marked) 
            Mark();
        else
            Unmark();
    }
    // Update is called once per frame
    void Update()
    {
        if (_marked != marked) {
            if(marked) 
                Mark();
            else
                Unmark();
        }
    }

    public void Mark() {
        _marked = true;
        marked = true;
        
        OnMarked.Invoke();
    }

    public void Unmark() {
        _marked = false;
        marked = false;
        OnUnmarked.Invoke();
    }
}
