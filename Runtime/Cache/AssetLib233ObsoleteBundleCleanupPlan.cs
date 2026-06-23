using System.Collections.Generic;

namespace AssetLib233.Runtime
{
    /// <summary>
    /// 废弃 Bundle 清理计划。
    /// 新版本 Manifest 不再引用的缓存 AB 会进入该计划，由调用方确认后删除。
    /// </summary>
    public sealed class AssetLib233ObsoleteBundleCleanupPlan
    {
        private readonly List<AssetLib233CacheRecord> _obsoleteRecords =
            new List<AssetLib233CacheRecord>(256);

        public IReadOnlyList<AssetLib233CacheRecord> ObsoleteRecords
        {
            get { return _obsoleteRecords; }
        }

        public void Clear()
        {
            _obsoleteRecords.Clear();
        }

        public void Add(AssetLib233CacheRecord record)
        {
            if (record == null)
            {
                return;
            }

            _obsoleteRecords.Add(record);
        }
    }
}
