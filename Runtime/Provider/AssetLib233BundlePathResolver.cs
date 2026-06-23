using System.IO;
using UnityEngine;

namespace AssetLib233.Runtime
{
    public static class AssetLib233BundlePathResolver
    {
        private const string CacheFolderName = "AssetLib233";

        public static string GetCacheRoot(string packageName)
        {
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
