using AssetLib233.Runtime;
using UnityEngine;

namespace AssetLib233.Plugin_MiniGame_WX
{
    /// <summary>
    /// 微信小游戏平台插件。
    /// 只处理微信宿主相关能力：下载并发、小游戏缓存根路径、首选加载方式。
    /// 这里刻意不直接引用微信 SDK 静态入口：不同微信 SDK 版本的 env API 差异较大，
    /// Unity 在小游戏平台会把 Application.persistentDataPath 映射到可写目录，先保证全平台可编译。
    /// </summary>
    public sealed class AssetLib233MiniGameWxPlugin : IAssetLib233PlatformPlugin
    {
        public EnumAssetLib233RuntimePlatform RuntimePlatform
        {
            get { return EnumAssetLib233RuntimePlatform.WechatMiniGame; }
        }

        public void BeforeInitializePackage(AssetLib233PackageConfig config)
        {
            AssetLib233RuntimeOptions.DownloadConcurrency = AssetLib233DownloadPolicy.MaxDownloadConcurrency;
        }

        public string GetPersistentRootPath(AssetLib233PackageConfig config)
        {
            return Application.persistentDataPath + "/AssetLib233_WX/" + config.PackageName;
        }

        public EnumAssetLib233LoadMethod GetPreferredLoadMethod(AssetLib233PackageConfig config)
        {
            return EnumAssetLib233LoadMethod.MiniGameSdk;
        }

        public IAssetLib233DownloadTransport CreateDownloadTransport(AssetLib233PackageConfig config)
        {
            return new AssetLib233MiniGameWxDownloadTransport();
        }
    }
}
