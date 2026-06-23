namespace AssetLib233.Editor
{
    /// <summary>
    /// 文案策略接口。发布、验证、报告统一走这里，方便中英文输出。
    /// </summary>
    public interface IAssetLib233TextStrategy
    {
        EnumAssetLib233Language Language { get; }

        string GetText(string key);
    }
}
