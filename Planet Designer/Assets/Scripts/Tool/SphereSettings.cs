using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereSettings : ScriptableObject
{
    private void Awake()
    {
        showNoiseLayers = name == "Terrain Settings";
    }

    [HideInInspector]
    public Sphere sphere;

    [Tooltip("Whether or not to automatically regenerate this sphere and all dependent features when adjusting any of the values below")]
    public bool autoRegenerate = true;

    [Tooltip("Fixes normals along the edges of each mesh")]
    public bool fixEdgeNormals = true;

    [Range(-1f, 1f)]
    public float vertexDistributionBias = -0.06f;

    [Range(3, 256)]
    [Tooltip("The number of vertices along each side of each mesh used to construct this sphere")]
    public int resolution = 150;

    [Range(10, 600f)]
    [Tooltip("The radius of this sphere")]
    public float radius = 150;

    [Tooltip("The material used by this sphere. Not recommended to change as this reference will always be reset to the material found in the planet folder when loading the planet")]
    public Material material;

    [Tooltip("NoiseLayers used to deform this sphere into a planet-like surface. Two to four NoiseLayers are recommended")]
    public List<NoiseLayer> noiseLayers = new List<NoiseLayer>();

    // Editor settings

    [HideInInspector] public bool showFixEdgeNormals = false;
    [HideInInspector] public bool showVertexDistributionBias = false;
    [HideInInspector] public bool showMaterial = false;
    [HideInInspector] public bool showNoiseLayers = true;

    // Context menu methods

    [ContextMenu("Toggle Fix Edge Normals")]
    private void ToggleFixEdgeNormals() => showFixEdgeNormals = !showFixEdgeNormals;

    [ContextMenu("Toggle Vertex Distribution Bias")]
    private void ToggleVertexDistributionBias() => showVertexDistributionBias = !showVertexDistributionBias;

    [ContextMenu("Toggle Material")]
    private void ToggleMaterial() => showMaterial = !showMaterial;

    [ContextMenu("Toggle Noise Layers")]
    private void ToggleNoiseLayers() => showNoiseLayers = !showNoiseLayers;

    
}
