using System.IO;
using UnityEngine;

namespace AssetLib233.Editor
{
    /// <summary>
    /// 适配项目现有火山 DCDN Go 工具。
    /// 只通过 .local 指向工具目录和配置名，仓库不保存 AK/SK、服务器路径、私密 CDN 配置。
    /// </summary>
    public static class AssetLib233CdnGoToolAdapter
    {
        public static bool TryRun(
            AssetLib233EditorPublishLocalConfig config,
            AssetLib233EditorPublishReport report,
            string stepName,
            string command,
            int timeoutMilliseconds,
            out bool isHandled)
        {
            isHandled = false;
            if (config == null || string.IsNullOrWhiteSpace(config.cdnGoToolConfigDirectory))
            {
                return true;
            }

            isHandled = true;
            string configDirectory = ResolveProjectRelativePath(config.cdnGoToolConfigDirectory);
            string configPath = ResolveConfigPath(configDirectory, config.cdnGoToolConfigName);
            string toolPath = ResolveToolPath(configDirectory, config.cdnGoToolExecutableName);
            string finalCommand = string.IsNullOrWhiteSpace(command) ? "run" : command.Trim();

            if (!Directory.Exists(configDirectory))
            {
                AddFailedStep(report, stepName, AssetLib233EditorI18n.Text("CdnGoConfigMissing") + ": " + configDirectory);
                return false;
            }

            if (!File.Exists(configPath))
            {
                AddFailedStep(report, stepName, AssetLib233EditorI18n.Text("CdnGoConfigMissing") + ": " + configPath);
                return false;
            }

            if (!File.Exists(toolPath))
            {
                AddFailedStep(report, stepName, AssetLib233EditorI18n.Text("CdnGoToolMissing") + ": " + toolPath);
                return false;
            }

            string reportRoot = ResolveReportRoot(config);
            Directory.CreateDirectory(reportRoot);
            string logPath = Path.Combine(reportRoot, report.reportId + "_" + stepName + ".log");
            string arguments = "-config \"" + Path.GetFileName(configPath) + "\" " + finalCommand;
            AssetLib233EditorPublishReportStep step = new AssetLib233EditorPublishReportStep();
            int exitCode = AssetLib233EditorProcessRunner.RunAndWait(
                stepName,
                toolPath,
                arguments,
                configDirectory,
                logPath,
                timeoutMilliseconds,
                step);
            report.AddStep(step);
            return exitCode == 0;
        }

        public static string ResolveConfigPathForReport(AssetLib233EditorPublishLocalConfig config)
        {
            if (config == null || string.IsNullOrWhiteSpace(config.cdnGoToolConfigDirectory))
            {
                return string.Empty;
            }

            return ResolveConfigPath(
                ResolveProjectRelativePath(config.cdnGoToolConfigDirectory),
                config.cdnGoToolConfigName);
        }

        private static string ResolveToolPath(string configDirectory, string executableName)
        {
            string safeExecutableName = string.IsNullOrWhiteSpace(executableName)
                ? "douyin-refresh-cdn.exe"
                : executableName.Trim();
            return Path.Combine(configDirectory, safeExecutableName);
        }

        private static string ResolveConfigPath(string configDirectory, string configName)
        {
            string safeConfigName = string.IsNullOrWhiteSpace(configName)
                ? "config-for-douyin-refresh-cdn.minigame_wx.json"
                : configName.Trim();
            return Path.Combine(configDirectory, safeConfigName);
        }

        private static string ResolveProjectRelativePath(string path)
        {
            if (Path.IsPathRooted(path))
            {
                return path;
            }

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.GetFullPath(Path.Combine(projectRoot, path));
        }

        private static string ResolveReportRoot(AssetLib233EditorPublishLocalConfig config)
        {
            string root = string.IsNullOrWhiteSpace(config.reportRoot)
                ? "Library/AssetLib233/PublishReports"
                : config.reportRoot;
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.IsPathRooted(root) ? root : Path.Combine(projectRoot, root);
        }

        private static void AddFailedStep(AssetLib233EditorPublishReport report, string stepName, string message)
        {
            AssetLib233EditorPublishReportStep step = new AssetLib233EditorPublishReportStep();
            step.name = stepName;
            step.exitCode = -1;
            step.message = message;
            step.startTimeUtc = System.DateTime.UtcNow.ToString("O");
            step.endTimeUtc = step.startTimeUtc;
            report.AddStep(step);
            Debug.LogError("[AssetLib233-CDN] " + message);
        }
    }
}
