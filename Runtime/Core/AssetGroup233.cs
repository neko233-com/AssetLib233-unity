using System;
using System.Collections.Generic;
using UnityEngine;

namespace AssetLib233.Runtime
{
    [Serializable]
    public sealed class AssetGroup233
    {
        [SerializeField] private string _groupName = AssetLib233Constants.DefaultPackageName;
        [SerializeField] private bool _requiredOnFirstEnter;
        [SerializeField] private bool _builtin;
        [SerializeField] private List<AssetCollector233> _collectors = new List<AssetCollector233>(8);

        public string GroupName
        {
            get { return _groupName; }
            set { _groupName = AssetLib233NameUtility.NormalizePackageName(value); }
        }

        public bool RequiredOnFirstEnter
        {
            get { return _requiredOnFirstEnter; }
            set { _requiredOnFirstEnter = value; }
        }

        public bool Builtin
        {
            get { return _builtin; }
            set { _builtin = value; }
        }

        public IReadOnlyList<AssetCollector233> Collectors
        {
            get { return _collectors; }
        }

        public void GetCollectorsNonAlloc(List<AssetCollector233> results)
        {
            if (results == null)
            {
                return;
            }

            results.Clear();
            for (int i = 0; i < _collectors.Count; i++)
            {
                results.Add(_collectors[i]);
            }
        }

        public void ClearCollectors()
        {
            _collectors.Clear();
        }

        public void AddCollector(AssetCollector233 collector)
        {
            if (collector == null)
            {
                return;
            }

            _collectors.Add(collector);
        }
    }
}
