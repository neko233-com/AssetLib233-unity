using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;

namespace AssetLib233.Editor
{
    /// <summary>
    /// CDN 溯源服务器客户端。
    /// 上传内容为发布报告 JSON；鉴权 token 从环境变量读取，避免写入仓库或 .local 示例。
    /// </summary>
    public static class AssetLib233EditorTraceServerClient
    {
        public static bool TryUploadReport(
            AssetLib233EditorPublishLocalConfig config,
            AssetLib233EditorPublishReport report,
            out string message)
        {
            message = string.Empty;
            if (string.IsNullOrWhiteSpace(config.cdnTraceServerUrl))
            {
                message = "未配置溯源服务器";
                return true;
            }

            try
            {
                string json = JsonUtility.ToJson(report, true);
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                HttpWebRequest request = WebRequest.Create(config.cdnTraceServerUrl) as HttpWebRequest;
                if (request == null)
                {
                    message = "创建 HTTP 请求失败";
                    return false;
                }

                request.Method = "POST";
                request.ContentType = "application/json; charset=utf-8";
                request.ContentLength = bytes.Length;
                string token = string.IsNullOrWhiteSpace(config.cdnTraceTokenEnvName)
                    ? string.Empty
                    : Environment.GetEnvironmentVariable(config.cdnTraceTokenEnvName);
                if (!string.IsNullOrWhiteSpace(token))
                {
                    request.Headers["Authorization"] = "Bearer " + token;
                }

                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(bytes, 0, bytes.Length);
                }

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    int statusCode = response != null ? (int)response.StatusCode : 0;
                    message = "溯源上报完成 statusCode = " + statusCode;
                    return statusCode >= 200 && statusCode < 300;
                }
            }
            catch (Exception exception)
            {
                message = exception.Message;
                return false;
            }
        }
    }
}
