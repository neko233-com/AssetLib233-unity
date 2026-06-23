namespace AssetLib233.Editor
{
    public sealed class AssetLib233ZhCnTextStrategy : IAssetLib233TextStrategy
    {
        public EnumAssetLib233Language Language
        {
            get { return EnumAssetLib233Language.ZhCn; }
        }

        public string GetText(string key)
        {
            switch (key)
            {
                case "ValidationStart":
                    return "AssetLib233 agent-first 验证开始";
                case "ValidationDone":
                    return "AssetLib233 agent-first 验证完成";
                case "ConfigMissing":
                    return "缺少配置";
                case "BuildProfileMissing":
                    return "BuildProfile 不存在";
                case "CollectorRootMissing":
                    return "Collector 根目录不存在";
                case "NoAssetCollected":
                    return "没有收集到资源";
                case "DuplicateAddress":
                    return "资源地址重复";
                case "AssetLoadFailed":
                    return "Editor 资源加载失败";
                case "CdnGoToolMissing":
                    return "CDN Go 工具不存在";
                case "CdnGoConfigMissing":
                    return "CDN Go 配置不存在";
                case "CdnGoRun":
                    return "运行 CDN Go 工具";
                case "Skipped":
                    return "未配置，跳过";
                default:
                    return key;
            }
        }
    }
}
