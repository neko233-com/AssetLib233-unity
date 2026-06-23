namespace AssetLib233.Runtime
{
    /// <summary>
    /// 资源 GC 配置。
    /// AutoCollectIntervalSeconds 控制自动扫描间隔；UnusedGraceSeconds 控制引用归零后延迟释放，避免频繁进出界面造成抖动。
    /// </summary>
    public sealed class AssetLib233GcOptions
    {
        public EnumAssetLib233GcMode Mode = EnumAssetLib233GcMode.AutoAndManual;
        public float AutoCollectIntervalSeconds = 30f;
        public float UnusedGraceSeconds = 10f;
        public int MaxReleaseCountPerTick = 16;
    }
}
