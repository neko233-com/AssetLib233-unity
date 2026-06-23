namespace AssetLib233.Runtime
{
    /// <summary>
    /// AssetGroup 在启动链路中的职责。
    /// LoginFast：登录界面可见前必须先准备的首组。
    /// RequiredPostLogin：登录成功后、进主城或选角前必须完成的资源组。
    /// OptionalBackground：玩法入口或后台低优先级资源组，不能抢首进 Required 带宽。
    /// </summary>
    public enum EnumAssetLib233GroupRole
    {
        LoginFast = 0,
        RequiredPostLogin = 1,
        OptionalBackground = 2
    }
}
