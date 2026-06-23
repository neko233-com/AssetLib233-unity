using System;
using System.Collections.Generic;

namespace AssetLib233.Editor
{
    [Serializable]
    public sealed class AssetLib233EditorBuildReport
    {
        public string reportId = "";
        public string reportPath = "";
        public string profilePath = "";
        public string outputRoot = "";
        public string platformName = "";
        public string buildStartUtc = "";
        public string buildEndUtc = "";
        public bool success;
        public string error = "";
        public bool enableBundleCrypto;
        public string bundleCryptoPassword = "";
        public int groupCount;
        public int bundleCount;
        public int assetCount;
        public int encryptedBundleCount;
        public long totalBundleBytes;
        public List<AssetLib233EditorBuildReportGroup> groups =
            new List<AssetLib233EditorBuildReportGroup>(16);

        public void AddGroup(AssetLib233EditorBuildReportGroup group)
        {
            if (group == null)
            {
                return;
            }

            groups.Add(group);
        }

        public void RefreshTotals()
        {
            groupCount = groups.Count;
            bundleCount = 0;
            assetCount = 0;
            encryptedBundleCount = 0;
            totalBundleBytes = 0L;
            for (int i = 0; i < groups.Count; i++)
            {
                AssetLib233EditorBuildReportGroup group = groups[i];
                if (group == null)
                {
                    continue;
                }

                bundleCount += group.bundleCount;
                assetCount += group.assetCount;
                encryptedBundleCount += group.encryptedBundleCount;
                totalBundleBytes += group.totalBundleBytes;
            }
        }
    }
}
