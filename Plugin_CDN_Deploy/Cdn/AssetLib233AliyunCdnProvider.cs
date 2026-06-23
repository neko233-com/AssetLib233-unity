namespace AssetLib233.Editor
{
    /// <summary>
    /// 阿里云 CDN provider。
    /// 默认复用外部命令；项目可注册自定义 provider 接官方 SDK 或公司发布系统。
    /// </summary>
    public sealed class AssetLib233AliyunCdnProvider : AssetLib233ExternalCommandCdnProvider
    {
        public AssetLib233AliyunCdnProvider()
            : base(EnumAssetLib233CdnProvider.Aliyun, "Aliyun")
        {
        }
    }
}
