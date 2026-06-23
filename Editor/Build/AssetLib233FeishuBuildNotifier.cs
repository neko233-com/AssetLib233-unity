using System.IO;
using System.Net;
using System.Text;

namespace AssetLib233.Editor
{
    /// <summary>
    /// 飞书构建通知。
    /// 默认只在环境变量 ASSETLIB233_FEISHU_WEBHOOK 或 ASSETLIB233_FEISHU_WEBHOOK_URL 存在时发送。
    /// </summary>
    public static class AssetLib233FeishuBuildNotifier
    {
        private const string WebhookEnvName = "ASSETLIB233_FEISHU_WEBHOOK";
        private const string WebhookUrlEnvName = "ASSETLIB233_FEISHU_WEBHOOK_URL";

        public static bool TryNotify(AssetLib233EditorBuildReport report, out string message)
        {
            message = string.Empty;
            if (report == null)
            {
                message = "report is null";
                return false;
            }

            string webhookUrl = System.Environment.GetEnvironmentVariable(WebhookEnvName);
            if (string.IsNullOrEmpty(webhookUrl))
            {
                webhookUrl = System.Environment.GetEnvironmentVariable(WebhookUrlEnvName);
            }

            if (string.IsNullOrEmpty(webhookUrl))
            {
                return false;
            }

            string payload = BuildPayload(report);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
            try
            {
                HttpWebRequest request = WebRequest.Create(webhookUrl) as HttpWebRequest;
                if (request == null)
                {
                    message = "webhook url invalid";
                    return false;
                }

                request.Method = "POST";
                request.ContentType = "application/json; charset=utf-8";
                request.ContentLength = payloadBytes.Length;
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(payloadBytes, 0, payloadBytes.Length);
                }

                using (WebResponse response = request.GetResponse())
                {
                    HttpWebResponse httpResponse = response as HttpWebResponse;
                    if (httpResponse != null)
                    {
                        message = "statusCode = " + (int)httpResponse.StatusCode;
                    }
                }

                return true;
            }
            catch (System.Exception exception)
            {
                message = exception.Message;
                return false;
            }
        }

        private static string BuildPayload(AssetLib233EditorBuildReport report)
        {
            StringBuilder textBuilder = new StringBuilder(512);
            textBuilder.Append(report.success ? "AssetLib233 构建成功" : "AssetLib233 构建失败");
            textBuilder.Append("\\n平台: ");
            textBuilder.Append(report.platformName);
            textBuilder.Append("\\nProfile: ");
            textBuilder.Append(report.profilePath);
            textBuilder.Append("\\nOutput: ");
            textBuilder.Append(report.outputRoot);
            textBuilder.Append("\\nGroups: ");
            textBuilder.Append(report.groupCount);
            textBuilder.Append(" | Bundles: ");
            textBuilder.Append(report.bundleCount);
            textBuilder.Append(" | Assets: ");
            textBuilder.Append(report.assetCount);
            textBuilder.Append("\\nReport: ");
            textBuilder.Append(report.reportPath);
            if (!string.IsNullOrEmpty(report.error))
            {
                textBuilder.Append("\\nError: ");
                textBuilder.Append(report.error);
            }

            string escapedText = EscapeJson(textBuilder.ToString());
            return "{\"msg_type\":\"text\",\"content\":{\"text\":\"" + escapedText + "\"}}";
        }

        private static string EscapeJson(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder(value.Length + 16);
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (c == '\\')
                {
                    builder.Append("\\\\");
                }
                else if (c == '"')
                {
                    builder.Append("\\\"");
                }
                else if (c == '\n')
                {
                    builder.Append("\\n");
                }
                else if (c == '\r')
                {
                    builder.Append("\\r");
                }
                else if (c == '\t')
                {
                    builder.Append("\\t");
                }
                else
                {
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }
    }
}
