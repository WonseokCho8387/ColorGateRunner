using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ColorGateRunner.Editor
{
    public static class TestBuildMenu
    {
        internal const string AndroidOutputPath =
            "Builds/Test/Android/ColorGateRunner.apk";
        internal const string WebGlOutputPath = "Builds/Test/WebGL";
        internal const int WebGlPortraitWidth = 540;
        internal const int WebGlPortraitHeight = 960;
        internal const string WebGlPortraitTemplate =
            "PROJECT:ColorGateRunnerPortrait";

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

            RefreshGeneratedContent();

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
            WebGlSettingsSnapshot webGlSettings =
                target == UnityEditor.BuildTarget.WebGL
                    ? CaptureWebGlSettings()
                    : default;
            try
            {
                if (target == UnityEditor.BuildTarget.Android)
                {
                    EditorUserBuildSettings.buildAppBundle = false;
                }
                else if (target == UnityEditor.BuildTarget.WebGL)
                {
                    ApplyWebGlPortraitSettings();
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
                if (target == UnityEditor.BuildTarget.WebGL)
                {
                    RestoreWebGlSettings(webGlSettings);
                }
            }
        }

        internal static void RefreshGeneratedContent()
        {
            Scene originalScene = SceneManager.GetActiveScene();
            string originalScenePath = originalScene.path;

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            GrayboxSceneBuilder.BuildGrayboxScene();
            GrayboxSceneBuilder.ValidateGeneratedScene();
            FrontendSceneBuilder.BuildFrontendScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            if (!string.IsNullOrWhiteSpace(originalScenePath) &&
                AssetDatabase.LoadAssetAtPath<SceneAsset>(originalScenePath) !=
                    null)
            {
                EditorSceneManager.OpenScene(
                    originalScenePath,
                    OpenSceneMode.Single);
            }

            Debug.Log(
                "Test build content refreshed: " +
                "StageCatalog, Campaign, Frontend, Boot.");
        }

        private static WebGlSettingsSnapshot CaptureWebGlSettings()
        {
            return new WebGlSettingsSnapshot(
                PlayerSettings.defaultWebScreenWidth,
                PlayerSettings.defaultWebScreenHeight,
                PlayerSettings.WebGL.template);
        }

        private static void ApplyWebGlPortraitSettings()
        {
            PlayerSettings.defaultWebScreenWidth = WebGlPortraitWidth;
            PlayerSettings.defaultWebScreenHeight = WebGlPortraitHeight;
            PlayerSettings.WebGL.template = WebGlPortraitTemplate;
        }

        private static void RestoreWebGlSettings(
            WebGlSettingsSnapshot settings)
        {
            PlayerSettings.defaultWebScreenWidth = settings.Width;
            PlayerSettings.defaultWebScreenHeight = settings.Height;
            PlayerSettings.WebGL.template = settings.Template;
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
            else if (target == UnityEditor.BuildTarget.Android &&
                File.Exists(absoluteOutputPath))
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

        private readonly struct WebGlSettingsSnapshot
        {
            public WebGlSettingsSnapshot(
                int width,
                int height,
                string template)
            {
                Width = width;
                Height = height;
                Template = template;
            }

            public int Width { get; }
            public int Height { get; }
            public string Template { get; }
        }
    }
}
