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
            AssetLib233GcStats stats = new AssetLib233GcStats();
            AssetLib233.Instance.AssetGcService.FillStats(stats);
            snapshot.TrackedAssetCount = stats.TrackedAssetCount;
            snapshot.AliveReferenceCount = stats.AliveReferenceCount;
            return snapshot;
        }
    }
}
