using UnityEngine;
using System.IO;
using System.Diagnostics;

namespace DucMinh.CustomBuild
{
    /// <summary>
    /// Service for Firebase App Distribution integration
    /// </summary>
    public static class FirebaseService
    {
        /// <summary>
        /// Handles Firebase App Distribution upload via CLI
        /// </summary>
        public static void UploadToFirebase(BuildConfig config, string buildPath)
        {
            UnityEngine.Debug.Log("[FirebaseService] Starting Firebase upload...");

            // Write release notes to file
            string releaseNotesPath = Path.Combine("Builds", "release-notes.txt");
            File.WriteAllText(releaseNotesPath, config.releaseNoteContent);

            // Construct Firebase CLI command
            string firebaseCmd = $"firebase appdistribution:distribute \"{buildPath}\" " +
                               $"--app \"{config.firebaseAppId}\" " +
                               $"--groups \"{config.testerGroups}\" " +
                               $"--release-notes-file \"{releaseNotesPath}\"";

            // Execute Firebase CLI
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {firebaseCmd}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            try
            {
                Process process = Process.Start(startInfo);

                // Capture output
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (!string.IsNullOrEmpty(output))
                {
                    UnityEngine.Debug.Log($"[Firebase] {output}");
                }

                if (!string.IsNullOrEmpty(error))
                {
                    UnityEngine.Debug.LogError($"[Firebase Error] {error}");
                }

                if (process.ExitCode == 0)
                {
                    UnityEngine.Debug.Log("[FirebaseService] Firebase upload completed successfully!");
                }
                else
                {
                    UnityEngine.Debug.LogError($"[FirebaseService] Firebase upload failed with exit code {process.ExitCode}");
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"[FirebaseService] Firebase upload exception: {ex.Message}");
            }
        }
    }
}
