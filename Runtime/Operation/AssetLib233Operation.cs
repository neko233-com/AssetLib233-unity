namespace AssetLib233.Runtime
{
    /// <summary>
    /// AssetLib233 异步操作基类。
    /// 支持 callback，也支持 C# await（通过自定义 awaiter）。UniTask 支持放在 Plugin_UniTask，避免 Runtime 强绑定 UniTask。
    /// </summary>
    public abstract class AssetLib233Operation
    {
        public System.Action<AssetLib233Operation> Completed;

        public EnumAssetLib233OperationStatus Status { protected set; get; }
        public string Error { protected set; get; }
        public float Progress { protected set; get; }

        public bool IsDone
        {
            get
            {
                return Status == EnumAssetLib233OperationStatus.Succeed ||
                       Status == EnumAssetLib233OperationStatus.Failed;
            }
        }

        public void Start()
        {
            if (Status != EnumAssetLib233OperationStatus.None)
            {
                return;
            }

            Status = EnumAssetLib233OperationStatus.Processing;
            Progress = 0f;
            OnStart();
        }

        public void Update()
        {
            if (Status != EnumAssetLib233OperationStatus.Processing)
            {
                return;
            }

            OnUpdate();
        }

        protected void SetSucceed()
        {
            Status = EnumAssetLib233OperationStatus.Succeed;
            Progress = 1f;
            Completed?.Invoke(this);
        }

        protected void SetFailed(string error)
        {
            Error = error;
            Status = EnumAssetLib233OperationStatus.Failed;
            Completed?.Invoke(this);
        }

        public AssetLib233OperationAwaiter GetAwaiter()
        {
            return new AssetLib233OperationAwaiter(this);
        }

        protected abstract void OnStart();
        protected abstract void OnUpdate();
    }
}
