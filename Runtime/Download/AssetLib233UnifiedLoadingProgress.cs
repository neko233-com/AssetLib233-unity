namespace AssetLib233.Runtime
{
    /// <summary>
    /// 多 AssetGroup 统一 Loading 进度。
    /// 所有组的文件数量和字节数聚合成一条进度，业务 UI 只需要显示这一条。
    /// </summary>
    public sealed class AssetLib233UnifiedLoadingProgress
    {
        public int TotalGroupCount;
        public int FinishedGroupCount;
        public int TotalFileCount;
        public int FinishedFileCount;
        public long TotalBytes;
        public long DownloadedBytes;
        public string CurrentGroupName;
        public string CurrentFileName;

        public float Progress
        {
            get
            {
                if (TotalBytes > 0)
                {
                    return (float)DownloadedBytes / TotalBytes;
                }

                if (TotalFileCount > 0)
                {
                    return (float)FinishedFileCount / TotalFileCount;
                }

                if (TotalGroupCount > 0)
                {
                    return (float)FinishedGroupCount / TotalGroupCount;
                }

                return 0f;
            }
        }

        public void Reset()
        {
            TotalGroupCount = 0;
            FinishedGroupCount = 0;
            TotalFileCount = 0;
            FinishedFileCount = 0;
            TotalBytes = 0;
            DownloadedBytes = 0;
            CurrentGroupName = string.Empty;
            CurrentFileName = string.Empty;
        }
    }
}
