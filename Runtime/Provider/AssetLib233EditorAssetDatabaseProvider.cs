#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AssetLib233.Runtime
{
    /// <summary>
    /// 编辑器模拟 Provider。按 Manifest 中的 AssetPath 直接走 AssetDatabase，方便本地用收集后的名字验证加载。
    /// </summary>
    public sealed class AssetLib233EditorAssetDatabaseProvider : IAssetLib233AssetProvider
    {
        public bool CanLoad(AssetInfo233 assetInfo)
        {
            return assetInfo != null &&
                   (!string.IsNullOrEmpty(assetInfo.AssetPath) || !string.IsNullOrEmpty(assetInfo.Address));
        }

        public AssetHandle233<TObject> LoadAssetAsync<TObject>(AssetPackage233 assetPackage, AssetInfo233 assetInfo)
            where TObject : Object
        {
            AssetHandle233<TObject> handle = new AssetHandle233<TObject>(
                assetPackage.PackageName,
                assetInfo == null ? string.Empty : assetInfo.Address);
            if (!CanLoad(assetInfo))
            {
                handle.SetFailed("EditorSimulate AssetInfo 无效");
                return handle;
            }

            string assetPath = ResolveEditorAssetPath(assetInfo);
            TObject assetObject = string.IsNullOrEmpty(assetPath)
                ? null
                : AssetDatabase.LoadAssetAtPath<TObject>(assetPath);
            if (assetObject == null)
            {
                handle.SetFailed(
                    "EditorSimulate 加载为空. group = " +
                    assetPackage.PackageName +
                    " | address = " +
                    assetInfo.Address +
                    " | path = " +
                    assetPath +
                    " | type = " +
                    typeof(TObject).Name);
                return handle;
            }

            handle.SetResult(assetObject);
            AssetLib233RuntimeDiagnostic.RecordEvent(
                "editor-load-ok group=" +
                assetPackage.PackageName +
                " address=" +
                assetInfo.Address +
                " path=" +
                assetPath +
                " type=" +
                typeof(TObject).Name);
            return handle;
        }

        private static string ResolveEditorAssetPath(AssetInfo233 assetInfo)
        {
            if (assetInfo == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(assetInfo.AssetPath))
            {
                return assetInfo.AssetPath;
            }

            if (string.IsNullOrEmpty(assetInfo.Address))
            {
                return string.Empty;
            }

            string[] guids = AssetDatabase.FindAssets(assetInfo.Address);
            string matchedPath = string.Empty;
            int matchedCount = 0;
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                if (!string.Equals(fileName, assetInfo.Address, System.StringComparison.Ordinal))
                {
                    continue;
                }

                matchedPath = assetPath;
                matchedCount++;
                if (matchedCount > 1)
                {
                    Debug.LogError("[AssetLib233] EditorSimulate 文件名重复，无法按短地址唯一加载: " + assetInfo.Address);
                    return string.Empty;
                }
            }

            return matchedPath;
        }
    }
}
#endif
