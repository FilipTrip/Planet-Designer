using System;

namespace EasyPoolKit
{
    public sealed class RecyclablePoolConfig
    {
        public RecycleObjectType ObjectType = RecycleObjectType.Object;
        public Type ReferenceType;
        public string PoolId = string.Empty;
        public Func<object> SpawnFunc;
        public int? InitCreateCount = null;
        public PoolReachMaxLimitType ReachMaxLimitType = PoolReachMaxLimitType.Default;
        public int? MaxSpawnCount = null;
        public PoolDespawnDestroyType DespawnDestroyType = PoolDespawnDestroyType.Default;
        public int? MaxDespawnCount = null;
        public PoolClearType ClearType = PoolClearType.Default;
        public float? AutoClearTime = null;
        public bool IfIgnoreTimeScale = false;
        public object[] ExtraArgs = Array.Empty<object>();
    }
}