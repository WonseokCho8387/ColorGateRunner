using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ColorGateRunner.Editor
{
    public static class TestBuildMenu
    {
        internal const string AndroidOutputPath =
            "Builds/Test/Android/ColorGateRunner.apk";
        internal const string WebGlOutputPath = "Builds/Test/WebGL";

        private const string MenuRoot =
            "Tools/Color Gate Runner/Test Builds/";

        [MenuItem(MenuRoot + "Build Android APK", priority = 200)]
        public static void BuildAndroidTest()
        {
            BuildTarget(
                UnityEditor.BuildTarget.Android,
                AndroidOutputPath,
                "Android APK");
        }

        [MenuItem(MenuRoot + "Build WebGL", priority = 201)]
        public static void BuildWebGlTest()
        {
            BuildTarget(
                UnityEditor.BuildTarget.WebGL,
                WebGlOutputPath,
                "WebGL");
        }

        [MenuItem(MenuRoot + "Build Android + WebGL", priority = 202)]
        public static void BuildAllTestBuilds()
        {
            BuildAndroidTest();
            BuildWebGlTest();
        }

        [MenuItem(MenuRoot + "Open Build Folder", priority = 220)]
        public static void OpenBuildFolder()
        {
            string absolutePath = GetAbsolutePath("Builds/Test");
            Directory.CreateDirectory(absolutePath);
            EditorUtility.RevealInFinder(absolutePath);
        }

        [MenuItem(MenuRoot + "Build Android APK", true)]
        [MenuItem(MenuRoot + "Build WebGL", true)]
        [MenuItem(MenuRoot + "Build Android + WebGL", true)]
        private static bool ValidateBuildCommands()
        {
            return !EditorApplication.isCompiling &&
                !EditorApplication.isPlayingOrWillChangePlaymode &&
                !BuildPipeline.isBuildingPlayer;
        }

        internal static BuildPlayerOptions CreateBuildOptions(
            UnityEditor.BuildTarget target,
            string outputPath)
        {
            return new BuildPlayerOptions
            {
                scenes = GetEnabledScenePaths(),
                locationPathName = GetAbsolutePath(outputPath),
                target = target,
                options = BuildOptions.Development
            };
        }

        internal static string[] GetEnabledScenePaths()
        {
            var scenePaths = new List<string>();
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            for (int index = 0; index < scenes.Length; index++)
            {
                EditorBuildSettingsScene scene = scenes[index];
                if (!scene.enabled)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(scene.path) ||
                    AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path) == null)
                {
                    throw new BuildFailedException(
                        $"Enabled build scene is missing: {scene.path}");
                }

                scenePaths.Add(scene.path);
            }

            if (scenePaths.Count == 0)
            {
                throw new BuildFailedException(
                    "No enabled scenes exist in Build Settings.");
            }

            return scenePaths.ToArray();
        }

        private static void BuildTarget(
            UnityEditor.BuildTarget target,
            string outputPath,
            string displayName)
        {
            if (!Application.isBatchMode &&
                !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.Log($"{displayName} test build cancelled.");
                return;
            }

            BuildTargetGroup targetGroup =
                BuildPipeline.GetBuildTargetGroup(target);
            if (!BuildPipeline.IsBuildTargetSupported(targetGroup, target))
            {
                throw new BuildFailedException(
                    $"{displayName} Build Support is not installed.");
            }

            if (EditorUserBuildSettings.activeBuildTarget != target &&
                !EditorUserBuildSettings.SwitchActiveBuildTarget(
                    targetGroup,
                    target))
            {
                throw new BuildFailedException(
                    $"Could not switch the active platform to {displayName}.");
            }

            BuildPlayerOptions options =
                CreateBuildOptions(target, outputPath);
            PrepareOutput(options.locationPathName, target);

            bool previousBuildAppBundle =
                EditorUserBuildSettings.buildAppBundle;
            try
            {
                if (target == UnityEditor.BuildTarget.Android)
                {
                    EditorUserBuildSettings.buildAppBundle = false;
                }

                BuildReport report = BuildPipeline.BuildPlayer(options);
                if (report.summary.result != BuildResult.Succeeded)
                {
                    throw new BuildFailedException(
                        $"{displayName} test build failed with " +
                        $"{report.summary.totalErrors} errors.");
                }

                string message =
                    $"{displayName} test build succeeded: " +
                    $"{report.summary.totalSize:N0} bytes\n" +
                    options.locationPathName;
                Debug.Log(message);
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog(
                        "Color Gate Runner",
                        message,
                        "OK");
                }
            }
            finally
            {
                EditorUserBuildSettings.buildAppBundle =
                    previousBuildAppBundle;
            }
        }

        private static void PrepareOutput(
            string absoluteOutputPath,
            UnityEditor.BuildTarget target)
        {
            if (target == UnityEditor.BuildTarget.WebGL &&
                Directory.Exists(absoluteOutputPath))
            {
                FileUtil.DeleteFileOrDirectory(absoluteOutputPath);
            }

            string directory = target == UnityEditor.BuildTarget.WebGL
                ? absoluteOutputPath
                : Path.GetDirectoryName(absoluteOutputPath);
            if (string.IsNullOrEmpty(directory))
            {
                throw new BuildFailedException(
                    $"Invalid build output path: {absoluteOutputPath}");
            }

            Directory.CreateDirectory(directory);
        }

        private static string GetAbsolutePath(string projectRelativePath)
        {
            string projectRoot =
                Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            return Path.GetFullPath(
                Path.Combine(projectRoot, projectRelativePath));
        }
    }
}
