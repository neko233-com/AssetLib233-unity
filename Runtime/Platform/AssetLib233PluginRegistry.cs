using System.Collections.Generic;

namespace AssetLib233.Runtime
{
    public static class AssetLib233PluginRegistry
    {
        private static readonly Dictionary<EnumAssetLib233RuntimePlatform, IAssetLib233PlatformPlugin> _plugins =
            new Dictionary<EnumAssetLib233RuntimePlatform, IAssetLib233PlatformPlugin>(8);

        private static readonly IAssetLib233PlatformPlugin _defaultPlugin = new AssetLib233DefaultPlatformPlugin();
        private static bool _isDefaultInstalled;

        public static void Register(IAssetLib233PlatformPlugin plugin)
        {
            if (plugin == null)
            {
                return;
            }

            _plugins[plugin.RuntimePlatform] = plugin;
        }

        public static IAssetLib233PlatformPlugin GetPlugin(EnumAssetLib233RuntimePlatform runtimePlatform)
        {
            InstallDefaultPlugins();
            if (_plugins.TryGetValue(runtimePlatform, out IAssetLib233PlatformPlugin plugin))
            {
                return plugin;
            }

            return _defaultPlugin;
        }

        public static void InstallDefaultPlugins()
        {
            if (_isDefaultInstalled)
            {
                return;
            }

            AssetLib233BuiltInPluginInstaller.Install();
            _isDefaultInstalled = true;
        }
    }
}
