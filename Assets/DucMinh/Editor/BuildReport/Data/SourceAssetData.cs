using System;
using UnityEditor.Build.Reporting;

namespace DucMinh.BuildReport
{
    [Serializable]
    public class SourceAssetData
    {
        public string Path;
        public string AssetType;
        public long   SizeBytes;
        // Scenes that reference this asset (populated by detailed build report)
        public string[] Scenes;

        public static SourceAssetData FromUnity(PackedAssetInfo asset)
        {
            return new SourceAssetData
            {
                Path      = asset.sourceAssetPath,
                AssetType = asset.type?.Name ?? "Unknown",
                SizeBytes = (long)asset.packedSize,
                Scenes    = Array.Empty<string>()
            };
        }
    }
}
