using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace DucMinh.CustomBuild
{
    /// <summary>
    /// Core build engine that handles the entire build process
    /// </summary>
    public static class BuildEngine
    {

        /// <summary>
        /// Main build process orchestrator
        /// </summary>
        public static void RunBuildProcess(BuildConfig config, bool isAab)
        {
            Debug.Log($"[BuildEngine] Starting build process - Type: {config.buildType}, Format: {(isAab ? "AAB" : "APK")}");

            // Step 1: Manage Define Symbols
            ManageDefineSymbols(config);

            // Step 2: Apply Build Optimizations
            ApplyBuildOptimizations(config);

            // Step 3: Update Version Info
            PlayerSettings.bundleVersion = config.version;
            PlayerSettings.Android.bundleVersionCode = config.buildNumber;

            // Step 4: Construct Output Path
            string extension = isAab ? "aab" : "apk";
            string buildPath = Path.Combine("Builds", $"{config.baseFileName}_{config.buildType}_v{config.version}_{config.buildNumber}.{extension}");

            // Ensure Builds directory exists
            Directory.CreateDirectory("Builds");

            // Step 5: Get all enabled scenes
            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                Debug.LogError("[BuildEngine] No scenes enabled in Build Settings!");
                return;
            }

            // Step 6: Set build options based on build type
            BuildOptions buildOptions = BuildOptions.None;
            if (config.buildType == BuildConfig.BuildType.Debug)
            {
                buildOptions = BuildOptions.Development | BuildOptions.AllowDebugging;
            }
            else
            {
                buildOptions = BuildOptions.CompressWithLz4HC;
            }

            // Set AAB or APK
            EditorUserBuildSettings.buildAppBundle = isAab;

            // Step 7: Build the player
            Debug.Log($"[BuildEngine] Building to: {buildPath}");
            var report = BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.Android, buildOptions);

            // Step 8: Check build result
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Failed)
            {
                Debug.LogError("[BuildEngine] Build failed!");
                return;
            }

            Debug.Log("[BuildEngine] Build succeeded!");

            // Step 9: Firebase Upload
            if (config.uploadToFirebase)
            {
                FirebaseService.UploadToFirebase(config, buildPath);
            }

            // Step 10: Send Notifications
            if (config.enableNotifications)
            {
                long sizeBytes = new FileInfo(buildPath).Length;
                NotificationService.BroadcastNotifications(config, buildPath, sizeBytes);
            }

            // NOTE: buildNumber is incremented automatically by BuildNumberPostprocessor
            // (IPostprocessBuildWithReport with callbackOrder 200) after every successful build.
            Debug.Log("[BuildEngine] Build process completed!");
        }

        /// <summary>
        /// Manages define symbols based on build type
        /// </summary>
        private static void ManageDefineSymbols(BuildConfig config)
        {
            NamedBuildTarget namedTarget = NamedBuildTarget.Android;
            string currentSymbols = PlayerSettings.GetScriptingDefineSymbols(namedTarget);

            // Split symbols by semicolon
            List<string> symbolList = currentSymbols.Split(';')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

            if (config.buildType == BuildConfig.BuildType.Release)
            {
                // Remove DEBUG_MODE for release builds
                symbolList.RemoveAll(s => s == "DEBUG_MODE");
                Debug.Log("[BuildEngine] Removed DEBUG_MODE symbol for Release build");
            }
            else
            {
                // Add DEBUG_MODE for debug builds if not already present
                if (!symbolList.Contains("DEBUG_MODE"))
                {
                    symbolList.Add("DEBUG_MODE");
                    Debug.Log("[BuildEngine] Added DEBUG_MODE symbol for Debug build");
                }
            }

            // Update symbols
            string newSymbols = string.Join(";", symbolList);
            PlayerSettings.SetScriptingDefineSymbols(namedTarget, newSymbols);
        }

        /// <summary>
        /// Applies build optimizations based on build type
        /// </summary>
        private static void ApplyBuildOptimizations(BuildConfig config)
        {
            NamedBuildTarget namedTarget = NamedBuildTarget.Android;

            if (config.buildType == BuildConfig.BuildType.Debug)
            {
                // Debug: Use Mono for faster builds
                PlayerSettings.SetScriptingBackend(namedTarget, ScriptingImplementation.Mono2x);
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
                Debug.Log("[BuildEngine] Applied Debug optimizations: Mono2x, ARMv7");
            }
            else
            {
                // Release: Use IL2CPP for better performance
                PlayerSettings.SetScriptingBackend(namedTarget, ScriptingImplementation.IL2CPP);
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                Debug.Log("[BuildEngine] Applied Release optimizations: IL2CPP, ARM64");
            }
        }

        /// <summary>
        /// Headless build method for CI/CD execution
        /// </summary>
        public static void BuildFromCommandLine()
        {
            Debug.Log("[BuildEngine] Starting headless build from command line...");

            // Get the database instance
            var database = BuildConfigDatabase.Instance;
            if (database == null)
            {
                Debug.LogError("[BuildEngine] Could not load BuildConfigDatabase!");
                EditorApplication.Exit(1);
                return;
            }

            BuildConfig config = null;
            bool isAab = false;

            // Parse command line arguments
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-buildAAB")
                {
                    isAab = true;
                }
                else if (args[i] == "-configName" && i + 1 < args.Length)
                {
                    string configName = args[i + 1];
                    config = database.GetConfigByName(configName);
                    Debug.Log($"[BuildEngine] Loading config by name: {configName}");
                    i++; // Skip next argument
                }
            }

            // If no config specified, use the selected one
            if (config == null)
            {
                config = database.SelectedConfig;
                Debug.Log($"[BuildEngine] Using selected config: {config.ConfigName}");
            }

            if (config == null)
            {
                Debug.LogError("[BuildEngine] No build config available!");
                EditorApplication.Exit(1);
                return;
            }

            // Run the build process
            RunBuildProcess(config, isAab);

            Debug.Log("[BuildEngine] Headless build completed!");
            EditorApplication.Exit(0);
        }
    }
}
