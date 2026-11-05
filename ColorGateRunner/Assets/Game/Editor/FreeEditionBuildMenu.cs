using System.IO;
using UnityEditor;
using UnityEngine;

namespace ColorGateRunner.Editor
{
    // The free itch.io web edition: the full Campaign with the normal Product
    // save, no Heart limit and no Shop (CGR_WEB_FREE). Nothing is sold.
    public static class FreeEditionBuildMenu
    {
        internal const string Define = "CGR_WEB_FREE";
        internal const string OutputFolder = "Builds/WebFree";

        internal static string Version => PlayerSettings.bundleVersion;

        internal static string ProjectPath(string relative) =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", relative));

        internal static BuildPlayerOptions CreateBuildOptions()
        {
            return new BuildPlayerOptions
            {
                scenes = TestBuildMenu.GetEnabledScenePaths(),
                locationPathName = ProjectPath($"{OutputFolder}/{Version}"),
                target = BuildTarget.WebGL,
                options = BuildOptions.None,
                extraScriptingDefines = new[] { Define }
            };
        }

        internal static string ZipPath =>
            ProjectPath($"{OutputFolder}/ColorGateRunner-Web-{Version}.zip");

        [MenuItem("Tools/Color Gate Runner/Free Edition/Build WebGL Free Edition")]
        public static void BuildWebGl()
        {
            BuildPlayerOptions options = CreateBuildOptions();
            // Fail before the long build rather than after it.
            if (File.Exists(ZipPath))
                throw new UnityEditor.Build.BuildFailedException(
                    "Preserve or move the existing free edition ZIP before rebuilding.");
            if (WebGlReleaseBuild.Build(options, "free edition"))
                WebGlReleaseBuild.Package(options.locationPathName, ZipPath);
        }
    }
}
