using System;

namespace AssetLib233.Runtime
{
    public static class AssetLib233DebugService
    {
        public static AssetLib233DebugSnapshot CaptureSnapshot()
        {
            EnumAssetLib233RuntimePlatform runtimePlatform = AssetLib233PlatformDetector.GetRuntimePlatform();
            AssetLib233DebugSnapshot snapshot = new AssetLib233DebugSnapshot();
            snapshot.CaptureTime = DateTime.UtcNow.ToString("O");
            snapshot.RuntimePlatform = runtimePlatform;
            snapshot.DownloadConcurrency = AssetLib233DownloadPolicy.GetDownloadConcurrency(runtimePlatform);
            return snapshot;
        }
    }
}
