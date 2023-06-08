using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoiseLayer : SurfaceModifier
{
    private Noise noise;

    [Min(0f)]
    [Tooltip("The amplitude of this NoiseLayer. Affects the magnitude of the terrain")]
    public float amplitude = 1f;

    [Min(0f)]
    [Tooltip("The scale of this NoiseLayer. Affects the scale of the details in the terrain")]
    public float scale = 1f;

    [Tooltip("Allows you to manipulate the random values within the noise. This gives you control over the amplidude of the noise at certain values")]
    public AnimationCurve manipulation;

    public Seed seed;

    public override void Run(Sphere sphere)
    {
        noise = new Noise(seed.value);

        foreach (SphereFace sphereFace in sphere.SphereFaces)
        {
            for (int i = 0; i < sphereFace.Vertices.Length; ++i)
            {
                ModifyVertex(ref sphereFace.Vertices[i]);
            }
        }
    }

    private void ModifyVertex(ref Vector3 vertex)
    {
        vertex +=
            vertex.normalized
            * Manipulate(noise.Evaluate((vertex.normalized + Vector3.one * seed.value) * scale))
            * amplitude
            * amplitude;
    }

    private float Manipulate(float value)
    {
        return manipulation.Evaluate((value + 1f) * 0.5f) * 2f - 1f;
    }
}
