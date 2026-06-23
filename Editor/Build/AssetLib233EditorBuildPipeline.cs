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
        private static readonly Dictionary<string, string> _assetPathToAddress = new Dictionary<string, string>(2048);
        private static readonly HashSet<string> _assetAddressSet = new HashSet<string>();
        private static readonly HashSet<string> _collectedAssetPathSet = new HashSet<string>();
        private static readonly AssetLib233DefaultPackRule _defaultPackRule = new AssetLib233DefaultPackRule();
        private static readonly AssetLib233DefaultBuildVerifier _defaultVerifier = new AssetLib233DefaultBuildVerifier();
        private static int _duplicateAddressCount;

        public static bool BuildProfile(AssetBuildProfile233 profile, string outputRoot, BuildTarget buildTarget, out string error)
        {
            return BuildProfile(
                profile,
                outputRoot,
                buildTarget,
                false,
                AssetLib233Constants.DefaultBundleCryptoPassword,
                out error);
        }

        public static bool BuildProfile(
            AssetBuildProfile233 profile,
            string outputRoot,
            BuildTarget buildTarget,
            bool enableBundleCrypto,
            string bundleCryptoPassword,
            out string error)
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

            AssetLib233EditorBuildReport report = CreateBuildReport(
                profile,
                outputRoot,
                buildTarget,
                enableBundleCrypto,
                bundleCryptoPassword);
            bool isOk = false;
            try
            {
                Directory.CreateDirectory(outputRoot);
                profile.GetGroupsNonAlloc(_groupCache);
                isOk = true;
                for (int i = 0; i < _groupCache.Count; i++)
                {
                    AssetGroup233 group = _groupCache[i];
                    if (group == null)
                    {
                        continue;
                    }

                    string groupOutputRoot = Path.Combine(outputRoot, AssetLib233NameUtility.NormalizePackageName(group.GroupName));
                    AssetLib233EditorBuildReportGroup groupReport;
                    if (!BuildGroup(
                            group,
                            groupOutputRoot,
                            buildTarget,
                            enableBundleCrypto,
                            bundleCryptoPassword,
                            out string groupError,
                            out groupReport))
                    {
                        report.AddGroup(groupReport);
                        error = groupError;
                        isOk = false;
                        break;
                    }

                    report.AddGroup(groupReport);
                }
            }
            catch (System.Exception exception)
            {
                error = exception.Message;
                isOk = false;
                Debug.LogException(exception);
            }
            finally
            {
                _groupCache.Clear();
                FinishBuildReport(report, isOk, error);
            }

            return isOk;
        }

        private static AssetLib233EditorBuildReport CreateBuildReport(
            AssetBuildProfile233 profile,
            string outputRoot,
            BuildTarget buildTarget,
            bool enableBundleCrypto,
            string bundleCryptoPassword)
        {
            AssetLib233EditorBuildReport report = new AssetLib233EditorBuildReport();
            report.reportId = System.DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            report.profilePath = AssetDatabase.GetAssetPath(profile);
            report.outputRoot = outputRoot;
            report.platformName = buildTarget.ToString();
            report.buildStartUtc = System.DateTime.UtcNow.ToString("o");
            report.enableBundleCrypto = enableBundleCrypto;
            report.bundleCryptoPassword = string.IsNullOrEmpty(bundleCryptoPassword)
                ? AssetLib233Constants.DefaultBundleCryptoPassword
                : bundleCryptoPassword;
            return report;
        }

        private static void FinishBuildReport(AssetLib233EditorBuildReport report, bool success, string error)
        {
            if (report == null)
            {
                return;
            }

            report.success = success;
            report.error = error;
            report.buildEndUtc = System.DateTime.UtcNow.ToString("o");
            report.RefreshTotals();
            string reportPath = AssetLib233EditorBuildReportStore.Save(report);
            Debug.Log("[AssetLib233] 构建报告已生成: " + reportPath);
            AssetLib233EditorBuildReportWindow.Open(report);
            AssetLib233EditorBuildNotificationCenter.Notify(report);
        }

        private static bool BuildGroup(
            AssetGroup233 group,
            string outputRoot,
            BuildTarget buildTarget,
            bool enableBundleCrypto,
            string bundleCryptoPassword,
            out string error,
            out AssetLib233EditorBuildReportGroup groupReport)
        {
            error = string.Empty;
            EnumAssetLib233CompressionMode compressionMode = ResolveCompressionMode(group, buildTarget);
            groupReport = CreateGroupReport(group, outputRoot, compressionMode);
            Directory.CreateDirectory(outputRoot);
            _assetRecords.Clear();
            _bundleInfos.Clear();
            _assetInfos.Clear();
            _bundleNames.Clear();
            _assetPathToAddress.Clear();
            _assetAddressSet.Clear();
            _collectedAssetPathSet.Clear();
            _duplicateAddressCount = 0;

            Dictionary<string, List<string>> bundleToAssets = new Dictionary<string, List<string>>(256);
            CollectGroupAssets(group, bundleToAssets);
            if (_duplicateAddressCount > 0)
            {
                error = "AssetGroup 存在重复收集地址，请修正重名资源或 AddressPrefix. group = " +
                        group.GroupName +
                        " | duplicateAddressCount = " +
                        _duplicateAddressCount;
                groupReport.error = error;
                return false;
            }

            if (bundleToAssets.Count == 0)
            {
                error = "AssetGroup 没有收集到资源: " + group.GroupName;
                groupReport.error = error;
                return false;
            }

            AssetBundleBuild[] builds = CreateAssetBundleBuilds(bundleToAssets);
            BuildAssetBundleOptions buildOptions = ResolveBuildOptions(compressionMode);
            AssetBundleManifest unityManifest = BuildPipeline.BuildAssetBundles(
                outputRoot,
                builds,
                buildOptions,
                buildTarget);
            if (unityManifest == null)
            {
                error = "BuildPipeline.BuildAssetBundles 返回空. group = " + group.GroupName;
                groupReport.error = error;
                return false;
            }

            AssetManifest233 manifest = CreateManifest(
                group,
                outputRoot,
                unityManifest,
                enableBundleCrypto,
                bundleCryptoPassword);
            WriteManifestAndVersionFiles(manifest, outputRoot);
            AssetLib233BuildVerifyContext verifyContext = new AssetLib233BuildVerifyContext();
            verifyContext.platformName = buildTarget.ToString();
            verifyContext.outputRoot = outputRoot;
            verifyContext.manifest = manifest;
            IAssetLib233BuildVerifier verifier = AssetLib233EditorBuildExtensionRegistry.ResolveVerifier(_defaultVerifier);
            if (!verifier.Verify(verifyContext, out string verifyError))
            {
                error = verifyError;
                groupReport.error = error;
                return false;
            }

            FillGroupReport(groupReport, manifest);
            Debug.Log(
                "[AssetLib233] 构建完成. group = " +
                group.GroupName +
                " | output = " +
                outputRoot +
                " | assets = " +
                manifest.Assets.Count +
                " | bundles = " +
                manifest.Bundles.Count +
                " | compression = " +
                compressionMode);
            return true;
        }

        private static AssetLib233EditorBuildReportGroup CreateGroupReport(
            AssetGroup233 group,
            string outputRoot,
            EnumAssetLib233CompressionMode compressionMode)
        {
            AssetLib233EditorBuildReportGroup groupReport = new AssetLib233EditorBuildReportGroup();
            groupReport.groupName = group == null ? string.Empty : group.GroupName;
            groupReport.outputRoot = outputRoot;
            groupReport.compressionMode = compressionMode.ToString();
            return groupReport;
        }

        private static void FillGroupReport(AssetLib233EditorBuildReportGroup groupReport, AssetManifest233 manifest)
        {
            if (groupReport == null || manifest == null)
            {
                return;
            }

            groupReport.success = true;
            groupReport.packageVersion = manifest.PackageVersion;
            groupReport.bundleCount = manifest.Bundles.Count;
            groupReport.assetCount = manifest.Assets.Count;
            groupReport.encryptedBundleCount = 0;
            groupReport.totalBundleBytes = 0L;
            for (int i = 0; i < manifest.Bundles.Count; i++)
            {
                AssetBundleInfo233 bundleInfo = manifest.Bundles[i];
                if (bundleInfo == null)
                {
                    continue;
                }

                groupReport.totalBundleBytes += bundleInfo.FileSize;
                if (bundleInfo.IsEncrypted)
                {
                    groupReport.encryptedBundleCount++;
                }
            }
        }

        private static EnumAssetLib233CompressionMode ResolveCompressionMode(AssetGroup233 group, BuildTarget buildTarget)
        {
            string platformName = buildTarget.ToString();
            string groupName = group == null ? string.Empty : group.GroupName;
            group.GetCollectorsNonAlloc(_collectorCache);
            EnumAssetLib233CompressionMode result = EnumAssetLib233CompressionMode.Lz4;
            bool hasResult = false;
            for (int i = 0; i < _collectorCache.Count; i++)
            {
                AssetCollector233 collector = _collectorCache[i];
                if (collector == null || !collector.Enabled)
                {
                    continue;
                }

                EnumAssetLib233CompressionMode compressionMode =
                    AssetLib233EditorBuildExtensionRegistry.ResolveCompressionMode(
                        platformName,
                        groupName,
                        collector.CollectorName);
                if (!hasResult)
                {
                    result = compressionMode;
                    hasResult = true;
                }
                else if (result != compressionMode)
                {
                    Debug.Log(
                        "[AssetLib233] 同一 AssetGroup 内暂不支持混合压缩，使用首个有效 Collector 压缩策略. group = " +
                        groupName +
                        " | first = " +
                        result +
                        " | ignored = " +
                        compressionMode);
                }
            }

            _collectorCache.Clear();
            return result;
        }

        private static BuildAssetBundleOptions ResolveBuildOptions(EnumAssetLib233CompressionMode compressionMode)
        {
            if (compressionMode == EnumAssetLib233CompressionMode.Uncompressed)
            {
                return BuildAssetBundleOptions.UncompressedAssetBundle;
            }

            if (compressionMode == EnumAssetLib233CompressionMode.Lzma)
            {
                return BuildAssetBundleOptions.None;
            }

            return BuildAssetBundleOptions.ChunkBasedCompression;
        }

        private static void CollectGroupAssets(AssetGroup233 group, Dictionary<string, List<string>> bundleToAssets)
        {
            IAssetLib233BuildPackRule packRule = AssetLib233EditorBuildExtensionRegistry.ResolvePackRule(_defaultPackRule);
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
                    string bundleName = packRule.GetBundleName(group, collector, assetPath);
                    string address = BuildAddress(collector, assetPath);
                    if (_assetAddressSet.Contains(address))
                    {
                        _duplicateAddressCount++;
                        Debug.LogError("[AssetLib233] 收集地址重复，已跳过重复资源. address = " + address + " | assetPath = " + assetPath);
                        continue;
                    }

                    _assetAddressSet.Add(address);
                    AddAssetToBundle(bundleToAssets, bundleName, assetPath);
                    _assetPathToAddress[assetPath] = address;
                    AssetLib233BuildAssetRecord record = new AssetLib233BuildAssetRecord();
                    record.Address = address;
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
                build.addressableNames = BuildAddressableNames(assets);
                builds[i] = build;
            }

            return builds;
        }

        private static string[] BuildAddressableNames(List<string> assets)
        {
            string[] addressableNames = new string[assets.Count];
            for (int i = 0; i < assets.Count; i++)
            {
                string assetPath = assets[i];
                if (!_assetPathToAddress.TryGetValue(assetPath, out string address))
                {
                    address = Path.GetFileNameWithoutExtension(assetPath);
                }

                addressableNames[i] = address;
            }

            return addressableNames;
        }

        private static AssetManifest233 CreateManifest(
            AssetGroup233 group,
            string outputRoot,
            AssetBundleManifest unityManifest,
            bool enableBundleCrypto,
            string bundleCryptoPassword)
        {
            string groupName = AssetLib233NameUtility.NormalizePackageName(group.GroupName);
            for (int i = 0; i < _bundleNames.Count; i++)
            {
                string bundleName = _bundleNames[i];
                string filePath = Path.Combine(outputRoot, bundleName);
                FileInfo fileInfo = new FileInfo(filePath);
                uint crc = 0;
                BuildPipeline.GetCRCForAssetBundle(filePath, out crc);
                if (enableBundleCrypto)
                {
                    ApplyBundleXorCrypto(filePath, bundleCryptoPassword);
                    fileInfo.Refresh();
                }

                AssetBundleInfo233 bundleInfo = new AssetBundleInfo233();
                bundleInfo.BundleName = bundleName;
                bundleInfo.FileName = bundleName;
                bundleInfo.FileHash = ComputeFileMd5(filePath);
                bundleInfo.FileSize = fileInfo.Exists ? fileInfo.Length : 0L;
                bundleInfo.FileCrc = crc;
                bundleInfo.IsEncrypted = enableBundleCrypto;
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
                assetInfo.AssetPath = string.Empty;
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
                return Path.GetFileNameWithoutExtension(assetPath);
            }

            return collector.AddressPrefix.TrimEnd('/') + "/" + Path.GetFileNameWithoutExtension(assetPath);
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

        private static void ApplyBundleXorCrypto(string filePath, string bundleCryptoPassword)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return;
            }

            byte[] keyBytes = AssetLib233XorCrypto.BuildKeyBytes(bundleCryptoPassword);
            byte[] buffer = new byte[64 * 1024];
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                long filePosition = 0L;
                while (true)
                {
                    int readCount = stream.Read(buffer, 0, buffer.Length);
                    if (readCount <= 0)
                    {
                        break;
                    }

                    AssetLib233XorCrypto.ApplyXorInPlace(buffer, 0, readCount, filePosition, keyBytes);
                    stream.Position = filePosition;
                    stream.Write(buffer, 0, readCount);
                    filePosition += readCount;
                }
            }
        }
    }
}
