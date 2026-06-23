using System.Collections.Generic;

namespace AssetLib233.Runtime
{
    public interface IAssetLib233Downloader
    {
        int MaxConcurrency { get; }
        AssetLib233DownloadProgress Progress { get; }
        void AddRequests(IReadOnlyList<AssetLib233DownloadRequest> requests);
        void Start();
        void Tick();
    }
}
