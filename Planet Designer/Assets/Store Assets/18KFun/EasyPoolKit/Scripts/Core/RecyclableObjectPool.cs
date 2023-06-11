
namespace EasyPoolKit
{
    public class RecyclableObjectPool : RecyclablePoolBase<IRecyclable>
    {
        public RecyclableObjectPool(RecyclablePoolConfig config) : base(config)
        {
        }

        protected override void OnObjectInit(IRecyclable usedObj)
        {
        }

        protected override void OnObjectEnqueue(IRecyclable usedObj)
        {
        }

        protected override void OnObjectDequeue(IRecyclable usedObj)
        {
        }

        protected override void OnObjectDeInit(IRecyclable usedObj)
        {
        }
    }
}