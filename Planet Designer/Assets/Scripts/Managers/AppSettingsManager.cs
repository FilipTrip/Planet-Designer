using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppSettingsManager : MonoBehaviour
{
    [SerializeField] private AppSettings appSettings;

    private void Awake()
    {
        RenderSettings.skybox = appSettings.skybox;
        DynamicGI.UpdateEnvironment();
    }
}
