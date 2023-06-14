using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeatureManager : MonoBehaviour
{
    public static FeatureManager Instance { get; private set; }

    [SerializeField, ReadOnly] private Feature selectedFeature;
    [SerializeField] private GameObject localForestPrefab;
    [SerializeField] private GameObject globalForestPrefab;

    private bool selectionUpdatedThisFrame;
    private Reticle reticle;

    public Feature SelectedFeature => selectedFeature;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        // Deselect if clicking outside the planet
        if (selectedFeature && !Reticle.Instance.OnPlanetSurface && !CameraController.Instance.BeingControlled && Input.GetMouseButtonUp(0))
            Select(null);

        // Deselect if pressing ESC
        if (selectedFeature && Input.GetKeyDown(KeyCode.Escape))
            Select(null);

        // Remove if pressing Backspace
        if (selectedFeature && Input.GetKeyDown(KeyCode.Backspace))
        {
            Feature toBeRemoved = selectedFeature;
            Select(null);
            Remove(toBeRemoved);
            FeatureOverview.Instance.Refresh();
        }

        // Try to select if pressing on planet
        if (!selectedFeature && Reticle.Instance.OnPlanetSurface && !CameraController.Instance.BeingControlled && Input.GetMouseButtonUp(0))
        {
            TrySelect();
        }

        // Only change selection once per frame
        if (selectionUpdatedThisFrame)
            selectionUpdatedThisFrame = false;

        // Update selected feature
        if (selectedFeature)
            selectedFeature.WhileSelected();
    }

    public void Select(Feature feature)
    {
        // Only select one feature each frame
        if (selectionUpdatedThisFrame)
            return;

        selectionUpdatedThisFrame = true;

        // Deselect current feature
        if (selectedFeature)
        {
            selectedFeature.Selected = false;
            selectedFeature.OnDeselected();

            // Return if trying to select the already selected feature
            if (selectedFeature == feature)
            {
                selectedFeature = null;
                return;
            }
            else
            {
                selectedFeature = null;
            }
        }

        // Select new feature
        if (feature)
        {
            selectedFeature = feature;
            selectedFeature.Selected = true;
            selectedFeature.OnSelected();
        }
    }

    private void TrySelect()
    {
        Debug.Log("Trying to select");

        foreach (Feature feature in Planet.Instance.Features)
        {
            if (feature.Selectable)
            {
                if (feature.CheckClickedOn())
                {
                    Select(feature);
                    return;
                }
            }
        }
    }

    public void Remove(Feature feature)
    {
        Debug.Log("Feature removed: " + gameObject.name);
        Planet.Instance.RemoveFeature(feature);
    }

    public LocalForest AddLocalForest(string forestName, LocalForestSettings forestSettings, ZoneSettings zoneSettings)
    {
        LocalForest localForest = Instantiate(localForestPrefab, Planet.Instance.FeaturesParent).GetComponent<LocalForest>();
        localForest.gameObject.name = forestName;
        localForest.Initialize(forestSettings);
        localForest.InitializeZone(zoneSettings);
        Planet.Instance.Features.Add(localForest);
        return localForest;
    }

    public GlobalForest AddGlobalForest(string forestName, GlobalForestSettings forestSettings)
    {
        GlobalForest globalForest = Instantiate(globalForestPrefab, Planet.Instance.FeaturesParent).GetComponent<GlobalForest>();
        globalForest.gameObject.name = forestName;
        globalForest.Initialize(forestSettings);
        Planet.Instance.Features.Add(globalForest);
        return globalForest;
    }
}
