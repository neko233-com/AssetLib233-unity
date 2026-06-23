namespace AssetLib233.Runtime
{
    /// <summary>
    /// 资源 GC 模式。
    /// Manual 只手动回收；Auto 按间隔扫描；AutoAndManual 同时支持自动和业务主动触发。
    /// </summary>
    public enum EnumAssetLib233GcMode
    {
        Manual = 0,
        Auto = 1,
        AutoAndManual = 2
    }
}
