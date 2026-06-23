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
        public string cdnRootUrl = "";
        public string cdnTraceServerUrl = "";
        public string cdnTraceTokenEnvName = "";
        public string sshConfigPath = "";
        public string privateNote = "";

        public static AssetLib233EditorPublishLocalConfig CreateDefault()
        {
            AssetLib233EditorPublishLocalConfig config = new AssetLib233EditorPublishLocalConfig();
            config.reportRoot = "Library/AssetLib233/PublishReports";
            return config;
        }
    }
}
