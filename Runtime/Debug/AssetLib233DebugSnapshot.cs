using System;
using System.Collections.Generic;

namespace AssetLib233.Runtime
{
    [Serializable]
    public sealed class AssetLib233DebugSnapshot
    {
        public string CaptureTime;
        public EnumAssetLib233RuntimePlatform RuntimePlatform;
        public int DownloadConcurrency;
        public int TrackedAssetCount;
        public int AliveReferenceCount;
        public List<AssetLib233DebugGroupSnapshot> Groups = new List<AssetLib233DebugGroupSnapshot>(8);
    }
}
