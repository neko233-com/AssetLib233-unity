namespace AssetLib233.Runtime
{
    /// <summary>
    /// 热更流水线。
    /// 步骤：请求版本 -> 加载二进制 Manifest -> 写入 AssetGroup -> 生成下载计划。
    /// 下载执行由平台 Downloader 负责，本类只管确定“需要下什么”。
    /// </summary>
    public sealed class AssetLib233HotUpdatePipeline
    {
        private readonly IAssetLib233VersionService _versionService;
        private readonly IAssetLib233ManifestLoader _manifestLoader;

        public AssetLib233HotUpdatePipeline(
            IAssetLib233VersionService versionService,
            IAssetLib233ManifestLoader manifestLoader)
        {
            _versionService = versionService;
            _manifestLoader = manifestLoader;
        }

        public bool TryBuildPlan(string groupName, string tag, AssetLib233HotUpdatePlan plan, out string error)
        {
            error = string.Empty;
            if (plan == null)
            {
                error = "HotUpdatePlan 为空";
                return false;
            }

            plan.Clear();
            if (_versionService == null)
            {
                error = "VersionService 为空";
                return false;
            }

            if (_manifestLoader == null)
            {
                error = "ManifestLoader 为空";
                return false;
            }

            if (!_versionService.TryGetRemoteVersion(groupName, out AssetLib233RemoteVersionInfo versionInfo, out error))
            {
                return false;
            }

            if (!_manifestLoader.TryLoadManifest(versionInfo, out AssetManifest233 manifest, out error))
            {
                return false;
            }

            AssetLib233.Instance.SetManifest(groupName, manifest);
            plan.GroupName = groupName;
            plan.ToVersion = versionInfo.PackageVersion;
            plan.NeedUpdateManifest = true;

            System.Collections.Generic.List<AssetLib233DownloadRequest> requests =
                new System.Collections.Generic.List<AssetLib233DownloadRequest>(256);
            AssetLib233.Instance.CreateHotUpdatePlanByTagNonAlloc(groupName, tag, requests);
            for (int i = 0; i < requests.Count; i++)
            {
                plan.AddDownloadRequest(requests[i]);
            }

            return true;
        }
    }
}
