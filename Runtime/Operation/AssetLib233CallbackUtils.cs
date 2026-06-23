namespace AssetLib233.Runtime
{
    /// <summary>
    /// callback 便捷工具，方便业务用链式写法注册完成回调。
    /// </summary>
    public static class AssetLib233CallbackUtils
    {
        public static TOperation OnCompleted<TOperation>(
            this TOperation operation,
            System.Action<TOperation> callback)
            where TOperation : AssetLib233Operation
        {
            if (operation == null || callback == null)
            {
                return operation;
            }

            operation.Completed += delegate { callback(operation); };
            return operation;
        }
    }
}
