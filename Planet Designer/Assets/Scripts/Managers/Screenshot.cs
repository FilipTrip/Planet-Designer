using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;
using Debug = UnityEngine.Debug;

[ExecuteAlways]
public class Screenshot : MonoBehaviour
{
    public enum ImageFormat { JPG, PNG }

    [SerializeField, ReadOnly, TextArea] private string shortcut = "Left CTRL/CMD + S to screenshot";
    [SerializeField, ReadOnly, TextArea] private string folderPath;
    [SerializeField] private ImageFormat imageFormat;

    private static Screenshot instance;

    [Header("Sequence settings")]
    [SerializeField] private int images;
    [SerializeField] private float interval;

    private void OnEnable()
    {
        instance = this;
        folderPath = Application.dataPath;
        folderPath = folderPath.Remove(folderPath.LastIndexOf("Assets"));
        folderPath += "Screenshots/";
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                Capture();
            }
        }
    }

    #if UNITY_EDITOR
    [MenuItem("Planet Designer/Screenshot %#1")]
    #endif
    public static void Capture()
    {
        string fileName = instance.folderPath + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-ff");
        string extension = "." + instance.imageFormat.ToString().ToLower();

        if (System.IO.File.Exists(fileName + extension))
        {
            int i = 0;
            string newName;

            do
            {
                newName = fileName + " (" + ++i + ")";
            }
            while (System.IO.File.Exists(newName + extension));

            fileName = newName;
        }

        Debug.Log("Screenshot taken: " + fileName + extension);
        ScreenCapture.CaptureScreenshot(fileName + extension);
    }

#if UNITY_EDITOR
    [MenuItem("Planet Designer/Screenshot Sequence %#2")]
#endif
    public static void CaptureSequence()
    {
        instance.StartCoroutine(instance.Coroutine_CaptureSequence());
    }

    private IEnumerator Coroutine_CaptureSequence()
    {
        for (int i = 0; i < images; ++i)
        {
            Capture();
            yield return new WaitForSeconds(interval);
        }

        yield return 0;
    }

}
