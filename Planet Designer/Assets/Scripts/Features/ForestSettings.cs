using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ForestSettings : ScriptableObject
{
    [HideInInspector]
    public Forest forest;

    [ReadOnly]
    public int numberOfObjects;

    [Tooltip("Automatically regenerate")]
    public bool autoRegenerate = true;

    [Tooltip("Seed")]
    public Seed seed;

    [Range(0f, 100f)]
    [Tooltip("Controls the success rate of placing a tree")]
    public float coverage = 50f;

    [Range(0f, 30f)]
    [Tooltip("Controls where trees are successfully placed. A lower value results in large clusters, while a higher value reults in a more even distribution")]
    public float clusters = 10f;

    [Tooltip("Stop trees som being placed under water")]
    public bool avoidOcean = true;

    [Tooltip("Stop trees from being placed on top of other objects of the default layer")]
    public bool avoidObjects = true;

    [Tooltip("A collection of prefabs to place. A prefab can be turned on or off without removing it from the list")]
    public List<SmartPrefab> prefabs = new List<SmartPrefab>();
}
