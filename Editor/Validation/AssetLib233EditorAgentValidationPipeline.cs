using System.Collections.Generic;
using System.IO;
using System.Text;
using AssetLib233.Runtime;
using UnityEditor;
using UnityEngine;

namespace AssetLib233.Editor
{
    /// <summary>
    /// Agent-first 自动化验证。
    /// 不执行 AB 打包，直接验证 BuildProfile、Collector、地址、Editor 可加载性，以及已发布 CDN 产物。
    /// </summary>
    public static class AssetLib233EditorAgentValidationPipeline
    {
        private const int DefaultTimeoutMilliseconds = 30 * 60 * 1000;

        private static readonly List<AssetGroup233> _groupCache = new List<AssetGroup233>(16);
        private static readonly List<AssetCollector233> _collectorCache = new List<AssetCollector233>(32);
        private static readonly Dictionary<string, string> _addressToAssetPath = new Dictionary<string, string>(4096);
        private static readonly HashSet<string> _groupNameSet = new HashSet<string>();
        private static readonly StringBuilder _messageBuilder = new StringBuilder(4096);

        [MenuItem("AssetLib233/Agent/Validate Without Build")]
        public static void RunAgentFirstValidationMenu()
        {
            RunAgentFirstValidation();
        }

        public static void RunAgentFirstValidationBatchMode()
        {
            bool success = RunAgentFirstValidation();
            EditorApplication.Exit(success ? 0 : 1);
        }

        public static bool RunAgentFirstValidation()
        {
            if (!AssetLib233EditorPublishLocalConfigLoader.TryLoad(
                    out AssetLib233EditorPublishLocalConfig config,
                    out string error))
            {
                Debug.LogError("[AssetLib233-Agent] " + AssetLib233EditorI18n.Text("ConfigMissing") + ": " + error);
                return false;
            }

            AssetLib233EditorI18n.SetLanguage(config.language);
            AssetLib233EditorPublishReport report = CreateReport(config);
            Debug.Log("[AssetLib233-Agent] " + AssetLib233EditorI18n.Text("ValidationStart"));

            bool success = true;
            success &= RunPreBuildValidation(config, report);
            success &= RunPublishedCdnValidation(config, report);

            report.success = success;
            report.endTimeUtc = System.DateTime.UtcNow.ToString("O");
            string reportPath = AssetLib233EditorPublishReportStore.Save(config, report);
            Debug.Log("[AssetLib233-Agent] " + AssetLib233EditorI18n.Text("ValidationDone") + ": " + reportPath);
            return success;
        }

        private static AssetLib233EditorPublishReport CreateReport(AssetLib233EditorPublishLocalConfig config)
        {
            AssetLib233EditorPublishReport report = new AssetLib233EditorPublishReport();
            report.reportId = System.DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + "_AssetLib233_AgentValidation";
            report.startTimeUtc = System.DateTime.UtcNow.ToString("O");
            report.projectPath = Directory.GetParent(Application.dataPath).FullName;
            report.buildOutputRoot = config.buildOutputRoot;
            report.cdnProvider = config.cdnProvider;
            report.cdnRegion = config.cdnRegion;
            report.cdnBucket = config.cdnBucket;
            report.cdnPathPrefix = config.cdnPathPrefix;
            report.uploadConfigName = AssetLib233EditorPublishConfigResolver.ResolveUploadConfigName(config);
            report.cdnGoToolConfigPath = AssetLib233CdnGoToolAdapter.ResolveConfigPathForReport(config);
            report.agentValidationPlatform = config.agentValidationPlatform;
            report.agentValidationEnvironment = config.agentValidationEnvironment;
            report.cdnRootUrl = config.cdnRootUrl;
            report.enableBundleCrypto = config.enableBundleCrypto;
            return report;
        }

