namespace AssetLib233.Editor
{
    /// <summary>
    /// CDN 服务商类型。
    /// 默认火山引擎中国，原因是国内小游戏 CDN 场景更常见，延迟和刷新链路更贴近微信小游戏。
    /// </summary>
    public enum EnumAssetLib233CdnProvider
    {
        VolcengineChina = 0,
        Aliyun = 1,
        TencentCloud = 2,
        Aws = 3,
        Custom = 1000
    }
}
