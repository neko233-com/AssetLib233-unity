namespace AssetLib233.Runtime
{
    public sealed class AssetLib233DownloadProgress
    {
        public int TotalCount;
        public int FinishedCount;
        public long TotalBytes;
        public long DownloadedBytes;
        public string CurrentFileName;

        public float Progress
        {
            get
            {
                if (TotalBytes <= 0)
                {
                    return TotalCount > 0 ? (float)FinishedCount / TotalCount : 0f;
                }

                return (float)DownloadedBytes / TotalBytes;
            }
        }
    }
}
