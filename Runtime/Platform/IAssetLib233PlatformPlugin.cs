namespace AssetLib233.Runtime
{
    /// <summary>
    /// 平台插件接口。只暴露 AssetLib233 自身能力，不依赖第三方资源系统。
    /// </summary>
    public interface IAssetLib233PlatformPlugin
    {
        EnumAssetLib233RuntimePlatform RuntimePlatform { get; }

        void BeforeInitializePackage(AssetLib233PackageConfig config);

        string GetPersistentRootPath(AssetLib233PackageConfig config);

        EnumAssetLib233LoadMethod GetPreferredLoadMethod(AssetLib233PackageConfig config);

        IAssetLib233DownloadTransport CreateDownloadTransport(AssetLib233PackageConfig config);
    }
}
