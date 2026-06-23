using System.IO;
using UnityEngine;

namespace AssetLib233.Editor
{
    /// <summary>
    /// CDN provider 执行上下文。
    /// 所有私密路径和 token 只来自 .local 配置或环境变量，不写入仓库。
    /// </summary>
    public sealed class AssetLib233CdnProviderContext
    {
        private readonly AssetLib233EditorPublishLocalConfig _config;
        private readonly AssetLib233EditorPublishReport _report;
        private readonly string _reportRoot;
        private readonly int _timeoutMilliseconds;

        public AssetLib233CdnProviderContext(
            AssetLib233EditorPublishLocalConfig config,
            AssetLib233EditorPublishReport report,
            string reportRoot,
            int timeoutMilliseconds)
        {
            _config = config;
            _report = report;
            _reportRoot = reportRoot;
            _timeoutMilliseconds = timeoutMilliseconds;
        }

        public AssetLib233EditorPublishLocalConfig Config
        {
            get { return _config; }
        }

        public AssetLib233EditorPublishReport Report
        {
            get { return _report; }
        }

        public string ReportRoot
        {
            get { return _reportRoot; }
        }

        public bool RunUploadTool(string stepName)
        {
            return RunToolStep(
                stepName,
                _config.uploadToolPath,
                _config.uploadArguments,
                _config.uploadWorkingDirectory);
        }

        public bool RunRefreshTool(string stepName)
        {
            return RunToolStep(
                stepName,
                _config.refreshToolPath,
                _config.refreshArguments,
                _config.refreshWorkingDirectory);
        }

        public void AddSkippedStep(string stepName, string message)
        {
            AssetLib233EditorPublishReportStep skippedStep = new AssetLib233EditorPublishReportStep();
            skippedStep.name = stepName;
            skippedStep.message = message;
            skippedStep.startTimeUtc = System.DateTime.UtcNow.ToString("O");
            skippedStep.endTimeUtc = skippedStep.startTimeUtc;
            _report.AddStep(skippedStep);
        }

        private bool RunToolStep(
            string stepName,
            string toolPath,
            string arguments,
            string workingDirectory)
        {
            if (string.IsNullOrWhiteSpace(toolPath))
            {
                AddSkippedStep(stepName, "未配置工具，跳过");
                return true;
            }

            Directory.CreateDirectory(_reportRoot);
            string logPath = Path.Combine(_reportRoot, _report.reportId + "_" + stepName + ".log");
            AssetLib233EditorPublishReportStep step = new AssetLib233EditorPublishReportStep();
            int exitCode = AssetLib233EditorProcessRunner.RunAndWait(
                stepName,
                toolPath,
                arguments,
                workingDirectory,
                logPath,
                _timeoutMilliseconds,
                step);
            _report.AddStep(step);
            if (exitCode != 0)
            {
                Debug.LogError("[AssetLib233-CDN] " + stepName + " 失败: " + step.message);
            }

            return exitCode == 0;
        }
    }
}
