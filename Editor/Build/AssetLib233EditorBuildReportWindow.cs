using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AssetLib233.Editor
{
    public sealed class AssetLib233EditorBuildReportWindow : EditorWindow
    {
        private AssetLib233EditorBuildReport _report;

        public static void Open(AssetLib233EditorBuildReport report)
        {
            if (Application.isBatchMode)
            {
                return;
            }

            AssetLib233EditorBuildReportWindow window = GetWindow<AssetLib233EditorBuildReportWindow>();
            window.titleContent = new GUIContent("AssetLib233 Build Report");
            window.minSize = new Vector2(640f, 420f);
            window._report = report;
            window.Show();
            window.Rebuild();
        }

        public void CreateGUI()
        {
            Rebuild();
        }

        private void Rebuild()
        {
            if (rootVisualElement == null)
            {
                return;
            }

            rootVisualElement.Clear();
            rootVisualElement.style.paddingLeft = 12;
            rootVisualElement.style.paddingRight = 12;
            rootVisualElement.style.paddingTop = 12;
            rootVisualElement.style.paddingBottom = 12;

            Label title = new Label("AssetLib233 Build Report");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.fontSize = 18;
            rootVisualElement.Add(title);

            if (_report == null)
            {
                rootVisualElement.Add(new Label("No build report."));
                return;
            }

            ToolbarButtonRow(rootVisualElement);
            ScrollView scrollView = new ScrollView();
            scrollView.style.flexGrow = 1f;
            scrollView.style.marginTop = 8f;
            rootVisualElement.Add(scrollView);

            AddLine(scrollView, "状态", _report.success ? "成功" : "失败");
            AddLine(scrollView, "平台", _report.platformName);
            AddLine(scrollView, "Profile", _report.profilePath);
            AddLine(scrollView, "输出目录", _report.outputRoot);
            AddLine(scrollView, "报告路径", _report.reportPath);
            AddLine(scrollView, "加密", _report.enableBundleCrypto ? "开启" : "关闭");
            AddLine(scrollView, "加密密码", _report.bundleCryptoPassword);
            AddLine(scrollView, "AssetGroup", _report.groupCount.ToString());
            AddLine(scrollView, "Bundle", _report.bundleCount.ToString());
            AddLine(scrollView, "Asset", _report.assetCount.ToString());
            AddLine(scrollView, "Encrypted Bundle", _report.encryptedBundleCount.ToString());
            AddLine(scrollView, "Total Bytes", _report.totalBundleBytes.ToString());
            if (!string.IsNullOrEmpty(_report.error))
            {
                AddLine(scrollView, "错误", _report.error);
            }

            for (int i = 0; i < _report.groups.Count; i++)
            {
                AssetLib233EditorBuildReportGroup group = _report.groups[i];
                if (group == null)
                {
                    continue;
                }

                AddGroup(scrollView, i, group);
            }
        }

        private void ToolbarButtonRow(VisualElement root)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginTop = 8f;
            root.Add(row);

            Button copyButton = new Button(CopyReportText);
            copyButton.text = "复制真机/构建排障文本";
            row.Add(copyButton);

            Button revealButton = new Button(RevealReportFile);
            revealButton.text = "打开报告文件位置";
            revealButton.style.marginLeft = 8f;
            row.Add(revealButton);
        }

        private static void AddLine(VisualElement root, string name, string value)
        {
            Label label = new Label(name + ": " + value);
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.marginTop = 4f;
            root.Add(label);
        }

        private static void AddGroup(VisualElement root, int index, AssetLib233EditorBuildReportGroup group)
        {
            Label title = new Label("Group " + index + " | " + group.groupName);
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginTop = 12f;
            root.Add(title);
            AddLine(root, "  状态", group.success ? "成功" : "失败");
            AddLine(root, "  版本", group.packageVersion);
            AddLine(root, "  压缩", group.compressionMode);
            AddLine(root, "  输出", group.outputRoot);
            AddLine(root, "  Bundle", group.bundleCount.ToString());
            AddLine(root, "  Asset", group.assetCount.ToString());
            AddLine(root, "  Encrypted Bundle", group.encryptedBundleCount.ToString());
            AddLine(root, "  Bytes", group.totalBundleBytes.ToString());
            if (!string.IsNullOrEmpty(group.error))
            {
                AddLine(root, "  错误", group.error);
            }
        }

        private void CopyReportText()
        {
            EditorGUIUtility.systemCopyBuffer = AssetLib233EditorBuildReportFormatter.ToCopyText(_report);
            Debug.Log("[AssetLib233] 构建排障文本已复制到剪贴板");
        }

        private void RevealReportFile()
        {
            if (_report == null || string.IsNullOrEmpty(_report.reportPath))
            {
                return;
            }

            EditorUtility.RevealInFinder(_report.reportPath);
        }
    }
}
