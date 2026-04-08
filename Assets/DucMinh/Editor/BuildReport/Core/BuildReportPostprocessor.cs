using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace DucMinh.BuildReport
{
    /// <summary>
    /// Automatically captures every build (Editor + CI batchmode).
    /// Called by Unity after BuildPipeline.BuildPlayer completes.
    /// </summary>
    public class BuildReportPostprocessor : IPostprocessBuildWithReport
    {
        // Lower callbackOrder = runs sooner among all postprocessors
        public int callbackOrder => 100;

        public void OnPostprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            try
            {
                var combined = CombinedBuildReportBuilder.Build(report);
                var folder   = CombinedBuildReportStorage.Save(combined);
                Debug.Log($"[BuildReport] Report saved → {folder}");

                // Refresh AssetDatabase so the saved JSON appears in Project window
                AssetDatabase.Refresh();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[BuildReport] Failed to capture build report: {ex}");
            }
        }
    }
}
