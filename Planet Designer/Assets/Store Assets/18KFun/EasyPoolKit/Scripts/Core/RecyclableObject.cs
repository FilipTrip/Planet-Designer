
namespace EasyPoolKit
{
    public class RecyclableObject : IRecyclable
    {
        public RecycleObjectType ObjectType => RecycleObjectType.Object;
        
        public string PoolId { get; set; } = string.Empty;
        
        public int ObjectId { get; set; } = -1;

        public string Name { get; set; } = string.Empty;

        public float UsedTime { get; set; } = 0f;

        public virtual void OnObjectInit(){}
        
        public virtual void OnObjectDeInit(){}
        
        public virtual void OnObjectSpawn(){}

        public virtual void OnObjectDespawn()
        {
            ObjectId = -1;
            UsedTime = 0f;
        }

        public virtual void OnObjectUpdate(float deltaTime)
        {
            UsedTime += deltaTime;
        }
    }
}
