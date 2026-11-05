using System.IO;
using UnityEditor;
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
            WebGlReleaseBuild.Build(CreateBuildOptions(), "playtest");
        }
    }
}
