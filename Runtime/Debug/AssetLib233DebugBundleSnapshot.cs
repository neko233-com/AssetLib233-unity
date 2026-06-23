using System;

namespace AssetLib233.Runtime
{
    [Serializable]
    public sealed class AssetLib233DebugBundleSnapshot
    {
        public string BundleName;
        public string FileName;
        public long FileSize;
        public int ReferenceCount;
        public bool IsLoaded;
        public bool IsDownloading;
        public float Progress;
    }
}
