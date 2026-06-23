namespace AssetLib233.Editor
{
    /// <summary>
    /// 腾讯云 CDN provider。
    /// 默认复用外部命令；适合微信小游戏 CDN、COS、刷新 URL / 目录刷新流水线。
    /// </summary>
    public sealed class AssetLib233TencentCloudCdnProvider : AssetLib233ExternalCommandCdnProvider
    {
        public AssetLib233TencentCloudCdnProvider()
            : base(EnumAssetLib233CdnProvider.TencentCloud, "TencentCloud")
        {
        }
    }
}
