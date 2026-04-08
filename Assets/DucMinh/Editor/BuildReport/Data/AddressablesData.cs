using System;
using System.Collections.Generic;

namespace DucMinh.BuildReport
{
    [Serializable]
    public class AddressablesAssetData
    {
        public string AssetPath;
        public string AssetGuid;
        public long   SizeBytes;
        public bool   IsImplicit;      // Not Addressable, pulled in as dependency
        public string BundleName;
        public string GroupName;
    }

    [Serializable]
    public class AddressablesBundleData
    {
        public string Name;
        public string GroupName;
        public long   SizeBytes;
        public bool   IsMonoScript;
        public List<AddressablesAssetData> Assets = new();
    }

    [Serializable]
    public class AddressablesGroupData
    {
        public string Name;
        public long   TotalSizeBytes;
        public List<AddressablesBundleData> Bundles = new();
    }

    [Serializable]
    public class DuplicationData
    {
        public string AssetPath;
        public string AssetGuid;
        public long   SizeBytes;
        public List<string> PresentInBundles = new(); // bundle names where duplicated
        public long   EstimatedWastedBytes;
    }
}
