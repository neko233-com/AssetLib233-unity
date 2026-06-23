namespace AssetLib233.Runtime
{
    /// <summary>
    /// 资源系统运行模式。
    /// EditorSimulate 用编辑器收集结果模拟运行；Host 从内置 + 远端缓存加载；Web 面向 WebGL / 小游戏；Offline 只读内置资源。
    /// EditorRemoteSimulation 在编辑器内模拟真机远端下载，不接业务 Loading。
    /// </summary>
    public enum EnumHotUpdateType
    {
        EditorSimulate = 0,
        Host = 1,
        Web = 2,
        Offline = 3,
        EditorRemoteSimulation = 4
    }
}
