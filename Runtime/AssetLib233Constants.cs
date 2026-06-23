using System.Text;

namespace AssetLib233.Runtime
{
    public static class AssetLib233Constants
    {
        public const int DefaultDownloadConcurrency = 10;
        public const long WebGLOperationTimeSliceMs = 30;
        public const string DefaultPackageName = "default";
        public const string LoginPackageName = "login";
        public const string VersionFileExtension = ".version";
        public const string ManifestFileExtension = ".manifest";
        public const string DefaultBundleCryptoPassword = "root";
    }

    public static class AssetLib233XorCrypto
    {
        public static byte[] BuildKeyBytes(string password)
        {
            string safePassword = string.IsNullOrEmpty(password)
                ? AssetLib233Constants.DefaultBundleCryptoPassword
                : password;
            return Encoding.UTF8.GetBytes(safePassword);
        }

        public static void ApplyXorInPlace(byte[] data, int offset, int count, long filePosition, string password)
        {
            byte[] keyBytes = BuildKeyBytes(password);
            ApplyXorInPlace(data, offset, count, filePosition, keyBytes);
        }

        public static void ApplyXorInPlace(byte[] data, int offset, int count, long filePosition, byte[] keyBytes)
        {
            if (data == null || keyBytes == null || keyBytes.Length == 0)
            {
                return;
            }

            int keyLength = keyBytes.Length;
            for (int i = 0; i < count; i++)
            {
                int keyIndex = (int)((filePosition + i) % keyLength);
                data[offset + i] ^= keyBytes[keyIndex];
            }
        }

        public static byte ApplyXorByte(byte rawValue, long filePosition, byte[] keyBytes)
        {
            if (keyBytes == null || keyBytes.Length == 0)
            {
                return rawValue;
            }

            int keyIndex = (int)(filePosition % keyBytes.Length);
            return (byte)(rawValue ^ keyBytes[keyIndex]);
        }
    }
}
