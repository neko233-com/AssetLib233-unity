using System.Collections.Generic;
using UnityEngine;

namespace AssetLib233.Runtime
{
    /// <summary>
    /// AssetLib233 用户 facade。
    /// 设计目标：
    /// 1. 用户只需要访问 AssetLib233.Instance，不关心 Manifest、下载器、文件系统、Provider 的内部细节。
    /// 2. AssetGroup 是热更资源组，等价项目语义中的 login / default / story / voice / cg 等独立包。
    /// 3. Collector 决定 AssetGroup 内资源如何切成 N 个 AB / RawBundle / ArchiveBundle。
    /// 4. 全流程按主线程异步模型设计，适配 WebGL / 小游戏单线程主循环。
    /// </summary>
    public sealed class AssetLib233
    {
        private static readonly AssetLib233 _instance = new AssetLib233();

        private readonly Dictionary<string, AssetPackage233> _packages = new Dictionary<string, AssetPackage233>(16);
        private readonly List<AssetPackage233> _packageList = new List<AssetPackage233>(16);
        private readonly List<AssetLib233Operation> _runningOperations = new List<AssetLib233Operation>(16);
        private readonly List<AssetInfo233> _assetQueryCache = new List<AssetInfo233>(256);
        private readonly List<AssetLib233DownloadRequest> _downloadRequestCache = new List<AssetLib233DownloadRequest>(256);
        private readonly AssetLib233AssetGcService _assetGcService = new AssetLib233AssetGcService();
        private bool _isInitialized;

        private AssetLib233()
        {
        }

        /// <summary>
        /// 全局单例入口。业务层统一通过该入口访问 AssetLib233。
        /// </summary>
        public static AssetLib233 Instance
        {
            get { return _instance; }
        }

        public bool IsInitialized
        {
            get { return _isInitialized; }
        }

        public int PackageCount
        {
            get { return _packages.Count; }
        }

        public AssetLib233AssetGcService AssetGcService
        {
            get { return _assetGcService; }
        }

        /// <summary>
        /// 初始化资源系统。只安装插件和设置全局参数，不主动下载任何资源。
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            AssetLib233PluginRegistry.InstallDefaultPlugins();
            EnumAssetLib233RuntimePlatform runtimePlatform = AssetLib233PlatformDetector.GetRuntimePlatform();
            AssetLib233RuntimeOptions.DownloadConcurrency = AssetLib233DownloadPolicy.GetDownloadConcurrency(runtimePlatform);
            _isInitialized = true;
        }

        /// <summary>
        /// 获取或创建 AssetGroup 运行时对象。
        /// </summary>
        public AssetPackage233 GetOrCreateGroup(string groupName)
        {
            Initialize();

            string safeGroupName = AssetLib233NameUtility.NormalizePackageName(groupName);
            if (_packages.TryGetValue(safeGroupName, out AssetPackage233 assetPackage))
            {
                return assetPackage;
            }

            assetPackage = new AssetPackage233(safeGroupName);
            _packages.Add(safeGroupName, assetPackage);
            _packageList.Add(assetPackage);
            return assetPackage;
        }

        /// <summary>
        /// 尝试获取 AssetGroup。
        /// </summary>
        public bool TryGetGroup(string groupName, out AssetPackage233 assetPackage)
        {
            string safeGroupName = AssetLib233NameUtility.NormalizePackageName(groupName);
            return _packages.TryGetValue(safeGroupName, out assetPackage);
        }

        /// <summary>
        /// 初始化指定 AssetGroup。初始化只建立运行上下文，Manifest 加载和下载由热更流水线负责。
        /// </summary>
        public AssetPackage233 InitializeGroup(AssetLib233PackageConfig config)
        {
            AssetPackage233 assetPackage = GetOrCreateGroup(config.PackageName);
            assetPackage.Initialize(config);
            return assetPackage;
        }

        /// <summary>
        /// 设置 AssetGroup 的 Manifest。通常由热更版本检查完成后调用。
        /// </summary>
        public void SetManifest(string groupName, AssetManifest233 manifest)
        {
            AssetPackage233 assetPackage = GetOrCreateGroup(groupName);
            assetPackage.SetManifest(manifest);
        }

        /// <summary>
        /// 按 group + address 加载资源。当前 Provider 骨架已独立，后续接真实 AB 加载器。
        /// </summary>
        public AssetHandle233<TObject> LoadAssetAsync<TObject>(string groupName, string address) where TObject : Object
        {
            AssetPackage233 assetPackage = GetOrCreateGroup(groupName);
            return assetPackage.LoadAssetAsync<TObject>(address);
        }

