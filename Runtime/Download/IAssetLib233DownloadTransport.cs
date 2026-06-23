namespace AssetLib233.Runtime
{
    /// <summary>
    /// 下载传输层接口。
    /// HTTP、微信小游戏、抖音小游戏、TapTap 小游戏都通过该接口接入；框架调度器不依赖 SDK 类型。
    /// </summary>
    public interface IAssetLib233DownloadTransport
    {
        bool CanStartRequest(AssetLib233DownloadRequest request);
        void StartRequest(AssetLib233DownloadRequest request);
        bool IsRequestDone(AssetLib233DownloadRequest request);
        bool IsRequestSuccess(AssetLib233DownloadRequest request);
        long GetDownloadedBytes(AssetLib233DownloadRequest request);
        string GetError(AssetLib233DownloadRequest request);
    }
}
