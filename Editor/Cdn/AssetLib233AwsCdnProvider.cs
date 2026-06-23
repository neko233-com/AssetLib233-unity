namespace AssetLib233.Editor
{
    /// <summary>
    /// AWS provider。
    /// 默认复用外部命令；典型链路为 S3 上传 + CloudFront invalidation。
    /// </summary>
    public sealed class AssetLib233AwsCdnProvider : AssetLib233ExternalCommandCdnProvider
    {
        public AssetLib233AwsCdnProvider()
            : base(EnumAssetLib233CdnProvider.Aws, "Aws")
        {
        }
    }
}
