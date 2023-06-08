using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Planet Designer/Forest Settings")]
public class ForestSettings : ScriptableObject
{
    private Forest forest;

    public Seed seed;

    [Range(0f, 100f)]
    [Tooltip("How many trees to place")]
    public float density = 50f;

    [Range(0f, 30f)]
    [Tooltip("The scale of the seed used to distribute trees. A lower value results in large clusters, while a higher value reults in a more even distribution")]
    public float seedScale = 10f;

    [Tooltip("Stop trees som being placed under water")]
    public bool avoidOcean = true;

    [Tooltip("Stop trees from being placed on top of other objects of the default layer")]
    public bool avoidObjects = true;

    [Tooltip("A collection of prefabs to place. A prefab can be turned on or off without removing it from the list")]
    public List<SelectablePrefab> prefabs = new List<SelectablePrefab>();

    public void SetForest(Forest forest)
    {
        this.forest = forest;
    }

    private void OnValidate()
    {
        if (forest)
        {
            forest.UpdateLayerMask();
            forest.Regenerate();
        }
    }
}
