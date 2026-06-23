using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using AssetLib233.Runtime;
using UnityEditor;
using UnityEngine;

namespace AssetLib233.Editor
{
    /// <summary>
    /// AssetLib233 原生构建管线。
    /// 从 BuildProfile 收集资源，调用 Unity BuildPipeline，生成 AssetLib233 manifest/version，并执行产物校验。
    /// </summary>
    public static class AssetLib233EditorBuildPipeline
    {
        private static readonly List<AssetGroup233> _groupCache = new List<AssetGroup233>(8);
        private static readonly List<AssetCollector233> _collectorCache = new List<AssetCollector233>(16);
        private static readonly List<AssetLib233BuildAssetRecord> _assetRecords = new List<AssetLib233BuildAssetRecord>(2048);
        private static readonly List<AssetBundleInfo233> _bundleInfos = new List<AssetBundleInfo233>(256);
        private static readonly List<AssetInfo233> _assetInfos = new List<AssetInfo233>(2048);
        private static readonly List<string> _bundleNames = new List<string>(256);
        private static readonly HashSet<string> _collectedAssetPathSet = new HashSet<string>();
        private static readonly AssetLib233DefaultPackRule _defaultPackRule = new AssetLib233DefaultPackRule();
        private static readonly AssetLib233DefaultBuildVerifier _defaultVerifier = new AssetLib233DefaultBuildVerifier();

        public static bool BuildProfile(AssetBuildProfile233 profile, string outputRoot, BuildTarget buildTarget, out string error)
        {
            error = string.Empty;
            if (profile == null)
            {
                error = "BuildProfile 为空";
                return false;
            }

            if (string.IsNullOrEmpty(outputRoot))
            {
                error = "输出目录为空";
                return false;
            }

            Directory.CreateDirectory(outputRoot);
            profile.GetGroupsNonAlloc(_groupCache);
            bool isOk = true;
            for (int i = 0; i < _groupCache.Count; i++)
            {
                AssetGroup233 group = _groupCache[i];
                if (group == null)
                {
                    continue;
                }

                string groupOutputRoot = Path.Combine(outputRoot, AssetLib233NameUtility.NormalizePackageName(group.GroupName));
                if (!BuildGroup(group, groupOutputRoot, buildTarget, out string groupError))
                {
                    error = groupError;
                    isOk = false;
                    break;
                }
            }

            _groupCache.Clear();
            return isOk;
        }

        private static bool BuildGroup(AssetGroup233 group, string outputRoot, BuildTarget buildTarget, out string error)
        {
            error = string.Empty;
            Directory.CreateDirectory(outputRoot);
            _assetRecords.Clear();
            _bundleInfos.Clear();
            _assetInfos.Clear();
            _bundleNames.Clear();
            _collectedAssetPathSet.Clear();

            Dictionary<string, List<string>> bundleToAssets = new Dictionary<string, List<string>>(256);
            CollectGroupAssets(group, bundleToAssets);
            if (bundleToAssets.Count == 0)
            {
                error = "AssetGroup 没有收集到资源: " + group.GroupName;
                return false;
            }

            AssetBundleBuild[] builds = CreateAssetBundleBuilds(bundleToAssets);
            AssetBundleManifest unityManifest = BuildPipeline.BuildAssetBundles(
                outputRoot,
                builds,
                BuildAssetBundleOptions.ChunkBasedCompression,
                buildTarget);
            if (unityManifest == null)
            {
                error = "BuildPipeline.BuildAssetBundles 返回空. group = " + group.GroupName;
                return false;
            }

            AssetManifest233 manifest = CreateManifest(group, outputRoot, unityManifest);
            WriteManifestAndVersionFiles(manifest, outputRoot);
            AssetLib233BuildVerifyContext verifyContext = new AssetLib233BuildVerifyContext();
            verifyContext.platformName = buildTarget.ToString();
            verifyContext.outputRoot = outputRoot;
            verifyContext.manifest = manifest;
            if (!_defaultVerifier.Verify(verifyContext, out string verifyError))
            {
                error = verifyError;
                return false;
            }

            Debug.Log(
                "[AssetLib233] 构建完成. group = " +
                group.GroupName +
                " | output = " +
                outputRoot +
                " | assets = " +
                manifest.Assets.Count +
                " | bundles = " +
                manifest.Bundles.Count);
            return true;
        }

        private static void CollectGroupAssets(AssetGroup233 group, Dictionary<string, List<string>> bundleToAssets)
        {
            group.GetCollectorsNonAlloc(_collectorCache);
            for (int i = 0; i < _collectorCache.Count; i++)
            {
                AssetCollector233 collector = _collectorCache[i];
                if (collector == null || !collector.Enabled || string.IsNullOrEmpty(collector.AssetRootPath))
                {
                    continue;
                }

                string[] guids = AssetDatabase.FindAssets(string.Empty, new[] { collector.AssetRootPath });
                for (int j = 0; j < guids.Length; j++)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guids[j]);
                    if (string.IsNullOrEmpty(assetPath) || Directory.Exists(assetPath))
                    {
                        continue;
                    }

                    if (assetPath.EndsWith(".cs") || assetPath.EndsWith(".asmdef"))
                    {
                        continue;
                    }

                    if (_collectedAssetPathSet.Contains(assetPath))
                    {
                        continue;
                    }

                    _collectedAssetPathSet.Add(assetPath);
                    string bundleName = _defaultPackRule.GetBundleName(group, collector, assetPath);
                    AddAssetToBundle(bundleToAssets, bundleName, assetPath);
                    AssetLib233BuildAssetRecord record = new AssetLib233BuildAssetRecord();
                    record.Address = BuildAddress(collector, assetPath);
                    record.AssetPath = assetPath;
                    record.BundleName = bundleName;
                    record.Tags = SplitTags(collector.TagText);
                    _assetRecords.Add(record);
                }
            }

