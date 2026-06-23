using System.Collections.Generic;
using UnityEngine;

namespace AssetLib233.Runtime
{
    [CreateAssetMenu(menuName = "AssetLib233/Settings", fileName = "AssetLib233Settings")]
    public sealed class AssetLib233Settings : ScriptableObject
    {
        [SerializeField] private List<AssetLib233PackageConfig> _packages = new List<AssetLib233PackageConfig>();

        public IReadOnlyList<AssetLib233PackageConfig> Packages
        {
            get { return _packages; }
        }

        public void GetPackagesNonAlloc(List<AssetLib233PackageConfig> results)
        {
            if (results == null)
            {
                return;
            }

            results.Clear();
            for (int i = 0; i < _packages.Count; i++)
            {
                results.Add(_packages[i]);
            }
        }
    }
}
