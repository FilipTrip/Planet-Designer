using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class AppSettingsMenu : MonoBehaviour
{
    [SerializeField] private AppSettings appSettings;
    [SerializeField] private List<Material> skyboxes;
    [SerializeField] private Toggle[] skyboxToggles;

    private void OnEnable()
    {
        skyboxToggles[skyboxes.IndexOf(appSettings.skybox)].SetIsOnWithoutNotify(true);
    }

    public void SetSkybox(int index)
    {
        if (!skyboxToggles[index].isOn)
            return;

        appSettings.skybox = RenderSettings.skybox = skyboxes[index];
        DynamicGI.UpdateEnvironment();
    }
}
