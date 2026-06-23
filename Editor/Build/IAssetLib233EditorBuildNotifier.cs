namespace AssetLib233.Editor
{
    /// <summary>
    /// 构建通知扩展点。
    /// 项目可注册飞书、企业微信、钉钉、自建平台等通知器。
    /// </summary>
    public interface IAssetLib233EditorBuildNotifier
    {
        void OnBuildReport(AssetLib233EditorBuildReport report);
    }
}
