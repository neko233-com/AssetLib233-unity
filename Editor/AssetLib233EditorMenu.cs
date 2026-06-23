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
    }
}
