using UnityEngine;

namespace AssetLib233.Runtime
{
    public static class AssetLib233PlatformDetector
    {
        public static EnumAssetLib233RuntimePlatform GetRuntimePlatform()
        {
#if UNITY_EDITOR
            return EnumAssetLib233RuntimePlatform.Editor;
#elif WX
            return EnumAssetLib233RuntimePlatform.WechatMiniGame;
#elif (UNITY_WEBGL || UNITY_MINIGAME) && (DOUYIN || TT || DOUYINMINIGAME)
            return EnumAssetLib233RuntimePlatform.DouyinMiniGame;
#elif (UNITY_WEBGL || UNITY_MINIGAME) && TAPMINIGAME
            return EnumAssetLib233RuntimePlatform.TapTapMiniGame;
#elif UNITY_WEBGL
            return EnumAssetLib233RuntimePlatform.WebGL;
#elif UNITY_IOS
            return EnumAssetLib233RuntimePlatform.IOS;
#elif UNITY_ANDROID
            return EnumAssetLib233RuntimePlatform.Android;
#elif UNITY_PS5
            return EnumAssetLib233RuntimePlatform.PS5;
#elif UNITY_STANDALONE_WIN
            return EnumAssetLib233RuntimePlatform.Windows;
#elif UNITY_STANDALONE_OSX
            return EnumAssetLib233RuntimePlatform.MacOS;
#elif UNITY_STANDALONE_LINUX
            return EnumAssetLib233RuntimePlatform.Linux;
#else
            Debug.LogError("[AssetLib233] 未识别运行平台");
            return EnumAssetLib233RuntimePlatform.Unknown;
#endif
        }

        public static bool IsMiniGame(EnumAssetLib233RuntimePlatform runtimePlatform)
        {
            return runtimePlatform == EnumAssetLib233RuntimePlatform.WechatMiniGame ||
                   runtimePlatform == EnumAssetLib233RuntimePlatform.DouyinMiniGame ||
                   runtimePlatform == EnumAssetLib233RuntimePlatform.TapTapMiniGame;
        }
    }
}
