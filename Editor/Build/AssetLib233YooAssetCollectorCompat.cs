using System.Collections.Generic;
using System.IO;
using AssetLib233.Runtime;
using UnityEngine;

namespace AssetLib233.Editor
{
    /// <summary>
    /// 旧 YooAsset Collector 配置兼容转换。
    /// 这里故意只读 YAML 文本，不引用 YooAsset.Editor，确保 YooAsset 删除后仍能构建。
    /// </summary>
    internal static class AssetLib233YooAssetCollectorCompat
    {
        public const string DefaultCollectorSettingPath = "Assets/AssetBundleCollectorSetting.asset";

        private sealed class LegacyCollectorData
        {
            public string PackageName;
            public string GroupName;
            public string CollectPath;
            public string AddressRuleName;
            public string PackRuleName;
            public string FilterRuleName;
            public string AssetTags;
            public int CollectorType;
            public bool GroupEnabled;
        }

        public static bool TryCreateProfile(
            string collectorSettingPath,
            string[] packageNameFilters,
            out AssetBuildProfile233 profile,
            out string error)
        {
            profile = null;
            error = string.Empty;
            if (string.IsNullOrEmpty(collectorSettingPath))
            {
                error = "旧 Collector 配置路径为空";
                return false;
            }

            if (!File.Exists(collectorSettingPath))
            {
                error = "旧 Collector 配置不存在: " + collectorSettingPath;
                return false;
            }

            string[] lines = File.ReadAllLines(collectorSettingPath);
            AssetBuildProfile233 result = ScriptableObject.CreateInstance<AssetBuildProfile233>();
            result.name = "AssetLib233_LegacyYooAssetCollector_RuntimeProfile";
            Dictionary<string, AssetGroup233> packageGroups = new Dictionary<string, AssetGroup233>(16);
            Dictionary<string, int> collectorNameCount = new Dictionary<string, int>(64);
            string currentPackageName = string.Empty;
            string currentGroupName = string.Empty;
            bool currentGroupEnabled = true;
            LegacyCollectorData currentCollector = null;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.StartsWith("  - PackageName: "))
                {
                    FlushCollector(result, packageGroups, collectorNameCount, currentCollector);
                    currentCollector = null;
                    currentPackageName = ParseYamlScalar(line);
                    currentGroupName = string.Empty;
                    currentGroupEnabled = true;
                    if (IsPackageSelected(currentPackageName, packageNameFilters))
                    {
                        EnsurePackageGroup(result, packageGroups, currentPackageName);
                    }

                    continue;
                }

                if (line.StartsWith("    - GroupName: "))
                {
                    FlushCollector(result, packageGroups, collectorNameCount, currentCollector);
                    currentCollector = null;
                    currentGroupName = ParseYamlScalar(line);
                    currentGroupEnabled = true;
                    continue;
                }

                if (line.StartsWith("      ActiveRuleName: "))
                {
                    string activeRuleName = ParseYamlScalar(line);
                    currentGroupEnabled = string.IsNullOrEmpty(activeRuleName)
                                          || string.Equals(activeRuleName, "EnableGroup", System.StringComparison.Ordinal);
                    continue;
                }

                if (line.StartsWith("      - CollectPath: "))
                {
                    FlushCollector(result, packageGroups, collectorNameCount, currentCollector);
                    currentCollector = null;
                    if (!IsPackageSelected(currentPackageName, packageNameFilters))
                    {
                        continue;
                    }

                    currentCollector = new LegacyCollectorData();
                    currentCollector.PackageName = currentPackageName;
                    currentCollector.GroupName = currentGroupName;
                    currentCollector.CollectPath = ParseYamlScalar(line);
                    currentCollector.AddressRuleName = "AddressByFileName";
                    currentCollector.PackRuleName = "PackCollector";
                    currentCollector.FilterRuleName = "CollectAll";
                    currentCollector.GroupEnabled = currentGroupEnabled;
                    currentCollector.CollectorType = 0;
                    continue;
                }

                if (currentCollector == null || !line.StartsWith("        "))
                {
                    continue;
                }

                if (line.StartsWith("        CollectorType: "))
                {
                    int collectorType = 0;
                    int.TryParse(ParseYamlScalar(line), out collectorType);
                    currentCollector.CollectorType = collectorType;
                }
                else if (line.StartsWith("        AddressRuleName: "))
                {
                    currentCollector.AddressRuleName = ParseYamlScalar(line);
                }
                else if (line.StartsWith("        PackRuleName: "))
                {
                    currentCollector.PackRuleName = ParseYamlScalar(line);
                }
                else if (line.StartsWith("        FilterRuleName: "))
                {
                    currentCollector.FilterRuleName = ParseYamlScalar(line);
                }
                else if (line.StartsWith("        AssetTags: "))
                {
                    currentCollector.AssetTags = ParseYamlScalar(line);
                }
            }

            FlushCollector(result, packageGroups, collectorNameCount, currentCollector);
            if (result.Groups.Count == 0)
            {
                error = "旧 Collector 配置没有转换出任何 AssetGroup";
                Object.DestroyImmediate(result);
                return false;
            }

