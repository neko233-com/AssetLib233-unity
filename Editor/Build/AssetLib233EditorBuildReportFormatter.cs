using System.Text;

namespace AssetLib233.Editor
{
    public static class AssetLib233EditorBuildReportFormatter
    {
        public static string ToCopyText(AssetLib233EditorBuildReport report)
        {
            if (report == null)
            {
                return "[AssetLib233] Build report is null";
            }

            StringBuilder builder = new StringBuilder(1024);
            builder.AppendLine("[AssetLib233 Build Report]");
            builder.AppendLine("success=" + report.success);
            builder.AppendLine("platform=" + report.platformName);
            builder.AppendLine("profile=" + report.profilePath);
            builder.AppendLine("output=" + report.outputRoot);
            builder.AppendLine("report=" + report.reportPath);
            builder.AppendLine("crypto=" + report.enableBundleCrypto);
            builder.AppendLine("cryptoPassword=" + report.bundleCryptoPassword);
            builder.AppendLine("groups=" + report.groupCount);
            builder.AppendLine("bundles=" + report.bundleCount);
            builder.AppendLine("assets=" + report.assetCount);
            builder.AppendLine("encryptedBundles=" + report.encryptedBundleCount);
            builder.AppendLine("totalBundleBytes=" + report.totalBundleBytes);
            if (!string.IsNullOrEmpty(report.error))
            {
                builder.AppendLine("error=" + report.error);
            }

            for (int i = 0; i < report.groups.Count; i++)
            {
                AssetLib233EditorBuildReportGroup group = report.groups[i];
                if (group == null)
                {
                    continue;
                }

                builder.AppendLine(
                    "group[" +
                    i +
                    "] name=" +
                    group.groupName +
                    " version=" +
                    group.packageVersion +
                    " compression=" +
                    group.compressionMode +
                    " success=" +
                    group.success +
                    " bundles=" +
                    group.bundleCount +
                    " assets=" +
                    group.assetCount +
                    " bytes=" +
                    group.totalBundleBytes);
                if (!string.IsNullOrEmpty(group.error))
                {
                    builder.AppendLine("group[" + i + "] error=" + group.error);
                }
            }

            return builder.ToString();
        }
    }
}
