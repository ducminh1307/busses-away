using System;
using System.Linq;
using UnityEditor.Build.Reporting;

namespace DucMinh.BuildReport
{
    [Serializable]
    public class BuildStepData
    {
        public string Name;
        public double DurationSeconds;
        public int    Depth;

        public static BuildStepData FromUnity(BuildStep step)
        {
            return new BuildStepData
            {
                Name            = step.name,
                DurationSeconds = step.duration.TotalSeconds,
                Depth           = step.depth
            };
        }
    }
}
