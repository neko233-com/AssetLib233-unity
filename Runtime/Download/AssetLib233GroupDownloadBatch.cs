using System.Collections.Generic;

namespace AssetLib233.Runtime
{
    /// <summary>
    /// 多 AssetGroup 下载批次。
    /// 用于登录后 default / story / voice / cg 等多组同时下载，并对 Loading 统一汇总。
    /// </summary>
    public sealed class AssetLib233GroupDownloadBatch
    {
        private readonly List<AssetLib233DownloadRequest> _requests =
            new List<AssetLib233DownloadRequest>(512);

        public IReadOnlyList<AssetLib233DownloadRequest> Requests
        {
            get { return _requests; }
        }

        public void Clear()
        {
            _requests.Clear();
        }

        public void AddRequests(IReadOnlyList<AssetLib233DownloadRequest> requests)
        {
            if (requests == null)
            {
                return;
            }

            for (int i = 0; i < requests.Count; i++)
            {
                if (requests[i] != null)
                {
                    _requests.Add(requests[i]);
                }
            }
        }

        public void FillUnifiedProgress(AssetLib233UnifiedLoadingProgress progress)
        {
            if (progress == null)
            {
                return;
            }

            progress.Reset();
            progress.TotalFileCount = _requests.Count;
            for (int i = 0; i < _requests.Count; i++)
            {
                AssetLib233DownloadRequest request = _requests[i];
                if (request.BundleInfo == null)
                {
                    continue;
                }

                progress.TotalBytes += request.BundleInfo.FileSize;
            }
        }
    }
}
