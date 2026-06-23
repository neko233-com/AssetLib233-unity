using System;
using System.Reflection;
using AssetLib233.Runtime;

namespace AssetLib233.Editor
{
    /// <summary>
    /// 编辑器构建侧加密配置解析器。
    /// AssetLib233 本体不依赖项目程序集；若项目提供 HotUpdate_Share 配置，则通过反射读取。
    /// </summary>
    internal static class AssetLib233EditorCryptoSettingsResolver
    {
        private const string ProjectCryptoSettingsTypeName = "Code.HotUpdate.AssetLib233HotUpdateCryptoSettings233";
        private const string ProjectCryptoSettingsAssemblyName = "HotUpdate_Share";

        public static bool ResolveEnableBundleCrypto()
        {
            Type settingsType = FindProjectCryptoSettingsType();
            if (settingsType == null)
            {
                return false;
            }

            FieldInfo fieldInfo = settingsType.GetField(
                "IsEnableBundleCryptoDefault",
                BindingFlags.Public | BindingFlags.Static);
            if (fieldInfo == null || fieldInfo.FieldType != typeof(bool))
            {
                return false;
            }

            object value = fieldInfo.GetValue(null);
            if (value is bool)
            {
                return (bool)value;
            }

            return false;
        }

        public static string ResolveBundleCryptoPassword()
        {
            Type settingsType = FindProjectCryptoSettingsType();
            if (settingsType == null)
            {
                return AssetLib233Constants.DefaultBundleCryptoPassword;
            }

            MethodInfo methodInfo = settingsType.GetMethod(
                "ResolveBundleCryptoPassword",
                BindingFlags.Public | BindingFlags.Static,
                null,
                Type.EmptyTypes,
                null);
            if (methodInfo != null && methodInfo.ReturnType == typeof(string))
            {
                object result = methodInfo.Invoke(null, null);
                string passwordFromMethod = result as string;
                if (!string.IsNullOrEmpty(passwordFromMethod))
                {
                    return passwordFromMethod;
                }
            }

            FieldInfo fieldInfo = settingsType.GetField(
                "BundleCryptoPassword",
                BindingFlags.Public | BindingFlags.Static);
            if (fieldInfo != null && fieldInfo.FieldType == typeof(string))
            {
                object value = fieldInfo.GetValue(null);
                string passwordFromField = value as string;
                if (!string.IsNullOrEmpty(passwordFromField))
                {
                    return passwordFromField;
                }
            }

            return AssetLib233Constants.DefaultBundleCryptoPassword;
        }

        public static string BuildDebugText(bool enableBundleCrypto, string bundleCryptoPassword)
        {
            Type settingsType = FindProjectCryptoSettingsType();
            if (settingsType == null)
            {
                return "AssetLib233Crypto(Editor) | projectConfig = missing | enable = " +
                       enableBundleCrypto +
                       " | password = " +
                       bundleCryptoPassword +
                       " | libraryDefault = " +
                       AssetLib233Constants.DefaultBundleCryptoPassword;
            }

            MethodInfo methodInfo = settingsType.GetMethod(
                "BuildDebugText",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(bool) },
                null);
            if (methodInfo != null && methodInfo.ReturnType == typeof(string))
            {
                object result = methodInfo.Invoke(null, new object[] { enableBundleCrypto });
                string debugText = result as string;
                if (!string.IsNullOrEmpty(debugText))
                {
                    return "AssetLib233Crypto(Editor) | projectConfig = found | " + debugText;
                }
            }

            return "AssetLib233Crypto(Editor) | projectConfig = found | enable = " +
                   enableBundleCrypto +
                   " | password = " +
                   bundleCryptoPassword +
                   " | libraryDefault = " +
                   AssetLib233Constants.DefaultBundleCryptoPassword;
        }

        private static Type FindProjectCryptoSettingsType()
        {
            Type settingsType = Type.GetType(ProjectCryptoSettingsTypeName + ", " + ProjectCryptoSettingsAssemblyName);
            if (settingsType != null)
            {
                return settingsType;
            }

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                Assembly assembly = assemblies[i];
                if (assembly == null)
                {
                    continue;
                }

                settingsType = assembly.GetType(ProjectCryptoSettingsTypeName, false);
                if (settingsType != null)
                {
                    return settingsType;
                }
            }

            return null;
        }
    }
}
