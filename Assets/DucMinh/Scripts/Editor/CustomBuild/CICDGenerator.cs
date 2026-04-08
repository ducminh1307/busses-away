using System.IO;
using UnityEditor;

namespace DucMinh.CustomBuild
{
    /// <summary>
    /// Generates CI/CD batch and shell scripts
    /// </summary>
    public static class CICDGenerator
    {
        /// <summary>
        /// Generates CI/CD batch and shell scripts for the currently selected config
        /// </summary>
        public static void GenerateScripts()
        {
            var database = BuildConfigDatabase.Instance;
            if (database == null)
            {
                UnityEngine.Debug.LogError("[CICDGenerator] BuildConfigDatabase not found!");
                return;
            }

            var config = database.SelectedConfig;
            GenerateScriptsForConfig(config);
        }

        /// <summary>
        /// Generates CI/CD batch and shell scripts for a specific config
        /// </summary>
        private static void GenerateScriptsForConfig(BuildConfig config)
        {
            UnityEngine.Debug.Log($"[CICDGenerator] Generating CI/CD scripts for config: {config.ConfigName}");

            string unityPath = EditorApplication.applicationPath;
            string projectPath = Directory.GetCurrentDirectory();
            string configName = config.ConfigName;

            // Generate Windows batch script
            string batchScript = $@"@echo off
echo ========================================
echo Auto Builder - Windows Batch Script
echo Config: {configName}
echo ========================================

""{unityPath}"" -quit -batchmode -projectPath ""{projectPath}"" -executeMethod DucMinh.CustomBuild.BuildEngine.BuildFromCommandLine -configName ""{configName}"" -logFile build_{configName}.log

echo Build process completed!
pause
";

            string batchFileName = $"Build_{configName}_Windows.bat";
            File.WriteAllText(batchFileName, batchScript);
            UnityEngine.Debug.Log($"[CICDGenerator] Generated {batchFileName}");

            // Generate Mac/Linux shell script
            string shellScript = $@"#!/bin/bash
echo ""=======================================""
echo ""Auto Builder - Mac/Linux Shell Script""
echo ""Config: {configName}""
echo ""=======================================""

""{unityPath}"" -quit -batchmode -projectPath ""{projectPath}"" -executeMethod DucMinh.CustomBuild.BuildEngine.BuildFromCommandLine -configName ""{configName}"" -logFile build_{configName}.log

echo ""Build process completed!""
";

            string shellFileName = $"Build_{configName}_Mac.sh";
            File.WriteAllText(shellFileName, shellScript);
            UnityEngine.Debug.Log($"[CICDGenerator] Generated {shellFileName}");

            AssetDatabase.Refresh();
            EditorUtility.RevealInFinder(batchFileName);

            UnityEngine.Debug.Log($"[CICDGenerator] CI/CD script generation completed for {configName}!");
        }
    }
}
