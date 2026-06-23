using System.IO;
using AssetLib233.Runtime;

namespace AssetLib233.Editor
{
    /// <summary>
    /// 默认产物校验器。
    /// 检查 Manifest 中声明的每个 Bundle 文件是否存在、大小是否匹配。
    /// </summary>
    public sealed class AssetLib233DefaultBuildVerifier : IAssetLib233BuildVerifier
    {
        public bool Verify(AssetLib233BuildVerifyContext context, out string error)
        {
            error = string.Empty;
            if (context == null)
            {
                error = "校验上下文为空";
                return false;
            }

            if (context.manifest == null)
            {
                error = "Manifest 为空";
                return false;
            }

            bool isOk = true;
            for (int i = 0; i < context.manifest.Bundles.Count; i++)
            {
                AssetBundleInfo233 bundleInfo = context.manifest.Bundles[i];
                string filePath = Path.Combine(context.outputRoot, bundleInfo.FileName);
                if (!File.Exists(filePath))
                {
                    context.errors.Add("Bundle 不存在: " + filePath);
                    isOk = false;
                    continue;
                }

                FileInfo fileInfo = new FileInfo(filePath);
                if (bundleInfo.FileSize > 0 && fileInfo.Length != bundleInfo.FileSize)
                {
                    context.errors.Add("Bundle 大小不匹配: " + filePath);
                    isOk = false;
                }
            }

            if (!isOk && context.errors.Count > 0)
            {
                error = context.errors[0];
            }

            return isOk;
        }
    }
}
