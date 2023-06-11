using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace EasyPoolKit
{
    public abstract class RecyclableGOPoolManagerBase : MonoBehaviour
    {
        public string PoolPrefix = string.Empty;

        protected Transform CachedRoot;
        
        private Dictionary<int, GameObject> _prefabTemplates = new Dictionary<int, GameObject>();
        
        private Dictionary<int, RecyclableGameObjectPool> _gameObjPools = new Dictionary<int, RecyclableGameObjectPool>();
        
        private List<RecyclablePoolInfo> _poolInfoList = new List<RecyclablePoolInfo>();

        private bool _ifAppQuit = false;
        
        public bool IfPoolValid(int prefabHash)
        {
            return _prefabTemplates.ContainsKey(prefabHash) && _gameObjPools.ContainsKey(prefabHash);
        }

        public RecyclableGameObjectPool RegisterPrefab(GameObject prefabAsset, RecyclablePoolConfig config = null)
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
                config = GetDefaultPrefabPoolConfig(PoolPrefix, prefabAsset, null);
            }
            
            if (config.SpawnFunc == null)
            {
                config.SpawnFunc = () => DefaultCreateObjectFunc(prefabHash);
            }

            if (config.ExtraArgs == null || config.ExtraArgs.Length == 0)
            {
                config.ExtraArgs = new object[] { CachedRoot };
            }

            _prefabTemplates[prefabHash] = prefabAsset;
            var newPool = new RecyclableGameObjectPool(config);
            _gameObjPools[prefabHash] = newPool;
            _poolInfoList.Add(newPool.GetPoolInfoReadOnly());

            return newPool;
        }

        public bool UnRegisterPrefab(int prefabHash)
        {
            _prefabTemplates.Remove(prefabHash);
            
            if (_gameObjPools.TryGetValue(prefabHash, out var pool))
            {
                pool.ClearAll();
                _poolInfoList.Remove(pool.GetPoolInfoReadOnly());
                _gameObjPools.Remove(prefabHash);

                return true;
            }

            return false;
        }

        public RecyclableMonoBehaviour SimpleSpawn(GameObject prefabTemplate)
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

            return pool.SpawnObject();
        }

        public T SimpleSpawn<T>(GameObject prefabTemplate) where T :RecyclableMonoBehaviour
        {
            return SimpleSpawn(prefabTemplate) as T;
        }
        
        public bool TrySpawn(int prefabHash, out RecyclableMonoBehaviour recyclableObj)
        {
            recyclableObj = null;
            if (_gameObjPools.TryGetValue(prefabHash, out var pool))
            {
                recyclableObj = pool.SpawnObject();
            }

            return recyclableObj != null;
        }
        
        public bool TrySpawn<T>(int prefabHash, out T recyclableObj) where T :RecyclableMonoBehaviour
        {
            recyclableObj = null;
            if (TrySpawn(prefabHash, out var newObj))
            {
                recyclableObj = newObj as T;
                return true;
            }
            
            return false;
        }

        public bool Despawn(RecyclableMonoBehaviour recyclableObj)
        {
            Assert.IsNotNull(recyclableObj);
            return recyclableObj.DespawnSelf();
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

            if (ifDestroy)
            {
                if (CachedRoot)
                {
                    for (int i = CachedRoot.childCount - 1; i >= 0; i--)
                    {
                        var child = CachedRoot.GetChild(i).gameObject;
                        Destroy(child);
                    }
                }

                _prefabTemplates.Clear();
                _poolInfoList.Clear();
                _gameObjPools.Clear();
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

        public List<RecyclablePoolInfo> GetPoolsInfo()
        {
            return _poolInfoList;
        }
        
        private RecyclableMonoBehaviour DefaultCreateObjectFunc(int prefabHash)
        {
            if (_prefabTemplates.TryGetValue(prefabHash, out var prefabAsset))
            {
                if (prefabAsset != null)
                {
                    var gameObj = Instantiate(prefabAsset);
                    var recyclableObj = gameObj.GetOrCreateRecyclableMono();
                    recyclableObj.PrefabHash = prefabHash;
                    return recyclableObj;
                }
            }
            
            Debug.LogError($"EasyPoolKit == Cannot create object: {prefabHash}");
            return null;
        }

        public abstract RecyclablePoolConfig GetDefaultPrefabPoolConfig(string poolPrefix, GameObject prefabAsset,
            Func<RecyclableMonoBehaviour> spawnFunc);
    }
}
