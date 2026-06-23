namespace AssetLib233.Editor
{
    /// <summary>
    /// Editor 工具链文案入口。
    /// 用户可通过 .local 的 language 字段切换 ZhCn / EnUs。
    /// </summary>
    public static class AssetLib233EditorI18n
    {
        private static readonly IAssetLib233TextStrategy _zhCnStrategy = new AssetLib233ZhCnTextStrategy();
        private static readonly IAssetLib233TextStrategy _enUsStrategy = new AssetLib233EnUsTextStrategy();
        private static IAssetLib233TextStrategy _strategy = _zhCnStrategy;

        public static EnumAssetLib233Language Language
        {
            get { return _strategy.Language; }
        }

        public static void SetLanguage(string languageText)
        {
            if (string.Equals(languageText, "en", System.StringComparison.OrdinalIgnoreCase) ||
                string.Equals(languageText, "en-us", System.StringComparison.OrdinalIgnoreCase) ||
                string.Equals(languageText, "enus", System.StringComparison.OrdinalIgnoreCase))
            {
                _strategy = _enUsStrategy;
                return;
            }

            _strategy = _zhCnStrategy;
        }

        public static string Text(string key)
        {
            return _strategy.GetText(key);
        }
    }
}
