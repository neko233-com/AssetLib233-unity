using AssetLib233.Runtime;
using UnityEngine;

#if (UNITY_WEBGL || UNITY_MINIGAME) && TAPMINIGAME
using TapTapMiniGame;
#endif

namespace AssetLib233.Plugin_MiniGame_TapTap
{
    /// <summary>
    /// TapTap 小游戏平台插件。
    /// 只在 TAPMINIGAME 宏下读取 Tap.env.USER_DATA_PATH；其它平台安全回落 persistentDataPath。
    /// </summary>
    public sealed class AssetLib233MiniGameTapTapPlugin : IAssetLib233PlatformPlugin
    {
        public EnumAssetLib233RuntimePlatform RuntimePlatform
        {
            get { return EnumAssetLib233RuntimePlatform.TapTapMiniGame; }
        }

        public void BeforeInitializePackage(AssetLib233PackageConfig config)
        {
            AssetLib233RuntimeOptions.DownloadConcurrency = AssetLib233DownloadPolicy.MaxDownloadConcurrency;
        }

        public string GetPersistentRootPath(AssetLib233PackageConfig config)
        {
#if (UNITY_WEBGL || UNITY_MINIGAME) && TAPMINIGAME
            return Tap.env.USER_DATA_PATH + "/AssetLib233_TapTap/" + config.PackageName;
#else
            return Application.persistentDataPath + "/AssetLib233_TapTap/" + config.PackageName;
#endif
        }

        public EnumAssetLib233LoadMethod GetPreferredLoadMethod(AssetLib233PackageConfig config)
        {
            return EnumAssetLib233LoadMethod.MiniGameSdk;
        }

        public IAssetLib233DownloadTransport CreateDownloadTransport(AssetLib233PackageConfig config)
        {
            return new AssetLib233UnityWebRequestDownloadTransport();
        }
    }
}
