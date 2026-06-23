using System;
using System.Collections.Generic;
using UnityEngine;

namespace AssetLib233.Runtime
{
    [Serializable]
    public sealed class AssetInfo233
    {
        [SerializeField] private string _address;
        [SerializeField] private string _assetPath;
        [SerializeField] private string _bundleName;
        [SerializeField] private string[] _tags = Array.Empty<string>();

        public string Address
        {
            get { return _address; }
            set { _address = value; }
        }

        public string AssetPath
        {
            get { return _assetPath; }
            set { _assetPath = value; }
        }

        public string BundleName
        {
            get { return _bundleName; }
            set { _bundleName = value; }
        }

        public IReadOnlyList<string> Tags
        {
            get { return _tags; }
        }

        public void SetTags(string[] tags)
        {
            _tags = tags ?? Array.Empty<string>();
        }

        public bool HasTag(string tag)
        {
            if (string.IsNullOrEmpty(tag) || _tags == null)
            {
                return false;
            }

            for (int i = 0; i < _tags.Length; i++)
            {
                if (_tags[i] == tag)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
