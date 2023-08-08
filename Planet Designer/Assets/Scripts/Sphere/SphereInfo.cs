using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class SphereInfo : MonoBehaviour
{
    [Header("Mesh")]
    [SerializeField][ReadOnly] public int vertices;
    [SerializeField][ReadOnly] public int triangles;

    [Header("Sphere")]
    [SerializeField][ReadOnly] public float surfaceArea;
    [SerializeField][ReadOnly] public float rectDensity;

    public void UpdateInfo(Sphere sphere)
    {
        /*SphereSettings settings = sphere.Settings;
        Mesh mesh = sphere.SphereFaces[0].MeshFilter.sharedMesh;

        // Mesh
        vertices = mesh.vertexCount * 6;
        triangles = mesh.triangles.Length * 6;

        // Sphere
        surfaceArea = 4f * Mathf.PI * settings.radius * settings.radius;
        rectDensity = triangles * 0.5f / surfaceArea;*/
    }
}
