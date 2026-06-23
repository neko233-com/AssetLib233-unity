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
        private readonly AssetLib233AssetBundleProvider _assetBundleProvider;
#if UNITY_EDITOR
        private readonly AssetLib233EditorAssetDatabaseProvider _editorAssetDatabaseProvider =
            new AssetLib233EditorAssetDatabaseProvider();
#endif
        private AssetLib233PackageConfig _config;
        private AssetManifest233 _manifest;
        private EnumAssetLib233PackageStatus _status;
        private string _lastError;

        public AssetPackage233(string packageName)
        {
            _packageName = AssetLib233NameUtility.NormalizePackageName(packageName);
            _assetBundleProvider = new AssetLib233AssetBundleProvider(this);
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

        public AssetLib233PackageConfig Config
        {
            get { return _config; }
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
            if (config.PlayMode == EnumHotUpdateType.EditorRemoteSimulation)
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
            AssetInfo233 assetInfo = ResolveAssetInfoForLoad(location);
            if (assetInfo == null)
            {
                AssetHandle233<TObject> failedHandle = new AssetHandle233<TObject>(_packageName, location);
                failedHandle.SetFailed(
                    "Manifest 中找不到资源. group = " +
                    _packageName +
                    " | address = " +
                    location +
                    " | manifestLoaded = " +
                    (_manifest != null).ToString());
                AssetLib233RuntimeDiagnostic.RecordEvent("load-miss group=" + _packageName + " address=" + location);
                return failedHandle;
            }

#if UNITY_EDITOR
            if (_config == null ||
                _config.PlayMode == EnumHotUpdateType.EditorSimulate ||
                _config.PlayMode == EnumHotUpdateType.EditorRemoteSimulation)
            {
                return _editorAssetDatabaseProvider.LoadAssetAsync<TObject>(this, assetInfo);
            }
#endif
            return _assetBundleProvider.LoadAssetAsync<TObject>(this, assetInfo);
        }

        public AssetHandle233<Object> LoadAssetAsync(string location)
        {
            return LoadAssetAsync<Object>(location);
        }

        public AssetHandle233<TObject> LoadAssetByPathAsync<TObject>(string assetPath) where TObject : Object
        {
            return LoadAssetAsync<TObject>(assetPath);
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

        public void BuildAllDownloadRequestsNonAlloc(System.Collections.Generic.List<AssetLib233DownloadRequest> results)
        {
            if (results == null)
            {
                return;
            }

            results.Clear();
            if (_manifest == null || _config == null)
            {
                return;
            }

            IAssetLib233RemoteServices remoteServices =
                new AssetLib233RemoteServices(_config.DefaultHostServer, _config.FallbackHostServer);
            for (int i = 0; i < _manifest.Bundles.Count; i++)
            {
                AssetBundleInfo233 bundleInfo = _manifest.Bundles[i];
                if (bundleInfo == null)
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

        public void Update()
        {
            _assetBundleProvider.Update();
        }

        public void UnloadAllBundles(bool unloadAllLoadedObjects)
        {
            _assetBundleProvider.UnloadAllBundles(unloadAllLoadedObjects);
        }

        public void FillDebugSnapshot(AssetLib233DebugGroupSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            snapshot.GroupName = _packageName;
            if (_manifest != null)
            {
                snapshot.AssetCount = _manifest.Assets.Count;
                snapshot.BundleCount = _manifest.Bundles.Count;
                long totalBundleSize = 0L;
                for (int i = 0; i < _manifest.Bundles.Count; i++)
                {
                    AssetBundleInfo233 bundleInfo = _manifest.Bundles[i];
                    if (bundleInfo == null)
                    {
                        continue;
                    }

                    totalBundleSize += bundleInfo.FileSize;
                    AssetLib233DebugBundleSnapshot bundleSnapshot = new AssetLib233DebugBundleSnapshot();
                    bundleSnapshot.BundleName = bundleInfo.BundleName;
                    bundleSnapshot.FileName = bundleInfo.FileName;
                    bundleSnapshot.FileSize = bundleInfo.FileSize;
                    bundleSnapshot.LocalPath = AssetLib233BundlePathResolver.GetCacheBundlePath(_packageName, bundleInfo);
                    snapshot.Bundles.Add(bundleSnapshot);
                }

                snapshot.TotalBundleSize = totalBundleSize;
            }

            _assetBundleProvider.FillDebugBundleSnapshots(snapshot.Bundles);
        }

        private AssetInfo233 ResolveAssetInfoForLoad(string location)
        {
            if (string.IsNullOrEmpty(location))
            {
                return null;
            }

            if (_manifest != null && _manifest.TryGetAssetInfo(location, out AssetInfo233 assetInfo))
            {
                return assetInfo;
            }

#if UNITY_EDITOR
            if (location.StartsWith("Assets/"))
            {
                AssetInfo233 editorAssetInfo = new AssetInfo233();
                editorAssetInfo.Address = location;
                editorAssetInfo.AssetPath = location;
                editorAssetInfo.BundleName = string.Empty;
                return editorAssetInfo;
            }
#endif
            return null;
        }
    }
}
