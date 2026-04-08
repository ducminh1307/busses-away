using System.Collections.Generic;
using System.Linq;

namespace DucMinh.BuildReport
{
    public static class CombinedBuildReportBuilder
    {
        public static CombinedBuildReport Build(UnityEditor.Build.Reporting.BuildReport unityReport)
        {
            var summary      = UnityBuildReportMapper.MapSummary(unityReport.summary);
            var steps        = UnityBuildReportMapper.MapSteps(unityReport.steps);
            var outputFiles  = UnityBuildReportMapper.MapOutputFiles(unityReport.GetFiles());
            var sourceAssets = UnityBuildReportMapper.MapSourceAssets(unityReport.packedAssets);

            var (addrGroups, duplications) = AddressablesLayoutParser.Parse();

            long addrTotal = addrGroups.Sum(g => g.TotalSizeBytes);
            int  bundles   = addrGroups.Sum(g => g.Bundles.Count);

            return new CombinedBuildReport
            {
                Summary               = summary,
                BuildSteps            = steps,
                OutputFiles           = outputFiles,
                SourceAssets          = sourceAssets,
                AddressablesGroups    = addrGroups,
                Duplications          = duplications,
                AddressablesTotalBytes = addrTotal,
                TotalGroups           = addrGroups.Count,
                TotalBundles          = bundles
            };
        }
    }
}
