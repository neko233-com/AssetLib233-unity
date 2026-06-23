namespace AssetLib233.Runtime
{
    /// <summary>
    /// Manifest 加载接口。支持本地缓存、远端下载、小游戏文件系统、编辑器模拟缓存。
    /// </summary>
    public interface IAssetLib233ManifestLoader
    {
        bool TryLoadManifest(AssetLib233RemoteVersionInfo versionInfo, out AssetManifest233 manifest, out string error);
    }
}
