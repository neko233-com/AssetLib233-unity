using System.Collections.Generic;

namespace AssetLib233.Runtime
{
    /// <summary>
    /// 轻量 List 池。
    /// 用途：框架内部高频临时列表复用，降低 GC；稳定优先，最大缓存数量有限，避免池子本身变成内存泄漏。
    /// </summary>
    public static class AssetLib233ListPool<T>
    {
        private const int MaxCachedListCount = 32;
        private static readonly Stack<List<T>> _cache = new Stack<List<T>>(MaxCachedListCount);

        public static List<T> Get()
        {
            if (_cache.Count > 0)
            {
                List<T> list = _cache.Pop();
                list.Clear();
                return list;
            }

            return new List<T>(32);
        }

        public static void Release(List<T> list)
        {
            if (list == null)
            {
                return;
            }

            list.Clear();
            if (_cache.Count >= MaxCachedListCount)
            {
                return;
            }

            _cache.Push(list);
        }
    }
}
