using System;
using UnityEditor.Build.Reporting;

namespace DucMinh.BuildReport
{
    [Serializable]
    public class OutputFileData
    {
        public string Path;
        public string Role;
        public long   SizeBytes;

        public static OutputFileData FromUnity(BuildFile file)
        {
            return new OutputFileData
            {
                Path      = file.path,
                Role      = file.role,
                SizeBytes = (long)file.size
            };
        }
    }
}
