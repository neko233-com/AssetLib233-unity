namespace AssetLib233.Editor
{
    /// <summary>
    /// 外部命令 CDN provider。
    /// 适合接入 Go 二进制、C# 控制台、官方 CLI、SSH rsync、公司内部发布工具。
    /// </summary>
    public class AssetLib233ExternalCommandCdnProvider : IAssetLib233CdnProvider
    {
        private readonly EnumAssetLib233CdnProvider _providerType;
        private readonly string _displayName;

        public AssetLib233ExternalCommandCdnProvider(
            EnumAssetLib233CdnProvider providerType,
            string displayName)
        {
            _providerType = providerType;
            _displayName = displayName;
        }

        public EnumAssetLib233CdnProvider ProviderType
        {
            get { return _providerType; }
        }

        public string DisplayName
        {
            get { return _displayName; }
        }

        public virtual bool Upload(AssetLib233CdnProviderContext context)
        {
            return context.RunUploadTool(DisplayName + "_UploadCDN");
        }

        public virtual bool Refresh(AssetLib233CdnProviderContext context)
        {
            return context.RunRefreshTool(DisplayName + "_RefreshCDN");
        }
    }
}
