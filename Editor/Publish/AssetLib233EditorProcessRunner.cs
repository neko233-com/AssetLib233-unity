using System;
using System.Diagnostics;
using System.IO;

namespace AssetLib233.Editor
{
    /// <summary>
    /// 编辑器外部进程执行器。
    /// 用途：调用打包、SSH 上传、CDN 刷新、溯源上报等外部工具，并把日志写入发布报告目录。
    /// </summary>
    public static class AssetLib233EditorProcessRunner
    {
        public static int RunAndWait(
            string stepName,
            string fileName,
            string arguments,
            string workingDirectory,
            string logPath,
            int timeoutMilliseconds,
            AssetLib233EditorPublishReportStep reportStep)
        {
            reportStep.name = stepName;
            reportStep.command = fileName + " " + arguments;
            reportStep.workingDirectory = workingDirectory;
            reportStep.logPath = logPath;
            reportStep.startTimeUtc = DateTime.UtcNow.ToString("O");

            if (string.IsNullOrWhiteSpace(fileName) || !File.Exists(fileName))
            {
                reportStep.exitCode = -1;
                reportStep.message = "工具不存在: " + fileName;
                reportStep.endTimeUtc = DateTime.UtcNow.ToString("O");
                return reportStep.exitCode;
            }

            if (string.IsNullOrWhiteSpace(workingDirectory))
            {
                workingDirectory = Path.GetDirectoryName(fileName);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(logPath));

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = fileName;
            startInfo.Arguments = arguments ?? string.Empty;
            startInfo.WorkingDirectory = workingDirectory;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            try
            {
                Process process = Process.Start(startInfo);
                if (process == null)
                {
                    reportStep.exitCode = -2;
                    reportStep.message = "进程启动失败";
                    reportStep.endTimeUtc = DateTime.UtcNow.ToString("O");
                    return reportStep.exitCode;
                }

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                bool isExited = process.WaitForExit(timeoutMilliseconds);
                if (!isExited)
                {
                    process.Kill();
                    reportStep.exitCode = -3;
                    reportStep.message = "执行超时";
                }
                else
                {
                    reportStep.exitCode = process.ExitCode;
                    reportStep.message = process.ExitCode == 0 ? "成功" : "失败";
                }

                File.WriteAllText(logPath, output + "\n" + error);
                reportStep.endTimeUtc = DateTime.UtcNow.ToString("O");
                return reportStep.exitCode;
            }
            catch (Exception exception)
            {
                reportStep.exitCode = -4;
                reportStep.message = exception.Message;
                reportStep.endTimeUtc = DateTime.UtcNow.ToString("O");
                File.WriteAllText(logPath, exception.ToString());
                return reportStep.exitCode;
            }
        }
    }
}
