using System;
using UnityEditor;
using UnityEngine;

namespace AssetLib233.Editor
{
    [Serializable]
    public sealed class AssetLib233EditorToolConfigMapping
    {
        public string key = "";
        public string platform = "";
        public string environment = "";
        public string macro = "";
        public string configName = "";
    }

    /// <summary>
    /// 本地发布配置。
    /// 该配置包含 SSH、CDN、溯源服务等私密路径或地址，只允许放在 .local 文件中，不提交到 GitHub。
    /// </summary>
    [Serializable]
    public sealed class AssetLib233EditorPublishLocalConfig
    {
        public string activeConfigKey = "";
        public string buildOutputRoot = "";
        public string nativeBuildProfilePath = "";
        public string nativeBuildTarget = "";
        public bool enableBundleCrypto;
        public string bundleCryptoPassword = "root";
        public string reportRoot = "Library/AssetLib233/PublishReports";
        public string buildToolPath = "";
        public string buildWorkingDirectory = "";
        public string buildArguments = "";
        public string uploadToolPath = "";
        public string uploadWorkingDirectory = "";
        public string uploadArguments = "";
        public string uploadConfigName = "";
        public AssetLib233EditorToolConfigMapping[] uploadConfigMappings = new AssetLib233EditorToolConfigMapping[0];
        public string refreshToolPath = "";
        public string refreshWorkingDirectory = "";
        public string refreshArguments = "";
        public string cdnProvider = "VolcengineChina";
        public string language = "zh-CN";
        public string cdnRegion = "cn";
        public string cdnBucket = "";
        public string cdnPathPrefix = "";
        public string cdnRefreshMode = "Directory";
        public string cdnProviderProfile = "";
        public string cdnGoToolConfigDirectory = "";
        public string cdnGoToolConfigName = "";
        public AssetLib233EditorToolConfigMapping[] cdnGoToolConfigMappings = new AssetLib233EditorToolConfigMapping[0];
        public string cdnGoToolExecutableName = "douyin-refresh-cdn.exe";
        public string cdnGoToolCommand = "run";
        public string agentValidationBuildProfilePath = "";
        public string agentValidationPlatform = "WX";
        public string agentValidationEnvironment = "test";
        public string[] agentValidationRequiredGroups = new string[] { "login", "default", "story" };
        public string[] agentValidationSampleAddresses = new string[0];
        public string cdnRootUrl = "";
        public string cdnTraceServerUrl = "";
        public string cdnTraceTokenEnvName = "";
        public string sshConfigPath = "";
        public string privateNote = "";

        public static AssetLib233EditorPublishLocalConfig CreateDefault()
        {
            AssetLib233EditorPublishLocalConfig config = new AssetLib233EditorPublishLocalConfig();
            config.bundleCryptoPassword = "root";
            config.reportRoot = "Library/AssetLib233/PublishReports";
            config.cdnProvider = "VolcengineChina";
            config.language = "zh-CN";
            config.cdnRegion = "cn";
            config.cdnRefreshMode = "Directory";
            config.cdnGoToolExecutableName = "douyin-refresh-cdn.exe";
            config.cdnGoToolCommand = "run";
            config.agentValidationPlatform = "WX";
            config.agentValidationEnvironment = "test";
            return config;
        }
    }

    internal static class AssetLib233EditorPublishConfigResolver
    {
        public const string UploadConfigNameToken = "{uploadConfigName}";
        public const string CdnGoConfigNameToken = "{cdnGoConfigName}";

        public static string ResolveUploadConfigName(AssetLib233EditorPublishLocalConfig config)
        {
            if (config == null)
            {
                return string.Empty;
            }

            string mapped = ResolveMappedConfigName(config, config.uploadConfigMappings);
            if (!string.IsNullOrEmpty(mapped))
            {
                return mapped;
            }

            return TrimOrEmpty(config.uploadConfigName);
        }

        public static string ResolveCdnGoConfigName(AssetLib233EditorPublishLocalConfig config)
        {
            if (config == null)
            {
                return string.Empty;
            }

            string mapped = ResolveMappedConfigName(config, config.cdnGoToolConfigMappings);
            if (!string.IsNullOrEmpty(mapped))
            {
                return mapped;
            }

            return TrimOrEmpty(config.cdnGoToolConfigName);
        }

