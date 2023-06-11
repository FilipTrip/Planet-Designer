using System;

namespace EasyPoolKit
{
    public interface IAbstractPool<T> where T : class
    {
        RecycleObjectType ObjectType { get; }

        Type ReferenceType { get; }

        string PoolId { get; }

        int? InitCreateCount { get; }

        int? MaxSpawnCount { get; }

        int? MaxDespawnCount { get; }

        float? AutoClearTime { get; }

        int GetCachedObjectCount();

        int GetUsedObjectCount();

        int GetTotalObjectCount();

        Func<object> SpawnFunc { get; set; }

        PoolReachMaxLimitType ReachMaxLimitType { get; }

        PoolDespawnDestroyType DespawnDestroyType { get; }

        PoolClearType ClearType { get; }

        bool IfIgnoreTimeScale { get; }
        
        T SpawnObject();

        bool TrySpawnObject(out T newObj);

        bool DespawnObject(T usedObj);

        void ClearUnusedObjects();
        
        void ClearAll();

        void OnPoolUpdate(float deltaTime);

        RecyclablePoolInfo GetPoolInfoReadOnly();
    }
}
