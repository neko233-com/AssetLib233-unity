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
        private readonly string _cryptoPassword;
        private AssetBundleCreateRequest _fileRequest;
        private AssetLib233XorBundleStream _xorStream;
        private UnityWebRequest _webRequest;
        private bool _isStarted;

        public bool IsDone;
        public AssetBundle Bundle;
        public string Error;
        public string UsedPath;

        public AssetLib233BundleLoadSlot(
            AssetBundleInfo233 bundleInfo,
            string localPath,
            string builtinPath,
            string cryptoPassword)
        {
            _bundleInfo = bundleInfo;
            _localPath = localPath;
            _builtinPath = builtinPath;
            _cryptoPassword = cryptoPassword;
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
                StartLoadFromLocalFile(_localPath);
                return;
            }

            if (!string.IsNullOrEmpty(_builtinPath) && File.Exists(_builtinPath))
            {
                StartLoadFromLocalFile(_builtinPath);
                return;
            }

            if (AssetLib233BundlePathResolver.IsWebPath(_builtinPath))
            {
                if (_bundleInfo.IsEncrypted)
                {
                    IsDone = true;
                    Error = "加密 AB 不支持直接 WebPath 加载，请先下载到本地缓存再用流式解密. url = " + _builtinPath;
                    return;
                }

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
            DisposeXorStream();
            IsDone = true;
            if (Bundle == null)
            {
                Error = "AssetBundle.LoadFromFileAsync 返回空. path = " + UsedPath;
            }
        }

        private void StartLoadFromLocalFile(string path)
        {
            UsedPath = path;
            if (_bundleInfo.IsEncrypted)
            {
                _xorStream = new AssetLib233XorBundleStream(path, _cryptoPassword);
                _fileRequest = AssetBundle.LoadFromStreamAsync(_xorStream, _bundleInfo.FileCrc);
                return;
            }

            _fileRequest = AssetBundle.LoadFromFileAsync(path, _bundleInfo.FileCrc);
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

        private void DisposeXorStream()
        {
            if (_xorStream == null)
            {
                return;
            }

            _xorStream.Dispose();
            _xorStream = null;
        }
    }

    internal sealed class AssetLib233XorBundleStream : FileStream
    {
        private readonly byte[] _keyBytes;

        public AssetLib233XorBundleStream(string path, string password)
            : base(path, FileMode.Open, FileAccess.Read, FileShare.Read)
        {
            _keyBytes = AssetLib233XorCrypto.BuildKeyBytes(password);
        }

        public override int Read(byte[] array, int offset, int count)
        {
            long startPosition = Position;
            int readCount = base.Read(array, offset, count);
            if (readCount > 0)
            {
                AssetLib233XorCrypto.ApplyXorInPlace(array, offset, readCount, startPosition, _keyBytes);
            }

            return readCount;
        }

        public override int ReadByte()
        {
            long startPosition = Position;
            int rawValue = base.ReadByte();
            if (rawValue < 0)
            {
                return rawValue;
            }

            return AssetLib233XorCrypto.ApplyXorByte((byte)rawValue, startPosition, _keyBytes);
        }
    }
}
