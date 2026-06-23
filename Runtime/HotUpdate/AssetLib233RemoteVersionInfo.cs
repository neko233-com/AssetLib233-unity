namespace AssetLib233.Runtime
{
    /// <summary>
    /// 远端版本信息。热更服务返回该结构后，AssetLib233 决定是否拉取新 Manifest 和 Bundle。
    /// </summary>
    public sealed class AssetLib233RemoteVersionInfo
    {
        public string GroupName;
        public string PackageVersion;
        public string ManifestFileName;
        public string ManifestHash;
        public long ManifestSize;
    }
}
