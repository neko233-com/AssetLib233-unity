namespace AssetLib233.Runtime
{
    /// <summary>
    /// login 快速首组操作。
    /// 目标是尽快让登录界面所需 AssetGroup 可用；不等待 default / story 等后续组。
    /// </summary>
    public sealed class AssetLib233LoginFastOperation : AssetLib233Operation
    {
        private readonly AssetLib233StartupPlan _startupPlan;

        public AssetLib233LoginFastOperation(AssetLib233StartupPlan startupPlan)
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

            if (_startupPlan.LoginGroupConfig == null)
            {
                SetFailed("LoginGroupConfig 为空");
                return;
            }

            _startupPlan.LoadingSink?.OnAssetLib233StageChanged("LoginGroup", "初始化 login 快速资源组");
            AssetLib233.Instance.InitializeGroup(_startupPlan.LoginGroupConfig);
            _startupPlan.LoadingSink?.OnAssetLib233ProgressChanged(1f, 0, 0);
            SetSucceed();
        }

        protected override void OnUpdate()
        {
        }
    }
}
