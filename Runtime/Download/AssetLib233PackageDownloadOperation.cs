using System.Collections.Generic;

namespace AssetLib233.Runtime
{
    /// <summary>
    /// 单个 AssetGroup 下载操作。Manifest 已就绪后，按 tag 或全量生成下载计划。
    /// </summary>
    public sealed class AssetLib233PackageDownloadOperation : AssetLib233Operation
    {
        private readonly string _groupName;
        private readonly string _tag;
        private readonly AssetLib233DownloadScheduler _scheduler = new AssetLib233DownloadScheduler();
        private readonly List<AssetLib233DownloadRequest> _requests = new List<AssetLib233DownloadRequest>(512);
        private bool _isSchedulerStarted;

        public AssetLib233PackageDownloadOperation(string groupName, string tag)
        {
            _groupName = AssetLib233NameUtility.NormalizePackageName(groupName);
            _tag = tag;
        }

        public AssetLib233UnifiedLoadingProgress LoadingProgress
        {
            get { return _scheduler.Progress; }
        }

        protected override void OnStart()
        {
            AssetPackage233 assetPackage = AssetLib233.Instance.GetOrCreateGroup(_groupName);
            if (assetPackage.Manifest == null)
            {
                SetFailed("下载前 Manifest 未就绪. group = " + _groupName);
                return;
            }

            _requests.Clear();
            if (string.IsNullOrEmpty(_tag))
            {
                assetPackage.BuildAllDownloadRequestsNonAlloc(_requests);
            }
            else
            {
                AssetLib233.Instance.CreateHotUpdatePlanByTagNonAlloc(_groupName, _tag, _requests);
            }

            IAssetLib233PlatformPlugin plugin =
                AssetLib233PluginRegistry.GetPlugin(AssetLib233PlatformDetector.GetRuntimePlatform());
            _scheduler.SetTransport(plugin.CreateDownloadTransport(assetPackage.Config));
            _scheduler.Options.MaxConcurrentCount = AssetLib233RuntimeOptions.DownloadConcurrency;
            _scheduler.AddRequests(_requests);
            _scheduler.Start();
            _isSchedulerStarted = true;
            Progress = 0f;
            AssetLib233RuntimeDiagnostic.RecordEvent(
                "package-download-start group=" +
                _groupName +
                " tag=" +
                (_tag ?? string.Empty) +
                " files=" +
                _requests.Count);
        }

        protected override void OnUpdate()
        {
            if (!_isSchedulerStarted)
            {
                return;
            }

            _scheduler.Tick();
            Progress = _scheduler.Progress.Progress;
            if (!_scheduler.IsDone())
            {
                return;
            }

            if (!string.IsNullOrEmpty(_scheduler.LastError))
            {
                SetFailed(_scheduler.LastError);
                AssetLib233RuntimeDiagnostic.RecordEvent("package-download-fail group=" + _groupName + " error=" + _scheduler.LastError);
                return;
            }

            SetSucceed();
            AssetLib233RuntimeDiagnostic.RecordEvent("package-download-ok group=" + _groupName);
        }
    }
}
