using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build.Reporting;

namespace DucMinh.BuildReport
{
    public static class UnityBuildReportMapper
    {
        public static BuildSummaryData MapSummary(BuildSummary summary)
            => BuildSummaryData.FromUnity(summary);

        public static List<BuildStepData> MapSteps(BuildStep[] steps)
        {
            var result = new List<BuildStepData>(steps.Length);
            foreach (var s in steps)
                result.Add(BuildStepData.FromUnity(s));
            return result;
        }

        public static List<OutputFileData> MapOutputFiles(BuildFile[] files)
        {
            var result = new List<OutputFileData>(files.Length);
            foreach (var f in files)
                result.Add(OutputFileData.FromUnity(f));
            return result;
        }

        public static List<SourceAssetData> MapSourceAssets(PackedAssets[] packedAssetsArray)
        {
            var dict = new Dictionary<string, SourceAssetData>();
            foreach (var packedAssets in packedAssetsArray)
            {
                foreach (var asset in packedAssets.contents)
                {
                    var data = SourceAssetData.FromUnity(asset);
                    if (!dict.TryGetValue(data.Path, out var existing))
                    {
                        dict[data.Path] = data;
                    }
                    else
                    {
                        // Accumulate size across multiple packed asset groups
                        existing.SizeBytes += data.SizeBytes;
                    }
                }
            }
            return dict.Values.ToList();
        }
    }
}
