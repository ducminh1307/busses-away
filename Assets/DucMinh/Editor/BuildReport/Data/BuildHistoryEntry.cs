using System;

namespace DucMinh.BuildReport
{
    /// <summary>
    /// Lightweight summary stored per build for the History tab.
    /// Read without loading the full CombinedReport.json.
    /// </summary>
    [Serializable]
    public class BuildHistoryEntry
    {
        public string BuildId;
        public string Platform;
        public string Result;
        public long   TotalSizeBytes;
        public double TotalTimeSeconds;
        public long   AddressablesTotalBytes;
        public int    ErrorCount;
        public int    WarningCount;
        public long   TimestampUtc;
        public string ReportFolderPath;

        public static BuildHistoryEntry FromReport(CombinedBuildReport report)
        {
            return new BuildHistoryEntry
            {
                BuildId                = report.Summary?.BuildId,
                Platform               = report.Summary?.Platform,
                Result                 = report.Summary?.Result,
                TotalSizeBytes         = report.Summary?.TotalSizeBytes ?? 0,
                TotalTimeSeconds       = report.Summary?.TotalTimeSeconds ?? 0,
                AddressablesTotalBytes = report.AddressablesTotalBytes,
                ErrorCount             = report.Summary?.ErrorCount ?? 0,
                WarningCount           = report.Summary?.WarningCount ?? 0,
                TimestampUtc           = report.Summary?.TimestampUtc ?? 0,
                ReportFolderPath       = report.ReportFolderPath
            };
        }
    }
}
