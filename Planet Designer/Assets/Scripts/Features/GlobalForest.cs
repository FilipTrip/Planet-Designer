using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class GlobalForest : Feature
{
    [SerializeField] private GlobalForestSettings settings;

    private LayerMask raycastLayerMask;
    private int terrainLayer, waterLayer, defaultLayer;
    private Noise noise;

    public GlobalForestSettings Settings => settings;

    public void Initialize(GlobalForestSettings forestSettings)
    {
        forestSettings.SetForest(this);
        settings = forestSettings;
        inspectObject = settings;

        terrainLayer = LayerMask.NameToLayer("Terrain");
        waterLayer = LayerMask.NameToLayer("Water");
        defaultLayer = LayerMask.NameToLayer("Default");
        UpdateLayerMask();

        Sphere.RegenerationCompleted.AddListener((sphere) => { Regenerate(); });
    }

    public void UpdateLayerMask()
    {
        if (settings.avoidOcean)
            raycastLayerMask = LayerMask.GetMask("Default", "Terrain", "Water");
        else
            raycastLayerMask = LayerMask.GetMask("Default", "Terrain");
    }

    public override void Regenerate()
    {
        RemoveTrees();
        PlaceTrees();
    }

    private void RemoveTrees()
    {
        // Checks if colliders are present and should be disabled
        bool DisableColliders()
        {
            foreach (ToggleablePrefab selectablePrefab in settings.prefabs)
                if (selectablePrefab.prefab && selectablePrefab.prefab.GetComponent<Collider>())
                    return true;
            return false;
        }

        if (DisableColliders())
        {
            // Disables colliders before destroying children

            Collider collider;
            foreach (Transform child in transform)
            {
                if (collider = child.GetComponent<Collider>())
                    collider.enabled = false;
                Destroy(child.gameObject);
            }
        }
        else
        {
            // Destroy children directly

            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void PlaceTrees()
    {
        noise = new Noise(settings.seed.value);
        Random.InitState(settings.seed.value);

        // Get selected prefabs
        
        List<ToggleablePrefab> prefabs = new List<ToggleablePrefab>();

        foreach (ToggleablePrefab selectablePrefab in settings.prefabs)
        {
            if (selectablePrefab.enabled && selectablePrefab.prefab && selectablePrefab.scale != 0f)
                prefabs.Add(selectablePrefab);
        }

        if (prefabs.Count == 0)
            return;

        // Place prefabs

        float sample1, sample2, sample3;
        Vector3 point;
        RaycastHit raycastHit;
        GameObject go;
        float raycastDistance = Planet.Instance.TerrainSphere.ElevationRange.max + 1;

        for (int i = 0; i < settings.amount; ++i)
        {
            // Get random point on unit sphere
            point = Random.onUnitSphere;
            
            // Sample noise to determine whether or not to place object on this point
            sample1 = noise.Evaluate(point * settings.seedScale).Remapped(-1f, 1f, 0f, 100f);

            if (sample1 > settings.density)
                continue;

            // Find position to place object
            if (!Physics.Raycast(point * raycastDistance, -point, out raycastHit, raycastDistance, raycastLayerMask))
                continue;

            // Avoid placing trees on certain layers
            if (settings.avoidOcean && raycastHit.collider.gameObject.layer == waterLayer)
                continue;

            if (settings.avoidObjects && raycastHit.collider.gameObject.layer == defaultLayer)
                continue;

            // Sample noise to determine which object to instantiate
            sample2 = noise.Evaluate(point * settings.seedScale * 2f).Remapped(-1f, 1f, 0f, prefabs.Count);

            // Sample noise to determine the object's rotation
            sample3 = noise.Evaluate(point * settings.seedScale * 3f).Remapped(-1f, 1f, 0f, 360f);

            // Instantiate object
            go = Instantiate(prefabs[(int)sample2].prefab, transform);
            go.transform.parent = transform;
            go.transform.localScale *= prefabs[(int)sample2].scale;
            go.transform.position = raycastHit.point;
            go.transform.up = point;
            go.transform.Rotate(0f, sample3, 0f);
        }
    }

    public override void WhileSelected()
    {
        //zone.ManualUpdate();
        settings.numberOfObjects = transform.childCount;
    }

    public override bool CheckClickedOn()
    {
        return false; //zone.CheckClickedOn();
    }
}
