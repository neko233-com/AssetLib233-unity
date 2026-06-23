using System.IO;
using UnityEngine.Networking;

namespace AssetLib233.Runtime
{
    /// <summary>
    /// AssetGroup 准备操作：请求版本、下载 Manifest、注入运行时。
    /// 全程主线程异步推进，可用于 Host / WebPlay / 小游戏平台。
    /// </summary>
    public sealed class AssetLib233PreparePackageOperation : AssetLib233Operation
    {
        private enum Step
        {
            None = 0,
            LoadLocalManifest = 1,
            RequestVersion = 2,
            DownloadManifest = 3,
            Done = 4
        }

        private readonly string _groupName;
        private UnityWebRequest _webRequest;
        private Step _step;
        private AssetLib233RemoteVersionInfo _versionInfo;
        private string _remoteHost;

        public AssetLib233PreparePackageOperation(string groupName)
        {
            _groupName = AssetLib233NameUtility.NormalizePackageName(groupName);
        }

        public AssetLib233RemoteVersionInfo VersionInfo
        {
            get { return _versionInfo; }
        }

        protected override void OnStart()
        {
            AssetPackage233 assetPackage = AssetLib233.Instance.GetOrCreateGroup(_groupName);
            AssetLib233PackageConfig config = assetPackage.Config;
            if (config == null)
            {
                config = new AssetLib233PackageConfig();
                config.PackageName = _groupName;
                AssetLib233.Instance.InitializeGroup(config);
            }

            _remoteHost = config.DefaultHostServer;
            if (config.PlayMode == EnumHotUpdateType.Offline ||
                config.PlayMode == EnumHotUpdateType.EditorSimulate)
            {
                _step = Step.LoadLocalManifest;
                return;
            }

            if (string.IsNullOrEmpty(_remoteHost))
            {
                SetFailed("远端 Host 为空，无法请求版本. group = " + _groupName);
                return;
            }

            StartRequestVersion();
        }

        protected override void OnUpdate()
        {
            switch (_step)
            {
                case Step.LoadLocalManifest:
                {
                    LoadLocalManifest();
                    break;
                }
                case Step.RequestVersion:
                {
                    UpdateRequestVersion();
                    break;
                }
                case Step.DownloadManifest:
                {
                    UpdateDownloadManifest();
                    break;
                }
            }
        }

        private void StartRequestVersion()
        {
            string versionUrl = _remoteHost + "/" + _groupName + AssetLib233Constants.VersionFileExtension;
            _webRequest = UnityWebRequest.Get(versionUrl);
            _webRequest.SendWebRequest();
            _step = Step.RequestVersion;
            Progress = 0.1f;
            AssetLib233RuntimeDiagnostic.RecordEvent("prepare-version-start group=" + _groupName + " url=" + versionUrl);
        }

        private void UpdateRequestVersion()
        {
            if (_webRequest == null || !_webRequest.isDone)
            {
                return;
            }

            if (IsWebRequestFailed(_webRequest))
            {
                string error = "请求版本失败. group = " + _groupName + " | url = " + _webRequest.url + " | error = " + _webRequest.error;
                DisposeWebRequest();
                SetFailed(error);
                AssetLib233RuntimeDiagnostic.RecordEvent("prepare-version-fail " + error);
                return;
            }

            string versionText = _webRequest.downloadHandler.text;
            DisposeWebRequest();
            if (!TryParseVersionText(versionText, out _versionInfo, out string parseError))
            {
                SetFailed(parseError);
                AssetLib233RuntimeDiagnostic.RecordEvent("prepare-version-parse-fail " + parseError);
                return;
            }

            StartDownloadManifest();
        }

        private void StartDownloadManifest()
        {
            string manifestFileName = _versionInfo.ManifestFileName;
            if (string.IsNullOrEmpty(manifestFileName))
            {
                manifestFileName = _groupName + AssetLib233Constants.ManifestFileExtension;
                _versionInfo.ManifestFileName = manifestFileName;
            }

            string manifestUrl = _remoteHost + "/" + manifestFileName;
            _webRequest = UnityWebRequest.Get(manifestUrl);
            _webRequest.SendWebRequest();
            _step = Step.DownloadManifest;
            Progress = 0.45f;
            AssetLib233RuntimeDiagnostic.RecordEvent("prepare-manifest-start group=" + _groupName + " url=" + manifestUrl);
        }

