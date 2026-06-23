namespace AssetLib233.Runtime
{
    public sealed class AssetLib233DownloadRequest
    {
        public string GroupName;
        public AssetBundleInfo233 BundleInfo;
        public string MainUrl;
        public string FallbackUrl;
        public int RetryCount;

        public string CurrentFileName
        {
            get
            {
                if (BundleInfo == null)
                {
                    return string.Empty;
                }

                return BundleInfo.FileName;
            }
        }
    }
}
