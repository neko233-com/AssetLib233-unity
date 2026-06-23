using AssetLib233.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AssetLib233.Editor
{
    public sealed class AssetLib233SettingsWindow : EditorWindow
    {
        private const string WindowTitle = "AssetLib233";
        private ObjectField _settingsField;
        private Label _platformLabel;
        private Label _concurrencyLabel;

        [MenuItem("Window/neko233/AssetLib233")]
        public static void Open()
        {
            AssetLib233SettingsWindow window = GetWindow<AssetLib233SettingsWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(420f, 240f);
            window.Show();
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.style.paddingLeft = 12;
            root.style.paddingRight = 12;
            root.style.paddingTop = 12;
            root.style.paddingBottom = 12;

            Label title = new Label("AssetLib233");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.fontSize = 18;
            root.Add(title);

            _platformLabel = new Label();
            _platformLabel.style.marginTop = 8;
            root.Add(_platformLabel);

            _concurrencyLabel = new Label();
            _concurrencyLabel.style.marginTop = 4;
            root.Add(_concurrencyLabel);

            _settingsField = new ObjectField("Settings");
            _settingsField.objectType = typeof(AssetLib233Settings);
            _settingsField.allowSceneObjects = false;
            _settingsField.style.marginTop = 12;
            root.Add(_settingsField);

            Button createButton = new Button(AssetLib233EditorMenu.CreateDefaultSettingsAsset);
            createButton.text = "创建默认设置资产";
            createButton.style.marginTop = 8;
            root.Add(createButton);

            Button selectButton = new Button(SelectCurrentSettings);
            selectButton.text = "选中设置资产";
            selectButton.style.marginTop = 4;
            root.Add(selectButton);

            Refresh();
        }

        private void Refresh()
        {
            EnumAssetLib233RuntimePlatform runtimePlatform = AssetLib233PlatformDetector.GetRuntimePlatform();
            int downloadConcurrency = AssetLib233DownloadPolicy.GetDownloadConcurrency(runtimePlatform);

            _platformLabel.text = "当前平台: " + runtimePlatform;
            _concurrencyLabel.text = "下载并发: " + downloadConcurrency;
            _settingsField.value = FindSettingsAsset();
        }

        private static AssetLib233Settings FindSettingsAsset()
        {
            string[] guids = AssetDatabase.FindAssets("t:AssetLib233Settings");
            if (guids == null || guids.Length == 0)
            {
                return null;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<AssetLib233Settings>(path);
        }

        private void SelectCurrentSettings()
        {
            AssetLib233Settings settings = _settingsField.value as AssetLib233Settings;
            if (settings == null)
            {
                settings = FindSettingsAsset();
            }

            if (settings == null)
            {
                AssetLib233EditorMenu.CreateDefaultSettingsAsset();
                return;
            }

            Selection.activeObject = settings;
            EditorGUIUtility.PingObject(settings);
        }
    }
}
