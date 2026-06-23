using System;
using System.Collections.Generic;
using UnityEngine;

namespace AssetLib233.Runtime
{
    [Serializable]
    public sealed class AssetBundleInfo233
    {
        [SerializeField] private string _bundleName;
        [SerializeField] private string _fileName;
        [SerializeField] private string _fileHash;
        [SerializeField] private long _fileSize;
        [SerializeField] private uint _fileCrc;
        [SerializeField] private bool _isEncrypted;
        [SerializeField] private EnumAssetLib233BundleType _bundleType;
        [SerializeField] private string[] _dependBundleNames = Array.Empty<string>();
        [SerializeField] private string[] _tags = Array.Empty<string>();

        public string BundleName
        {
            get { return _bundleName; }
            set { _bundleName = value; }
        }

        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }

        public string FileHash
        {
            get { return _fileHash; }
            set { _fileHash = value; }
        }

        public long FileSize
        {
            get { return _fileSize; }
            set { _fileSize = value; }
        }

        public uint FileCrc
        {
            get { return _fileCrc; }
            set { _fileCrc = value; }
        }

        public bool IsEncrypted
        {
            get { return _isEncrypted; }
            set { _isEncrypted = value; }
        }

        public EnumAssetLib233BundleType BundleType
        {
            get { return _bundleType; }
            set { _bundleType = value; }
        }

        public IReadOnlyList<string> DependBundleNames
        {
            get { return _dependBundleNames; }
        }

        public IReadOnlyList<string> Tags
        {
            get { return _tags; }
        }

        public void SetDependBundleNames(string[] dependBundleNames)
        {
            _dependBundleNames = dependBundleNames ?? Array.Empty<string>();
        }

        public void SetTags(string[] tags)
        {
            _tags = tags ?? Array.Empty<string>();
        }
    }
}
