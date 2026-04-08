using System;
using UnityEditor.Build.Reporting;

namespace DucMinh.BuildReport
{
    [Serializable]
    public class BuildSummaryData
    {
        public string BuildId;
        public string Platform;
        public string BuildTarget;
        public string Result;           // Succeeded / Failed / Cancelled
        public long   TotalSizeBytes;
        public double TotalTimeSeconds;
        public int    ErrorCount;
        public int    WarningCount;
        public string BuildOutputPath;
        public long   TimestampUtc;     // Unix timestamp (UTC)

        public static BuildSummaryData FromUnity(BuildSummary summary)
        {
            return new BuildSummaryData
            {
                BuildId         = Guid.NewGuid().ToString(),
                Platform        = summary.platform.ToString(),
                BuildTarget     = summary.platformGroup.ToString(),
                Result          = summary.result.ToString(),
                TotalSizeBytes  = (long)summary.totalSize,
                TotalTimeSeconds = summary.totalTime.TotalSeconds,
                ErrorCount      = summary.totalErrors,
                WarningCount    = summary.totalWarnings,
                BuildOutputPath = summary.outputPath,
                TimestampUtc    = new DateTimeOffset(summary.buildEndedAt).ToUnixTimeSeconds()
            };
        }
    }
}
