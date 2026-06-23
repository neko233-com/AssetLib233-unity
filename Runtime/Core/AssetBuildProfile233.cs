using System.Collections.Generic;
using UnityEngine;

namespace AssetLib233.Runtime
{
    [CreateAssetMenu(menuName = "neko233/AssetLib233/Build Profile", fileName = "AssetLib233BuildProfile")]
    public sealed class AssetBuildProfile233 : ScriptableObject
    {
        [SerializeField] private List<AssetGroup233> _groups = new List<AssetGroup233>(8);

        public IReadOnlyList<AssetGroup233> Groups
        {
            get { return _groups; }
        }

        public void GetGroupsNonAlloc(List<AssetGroup233> results)
        {
            if (results == null)
            {
                return;
            }

            results.Clear();
            for (int i = 0; i < _groups.Count; i++)
            {
                results.Add(_groups[i]);
            }
        }
    }
}
