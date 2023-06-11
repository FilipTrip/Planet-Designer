using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace EasyPoolKit
{
    public class RecyclableGOPoolKit : RecyclableGOPoolManagerBase
    {
        public static RecyclableGOPoolKit Instance
        {
            get
            {
                if (_instance == null)
                {
                    var poolRoot = new GameObject("RecyclableGOPoolKit");
                    var cachedRoot = new GameObject("CachedRoot");
                    cachedRoot.transform.SetParent(poolRoot.transform, false);
                    cachedRoot.gameObject.SetActive(false);
                    _instance = poolRoot.AddComponent<RecyclableGOPoolKit>();
                    _instance.CachedRoot = cachedRoot.transform;
                    DontDestroyOnLoad(_instance.gameObject);
                }

                return _instance;
            }
        }

        private static RecyclableGOPoolKit _instance;

        private void Awake()
        {
            if (_instance != null)
            {
                Debug.LogError("EasyPoolKit == Don't attach RecyclableGOPoolManager on any object, use SimpleGOPoolManager.Instance instead!");
                Destroy(gameObject);
            }
        }

        public override RecyclablePoolConfig GetDefaultPrefabPoolConfig(string poolPrefix, GameObject prefabAsset, Func<RecyclableMonoBehaviour> spawnFunc)
        {
            Assert.IsNotNull(prefabAsset);
            var poolConfig = new RecyclablePoolConfig()
            {
                ObjectType = RecycleObjectType.RecyclableGameObject,
                ReferenceType = typeof(GameObject),
                PoolId = $"RecyclableGOPool_{poolPrefix ?? string.Empty}_{prefabAsset.GetInstanceID()}",
                SpawnFunc = spawnFunc,
            };
            
            return poolConfig;
        }
    }
}
