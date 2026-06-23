namespace AssetLib233.Editor
{
    /// <summary>
    /// CDN 发布策略接口。
    /// Editor 层只定义策略，不强依赖任何云 SDK；团队可用 Go / C# / 官方 CLI / SSH 脚本接入。
    /// </summary>
    public interface IAssetLib233CdnProvider
    {
        EnumAssetLib233CdnProvider ProviderType { get; }

        string DisplayName { get; }

        bool Upload(AssetLib233CdnProviderContext context);

        bool Refresh(AssetLib233CdnProviderContext context);
    }
}
