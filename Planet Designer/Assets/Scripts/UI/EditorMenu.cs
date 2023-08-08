using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;

public class EditorMenu : MonoBehaviour
{
    public static EditorMenu Instance { get; private set; }

    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private TextMeshProUGUI helpText;
    [SerializeField] private TextMeshProUGUI cameraControlText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        FeatureOverview.Instance.Refresh();
    }

    public void SetHelpText(string text)
    {
        helpText.text = text;
    }

    public void ShowCameraControlText(bool visible)
    {
        cameraControlText.gameObject.SetActive(visible);
    }

    // Button actions

    public void SelectSurface()
    {
        #if UNITY_EDITOR
        Selection.activeObject = Planet.Instance.SurfaceSettings;
        #endif
    }

    public void SelectTerrainMaterial()
    {
        #if UNITY_EDITOR
        Selection.activeObject = Planet.Instance.SurfaceSettings.terrainMaterial;
        #endif
    }

    public void SelectOceanMaterial()
    {
        #if UNITY_EDITOR
        Selection.activeObject = Planet.Instance.SurfaceSettings.oceanMaterial;
        #endif
    }

    public void NewLocalForest()
    {
        string forestName = resourceManager.CreateLocalForest(Planet.Instance.PlanetName, out LocalForestSettings forestSettings, out ZoneSettings zoneSettings);
        LocalForest forest = FeatureManager.Instance.AddLocalForest(forestName, forestSettings, zoneSettings);
        forest.Select();
        FeatureOverview.Instance.Refresh();
    }

    public void NewGlobalForest()
    {
        string forestName = resourceManager.CreateGlobalForest(Planet.Instance.PlanetName, out GlobalForestSettings forestSettings);
        GlobalForest forest = FeatureManager.Instance.AddGlobalForest(forestName, forestSettings);
        forest.Select();
        FeatureOverview.Instance.Refresh();
    }

    public void RandomizePlanet()
    {
        Planet.Instance.RandomizeSeeds();
        Planet.Instance.Regenerate();
    }

}
