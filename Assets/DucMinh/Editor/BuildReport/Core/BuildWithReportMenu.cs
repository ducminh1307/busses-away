using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace DucMinh.BuildReport
{
    public static class BuildWithReportMenu
    {
        [MenuItem("Tools/Build/Build With Report")]
        private static void BuildWithReport()
        {
            var scenes = EditorBuildSettings.scenes;
            if (scenes.Length == 0)
            {
                Debug.LogWarning("[BuildReport] No scenes in Build Settings. Add scenes first.");
                return;
            }

            var options = new BuildPlayerOptions
            {
                scenes      = System.Array.ConvertAll(scenes, s => s.path),
                locationPathName = EditorUserBuildSettings.GetBuildLocation(EditorUserBuildSettings.activeBuildTarget),
                target      = EditorUserBuildSettings.activeBuildTarget,
                options     = BuildOptions.DetailedBuildReport
            };

            if (string.IsNullOrEmpty(options.locationPathName))
                options.locationPathName = EditorUtility.SaveFolderPanel("Choose Output Folder", "", "");

            if (string.IsNullOrEmpty(options.locationPathName)) return;

            UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == BuildResult.Succeeded)
                Debug.Log($"[BuildReport] Build succeeded. Report captured automatically.");
            else
                Debug.LogWarning($"[BuildReport] Build {report.summary.result}. Check console for errors.");
        }
    }
}
