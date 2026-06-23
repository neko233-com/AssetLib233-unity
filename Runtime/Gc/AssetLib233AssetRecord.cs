using UnityEngine;

namespace AssetLib233.Runtime
{
    /// <summary>
    /// 单个资源引用记录。
    /// 记录引用数、最后释放时间和资源对象，供自动 / 手动 GC 判断。
    /// </summary>
    public sealed class AssetLib233AssetRecord
    {
        public string GroupName;
        public string Address;
        public Object AssetObject;
        public int ReferenceCount;
        public float LastZeroReferenceTime;
        public bool MarkedForRelease;

        public string Key
        {
            get { return GroupName + "::" + Address; }
        }
    }
}
