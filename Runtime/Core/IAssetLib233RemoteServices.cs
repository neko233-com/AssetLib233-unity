namespace AssetLib233.Runtime
{
    /// <summary>
    /// 远端资源地址查询接口。平台插件和下载器只依赖这个接口，不绑定任何第三方资源库。
    /// </summary>
    public interface IAssetLib233RemoteServices
    {
        string GetRemoteMainUrl(string fileName);
        string GetRemoteFallbackUrl(string fileName);
    }
}
