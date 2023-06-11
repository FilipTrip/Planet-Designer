using UnityEngine;

[System.Serializable]
public class ToggleablePrefab
{
    [Tooltip("The prefab to use")]
    public GameObject prefab;

    [Tooltip("Whether or not to use this prefab")]
    public bool enabled = true;

    [Tooltip("The scale of this prefab (relative to its original scale)")]
    public float scale = 1.0f;
}
