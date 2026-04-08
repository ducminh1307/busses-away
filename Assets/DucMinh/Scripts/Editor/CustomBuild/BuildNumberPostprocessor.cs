using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using BuildResult = UnityEditor.Build.Reporting.BuildResult;

namespace DucMinh.CustomBuild
{
    /// <summary>
    /// Automatically increments the bundle version code (buildNumber) in the selected
    /// BuildConfig after every successful player build.
    ///
    /// Priority 200 – runs AFTER BuildReportPostprocessor (100) so the report is 
    /// already saved before we mutate the config.
    /// </summary>
    public class BuildNumberPostprocessor : IPostprocessBuildWithReport
    {
        public int callbackOrder => 200;

        public void OnPostprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            // Only increment on successful builds
            if (report.summary.result != BuildResult.Succeeded)
                return;

            var database = BuildConfigDatabase.Instance;
            if (database == null)
            {
                Debug.LogWarning("[BuildNumber] BuildConfigDatabase not found – bundle code NOT incremented.");
                return;
            }

            var config = database.SelectedConfig;
            if (config == null)
            {
                Debug.LogWarning("[BuildNumber] No selected BuildConfig – bundle code NOT incremented.");
                return;
            }

            int oldCode = config.buildNumber;
            config.buildNumber++;

            // Persist the change
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();

            Debug.Log($"[BuildNumber] Build succeeded → bundle code incremented: {oldCode} → {config.buildNumber} (config: {config.ConfigName})");
        }
    }
}
