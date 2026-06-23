using System.IO;
using UnityEditor;
using UnityEngine;

namespace AssetLib233.Editor
{
    /// <summary>
    /// .local 配置加载器。
    /// 默认读取项目根目录 AssetLib233.publish.local.json；也支持通过环境变量 ASSETLIB233_LOCAL_CONFIG 指定路径。
    /// </summary>
    public static class AssetLib233EditorPublishLocalConfigLoader
    {
        private const string EnvName = "ASSETLIB233_LOCAL_CONFIG";
        private const string DefaultFileName = "AssetLib233.publish.local.json";
        private const string ProjectFileName = "AssetLib233.publish.project.json";

        public static string GetDefaultLocalConfigPath()
        {
            string envPath = System.Environment.GetEnvironmentVariable(EnvName);
            if (!string.IsNullOrWhiteSpace(envPath))
            {
                return envPath;
            }

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string localPath = Path.Combine(projectRoot, DefaultFileName);
            if (File.Exists(localPath))
            {
                return localPath;
            }

            string projectConfigPath = Path.Combine(projectRoot, ProjectFileName);
            if (File.Exists(projectConfigPath))
            {
                return projectConfigPath;
            }

            return localPath;
        }

        public static bool TryLoad(out AssetLib233EditorPublishLocalConfig config, out string error)
        {
            string path = GetDefaultLocalConfigPath();
            return TryLoad(path, out config, out error);
        }

        public static bool TryLoad(string path, out AssetLib233EditorPublishLocalConfig config, out string error)
        {
            config = null;
            error = string.Empty;
            if (string.IsNullOrWhiteSpace(path))
            {
                error = ".local 配置路径为空";
                return false;
            }

            if (!File.Exists(path))
            {
                error = "找不到 .local 配置: " + path;
                return false;
            }

            string json = File.ReadAllText(path);
            config = JsonUtility.FromJson<AssetLib233EditorPublishLocalConfig>(json);
            if (config == null)
            {
                error = ".local 配置解析失败: " + path;
                return false;
            }

            return true;
        }

        [MenuItem("AssetLib233/Publish/Create .local Example")]
        public static void CreateLocalExample()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string path = Path.Combine(projectRoot, "AssetLib233.publish.local.example.json");
            AssetLib233EditorPublishLocalConfig config = AssetLib233EditorPublishLocalConfig.CreateDefault();
            string json = JsonUtility.ToJson(config, true);
            File.WriteAllText(path, json);
            EditorUtility.RevealInFinder(path);
        }
    }
}
