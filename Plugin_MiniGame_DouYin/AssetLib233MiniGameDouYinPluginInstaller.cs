using AssetLib233.Runtime;
using UnityEngine;

namespace AssetLib233.Plugin_MiniGame_DouYin
{
    public static class AssetLib233MiniGameDouYinPluginInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InstallOnRuntimeLoad()
        {
            AssetLib233PluginRegistry.Register(new AssetLib233MiniGameDouYinPlugin());
        }
    }
}
