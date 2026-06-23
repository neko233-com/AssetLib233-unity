using System;
using System.Collections.Generic;

namespace AssetLib233.Editor
{
    /// <summary>
    /// 发布报告。
    /// 每次 build/upload/refresh 都生成单独报告文件，方便回溯 CDN、构建产物和外部工具日志。
    /// </summary>
    [Serializable]
    public sealed class AssetLib233EditorPublishReport
    {
        public string reportId = "";
        public string startTimeUtc = "";
        public string endTimeUtc = "";
        public string projectPath = "";
        public string buildOutputRoot = "";
        public string cdnProvider = "";
        public string cdnRegion = "";
        public string cdnBucket = "";
        public string cdnPathPrefix = "";
        public string cdnGoToolConfigPath = "";
        public string agentValidationPlatform = "";
        public string agentValidationEnvironment = "";
        public string cdnRootUrl = "";
        public bool success;
        public List<AssetLib233EditorPublishReportStep> steps =
            new List<AssetLib233EditorPublishReportStep>(16);

        public void AddStep(AssetLib233EditorPublishReportStep step)
        {
            if (step == null)
            {
                return;
            }

            steps.Add(step);
        }
    }
}
