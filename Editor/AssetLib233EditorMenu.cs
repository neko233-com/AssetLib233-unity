using AssetLib233.Runtime;
using UnityEditor;
using UnityEngine;

namespace AssetLib233.Editor
{
    public static class AssetLib233EditorMenu
    {
        private const string DefaultSettingsPath = "Assets/neko233/AssetLib233/AssetLib233Settings.asset";

        [MenuItem("neko233/AssetLib233/打开设置窗口")]
        public static void OpenSettingsWindow()
        {
            AssetLib233SettingsWindow.Open();
        }

        [MenuItem("neko233/AssetLib233/创建默认设置资产")]
        public static void CreateDefaultSettingsAsset()
        {
            AssetLib233Settings settings = AssetDatabase.LoadAssetAtPath<AssetLib233Settings>(DefaultSettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<AssetLib233Settings>();
                AssetDatabase.CreateAsset(settings, DefaultSettingsPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            Selection.activeObject = settings;
            EditorGUIUtility.PingObject(settings);
        }

        [MenuItem("neko233/AssetLib233/构建选中 BuildProfile")]
        public static void BuildSelectedProfile()
        {
            AssetBuildProfile233 profile = Selection.activeObject as AssetBuildProfile233;
            if (profile == null)
            {
                Debug.LogError("[AssetLib233] 请先在 Project 面板选中 AssetBuildProfile233");
                return;
            }

            string outputRoot = "AssetBundles/AssetLib233/" + EditorUserBuildSettings.activeBuildTarget;
            if (!AssetLib233EditorBuildPipeline.BuildProfile(
                    profile,
                    outputRoot,
                    EditorUserBuildSettings.activeBuildTarget,
                    out string error))
            {
                Debug.LogError("[AssetLib233] 构建失败: " + error);
                return;
            }

            AssetDatabase.Refresh();
        }
    }
}
