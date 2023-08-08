using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

[ExecuteAlways]
public class Planet : MonoBehaviour
{
    [SerializeField, ReadOnly] private string planetName;
    [SerializeField, ReadOnly] private bool initialized;

    [SerializeField] private SurfaceSettings surfaceSettings;
    [SerializeField] private Sphere oceanSphere;
    [SerializeField] private Sphere terrainSphere;
    [SerializeField] private Transform featuresParent;
    [SerializeField] private Transform objectsParent;

    private List<Feature> features = new List<Feature>();

    public static Planet Instance { get; private set; }
    public static UnityEvent RegenerationCompleted = new UnityEvent();
    public static UnityEvent Loaded = new UnityEvent();

    public string PlanetName { get { return planetName; } set { planetName = value; } }
    public SurfaceSettings SurfaceSettings { get { return surfaceSettings; } set { surfaceSettings = value; } }
    public Sphere TerrainSphere => terrainSphere;
    public Sphere OceanSphere => oceanSphere;
    public Transform FeaturesParent => featuresParent;
    public Transform ObjectsParent => objectsParent;
    public List<Feature> Features => features;

    private void OnValidate()
    {
        if (!initialized)
            return;

        Regenerate();
    }

    public void Initialize()
    {
        Instance = this;
        initialized = true;

        terrainSphere.Initialize();
        oceanSphere.Initialize();

        terrainSphere.RegenerationCompleted.AddListener(oceanSphere.AutoRegenerate);
    }

    public void AutoRegenerate()
    {
        if (surfaceSettings.autoRegenerate)
            Regenerate();
    }

    [ContextMenu("Regenerate")]
    public void Regenerate()
    {
        if (!initialized)
            return;

        Stopwatch stopwatch = Stopwatch.StartNew();
        terrainSphere.Regenerate();

        stopwatch.Stop();
        Debug.Log("Regenerated " + gameObject.name + " (" + stopwatch.ElapsedMilliseconds + "ms)");
        RegenerationCompleted.Invoke();
    }

    public void RandomizeSeeds()
    {
        // Randomize seeds for all noise layers

        foreach (NoiseLayer noiseLayer in surfaceSettings.terrainNoiseLayers)
            noiseLayer.seed.New();

        // Look for all seeds in features and randomize them

        Forest forest;
        foreach (Feature feature in features)
        {
            if (forest = feature.GetComponent<Forest>())
                forest.Settings.seed.New();
        }
    }

}