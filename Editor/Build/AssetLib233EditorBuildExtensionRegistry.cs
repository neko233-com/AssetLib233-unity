using AssetLib233.Runtime;

namespace AssetLib233.Editor
{
    /// <summary>
    /// 构建扩展注册表。
    /// 业务工程可以在 Editor 初始化时注册自己的打包规则、压缩策略、产物校验器。
    /// </summary>
    public static class AssetLib233EditorBuildExtensionRegistry
    {
        private static IAssetLib233BuildPackRule _packRule;
        private static IAssetLib233BuildCompressionStrategy _compressionStrategy;
        private static IAssetLib233BuildVerifier _verifier;

        public static void RegisterPackRule(IAssetLib233BuildPackRule packRule)
        {
            _packRule = packRule;
        }

        public static void RegisterCompressionStrategy(IAssetLib233BuildCompressionStrategy compressionStrategy)
        {
            _compressionStrategy = compressionStrategy;
        }

        public static void RegisterVerifier(IAssetLib233BuildVerifier verifier)
        {
            _verifier = verifier;
        }

        public static void Clear()
        {
            _packRule = null;
            _compressionStrategy = null;
            _verifier = null;
        }

        internal static IAssetLib233BuildPackRule ResolvePackRule(IAssetLib233BuildPackRule defaultPackRule)
        {
            return _packRule == null ? defaultPackRule : _packRule;
        }

        internal static EnumAssetLib233CompressionMode ResolveCompressionMode(
            string platformName,
            string assetGroupName,
            string collectorName)
        {
            if (_compressionStrategy == null)
            {
                return EnumAssetLib233CompressionMode.Lz4;
            }

            EnumAssetLib233CompressionMode compressionMode =
                _compressionStrategy.GetCompressionMode(platformName, assetGroupName, collectorName);
            if (compressionMode == EnumAssetLib233CompressionMode.PlatformDefault)
            {
                return EnumAssetLib233CompressionMode.Lz4;
            }

            return compressionMode;
        }

        internal static IAssetLib233BuildVerifier ResolveVerifier(IAssetLib233BuildVerifier defaultVerifier)
        {
            return _verifier == null ? defaultVerifier : _verifier;
        }
    }
}
