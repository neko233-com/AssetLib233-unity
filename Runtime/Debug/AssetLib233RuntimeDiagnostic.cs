using System.Text;

namespace AssetLib233.Runtime
{
    /// <summary>
    /// 真机可复制诊断串。用于排查“Editor 正常、真机 AB 下载/加载为空”这类问题。
    /// </summary>
    public static class AssetLib233RuntimeDiagnostic
    {
        private const int EventCapacity = 80;
        private static readonly string[] _events = new string[EventCapacity];
        private static int _nextEventIndex;
        private static int _eventCount;
        private static readonly StringBuilder _builder = new StringBuilder(8192);

        public static void RecordEvent(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            _events[_nextEventIndex] = System.DateTime.UtcNow.ToString("HH:mm:ss.fff") + " " + message;
            _nextEventIndex++;
            if (_nextEventIndex >= EventCapacity)
            {
                _nextEventIndex = 0;
            }

            if (_eventCount < EventCapacity)
            {
                _eventCount++;
            }
        }

        public static string BuildCopyableReport()
        {
            AssetLib233DebugSnapshot snapshot = AssetLib233.Instance.CaptureDebugSnapshot();
            _builder.Length = 0;
            _builder.AppendLine("===== AssetLib233 Runtime Diagnostic =====");
            _builder.Append("time=").AppendLine(snapshot.CaptureTime);
            _builder.Append("platform=").AppendLine(snapshot.RuntimePlatform.ToString());
            _builder.Append("downloadConcurrency=").AppendLine(snapshot.DownloadConcurrency.ToString());
            _builder.Append("trackedAssets=").Append(snapshot.TrackedAssetCount.ToString());
            _builder.Append(" aliveRefs=").AppendLine(snapshot.AliveReferenceCount.ToString());
            _builder.Append("packageCount=").AppendLine(snapshot.Groups.Count.ToString());

            for (int i = 0; i < snapshot.Groups.Count; i++)
            {
                AssetLib233DebugGroupSnapshot group = snapshot.Groups[i];
                _builder.AppendLine("----------------------------------------");
                _builder.Append("group=").Append(group.GroupName);
                _builder.Append(" assets=").Append(group.AssetCount.ToString());
                _builder.Append(" bundles=").Append(group.BundleCount.ToString());
                _builder.Append(" totalBytes=").AppendLine(group.TotalBundleSize.ToString());

                for (int j = 0; j < group.Bundles.Count; j++)
                {
                    AssetLib233DebugBundleSnapshot bundle = group.Bundles[j];
                    _builder.Append("  bundle=").Append(bundle.BundleName);
                    _builder.Append(" file=").Append(bundle.FileName);
                    _builder.Append(" size=").Append(bundle.FileSize.ToString());
                    _builder.Append(" ref=").Append(bundle.ReferenceCount.ToString());
                    _builder.Append(" loaded=").Append(bundle.IsLoaded.ToString());
                    _builder.Append(" path=").AppendLine(bundle.LocalPath);
                }
            }

            _builder.AppendLine("----------------------------------------");
            _builder.AppendLine("recent-events:");
            int startIndex = _nextEventIndex - _eventCount;
            if (startIndex < 0)
            {
                startIndex += EventCapacity;
            }

            for (int i = 0; i < _eventCount; i++)
            {
                int eventIndex = startIndex + i;
                if (eventIndex >= EventCapacity)
                {
                    eventIndex -= EventCapacity;
                }

                _builder.AppendLine(_events[eventIndex]);
            }

            _builder.AppendLine("===== End AssetLib233 Runtime Diagnostic =====");
            return _builder.ToString();
        }
    }
}
