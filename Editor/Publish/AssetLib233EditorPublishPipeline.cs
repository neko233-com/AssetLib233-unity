using System;
using System.IO;
using AssetLib233.Runtime;
using UnityEditor;
using UnityEngine;

namespace AssetLib233.Editor
{
    /// <summary>
    /// AssetLib233 编辑器发布流水线。
    /// 一条龙流程：打包 -> SSH / 工具上传 CDN -> CDN 刷新 -> 生成报告 -> 可选溯源上报。
    /// 私密配置全部来自 .local 文件，仓库只保留 schema 和示例。
    /// </summary>
    public static class AssetLib233EditorPublishPipeline
    {
        private const int DefaultTimeoutMilliseconds = 60 * 60 * 1000;

        [MenuItem("AssetLib233/Publish/Run Agent First Pipeline")]
        public static void RunAgentFirstPipelineMenu()
        {
            RunAgentFirstPipeline();
        }

        public static void RunAgentFirstPipelineBatchMode()
        {
            bool success = RunAgentFirstPipeline();
            EditorApplication.Exit(success ? 0 : 1);
        }

        public static bool RunAgentFirstPipeline()
        {
            if (!AssetLib233EditorPublishLocalConfigLoader.TryLoad(
                    out AssetLib233EditorPublishLocalConfig config,
                    out string error))
            {
                Debug.LogError("[AssetLib233-Publish] " + error);
                return false;
            }

            AssetLib233EditorPublishReport report = CreateReport(config);
            AssetLib233EditorI18n.SetLanguage(config.language);
            bool success = true;

            success &= RunNativeBuildStep(config, report);
            success &= RunExternalStep(config, report, "Build", config.buildToolPath, config.buildArguments, config.buildWorkingDirectory);
            success &= RunCdnStep(config, report, true);
            success &= RunCdnStep(config, report, false);
            success &= RunTraceStep(config, report);

            report.success = success;
            report.endTimeUtc = DateTime.UtcNow.ToString("O");
            string reportPath = AssetLib233EditorPublishReportStore.Save(config, report);
            Debug.Log("[AssetLib233-Publish] 报告已生成: " + reportPath);
            return success;
        }

        private static AssetLib233EditorPublishReport CreateReport(AssetLib233EditorPublishLocalConfig config)
        {
            AssetLib233EditorPublishReport report = new AssetLib233EditorPublishReport();
            report.reportId = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + "_AssetLib233";
            report.startTimeUtc = DateTime.UtcNow.ToString("O");
            report.projectPath = Directory.GetParent(Application.dataPath).FullName;
            report.buildOutputRoot = config.buildOutputRoot;
            report.cdnProvider = config.cdnProvider;
            report.cdnRegion = config.cdnRegion;
            report.cdnBucket = config.cdnBucket;
            report.cdnPathPrefix = config.cdnPathPrefix;
            report.uploadConfigName = AssetLib233EditorPublishConfigResolver.ResolveUploadConfigName(config);
            report.cdnGoToolConfigPath = AssetLib233CdnGoToolAdapter.ResolveConfigPathForReport(config);
            report.agentValidationPlatform = config.agentValidationPlatform;
            report.agentValidationEnvironment = config.agentValidationEnvironment;
            report.cdnRootUrl = config.cdnRootUrl;
            report.enableBundleCrypto = config.enableBundleCrypto;
            return report;
        }

