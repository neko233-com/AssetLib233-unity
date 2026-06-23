using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace AssetLib233.Runtime
{
    internal sealed class AssetLib233BundleLoadSlot
    {
        private readonly AssetBundleInfo233 _bundleInfo;
        private readonly string _localPath;
        private readonly string _builtinPath;
        private AssetBundleCreateRequest _fileRequest;
        private UnityWebRequest _webRequest;
        private bool _isStarted;

        public bool IsDone;
        public AssetBundle Bundle;
        public string Error;
        public string UsedPath;

        public AssetLib233BundleLoadSlot(AssetBundleInfo233 bundleInfo, string localPath, string builtinPath)
        {
            _bundleInfo = bundleInfo;
            _localPath = localPath;
            _builtinPath = builtinPath;
        }

        public void Update()
        {
            if (IsDone)
            {
                return;
            }

            if (!_isStarted)
            {
                StartLoad();
                return;
            }

            if (_fileRequest != null)
            {
                UpdateFileRequest();
                return;
            }

            if (_webRequest != null)
            {
                UpdateWebRequest();
            }
        }

        private void StartLoad()
        {
            _isStarted = true;
            if (!string.IsNullOrEmpty(_localPath) && File.Exists(_localPath))
            {
                UsedPath = _localPath;
                _fileRequest = AssetBundle.LoadFromFileAsync(_localPath, _bundleInfo.FileCrc);
                return;
            }

            if (!string.IsNullOrEmpty(_builtinPath) && File.Exists(_builtinPath))
            {
                UsedPath = _builtinPath;
                _fileRequest = AssetBundle.LoadFromFileAsync(_builtinPath, _bundleInfo.FileCrc);
                return;
            }

            if (AssetLib233BundlePathResolver.IsWebPath(_builtinPath))
            {
                UsedPath = _builtinPath;
                _webRequest = UnityWebRequestAssetBundle.GetAssetBundle(_builtinPath, _bundleInfo.FileCrc);
                _webRequest.SendWebRequest();
                return;
            }

            IsDone = true;
            Error = "AB 文件不存在. cache = " + _localPath + " | builtin = " + _builtinPath;
        }

        private void UpdateFileRequest()
        {
            if (!_fileRequest.isDone)
            {
                return;
            }

            Bundle = _fileRequest.assetBundle;
            _fileRequest = null;
            IsDone = true;
            if (Bundle == null)
            {
                Error = "AssetBundle.LoadFromFileAsync 返回空. path = " + UsedPath;
            }
        }

        private void UpdateWebRequest()
        {
            if (!_webRequest.isDone)
            {
                return;
            }

#if UNITY_2020_1_OR_NEWER
            bool isFailed = _webRequest.result != UnityWebRequest.Result.Success;
#else
            bool isFailed = _webRequest.isNetworkError || _webRequest.isHttpError;
#endif
            if (isFailed)
            {
                Error = "UnityWebRequestAssetBundle 下载内置 AB 失败. url = " + UsedPath + " | error = " + _webRequest.error;
                _webRequest.Dispose();
                _webRequest = null;
                IsDone = true;
                return;
            }

            Bundle = DownloadHandlerAssetBundle.GetContent(_webRequest);
            _webRequest.Dispose();
            _webRequest = null;
            IsDone = true;
            if (Bundle == null)
            {
                Error = "UnityWebRequestAssetBundle 返回空. url = " + UsedPath;
            }
        }
    }
}
