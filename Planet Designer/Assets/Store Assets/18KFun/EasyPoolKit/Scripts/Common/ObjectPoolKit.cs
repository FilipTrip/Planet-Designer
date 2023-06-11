using System;
using System.Collections.Generic;

namespace EasyPoolKit
{
    /// <summary>
    /// Object Pool used for any C# objects inherited the interface of IRecyclable
    /// </summary>
    public static class ObjectPoolKit
    {
        private static Dictionary<Type, RecyclableObjectPool> _poolDictionary = new Dictionary<Type, RecyclableObjectPool>();

        private static List<RecyclablePoolInfo> _poolInfoList = new List<RecyclablePoolInfo>();
        
        public static int TotalCount => _poolInfoList.Count;

        public static T Spawn<T>() where T : class, IRecyclable, new()
        {
            Type refType = typeof(T);
            
            if (!_poolDictionary.TryGetValue(refType, out var pool))
            {
                var newPool = new RecyclableObjectPool(GetPoolConfig<T>(refType));
                _poolDictionary.Add(refType, newPool);
                _poolInfoList.Add(newPool.GetPoolInfoReadOnly());
            }
            
            return _poolDictionary[refType].SpawnObject() as T;
        }

        public static bool Despawn(IRecyclable usedObj)
        {
            var refType = usedObj.GetType();
            
            if (_poolDictionary.TryGetValue(refType, out var pool))
            {
                return pool.DespawnObject(usedObj);
            }
            
            return false;
        }

        public static bool ClearPoolByType(Type type, bool onlyClearUnused = false)
        {
            if (_poolDictionary.TryGetValue(type, out var pool))
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

        public static void ClearAllUnusedObjects()
        {
            foreach (var poolPair in _poolDictionary)
            {
                poolPair.Value.ClearUnusedObjects();
            }
        }

        public static void ClearAllPools(bool ifDestroy = false)
        {
            foreach (var pool in _poolDictionary)
            {
                pool.Value.ClearAll();
            }

            if (ifDestroy)
            {
                _poolInfoList.Clear();
                _poolDictionary.Clear();
            }
        }

        public static List<RecyclablePoolInfo> GetPoolsInfo()
        {
            return _poolInfoList;
        }

        private static RecyclablePoolConfig GetPoolConfig<T>(Type refType) where T : class, IRecyclable, new()
        {
            var poolConfig = new RecyclablePoolConfig()
            {
                ObjectType = RecycleObjectType.Object,
                ReferenceType = refType,
                PoolId = $"Reference_{refType.ToString()}",
                SpawnFunc = () => new T(),
            };

            return poolConfig;
        }
    }
}
