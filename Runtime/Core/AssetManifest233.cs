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
            return _assetByAddress.TryGetValue(address, out assetInfo);
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
            _bundleByName.Clear();

            for (int i = 0; i < _assets.Count; i++)
            {
                AssetInfo233 assetInfo = _assets[i];
                if (assetInfo == null || string.IsNullOrEmpty(assetInfo.Address))
                {
                    continue;
                }

                _assetByAddress[assetInfo.Address] = assetInfo;
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
    }
}
