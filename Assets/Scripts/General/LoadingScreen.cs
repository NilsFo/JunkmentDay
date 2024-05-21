using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{
    public bool autoChangeScene = false;
    public string sceneName = "GameplayScene";

    private bool _changeSceneRequested = false;
    private AsyncOperation _asyncLoad;

    private void Awake()
    {
        _changeSceneRequested = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        _asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        _asyncLoad.allowSceneActivation = false;
    }

    public void ChangeScene()
    {
        _changeSceneRequested = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (_asyncLoad != null && _changeSceneRequested)
        {
            _asyncLoad.allowSceneActivation = true;
        }

        if (GetLoadingPercentage() >= 1 && autoChangeScene)
        {
            ChangeScene();
        }
    }

    public float GetLoadingPercentage()
    {
        if (_asyncLoad != null)
        {
            float progress = _asyncLoad.progress / 0.9f;
            // int percentage = (int)(progress * 100);
            return progress;
        }

        return 0f;
    }
}