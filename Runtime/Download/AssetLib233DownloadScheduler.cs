using System.Collections.Generic;

namespace AssetLib233.Runtime
{
    /// <summary>
    /// 主线程下载调度器。
    /// 支持单文件下载和批量并发下载，所有 AssetGroup 汇总到一个 UnifiedLoadingProgress。
    /// </summary>
    public sealed class AssetLib233DownloadScheduler
    {
        private readonly List<AssetLib233DownloadRequest> _pendingRequests = new List<AssetLib233DownloadRequest>(512);
        private readonly List<AssetLib233DownloadRequest> _runningRequests = new List<AssetLib233DownloadRequest>(16);
        private readonly List<AssetLib233DownloadRequest> _finishedRequests = new List<AssetLib233DownloadRequest>(512);
        private readonly AssetLib233UnifiedLoadingProgress _progress = new AssetLib233UnifiedLoadingProgress();
        private readonly AssetLib233DownloadSchedulerOptions _options = new AssetLib233DownloadSchedulerOptions();
        private IAssetLib233DownloadTransport _transport;
        private bool _isStarted;
        private string _lastError = string.Empty;

        public AssetLib233UnifiedLoadingProgress Progress
        {
            get { return _progress; }
        }

        public string LastError
        {
            get { return _lastError; }
        }

        public AssetLib233DownloadSchedulerOptions Options
        {
            get { return _options; }
        }

        public void SetTransport(IAssetLib233DownloadTransport transport)
        {
            _transport = transport;
        }

        public void AddRequests(IReadOnlyList<AssetLib233DownloadRequest> requests)
        {
            if (requests == null)
            {
                return;
            }

            for (int i = 0; i < requests.Count; i++)
            {
                AssetLib233DownloadRequest request = requests[i];
                if (request == null)
                {
                    continue;
                }

                _pendingRequests.Add(request);
                if (request.BundleInfo != null)
                {
                    _progress.TotalBytes += request.BundleInfo.FileSize;
                }
            }

            _progress.TotalFileCount = _pendingRequests.Count + _runningRequests.Count + _finishedRequests.Count;
        }

        public void Start()
        {
            _isStarted = true;
        }

        public bool IsDone()
        {
            return _isStarted &&
                   _pendingRequests.Count == 0 &&
                   _runningRequests.Count == 0;
        }

        public void Tick()
        {
            if (!_isStarted || _transport == null)
            {
                return;
            }

            UpdateRunningRequests();
            StartPendingRequests();
            UpdateProgressBytes();
        }

        private void StartPendingRequests()
        {
            int maxConcurrentCount = _options.DownloadMode == EnumAssetLib233DownloadMode.SingleFile
                ? 1
                : _options.MaxConcurrentCount;

            while (_runningRequests.Count < maxConcurrentCount && _pendingRequests.Count > 0)
            {
                AssetLib233DownloadRequest request = _pendingRequests[0];
                _pendingRequests.RemoveAt(0);
                if (!_transport.CanStartRequest(request))
                {
                    _lastError = "传输层拒绝下载: " + request.CurrentFileName;
                    _finishedRequests.Add(request);
                    continue;
                }

                _transport.StartRequest(request);
                _runningRequests.Add(request);
                _progress.CurrentGroupName = request.GroupName;
                _progress.CurrentFileName = request.CurrentFileName;
            }
        }

        private void UpdateRunningRequests()
        {
            for (int i = _runningRequests.Count - 1; i >= 0; i--)
            {
                AssetLib233DownloadRequest request = _runningRequests[i];
                if (!_transport.IsRequestDone(request))
                {
                    continue;
                }

                if (!_transport.IsRequestSuccess(request))
                {
                    request.RetryCount--;
                    if (request.RetryCount > 0)
                    {
                        _pendingRequests.Add(request);
                    }
                    else
                    {
                        _lastError = _transport.GetError(request);
                        _finishedRequests.Add(request);
                    }
                }
                else
                {
                    _finishedRequests.Add(request);
                    _progress.FinishedFileCount++;
                }

                _runningRequests.RemoveAt(i);
            }
        }

        private void UpdateProgressBytes()
        {
            long downloadedBytes = 0;
            for (int i = 0; i < _finishedRequests.Count; i++)
            {
                AssetLib233DownloadRequest request = _finishedRequests[i];
                if (request.BundleInfo != null)
                {
                    downloadedBytes += request.BundleInfo.FileSize;
                }
            }

            for (int i = 0; i < _runningRequests.Count; i++)
            {
                downloadedBytes += _transport.GetDownloadedBytes(_runningRequests[i]);
            }

            _progress.DownloadedBytes = downloadedBytes;
        }
    }
}
