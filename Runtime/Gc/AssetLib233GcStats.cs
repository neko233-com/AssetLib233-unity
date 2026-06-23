namespace AssetLib233.Runtime
{
    /// <summary>
    /// GC 统计数据。调试窗口和日志可直接展示。
    /// </summary>
    public sealed class AssetLib233GcStats
    {
        public int TrackedAssetCount;
        public int ReleasedAssetCount;
        public int AliveReferenceCount;
    }
}
