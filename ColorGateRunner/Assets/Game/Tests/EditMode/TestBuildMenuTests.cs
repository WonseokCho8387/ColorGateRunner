using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class TestBuildMenuTests
    {
        private const BindingFlags StaticNonPublic =
            BindingFlags.Static | BindingFlags.NonPublic;

        [Test]
        public void EnabledBuildScenes_ContainThePlayableScene()
        {
            MethodInfo method = RequireMenuType().GetMethod(
                "GetEnabledScenePaths",
                StaticNonPublic);

            Assert.That(method, Is.Not.Null);
            string[] scenes = (string[])method.Invoke(null, null);

            Assert.That(
                scenes,
                Is.EqualTo(new[] { "Assets/Scenes/SampleScene.unity" }));
        }

        [TestCase(BuildTarget.Android, "Builds/Test/Android/ColorGateRunner.apk")]
        [TestCase(BuildTarget.WebGL, "Builds/Test/WebGL")]
        public void BuildOptions_UseDevelopmentModeAndExpectedOutput(
            BuildTarget target,
            string relativeOutput)
        {
            MethodInfo method = RequireMenuType().GetMethod(
                "CreateBuildOptions",
                StaticNonPublic);

            Assert.That(method, Is.Not.Null);
            var options = (BuildPlayerOptions)method.Invoke(
                null,
                new object[] { target, relativeOutput });
            string projectRoot = Path.GetFullPath(
                Path.Combine(Application.dataPath, ".."));

            Assert.That(options.target, Is.EqualTo(target));
            Assert.That(
                options.options & BuildOptions.Development,
                Is.EqualTo(BuildOptions.Development));
            Assert.That(
                options.locationPathName,
                Is.EqualTo(Path.GetFullPath(
                    Path.Combine(projectRoot, relativeOutput))));
            Assert.That(
                options.scenes,
                Is.EqualTo(new[] { "Assets/Scenes/SampleScene.unity" }));
        }

        private static Type RequireMenuType()
        {
            Type type = Type.GetType(
                "ColorGateRunner.Editor.TestBuildMenu, " +
                "ColorGateRunner.Editor");
            Assert.That(type, Is.Not.Null);
            return type;
        }
    }
}
