using System.IO;

namespace AssetLib233.Runtime
{
    /// <summary>
    /// 废弃 AB 清理器。
    /// 只删除 AssetLib233 缓存索引内记录的文件，避免误删非框架文件。
    /// </summary>
    public static class AssetLib233ObsoleteBundleCleaner
    {
        public static void BuildCleanupPlan(
            AssetLib233CacheIndex cacheIndex,
            AssetManifest233 newManifest,
            AssetLib233ObsoleteBundleCleanupPlan plan)
        {
            if (cacheIndex == null || newManifest == null || plan == null)
            {
                return;
            }

            plan.Clear();
            System.Collections.Generic.List<AssetLib233CacheRecord> records =
                AssetLib233ListPool<AssetLib233CacheRecord>.Get();
            cacheIndex.GetRecordsNonAlloc(records);
            for (int i = 0; i < records.Count; i++)
            {
                AssetLib233CacheRecord record = records[i];
                if (record == null)
                {
                    continue;
                }

                if (!newManifest.TryGetBundleInfo(record.BundleName, out AssetBundleInfo233 bundleInfo))
                {
                    plan.Add(record);
                    continue;
                }

                if (bundleInfo.FileHash != record.FileHash)
                {
                    plan.Add(record);
                }
            }

            AssetLib233ListPool<AssetLib233CacheRecord>.Release(records);
        }

        public static int DeleteFiles(AssetLib233ObsoleteBundleCleanupPlan plan)
        {
            if (plan == null)
            {
                return 0;
            }

            int deleteCount = 0;
            for (int i = 0; i < plan.ObsoleteRecords.Count; i++)
            {
                AssetLib233CacheRecord record = plan.ObsoleteRecords[i];
                if (record == null || string.IsNullOrWhiteSpace(record.LocalPath))
                {
                    continue;
                }

                if (!File.Exists(record.LocalPath))
                {
                    continue;
                }

                File.Delete(record.LocalPath);
                deleteCount++;
            }

            return deleteCount;
        }
    }
}
