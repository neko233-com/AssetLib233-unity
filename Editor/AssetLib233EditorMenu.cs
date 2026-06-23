using AssetLib233.Runtime;
using UnityEditor;
using UnityEngine;

namespace AssetLib233.Editor
{
    public static class AssetLib233EditorMenu
    {
        private const string DefaultSettingsPath = "Assets/neko233/AssetLib233/AssetLib233Settings.asset";
        private const string DefaultBuildOutputRoot = "AssetBundles/AssetLib233";
        private const string TopMenuRoot = "AssetLib233/";
        private const string LegacyMenuRoot = "neko233/AssetLib233/";

        [MenuItem(TopMenuRoot + "面板/打开运行设置窗口", priority = 10)]
        public static void OpenSettingsWindow()
        {
            AssetLib233SettingsWindow.Open();
        }

        [MenuItem(LegacyMenuRoot + "打开设置窗口", priority = 10)]
        public static void LegacyOpenSettingsWindow()
        {
            OpenSettingsWindow();
        }

        [MenuItem(TopMenuRoot + "面板/打开收集配置窗口", priority = 11)]
        public static void OpenBuildProfileWindow()
        {
            AssetLib233BuildProfileWindow.Open();
        }

        [MenuItem(LegacyMenuRoot + "打开收集配置窗口", priority = 11)]
        public static void LegacyOpenBuildProfileWindow()
        {
            OpenBuildProfileWindow();
        }

        [MenuItem(TopMenuRoot + "配置/创建默认运行设置资产", priority = 30)]
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

        [MenuItem(LegacyMenuRoot + "创建默认设置资产", priority = 30)]
        public static void LegacyCreateDefaultSettingsAsset()
        {
            CreateDefaultSettingsAsset();
        }

        [MenuItem(TopMenuRoot + "配置/从旧 YooAsset Collector 生成 BuildProfile", priority = 31)]
        public static void CreateBuildProfileFromLegacyCollector()
        {
            string profilePath = "Assets/neko233/AssetLib233/AssetLib233BuildProfile.asset";
            AssetBuildProfile233 existingProfile = AssetDatabase.LoadAssetAtPath<AssetBuildProfile233>(profilePath);
            if (existingProfile != null)
            {
                bool isOverwrite = EditorUtility.DisplayDialog(
                    "AssetLib233",
                    "已存在 AssetLib233BuildProfile.asset，是否用旧 YooAsset Collector 重新生成？",
                    "重新生成",
                    "取消");
                if (!isOverwrite)
                {
                    Selection.activeObject = existingProfile;
                    EditorGUIUtility.PingObject(existingProfile);
                    return;
                }

                AssetDatabase.DeleteAsset(profilePath);
            }

            if (!AssetLib233YooAssetCollectorCompat.TryCreateProfile(
                    AssetLib233YooAssetCollectorCompat.DefaultCollectorSettingPath,
                    null,
                    out AssetBuildProfile233 profile,
                    out string error))
            {
                Debug.LogError("[AssetLib233] 旧 YooAsset Collector 转 BuildProfile 失败: " + error);
                return;
            }

            AssetDatabase.CreateAsset(profile, profilePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = profile;
            EditorGUIUtility.PingObject(profile);
            Debug.Log("[AssetLib233] 已生成 BuildProfile: " + profilePath);
        }

        [MenuItem(LegacyMenuRoot + "从旧 YooAsset Collector 生成 BuildProfile", priority = 31)]
        public static void LegacyCreateBuildProfileFromLegacyCollector()
        {
            CreateBuildProfileFromLegacyCollector();
        }

        [MenuItem(TopMenuRoot + "构建/构建选中 BuildProfile", priority = 100)]
        public static void BuildSelectedProfile()
        {
            AssetBuildProfile233 profile = Selection.activeObject as AssetBuildProfile233;
            if (profile == null)
            {
                Debug.LogError("[AssetLib233] 请先在 Project 面板选中 AssetBuildProfile233");
                return;
            }

            BuildProfileWithProjectCrypto(profile, GetDefaultBuildOutputRoot(), "选中 BuildProfile");
        }

        [MenuItem(LegacyMenuRoot + "构建选中 BuildProfile", priority = 100)]
        public static void LegacyBuildSelectedProfile()
        {
            BuildSelectedProfile();
        }

        [MenuItem(TopMenuRoot + "构建/从旧 YooAsset Collector 构建全部 AssetGroup", priority = 110)]
        public static void BuildAllGroupsFromLegacyCollector()
        {
            BuildLegacyCollectorGroups("全部 AssetGroup", null);
        }

        [MenuItem(TopMenuRoot + "构建/从旧 YooAsset Collector 构建 login 首包", priority = 111)]
        public static void BuildLoginGroupFromLegacyCollector()
        {
            BuildLegacyCollectorGroups("login 首包", new[] { "login" });
        }

        [MenuItem(TopMenuRoot + "构建/从旧 YooAsset Collector 构建 default+story 内容包", priority = 112)]
        public static void BuildRequiredContentGroupsFromLegacyCollector()
        {
            BuildLegacyCollectorGroups("default+story 内容包", new[] { "default", "story" });
        }

        [MenuItem(TopMenuRoot + "报告/打开构建报告目录", priority = 200)]
        public static void OpenBuildReportDirectory()
        {
            string reportDirectory = "Library/AssetLib233/BuildReports";
            if (!System.IO.Directory.Exists(reportDirectory))
            {
                System.IO.Directory.CreateDirectory(reportDirectory);
            }

            EditorUtility.RevealInFinder(reportDirectory);
        }

        private static void BuildLegacyCollectorGroups(string buildTitle, string[] packageFilters)
        {
            if (!AssetLib233YooAssetCollectorCompat.TryCreateProfile(
                    AssetLib233YooAssetCollectorCompat.DefaultCollectorSettingPath,
                    packageFilters,
                    out AssetBuildProfile233 profile,
                    out string error))
            {
                Debug.LogError("[AssetLib233] 旧 YooAsset Collector 转换失败: " + error);
                return;
            }

            try
            {
                BuildProfileWithProjectCrypto(profile, GetDefaultBuildOutputRoot(), "旧 Collector -> " + buildTitle);
            }
            finally
            {
                ScriptableObject.DestroyImmediate(profile);
            }
        }

        private static void BuildProfileWithProjectCrypto(AssetBuildProfile233 profile, string outputRoot, string buildTitle)
        {
            bool enableBundleCrypto = AssetLib233EditorCryptoSettingsResolver.ResolveEnableBundleCrypto();
            string bundleCryptoPassword = AssetLib233EditorCryptoSettingsResolver.ResolveBundleCryptoPassword();
            Debug.Log("[AssetLib233] 开始构建: " + buildTitle);
            Debug.Log("[AssetLib233] " + AssetLib233EditorCryptoSettingsResolver.BuildDebugText(enableBundleCrypto, bundleCryptoPassword));
            if (!AssetLib233EditorBuildPipeline.BuildProfile(
                    profile,
                    outputRoot,
                    EditorUserBuildSettings.activeBuildTarget,
                    enableBundleCrypto,
                    bundleCryptoPassword,
                    out string error))
            {
                Debug.LogError("[AssetLib233] 构建失败: " + error);
                return;
            }

            AssetDatabase.Refresh();
        }

        private static string GetDefaultBuildOutputRoot()
        {
            return DefaultBuildOutputRoot + "/" + EditorUserBuildSettings.activeBuildTarget;
        }
    }
}
