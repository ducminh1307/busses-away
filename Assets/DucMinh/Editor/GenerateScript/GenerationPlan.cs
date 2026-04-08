using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DucMinh.GenerateScript
{
    /// <summary>
    /// Kế hoạch generation gồm nhiều files/folders — dùng cho Phase 6 Batch.
    /// </summary>
    public class GenerationPlan
    {
        public List<PlannedFile> Files { get; } = new List<PlannedFile>();
        public List<PlannedFolder> Folders { get; } = new List<PlannedFolder>();
        public List<Action> PostHooks { get; } = new List<Action>();

        public void AddFile(string relativeAssetsPath, string content)
            => Files.Add(new PlannedFile(relativeAssetsPath, content));

        public void AddFolder(string relativeAssetsPath)
            => Folders.Add(new PlannedFolder(relativeAssetsPath));

        public void AddPostHook(Action hook)
            => PostHooks?.Add(hook);
    }

    public class PlannedFile
    {
        public string RelativeAssetsPath { get; }
        public string Content { get; }

        public PlannedFile(string relativeAssetsPath, string content)
        {
            RelativeAssetsPath = relativeAssetsPath;
            Content = content;
        }

        public string AbsolutePath =>
            Path.Combine(Application.dataPath, RelativeAssetsPath.Replace("Assets/", ""));
    }

    public class PlannedFolder
    {
        public string RelativeAssetsPath { get; }

        public PlannedFolder(string relativeAssetsPath)
        {
            RelativeAssetsPath = relativeAssetsPath;
        }

        public string AbsolutePath =>
            Path.Combine(Application.dataPath, RelativeAssetsPath.Replace("Assets/", ""));
    }
}
