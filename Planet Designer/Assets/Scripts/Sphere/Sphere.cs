using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

[ExecuteAlways]
public class Sphere : MonoBehaviour
{
    public enum SphereType { Ocean, Terrain }

    [SerializeField, HideInInspector] private SphereFace[] sphereFaces;
    [SerializeField, HideInInspector] private SphereInfo info;
    [SerializeField, HideInInspector] private bool initialized;
    [SerializeField, HideInInspector] private Range elevationRange;
    [SerializeField, HideInInspector] private SphereType sphereType;

    public UnityEvent RegenerationCompleted = new UnityEvent();

    public SphereFace[] SphereFaces => sphereFaces;
    public Range ElevationRange => elevationRange;

    public void AutoRegenerate()
    {
        if (Planet.Instance.SurfaceSettings.autoRegenerate)
            Regenerate();
    }

    [ContextMenu("Regenerate")]
    public void Regenerate()
    {
        if (!initialized)
            return;

        if (sphereFaces == null)
            sphereFaces = GetComponentsInChildren<SphereFace>();

        Stopwatch stopwatch = Stopwatch.StartNew();

        ReconstructData();
        UpdateMesh();
        UpdateShaders();
        info.UpdateInfo(this);

        stopwatch.Stop();
        Debug.Log("Regenerated " + gameObject.name + " (" + stopwatch.ElapsedMilliseconds + "ms)");
        RegenerationCompleted.Invoke();
    }

    public void Initialize()
    {
        if (initialized)
            return;

        sphereType = gameObject.name.Contains("Ocean") ? SphereType.Ocean : SphereType.Terrain;
        initialized = true;
        sphereFaces = new SphereFace[6];
        info = GetComponent<SphereInfo>();
        elevationRange = new Range();

        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        for (int i = 0; i < sphereFaces.Length; ++i)
        {
            sphereFaces[i] = new GameObject("Sphere Face").AddComponent<SphereFace>();
            sphereFaces[i].Initialize(transform, directions[i]);
        }
    }

    private void ReconstructData()
    {
        int resolution = sphereType == SphereType.Terrain ?
            Planet.Instance.SurfaceSettings.terrainResolution :
            Planet.Instance.SurfaceSettings.oceanResolution;

        float radius = sphereType == SphereType.Terrain ?
            Planet.Instance.SurfaceSettings.terrainRadius :
            Planet.Instance.SurfaceSettings.oceanRadius;

        if (Planet.Instance.SurfaceSettings.fixEdgeNormals)
            foreach (SphereFace sphereFace in sphereFaces)
                sphereFace.ReconstructData_SeamlessNormals(resolution, radius);
        else
            foreach (SphereFace sphereFace in sphereFaces)
                sphereFace.ReconstructData_NoNormalFix(resolution, radius);

        if (sphereType == SphereType.Terrain)
            foreach (NoiseLayer noiseLayer in Planet.Instance.SurfaceSettings.terrainNoiseLayers)
                if (noiseLayer.enabled)
                    noiseLayer.Run(this);
    }

    private void UpdateMesh()
    {
        elevationRange.Set(sphereFaces[0].Vertices[0].magnitude);

        Material material = sphereType == SphereType.Terrain ?
            Planet.Instance.SurfaceSettings.terrainMaterial :
            Planet.Instance.SurfaceSettings.oceanMaterial;

        foreach (SphereFace sphereFace in sphereFaces)
        {
            sphereFace.UpdateMesh(material);

            foreach (Vector3 vertex in sphereFace.Vertices)
                elevationRange.Expand(vertex.magnitude);
        }
    }

    private void UpdateShaders()
    {
        // If this is ocean sphere: Update the terrain shader's ocean level
        if (sphereType == SphereType.Ocean)
        {
            Planet.Instance.SurfaceSettings.terrainMaterial.SetFloat("_Ocean_Elevation", Planet.Instance.SurfaceSettings.oceanRadius);
        }

        // If this is terrain sphere: Update the terrain shader's elevation range
        else
        {
            Planet.Instance.SurfaceSettings.terrainMaterial.SetFloat("_Min_Elevation", elevationRange.min);
            Planet.Instance.SurfaceSettings.terrainMaterial.SetFloat("_Max_Elevation", elevationRange.max);
        }   
    }

    [ContextMenu("Toggle Mesh In Hierarchy")]
    private void ToggleMeshInHierarchy()
    {
        bool hide = sphereFaces[0].MeshFilter.gameObject.hideFlags == HideFlags.None;

        foreach (SphereFace sphereFace in sphereFaces)
            sphereFace.MeshFilter.gameObject.hideFlags = hide ? HideFlags.HideInHierarchy : HideFlags.None;
    }

}
