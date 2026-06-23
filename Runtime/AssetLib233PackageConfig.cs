using UnityEngine;

namespace AssetLib233.Runtime
{
    [System.Serializable]
    public sealed class AssetLib233PackageConfig
    {
        [SerializeField] private string _packageName = AssetLib233Constants.DefaultPackageName;
        [SerializeField] private EnumAssetLib233PlayMode _playMode = EnumAssetLib233PlayMode.Host;
        [SerializeField] private string _defaultHostServer = "";
        [SerializeField] private string _fallbackHostServer = "";
        [SerializeField] private bool _autoUnloadBundleWhenUnused = true;
        [SerializeField] private bool _enableBundleCrypto;
        [SerializeField] private long _operationTimeSliceMs = AssetLib233Constants.WebGLOperationTimeSliceMs;
        [SerializeField] private int _downloadConcurrency = AssetLib233Constants.DefaultDownloadConcurrency;

        public string PackageName
        {
            get { return _packageName; }
            set { _packageName = AssetLib233NameUtility.NormalizePackageName(value); }
        }

        public EnumAssetLib233PlayMode PlayMode
        {
            get { return _playMode; }
            set { _playMode = value; }
        }

        public string DefaultHostServer
        {
            get { return _defaultHostServer; }
            set { _defaultHostServer = AssetLib233NameUtility.NormalizeHost(value); }
        }

        public string FallbackHostServer
        {
            get { return _fallbackHostServer; }
            set { _fallbackHostServer = AssetLib233NameUtility.NormalizeHost(value); }
        }

        public bool AutoUnloadBundleWhenUnused
        {
            get { return _autoUnloadBundleWhenUnused; }
            set { _autoUnloadBundleWhenUnused = value; }
        }

        public bool EnableBundleCrypto
        {
            get { return _enableBundleCrypto; }
            set { _enableBundleCrypto = value; }
        }

        public long OperationTimeSliceMs
        {
            get { return _operationTimeSliceMs; }
            set { _operationTimeSliceMs = value; }
        }

        public int DownloadConcurrency
        {
            get { return _downloadConcurrency; }
            set { _downloadConcurrency = value; }
        }
    }
}
