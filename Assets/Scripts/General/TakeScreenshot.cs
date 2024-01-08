using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;

public class TakeScreenshot : MonoBehaviour
{
    [Header("Config")] public bool editorOnly = true;
    public bool autoScreenshotSize = false;
    private bool _autoScreenshotSize;

    [Header("Parameters")] public int width = 1920;
    public int height = 1080;
    private int _width = 0;
    private int _height = 0;
    public KeyCode captureKey = KeyCode.Return;
    public TextureFormat textureFormat = TextureFormat.RGB24;

    private string _outPath;

    // Start is called before the first frame update
    void Start()
    {
        _outPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        _width = width;
        _height = height;
        _autoScreenshotSize = autoScreenshotSize;

        if (editorOnly && !Application.isEditor)
        {
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (autoScreenshotSize != _autoScreenshotSize)
        {
            autoScreenshotSize = _autoScreenshotSize;
            width = _width;
            height = _height;
        }

        if (autoScreenshotSize)
        {
            Camera c = Camera.current;
            width = c.pixelWidth;
            height = c.pixelHeight;
        }

        if (Input.GetKeyDown(captureKey))
        {
            RequestScreenshot();
        }
    }

    void RequestScreenshot()
    {
        string fileName = "Screenshot_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";
        string filePath = _outPath + "/" + fileName;

        // Start a coroutine to capture the screenshot
        StartCoroutine(CaptureScreenshot(filePath));
    }

    IEnumerator CaptureScreenshot(string filePath)
    {
        // Wait for end of frame to ensure all rendering is complete
        yield return new WaitForEndOfFrame();

        // Create a temporary RenderTexture
        RenderTexture rt = new RenderTexture(width, height, 24);
        RenderTexture prev = Camera.main.targetTexture;

        // Set the camera to render to the RenderTexture
        Camera.main.targetTexture = rt;
        Camera.main.Render();

        // Create a Texture2D and read the RenderTexture image into it
        Texture2D screenShot = new Texture2D(width, height, textureFormat, false);
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);

        // Apply and encode to PNG
        screenShot.Apply();
        byte[] bytes = screenShot.EncodeToPNG();

        // Restore the original RenderTexture
        Camera.main.targetTexture = prev;
        RenderTexture.active = null;
        Destroy(rt);

        // Write to a file
        System.IO.File.WriteAllBytes(filePath, bytes);
        Debug.Log("Screenshot saved to: " + filePath);
    }
}