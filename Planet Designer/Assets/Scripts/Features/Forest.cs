using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public abstract class Forest : Feature
{
    protected struct PoolManager
    {
        public Transform transform;
        public SmartPrefab smartPrefab;
        public GameObject prefab;
    }

    [SerializeField] protected List<PoolManager> poolManagers = new List<PoolManager>();
    [SerializeField] protected ForestSettings settings;

    protected LayerMask raycastLayerMask;
    protected int terrainLayer, waterLayer, defaultLayer;
    protected Noise noise;

    // Preallocated local variables (to avoid creating these thousands of times)
    private float sample1, sample2, sample3;
    private RaycastHit raycastHit;
    private GameObject go;
    private SmartPrefab smartPrefab;
    private float raycastDistance;

    public ForestSettings Settings => settings;

    public void Initialize(ForestSettings settings)
    {
        this.settings = settings;
        inspectObject = settings;
        settings.forest = this;
        
        terrainLayer = LayerMask.NameToLayer("Terrain");
        waterLayer = LayerMask.NameToLayer("Water");
        defaultLayer = LayerMask.NameToLayer("Default");
        UpdateLayerMask();

        Planet.Instance.TerrainSphere.RegenerationCompleted.AddListener(AutoRegenerate);
    }

    public void UpdateLayerMask()
    {
        if (settings.avoidOcean)
            raycastLayerMask = LayerMask.GetMask("Default", "Terrain", "Water");
        else
            raycastLayerMask = LayerMask.GetMask("Default", "Terrain");
    }

    public override void AutoRegenerate()
    {
        if (settings.autoRegenerate)
            Regenerate();
    }

    public override void Regenerate()
    {
        noise = new Noise(settings.seed.value);
        raycastDistance = Planet.Instance.TerrainSphere.ElevationRange.max + 1;

        UpdateObjectPools();
        RemoveAllTrees();
    }

    protected void UpdateObjectPools()
    {
        // Remove pools for removed smartPrefabs or if prefab was changed

        for (int i = 0; i < poolManagers.Count; ++i)
        {
            // If this poolManager exists for a smartPrefab that has been removed
            if (!settings.prefabs.Contains(poolManagers[i].smartPrefab))
                RemovePoolManager();

            // If the prefab in the smartPrefab was changed
            else if (poolManagers[i].smartPrefab.prefab != poolManagers[i].prefab)
                RemovePoolManager();

            void RemovePoolManager()
            {
                Destroy(poolManagers[i].transform.gameObject);
                poolManagers.RemoveAt(i);
                --i;
            }
        }

        // Create pools for new prefabs

        foreach (SmartPrefab smartPrefab in settings.prefabs)
        {
            // If these is no poolManager for this smartPrefab
            if (poolManagers.FindIndex(item => item.smartPrefab == smartPrefab) == -1)
            {
                GameObject poolManagerObj = new GameObject("Pool Manager");
                poolManagerObj.transform.parent = transform;

                PoolManager poolManager = new PoolManager();
                poolManager.transform = poolManagerObj.transform;
                poolManager.smartPrefab = smartPrefab;
                poolManager.prefab = smartPrefab.prefab;
                poolManagers.Add(poolManager);

                // Initialize object pool

                smartPrefab.objectPool = new ObjectPool<GameObject>(
                    () => { return Instantiate(smartPrefab.prefab, poolManager.transform); },
                    null,
                    go => go.SetActive(false),
                    go => Destroy(go),
                    false, 2000, 30000);
            }
        }
    }

    protected void RemoveAllTrees()
    {
        // Checks if colliders are present and should be disabled
        bool DisableColliders()
        {
            foreach (SmartPrefab selectablePrefab in settings.prefabs)
                if (selectablePrefab.prefab && selectablePrefab.prefab.GetComponent<Collider>())
                    return true;
            return false;
        }

        if (DisableColliders())
        {
            // Disables colliders before destroying trees

            Collider collider;
            for (int i = 0; i < poolManagers.Count; ++i)
            {
                foreach (Transform tree in poolManagers[i].transform)
                {
                    if (collider = tree.GetComponent<Collider>())
                        collider.enabled = false;

                    //Destroy(tree.gameObject);
                    poolManagers[i].smartPrefab.objectPool.Release(tree.gameObject);
                }
            }
        }
        else
        {
            // Destroy trees directly

            for (int i = 0; i < poolManagers.Count; ++i)
            {
                foreach (Transform tree in poolManagers[i].transform)
                {
                    //Destroy(tree.gameObject);
                    poolManagers[i].smartPrefab.objectPool.Release(tree.gameObject);
                }
            }
        }
    }

    protected List<SmartPrefab> GetEnabledPrefabs()
    {
        List<SmartPrefab> enabledPrefabs = new List<SmartPrefab>();

        foreach (SmartPrefab smartPrefab in settings.prefabs)
        {
            if (smartPrefab.enabled && smartPrefab.prefab && smartPrefab.scale != 0f)
                enabledPrefabs.Add(smartPrefab);
        }

        return enabledPrefabs;
    }

    protected void TryPlaceTreeAtPoint(Vector3 point, List<SmartPrefab> enabledPrefabs)
    {
        // Sample noise to determine whether or not to place object on this point
        sample1 = noise.Evaluate(point * settings.clusters).Remapped(-1f, 1f, 0f, 100f);

        if (sample1 > settings.coverage)
            return;

        // Find position to place object
        if (!Physics.Raycast(point * raycastDistance, -point, out raycastHit, raycastDistance, raycastLayerMask))
            return;

        // Avoid placing trees on certain layers
        if (settings.avoidOcean && raycastHit.collider.gameObject.layer == waterLayer)
            return;

        if (settings.avoidObjects && raycastHit.collider.gameObject.layer == defaultLayer)
            return;

        // Sample noise to determine which object to instantiate
        sample2 = noise.Evaluate(point * settings.clusters * 2f).Remapped(-1f, 1f, 0f, enabledPrefabs.Count);

        // Sample noise to determine the object's rotation
        sample3 = noise.Evaluate(point * settings.clusters * 3f).Remapped(-1f, 1f, 0f, 360f);

        // Instantiate object

        smartPrefab = enabledPrefabs[(int)sample2];

        //go = Instantiate(smartPrefab.prefab, transform.GetChild(0));

        do
        {
            go = smartPrefab.objectPool.Get();
        }
        while (go == null);

        go.SetActive(true);
        go.transform.localScale = smartPrefab.prefab.transform.localScale * smartPrefab.scale;
        go.transform.position = raycastHit.point;
        go.transform.up = point;
        go.transform.Rotate(0f, sample3, 0f);
    }

    public override void WhileSelected()
    {
        settings.numberOfObjects = 0;

        foreach (PoolManager poolManager in poolManagers)
            settings.numberOfObjects += poolManager.transform.childCount;
    }

}
