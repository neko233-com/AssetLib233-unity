using System.Collections.Generic;
using UnityEngine;

namespace AssetLib233.Runtime
{
    /// <summary>
    /// AssetLib233 资源 GC 服务。
    /// 只管理 AssetLib233 跟踪到的资源对象；不直接调用 Resources.UnloadUnusedAssets，避免小游戏 / WebGL 运行时出现大峰值。
    /// </summary>
    public sealed class AssetLib233AssetGcService
    {
        private readonly Dictionary<string, AssetLib233AssetRecord> _records =
            new Dictionary<string, AssetLib233AssetRecord>(1024);

        private readonly List<string> _releaseKeyCache = new List<string>(64);
        private readonly AssetLib233GcOptions _options = new AssetLib233GcOptions();
        private float _lastAutoCollectTime;

        public AssetLib233GcOptions Options
        {
            get { return _options; }
        }

        public void Retain(string groupName, string address, Object assetObject)
        {
            string key = BuildKey(groupName, address);
            if (!_records.TryGetValue(key, out AssetLib233AssetRecord record))
            {
                record = new AssetLib233AssetRecord();
                record.GroupName = groupName;
                record.Address = address;
                record.AssetObject = assetObject;
                _records.Add(key, record);
            }

            record.ReferenceCount++;
            record.MarkedForRelease = false;
        }

        public void Release(string groupName, string address)
        {
            string key = BuildKey(groupName, address);
            if (!_records.TryGetValue(key, out AssetLib233AssetRecord record))
            {
                return;
            }

            if (record.ReferenceCount > 0)
            {
                record.ReferenceCount--;
            }

            if (record.ReferenceCount == 0)
            {
                record.LastZeroReferenceTime = Time.realtimeSinceStartup;
            }
        }

        public void Tick()
        {
            if (_options.Mode == EnumAssetLib233GcMode.Manual)
            {
                return;
            }

            float now = Time.realtimeSinceStartup;
            if (now - _lastAutoCollectTime < _options.AutoCollectIntervalSeconds)
            {
                return;
            }

            _lastAutoCollectTime = now;
            Collect(false);
        }

        public void Collect(bool force)
        {
            _releaseKeyCache.Clear();
            float now = Time.realtimeSinceStartup;
            int releasedCount = 0;

            Dictionary<string, AssetLib233AssetRecord>.Enumerator enumerator = _records.GetEnumerator();
            while (enumerator.MoveNext())
            {
                AssetLib233AssetRecord record = enumerator.Current.Value;
                if (record.ReferenceCount > 0)
                {
                    continue;
                }

                if (!force && now - record.LastZeroReferenceTime < _options.UnusedGraceSeconds)
                {
                    continue;
                }

                _releaseKeyCache.Add(enumerator.Current.Key);
                releasedCount++;
                if (releasedCount >= _options.MaxReleaseCountPerTick)
                {
                    break;
                }
            }

            for (int i = 0; i < _releaseKeyCache.Count; i++)
            {
                _records.Remove(_releaseKeyCache[i]);
            }
        }

        public void FillStats(AssetLib233GcStats stats)
        {
            if (stats == null)
            {
                return;
            }

            stats.TrackedAssetCount = _records.Count;
            stats.AliveReferenceCount = 0;

            Dictionary<string, AssetLib233AssetRecord>.Enumerator enumerator = _records.GetEnumerator();
            while (enumerator.MoveNext())
            {
                stats.AliveReferenceCount += enumerator.Current.Value.ReferenceCount;
            }
        }

        private static string BuildKey(string groupName, string address)
        {
            return groupName + "::" + address;
        }
    }
}
