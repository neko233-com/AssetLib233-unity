using AssetLib233.Runtime;

namespace AssetLib233.Editor
{
    /// <summary>
    /// 打包压缩策略接口。用户可按平台、AssetGroup、Collector 自定义压缩模式。
    /// </summary>
    public interface IAssetLib233BuildCompressionStrategy
    {
        EnumAssetLib233CompressionMode GetCompressionMode(
            string platformName,
            string assetGroupName,
            string collectorName);
    }
}
