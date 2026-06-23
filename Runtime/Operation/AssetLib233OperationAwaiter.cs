using System;
using System.Runtime.CompilerServices;

namespace AssetLib233.Runtime
{
    /// <summary>
    /// C# async/await 适配器。等待逻辑由调用方继续 Tick operation，避免隐藏后台执行。
    /// </summary>
    public sealed class AssetLib233OperationAwaiter : INotifyCompletion
    {
        private readonly AssetLib233Operation _operation;

        public AssetLib233OperationAwaiter(AssetLib233Operation operation)
        {
            _operation = operation;
        }

        public bool IsCompleted
        {
            get { return _operation == null || _operation.IsDone; }
        }

        public void OnCompleted(Action continuation)
        {
            if (_operation == null)
            {
                continuation?.Invoke();
                return;
            }

            if (_operation.IsDone)
            {
                continuation?.Invoke();
                return;
            }

            _operation.Completed += delegate { continuation?.Invoke(); };
        }

        public AssetLib233Operation GetResult()
        {
            return _operation;
        }
    }
}
