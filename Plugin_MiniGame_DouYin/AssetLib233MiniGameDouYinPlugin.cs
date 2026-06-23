using AssetLib233.Runtime;
using UnityEngine;

namespace AssetLib233.Plugin_MiniGame_DouYin
{
    /// <summary>
    /// 抖音小游戏平台插件。
    /// 当前保持无 SDK 裸引用，避免非抖音平台编译失败；实际下载器后续通过宏隔离接入 TTSDK。
    /// </summary>
    public sealed class AssetLib233MiniGameDouYinPlugin : IAssetLib233PlatformPlugin
    {
        public EnumAssetLib233RuntimePlatform RuntimePlatform
        {
            get { return EnumAssetLib233RuntimePlatform.DouyinMiniGame; }
        }

        public void BeforeInitializePackage(AssetLib233PackageConfig config)
        {
            AssetLib233RuntimeOptions.DownloadConcurrency = AssetLib233DownloadPolicy.MaxDownloadConcurrency;
        }

        public string GetPersistentRootPath(AssetLib233PackageConfig config)
        {
            return Application.persistentDataPath + "/AssetLib233_DouYin/" + config.PackageName;
        }

        public EnumAssetLib233LoadMethod GetPreferredLoadMethod(AssetLib233PackageConfig config)
        {
            return EnumAssetLib233LoadMethod.MiniGameSdk;
        }
    }
}
