using AssetLib233.Runtime;

namespace AssetLib233.Editor
{
    /// <summary>
    /// 打包规则接口。比固定枚举更开放，允许用户把目录、标签、资源类型映射到任意 bundle 名。
    /// </summary>
    public interface IAssetLib233BuildPackRule
    {
        string GetBundleName(AssetGroup233 group, AssetCollector233 collector, string assetPath);
    }
}
