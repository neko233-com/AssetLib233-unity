using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AssetLib233.Editor
{
    public static class AssetLib233EditorPublishReportStore
    {
        public static string Save(AssetLib233EditorPublishLocalConfig config, AssetLib233EditorPublishReport report)
        {
            string root = string.IsNullOrWhiteSpace(config.reportRoot)
                ? "Library/AssetLib233/PublishReports"
                : config.reportRoot;
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string fullRoot = Path.IsPathRooted(root) ? root : Path.Combine(projectRoot, root);
            Directory.CreateDirectory(fullRoot);

            string fileName = report.reportId + ".json";
            string path = Path.Combine(fullRoot, fileName);
            string json = JsonUtility.ToJson(report, true);
            File.WriteAllText(path, json);
            return path;
        }

        [MenuItem("AssetLib233/Publish - Open Reports")]
        public static void OpenReports()
        {
            if (!AssetLib233EditorPublishLocalConfigLoader.TryLoad(
                    out AssetLib233EditorPublishLocalConfig config,
                    out string error))
            {
                Debug.LogError("[AssetLib233] " + error);
                return;
            }

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string root = Path.IsPathRooted(config.reportRoot)
                ? config.reportRoot
                : Path.Combine(projectRoot, config.reportRoot);
            Directory.CreateDirectory(root);
            EditorUtility.RevealInFinder(root);
        }
    }
}
