
namespace EasyPoolKit
{
    /// <summary>
    /// When user try to get a object but the pool reaches the max count
    /// Default => create new objects and return
    /// RejectNull => don't create new objects and return null
    /// RecycleOldest => force recycle the oldest one and return
    /// </summary>
    public enum PoolReachMaxLimitType
    {
        Default, //No limit
        RejectNull,
        RecycleOldest,
    }

    /// <summary>
    /// When a object give back to the pool, if the pool size is larger than the limit
    /// Default => do nothing
    /// DestroyToLimit => destroy the pooled object to make the pool size equals to the limit
    /// </summary>
    public enum PoolDespawnDestroyType
    {
        Default, //Not destroy
        DestroyToLimit,
    }

    /// <summary>
    /// When clear the pool
    /// Default => clear all
    /// ClearToLimit => clear and make the pool resize to limit count
    /// </summary>
    public enum PoolClearType
    {
        Default, //Clear all
        ClearToLimit,
    }
}
