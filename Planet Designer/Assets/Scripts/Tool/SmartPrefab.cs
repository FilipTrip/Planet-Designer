using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

[System.Serializable]
public class SmartPrefab
{
    [Tooltip("Whether or not to use this prefab")]
    public bool enabled;

    [Tooltip("The prefab to use")]
    public GameObject prefab;

    [Tooltip("The scale of this prefab (relative to its original scale)")]
    public float scale;

    public ObjectPool<GameObject> objectPool;

}
