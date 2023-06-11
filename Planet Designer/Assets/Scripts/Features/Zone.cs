using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Zone : MonoBehaviour
{
    [SerializeField] private ZoneSettings settings;
    [SerializeField] private Feature feature;

    public ZoneSettings Settings => settings;
    public Feature Feature => feature;

    public void Initialize(ZoneSettings settings, Feature feature)
    {
        this.settings = settings;
        this.feature = feature;
    }

    public void ManualUpdate()
    {
        if (CameraController.Instance.BeingControlled || !Reticle.Instance.OnPlanetSurface || !Input.GetMouseButton(0))
            return;

        Vector3 mouseSurfaceDirection = Reticle.Instance.transform.position.normalized;
        List<Vector3> affectedPoints = new List<Vector3>();

        // Remove points
        if (Input.GetKey(KeyCode.LeftShift))
        {
            for (int i = settings.points.Count - 1; i >= 0; --i)
            {
                // Remove point if its center is within the brush
                if (Vector3.Angle(settings.points[i], mouseSurfaceDirection) < Reticle.Instance.BrushAngle)
                {
                    affectedPoints.Add(settings.points[i]);
                    settings.points.RemoveAt(i);
                }
            }

            // Update feature if points where added
            if (affectedPoints.Count != 0)
            {
                // Use smart regeneration if feature is forest
                LocalForest forest;
                if (forest = feature.GetComponent<LocalForest>())
                    forest.SmartRegen_RemoveTrees(affectedPoints);
                else
                    feature.Regenerate();
            }
        }

        // Add points
        else
        {
            int pointAddAttempts = Mathf.Max(300, (int)(Reticle.Instance.BrushAngle * 20));
            float pointRadius = Vector2.Distance(Vector2.up, Vector2.up.RotateAroundZero(settings.pointAngle * Mathf.Deg2Rad));

            for (int i = 0; i < pointAddAttempts; ++i)
            {
                if (affectedPoints.Count == 0 && i >= pointAddAttempts * 0.5f)
                    break;

                // Get direction vector with random offset within the brush angle
                Vector3 newPoint = DirectionOffsetter.Offset(Reticle.Instance.transform.position, Reticle.Instance.BrushAngle, true);

                // Make sure point is not too close to any other point
                if (Clear())
                {
                    // Add point
                    settings.points.Add(newPoint);
                    affectedPoints.Add(newPoint);
                }

                bool Clear()
                {
                    // Check newer points first (since they are more likely to block new points)
                    for (int i = settings.points.Count - 1; i >= 0; --i)
                    {
                        // Angle check
                        //if (Vector3.Angle(newPoint, settings.points[i]) < settings.pointAngle)
                        //return false;

                        // Radius check (more performant)
                        if (Vector3.Distance(newPoint, settings.points[i]) < pointRadius)
                            return false;
                    }

                    return true;
                }
            }

            // Update feature if points where added
            if (affectedPoints.Count != 0)
            {
                // Use smart regeneration if feature is forest
                LocalForest forest;
                if (forest = feature.GetComponent<LocalForest>())
                    forest.SmartRegen_AddTrees(affectedPoints);
                else
                    feature.Regenerate();
            }
        }

        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(settings);
        #endif
    }

    private void OnDrawGizmos()
    {
        float radius = Planet.Instance.TerrainSphere.Settings.radius;
        float pointRadius = Vector2.Distance(new Vector2(0, radius), new Vector2(0, radius).RotateAroundZero(settings.pointAngle * Mathf.Deg2Rad));

        Gizmos.color = new Color(1f, 1f, 1f, 0.5f);

        foreach (Vector3 zonePoint in settings.points)
            Gizmos.DrawSphere(zonePoint * radius, pointRadius);
    }

    public bool CheckClickedOn()
    {
        float pointRadius = Vector2.Distance(Vector2.up, Vector2.up.RotateAroundZero(settings.pointAngle * Mathf.Deg2Rad));
        pointRadius *= 1.5f;

        Vector3 normalizedMousePosition = Reticle.Instance.transform.position.normalized;

        foreach (Vector3 zonePoint in settings.points)
        {
            if (Vector3.Distance(zonePoint, normalizedMousePosition) < pointRadius)
                return true;
        }

        return false;
    }


}
