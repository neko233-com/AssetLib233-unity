using System.Collections.Generic;
using UnityEngine;

namespace AssetLib233.Runtime
{
    /// <summary>
    /// 真实 AssetBundle Provider。
    /// 只用 Unity 主线程异步对象推进，避免 WebGL / 小游戏上线程与锁带来的不可控问题。
    /// </summary>
    public sealed class AssetLib233AssetBundleProvider : IAssetLib233AssetProvider
    {
        private readonly AssetPackage233 _assetPackage;
        private readonly Dictionary<string, AssetLib233LoadedBundle> _loadedBundleByName =
            new Dictionary<string, AssetLib233LoadedBundle>(128);
        private readonly Dictionary<string, AssetLib233BundleLoadSlot> _loadingSlotByName =
            new Dictionary<string, AssetLib233BundleLoadSlot>(32);
        private readonly Dictionary<string, string> _bundleLoadErrorByName = new Dictionary<string, string>(32);
        private readonly List<AssetLib233AssetBundleLoadOperationBase> _operations =
            new List<AssetLib233AssetBundleLoadOperationBase>(64);
        private readonly List<string> _slotDoneNames = new List<string>(16);

        public AssetLib233AssetBundleProvider(AssetPackage233 assetPackage)
        {
            _assetPackage = assetPackage;
        }

        public bool CanLoad(AssetInfo233 assetInfo)
        {
            return assetInfo != null && !string.IsNullOrEmpty(assetInfo.BundleName);
        }

        public AssetHandle233<TObject> LoadAssetAsync<TObject>(AssetPackage233 assetPackage, AssetInfo233 assetInfo)
            where TObject : Object
        {
            AssetHandle233<TObject> handle = new AssetHandle233<TObject>(
                assetPackage.PackageName,
                assetInfo == null ? string.Empty : assetInfo.Address);
            if (!CanLoad(assetInfo))
            {
                handle.SetFailed("AssetInfo 无法通过 AB Provider 加载");
                return handle;
            }

            AssetLib233AssetBundleLoadOperation<TObject> operation =
                new AssetLib233AssetBundleLoadOperation<TObject>(assetPackage, assetInfo, handle, this);
            _operations.Add(operation);
            return handle;
        }

        public void Update()
        {
            UpdateLoadingSlots();

            for (int i = _operations.Count - 1; i >= 0; i--)
            {
                AssetLib233AssetBundleLoadOperationBase operation = _operations[i];
                operation.Update();
                if (operation.IsDone)
                {
                    _operations.RemoveAt(i);
                }
            }
        }

        internal bool TryRetainLoadedBundle(string bundleName, out AssetLib233LoadedBundle loadedBundle)
        {
            if (_loadedBundleByName.TryGetValue(bundleName, out loadedBundle) && loadedBundle.Bundle != null)
            {
                loadedBundle.ReferenceCount++;
                return true;
            }

            loadedBundle = null;
            return false;
        }

        public bool TryGetBundleLoadError(string bundleName, out string error)
        {
            return _bundleLoadErrorByName.TryGetValue(bundleName, out error);
        }

        internal void EnsureBundleLoading(AssetBundleInfo233 bundleInfo)
        {
            if (bundleInfo == null || string.IsNullOrEmpty(bundleInfo.BundleName))
            {
                return;
            }

            if (_loadedBundleByName.ContainsKey(bundleInfo.BundleName))
            {
                return;
            }

            if (_loadingSlotByName.ContainsKey(bundleInfo.BundleName))
            {
                return;
            }

            _bundleLoadErrorByName.Remove(bundleInfo.BundleName);
            string localPath = AssetLib233BundlePathResolver.GetCacheBundlePath(_assetPackage.PackageName, bundleInfo);
            string builtinPath = AssetLib233BundlePathResolver.GetBuiltinBundlePath(_assetPackage.PackageName, bundleInfo);
            string cryptoPassword = _assetPackage.Config == null
                ? AssetLib233Constants.DefaultBundleCryptoPassword
                : _assetPackage.Config.BundleCryptoPassword;
            AssetLib233BundleLoadSlot slot = new AssetLib233BundleLoadSlot(
                bundleInfo,
                localPath,
                builtinPath,
                cryptoPassword);
            _loadingSlotByName.Add(bundleInfo.BundleName, slot);
            AssetLib233RuntimeDiagnostic.RecordEvent(
                "bundle-load-start group=" +
                _assetPackage.PackageName +
                " bundle=" +
                bundleInfo.BundleName +
                " cache=" +
                localPath +
                " builtin=" +
                builtinPath);
        }

