namespace AssetLib233.Runtime
{
    /// <summary>
    /// 编辑器真机下载模拟配置。
    /// 用于 Editor 内调试远端版本、Manifest、Bundle 下载和缓存，不驱动业务 Loading。
    /// </summary>
    public sealed class AssetLib233EditorRemoteSimulationConfig
    {
        public bool Enabled;
        public bool AutoDownloadManifest = true;
        public bool AutoDownloadBundles = true;
        public string CacheRoot;
    }
}
