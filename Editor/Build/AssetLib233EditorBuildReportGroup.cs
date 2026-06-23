using System;

namespace AssetLib233.Editor
{
    [Serializable]
    public sealed class AssetLib233EditorBuildReportGroup
    {
        public string groupName = "";
        public string outputRoot = "";
        public string packageVersion = "";
        public string compressionMode = "";
        public bool success;
        public string error = "";
        public int bundleCount;
        public int assetCount;
        public int encryptedBundleCount;
        public long totalBundleBytes;
    }
}
