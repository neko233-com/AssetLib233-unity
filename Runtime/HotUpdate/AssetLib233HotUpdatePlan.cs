using System.Collections.Generic;

namespace AssetLib233.Runtime
{
    /// <summary>
    /// 热更计划。包含本次需要下载的 Manifest 与 Bundle 列表。
    /// </summary>
    public sealed class AssetLib233HotUpdatePlan
    {
        private readonly List<AssetLib233DownloadRequest> _downloadRequests = new List<AssetLib233DownloadRequest>(256);

        public string GroupName;
        public string FromVersion;
        public string ToVersion;
        public bool NeedUpdateManifest;

        public IReadOnlyList<AssetLib233DownloadRequest> DownloadRequests
        {
            get { return _downloadRequests; }
        }

        public void AddDownloadRequest(AssetLib233DownloadRequest request)
        {
            if (request == null)
            {
                return;
            }

            _downloadRequests.Add(request);
        }

        public void Clear()
        {
            GroupName = string.Empty;
            FromVersion = string.Empty;
            ToVersion = string.Empty;
            NeedUpdateManifest = false;
            _downloadRequests.Clear();
        }
    }
}
