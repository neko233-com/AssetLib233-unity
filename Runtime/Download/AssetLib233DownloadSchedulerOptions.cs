namespace AssetLib233.Runtime
{
    /// <summary>
    /// 下载调度配置。并发上限默认 10，贴合微信小游戏弱网与文件系统调度口径。
    /// </summary>
    public sealed class AssetLib233DownloadSchedulerOptions
    {
        public EnumAssetLib233DownloadMode DownloadMode = EnumAssetLib233DownloadMode.ParallelBatch;
        public int MaxConcurrentCount = AssetLib233DownloadPolicy.MaxDownloadConcurrency;
        public int FailedRetryCount = 3;
        public bool RequiredGroupFirst = true;
    }
}
