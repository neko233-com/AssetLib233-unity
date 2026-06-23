using System;
using System.Collections.Generic;
using UnityEngine;

namespace AssetLib233.Runtime
{
    [Serializable]
    public sealed class AssetManifest233
    {
        [SerializeField] private string _assetLibVersion = "0.1.0";
        [SerializeField] private string _groupName;
        [SerializeField] private string _packageVersion;
        [SerializeField] private List<AssetInfo233> _assets = new List<AssetInfo233>(1024);
        [SerializeField] private List<AssetBundleInfo233> _bundles = new List<AssetBundleInfo233>(256);

        private readonly Dictionary<string, AssetInfo233> _assetByAddress = new Dictionary<string, AssetInfo233>(1024);
        private readonly Dictionary<string, AssetInfo233> _assetByPath = new Dictionary<string, AssetInfo233>(1024);
        private readonly Dictionary<string, AssetBundleInfo233> _bundleByName = new Dictionary<string, AssetBundleInfo233>(256);
        private bool _isIndexDirty = true;

        public string AssetLibVersion
        {
            get { return _assetLibVersion; }
            set { _assetLibVersion = value; }
        }

        public string GroupName
        {
            get { return _groupName; }
            set { _groupName = value; }
        }

        public string PackageVersion
        {
            get { return _packageVersion; }
            set { _packageVersion = value; }
        }

        public IReadOnlyList<AssetInfo233> Assets
        {
            get { return _assets; }
        }

        public IReadOnlyList<AssetBundleInfo233> Bundles
        {
            get { return _bundles; }
        }

        public void SetAssets(List<AssetInfo233> assets)
        {
            _assets = assets ?? new List<AssetInfo233>(0);
            _isIndexDirty = true;
        }

        public void SetBundles(List<AssetBundleInfo233> bundles)
        {
            _bundles = bundles ?? new List<AssetBundleInfo233>(0);
            _isIndexDirty = true;
        }

        public bool TryGetAssetInfo(string address, out AssetInfo233 assetInfo)
        {
            RebuildIndexIfNeeded();
            if (_assetByAddress.TryGetValue(address, out assetInfo))
            {
                return true;
            }

            string pathAddress = BuildAddressFromPath(address);
            if (!string.IsNullOrEmpty(pathAddress) && _assetByAddress.TryGetValue(pathAddress, out assetInfo))
            {
                return true;
            }

            return _assetByPath.TryGetValue(NormalizePathKey(address), out assetInfo);
        }

        public bool TryGetAssetInfoByPath(string assetPath, out AssetInfo233 assetInfo)
        {
            RebuildIndexIfNeeded();
            if (_assetByPath.TryGetValue(NormalizePathKey(assetPath), out assetInfo))
            {
                return true;
            }

            string pathAddress = BuildAddressFromPath(assetPath);
            return !string.IsNullOrEmpty(pathAddress) && _assetByAddress.TryGetValue(pathAddress, out assetInfo);
        }

        public bool TryGetBundleInfo(string bundleName, out AssetBundleInfo233 bundleInfo)
        {
            RebuildIndexIfNeeded();
            return _bundleByName.TryGetValue(bundleName, out bundleInfo);
        }

        public void GetAssetsByTagNonAlloc(string tag, List<AssetInfo233> results)
        {
            if (results == null)
            {
                return;
            }

            results.Clear();
            if (string.IsNullOrEmpty(tag))
            {
                return;
            }

            for (int i = 0; i < _assets.Count; i++)
            {
                AssetInfo233 assetInfo = _assets[i];
                if (assetInfo != null && assetInfo.HasTag(tag))
                {
                    results.Add(assetInfo);
                }
            }
        }

        private void RebuildIndexIfNeeded()
        {
            if (!_isIndexDirty)
            {
                return;
            }

            _assetByAddress.Clear();
            _assetByPath.Clear();
            _bundleByName.Clear();

            for (int i = 0; i < _assets.Count; i++)
            {
                AssetInfo233 assetInfo = _assets[i];
                if (assetInfo == null || string.IsNullOrEmpty(assetInfo.Address))
                {
                    continue;
                }

                _assetByAddress[assetInfo.Address] = assetInfo;
                if (!string.IsNullOrEmpty(assetInfo.AssetPath))
                {
                    _assetByPath[NormalizePathKey(assetInfo.AssetPath)] = assetInfo;
                }
            }

            for (int i = 0; i < _bundles.Count; i++)
            {
                AssetBundleInfo233 bundleInfo = _bundles[i];
                if (bundleInfo == null || string.IsNullOrEmpty(bundleInfo.BundleName))
                {
                    continue;
                }

                _bundleByName[bundleInfo.BundleName] = bundleInfo;
            }

            _isIndexDirty = false;
        }

        private static string NormalizePathKey(string path)
        {
            return string.IsNullOrEmpty(path) ? string.Empty : path.Replace('\\', '/');
        }

        private static string BuildAddressFromPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            string safePath = path.Replace('\\', '/');
            int slashIndex = safePath.LastIndexOf('/');
            string fileName = slashIndex >= 0 ? safePath.Substring(slashIndex + 1) : safePath;
            int dotIndex = fileName.LastIndexOf('.');
            if (dotIndex > 0)
            {
                fileName = fileName.Substring(0, dotIndex);
            }

            return fileName;
        }
    }
}