        internal void ReleaseBundles(List<AssetLib233LoadedBundle> loadedBundles)
        {
            if (loadedBundles == null)
            {
                return;
            }

            for (int i = 0; i < loadedBundles.Count; i++)
            {
                AssetLib233LoadedBundle loadedBundle = loadedBundles[i];
                if (loadedBundle == null)
                {
                    continue;
                }

                loadedBundle.ReferenceCount--;
                if (loadedBundle.ReferenceCount <= 0)
                {
                    UnloadBundle(loadedBundle.BundleName);
                }
            }
        }

        public void UnloadAllBundles(bool unloadAllLoadedObjects)
        {
            List<string> bundleNames = new List<string>(_loadedBundleByName.Count);
            Dictionary<string, AssetLib233LoadedBundle>.KeyCollection keys = _loadedBundleByName.Keys;
            Dictionary<string, AssetLib233LoadedBundle>.KeyCollection.Enumerator keyEnumerator = keys.GetEnumerator();
            while (keyEnumerator.MoveNext())
            {
                bundleNames.Add(keyEnumerator.Current);
            }

            for (int i = 0; i < bundleNames.Count; i++)
            {
                UnloadBundle(bundleNames[i], unloadAllLoadedObjects);
            }
        }

        public void FillDebugBundleSnapshots(List<AssetLib233DebugBundleSnapshot> results)
        {
            if (results == null)
            {
                return;
            }

            Dictionary<string, AssetLib233LoadedBundle>.Enumerator enumerator = _loadedBundleByName.GetEnumerator();
            while (enumerator.MoveNext())
            {
                AssetLib233LoadedBundle loadedBundle = enumerator.Current.Value;
                if (loadedBundle == null)
                {
                    continue;
                }

                AssetLib233DebugBundleSnapshot snapshot = new AssetLib233DebugBundleSnapshot();
                snapshot.BundleName = loadedBundle.BundleName;
                snapshot.ReferenceCount = loadedBundle.ReferenceCount;
                snapshot.LocalPath = loadedBundle.LocalPath;
                snapshot.IsLoaded = loadedBundle.Bundle != null;
                results.Add(snapshot);
            }
        }

        private void UpdateLoadingSlots()
        {
            _slotDoneNames.Clear();
            Dictionary<string, AssetLib233BundleLoadSlot>.Enumerator enumerator = _loadingSlotByName.GetEnumerator();
            while (enumerator.MoveNext())
            {
                string bundleName = enumerator.Current.Key;
                AssetLib233BundleLoadSlot slot = enumerator.Current.Value;
                slot.Update();
                if (slot.IsDone)
                {
                    _slotDoneNames.Add(bundleName);
                }
            }

            for (int i = 0; i < _slotDoneNames.Count; i++)
            {
                string bundleName = _slotDoneNames[i];
                AssetLib233BundleLoadSlot slot = _loadingSlotByName[bundleName];
                _loadingSlotByName.Remove(bundleName);
                if (slot.Bundle == null)
                {
                    _bundleLoadErrorByName[bundleName] = slot.Error;
                    AssetLib233RuntimeDiagnostic.RecordEvent("bundle-load-fail bundle=" + bundleName + " error=" + slot.Error);
                    continue;
                }

                AssetLib233LoadedBundle loadedBundle = new AssetLib233LoadedBundle();
                loadedBundle.BundleName = bundleName;
                loadedBundle.Bundle = slot.Bundle;
                loadedBundle.ReferenceCount = 0;
                loadedBundle.LocalPath = slot.UsedPath;
                _loadedBundleByName[bundleName] = loadedBundle;
                _bundleLoadErrorByName.Remove(bundleName);
                AssetLib233RuntimeDiagnostic.RecordEvent("bundle-load-ok bundle=" + bundleName + " path=" + slot.UsedPath);
            }

            _slotDoneNames.Clear();
        }

        private void UnloadBundle(string bundleName)
        {
            UnloadBundle(bundleName, false);
        }

        private void UnloadBundle(string bundleName, bool unloadAllLoadedObjects)
        {
            if (!_loadedBundleByName.TryGetValue(bundleName, out AssetLib233LoadedBundle loadedBundle))
            {
                return;
            }

            if (loadedBundle.Bundle != null)
            {
                loadedBundle.Bundle.Unload(unloadAllLoadedObjects);
            }

            _loadedBundleByName.Remove(bundleName);
            AssetLib233RuntimeDiagnostic.RecordEvent("bundle-unload bundle=" + bundleName + " all=" + unloadAllLoadedObjects);
        }
    }
}
