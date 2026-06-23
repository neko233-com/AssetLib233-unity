namespace AssetLib233.Runtime
{
    /// <summary>
    /// AssetBundle 压缩策略。
    /// MiniGame 推荐 LZ4，避免 WebGL / 小游戏平台运行时不兼容和解压峰值。
    /// </summary>
    public enum EnumAssetLib233CompressionMode
    {
        Uncompressed = 0,
        Lz4 = 1,
        Lzma = 2,
        PlatformDefault = 3
    }
}
