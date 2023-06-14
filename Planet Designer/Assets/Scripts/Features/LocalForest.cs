using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class LocalForest : Forest
{
    [SerializeField] private Zone zone;

    public void InitializeZone(ZoneSettings zoneSettings)
    {
        zone.Initialize(zoneSettings, this);
    }

    public override void Regenerate()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        
        base.Regenerate();
        SmartRegen_PlaceTrees(zone.Settings.points);

        Debug.Log("Regenerated " + gameObject.name + " (" + stopwatch.ElapsedMilliseconds + "ms)");
    }

    /// <summary>
    /// Generates trees using only the provided zone points
    /// </summary>
    public void SmartRegen_PlaceTrees(List<Vector3> zonePoints)
    {
        List<SmartPrefab> enabledPrefabs = GetEnabledPrefabs();

        if (enabledPrefabs.Count == 0)
            return;

        foreach (Vector3 point in zonePoints)
        {
            TryPlaceTreeAtPoint(point, enabledPrefabs);
        }
    }

    /// <summary>
    /// Removes all trees positioned on the provided zone points
    /// </summary>
    public void SmartRegen_RemoveTrees(List<Vector3> zonePoints)
    {
        foreach (PoolManager poolManager in poolManagers)
        {
            foreach (Transform tree in poolManager.transform)
            {
                for (int i = 0; i < zonePoints.Count; ++i)
                {
                    if (tree.position.normalized == zonePoints[i])
                    {
                        //Destroy(tree.gameObject);
                        poolManager.smartPrefab.objectPool.Release(tree.gameObject);

                        zonePoints.RemoveAt(i);
                        --i;
                    }
                }
            }
        }
    }

    public override void WhileSelected()
    {
        base.WhileSelected();
        zone.WhileSelected();
    }

    public override bool CheckClickedOn()
    {
        return zone.CheckClickedOn();
    }
}