            _collectorCache.Clear();
        }

        private static void AddAssetToBundle(Dictionary<string, List<string>> bundleToAssets, string bundleName, string assetPath)
        {
            if (!bundleToAssets.TryGetValue(bundleName, out List<string> assets))
            {
                assets = new List<string>(32);
                bundleToAssets.Add(bundleName, assets);
                _bundleNames.Add(bundleName);
            }

            assets.Add(assetPath);
        }

        private static AssetBundleBuild[] CreateAssetBundleBuilds(Dictionary<string, List<string>> bundleToAssets)
        {
            AssetBundleBuild[] builds = new AssetBundleBuild[_bundleNames.Count];
            for (int i = 0; i < _bundleNames.Count; i++)
            {
                string bundleName = _bundleNames[i];
                List<string> assets = bundleToAssets[bundleName];
                AssetBundleBuild build = new AssetBundleBuild();
                build.assetBundleName = bundleName;
                build.assetNames = assets.ToArray();
                builds[i] = build;
            }

            return builds;
        }

        private static AssetManifest233 CreateManifest(AssetGroup233 group, string outputRoot, AssetBundleManifest unityManifest)
        {
            string groupName = AssetLib233NameUtility.NormalizePackageName(group.GroupName);
            for (int i = 0; i < _bundleNames.Count; i++)
            {
                string bundleName = _bundleNames[i];
                string filePath = Path.Combine(outputRoot, bundleName);
                FileInfo fileInfo = new FileInfo(filePath);
                AssetBundleInfo233 bundleInfo = new AssetBundleInfo233();
                bundleInfo.BundleName = bundleName;
                bundleInfo.FileName = bundleName;
                bundleInfo.FileHash = ComputeFileMd5(filePath);
                bundleInfo.FileSize = fileInfo.Exists ? fileInfo.Length : 0L;
                uint crc = 0;
                BuildPipeline.GetCRCForAssetBundle(filePath, out crc);
                bundleInfo.FileCrc = crc;
                bundleInfo.BundleType = EnumAssetLib233BundleType.AssetBundle;
                bundleInfo.SetDependBundleNames(unityManifest.GetDirectDependencies(bundleName));
                bundleInfo.SetTags(System.Array.Empty<string>());
                _bundleInfos.Add(bundleInfo);
            }

            for (int i = 0; i < _assetRecords.Count; i++)
            {
                AssetLib233BuildAssetRecord record = _assetRecords[i];
                AssetInfo233 assetInfo = new AssetInfo233();
                assetInfo.Address = record.Address;
                assetInfo.AssetPath = record.AssetPath;
                assetInfo.BundleName = record.BundleName;
                assetInfo.SetTags(record.Tags);
                _assetInfos.Add(assetInfo);
            }

            AssetManifest233 manifest = new AssetManifest233();
            manifest.AssetLibVersion = "0.1.0";
            manifest.GroupName = groupName;
            manifest.PackageVersion = System.DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            manifest.SetBundles(new List<AssetBundleInfo233>(_bundleInfos));
            manifest.SetAssets(new List<AssetInfo233>(_assetInfos));
            return manifest;
        }

        private static void WriteManifestAndVersionFiles(AssetManifest233 manifest, string outputRoot)
        {
            byte[] manifestBytes = AssetManifestBinarySerializer233.Serialize(manifest);
            string manifestFileName = manifest.GroupName + AssetLib233Constants.ManifestFileExtension;
            string manifestPath = Path.Combine(outputRoot, manifestFileName);
            File.WriteAllBytes(manifestPath, manifestBytes);

            string manifestHash = ComputeFileMd5(manifestPath);
            string versionFileName = manifest.GroupName + AssetLib233Constants.VersionFileExtension;
            string versionPath = Path.Combine(outputRoot, versionFileName);
            string versionText =
                manifest.PackageVersion +
                "|" +
                manifestFileName +
                "|" +
                manifestHash +
                "|" +
                manifestBytes.Length;
            File.WriteAllText(versionPath, versionText, Encoding.UTF8);
        }

        private static string BuildAddress(AssetCollector233 collector, string assetPath)
        {
            if (string.IsNullOrEmpty(collector.AddressPrefix))
            {
                return assetPath;
            }

            string rootPath = collector.AssetRootPath.Replace('\\', '/').TrimEnd('/');
            string safeAssetPath = assetPath.Replace('\\', '/');
            string relativePath = safeAssetPath;
            if (safeAssetPath.StartsWith(rootPath))
            {
                relativePath = safeAssetPath.Substring(rootPath.Length).TrimStart('/');
            }

            return collector.AddressPrefix.TrimEnd('/') + "/" + relativePath;
        }

        private static string[] SplitTags(string tagText)
        {
            if (string.IsNullOrEmpty(tagText))
            {
                return System.Array.Empty<string>();
            }

            return tagText.Split(',', ';', '|');
        }

        private static string ComputeFileMd5(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return string.Empty;
            }

            using (MD5 md5 = MD5.Create())
            {
                using (FileStream stream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = md5.ComputeHash(stream);
                    StringBuilder builder = new StringBuilder(hashBytes.Length * 2);
                    for (int i = 0; i < hashBytes.Length; i++)
                    {
                        builder.Append(hashBytes[i].ToString("x2"));
                    }

                    return builder.ToString();
                }
            }
        }
    }
}
