namespace AssetLib233.Runtime
{
    public interface IAssetLib233FileSystem
    {
        string Name { get; }
        bool Exists(AssetBundleInfo233 bundleInfo);
        string GetLocalPath(AssetBundleInfo233 bundleInfo);
        byte[] ReadAllBytes(AssetBundleInfo233 bundleInfo);
    }
}
