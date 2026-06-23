namespace AssetLib233.Runtime
{
    public static class AssetLib233NameUtility
    {
        public static string NormalizePackageName(string packageName)
        {
            if (string.IsNullOrWhiteSpace(packageName))
            {
                return AssetLib233Constants.DefaultPackageName;
            }

            return packageName.Trim();
        }

        public static string NormalizeHost(string host)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                return string.Empty;
            }

            return host.Replace('\\', '/').TrimEnd('/');
        }
    }
}
