using System.IO;

namespace AssetLib233.Runtime
{
    public sealed class AssetLib233LocalFileSystem : IAssetLib233FileSystem
    {
        private readonly string _rootPath;

        public AssetLib233LocalFileSystem(string rootPath)
        {
            _rootPath = AssetLib233NameUtility.NormalizeHost(rootPath);
        }

        public string Name
        {
            get { return "LocalFileSystem"; }
        }

        public bool Exists(AssetBundleInfo233 bundleInfo)
        {
            string path = GetLocalPath(bundleInfo);
            return File.Exists(path);
        }

        public string GetLocalPath(AssetBundleInfo233 bundleInfo)
        {
            string fileName = bundleInfo != null ? bundleInfo.FileName : string.Empty;
            return Path.Combine(_rootPath, fileName).Replace('\\', '/');
        }

        public byte[] ReadAllBytes(AssetBundleInfo233 bundleInfo)
        {
            return File.ReadAllBytes(GetLocalPath(bundleInfo));
        }
    }
}
