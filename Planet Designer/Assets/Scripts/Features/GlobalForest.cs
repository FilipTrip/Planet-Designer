using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Pool;
using Debug = UnityEngine.Debug;

public class GlobalForest : Forest
{
    public override void Regenerate()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        base.Regenerate();
        PlaceTrees();
        
        Debug.Log("Regenerated " + gameObject.name + " (" + stopwatch.ElapsedMilliseconds + "ms)");
    }

    private void PlaceTrees()
    {
        Random.InitState(settings.seed.value);
        List<SmartPrefab> enabledPrefabs = GetEnabledPrefabs();

        if (enabledPrefabs.Count == 0)
            return;

        float density = ((GlobalForestSettings)settings).density;
        int amount = (int)(density * density);

        for (int i = 0; i < amount; ++i)
        {
            TryPlaceTreeAtPoint(Random.onUnitSphere, enabledPrefabs);
        }
    }

    public override void WhileSelected()
    {
        //zone.ManualUpdate();

        settings.numberOfObjects = 0;

        foreach (PoolManager poolManager in poolManagers)
            settings.numberOfObjects += poolManager.transform.childCount;
    }

    public override bool CheckClickedOn()
    {
        return false; //zone.CheckClickedOn();
    }
}
