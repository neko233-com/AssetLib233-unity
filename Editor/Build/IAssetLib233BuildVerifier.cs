namespace AssetLib233.Editor
{
    /// <summary>
    /// 打包产物校验接口。用于确认 AB、Manifest、依赖、hash、crc、size 是否完整，确保能替代原有资源系统。
    /// </summary>
    public interface IAssetLib233BuildVerifier
    {
        bool Verify(AssetLib233BuildVerifyContext context, out string error);
    }
}