        /// <summary>
        /// 按 tag 创建热更下载计划。调用方传入 results，避免高频路径分配。
        /// </summary>
        public void CreateHotUpdatePlanByTagNonAlloc(
            string groupName,
            string tag,
            List<AssetLib233DownloadRequest> results)
        {
            AssetPackage233 assetPackage = GetOrCreateGroup(groupName);
            assetPackage.BuildDownloadRequestsNonAlloc(tag, results, _assetQueryCache);
        }

        /// <summary>
        /// 创建多 AssetGroup 统一下载操作。所有进度聚合为一条 Loading。
        /// </summary>
        public AssetLib233MultiGroupDownloadOperation CreateMultiGroupDownloadOperation(
            AssetLib233GroupDownloadBatch batch,
            IAssetLib233LoadingSink loadingSink)
        {
            AssetLib233MultiGroupDownloadOperation operation =
                new AssetLib233MultiGroupDownloadOperation(batch, loadingSink);
            operation.Start();
            _runningOperations.Add(operation);
            return operation;
        }

        public AssetLib233PreparePackageOperation CreatePreparePackageOperation(string groupName)
        {
            AssetLib233PreparePackageOperation operation = new AssetLib233PreparePackageOperation(groupName);
            operation.Start();
            _runningOperations.Add(operation);
            return operation;
        }

        public AssetLib233PackageDownloadOperation CreatePackageDownloadOperation(string groupName, string tag)
        {
            AssetLib233PackageDownloadOperation operation =
                new AssetLib233PackageDownloadOperation(groupName, tag);
            operation.Start();
            _runningOperations.Add(operation);
            return operation;
        }

        /// <summary>
        /// 捕获调试快照。调试窗口 / 运行时控制台 / 远端诊断都可以读取该结构。
        /// </summary>
        public AssetLib233DebugSnapshot CaptureDebugSnapshot()
        {
            AssetLib233DebugSnapshot snapshot = AssetLib233DebugService.CaptureSnapshot();
            FillDebugSnapshot(snapshot);
            return snapshot;
        }

        /// <summary>
        /// 资源系统主线程 Tick。建议由宿主在 Update 中调用，用于自动 Asset GC、下载器推进和后续 Provider 推进。
        /// </summary>
        public void Tick()
        {
            _assetGcService.Tick();
            for (int i = _runningOperations.Count - 1; i >= 0; i--)
            {
                AssetLib233Operation operation = _runningOperations[i];
                operation.Update();
                if (operation.IsDone)
                {
                    _runningOperations.RemoveAt(i);
                }
            }

            for (int i = 0; i < _packageList.Count; i++)
            {
                _packageList[i].Update();
            }
        }

        /// <summary>
        /// 手动资源 GC。force 为 true 时忽略宽限期，适合切大场景或退出玩法后调用。
        /// </summary>
        public void CollectAssets(bool force)
        {
            _assetGcService.Collect(force);
        }

        public void RetainAsset(string groupName, string address, Object assetObject)
        {
            _assetGcService.Retain(groupName, address, assetObject);
        }

        public void ReleaseAsset(string groupName, string address)
        {
            _assetGcService.Release(groupName, address);
        }

        /// <summary>
        /// 设置主线程异步操作时间片。WebGL / 小游戏可调大，减少资源初始化长尾等待。
        /// </summary>
        public void SetOperationTimeSlice(long milliseconds)
        {
            if (milliseconds <= 0)
            {
                Debug.LogError("[AssetLib233] OperationTimeSlice 必须大于 0");
                return;
            }

            AssetLib233RuntimeOptions.OperationTimeSliceMs = milliseconds;
        }

        /// <summary>
        /// 清空运行态缓存。仅用于测试或完整重启资源系统，不会删除磁盘缓存。
        /// </summary>
        public void ClearRuntimeCache()
        {
            _packages.Clear();
            _packageList.Clear();
            _runningOperations.Clear();
            _assetQueryCache.Clear();
            _downloadRequestCache.Clear();
            _assetGcService.Collect(true);
            _isInitialized = false;
        }

        public string BuildRuntimeDiagnosticString()
        {
            return AssetLib233RuntimeDiagnostic.BuildCopyableReport();
        }

        internal void FillDebugSnapshot(AssetLib233DebugSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            for (int i = 0; i < _packageList.Count; i++)
            {
                AssetLib233DebugGroupSnapshot groupSnapshot = new AssetLib233DebugGroupSnapshot();
                _packageList[i].FillDebugSnapshot(groupSnapshot);
                snapshot.Groups.Add(groupSnapshot);
            }
        }

        public static AssetPackage233 GetOrCreatePackage(string packageName)
        {
            return Instance.GetOrCreateGroup(packageName);
        }

        public static AssetHandle233<TObject> LoadAsync<TObject>(string groupName, string address) where TObject : Object
        {
            return Instance.LoadAssetAsync<TObject>(groupName, address);
        }
    }
}
