using System.Collections.Generic;
using System.IO;

namespace AssetLib233.Runtime
{
    public static class AssetManifestBinarySerializer233
    {
        private const int Magic = 0x324C4132;
        private const int Version = 1;

        public static byte[] Serialize(AssetManifest233 manifest)
        {
            if (manifest == null)
            {
                return System.Array.Empty<byte>();
            }

            using (MemoryStream memoryStream = new MemoryStream(64 * 1024))
            {
                using (BinaryWriter writer = new BinaryWriter(memoryStream))
                {
                    writer.Write(Magic);
                    writer.Write(Version);
                    writer.Write(manifest.AssetLibVersion ?? string.Empty);
                    writer.Write(manifest.GroupName ?? string.Empty);
                    writer.Write(manifest.PackageVersion ?? string.Empty);
                    WriteBundles(writer, manifest.Bundles);
                    WriteAssets(writer, manifest.Assets);
                }

                return memoryStream.ToArray();
            }
        }

        public static bool TryDeserialize(byte[] bytes, out AssetManifest233 manifest, out string error)
        {
            manifest = null;
            error = string.Empty;

            if (bytes == null || bytes.Length == 0)
            {
                error = "Manifest bytes 为空";
                return false;
            }

            using (MemoryStream memoryStream = new MemoryStream(bytes))
            {
                using (BinaryReader reader = new BinaryReader(memoryStream))
                {
                    int magic = reader.ReadInt32();
                    if (magic != Magic)
                    {
                        error = "Manifest magic 不匹配";
                        return false;
                    }

                    int version = reader.ReadInt32();
                    if (version != Version)
                    {
                        error = "Manifest version 不支持: " + version;
                        return false;
                    }

                    AssetManifest233 result = new AssetManifest233();
                    result.AssetLibVersion = reader.ReadString();
                    result.GroupName = reader.ReadString();
                    result.PackageVersion = reader.ReadString();
                    result.SetBundles(ReadBundles(reader));
                    result.SetAssets(ReadAssets(reader));
                    manifest = result;
                    return true;
                }
            }
        }

        private static void WriteBundles(BinaryWriter writer, IReadOnlyList<AssetBundleInfo233> bundles)
        {
            int count = bundles != null ? bundles.Count : 0;
            writer.Write(count);
            for (int i = 0; i < count; i++)
            {
                AssetBundleInfo233 bundleInfo = bundles[i];
                writer.Write(bundleInfo.BundleName ?? string.Empty);
                writer.Write(bundleInfo.FileName ?? string.Empty);
                writer.Write(bundleInfo.FileHash ?? string.Empty);
                writer.Write(bundleInfo.FileSize);
                writer.Write(bundleInfo.FileCrc);
                writer.Write(bundleInfo.IsEncrypted);
                writer.Write((int)bundleInfo.BundleType);
                WriteStringList(writer, bundleInfo.DependBundleNames);
                WriteStringList(writer, bundleInfo.Tags);
            }
        }

        private static List<AssetBundleInfo233> ReadBundles(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            List<AssetBundleInfo233> bundles = new List<AssetBundleInfo233>(count);
            for (int i = 0; i < count; i++)
            {
                AssetBundleInfo233 bundleInfo = new AssetBundleInfo233();
                bundleInfo.BundleName = reader.ReadString();
                bundleInfo.FileName = reader.ReadString();
                bundleInfo.FileHash = reader.ReadString();
                bundleInfo.FileSize = reader.ReadInt64();
                bundleInfo.FileCrc = reader.ReadUInt32();
                bundleInfo.IsEncrypted = reader.ReadBoolean();
                bundleInfo.BundleType = (EnumAssetLib233BundleType)reader.ReadInt32();
                bundleInfo.SetDependBundleNames(ReadStringArray(reader));
                bundleInfo.SetTags(ReadStringArray(reader));
                bundles.Add(bundleInfo);
            }

            return bundles;
        }

        private static void WriteAssets(BinaryWriter writer, IReadOnlyList<AssetInfo233> assets)
        {
            int count = assets != null ? assets.Count : 0;
            writer.Write(count);
            for (int i = 0; i < count; i++)
            {
                AssetInfo233 assetInfo = assets[i];
                writer.Write(assetInfo.Address ?? string.Empty);
                writer.Write(assetInfo.AssetPath ?? string.Empty);
                writer.Write(assetInfo.BundleName ?? string.Empty);
                WriteStringList(writer, assetInfo.Tags);
            }
        }

        private static List<AssetInfo233> ReadAssets(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            List<AssetInfo233> assets = new List<AssetInfo233>(count);
            for (int i = 0; i < count; i++)
            {
                AssetInfo233 assetInfo = new AssetInfo233();
                assetInfo.Address = reader.ReadString();
                assetInfo.AssetPath = reader.ReadString();
                assetInfo.BundleName = reader.ReadString();
                assetInfo.SetTags(ReadStringArray(reader));
                assets.Add(assetInfo);
            }

            return assets;
        }

        private static void WriteStringList(BinaryWriter writer, IReadOnlyList<string> values)
        {
            int count = values != null ? values.Count : 0;
            writer.Write(count);
            for (int i = 0; i < count; i++)
            {
                writer.Write(values[i] ?? string.Empty);
            }
        }

        private static string[] ReadStringArray(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            string[] values = new string[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = reader.ReadString();
            }

            return values;
        }
    }
}
