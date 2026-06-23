using AssetLib233.Runtime;
using UnityEngine.Networking;

namespace AssetLib233.Plugin_MiniGame_WX
{
    /// <summary>
    /// 微信小游戏下载传输层。
    /// 使用微信转换插件识别的 preload header，让真机下载进入小游戏缓存链路。
    /// </summary>
    public sealed class AssetLib233MiniGameWxDownloadTransport : AssetLib233UnityWebRequestDownloadTransport
    {
        protected override UnityWebRequest CreateRequest(string url, string tempPath)
        {
            UnityWebRequest request = base.CreateRequest(url, tempPath);
            request.SetRequestHeader("wechatminigame-preload", "1");
            return request;
        }
    }
}
