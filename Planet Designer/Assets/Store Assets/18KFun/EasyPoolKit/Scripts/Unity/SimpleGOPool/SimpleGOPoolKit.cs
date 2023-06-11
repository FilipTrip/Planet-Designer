using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace EasyPoolKit
{
    public class SimpleGOPoolKit : MonoBehaviour
    {
        public static SimpleGOPoolKit Instance
        {
            get
            {
                if (_instance == null)
                {
                    var poolRoot = new GameObject("SimpleGOPoolKit");
                    var cachedRoot = new GameObject("CachedRoot");
                    cachedRoot.transform.SetParent(poolRoot.transform, false);
                    cachedRoot.gameObject.SetActive(false);
                    _instance = poolRoot.AddComponent<SimpleGOPoolKit>();
                    _instance._cachedRoot = cachedRoot.transform;
                    DontDestroyOnLoad(_instance.gameObject);
                }

                return _instance;
            }
        }

        private static SimpleGOPoolKit _instance;
        
        private Transform _cachedRoot;
        
        private Dictionary<int, GameObject> _prefabTemplates = new Dictionary<int, GameObject>();
        
        private Dictionary<int, SimpleGameObjectPool> _gameObjPools = new Dictionary<int, SimpleGameObjectPool>();

        private Dictionary<int, int> _gameObjRelations = new Dictionary<int, int>();

        private List<RecyclablePoolInfo> _poolInfoList = new List<RecyclablePoolInfo>();

        private bool _ifAppQuit = false;
        
        private void Awake()
        {
            if (_instance != null)
            {
                Debug.LogError("EasyPoolKit == Don't attach SimpleGOPoolManager on any object, use SimpleGOPoolManager.Instance instead!");
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            foreach (var poolPair in _gameObjPools)
            {
                var pool = poolPair.Value;
                var deltaTime = pool.IfIgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
                pool.OnPoolUpdate(deltaTime);
            }
        }
        
        private void OnApplicationQuit()
        {
            _ifAppQuit = true;
        }

        private void OnDestroy()
        {
            if (!_ifAppQuit)
            {
                ClearAllPools(true);
            }
        }
        
        public bool IfPoolValid(int prefabHash)
        {
            return _prefabTemplates.ContainsKey(prefabHash) && _gameObjPools.ContainsKey(prefabHash);
        }

        public SimpleGameObjectPool RegisterPrefab(GameObject prefabAsset, RecyclablePoolConfig config = null)
        {
            Assert.IsNotNull(prefabAsset);

            var prefabHash = prefabAsset.GetInstanceID();

#if EASY_POOL_DEBUG
            if (_gameObjPools.ContainsKey(prefabHash) || _prefabTemplates.ContainsKey(prefabHash))
            {
                Debug.LogError($"EasyPoolKit == RegisterPrefab {prefabAsset.name} but already registered");
                return null;
            }
#endif
            
            if (config == null)
            {
                config = GetSimpleGOPoolConfig(prefabAsset, null);
            }
            
            if (config.SpawnFunc == null)
            {
                config.SpawnFunc = () => DefaultCreateObjectFunc(prefabHash);
            }

            if (config.ExtraArgs == null || config.ExtraArgs.Length == 0)
            {
                config.ExtraArgs = new object[] { _cachedRoot };
            }

            _prefabTemplates[prefabHash] = prefabAsset;
            var newPool = new SimpleGameObjectPool(config);
            _gameObjPools[prefabHash] = newPool;
            _poolInfoList.Add(newPool.GetPoolInfoReadOnly());

            return newPool;
        }

        public bool UnRegisterPrefab(int prefabHash)
        {
            _prefabTemplates.Remove(prefabHash);
            RemoveObjectsRelationByAssetHash(prefabHash);
            if (_gameObjPools.TryGetValue(prefabHash, out var pool))
            {
                pool.ClearAll();
                _poolInfoList.Remove(pool.GetPoolInfoReadOnly());
                _gameObjPools.Remove(prefabHash);

                return true;
            }

            return false;
        }

        public GameObject SimpleSpawn(GameObject prefabTemplate)
        {
            if (prefabTemplate == null)
            {
                return null;
            }

            var prefabHash = prefabTemplate.GetInstanceID();

            if (!_gameObjPools.TryGetValue(prefabHash, out var pool))
            {
                pool = RegisterPrefab(prefabTemplate);
            }

            var newObj = pool.SpawnObject();

            _gameObjRelations[newObj.GetInstanceID()] = prefabHash;
            
            return newObj;
        }
        
        public bool TrySpawn(int prefabHash, out GameObject newObj)
        {
            newObj = null;
            if (_gameObjPools.TryGetValue(prefabHash, out var pool))
            {
                newObj = pool.SpawnObject();
                _gameObjRelations[newObj.GetInstanceID()] = prefabHash;
            }

            return newObj != null;
        }

        public bool Despawn(GameObject usedObj)
        {
            Assert.IsNotNull(usedObj);

            var objHash = usedObj.GetInstanceID();

            if (!_gameObjRelations.TryGetValue(objHash, out var assetHash))
            {
                return false;
            }

            if (!_gameObjPools.TryGetValue(assetHash, out var pool))
            {
                return false;
            }

            _gameObjRelations.Remove(objHash);
            
            return pool.DespawnObject(usedObj);
        }

        public bool ClearPoolByAssetHash(int prefabHash, bool onlyClearUnused = false)
        {
            if (_gameObjPools.TryGetValue(prefabHash, out var pool))
            {
                if (onlyClearUnused)
                {
                    pool.ClearUnusedObjects();   
                    return true;
                }
                else
                {
                    pool.ClearAll();
                    return true;
                }
            }
            
            return false;
        }
        
        public void ClearAllUnusedObjects()
        {
            foreach (var poolPair in _gameObjPools)
            {
                poolPair.Value.ClearUnusedObjects();
            }
        }
        
        public void ClearAllPools(bool ifDestroy)
        {
            foreach (var pool in _gameObjPools)
            {
                pool.Value.ClearAll();
            }

            _gameObjRelations.Clear();
            
            if (ifDestroy)
            {
                if (_cachedRoot)
                {
                    for (int i = _cachedRoot.childCount - 1; i >= 0; i--)
                    {
                        var child = _cachedRoot.GetChild(i).gameObject;
                        Destroy(child);
                    }
                }

                _prefabTemplates.Clear();
                _poolInfoList.Clear();
                _gameObjPools.Clear();
            }
        }

        private List<int> _tempRemovedObjectList = new List<int>();
        
        private void RemoveObjectsRelationByAssetHash(int assetHash)
        {
            foreach (var relation in _gameObjRelations)
            {
                if (relation.Value == assetHash)
                {
                    _tempRemovedObjectList.Add(relation.Key);
                }
            }

            foreach (var removeItem in _tempRemovedObjectList)
            {
                _gameObjRelations.Remove(removeItem);
            }
            
            _tempRemovedObjectList.Clear();
        }

        public List<RecyclablePoolInfo> GetPoolsInfo()
        {
            return _poolInfoList;
        }
        
        private GameObject DefaultCreateObjectFunc(int prefabHash)
        {
            if (_prefabTemplates.TryGetValue(prefabHash, out var prefabAsset))
            {
                if (prefabAsset != null)
                {
                    var gameObj = Instantiate(prefabAsset);
                    return gameObj;
                }
            }
            
            Debug.LogError($"EasyPoolKit == Cannot create object: {prefabHash}");
            return null;
        }

        public RecyclablePoolConfig GetSimpleGOPoolConfig(GameObject prefabAsset, Func<GameObject> spawnFunc)
        {
            Assert.IsNotNull(prefabAsset);

            var poolConfig = new RecyclablePoolConfig()
            {
                ObjectType = RecycleObjectType.GameObject,
                ReferenceType = typeof(GameObject),
                PoolId = $"SimplePool_{prefabAsset.name}_{prefabAsset.GetInstanceID()}",
                SpawnFunc = spawnFunc,
            };

            return poolConfig;
        }
    }
}
