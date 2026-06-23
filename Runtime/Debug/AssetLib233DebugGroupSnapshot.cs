using System;
using System.Collections.Generic;

namespace AssetLib233.Runtime
{
    [Serializable]
    public sealed class AssetLib233DebugGroupSnapshot
    {
        public string GroupName;
        public int AssetCount;
        public int BundleCount;
        public long TotalBundleSize;
        public List<AssetLib233DebugBundleSnapshot> Bundles = new List<AssetLib233DebugBundleSnapshot>(128);
    }
}
