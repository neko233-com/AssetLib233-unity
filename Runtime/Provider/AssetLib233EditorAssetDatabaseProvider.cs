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
            return assetInfo != null && !string.IsNullOrEmpty(assetInfo.AssetPath);
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

            TObject assetObject = AssetDatabase.LoadAssetAtPath<TObject>(assetInfo.AssetPath);
            if (assetObject == null)
            {
                handle.SetFailed(
                    "EditorSimulate 加载为空. group = " +
                    assetPackage.PackageName +
                    " | address = " +
                    assetInfo.Address +
                    " | path = " +
                    assetInfo.AssetPath +
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
                assetInfo.AssetPath +
                " type=" +
                typeof(TObject).Name);
            return handle;
        }
    }
}
#endif