        private void UpdateDownloadManifest()
        {
            if (_webRequest == null || !_webRequest.isDone)
            {
                return;
            }

            if (IsWebRequestFailed(_webRequest))
            {
                string error = "下载 Manifest 失败. group = " + _groupName + " | url = " + _webRequest.url + " | error = " + _webRequest.error;
                DisposeWebRequest();
                SetFailed(error);
                AssetLib233RuntimeDiagnostic.RecordEvent("prepare-manifest-fail " + error);
                return;
            }

            byte[] manifestBytes = _webRequest.downloadHandler.data;
            DisposeWebRequest();
            ApplyManifestBytes(manifestBytes, true);
        }

        private void LoadLocalManifest()
        {
            string cachePath = Path.Combine(
                AssetLib233BundlePathResolver.GetCacheRoot(_groupName),
                _groupName + AssetLib233Constants.ManifestFileExtension);
            string builtinPath = Path.Combine(
                AssetLib233BundlePathResolver.GetBuiltinRoot(_groupName),
                _groupName + AssetLib233Constants.ManifestFileExtension);
            if (File.Exists(cachePath))
            {
                ApplyManifestBytes(File.ReadAllBytes(cachePath), false);
                return;
            }

            if (File.Exists(builtinPath))
            {
                ApplyManifestBytes(File.ReadAllBytes(builtinPath), false);
                return;
            }

            SetFailed("本地 Manifest 不存在. cache = " + cachePath + " | builtin = " + builtinPath);
        }

        private void ApplyManifestBytes(byte[] manifestBytes, bool saveToCache)
        {
            if (!AssetManifestBinarySerializer233.TryDeserialize(manifestBytes, out AssetManifest233 manifest, out string error))
            {
                SetFailed("Manifest 反序列化失败. group = " + _groupName + " | error = " + error);
                return;
            }

            AssetLib233.Instance.SetManifest(_groupName, manifest);
            if (saveToCache)
            {
                SaveManifestToCache(manifestBytes);
            }

            _step = Step.Done;
            AssetLib233RuntimeDiagnostic.RecordEvent(
                "prepare-ok group=" +
                _groupName +
                " version=" +
                manifest.PackageVersion +
                " assets=" +
                manifest.Assets.Count +
                " bundles=" +
                manifest.Bundles.Count);
            SetSucceed();
        }

        private void SaveManifestToCache(byte[] manifestBytes)
        {
            if (manifestBytes == null || manifestBytes.Length == 0)
            {
                return;
            }

            try
            {
                string cacheRoot = AssetLib233BundlePathResolver.GetCacheRoot(_groupName);
                Directory.CreateDirectory(cacheRoot);
                string cachePath = Path.Combine(cacheRoot, _groupName + AssetLib233Constants.ManifestFileExtension);
                File.WriteAllBytes(cachePath, manifestBytes);
            }
            catch (System.Exception ex)
            {
                AssetLib233RuntimeDiagnostic.RecordEvent("prepare-manifest-cache-write-fail group=" + _groupName + " error=" + ex.Message);
            }
        }

        private bool TryParseVersionText(string versionText, out AssetLib233RemoteVersionInfo versionInfo, out string error)
        {
            versionInfo = null;
            error = string.Empty;
            if (string.IsNullOrEmpty(versionText))
            {
                error = "版本文件为空. group = " + _groupName;
                return false;
            }

            string safeText = versionText.Trim();
            string[] parts = safeText.Split('|');
            AssetLib233RemoteVersionInfo info = new AssetLib233RemoteVersionInfo();
            info.GroupName = _groupName;
            info.PackageVersion = parts.Length > 0 ? parts[0].Trim() : safeText;
            info.ManifestFileName = parts.Length > 1 ? parts[1].Trim() : _groupName + AssetLib233Constants.ManifestFileExtension;
            info.ManifestHash = parts.Length > 2 ? parts[2].Trim() : string.Empty;
            if (parts.Length > 3 && long.TryParse(parts[3].Trim(), out long manifestSize))
            {
                info.ManifestSize = manifestSize;
            }

            if (string.IsNullOrEmpty(info.PackageVersion))
            {
                error = "版本号为空. group = " + _groupName;
                return false;
            }

            versionInfo = info;
            return true;
        }

        private static bool IsWebRequestFailed(UnityWebRequest request)
        {
#if UNITY_2020_1_OR_NEWER
            return request.result != UnityWebRequest.Result.Success;
#else
            return request.isNetworkError || request.isHttpError;
#endif
        }

        private void DisposeWebRequest()
        {
            if (_webRequest == null)
            {
                return;
            }

            _webRequest.Dispose();
            _webRequest = null;
        }
    }
}