        public static string ResolveUploadArguments(AssetLib233EditorPublishLocalConfig config)
        {
            if (config == null)
            {
                return string.Empty;
            }

            string configName = ResolveUploadConfigName(config);
            string arguments = config.uploadArguments ?? string.Empty;
            if (arguments.Contains(UploadConfigNameToken))
            {
                if (string.IsNullOrWhiteSpace(configName))
                {
                    return string.Empty;
                }

                return arguments.Replace(UploadConfigNameToken, configName);
            }

            if (string.IsNullOrWhiteSpace(arguments) && !string.IsNullOrWhiteSpace(configName))
            {
                return "--config \"" + configName + "\"";
            }

            return arguments;
        }

        public static string ResolveCdnGoArguments(string configName, string command)
        {
            if (string.IsNullOrWhiteSpace(configName))
            {
                return string.Empty;
            }

            string safeConfigName = configName.Trim();
            string safeCommand = string.IsNullOrWhiteSpace(command) ? "run" : command.Trim();
            return "-config \"" + safeConfigName + "\" " + safeCommand;
        }

        private static string ResolveMappedConfigName(
            AssetLib233EditorPublishLocalConfig config,
            AssetLib233EditorToolConfigMapping[] mappings)
        {
            if (mappings == null || mappings.Length == 0)
            {
                return string.Empty;
            }

            string activeKey = ResolveActiveKey(config);
            string activePlatform = ResolveActivePlatform(config);
            string activeEnvironment = ResolveActiveEnvironment(config);
            string activeDefines = ResolveActiveDefines();

            string result = FindByExactKey(mappings, activeKey);
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }

            result = FindByMacro(mappings, activeDefines);
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }

