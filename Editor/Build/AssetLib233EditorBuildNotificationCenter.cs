using System;
using System.Collections.Generic;
using UnityEngine;

namespace AssetLib233.Editor
{
    public static class AssetLib233EditorBuildNotificationCenter
    {
        private static readonly List<IAssetLib233EditorBuildNotifier> _customNotifiers =
            new List<IAssetLib233EditorBuildNotifier>(8);

        public static void Register(IAssetLib233EditorBuildNotifier notifier)
        {
            if (notifier == null || _customNotifiers.Contains(notifier))
            {
                return;
            }

            _customNotifiers.Add(notifier);
        }

        public static void ClearCustomNotifiers()
        {
            _customNotifiers.Clear();
        }

        public static void Notify(AssetLib233EditorBuildReport report)
        {
            for (int i = 0; i < _customNotifiers.Count; i++)
            {
                IAssetLib233EditorBuildNotifier notifier = _customNotifiers[i];
                if (notifier == null)
                {
                    continue;
                }

                try
                {
                    notifier.OnBuildReport(report);
                }
                catch (Exception exception)
                {
                    Debug.LogError("[AssetLib233] 自定义构建通知器异常: " + exception.Message);
                }
            }

            string feishuMessage;
            if (!AssetLib233FeishuBuildNotifier.TryNotify(report, out feishuMessage))
            {
                if (!string.IsNullOrEmpty(feishuMessage))
                {
                    Debug.Log("[AssetLib233] 飞书构建通知未发送: " + feishuMessage);
                }
            }
        }
    }
}
