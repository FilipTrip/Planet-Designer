using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace EasyPoolKit
{
    public class SimpleGameObjectPool : ISimpleGameObjectPool
    {
        private Transform _cachedRoot;
        
        public RecycleObjectType ObjectType => RecycleObjectType.GameObject;

        public Type ReferenceType { get; private set; }

        public string PoolId { get; private set; }

        protected Queue<GameObject> CachedQueue { get; private set; }

        protected LinkedList<GameObject> UsedList { get; private set; }

        protected Dictionary<int, float> UsedTimeDic { get; private set; }

        public int? InitCreateCount { get; private set; }

        public int? MaxSpawnCount { get; private set; }

        public int? MaxDespawnCount { get; private set; }

        public float? AutoClearTime { get; private set; }

        public Func<object> SpawnFunc { get; set; }

        public  PoolReachMaxLimitType ReachMaxLimitType { get; private set; }

        public PoolDespawnDestroyType DespawnDestroyType { get; private set; }

        public PoolClearType ClearType { get; private set; }

        public bool IfIgnoreTimeScale { get; private set; }

        private int _objectCounter = 0;

        private bool _ifInit = false;
        
        protected RecyclablePoolInfo PoolInfo;
        
        public SimpleGameObjectPool(RecyclablePoolConfig config)
        {
            InitByConfig(config);
        }

        protected RecyclablePoolInfo InitByConfig(RecyclablePoolConfig config)
        {
            Assert.IsTrue(!_ifInit);
            Assert.IsNotNull(config);
            Assert.IsNotNull(config.SpawnFunc);
            Assert.IsTrue(config.ObjectType == RecycleObjectType.GameObject);
            
            // ObjectType = config.ObjectType;
            ReferenceType = config.ReferenceType;
            PoolId = config.PoolId;
            SpawnFunc = config.SpawnFunc;
            ReachMaxLimitType = config.ReachMaxLimitType;
            DespawnDestroyType = config.DespawnDestroyType;
            ClearType = config.ClearType;
            InitCreateCount = config.InitCreateCount;
            MaxSpawnCount = config.MaxSpawnCount;
            MaxDespawnCount = config.MaxDespawnCount;
            AutoClearTime = config.AutoClearTime;
            IfIgnoreTimeScale = config.IfIgnoreTimeScale;

            PoolInfo = new RecyclablePoolInfo(config, GetCachedObjectCount, GetUsedObjectCount, GetTotalObjectCount, this);
            
            CachedQueue = MaxSpawnCount.HasValue
                ? new Queue<GameObject>(MaxSpawnCount.Value + 1)
                : new Queue<GameObject>();

            UsedList = new LinkedList<GameObject>();
            UsedTimeDic = new Dictionary<int, float>();
            
            OnInitByParams(config.ExtraArgs);
            InitCachedPool();
            _ifInit = true;
            
#if EASY_POOL_DEBUG
            Debug.Log($"EasyPoolKit == Create:Root:\n{GetDebugConfigInfo()}");

            //Check if params are valid
            if (ReachMaxLimitType == PoolReachMaxLimitType.RejectNull ||
                ReachMaxLimitType == PoolReachMaxLimitType.RecycleOldest)
            {
                Assert.IsTrue(MaxSpawnCount.HasValue && MaxSpawnCount.Value > 0);
            }

            if (DespawnDestroyType == PoolDespawnDestroyType.DestroyToLimit)
            {
                Assert.IsTrue(MaxDespawnCount.HasValue && MaxDespawnCount.Value > 0);
            }

            if (MaxSpawnCount.HasValue && MaxDespawnCount.HasValue)
            {
                Assert.IsTrue(MaxDespawnCount.Value <= MaxSpawnCount.Value);
            }

            if (MaxSpawnCount.HasValue && InitCreateCount.HasValue)
            {
                Assert.IsTrue(InitCreateCount.Value <= MaxSpawnCount.Value);
            }
#endif
            return PoolInfo;
        }
        
        public GameObject SpawnObject()
        {
            GameObject cachedObj = null;

            if (GetCachedObjectCount() > 0)
            {
                cachedObj = CachedQueue.Dequeue();
            }
            else
            {
                bool ifReachLimit = false;

                if (ReachMaxLimitType != PoolReachMaxLimitType.Default && MaxSpawnCount.HasValue)
                {
                    ifReachLimit = GetTotalObjectCount() >= MaxSpawnCount.Value;
                }

                if (!ifReachLimit)
                {
                    cachedObj = CreateObject();
                }
                else
                {
                    switch (ReachMaxLimitType)
                    {
                        case PoolReachMaxLimitType.RecycleOldest:
                            cachedObj = RecycleOldestObject();
                            break;
                        case PoolReachMaxLimitType.Default:
                            //Impossible
                        case PoolReachMaxLimitType.RejectNull:
                        default:
                            //do nothing
                            break;
                    }
                }
            }

            if (cachedObj != null)
            {
                OnObjectDequeue(cachedObj);
                UsedTimeDic[cachedObj.GetInstanceID()] = 0f;
                UsedList.AddLast(cachedObj);
            }

            return cachedObj;
        }

        public bool TrySpawnObject(out GameObject newObj)
        {
            newObj = SpawnObject();
            return newObj != null;
        }

        public bool DespawnObject(GameObject usedObj)
        {
#if EASY_POOL_DEBUG
            if (usedObj == null)
            {
                Debug.LogError("EasyPoolKit == DespawnObject usedObj should not be null");
                return false;
            }
#endif       
            var usedNode = UsedList.Find(usedObj);

            if (usedNode == null)
            {
                Debug.LogError($"EasyPoolKit == Cannot find usedObj:{usedObj.name}");
                return false;
            }

            bool ifReachLimit = false;
            
            if (DespawnDestroyType == PoolDespawnDestroyType.DestroyToLimit)
            {
                if (MaxDespawnCount.HasValue && GetTotalObjectCount() > MaxDespawnCount.Value)
                {
                    ifReachLimit = true;
                }
            }

            UsedList.Remove(usedNode);
            UsedTimeDic.Remove(usedObj.GetInstanceID());

            if (ifReachLimit)
            {
                DestroyPoolObject(usedObj);
            }
            else
            {
                CachedQueue.Enqueue(usedObj);
                OnObjectEnqueue(usedObj);
            }

            return true;
        }

        public void ClearUnusedObjects()
        {
            foreach (var cachedItem in CachedQueue)
            {
                DestroyPoolObject(cachedItem);
            }

            CachedQueue.Clear();
        }
        
        public void ClearAll()
        {
            switch (ClearType)
            {
                case PoolClearType.Default:
                    PoolClearAll();
                    break;
                case PoolClearType.ClearToLimit:
                    PoolClearToLimit();
                    break;
            }
        }

        public int GetCachedObjectCount()
        {
            return CachedQueue.Count;
        }

        public int GetUsedObjectCount()
        {
            return UsedList.Count;
        }

        public int GetTotalObjectCount()
        {
            return CachedQueue.Count + UsedList.Count;
        }
        
        public void OnPoolUpdate(float deltaTime)
        {
            if (UsedList.Count > 0)
            {
                var beginNode = UsedList.First;
                float collectTime = AutoClearTime ?? -1f;
                
                while (beginNode != null)
                {
                    var currentNode = beginNode;
                    beginNode = beginNode.Next;
                    var usedObj = currentNode.Value;

                    var objId = usedObj.GetInstanceID();
                    
                    var usedTime = UsedTimeDic[objId];
                    usedTime += deltaTime;
                    UsedTimeDic[objId] = usedTime;
                    
                    if (collectTime > 0 && usedTime >= collectTime)
                    {
                        DespawnObject(usedObj);
                    }
                }
            }
        }

        public RecyclablePoolInfo GetPoolInfoReadOnly()
        {
            return PoolInfo;
        }

        private void InitCachedPool()
        {
            if (!InitCreateCount.HasValue)
            {
                return;
            }

            var initCount = InitCreateCount.Value;

            for (int i = 0; i < initCount; i++)
            {
                var cachedObj = CreateObject();
                CachedQueue.Enqueue(cachedObj);
                OnObjectEnqueue(cachedObj);
            }
        }

        private GameObject CreateObject()
        {
            _objectCounter++;
            var cachedObj = SpawnFunc?.Invoke() as GameObject;

            if (cachedObj)
            {
#if EASY_POOL_DEBUG
                cachedObj.name = $"{PoolId}_{_objectCounter}";
#endif
                OnObjectInit(cachedObj);
            }
#if EASY_POOL_DEBUG
            else
            {
                Debug.LogError($"EasyPoolKit == GenerateObject {PoolId} Should not be null!");
            }
#endif

            return cachedObj;
        }

        protected virtual void OnInitByParams(object[] args)
        {
            var extraArgs = args;

            if (extraArgs != null && extraArgs.Length > 0 && extraArgs[0] is Transform root)
            {
                _cachedRoot = root;
            }
            else
            {
                Debug.LogError("EasyPoolKit == Create RecyclableMonoPool should input the root Transform in extraArgs[0]");
            }
        }
        
        protected virtual void OnObjectInit(GameObject usedObj){ }

        protected virtual void OnObjectEnqueue(GameObject usedObj)
        {
            usedObj.transform.SetParent(_cachedRoot, true);
        }

        protected virtual void OnObjectDequeue(GameObject usedObj)
        {
            usedObj.transform.SetParent(null, true);
        }

        protected virtual void OnObjectDeInit(GameObject usedObj)
        {
            if (usedObj)
            {
                UnityEngine.Object.Destroy(usedObj);
            }
        }

        private GameObject RecycleOldestObject()
        {
#if EASY_POOL_DEBUG
            Assert.IsTrue(GetUsedObjectCount() > 0, $"EasyPoolKit == RecycleOldestObject {PoolId} UsedCount should > 0");
#endif
            var firstNode = UsedList.First;
            UsedList.Remove(firstNode);
            var oldestObj = firstNode.Value;

            if (oldestObj)
            {
                
            }
#if EASY_POOL_DEBUG
            else
            {
                Debug.LogError($"EasyPoolKit == RecycleOldestObject {PoolId} from UsedList Should not be null!");
            }
#endif
            
            //As it is a free node, need not to add it to cachedQueue 
            return oldestObj;
        }

        private void PoolClearAll()
        {
            foreach (var cachedItem in CachedQueue)
            {
                DestroyPoolObject(cachedItem);
            }

            CachedQueue.Clear();

            while (UsedList.Count > 0)
            {
                var firstNode = UsedList.First;
                var firstObj = firstNode.Value;
                UsedList.Remove(firstNode);
                DestroyPoolObject(firstObj);
            }

            UsedList.Clear();
        }

        private void PoolClearToLimit()
        {
            if (!InitCreateCount.HasValue)
            {
                PoolClearAll();
                return;
            }

            PoolClearRetain();

            int removeCount = GetCachedObjectCount() - InitCreateCount.Value;

            while (removeCount > 0)
            {
                removeCount--;
                var cachedObj = CachedQueue.Dequeue();
                DestroyPoolObject(cachedObj);
            }
        }

        private void PoolClearRetain()
        {
            while (UsedList.Count > 0)
            {
                var firstNode = UsedList.First;
                var firstObj = firstNode.Value;
                DespawnObject(firstObj);
            }
        }

        private void DestroyPoolObject(GameObject poolObj)
        {
            if (poolObj)
            {
                OnObjectDeInit(poolObj);
            }
        }
        
        #region Debug
        public string GetDebugConfigInfo()
        {
            return PoolInfo?.GetDebugConfigInfo() ?? string.Empty;
        }

        public string GetDebugRunningInfo()
        {
            return PoolInfo?.GetDebugRunningInfo() ?? string.Empty;
        }
        #endregion
    }
}
