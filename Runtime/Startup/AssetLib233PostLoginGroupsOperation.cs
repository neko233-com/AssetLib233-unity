using System.Collections.Generic;

namespace AssetLib233.Runtime
{
    /// <summary>
    /// 登录后 N 个 AssetGroup 初始化 / 下载计划操作。
    /// 业务 Loading 暂不接入也没关系；没有 LoadingSink 时静默执行。
    /// </summary>
    public sealed class AssetLib233PostLoginGroupsOperation : AssetLib233Operation
    {
        private readonly AssetLib233StartupPlan _startupPlan;
        private readonly List<AssetLib233PackageConfig> _groupCache = new List<AssetLib233PackageConfig>(8);
        private int _currentIndex;

        public AssetLib233PostLoginGroupsOperation(AssetLib233StartupPlan startupPlan)
        {
            _startupPlan = startupPlan;
        }

        protected override void OnStart()
        {
            if (_startupPlan == null)
            {
                SetFailed("启动计划为空");
                return;
            }

            _startupPlan.GetPostLoginGroupsNonAlloc(_groupCache);
            if (_groupCache.Count == 0)
            {
                SetSucceed();
                return;
            }

            _currentIndex = 0;
            Progress = 0f;
            _startupPlan.LoadingSink?.OnAssetLib233StageChanged("PostLoginGroups", "开始初始化登录后资源组");
        }

        protected override void OnUpdate()
        {
            if (_currentIndex >= _groupCache.Count)
            {
                _startupPlan.LoadingSink?.OnAssetLib233ProgressChanged(1f, 0, 0);
                SetSucceed();
                return;
            }

            AssetLib233PackageConfig config = _groupCache[_currentIndex];
            AssetLib233.Instance.InitializeGroup(config);

            _currentIndex++;
            Progress = (float)_currentIndex / _groupCache.Count;
            _startupPlan.LoadingSink?.OnAssetLib233StageChanged(
                "PostLoginGroups",
                "已初始化资源组: " + config.PackageName);
            _startupPlan.LoadingSink?.OnAssetLib233ProgressChanged(Progress, 0, 0);
        }
    }
}
