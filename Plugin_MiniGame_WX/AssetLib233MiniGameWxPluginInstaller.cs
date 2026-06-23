using AssetLib233.Runtime;
using UnityEngine;

namespace AssetLib233.Plugin_MiniGame_WX
{
    public static class AssetLib233MiniGameWxPluginInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InstallOnRuntimeLoad()
        {
            AssetLib233PluginRegistry.Register(new AssetLib233MiniGameWxPlugin());
        }
    }
}