        private static bool RunNativeBuildStep(AssetLib233EditorPublishLocalConfig config, AssetLib233EditorPublishReport report)
        {
            AssetLib233EditorPublishReportStep step = new AssetLib233EditorPublishReportStep();
            step.name = "NativeAssetLib233Build";
            step.startTimeUtc = DateTime.UtcNow.ToString("O");
            if (string.IsNullOrWhiteSpace(config.nativeBuildProfilePath))
            {
                step.message = "未配置，跳过";
                step.endTimeUtc = step.startTimeUtc;
                report.AddStep(step);
                return true;
            }

            AssetBuildProfile233 profile = AssetDatabase.LoadAssetAtPath<AssetBuildProfile233>(config.nativeBuildProfilePath);
            if (profile == null)
            {
                step.exitCode = -1;
                step.message = "找不到 BuildProfile: " + config.nativeBuildProfilePath;
                step.endTimeUtc = DateTime.UtcNow.ToString("O");
                report.AddStep(step);
                return false;
            }

            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            if (!string.IsNullOrWhiteSpace(config.nativeBuildTarget))
            {
                if (!Enum.TryParse<BuildTarget>(config.nativeBuildTarget, out buildTarget))
                {
                    step.exitCode = -1;
                    step.message = "nativeBuildTarget 无法解析: " + config.nativeBuildTarget;
                    step.endTimeUtc = DateTime.UtcNow.ToString("O");
                    report.AddStep(step);
                    return false;
                }
            }

            string outputRoot = string.IsNullOrWhiteSpace(config.buildOutputRoot)
                ? "AssetBundles/AssetLib233/" + buildTarget
                : config.buildOutputRoot;
            bool success = AssetLib233EditorBuildPipeline.BuildProfile(
                profile,
                outputRoot,
                buildTarget,
                config.enableBundleCrypto,
                config.bundleCryptoPassword,
                out string error);
            step.exitCode = success ? 0 : -1;
            step.command = "AssetLib233EditorBuildPipeline.BuildProfile";
            step.message = success ? "构建成功: " + outputRoot : error;
            step.endTimeUtc = DateTime.UtcNow.ToString("O");
            report.AddStep(step);
            return success;
        }

        private static bool RunCdnStep(
            AssetLib233EditorPublishLocalConfig config,
            AssetLib233EditorPublishReport report,
            bool isUpload)
        {
            string reportRoot = ResolveReportRoot(config);
            AssetLib233CdnProviderContext context = new AssetLib233CdnProviderContext(
                config,
                report,
                reportRoot,
                DefaultTimeoutMilliseconds);
            IAssetLib233CdnProvider provider = AssetLib233CdnProviderFactory.Create(config);
            Debug.Log("[AssetLib233-CDN] 使用 provider: " + provider.DisplayName);
            return isUpload ? provider.Upload(context) : provider.Refresh(context);
        }

        private static bool RunExternalStep(
            AssetLib233EditorPublishLocalConfig config,
            AssetLib233EditorPublishReport report,
            string stepName,
            string toolPath,
            string arguments,
            string workingDirectory)
        {
            if (string.IsNullOrWhiteSpace(toolPath))
            {
                AssetLib233EditorPublishReportStep skippedStep = new AssetLib233EditorPublishReportStep();
                skippedStep.name = stepName;
                skippedStep.message = "未配置，跳过";
                skippedStep.startTimeUtc = DateTime.UtcNow.ToString("O");
                skippedStep.endTimeUtc = skippedStep.startTimeUtc;
                report.AddStep(skippedStep);
                return true;
            }

            string reportRoot = ResolveReportRoot(config);
            string logPath = Path.Combine(reportRoot, report.reportId + "_" + stepName + ".log");
            AssetLib233EditorPublishReportStep step = new AssetLib233EditorPublishReportStep();
            int exitCode = AssetLib233EditorProcessRunner.RunAndWait(
                stepName,
                toolPath,
                arguments,
                workingDirectory,
                logPath,
                DefaultTimeoutMilliseconds,
                step);
            report.AddStep(step);
            return exitCode == 0;
        }

        private static bool RunTraceStep(AssetLib233EditorPublishLocalConfig config, AssetLib233EditorPublishReport report)
        {
            AssetLib233EditorPublishReportStep step = new AssetLib233EditorPublishReportStep();
            step.name = "TraceServer";
            step.startTimeUtc = DateTime.UtcNow.ToString("O");
            step.command = config.cdnTraceServerUrl;
            bool success = AssetLib233EditorTraceServerClient.TryUploadReport(config, report, out string message);
            step.exitCode = success ? 0 : -1;
            step.message = message;
            step.endTimeUtc = DateTime.UtcNow.ToString("O");
            report.AddStep(step);
            return success;
        }

        private static string ResolveReportRoot(AssetLib233EditorPublishLocalConfig config)
        {
            string root = string.IsNullOrWhiteSpace(config.reportRoot)
                ? "Library/AssetLib233/PublishReports"
                : config.reportRoot;
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.IsPathRooted(root) ? root : Path.Combine(projectRoot, root);
        }
    }
}
