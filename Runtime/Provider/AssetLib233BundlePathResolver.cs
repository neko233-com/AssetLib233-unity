using System.IO;
using UnityEngine;

namespace AssetLib233.Runtime
{
    public static class AssetLib233BundlePathResolver
    {
        private const string CacheFolderName = "AssetLib233";

        public static string GetCacheRoot(string packageName)
        {
            if (AssetLib233.Instance.TryGetGroup(packageName, out AssetPackage233 assetPackage) &&
                assetPackage.Config != null)
            {
                IAssetLib233PlatformPlugin plugin =
                    AssetLib233PluginRegistry.GetPlugin(AssetLib233PlatformDetector.GetRuntimePlatform());
                return plugin.GetPersistentRootPath(assetPackage.Config);
            }

            return Path.Combine(Application.persistentDataPath, CacheFolderName, packageName);
        }

        public static string GetBuiltinRoot(string packageName)
        {
            return Path.Combine(Application.streamingAssetsPath, CacheFolderName, packageName);
        }

        public static string GetCacheBundlePath(string packageName, AssetBundleInfo233 bundleInfo)
        {
            if (bundleInfo == null)
            {
                return string.Empty;
            }

            return Path.Combine(GetCacheRoot(packageName), bundleInfo.FileName);
        }

        public static string GetBuiltinBundlePath(string packageName, AssetBundleInfo233 bundleInfo)
        {
            if (bundleInfo == null)
            {
                return string.Empty;
            }

            return Path.Combine(GetBuiltinRoot(packageName), bundleInfo.FileName);
        }

        public static bool IsWebPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            return path.Contains("://") || path.StartsWith("jar:file:");
        }
    }
}