            profile = result;
            return true;
        }

        private static void FlushCollector(
            AssetBuildProfile233 profile,
            Dictionary<string, AssetGroup233> packageGroups,
            Dictionary<string, int> collectorNameCount,
            LegacyCollectorData data)
        {
            if (profile == null || data == null || string.IsNullOrEmpty(data.PackageName))
            {
                return;
            }

            AssetGroup233 group = EnsurePackageGroup(profile, packageGroups, data.PackageName);
            if (group == null)
            {
                return;
            }

            AssetCollector233 collector = new AssetCollector233();
            collector.AssetRootPath = data.CollectPath;
            collector.AddressRuleName = data.AddressRuleName;
            collector.FilterRuleName = data.FilterRuleName;
            collector.LegacyPackRuleName = data.PackRuleName;
            collector.TagText = data.AssetTags;
            collector.Enabled = data.GroupEnabled && data.CollectorType == 0;
            collector.PackRule = ResolvePackRule(data.PackRuleName);
            collector.CollectorName = ResolveCollectorName(data, collectorNameCount);
            group.AddCollector(collector);
        }

        private static AssetGroup233 EnsurePackageGroup(
            AssetBuildProfile233 profile,
            Dictionary<string, AssetGroup233> packageGroups,
            string packageName)
        {
            if (profile == null || packageGroups == null || string.IsNullOrEmpty(packageName))
            {
                return null;
            }

            string safePackageName = AssetLib233NameUtility.NormalizePackageName(packageName);
            if (packageGroups.TryGetValue(safePackageName, out AssetGroup233 group))
            {
                return group;
            }

            group = new AssetGroup233();
            group.GroupName = safePackageName;
            group.Builtin = string.Equals(safePackageName, "login", System.StringComparison.Ordinal);
            group.RequiredOnFirstEnter = string.Equals(safePackageName, "default", System.StringComparison.Ordinal)
                                         || string.Equals(safePackageName, "story", System.StringComparison.Ordinal);
            packageGroups.Add(safePackageName, group);
            profile.AddGroup(group);
            return group;
        }

        private static EnumAssetLib233CollectorPackRule ResolvePackRule(string packRuleName)
        {
            if (string.Equals(packRuleName, "PackByTopDirectory", System.StringComparison.Ordinal))
            {
                return EnumAssetLib233CollectorPackRule.PackByTopDirectory;
            }

            if (string.Equals(packRuleName, "PackBySecondDirectory", System.StringComparison.Ordinal))
            {
                return EnumAssetLib233CollectorPackRule.PackBySecondDirectory;
            }

            if (string.Equals(packRuleName, "PackSeparately", System.StringComparison.Ordinal))
            {
                return EnumAssetLib233CollectorPackRule.PackSeparately;
            }

            if (string.Equals(packRuleName, "PackByFileExtension", System.StringComparison.Ordinal))
            {
                return EnumAssetLib233CollectorPackRule.PackByFileExtension;
            }

            return EnumAssetLib233CollectorPackRule.PackTogether;
        }

        private static string ResolveCollectorName(LegacyCollectorData data, Dictionary<string, int> collectorNameCount)
        {
            string groupName = string.IsNullOrEmpty(data.GroupName) ? "Collector" : data.GroupName;
            string baseName = groupName;
            if (string.Equals(data.PackRuleName, "PackCollector", System.StringComparison.Ordinal))
            {
                baseName = groupName;
            }
            else if (string.Equals(data.PackRuleName, "PackGroup", System.StringComparison.Ordinal))
            {
                baseName = groupName;
            }

            string packageName = string.IsNullOrEmpty(data.PackageName) ? "default" : data.PackageName;
            string countKey = packageName + "/" + baseName;
            if (!collectorNameCount.TryGetValue(countKey, out int count))
            {
                collectorNameCount.Add(countKey, 1);
                return baseName;
            }

            collectorNameCount[countKey] = count + 1;
            string leafName = GetPathLeafName(data.CollectPath);
            if (string.IsNullOrEmpty(leafName))
            {
                leafName = count.ToString();
            }

            return baseName + "_" + leafName;
        }

        private static string GetPathLeafName(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            string safePath = path.Replace('\\', '/').TrimEnd('/');
            int slashIndex = safePath.LastIndexOf('/');
            if (slashIndex < 0 || slashIndex >= safePath.Length - 1)
            {
                return safePath;
            }

            return safePath.Substring(slashIndex + 1);
        }

        private static bool IsPackageSelected(string packageName, string[] packageNameFilters)
        {
            if (packageNameFilters == null || packageNameFilters.Length == 0)
            {
                return true;
            }

            for (int i = 0; i < packageNameFilters.Length; i++)
            {
                if (string.Equals(packageName, packageNameFilters[i], System.StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static string ParseYamlScalar(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                return string.Empty;
            }

            int colonIndex = line.IndexOf(':');
            if (colonIndex < 0 || colonIndex >= line.Length - 1)
            {
                return string.Empty;
            }

            string value = line.Substring(colonIndex + 1).Trim();
            if (value.Length >= 2 && value[0] == '"' && value[value.Length - 1] == '"')
            {
                return value.Substring(1, value.Length - 2);
            }

            return value;
        }
    }
}