        private static bool RunPreBuildValidation(
            AssetLib233EditorPublishLocalConfig config,
            AssetLib233EditorPublishReport report)
        {
            AssetLib233EditorPublishReportStep step = CreateStep("PreBuildProfileAndLoadValidation");
            string profilePath = ResolveBuildProfilePath(config);
            step.command = "Validate BuildProfile without AssetBundle build";
            step.workingDirectory = Directory.GetParent(Application.dataPath).FullName;

            AssetBuildProfile233 profile = AssetDatabase.LoadAssetAtPath<AssetBuildProfile233>(profilePath);
            if (profile == null)
            {
                step.exitCode = -1;
                step.message = AssetLib233EditorI18n.Text("BuildProfileMissing") + ": " + profilePath;
                FinishStep(report, step);
                return false;
            }

            int groupCount = 0;
            int collectorCount = 0;
            int assetCount = 0;
            int duplicateAddressCount = 0;
            int missingRootCount = 0;
            int loadFailedCount = 0;
            int requiredMissingCount = 0;
            int sampleMissingCount = 0;

            _messageBuilder.Length = 0;
            _addressToAssetPath.Clear();
            _groupNameSet.Clear();
            profile.GetGroupsNonAlloc(_groupCache);
            groupCount = _groupCache.Count;
            for (int i = 0; i < _groupCache.Count; i++)
            {
                AssetGroup233 group = _groupCache[i];
                if (group == null)
                {
                    continue;
                }

                string groupName = AssetLib233NameUtility.NormalizePackageName(group.GroupName);
                _groupNameSet.Add(groupName);
                group.GetCollectorsNonAlloc(_collectorCache);
                collectorCount += _collectorCache.Count;
                for (int j = 0; j < _collectorCache.Count; j++)
                {
                    AssetCollector233 collector = _collectorCache[j];
                    if (collector == null || !collector.Enabled)
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(collector.AssetRootPath) ||
                        !AssetDatabase.IsValidFolder(collector.AssetRootPath))
                    {
                        missingRootCount++;
                        AppendLineLimited(
                            AssetLib233EditorI18n.Text("CollectorRootMissing") +
                            " | group=" +
                            groupName +
                            " | collector=" +
                            collector.CollectorName +
                            " | root=" +
                            collector.AssetRootPath);
                        continue;
                    }

                    string[] guids = AssetDatabase.FindAssets(string.Empty, new[] { collector.AssetRootPath });
                    for (int k = 0; k < guids.Length; k++)
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(guids[k]);
                        if (!IsValidCollectedAsset(assetPath))
                        {
                            continue;
                        }

                        assetCount++;
                        string address = BuildAddress(collector, assetPath);
                        if (_addressToAssetPath.TryGetValue(address, out string firstAssetPath))
                        {
                            duplicateAddressCount++;
                            AppendLineLimited(
                                AssetLib233EditorI18n.Text("DuplicateAddress") +
                                " | address=" +
                                address +
                                " | first=" +
                                firstAssetPath +
                                " | duplicate=" +
                                assetPath);
                        }
                        else
                        {
                            _addressToAssetPath.Add(address, assetPath);
                        }

                        Object assetObject = AssetDatabase.LoadMainAssetAtPath(assetPath);
                        if (assetObject == null)
                        {
                            loadFailedCount++;
                            AppendLineLimited(AssetLib233EditorI18n.Text("AssetLoadFailed") + ": " + assetPath);
                        }
                    }
                }

                _collectorCache.Clear();
            }

