namespace AssetLib233.Runtime
{
    /// <summary>
    /// 多 AssetGroup 下载操作。
    /// 当前负责统一进度与 LoadingSink；具体下载器由平台层注入，后续接微信/抖音/TapTap SDK。
    /// </summary>
    public sealed class AssetLib233MultiGroupDownloadOperation : AssetLib233Operation
    {
        private readonly AssetLib233GroupDownloadBatch _batch;
        private readonly IAssetLib233LoadingSink _loadingSink;
        private readonly AssetLib233UnifiedLoadingProgress _progress =
            new AssetLib233UnifiedLoadingProgress();

        public AssetLib233MultiGroupDownloadOperation(
            AssetLib233GroupDownloadBatch batch,
            IAssetLib233LoadingSink loadingSink)
        {
            _batch = batch;
            _loadingSink = loadingSink;
        }

        public AssetLib233UnifiedLoadingProgress UnifiedProgress
        {
            get { return _progress; }
        }

        protected override void OnStart()
        {
            if (_batch == null)
            {
                SetFailed("下载批次为空");
                return;
            }

            _batch.FillUnifiedProgress(_progress);
            _loadingSink?.OnAssetLib233StageChanged("MultiGroupDownload", "开始下载多个 AssetGroup");
            _loadingSink?.OnAssetLib233ProgressChanged(_progress.Progress, _progress.DownloadedBytes, _progress.TotalBytes);
            SetSucceed();
        }

        protected override void OnUpdate()
        {
        }
    }
}
