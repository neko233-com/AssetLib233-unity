using System.Collections.Generic;
using UnityEngine;

namespace AssetLib233.Runtime
{
    internal abstract class AssetLib233AssetBundleLoadOperationBase
    {
        public bool IsDone { get; protected set; }
        public abstract void Update();
    }

    internal sealed class AssetLib233AssetBundleLoadOperation<TObject> : AssetLib233AssetBundleLoadOperationBase
        where TObject : Object
    {
        private readonly AssetPackage233 _assetPackage;
        private readonly AssetInfo233 _assetInfo;
        private readonly AssetHandle233<TObject> _handle;
        private readonly AssetLib233AssetBundleProvider _provider;
        private readonly List<AssetBundleInfo233> _bundleLoadList = new List<AssetBundleInfo233>(8);
        private readonly List<AssetLib233LoadedBundle> _retainedBundles = new List<AssetLib233LoadedBundle>(8);
        private AssetBundleRequest _assetRequest;
        private int _bundleLoadIndex;
        private bool _isAssetRequestStarted;
        private bool _isReleaseCallbackBound;

        public AssetLib233AssetBundleLoadOperation(
            AssetPackage233 assetPackage,
            AssetInfo233 assetInfo,
            AssetHandle233<TObject> handle,
            AssetLib233AssetBundleProvider provider)
        {
            _assetPackage = assetPackage;
            _assetInfo = assetInfo;
            _handle = handle;
            _provider = provider;
            BuildBundleLoadList();
        }

        public override void Update()
        {
            if (IsDone)
            {
                return;
            }

            if (_bundleLoadList.Count == 0)
            {
                Fail("资源没有绑定 AB. address = " + _assetInfo.Address + " | path = " + _assetInfo.AssetPath);
                return;
            }

            if (_bundleLoadIndex < _bundleLoadList.Count)
            {
                UpdateBundleLoading();
                return;
            }

            UpdateAssetLoading();
        }

        private void BuildBundleLoadList()
        {
            if (_assetPackage.Manifest == null)
            {
                return;
            }

            if (!_assetPackage.Manifest.TryGetBundleInfo(_assetInfo.BundleName, out AssetBundleInfo233 mainBundleInfo))
            {
                return;
            }

            AppendBundleAndDependencies(mainBundleInfo);
        }

        private void AppendBundleAndDependencies(AssetBundleInfo233 bundleInfo)
        {
            if (bundleInfo == null)
            {
                return;
            }

            IReadOnlyList<string> dependBundleNames = bundleInfo.DependBundleNames;
            for (int i = 0; i < dependBundleNames.Count; i++)
            {
                string dependBundleName = dependBundleNames[i];
                if (string.IsNullOrEmpty(dependBundleName))
                {
                    continue;
                }

                if (_assetPackage.Manifest.TryGetBundleInfo(dependBundleName, out AssetBundleInfo233 dependBundleInfo))
                {
                    AppendBundleAndDependencies(dependBundleInfo);
                }
            }

            if (!ContainsBundle(bundleInfo.BundleName))
            {
                _bundleLoadList.Add(bundleInfo);
            }
        }

        private bool ContainsBundle(string bundleName)
        {
            for (int i = 0; i < _bundleLoadList.Count; i++)
            {
                AssetBundleInfo233 bundleInfo = _bundleLoadList[i];
                if (bundleInfo != null && bundleInfo.BundleName == bundleName)
                {
                    return true;
                }
            }

            return false;
        }

        private void UpdateBundleLoading()
        {
            AssetBundleInfo233 bundleInfo = _bundleLoadList[_bundleLoadIndex];
            if (_provider.TryRetainLoadedBundle(bundleInfo.BundleName, out AssetLib233LoadedBundle loadedBundle))
            {
                _retainedBundles.Add(loadedBundle);
                _bundleLoadIndex++;
                float progress = _bundleLoadIndex / (float)(_bundleLoadList.Count + 1);
                _handle.SetProgress(progress);
                return;
            }

            if (_provider.TryGetBundleLoadError(bundleInfo.BundleName, out string error))
            {
                Fail(error);
                return;
            }

            _provider.EnsureBundleLoading(bundleInfo);
        }

        private void UpdateAssetLoading()
        {
            AssetLib233LoadedBundle mainBundle = FindRetainedBundle(_assetInfo.BundleName);
            if (mainBundle == null || mainBundle.Bundle == null)
            {
                Fail("主 AB 未加载成功. bundle = " + _assetInfo.BundleName);
                return;
            }

            if (!_isAssetRequestStarted)
            {
                _isAssetRequestStarted = true;
                string assetPath = string.IsNullOrEmpty(_assetInfo.AssetPath) ? _assetInfo.Address : _assetInfo.AssetPath;
                _assetRequest = mainBundle.Bundle.LoadAssetAsync<TObject>(assetPath);
                _handle.SetProgress(0.95f);
                return;
            }

            if (_assetRequest == null || !_assetRequest.isDone)
            {
                return;
            }

            TObject assetObject = _assetRequest.asset as TObject;
            _assetRequest = null;
            if (assetObject == null)
            {
                Fail(
                    "资源加载为空. group = " +
                    _assetPackage.PackageName +
                    " | address = " +
                    _assetInfo.Address +
                    " | path = " +
                    _assetInfo.AssetPath +
                    " | bundle = " +
                    _assetInfo.BundleName +
                    " | type = " +
                    typeof(TObject).Name);
                return;
            }

            BindReleaseCallbackIfNeeded();
            _handle.SetResult(assetObject);
            AssetLib233RuntimeDiagnostic.RecordEvent(
                "load-ok group=" +
                _assetPackage.PackageName +
                " address=" +
                _assetInfo.Address +
                " bundle=" +
                _assetInfo.BundleName +
                " type=" +
                typeof(TObject).Name);
            IsDone = true;
        }

        private AssetLib233LoadedBundle FindRetainedBundle(string bundleName)
        {
            for (int i = 0; i < _retainedBundles.Count; i++)
            {
                AssetLib233LoadedBundle loadedBundle = _retainedBundles[i];
                if (loadedBundle != null && loadedBundle.BundleName == bundleName)
                {
                    return loadedBundle;
                }
            }

            return null;
        }

        private void BindReleaseCallbackIfNeeded()
        {
            if (_isReleaseCallbackBound)
            {
                return;
            }

            _isReleaseCallbackBound = true;
            _handle.SetReleaseCallback(ReleaseRetainedBundles);
        }

        private void Fail(string error)
        {
            ReleaseRetainedBundles();
            _handle.SetFailed(error);
            AssetLib233RuntimeDiagnostic.RecordEvent("load-fail " + error);
            IsDone = true;
        }

        private void ReleaseRetainedBundles()
        {
            if (_retainedBundles.Count == 0)
            {
                return;
            }

            _provider.ReleaseBundles(_retainedBundles);
            _retainedBundles.Clear();
        }
    }
}
