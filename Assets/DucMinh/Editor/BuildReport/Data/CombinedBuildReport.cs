using System;
using System.Collections.Generic;

namespace DucMinh.BuildReport
{
    /// <summary>
    /// Root data object for one complete build report.
    /// Combines Unity BuildReport + Addressables layout.
    /// Serialised to Assets/BuildReports/[timestamp_platform]/CombinedReport.json
    /// </summary>
    [Serializable]
    public class CombinedBuildReport
    {
        public BuildSummaryData           Summary;
        public List<BuildStepData>        BuildSteps      = new();
        public List<OutputFileData>       OutputFiles     = new();
        public List<SourceAssetData>      SourceAssets    = new();
        public List<AddressablesGroupData> AddressablesGroups = new();
        public List<DuplicationData>      Duplications    = new();

        // Computed
        public long AddressablesTotalBytes;
        public int  TotalGroups;
        public int  TotalBundles;

        public string ReportFolderPath; // absolute path on disk
    }
}
