namespace AssetLib233.Runtime
{
    /// <summary>
    /// 运行时全局选项。全部是主线程配置，不引入线程 / 锁。
    /// </summary>
    public static class AssetLib233RuntimeOptions
    {
        public static long OperationTimeSliceMs = AssetLib233Constants.WebGLOperationTimeSliceMs;
        public static int DownloadConcurrency = AssetLib233Constants.DefaultDownloadConcurrency;
    }
}
