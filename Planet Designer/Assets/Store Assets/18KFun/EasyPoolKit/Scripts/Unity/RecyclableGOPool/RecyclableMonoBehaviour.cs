using UnityEngine;
using UnityEngine.Assertions;

namespace EasyPoolKit
{
    public class RecyclableMonoBehaviour : MonoBehaviour, IRecyclable
    {
        public static readonly string MessageOnInit = "OnObjectInit";
        public static readonly string MessageOnDeInit = "OnObjectDeInit";
        public static readonly string MessageOnSpawn = "OnObjectSpawn";
        public static readonly string MessageOnDespawn = "OnObjectDespawn";
        
        public RecycleObjectType ObjectType => RecycleObjectType.RecyclableGameObject;

        public bool EnableMessage = false;

        public RecyclableGameObjectPool Pool { get; set; }

        public string PoolId { get; set; }
        
        public int ObjectId { get; set; }

        public int PrefabHash { get; set; }

        public string Name
        {
            get => gameObject.name;
            set => gameObject.name = value;
        }
        
        public float UsedTime { get; set; }

        public virtual void OnObjectInit()
        {
            if (EnableMessage)
            {
                SendMessage(MessageOnInit, SendMessageOptions.DontRequireReceiver);
            }
        }

        public virtual void OnObjectDeInit()
        {
            if (EnableMessage)
            {
                SendMessage(MessageOnDeInit, SendMessageOptions.DontRequireReceiver);
            }
        }

        public virtual void OnObjectSpawn()
        {
            if (EnableMessage)
            {
                SendMessage(MessageOnSpawn, SendMessageOptions.DontRequireReceiver);
            }
        }

        public virtual void OnObjectDespawn()
        {
            UsedTime = 0;
            PrefabHash = 0;
            
            if (EnableMessage)
            {
                SendMessage(MessageOnDespawn, SendMessageOptions.DontRequireReceiver);
            }
        }

        public virtual void OnObjectUpdate(float deltaTime)
        {
            UsedTime += deltaTime;
        }

        public bool DespawnSelf()
        {
            return Pool.DespawnObject(this);
        }
    }

    public static class RecyclableMonoBehaviourExt
    {
        public static RecyclableMonoBehaviour GetOrCreateRecyclableMono(this GameObject gameObj,
            bool enableMessage = false)
        {
            Assert.IsNotNull(gameObj);
            var recyclableMono = gameObj.GetComponent<RecyclableMonoBehaviour>();
            if (recyclableMono)
            {
                return recyclableMono;
            }
            recyclableMono = gameObj.AddComponent<RecyclableMonoBehaviour>();
            recyclableMono.EnableMessage = enableMessage;
            return recyclableMono;
        }
    }
}
