using System.IO;
using UnityEngine;

namespace AssetLib233.Editor
{
    /// <summary>
    /// 构建报告存储。
    /// 默认写入 Library，避免报告携带本机路径或密码时误入 Git。
    /// </summary>
    public static class AssetLib233EditorBuildReportStore
    {
        private const string ReportDirectory = "Library/AssetLib233/BuildReports";

        public static string Save(AssetLib233EditorBuildReport report)
        {
            if (report == null)
            {
                return string.Empty;
            }

            Directory.CreateDirectory(ReportDirectory);
            string reportId = string.IsNullOrEmpty(report.reportId)
                ? System.DateTime.UtcNow.ToString("yyyyMMddHHmmss")
                : report.reportId;
            string fileName = "AssetLib233_BuildReport_" + SanitizeFileName(reportId) + ".json";
            string reportPath = Path.Combine(ReportDirectory, fileName).Replace('\\', '/');
            report.reportPath = reportPath;
            string json = JsonUtility.ToJson(report, true);
            File.WriteAllText(reportPath, json, System.Text.Encoding.UTF8);
            return reportPath;
        }

        private static string SanitizeFileName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "empty";
            }

            char[] invalidChars = Path.GetInvalidFileNameChars();
            string result = value;
            for (int i = 0; i < invalidChars.Length; i++)
            {
                result = result.Replace(invalidChars[i], '_');
            }

            return result;
        }
    }
}
