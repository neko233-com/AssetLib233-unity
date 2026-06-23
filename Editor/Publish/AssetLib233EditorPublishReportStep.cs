using System;

namespace AssetLib233.Editor
{
    [Serializable]
    public sealed class AssetLib233EditorPublishReportStep
    {
        public string name = "";
        public string command = "";
        public string workingDirectory = "";
        public int exitCode;
        public string startTimeUtc = "";
        public string endTimeUtc = "";
        public string logPath = "";
        public string message = "";
    }
}
