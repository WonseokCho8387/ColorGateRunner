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
        public void EnabledBuildScenes_AreBootFrontendCampaign()
        {
            MethodInfo method = RequireMenuType().GetMethod(
                "GetEnabledScenePaths",
                StaticNonPublic);

            Assert.That(method, Is.Not.Null);
            string[] scenes = (string[])method.Invoke(null, null);

            Assert.That(scenes[0], Is.EqualTo("Assets/Scenes/Boot.unity"));
            Assert.That(scenes[1], Is.EqualTo("Assets/Scenes/Frontend.unity"));
            Assert.That(scenes[2], Is.EqualTo("Assets/Scenes/SampleScene.unity"));
            Assert.That(scenes.Length, Is.EqualTo(3));
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
            Assert.That(options.scenes[0],
                Is.EqualTo("Assets/Scenes/Boot.unity"));
            Assert.That(options.scenes[1],
                Is.EqualTo("Assets/Scenes/Frontend.unity"));
            Assert.That(options.scenes[2],
                Is.EqualTo("Assets/Scenes/SampleScene.unity"));
        }

        [Test]
        public void WebGlPortraitTemplate_UsesExactNineBySixteenFrame()
        {
            Type menuType = RequireMenuType();
            Assert.That(
                RequireConstant<int>(menuType, "WebGlPortraitWidth"),
                Is.EqualTo(540));
            Assert.That(
                RequireConstant<int>(menuType, "WebGlPortraitHeight"),
                Is.EqualTo(960));
            Assert.That(
                RequireConstant<string>(
                    menuType,
                    "WebGlPortraitTemplate"),
                Is.EqualTo("PROJECT:ColorGateRunnerPortrait"));

            string projectRoot = Path.GetFullPath(
                Path.Combine(Application.dataPath, ".."));
            string templatePath = Path.Combine(
                projectRoot,
                "Assets",
                "WebGLTemplates",
                "ColorGateRunnerPortrait",
                "index.html");
            Assert.That(File.Exists(templatePath), Is.True);
            string template = File.ReadAllText(templatePath);

            Assert.That(template, Does.Contain("aspect-ratio: 9 / 16"));
            Assert.That(template, Does.Contain("56.25vh"));
            Assert.That(template, Does.Contain("177.7777778vw"));
            Assert.That(template, Does.Contain("width={{{ WIDTH }}}"));
            Assert.That(template, Does.Contain("height={{{ HEIGHT }}}"));
        }

        [Test]
        public void WebGlPortraitSettings_ApplyAndRestoreEditorValues()
        {
            Type menuType = RequireMenuType();
            MethodInfo capture = menuType.GetMethod(
                "CaptureWebGlSettings",
                StaticNonPublic);
            MethodInfo apply = menuType.GetMethod(
                "ApplyWebGlPortraitSettings",
                StaticNonPublic);
            MethodInfo restore = menuType.GetMethod(
                "RestoreWebGlSettings",
                StaticNonPublic);
            Assert.That(capture, Is.Not.Null);
            Assert.That(apply, Is.Not.Null);
            Assert.That(restore, Is.Not.Null);

            int originalWidth = PlayerSettings.defaultWebScreenWidth;
            int originalHeight = PlayerSettings.defaultWebScreenHeight;
            string originalTemplate = PlayerSettings.WebGL.template;
            object snapshot = capture.Invoke(null, null);
            try
            {
                apply.Invoke(null, null);
                Assert.That(
                    PlayerSettings.defaultWebScreenWidth,
                    Is.EqualTo(540));
                Assert.That(
                    PlayerSettings.defaultWebScreenHeight,
                    Is.EqualTo(960));
                Assert.That(
                    PlayerSettings.WebGL.template,
                    Is.EqualTo("PROJECT:ColorGateRunnerPortrait"));
            }
            finally
            {
                restore.Invoke(null, new[] { snapshot });
            }

            Assert.That(
                PlayerSettings.defaultWebScreenWidth,
                Is.EqualTo(originalWidth));
            Assert.That(
                PlayerSettings.defaultWebScreenHeight,
                Is.EqualTo(originalHeight));
            Assert.That(
                PlayerSettings.WebGL.template,
                Is.EqualTo(originalTemplate));
        }

        private static T RequireConstant<T>(Type type, string name)
        {
            FieldInfo field = type.GetField(name, StaticNonPublic);
            Assert.That(field, Is.Not.Null, name);
            return (T)field.GetRawConstantValue();
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
