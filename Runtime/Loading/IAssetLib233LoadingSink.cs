namespace AssetLib233.Runtime
{
    /// <summary>
    /// Loading 进度接收器。
    /// 业务层可以以后实现该接口接 UI；当前 AssetLib233 只调用接口，不依赖任何 UI 系统。
    /// </summary>
    public interface IAssetLib233LoadingSink
    {
        void OnAssetLib233StageChanged(string stageName, string message);
        void OnAssetLib233ProgressChanged(float progress, long downloadedBytes, long totalBytes);
        void OnAssetLib233Failed(string stageName, string error);
    }
}
