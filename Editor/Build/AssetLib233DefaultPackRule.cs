using System.IO;
using AssetLib233.Runtime;

namespace AssetLib233.Editor
{
    public sealed class AssetLib233DefaultPackRule : IAssetLib233BuildPackRule
    {
        public string GetBundleName(AssetGroup233 group, AssetCollector233 collector, string assetPath)
        {
            string groupName = AssetLib233NameUtility.NormalizePackageName(group.GroupName).ToLowerInvariant();
            string collectorName = string.IsNullOrEmpty(collector.CollectorName)
                ? "collector"
                : collector.CollectorName.ToLowerInvariant();

            switch (collector.PackRule)
            {
                case EnumAssetLib233CollectorPackRule.PackSeparately:
                {
                    return groupName + "_" + NormalizePathToBundleName(Path.ChangeExtension(assetPath, null)) + ".ab";
                }
                case EnumAssetLib233CollectorPackRule.PackByTopDirectory:
                {
                    return groupName + "_" + collectorName + "_" + GetDirectorySegment(collector.AssetRootPath, assetPath, 0) + ".ab";
                }
                case EnumAssetLib233CollectorPackRule.PackBySecondDirectory:
                {
                    return groupName + "_" + collectorName + "_" + GetDirectorySegment(collector.AssetRootPath, assetPath, 1) + ".ab";
                }
                case EnumAssetLib233CollectorPackRule.PackByFileExtension:
                {
                    string extension = Path.GetExtension(assetPath);
                    if (string.IsNullOrEmpty(extension))
                    {
                        extension = "noext";
                    }

                    return groupName + "_" + collectorName + "_" + NormalizePathToBundleName(extension.TrimStart('.')) + ".ab";
                }
                case EnumAssetLib233CollectorPackRule.PackTogether:
                default:
                {
                    return groupName + "_" + NormalizePathToBundleName(collectorName) + ".ab";
                }
            }
        }

        private static string GetDirectorySegment(string rootPath, string assetPath, int index)
        {
            string safeRoot = string.IsNullOrEmpty(rootPath) ? string.Empty : rootPath.Replace('\\', '/').TrimEnd('/');
            string safeAssetPath = string.IsNullOrEmpty(assetPath) ? string.Empty : assetPath.Replace('\\', '/');
            string relativePath = safeAssetPath;
            if (!string.IsNullOrEmpty(safeRoot) && safeAssetPath.StartsWith(safeRoot))
            {
                relativePath = safeAssetPath.Substring(safeRoot.Length).TrimStart('/');
            }

            string[] parts = relativePath.Split('/');
            if (parts.Length <= index)
            {
                return "root";
            }

            return NormalizePathToBundleName(parts[index]);
        }

        private static string NormalizePathToBundleName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "empty";
            }

            string result = value.Replace('\\', '_')
                .Replace('/', '_')
                .Replace(' ', '_')
                .Replace('.', '_')
                .Replace('-', '_')
                .ToLowerInvariant();
            return result;
        }
    }
}
