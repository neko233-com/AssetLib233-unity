using System.IO;
using UnityEngine.Networking;

namespace AssetLib233.Runtime
{
    internal sealed class AssetLib233UnityWebRequestDownloadState
    {
        public UnityWebRequest Request;
        public string TargetPath;
        public string TempPath;
        public bool IsDone;
        public bool IsSuccess;
        public string Error;
        public long DownloadedBytes;
    }

    /// <summary>
    /// 默认 HTTP 下载传输层。0 依赖，Host / Web / EditorRemoteSimulation 都可复用。
    /// </summary>
    public class AssetLib233UnityWebRequestDownloadTransport : IAssetLib233DownloadTransport
    {
        public bool CanStartRequest(AssetLib233DownloadRequest request)
        {
            if (request == null || request.BundleInfo == null)
            {
                return false;
            }

            return !string.IsNullOrEmpty(request.MainUrl) || !string.IsNullOrEmpty(request.FallbackUrl);
        }

        public void StartRequest(AssetLib233DownloadRequest request)
        {
            if (request == null || request.BundleInfo == null)
            {
                return;
            }

            AssetLib233UnityWebRequestDownloadState state = new AssetLib233UnityWebRequestDownloadState();
            state.TargetPath = AssetLib233BundlePathResolver.GetCacheBundlePath(request.GroupName, request.BundleInfo);
            state.TempPath = state.TargetPath + ".download";
            request.UserData = state;

            if (IsCacheFileValid(state.TargetPath, request.BundleInfo.FileSize))
            {
                state.IsDone = true;
                state.IsSuccess = true;
                state.DownloadedBytes = request.BundleInfo.FileSize;
                AssetLib233RuntimeDiagnostic.RecordEvent("download-cache-hit file=" + request.CurrentFileName + " path=" + state.TargetPath);
                return;
            }

            try
            {
                string directory = Path.GetDirectoryName(state.TargetPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (File.Exists(state.TempPath))
                {
                    File.Delete(state.TempPath);
                }

                string url = string.IsNullOrEmpty(request.MainUrl) ? request.FallbackUrl : request.MainUrl;
                state.Request = CreateRequest(url, state.TempPath);
                state.Request.SendWebRequest();
                AssetLib233RuntimeDiagnostic.RecordEvent("download-start file=" + request.CurrentFileName + " url=" + url + " path=" + state.TargetPath);
            }
            catch (System.Exception ex)
            {
                state.IsDone = true;
                state.IsSuccess = false;
                state.Error = ex.Message;
            }
        }

        public bool IsRequestDone(AssetLib233DownloadRequest request)
        {
            AssetLib233UnityWebRequestDownloadState state = GetState(request);
            if (state == null)
            {
                return true;
            }

            if (state.IsDone)
            {
                return true;
            }

            if (state.Request == null)
            {
                state.IsDone = true;
                state.IsSuccess = false;
                state.Error = "UnityWebRequest 为空";
                return true;
            }

            state.DownloadedBytes = (long)state.Request.downloadedBytes;
            if (!state.Request.isDone)
            {
                return false;
            }

            CompleteRequest(request, state);
            return true;
        }

        public bool IsRequestSuccess(AssetLib233DownloadRequest request)
        {
            AssetLib233UnityWebRequestDownloadState state = GetState(request);
            return state != null && state.IsSuccess;
        }

        public long GetDownloadedBytes(AssetLib233DownloadRequest request)
        {
            AssetLib233UnityWebRequestDownloadState state = GetState(request);
            if (state == null)
            {
                return 0L;
            }

            return state.DownloadedBytes;
        }

        public string GetError(AssetLib233DownloadRequest request)
        {
            AssetLib233UnityWebRequestDownloadState state = GetState(request);
            return state == null ? "下载状态为空" : state.Error;
        }

        private static AssetLib233UnityWebRequestDownloadState GetState(AssetLib233DownloadRequest request)
        {
            if (request == null)
            {
                return null;
            }

            return request.UserData as AssetLib233UnityWebRequestDownloadState;
        }

        private static bool IsCacheFileValid(string path, long expectedSize)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return false;
            }

            if (expectedSize <= 0)
            {
                return true;
            }

            FileInfo fileInfo = new FileInfo(path);
            return fileInfo.Length == expectedSize;
        }

        private static bool IsWebRequestFailed(UnityWebRequest request)
        {
#if UNITY_2020_1_OR_NEWER
            return request.result != UnityWebRequest.Result.Success;
#else
            return request.isNetworkError || request.isHttpError;
#endif
        }

        protected virtual UnityWebRequest CreateRequest(string url, string tempPath)
        {
            UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
            request.downloadHandler = new DownloadHandlerFile(tempPath);
            return request;
        }

        private void CompleteRequest(AssetLib233DownloadRequest request, AssetLib233UnityWebRequestDownloadState state)
        {
            if (IsWebRequestFailed(state.Request))
            {
                state.IsDone = true;
                state.IsSuccess = false;
                state.Error = state.Request.error;
                DisposeRequest(state);
                AssetLib233RuntimeDiagnostic.RecordEvent("download-fail file=" + request.CurrentFileName + " error=" + state.Error);
                return;
            }

            try
            {
                if (File.Exists(state.TargetPath))
                {
                    File.Delete(state.TargetPath);
                }

                File.Move(state.TempPath, state.TargetPath);
                state.DownloadedBytes = request.BundleInfo.FileSize;
                state.IsSuccess = true;
                state.IsDone = true;
                AssetLib233RuntimeDiagnostic.RecordEvent("download-ok file=" + request.CurrentFileName + " path=" + state.TargetPath);
            }
            catch (System.Exception ex)
            {
                state.IsSuccess = false;
                state.IsDone = true;
                state.Error = ex.Message;
                AssetLib233RuntimeDiagnostic.RecordEvent("download-save-fail file=" + request.CurrentFileName + " error=" + ex.Message);
            }
            finally
            {
                DisposeRequest(state);
            }
        }

        private static void DisposeRequest(AssetLib233UnityWebRequestDownloadState state)
        {
            if (state == null || state.Request == null)
            {
                return;
            }

            state.Request.Dispose();
            state.Request = null;
        }
    }
}