            requiredMissingCount = CountMissingRequiredGroups(config.agentValidationRequiredGroups);
            sampleMissingCount = CountMissingSampleAddresses(config.agentValidationSampleAddresses);
            _groupCache.Clear();
            bool success = assetCount > 0 &&
                           duplicateAddressCount == 0 &&
                           missingRootCount == 0 &&
                           loadFailedCount == 0 &&
                           requiredMissingCount == 0 &&
                           sampleMissingCount == 0;
            step.exitCode = success ? 0 : -1;
            step.message =
                "platform=" +
                config.agentValidationPlatform +
                " | env=" +
                config.agentValidationEnvironment +
                " | profile=" +
                profilePath +
                " | groups=" +
                groupCount +
                " | collectors=" +
                collectorCount +
                " | assets=" +
                assetCount +
                " | duplicateAddress=" +
                duplicateAddressCount +
                " | missingRoot=" +
                missingRootCount +
                " | loadFailed=" +
                loadFailedCount +
                " | requiredMissing=" +
                requiredMissingCount +
                " | sampleMissing=" +
                sampleMissingCount +
                "\n" +
                _messageBuilder.ToString();
            FinishStep(report, step);
            return success;
        }

        private static bool RunPublishedCdnValidation(
            AssetLib233EditorPublishLocalConfig config,
            AssetLib233EditorPublishReport report)
        {
            bool isHandled;
            bool success = AssetLib233CdnGoToolAdapter.TryRun(
                config,
                report,
                "PublishedCdnGoVerify",
                config.cdnGoToolCommand,
                DefaultTimeoutMilliseconds,
                out isHandled);
            if (isHandled)
            {
                return success;
            }

            AssetLib233EditorPublishReportStep step = CreateStep("PublishedCdnGoVerify");
            step.exitCode = 0;
            step.message = AssetLib233EditorI18n.Text("Skipped") + ": cdnGoToolConfigDirectory";
            FinishStep(report, step);
            return true;
        }

        private static string ResolveBuildProfilePath(AssetLib233EditorPublishLocalConfig config)
        {
            if (!string.IsNullOrWhiteSpace(config.agentValidationBuildProfilePath))
            {
                return config.agentValidationBuildProfilePath;
            }

            if (!string.IsNullOrWhiteSpace(config.nativeBuildProfilePath))
            {
                return config.nativeBuildProfilePath;
            }

            return "Assets/neko233/AssetLib233/AssetLib233BuildProfile.asset";
        }

        private static bool IsValidCollectedAsset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath) || Directory.Exists(assetPath))
            {
                return false;
            }

            if (assetPath.EndsWith(".cs", System.StringComparison.OrdinalIgnoreCase) ||
                assetPath.EndsWith(".asmdef", System.StringComparison.OrdinalIgnoreCase) ||
                assetPath.EndsWith(".meta", System.StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        private static int CountMissingRequiredGroups(string[] requiredGroups)
        {
            if (requiredGroups == null || requiredGroups.Length == 0)
            {
                return 0;
            }

            int missingCount = 0;
            for (int i = 0; i < requiredGroups.Length; i++)
            {
                string groupName = AssetLib233NameUtility.NormalizePackageName(requiredGroups[i]);
                if (string.IsNullOrEmpty(groupName))
                {
                    continue;
                }

                if (!_groupNameSet.Contains(groupName))
                {
                    missingCount++;
                    AppendLineLimited("missing required group: " + groupName);
                }
            }

            return missingCount;
        }

        private static int CountMissingSampleAddresses(string[] sampleAddresses)
        {
            if (sampleAddresses == null || sampleAddresses.Length == 0)
            {
                return 0;
            }

            int missingCount = 0;
            for (int i = 0; i < sampleAddresses.Length; i++)
            {
                string address = sampleAddresses[i];
                if (string.IsNullOrWhiteSpace(address))
                {
                    continue;
                }

                if (!_addressToAssetPath.ContainsKey(address.Trim()))
                {
                    missingCount++;
                    AppendLineLimited("missing sample address: " + address);
                }
            }

            return missingCount;
        }

        private static string BuildAddress(AssetCollector233 collector, string assetPath)
        {
            if (string.IsNullOrEmpty(collector.AddressPrefix))
            {
                return Path.GetFileNameWithoutExtension(assetPath);
            }

            return collector.AddressPrefix.TrimEnd('/') + "/" + Path.GetFileNameWithoutExtension(assetPath);
        }

        private static AssetLib233EditorPublishReportStep CreateStep(string stepName)
        {
            AssetLib233EditorPublishReportStep step = new AssetLib233EditorPublishReportStep();
            step.name = stepName;
            step.startTimeUtc = System.DateTime.UtcNow.ToString("O");
            return step;
        }

        private static void FinishStep(AssetLib233EditorPublishReport report, AssetLib233EditorPublishReportStep step)
        {
            step.endTimeUtc = System.DateTime.UtcNow.ToString("O");
            report.AddStep(step);
        }

        private static void AppendLineLimited(string line)
        {
            if (_messageBuilder.Length > 12000)
            {
                return;
            }

            _messageBuilder.AppendLine(line);
        }
    }
}
