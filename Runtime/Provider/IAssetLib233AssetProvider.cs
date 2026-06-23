using UnityEngine;

namespace AssetLib233.Runtime
{
    /// <summary>
    /// 资源 Provider 接口。AB、RawFile、Scene、内置资源、编辑器模拟都通过 Provider 统一接入。
    /// </summary>
    public interface IAssetLib233AssetProvider
    {
        bool CanLoad(AssetInfo233 assetInfo);
        AssetHandle233<TObject> LoadAssetAsync<TObject>(AssetPackage233 assetPackage, AssetInfo233 assetInfo) where TObject : Object;
    }
}
