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

    [Tooltip("The settings used for this sphere")]
    [SerializeField] private SphereSettings settings;
    [SerializeField, HideInInspector] private SphereFace[] sphereFaces;
    [SerializeField, HideInInspector] private SphereInfo info;
    [SerializeField, HideInInspector] private bool initialized;
    [SerializeField, HideInInspector] private Range elevationRange;
    [SerializeField, HideInInspector] private SphereType sphereType;
    [SerializeField, HideInInspector] private Planet planet;

    public UnityEvent RegenerationCompleted = new UnityEvent();

    public SphereSettings Settings { get { return settings; } set { settings = value; } }
    public SphereFace[] SphereFaces => sphereFaces;
    public Range ElevationRange => elevationRange;

    public void AutoRegenerate()
    {
        if (settings && settings.autoRegenerate)
            Regenerate();
    }

    [ContextMenu("Regenerate")]
    public void Regenerate()
    {
        if (!initialized || !settings)
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

        planet = GetComponentInParent<Planet>();
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

    public void ReconstructData()
    {
        if (settings == null)
            return;

        if (settings.fixEdgeNormals)
            foreach (SphereFace sphereFace in sphereFaces)
                sphereFace.ReconstructData_SeamlessNormals(settings);
        else
            foreach (SphereFace sphereFace in sphereFaces)
                sphereFace.ReconstructData_NoNormalFix(settings);

        foreach (NoiseLayer noiseLayer in settings.noiseLayers)
            if (noiseLayer.enabled)
                noiseLayer.Run(this);
    }

    public void UpdateMesh()
    {
        elevationRange.Set(sphereFaces[0].Vertices[0].magnitude);

        foreach (SphereFace sphereFace in sphereFaces)
        {
            sphereFace.UpdateMesh(settings);

            foreach (Vector3 vertex in sphereFace.Vertices)
                elevationRange.Expand(vertex.magnitude);
        }
    }

    public void UpdateShaders()
    {
        // If this is ocean sphere: Update the terrain shader's ocean level
        if (sphereType == SphereType.Ocean)
        {
            if (planet.TerrainSphere.settings != null && planet.TerrainSphere.settings.material != null)
                planet.TerrainSphere.settings.material.SetFloat("_Ocean_Elevation", settings.radius);
        }

        // If this is terrain sphere: Update the terrain shader's elevation range
        else if (settings != null && settings.material != null)
        {
            settings.material.SetFloat("_Min_Elevation", elevationRange.min);
            settings.material.SetFloat("_Max_Elevation", elevationRange.max);
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
