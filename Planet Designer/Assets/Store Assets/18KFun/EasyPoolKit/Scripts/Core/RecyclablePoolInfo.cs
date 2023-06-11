using System;

namespace EasyPoolKit
{
    public sealed class RecyclablePoolInfo
    {
        public RecycleObjectType ObjectType { get; private set; } = RecycleObjectType.Object;
        public Type ReferenceType { get; private set; }
        public string PoolId { get; private set; } = string.Empty;
        public int? InitCreateCount { get; private set; } = null;
        public PoolReachMaxLimitType ReachMaxLimitType { get; private set; } = PoolReachMaxLimitType.Default;
        public int? MaxSpawnCount { get; private set; } = null;
        public PoolDespawnDestroyType DespawnDestroyType { get; private set; } = PoolDespawnDestroyType.Default;
        public int? MaxDespawnCount { get; private set; } = null;
        public PoolClearType ClearType { get; private set; } = PoolClearType.Default;
        public float? AutoClearTime { get; private set; } = null;
        public bool IfIgnoreTimeScale { get; private set; } = false;

        public int CachedObjectCount => _getCachedCount?.Invoke() ?? 0;
        
        public int UsedObjectCount => _getUsedCount?.Invoke() ?? 0;
        
        public int TotalObjectCount => _getTotalCount?.Invoke() ?? 0;

        public object ExtraInfo { get; private set; } = null;

        private Func<int> _getCachedCount;

        private Func<int> _getUsedCount;

        private Func<int> _getTotalCount;
        
        public RecyclablePoolInfo(RecyclablePoolConfig config, 
            Func<int> getCachedCount,
            Func<int> getUsedCount,
            Func<int> getTotalCount, 
            object extraInfo)
        {
            _getCachedCount = getCachedCount;
            _getUsedCount = getUsedCount;
            _getTotalCount = getTotalCount;
            ObjectType = config.ObjectType;
            ReferenceType = config.ReferenceType;
            PoolId = config.PoolId;
            InitCreateCount = config.InitCreateCount;
            ReachMaxLimitType = config.ReachMaxLimitType;
            MaxSpawnCount = config.MaxSpawnCount;
            DespawnDestroyType = config.DespawnDestroyType;
            MaxDespawnCount = config.MaxDespawnCount;
            ClearType = config.ClearType;
            AutoClearTime = config.AutoClearTime;
            IfIgnoreTimeScale = config.IfIgnoreTimeScale;

            _getCachedCount = getCachedCount;
            _getUsedCount = getUsedCount;
            _getTotalCount = getTotalCount;
            ExtraInfo = extraInfo;
        }

        public string GetDebugConfigInfo()
        {
#if EASY_POOL_DEBUG
            var stringBuilder = new System.Text.StringBuilder();
            stringBuilder.Append("RecyclablePoolInfo == ConfigInfo\n")
                .Append("PoolID:").Append(PoolId).Append('\n')
                .Append("ObjectType:").Append(ObjectType.ToString()).Append('\n')
                .Append("ReferenceType:").Append(ReferenceType.ToString()).Append('\n')
                .Append("==========").Append(ReferenceType.ToString()).Append('\n')
                .Append("InitCreateCount:").Append(InitCreateCount.HasValue ? InitCreateCount.Value.ToString() : "-").Append('\n')
                .Append("==========").Append(ReferenceType.ToString()).Append('\n')
                .Append("ReachMaxLimitType:").Append(ReachMaxLimitType.ToString()).Append('\n')
                .Append("MaxSpawnCount:").Append(MaxSpawnCount.HasValue ? MaxSpawnCount.Value.ToString() : "-").Append('\n')
                .Append("==========").Append(ReferenceType.ToString()).Append('\n')
                .Append("DespawnDestroyType:").Append(DespawnDestroyType.ToString()).Append('\n')
                .Append("MaxDespawnCount:").Append(MaxDespawnCount.HasValue ? MaxDespawnCount.Value.ToString() : "-").Append('\n')
                .Append("==========").Append(ReferenceType.ToString()).Append('\n')
                .Append("ClearType:").Append(ClearType.ToString()).Append('\n')
                .Append("==========").Append(ReferenceType.ToString()).Append('\n')
                .Append("AutoClearTime:").Append((AutoClearTime.HasValue ? AutoClearTime.Value.ToString() : "-")).Append('\n')
                .Append("==========").Append(ReferenceType.ToString()).Append('\n')
                .Append("IfIgnoreTimeScale:").Append(IfIgnoreTimeScale.ToString()).Append('\n');

            return stringBuilder.ToString();
#else
            return string.Empty;
#endif
        }

        public string GetDebugRunningInfo()
        {
#if EASY_POOL_DEBUG
            var stringBuilder = new System.Text.StringBuilder();
            stringBuilder.Append("RecyclablePoolInfo == RunningInfo\n")
                .Append("PoolID:").Append(PoolId).Append('\n')
                .Append("ObjectType:").Append(ObjectType.ToString()).Append('\n')
                .Append("ReferenceType:").Append(ReferenceType.ToString()).Append('\n')
                .Append("==========").Append(ReferenceType.ToString()).Append('\n')
                .Append("CachedObjectCount:").Append(CachedObjectCount).Append('/').Append(TotalObjectCount).Append('\n')
                .Append("UsedObjectCount:").Append(UsedObjectCount).Append('/').Append(TotalObjectCount).Append('\n')
                .Append("TotalObjectCount").Append('/').Append("MaxSpawnCount:").Append(MaxSpawnCount.HasValue ? MaxSpawnCount.Value.ToString() : "-").Append('\n');

            return stringBuilder.ToString();
#else
            return string.Empty;
#endif
        }
    }
}
