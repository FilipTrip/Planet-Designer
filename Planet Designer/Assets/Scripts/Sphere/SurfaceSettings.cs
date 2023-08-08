using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceSettings : ScriptableObject
{
    // Common

    [Tooltip("Whether or not to automatically regenerate this sphere and all dependent features when adjusting any of the values below")]
    public bool autoRegenerate = true;

    [Tooltip("Fixes normals along the edges of each mesh")]
    public bool fixEdgeNormals = true;

    [Range(-1f, 1f)]
    [Tooltip("Affects the distribution of vertices")]
    public float vertexDistributionBias = -0.06f;

    // Terrain Sphere

    [Range(5, 254)]
    [Tooltip("The number of vertices along each side of each mesh used to construct this sphere")]
    public int terrainResolution = 150;

    [Range(10, 600f)]
    [Tooltip("The radius of this sphere")]
    public float terrainRadius = 150;

    [Tooltip("The material used by this sphere. Not recommended to change as this reference will always be reset to the material found in the planet folder when loading the planet")]
    public Material terrainMaterial;

    [Tooltip("Noise layers used to deform a sphere into a planet-like surface. Multiple layers will be multiplied. Two to four Layers are recommended")]
    public List<NoiseLayer> terrainNoiseLayers = new List<NoiseLayer>();

    // Ocean Sphere

    [Range(5, 254)]
    [Tooltip("The number of vertices along the side of each mesh used to construct this sphere")]
    public int oceanResolution = 150;

    [Range(10, 600f)]
    [Tooltip("The radius of this sphere")]
    public float oceanRadius = 150;

    [Tooltip("The material used by this sphere. Not recommended to change as this reference will always be reset to the material found in the planet folder when loading the planet")]
    public Material oceanMaterial;

    [Range(0.01f, 3f)]
    [Tooltip("The ocean level relative to the radius of the terrain sphere")]
    public float oceanLevel = 0.98f;

    // Editor settings

    [HideInInspector] public static bool showHiddenProperties = false;

    // Methods

    [ContextMenu("Toggle Hidden Properties")]
    public void ToggleHiddenProperties()
    {
        showHiddenProperties = !showHiddenProperties;
    }

    public void UpdateOceanRadius()
    {
        oceanRadius = terrainRadius * oceanLevel;
    }

}
