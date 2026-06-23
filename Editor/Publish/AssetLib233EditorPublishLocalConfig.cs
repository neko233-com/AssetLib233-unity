using System;
using UnityEngine;

namespace AssetLib233.Editor
{
    /// <summary>
    /// 本地发布配置。
    /// 该配置包含 SSH、CDN、溯源服务等私密路径或地址，只允许放在 .local 文件中，不提交到 GitHub。
    /// </summary>
    [Serializable]
    public sealed class AssetLib233EditorPublishLocalConfig
    {
        public string buildOutputRoot = "";
        public string nativeBuildProfilePath = "";
        public string nativeBuildTarget = "";
        public string reportRoot = "Library/AssetLib233/PublishReports";
        public string buildToolPath = "";
        public string buildWorkingDirectory = "";
        public string buildArguments = "";
        public string uploadToolPath = "";
        public string uploadWorkingDirectory = "";
        public string uploadArguments = "";
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
}
