using System;
using System.Collections.Generic;

namespace AssetLib233.Editor
{
    /// <summary>
    /// CDN provider 工厂。
    /// 内置火山、阿里、腾讯、AWS；用户可在 Editor 启动时 Register 自定义 provider。
    /// </summary>
    public static class AssetLib233CdnProviderFactory
    {
        private static readonly Dictionary<EnumAssetLib233CdnProvider, IAssetLib233CdnProvider> _providers =
            new Dictionary<EnumAssetLib233CdnProvider, IAssetLib233CdnProvider>(8);

        static AssetLib233CdnProviderFactory()
        {
            Register(new AssetLib233VolcengineChinaCdnProvider());
            Register(new AssetLib233AliyunCdnProvider());
            Register(new AssetLib233TencentCloudCdnProvider());
            Register(new AssetLib233AwsCdnProvider());
            Register(new AssetLib233ExternalCommandCdnProvider(EnumAssetLib233CdnProvider.Custom, "Custom"));
        }

        public static void Register(IAssetLib233CdnProvider provider)
        {
            if (provider == null)
            {
                return;
            }

            _providers[provider.ProviderType] = provider;
        }

        public static IAssetLib233CdnProvider Create(AssetLib233EditorPublishLocalConfig config)
        {
            EnumAssetLib233CdnProvider providerType = ResolveProviderType(config);
            if (_providers.TryGetValue(providerType, out IAssetLib233CdnProvider provider))
            {
                return provider;
            }

            return _providers[EnumAssetLib233CdnProvider.VolcengineChina];
        }

        private static EnumAssetLib233CdnProvider ResolveProviderType(AssetLib233EditorPublishLocalConfig config)
        {
            if (config == null || string.IsNullOrWhiteSpace(config.cdnProvider))
            {
                return EnumAssetLib233CdnProvider.VolcengineChina;
            }

            if (Enum.TryParse(config.cdnProvider, true, out EnumAssetLib233CdnProvider providerType))
            {
                return providerType;
            }

            return EnumAssetLib233CdnProvider.VolcengineChina;
        }
    }
}
