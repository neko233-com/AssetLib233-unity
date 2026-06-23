namespace AssetLib233.Runtime
{
    /// <summary>
    /// 版本服务接口。HTTP、小游戏 SDK、内置离线版本都可以实现该接口。
    /// </summary>
    public interface IAssetLib233VersionService
    {
        bool TryGetRemoteVersion(string groupName, out AssetLib233RemoteVersionInfo versionInfo, out string error);
    }
}
