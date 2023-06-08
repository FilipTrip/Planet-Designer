using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Planet Designer/Sphere Settings")]
public class SphereSettings : ScriptableObject
{
    private Sphere sphere;

    [Tooltip("Whether or not to automatically regenerate this sphere and all dependent features when adjusting any of the values below")]
    public bool autoRegenerate = true;

    [HideInInspector]
    public bool fixEdgeNormals = true;

    [Range(3, 256)]
    [Tooltip("The number of vertices along each side of each mesh used to construct this sphere")]
    public int resolution = 150;

    //[Range(-0.2f, 0.0f)]
    [Range(-1f, 1f), HideInInspector]
    public float vertexDistributionBias = -0.06f;

    [Range(10, 600f)]
    [Tooltip("The radius of this sphere")]
    public float radius = 150;

    [Tooltip("The material used by this sphere. Not recommended to change as this reference will always be reset to the material found in the planet folder when loading the planet")]
    public Material material;

    [Tooltip("NoiseLayers used to deform this sphere into a planet-like surface. Two to four NoiseLayers are recommended")]
    public List<NoiseLayer> noiseLayers = new List<NoiseLayer>();

    public void SetSphere(Sphere sphere)
    {
        this.sphere = sphere;
    }

    private void OnValidate()
    {
        if (sphere && autoRegenerate)
            sphere.Regenerate();
    }

}
