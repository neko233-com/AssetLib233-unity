namespace AssetLib233.Runtime
{
    /// <summary>
    /// AssetGroup 运行时状态。
    /// </summary>
    public enum EnumAssetLib233PackageStatus
    {
        None = 0,
        Initialized = 1,
        ManifestLoaded = 2,
        Downloading = 3,
        Ready = 4,
        Failed = 5
    }
}
