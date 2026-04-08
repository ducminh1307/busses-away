using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace DucMinh.BuildReport
{
    public static class CombinedBuildReportStorage
    {
        public const string ReportsRoot = "Assets/BuildReports";
        private const string ReportFileName = "CombinedReport.json";

        // ────────────────────── Save ──────────────────────

        public static string Save(CombinedBuildReport report)
        {
            var timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
            var folderName = $"{timestamp}_{report.Summary?.Platform ?? "Unknown"}";
            var folderPath = Path.Combine(ReportsRoot, folderName);
            var absFolder  = Path.GetFullPath(folderPath);

            Directory.CreateDirectory(absFolder);

            report.ReportFolderPath = absFolder;

            var json = JsonUtility.ToJson(report, prettyPrint: true);
            File.WriteAllText(Path.Combine(absFolder, ReportFileName), json);

            // Also save a lightweight history entry
            SaveHistoryEntry(BuildHistoryEntry.FromReport(report), absFolder);

            Debug.Log($"[BuildReport] Saved to {absFolder}");
            return absFolder;
        }

        private static void SaveHistoryEntry(BuildHistoryEntry entry, string folderPath)
        {
            var json = JsonUtility.ToJson(entry, prettyPrint: true);
            File.WriteAllText(Path.Combine(folderPath, "HistoryEntry.json"), json);
        }

        // ────────────────────── Load ──────────────────────

        public static CombinedBuildReport LoadLatest()
        {
            var entries = LoadAllHistoryEntries();
            if (entries.Count == 0) return null;
            var latest = entries.OrderByDescending(e => e.TimestampUtc).First();
            return Load(latest.ReportFolderPath);
        }

        public static CombinedBuildReport Load(string folderPath)
        {
            var filePath = Path.Combine(folderPath, ReportFileName);
            if (!File.Exists(filePath)) return null;
            try
            {
                var json = File.ReadAllText(filePath);
                return JsonUtility.FromJson<CombinedBuildReport>(json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[BuildReport] Failed to load report from {filePath}: {ex.Message}");
                return null;
            }
        }

        public static List<BuildHistoryEntry> LoadAllHistoryEntries()
        {
            var result = new List<BuildHistoryEntry>();
            var absRoot = Path.GetFullPath(ReportsRoot);
            if (!Directory.Exists(absRoot)) return result;

            foreach (var dir in Directory.GetDirectories(absRoot))
            {
                var entryFile = Path.Combine(dir, "HistoryEntry.json");
                if (!File.Exists(entryFile)) continue;
                try
                {
                    var json  = File.ReadAllText(entryFile);
                    var entry = JsonUtility.FromJson<BuildHistoryEntry>(json);
                    if (entry != null)
                    {
                        entry.ReportFolderPath = dir;
                        result.Add(entry);
                    }
                }
                catch { /* skip corrupt entries */ }
            }
            return result;
        }
    }
}
