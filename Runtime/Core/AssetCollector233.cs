using System;
using UnityEngine;

namespace AssetLib233.Runtime
{
    [Serializable]
    public sealed class AssetCollector233
    {
        [SerializeField] private string _collectorName;
        [SerializeField] private string _assetRootPath;
        [SerializeField] private string _addressPrefix;
        [SerializeField] private string _tagText;
        [SerializeField] private bool _enabled = true;
        [SerializeField] private EnumAssetLib233CollectorPackRule _packRule = EnumAssetLib233CollectorPackRule.PackTogether;

        public string CollectorName
        {
            get { return _collectorName; }
            set { _collectorName = value; }
        }

        public string AssetRootPath
        {
            get { return _assetRootPath; }
            set { _assetRootPath = value; }
        }

        public string AddressPrefix
        {
            get { return _addressPrefix; }
            set { _addressPrefix = value; }
        }

        public string TagText
        {
            get { return _tagText; }
            set { _tagText = value; }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        public EnumAssetLib233CollectorPackRule PackRule
        {
            get { return _packRule; }
            set { _packRule = value; }
        }
    }
}
