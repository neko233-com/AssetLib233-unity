using AssetLib233.Runtime;
using Cysharp.Threading.Tasks;

namespace AssetLib233.Plugin_UniTask
{
    /// <summary>
    /// UniTask 适配扩展。Runtime 不依赖 UniTask；需要时引用本插件程序集即可。
    /// </summary>
    public static class AssetLib233UniTaskExtensions
    {
        public static async UniTask<TOperation> ToUniTask<TOperation>(this TOperation operation)
            where TOperation : AssetLib233Operation
        {
            if (operation == null)
            {
                return null;
            }

            while (!operation.IsDone)
            {
                operation.Update();
                await UniTask.Yield();
            }

            return operation;
        }
    }
}
