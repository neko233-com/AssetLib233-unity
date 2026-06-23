using System;
using UnityEngine;

namespace AssetLib233.Runtime
{
    [Serializable]
    public class AssetReference233
    {
        [SerializeField] private string _groupName = AssetLib233Constants.DefaultPackageName;
        [SerializeField] private string _address;
        [SerializeField] private string _assetGuid;

        public string GroupName
        {
            get { return _groupName; }
            set { _groupName = AssetLib233NameUtility.NormalizePackageName(value); }
        }

        public string Address
        {
            get { return _address; }
            set { _address = value; }
        }

        public string AssetGuid
        {
            get { return _assetGuid; }
            set { _assetGuid = value; }
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(_address) || !string.IsNullOrEmpty(_assetGuid);
        }
    }
}
