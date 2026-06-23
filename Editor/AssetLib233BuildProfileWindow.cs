using AssetLib233.Runtime;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AssetLib233.Editor
{
    public sealed class AssetLib233BuildProfileWindow : EditorWindow
    {
        private readonly List<AssetGroup233> _groupItems = new List<AssetGroup233>(16);
        private ObjectField _profileField;
        private ListView _groupListView;

        public static void Open()
        {
            AssetLib233BuildProfileWindow window = GetWindow<AssetLib233BuildProfileWindow>();
            window.titleContent = new GUIContent("AssetLib233 Build Profile");
            window.minSize = new Vector2(560f, 360f);
            window.Show();
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.style.paddingLeft = 12;
            root.style.paddingRight = 12;
            root.style.paddingTop = 12;
            root.style.paddingBottom = 12;

            Label title = new Label("AssetGroup / Collector");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.fontSize = 18;
            root.Add(title);

            _profileField = new ObjectField("Build Profile");
            _profileField.objectType = typeof(AssetBuildProfile233);
            _profileField.allowSceneObjects = false;
            _profileField.style.marginTop = 10;
            _profileField.RegisterValueChangedCallback(OnProfileChanged);
            root.Add(_profileField);

            Button createButton = new Button(CreateDefaultProfile);
            createButton.text = "创建默认 Build Profile";
            createButton.style.marginTop = 6;
            root.Add(createButton);

            _groupListView = new ListView();
            _groupListView.style.marginTop = 10;
            _groupListView.style.flexGrow = 1;
            _groupListView.makeItem = MakeGroupItem;
            _groupListView.bindItem = BindGroupItem;
            root.Add(_groupListView);

            Refresh();
        }

        private void OnProfileChanged(ChangeEvent<Object> evt)
        {
            Refresh();
        }

        private VisualElement MakeGroupItem()
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Column;
            row.style.paddingTop = 6;
            row.style.paddingBottom = 6;

            Label groupLabel = new Label();
            groupLabel.name = "group";
            groupLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            row.Add(groupLabel);

            Label collectorLabel = new Label();
            collectorLabel.name = "collector";
            collectorLabel.style.marginTop = 2;
            row.Add(collectorLabel);

            return row;
        }

        private void BindGroupItem(VisualElement element, int index)
        {
            if (index < 0 || index >= _groupItems.Count)
            {
                return;
            }

            AssetGroup233 group = _groupItems[index];
            Label groupLabel = element.Q<Label>("group");
            Label collectorLabel = element.Q<Label>("collector");
            groupLabel.text = group.GroupName + " | Required = " + group.RequiredOnFirstEnter + " | Builtin = " + group.Builtin;
            collectorLabel.text = "Collectors: " + group.Collectors.Count;
        }

        private void Refresh()
        {
            AssetBuildProfile233 profile = _profileField != null ? _profileField.value as AssetBuildProfile233 : null;
            if (profile == null)
            {
                profile = FindProfileAsset();
                if (_profileField != null)
                {
                    _profileField.value = profile;
                }
            }

            if (_groupListView == null)
            {
                return;
            }

            _groupItems.Clear();
            if (profile != null)
            {
                profile.GetGroupsNonAlloc(_groupItems);
            }

            _groupListView.itemsSource = _groupItems;
            _groupListView.Rebuild();
        }

        private static AssetBuildProfile233 FindProfileAsset()
        {
            string[] guids = AssetDatabase.FindAssets("t:AssetBuildProfile233");
            if (guids == null || guids.Length == 0)
            {
                return null;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<AssetBuildProfile233>(path);
        }

        private static void CreateDefaultProfile()
        {
            const string profilePath = "Assets/neko233/AssetLib233/AssetLib233BuildProfile.asset";
            AssetBuildProfile233 profile = AssetDatabase.LoadAssetAtPath<AssetBuildProfile233>(profilePath);
            if (profile == null)
            {
                profile = CreateInstance<AssetBuildProfile233>();
                AssetDatabase.CreateAsset(profile, profilePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            Selection.activeObject = profile;
            EditorGUIUtility.PingObject(profile);
        }
    }
}
