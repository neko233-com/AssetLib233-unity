using AssetLib233.Runtime;
using UnityEngine;

namespace AssetLib233.Plugin_MiniGame_TapTap
{
    public static class AssetLib233MiniGameTapTapPluginInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InstallOnRuntimeLoad()
        {
            AssetLib233PluginRegistry.Register(new AssetLib233MiniGameTapTapPlugin());
        }
    }
}
