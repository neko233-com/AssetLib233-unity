namespace AssetLib233.Editor
{
    public sealed class AssetLib233EnUsTextStrategy : IAssetLib233TextStrategy
    {
        public EnumAssetLib233Language Language
        {
            get { return EnumAssetLib233Language.EnUs; }
        }

        public string GetText(string key)
        {
            switch (key)
            {
                case "ValidationStart":
                    return "AssetLib233 agent-first validation started";
                case "ValidationDone":
                    return "AssetLib233 agent-first validation finished";
                case "ConfigMissing":
                    return "Missing config";
                case "BuildProfileMissing":
                    return "BuildProfile not found";
                case "CollectorRootMissing":
                    return "Collector root does not exist";
                case "NoAssetCollected":
                    return "No asset collected";
                case "DuplicateAddress":
                    return "Duplicate asset address";
                case "AssetLoadFailed":
                    return "Editor asset load failed";
                case "CdnGoToolMissing":
                    return "CDN Go tool missing";
                case "CdnGoConfigMissing":
                    return "CDN Go config missing";
                case "CdnGoRun":
                    return "Run CDN Go tool";
                case "Skipped":
                    return "Not configured, skipped";
                default:
                    return key;
            }
        }
    }
}
