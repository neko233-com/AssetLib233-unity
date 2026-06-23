using System.Collections.Generic;

namespace AssetLib233.Runtime
{
    public sealed class AssetLib233CacheIndex
    {
        private readonly Dictionary<string, AssetLib233CacheRecord> _records =
            new Dictionary<string, AssetLib233CacheRecord>(512);

        public int Count
        {
            get { return _records.Count; }
        }

        public bool TryGetRecord(string bundleName, out AssetLib233CacheRecord record)
        {
            return _records.TryGetValue(bundleName, out record);
        }

        public void SetRecord(AssetLib233CacheRecord record)
        {
            if (record == null || string.IsNullOrEmpty(record.BundleName))
            {
                return;
            }

            _records[record.BundleName] = record;
        }

        public void GetRecordsNonAlloc(List<AssetLib233CacheRecord> results)
        {
            if (results == null)
            {
                return;
            }

            results.Clear();
            Dictionary<string, AssetLib233CacheRecord>.Enumerator enumerator = _records.GetEnumerator();
            while (enumerator.MoveNext())
            {
                results.Add(enumerator.Current.Value);
            }
        }

        public void RemoveRecord(string bundleName)
        {
            if (string.IsNullOrEmpty(bundleName))
            {
                return;
            }

            _records.Remove(bundleName);
        }

        public void Clear()
        {
            _records.Clear();
        }
    }
}