            result = FindByPlatformEnvironment(mappings, activePlatform, activeEnvironment, true, true);
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }

            result = FindByPlatformEnvironment(mappings, activePlatform, activeEnvironment, true, false);
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }

            if (string.IsNullOrEmpty(activePlatform) && string.IsNullOrEmpty(activeDefines))
            {
                result = FindByPlatformEnvironment(mappings, activePlatform, activeEnvironment, false, true);
                if (!string.IsNullOrEmpty(result))
                {
                    return result;
                }

                return FindByExactKey(mappings, "default");
            }

            return string.Empty;
        }

        private static string FindByExactKey(AssetLib233EditorToolConfigMapping[] mappings, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            for (int i = 0; i < mappings.Length; i++)
            {
                AssetLib233EditorToolConfigMapping mapping = mappings[i];
                if (mapping == null || string.IsNullOrWhiteSpace(mapping.configName))
                {
                    continue;
                }

                if (string.Equals(TrimOrEmpty(mapping.key), key, StringComparison.OrdinalIgnoreCase))
                {
                    return mapping.configName.Trim();
                }
            }

            return string.Empty;
        }

        private static string FindByMacro(AssetLib233EditorToolConfigMapping[] mappings, string activeDefines)
        {
            if (string.IsNullOrEmpty(activeDefines))
            {
                return string.Empty;
            }

            for (int i = 0; i < mappings.Length; i++)
            {
                AssetLib233EditorToolConfigMapping mapping = mappings[i];
                if (mapping == null ||
                    string.IsNullOrWhiteSpace(mapping.macro) ||
                    string.IsNullOrWhiteSpace(mapping.configName))
                {
                    continue;
                }

                if (ContainsDefine(activeDefines, mapping.macro.Trim()))
                {
                    return mapping.configName.Trim();
                }
            }

            return string.Empty;
        }

        private static string FindByPlatformEnvironment(
            AssetLib233EditorToolConfigMapping[] mappings,
            string platform,
            string environment,
            bool requirePlatform,
            bool requireEnvironment)
        {
            if (requirePlatform && string.IsNullOrEmpty(platform))
            {
                return string.Empty;
            }

            if (requireEnvironment && string.IsNullOrEmpty(environment))
            {
                return string.Empty;
            }

            for (int i = 0; i < mappings.Length; i++)
            {
                AssetLib233EditorToolConfigMapping mapping = mappings[i];
                if (mapping == null || string.IsNullOrWhiteSpace(mapping.configName))
                {
                    continue;
                }

                bool platformOk = !requirePlatform ||
                                  string.Equals(TrimOrEmpty(mapping.platform), platform, StringComparison.OrdinalIgnoreCase);
                bool environmentOk = !requireEnvironment ||
                                     string.Equals(TrimOrEmpty(mapping.environment), environment, StringComparison.OrdinalIgnoreCase);
                if (platformOk && environmentOk)
                {
                    return mapping.configName.Trim();
                }
            }

            return string.Empty;
        }

        private static string ResolveActiveKey(AssetLib233EditorPublishLocalConfig config)
        {
            string envKey = Environment.GetEnvironmentVariable("ASSETLIB233_CONFIG_KEY");
            if (!string.IsNullOrWhiteSpace(envKey))
            {
                return envKey.Trim();
            }

            return TrimOrEmpty(config.activeConfigKey);
        }

        private static string ResolveActivePlatform(AssetLib233EditorPublishLocalConfig config)
        {
            string envPlatform = Environment.GetEnvironmentVariable("ASSETLIB233_PLATFORM");
            if (!string.IsNullOrWhiteSpace(envPlatform))
            {
                return envPlatform.Trim();
            }

            string buildTargetPlatform = ResolvePlatformFromBuildTarget(EditorUserBuildSettings.activeBuildTarget);
            if (!string.IsNullOrEmpty(buildTargetPlatform))
            {
                return buildTargetPlatform;
            }

            return TrimOrEmpty(config.agentValidationPlatform);
        }

        private static string ResolveActiveEnvironment(AssetLib233EditorPublishLocalConfig config)
        {
            string envName = Environment.GetEnvironmentVariable("ASSETLIB233_ENV");
            if (!string.IsNullOrWhiteSpace(envName))
            {
                return envName.Trim();
            }

            return TrimOrEmpty(config.agentValidationEnvironment);
        }

        private static string ResolveActiveDefines()
        {
            BuildTargetGroup targetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup) ?? string.Empty;
            string buildTargetMacro = ResolveMacroFromBuildTarget(EditorUserBuildSettings.activeBuildTarget);
            if (!string.IsNullOrEmpty(buildTargetMacro))
            {
                defines = defines + ";" + buildTargetMacro;
            }

            string extraMacro = Environment.GetEnvironmentVariable("ASSETLIB233_MACRO");
            if (!string.IsNullOrWhiteSpace(extraMacro))
            {
                defines = defines + ";" + extraMacro.Trim();
            }

            return defines;
        }

        private static string ResolvePlatformFromBuildTarget(BuildTarget buildTarget)
        {
            string targetName = buildTarget.ToString();
            if (targetName.IndexOf("TapTap", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "TAPMINIGAME";
            }

            if (targetName.IndexOf("WeChat", StringComparison.OrdinalIgnoreCase) >= 0 ||
                targetName.IndexOf("Weixin", StringComparison.OrdinalIgnoreCase) >= 0 ||
                targetName.IndexOf("WX", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "WX";
            }

            if (buildTarget == BuildTarget.Android)
            {
                return "Android";
            }

            if (buildTarget == BuildTarget.iOS)
            {
                return "iOS";
            }

            if (buildTarget == BuildTarget.WebGL)
            {
                return "WebGL";
            }

            return string.Empty;
        }

        private static string ResolveMacroFromBuildTarget(BuildTarget buildTarget)
        {
            string platform = ResolvePlatformFromBuildTarget(buildTarget);
            if (string.Equals(platform, "Android", StringComparison.OrdinalIgnoreCase))
            {
                return "UNITY_ANDROID";
            }

            if (string.Equals(platform, "iOS", StringComparison.OrdinalIgnoreCase))
            {
                return "UNITY_IOS";
            }

            if (string.Equals(platform, "WebGL", StringComparison.OrdinalIgnoreCase))
            {
                return "UNITY_WEBGL";
            }

            return platform;
        }

        private static bool ContainsDefine(string defines, string macro)
        {
            if (string.IsNullOrEmpty(defines) || string.IsNullOrEmpty(macro))
            {
                return false;
            }

            string[] parts = defines.Split(';');
            for (int i = 0; i < parts.Length; i++)
            {
                if (string.Equals(parts[i].Trim(), macro, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static string TrimOrEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
