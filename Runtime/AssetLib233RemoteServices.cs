namespace AssetLib233.Runtime
{
    /// <summary>
    /// 默认远端 URL 查询器。只做字符串拼接，避免小游戏 / HybridCLR 运行时兼容风险。
    /// </summary>
    public sealed class AssetLib233RemoteServices : IAssetLib233RemoteServices
    {
        private readonly string _defaultHostServer;
        private readonly string _fallbackHostServer;

        public AssetLib233RemoteServices(string defaultHostServer, string fallbackHostServer)
        {
            _defaultHostServer = AssetLib233NameUtility.NormalizeHost(defaultHostServer);
            _fallbackHostServer = AssetLib233NameUtility.NormalizeHost(fallbackHostServer);
        }

        string IAssetLib233RemoteServices.GetRemoteMainUrl(string fileName)
        {
            return _defaultHostServer + "/" + fileName;
        }

        string IAssetLib233RemoteServices.GetRemoteFallbackUrl(string fileName)
        {
            return _fallbackHostServer + "/" + fileName;
        }
    }
}
