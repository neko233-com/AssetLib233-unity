namespace AssetLib233.Runtime
{
    /// <summary>
    /// 下载模式。
    /// SingleFile 适合首包关键小文件；ParallelBatch 适合登录后多个 AssetGroup 聚合下载。
    /// </summary>
    public enum EnumAssetLib233DownloadMode
    {
        SingleFile = 0,
        ParallelBatch = 1
    }
}
