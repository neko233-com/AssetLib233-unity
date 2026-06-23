namespace AssetLib233.Runtime
{
    public static class AssetLib233DownloadPolicy
    {
        public const int MaxDownloadConcurrency = 10;

        public static int GetDownloadConcurrency(EnumAssetLib233RuntimePlatform runtimePlatform)
        {
            return MaxDownloadConcurrency;
        }
    }
}
