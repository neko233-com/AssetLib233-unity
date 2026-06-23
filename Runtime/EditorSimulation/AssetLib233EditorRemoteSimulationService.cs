using UnityEngine;

namespace AssetLib233.Runtime
{
    /// <summary>
    /// 编辑器真机下载模拟服务。
    /// Runtime 保留轻量配置入口，Editor 工具和下载器读取配置后跑完整远端链路。
    /// </summary>
    public static class AssetLib233EditorRemoteSimulationService
    {
        private static readonly AssetLib233EditorRemoteSimulationConfig _config =
            new AssetLib233EditorRemoteSimulationConfig();

        public static AssetLib233EditorRemoteSimulationConfig Config
        {
            get { return _config; }
        }

        public static void Enable(string cacheRoot)
        {
            _config.Enabled = true;
            _config.CacheRoot = string.IsNullOrEmpty(cacheRoot)
                ? Application.temporaryCachePath + "/AssetLib233EditorRemoteSimulation"
                : cacheRoot;
        }

        public static void Disable()
        {
            _config.Enabled = false;
        }
    }
}
