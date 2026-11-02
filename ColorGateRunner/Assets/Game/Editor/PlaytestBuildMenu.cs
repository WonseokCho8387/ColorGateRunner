using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ColorGateRunner.Editor
{
    public static class PlaytestBuildMenu
    {
        internal const string OutputPath = "Builds/Playtest/v0.1";

        internal static BuildPlayerOptions CreateBuildOptions()
        {
            return new BuildPlayerOptions
            {
                scenes = TestBuildMenu.GetEnabledScenePaths(),
                locationPathName = Path.GetFullPath(Path.Combine(Application.dataPath, "..", OutputPath)),
                target = BuildTarget.WebGL,
                options = BuildOptions.None,
                extraScriptingDefines = new[] { "CGR_PLAYTEST" }
            };
        }

        public static void PrepareScenesForValidation()
        {
            TestBuildMenu.RefreshGeneratedContent();
            TestBuildMenu.RefreshGeneratedContent();
        }

        [MenuItem("Tools/Color Gate Runner/Playtest/Build WebGL v0.1")]
        public static void BuildWebGl()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new BuildFailedException("Exit Play Mode before building.");
            BuildPlayerOptions options = CreateBuildOptions();
            if (Directory.Exists(options.locationPathName))
                throw new BuildFailedException("Preserve or move the existing playtest output before rebuilding.");
            if (!Application.isBatchMode &&
                !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
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
                    throw new BuildFailedException("Playtest build failed; output is not a release candidate.");
                Debug.Log($"Playtest WebGL succeeded: {report.summary.totalSize} bytes; {options.locationPathName}");
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
    }
}
