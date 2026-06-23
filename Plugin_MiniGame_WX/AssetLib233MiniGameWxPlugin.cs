using AssetLib233.Runtime;
using UnityEngine;

#if WX
using WeChatWASM;
#endif

namespace AssetLib233.Plugin_MiniGame_WX
{
    /// <summary>
    /// 微信小游戏平台插件。
    /// 只处理微信宿主相关能力：下载并发、小游戏缓存根路径、首选加载方式。
    /// 不依赖第三方资源系统，不创建外部 FileSystem。
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
#if WX
            return WX.env.USER_DATA_PATH + "/AssetLib233_WX/" + config.PackageName;
#else
            return Application.persistentDataPath + "/AssetLib233_WX/" + config.PackageName;
#endif
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
