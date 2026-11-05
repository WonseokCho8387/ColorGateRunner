using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ColorGateRunner.Editor
{
    // Shared external (non-development) WebGL build: portrait template, Gzip
    // with decompression fallback so plain static hosts such as itch.io serve
    // it without Content-Encoding headers, and stripped engine code. Player
    // settings are restored afterwards so no ProjectSettings change remains.
    internal static class WebGlReleaseBuild
    {
        // itch.io HTML5 upload limits (per file, file count, total size).
        internal const long ItchMaxFileBytes = 200L * 1024 * 1024;
        internal const int ItchMaxFileCount = 1000;
        internal const long ItchMaxTotalBytes = 500L * 1024 * 1024;

        // Returns false when the user cancels saving modified Scenes.
        internal static bool Build(BuildPlayerOptions options, string label)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new BuildFailedException("Exit Play Mode before building.");
            if (Directory.Exists(options.locationPathName))
                throw new BuildFailedException($"Preserve or move the existing {label} output before rebuilding.");
            if (!Application.isBatchMode &&
                !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
                throw new BuildFailedException("WebGL Build Support is not installed.");
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL &&
                !EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL))
                throw new BuildFailedException("Could not switch the active platform to WebGL.");
            TestBuildMenu.RefreshGeneratedContent();
            int width = PlayerSettings.defaultWebScreenWidth;
            int height = PlayerSettings.defaultWebScreenHeight;
            string template = PlayerSettings.WebGL.template;
            var compression = PlayerSettings.WebGL.compressionFormat;
            bool fallback = PlayerSettings.WebGL.decompressionFallback;
            bool strip = PlayerSettings.stripEngineCode;
            try
            {
                PlayerSettings.defaultWebScreenWidth = TestBuildMenu.WebGlPortraitWidth;
                PlayerSettings.defaultWebScreenHeight = TestBuildMenu.WebGlPortraitHeight;
                PlayerSettings.WebGL.template = TestBuildMenu.WebGlPortraitTemplate;
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
                PlayerSettings.WebGL.decompressionFallback = true;
                PlayerSettings.stripEngineCode = true;
                BuildReport report = BuildPipeline.BuildPlayer(options);
                if (report.summary.result != BuildResult.Succeeded)
                    throw new BuildFailedException($"{label} build failed; output is not a release candidate.");
                Debug.Log($"{label} WebGL succeeded: {report.summary.totalSize} bytes; {options.locationPathName}");
                return true;
            }
            finally
            {
                PlayerSettings.defaultWebScreenWidth = width;
                PlayerSettings.defaultWebScreenHeight = height;
                PlayerSettings.WebGL.template = template;
                PlayerSettings.WebGL.compressionFormat = compression;
                PlayerSettings.WebGL.decompressionFallback = fallback;
                PlayerSettings.stripEngineCode = strip;
            }
        }

        // Unity writes Burst debug symbols beside the player; they must not ship.
        internal static bool ShouldShip(string relativePath)
        {
            string normalized = relativePath.Replace('\\', '/');
            return !normalized.Split('/').Any(part => part.EndsWith("_DoNotShip"));
        }

        // Returns null when the shipped file sizes fit itch.io, else the reason.
        internal static string CheckItchLimits(IReadOnlyCollection<long> fileSizes)
        {
            if (fileSizes.Count == 0) return "The build has no files to ship.";
            if (fileSizes.Count > ItchMaxFileCount)
                return $"{fileSizes.Count} files exceed the {ItchMaxFileCount}-file limit.";
            if (fileSizes.Any(size => size > ItchMaxFileBytes))
                return "A file exceeds the 200 MB per-file limit.";
            if (fileSizes.Sum() > ItchMaxTotalBytes)
                return "The build exceeds the 500 MB total limit.";
            return null;
        }

        // Zips the shippable files with index.html at the archive root.
        internal static void Package(string buildFolder, string zipPath)
        {
            List<string> files = Directory
                .GetFiles(buildFolder, "*", SearchOption.AllDirectories)
                .Select(path => Path.GetRelativePath(buildFolder, path))
                .Where(ShouldShip)
                .OrderBy(path => path, System.StringComparer.Ordinal)
                .ToList();
            if (!files.Contains("index.html"))
                throw new BuildFailedException("index.html is missing from the build root.");
            string limit = CheckItchLimits(files
                .Select(path => new FileInfo(Path.Combine(buildFolder, path)).Length)
                .ToList());
            if (limit != null) throw new BuildFailedException(limit);
            if (File.Exists(zipPath))
                throw new BuildFailedException("Preserve or move the existing ZIP before packaging.");
            using ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Create);
            foreach (string path in files)
            {
                archive.CreateEntryFromFile(
                    Path.Combine(buildFolder, path),
                    path.Replace('\\', '/'),
                    System.IO.Compression.CompressionLevel.Optimal);
            }
            Debug.Log($"Packaged {files.Count} files: {zipPath}");
        }
    }
}
