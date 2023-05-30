using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Forest : Feature
{
    [SerializeField] private Zone zone;
    [SerializeField] private ForestSettings settings;
    private LayerMask raycastLayerMask;
    private int terrainLayer, waterLayer, defaultLayer;
    private Noise noise;

    public ForestSettings Settings => settings;
    public override Object InspectObject() => settings;

    public void Initialize(ForestSettings forestSettings, ZoneSettings zoneSettings)
    {
        forestSettings.SetForest(this);
        settings = forestSettings;

        terrainLayer = LayerMask.NameToLayer("Terrain");
        waterLayer = LayerMask.NameToLayer("Water");
        defaultLayer = LayerMask.NameToLayer("Default");
        UpdateLayerMask();

        zone.Initialize(zoneSettings, this);

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
        // Reset

        noise = new Noise(settings.seed.value);

        bool DisableColliders()
        {
            foreach (SelectablePrefab selectablePrefab in settings.prefabs)
                if (selectablePrefab.prefab.GetComponent<Collider>())
                    return true;
            return false;
        }

        if (DisableColliders())
        {
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
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }

        // Generate trees using all zone points
        SmartRegen_AddTrees(zone.Settings.points);
    }

    /// <summary>
    /// Generates trees using only the provided zone points
    /// </summary>
    public void SmartRegen_AddTrees(List<Vector3> zonePoints)
    {
        // Get selected prefabs

        List<SelectablePrefab> prefabs = new List<SelectablePrefab>();

        foreach (SelectablePrefab selectablePrefab in settings.prefabs)
        {
            if (selectablePrefab.selected && selectablePrefab.prefab && selectablePrefab.scale != 0f)
                prefabs.Add(selectablePrefab);
        }

        if (prefabs.Count == 0)
            return;

        // Place prefabs in zone

        RaycastHit raycastHit;
        GameObject go;
        float raycastDistance = Planet.Instance.TerrainSphere.ElevationRange.max + 1;

        foreach (Vector3 point in zonePoints)
        {
            // Sample noise to determine whether or not to place object on this point
            float sample1 = noise.Evaluate(point * settings.seedScale).Remapped(-1f, 1f, 0f, 100f);

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
            float sample2 = noise.Evaluate(point * settings.seedScale * 2f).Remapped(-1f, 1f, 0f, prefabs.Count);

            // Sample noise to determine the object's rotation
            float sample3 = noise.Evaluate(point * settings.seedScale * 3f).Remapped(-1f, 1f, 0f, 360f);

            // Instantiate object
            go = Instantiate(prefabs[(int)sample2].prefab, transform);
            go.transform.localScale *= prefabs[(int)sample2].scale;
            go.transform.position = raycastHit.point;
            go.transform.up = point;
            go.transform.Rotate(0f, sample3, 0f);
        }
    }

    /// <summary>
    /// Removes all trees positioned on the provided zone points
    /// </summary>
    public void SmartRegen_RemoveTrees(List<Vector3> zonePoints)
    {
        foreach (Transform child in transform)
        {
            for (int i = zonePoints.Count - 1; i >= 0; --i)
            {
                if (child.position.normalized == zonePoints[i])
                {
                    Destroy(child.gameObject);
                    zonePoints.RemoveAt(i);
                }
            }
        }
    }
}
