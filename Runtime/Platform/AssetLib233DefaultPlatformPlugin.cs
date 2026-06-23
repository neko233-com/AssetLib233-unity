using UnityEngine;

namespace AssetLib233.Runtime
{
    /// <summary>
    /// 默认平台插件。覆盖 PC / iOS / Android / PS5 / 普通 WebGL。
    /// </summary>
    public sealed class AssetLib233DefaultPlatformPlugin : IAssetLib233PlatformPlugin
    {
        public EnumAssetLib233RuntimePlatform RuntimePlatform
        {
            get { return EnumAssetLib233RuntimePlatform.Unknown; }
        }

        public void BeforeInitializePackage(AssetLib233PackageConfig config)
        {
            AssetLib233RuntimeOptions.DownloadConcurrency =
                AssetLib233DownloadPolicy.GetDownloadConcurrency(AssetLib233PlatformDetector.GetRuntimePlatform());
        }

        public string GetPersistentRootPath(AssetLib233PackageConfig config)
        {
            return Application.persistentDataPath + "/AssetLib233/" + config.PackageName;
        }

        public EnumAssetLib233LoadMethod GetPreferredLoadMethod(AssetLib233PackageConfig config)
        {
            if (config.PlayMode == EnumAssetLib233PlayMode.EditorRemoteSimulation)
            {
                return EnumAssetLib233LoadMethod.EditorRemoteSimulation;
            }

            if (config.PlayMode == EnumAssetLib233PlayMode.Web)
            {
                return EnumAssetLib233LoadMethod.WebRequest;
            }

            return EnumAssetLib233LoadMethod.LoadFromFile;
        }

        public IAssetLib233DownloadTransport CreateDownloadTransport(AssetLib233PackageConfig config)
        {
            return new AssetLib233UnityWebRequestDownloadTransport();
        }
    }
}
