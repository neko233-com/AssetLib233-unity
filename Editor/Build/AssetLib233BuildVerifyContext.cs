using System;
using System.Collections.Generic;
using AssetLib233.Runtime;

namespace AssetLib233.Editor
{
    [Serializable]
    public sealed class AssetLib233BuildVerifyContext
    {
        public string platformName = "";
        public string outputRoot = "";
        public AssetManifest233 manifest;
        public List<string> errors = new List<string>(32);
    }
}
