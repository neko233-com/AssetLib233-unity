using System.Collections.Generic;

namespace AssetLib233.Runtime
{
    /// <summary>
    /// 启动计划。
    /// LoginGroupConfig 只描述首个快速 AssetGroup，例如 login。
    /// PostLoginGroupConfigs 描述登录后才硬等或后台拉取的 N 个 AssetGroup，例如 default / story / voice / cg。
    /// </summary>
    public sealed class AssetLib233StartupPlan
    {
        private readonly List<AssetLib233PackageConfig> _postLoginGroupConfigs =
            new List<AssetLib233PackageConfig>(8);

        public AssetLib233PackageConfig LoginGroupConfig;
        public IAssetLib233LoadingSink LoadingSink;

        public IReadOnlyList<AssetLib233PackageConfig> PostLoginGroupConfigs
        {
            get { return _postLoginGroupConfigs; }
        }

        public void AddPostLoginGroup(AssetLib233PackageConfig config)
        {
            if (config == null)
            {
                return;
            }

            _postLoginGroupConfigs.Add(config);
        }

        public void GetPostLoginGroupsNonAlloc(List<AssetLib233PackageConfig> results)
        {
            if (results == null)
            {
                return;
            }

            results.Clear();
            for (int i = 0; i < _postLoginGroupConfigs.Count; i++)
            {
                results.Add(_postLoginGroupConfigs[i]);
            }
        }
    }
}
