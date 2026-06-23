using UnityEngine;

namespace AssetLib233.Runtime
{
    /// <summary>
    /// AssetGroup 运行时对象。负责 Manifest 查询、下载计划生成、资源加载入口和调试状态。
    /// </summary>
    public sealed class AssetPackage233
    {
        private readonly string _packageName;
        private readonly AssetLib233CacheIndex _cacheIndex = new AssetLib233CacheIndex();
        private AssetLib233PackageConfig _config;
        private AssetManifest233 _manifest;
        private EnumAssetLib233PackageStatus _status;
        private string _lastError;

        public AssetPackage233(string packageName)
        {
            _packageName = AssetLib233NameUtility.NormalizePackageName(packageName);
        }

        public string PackageName
        {
            get { return _packageName; }
        }

        public EnumAssetLib233PackageStatus Status
        {
            get { return _status; }
        }

        public string LastError
        {
            get { return _lastError; }
        }

        public AssetManifest233 Manifest
        {
            get { return _manifest; }
        }

        public AssetLib233CacheIndex CacheIndex
        {
            get { return _cacheIndex; }
        }

        public void Initialize(AssetLib233PackageConfig config)
        {
            if (config == null)
            {
                _lastError = "AssetGroup 初始化配置为空";
                _status = EnumAssetLib233PackageStatus.Failed;
                Debug.LogError("[AssetLib233] " + _lastError);
                return;
            }

            _config = config;
            IAssetLib233PlatformPlugin plugin = AssetLib233PluginRegistry.GetPlugin(AssetLib233PlatformDetector.GetRuntimePlatform());
            plugin.BeforeInitializePackage(config);
            if (config.PlayMode == EnumAssetLib233PlayMode.EditorRemoteSimulation)
            {
                AssetLib233EditorRemoteSimulationService.Enable(string.Empty);
            }

            _status = EnumAssetLib233PackageStatus.Initialized;
        }

        public void SetManifest(AssetManifest233 manifest)
        {
            _manifest = manifest;
            _status = manifest != null ? EnumAssetLib233PackageStatus.ManifestLoaded : EnumAssetLib233PackageStatus.Initialized;
        }

        public bool TryGetAssetInfo(string address, out AssetInfo233 assetInfo)
        {
            assetInfo = null;
            if (_manifest == null)
            {
                return false;
            }

            return _manifest.TryGetAssetInfo(address, out assetInfo);
        }

        public AssetHandle233<TObject> LoadAssetAsync<TObject>(string location) where TObject : Object
        {
            AssetHandle233<TObject> handle = new AssetHandle233<TObject>(_packageName, location);
            handle.SetFailed("AssetLib233 Provider 尚未接入真实 AB 加载: " + location);
            return handle;
        }

        public AssetHandle233<Object> LoadAssetAsync(string location)
        {
            AssetHandle233<Object> handle = new AssetHandle233<Object>(_packageName, location);
            handle.SetFailed("AssetLib233 Provider 尚未接入真实 AB 加载: " + location);
            return handle;
        }

        public void BuildDownloadRequestsNonAlloc(
            string tag,
            System.Collections.Generic.List<AssetLib233DownloadRequest> results,
            System.Collections.Generic.List<AssetInfo233> assetQueryCache)
        {
            if (results == null)
            {
                return;
            }

            results.Clear();
            if (_manifest == null)
            {
                return;
            }

            IAssetLib233RemoteServices remoteServices =
                new AssetLib233RemoteServices(_config.DefaultHostServer, _config.FallbackHostServer);
            if (assetQueryCache == null)
            {
                return;
            }

            _manifest.GetAssetsByTagNonAlloc(tag, assetQueryCache);
            for (int i = 0; i < assetQueryCache.Count; i++)
            {
                AssetInfo233 assetInfo = assetQueryCache[i];
                if (!_manifest.TryGetBundleInfo(assetInfo.BundleName, out AssetBundleInfo233 bundleInfo))
                {
                    continue;
                }

                AssetLib233DownloadRequest request = new AssetLib233DownloadRequest();
                request.GroupName = _packageName;
                request.BundleInfo = bundleInfo;
                request.MainUrl = remoteServices.GetRemoteMainUrl(bundleInfo.FileName);
                request.FallbackUrl = remoteServices.GetRemoteFallbackUrl(bundleInfo.FileName);
                request.RetryCount = 3;
                results.Add(request);
            }
        }
    }
}
