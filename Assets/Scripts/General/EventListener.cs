using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class EventListener : MonoBehaviour
{
    public UnityEvent onCollisionEnter, onCollisionExit, onCollisionStay;
    public UnityEvent onTriggerEnter, onTriggerExit, onTriggerStay;
    public UnityEvent onEnabled, onDisabled;

    public UnityEvent onUnityEditor, onWebBuild, onDesktopBuild;

    void Start()
    {
        onCollisionEnter ??= new UnityEvent();
        onCollisionExit ??= new UnityEvent();
        onCollisionStay ??= new UnityEvent();
        onTriggerEnter ??= new UnityEvent();
        onTriggerExit ??= new UnityEvent();
        onTriggerStay ??= new UnityEvent();
        onEnabled ??= new UnityEvent();
        onDisabled ??= new UnityEvent();
        onUnityEditor ??= new UnityEvent();
        onWebBuild ??= new UnityEvent();
        onDesktopBuild ??= new UnityEvent();

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            onWebBuild.Invoke();
        }

        if (Application.platform == RuntimePlatform.WindowsEditor ||
            Application.platform == RuntimePlatform.LinuxEditor ||
            Application.platform == RuntimePlatform.OSXEditor)
        {
            onUnityEditor.Invoke();
        }

        if (Application.platform == RuntimePlatform.WindowsPlayer ||
            Application.platform == RuntimePlatform.LinuxPlayer ||
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            onUnityEditor.Invoke();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        onCollisionEnter.Invoke();
    }

    private void OnCollisionExit(Collision other)
    {
        onCollisionExit.Invoke();
    }

    private void OnCollisionStay(Collision collisionInfo)
    {
        onCollisionStay.Invoke();
    }

    private void OnTriggerEnter(Collider other)
    {
        onTriggerEnter.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        onTriggerExit.Invoke();
    }

    private void OnTriggerStay(Collider other)
    {
        onTriggerStay.Invoke();
    }

    private void OnEnable()
    {
        onEnabled.Invoke();
    }

    private void OnDisable()
    {
        onDisabled.Invoke();
    }
}