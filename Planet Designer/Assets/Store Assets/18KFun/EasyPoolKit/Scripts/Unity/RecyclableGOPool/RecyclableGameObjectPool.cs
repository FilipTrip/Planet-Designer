using UnityEngine;

namespace EasyPoolKit
{
    public class RecyclableGameObjectPool : RecyclablePoolBase<RecyclableMonoBehaviour>
    {
        private Transform _cachedRoot;

        public RecyclableGameObjectPool(RecyclablePoolConfig config) : base(config)
        {
        }

        protected override void OnInitByParams(object[] args)
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
        
        protected override void OnObjectInit(RecyclableMonoBehaviour usedObj)
        {
            usedObj.Pool = this;
            usedObj.PoolId = PoolId;
        }

        protected override void OnObjectEnqueue(RecyclableMonoBehaviour usedObj)
        {
            usedObj.transform.SetParent(_cachedRoot, true);
        }

        protected override void OnObjectDequeue(RecyclableMonoBehaviour usedObj)
        {
            usedObj.transform.SetParent(null, true);
        }

        protected override void OnObjectDeInit(RecyclableMonoBehaviour usedObj)
        {
            if (usedObj)
            {
                Object.Destroy(usedObj.gameObject);
            }
        }
    }
}
