namespace AssetLib233.Runtime
{
    /// <summary>
    /// 启动场景 facade。
    /// AppRoot / CodeHotUpdate 可以先调用这里，而不直接操作底层 Package / Manifest。
    /// </summary>
    public static class AssetLib233StartupFacade
    {
        public static AssetLib233LoginFastOperation CreateLoginFastOperation(AssetLib233StartupPlan startupPlan)
        {
            AssetLib233LoginFastOperation operation = new AssetLib233LoginFastOperation(startupPlan);
            operation.Start();
            return operation;
        }

        public static AssetLib233PostLoginGroupsOperation CreatePostLoginGroupsOperation(AssetLib233StartupPlan startupPlan)
        {
            AssetLib233PostLoginGroupsOperation operation = new AssetLib233PostLoginGroupsOperation(startupPlan);
            operation.Start();
            return operation;
        }
    }
}
