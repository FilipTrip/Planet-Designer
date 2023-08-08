using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SphereFace : MonoBehaviour
{
    [SerializeField] private Mesh mesh;
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private MeshCollider meshCollider;

    [SerializeField] private int resolution;
    [SerializeField] private Vector3 localUp;
    [SerializeField] private Vector3 axisA;
    [SerializeField] private Vector3 axisB;

    [SerializeField] private Vector3[] vertices;
    [SerializeField] private int[] triangles;
    [SerializeField] private Vector3[] normals;
    [SerializeField] private Vector2[] uvs;

    [SerializeField, HideInInspector] private bool initialized;

    [SerializeField] private bool drawVertices;
    [SerializeField] private bool drawNormals;

    public MeshFilter MeshFilter => meshFilter;
    public Vector3[] Vertices => vertices;
    public int[] Triangles => triangles;

    public void Initialize(Transform parent, Vector3 localUp)
    {
        if (initialized)
            return;

        initialized = true;
        this.localUp = localUp;
        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);

        transform.parent = parent;
        //gameObject.hideFlags = HideFlags.HideInHierarchy;
        gameObject.layer = transform.parent.gameObject.layer;

        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        
        mesh = new Mesh();
        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    public void ReconstructData_NoNormalFix(int resolution, float radius)
    {
        this.resolution = resolution;
        float vertexDistributionBias = Planet.Instance.SurfaceSettings.vertexDistributionBias;

        vertices = new Vector3[resolution * resolution];
        triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        uvs = new Vector2[resolution * resolution];

        int triangleIndex = 0;
        int vertexIndex = 0;

        Vector2 percent;
        Vector3 pointOnUnitCube, pointOnUnitSphere;

        for (int y = 0; y < resolution; ++y)
        {
            for (int x = 0; x < resolution; ++x)
            {
                percent = new Vector2(x, y) / (resolution - 1);

                percent = new Vector2(
                    percent.x.CustomSmoothstep(vertexDistributionBias),
                    percent.y.CustomSmoothstep(vertexDistributionBias));

                pointOnUnitCube = localUp + (percent.x - 0.5f) * 2f * axisA + (percent.y - 0.5f) * 2f * axisB;
                pointOnUnitSphere = pointOnUnitCube.normalized;

                vertices[vertexIndex] = pointOnUnitSphere * radius;

                if (x != resolution - 1 && y != resolution - 1)
                {
                    triangles[triangleIndex] = vertexIndex;
                    triangles[triangleIndex + 1] = vertexIndex + resolution + 1;
                    triangles[triangleIndex + 2] = vertexIndex + resolution;

                    triangles[triangleIndex + 3] = vertexIndex;
                    triangles[triangleIndex + 4] = vertexIndex + 1;
                    triangles[triangleIndex + 5] = vertexIndex + resolution + 1;
                    triangleIndex += 6;
                }

                // Set uv of vertex
                uvs[vertexIndex] = new Vector2(percent.x, percent.y);

                // Increment vertexIndex
                ++vertexIndex;
            }
        }
    }

    public void ReconstructData_SeamlessNormals(int resolution, float radius)
    {
        this.resolution = resolution;
        float vertexDistributionBias = Planet.Instance.SurfaceSettings.vertexDistributionBias;

        int vertexCount = resolution * resolution;
        int expandedVertexCount = vertexCount + resolution * 4;

        // Triangle count = width * height * 2 triangles per square * 3 vertices per triangle
        int triangleCount = (resolution - 1) * (resolution - 1) * 2 * 3;

        // Expanded count = triangle count + width * 2 triangles per unit * 3 vertices per triangle * 4 sides to expand
        int expandedTriangleCount = triangleCount + (resolution - 1) * 2 * 3 * 4;

        vertices = new Vector3[expandedVertexCount];
        triangles = new int[expandedTriangleCount];
        uvs = new Vector2[vertexCount];

        int vertexIndex = 0;
        int expandedVertexIndex = 0;
        int expandedTriangleIndex = 0;

        Vector3 percent;
        Vector3 pointOnUnitCube;
        bool xEdge, yEdge;

        // Precompute percent.z for edge vertices
        float precomputedZ = (resolution - 2f) / (resolution - 1f);
        precomputedZ = precomputedZ.CustomSmoothstep(vertexDistributionBias);

        // Loop though all vertices on the mesh
        for (int y = -1; y <= resolution; ++y)
        {
            yEdge = y == -1 || y == resolution;

            for (int x = -1; x <= resolution; ++x)
            {
                xEdge = x == -1 || x == resolution;
                
                // Don't create vertex for the four corners of the expanded mesh
                if (xEdge && yEdge)
                    continue;

                // Calculate vertex position on unit cube
                // (Edge vertices will be clamped to the real mesh by the smoothstep function)

                percent.x = (float)x / (resolution - 1);
                percent.y = (float)y / (resolution - 1);
                percent.z = xEdge || yEdge ? precomputedZ : 1f;

                // Use smoothstep function to bias the vertex distribution in order to lower the distortion in the mesh
                percent.x = percent.x.CustomSmoothstep(vertexDistributionBias);
                percent.y = percent.y.CustomSmoothstep(vertexDistributionBias);

                pointOnUnitCube =
                      (percent.z - 0.5f) * 2f * localUp
                    + (percent.x - 0.5f) * 2f * axisA
                    + (percent.y - 0.5f) * 2f * axisB;

                // Normalize unit cube position to get unit sphere position, multipy by radius
                vertices[expandedVertexIndex] = pointOnUnitCube.normalized * radius;

                // If vertex is not on left or bottom edge
                if (x != resolution && y != resolution)
                {
                    // And if vertex does not expand down left into a corner
                    if (expandedVertexIndex != resolution - 1 &&
                        expandedVertexIndex != expandedVertexCount - resolution - 2 - resolution &&
                        expandedVertexIndex != expandedVertexCount - resolution - 2 )
                    {
                        // Calculate index of vertex below (normally vertexIndex + resolution)
                        int indexBelow = expandedVertexIndex + resolution + (y == -1 || y == resolution - 1 ? 1 : 2);

                        // Create mesh square reaching down left

                        // 1 -
                        // 3 2
                        triangles[expandedTriangleIndex    ] = expandedVertexIndex;
                        triangles[expandedTriangleIndex + 1] = indexBelow + 1;
                        triangles[expandedTriangleIndex + 2] = indexBelow;

                        // 1 2
                        // - 3
                        triangles[expandedTriangleIndex + 3] = expandedVertexIndex;
                        triangles[expandedTriangleIndex + 4] = expandedVertexIndex + 1;
                        triangles[expandedTriangleIndex + 5] = indexBelow + 1;

                        expandedTriangleIndex += 6;
                    }
                }

                // If not edge vertex
                if (!xEdge && !yEdge)
                {
                    // Set uv of vertex
                    uvs[vertexIndex++] = new Vector2(percent.x, percent.y);
                }

                // Increment expandedVertexIndex
                ++expandedVertexIndex;
            }
        }
    }

    public void UpdateMesh(Material material)
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        normals = mesh.normals;
        
        if (Planet.Instance.SurfaceSettings.fixEdgeNormals)
            RemoveOuterEdge();

        mesh.SetUVs(0, uvs);
        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
        meshRenderer.sharedMaterial = material;
    }

    private void RemoveOuterEdge()
    {
        int vertexCount = resolution * resolution;

        // Triangle count = width * height * 2 triangles per square * 3 vertices per triangle
        int triangleCount = (resolution - 1) * (resolution - 1) * 2 * 3;

        Vector3[] newVertices = new Vector3[vertexCount];
        int[] newTriangles = new int[triangleCount];
        Vector3[] newNormals = new Vector3[vertexCount];

        int newVertexIndex = 0;
        int oldVertexIndex = 0;
        int newTriangleIndex = 0;

        bool xEdge, yEdge;

        // Loop though all vertices on the mesh
        for (int y = -1; y <= resolution; ++y)
        {
            yEdge = y == -1 || y == resolution;

            for (int x = -1; x <= resolution; ++x)
            {
                xEdge = x == -1 || x == resolution;

                // No verices exist in the corners
                if (xEdge && yEdge)
                    continue;

                // If edge vertex, don't copy
                if (xEdge || yEdge)
                {
                    ++oldVertexIndex;
                    continue;
                }

                // Copy vertex position and normal
                newVertices[newVertexIndex] = vertices[oldVertexIndex];
                newNormals[newVertexIndex] = normals[oldVertexIndex];
                
                // If new vertex expands down left into a square within the non-expanded mesh
                if (x < resolution - 1 && y < resolution - 1)
                {
                    // Create mesh square reaching down left

                    // 1 -
                    // 3 2
                    newTriangles[newTriangleIndex    ] = newVertexIndex;
                    newTriangles[newTriangleIndex + 1] = newVertexIndex + resolution + 1;
                    newTriangles[newTriangleIndex + 2] = newVertexIndex + resolution;

                    // 1 2
                    // - 3
                    newTriangles[newTriangleIndex + 3] = newVertexIndex;
                    newTriangles[newTriangleIndex + 4] = newVertexIndex + 1;
                    newTriangles[newTriangleIndex + 5] = newVertexIndex + resolution + 1;

                    newTriangleIndex += 6;
                }

                ++newVertexIndex;
                ++oldVertexIndex;
            }
        }

        mesh.Clear();
        mesh.vertices = vertices = newVertices;
        mesh.triangles = triangles = newTriangles;
        mesh.normals = normals = newNormals;
    }

    private void OnDrawGizmos()
    {
        if (drawVertices || drawNormals)
        {
            Vector3 vertex;
            for (int i = 0; i < vertices.Length; ++i)
            {
                vertex = vertices[i];

                if (drawVertices)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawSphere(vertex, 1f);
                }

                if (drawNormals)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(vertex, vertex + normals[i] * 5f);
                }
            }
        }
    }

}