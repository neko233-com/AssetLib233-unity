namespace AssetLib233.Editor
{
    /// <summary>
    /// 火山引擎中国 CDN provider。
    /// 默认策略：使用 .local 配置中的 upload / refresh 外部命令，便于接入火山官方 CLI 或内部 Go 发布工具。
    /// </summary>
    public sealed class AssetLib233VolcengineChinaCdnProvider : AssetLib233ExternalCommandCdnProvider
    {
        public AssetLib233VolcengineChinaCdnProvider()
            : base(EnumAssetLib233CdnProvider.VolcengineChina, "VolcengineChina")
        {
        }

        public override bool Refresh(AssetLib233CdnProviderContext context)
        {
            bool isHandled;
            bool success = AssetLib233CdnGoToolAdapter.TryRun(
                context.Config,
                context.Report,
                "VolcengineChina_GoRefreshVerify",
                context.Config.cdnGoToolCommand,
                context.TimeoutMilliseconds,
                out isHandled);
            if (isHandled)
            {
                return success;
            }

            return base.Refresh(context);
        }
    }
}
