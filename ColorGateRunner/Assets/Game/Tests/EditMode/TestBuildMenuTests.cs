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
                options.options & BuildOptions.CleanBuildCache,
                Is.EqualTo(BuildOptions.None));
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

        [Test]
        public void AndroidGradle_AlignsKotlinLibrariesIdempotently()
        {
            Type type = Type.GetType(
                "ColorGateRunner.Editor.AndroidGradleDependencyPostprocessor, " +
                "ColorGateRunner.Editor");
            Assert.That(type, Is.Not.Null);
            Assert.That(
                type.GetInterface(
                    "UnityEditor.Android.IPostGenerateGradleAndroidProject"),
                Is.Not.Null,
                "The Kotlin alignment callback must stay registered even " +
                "when the Editor starts on WebGL.");
            MethodInfo inject = type.GetMethod(
                "InjectKotlinResolution",
                StaticNonPublic);
            Assert.That(inject, Is.Not.Null);

            const string source =
                "plugins {\n}\n\ntasks.register('clean', Delete) {\n}\n";
            string first = (string)inject.Invoke(null, new object[] { source });
            string second = (string)inject.Invoke(null, new object[] { first });

            Assert.That(first, Does.Contain(
                "org.jetbrains.kotlin:kotlin-stdlib:1.8.22"));
            Assert.That(first, Does.Contain(
                "exclude group: 'org.jetbrains.kotlin', module: 'kotlin-stdlib-jdk7'"));
            Assert.That(first, Does.Contain(
                "exclude group: 'org.jetbrains.kotlin', module: 'kotlin-stdlib-jdk8'"));
            Assert.That(
                first.IndexOf("plugins {", StringComparison.Ordinal),
                Is.LessThan(first.IndexOf(
                    "// Color Gate Runner Kotlin dependency alignment",
                    StringComparison.Ordinal)));
            Assert.That(
                first.IndexOf(
                    "// Color Gate Runner Kotlin dependency alignment",
                    StringComparison.Ordinal),
                Is.LessThan(first.IndexOf(
                    "tasks.register('clean'",
                    StringComparison.Ordinal)));
            Assert.That(second, Is.EqualTo(first));
        }

        [Test]
        public void AndroidGradle_ReplacesLegacyPrefixAlignment()
        {
            Type type = Type.GetType(
                "ColorGateRunner.Editor.AndroidGradleDependencyPostprocessor, " +
                "ColorGateRunner.Editor");
            MethodInfo inject = type.GetMethod(
                "InjectKotlinResolution",
                StaticNonPublic);
            Assert.That(inject, Is.Not.Null);

            const string legacy =
                "// Color Gate Runner Kotlin dependency alignment\n" +
                "allprojects {\n" +
                "    configurations.configureEach {\n" +
                "        resolutionStrategy {\n" +
                "            force 'org.jetbrains.kotlin:kotlin-stdlib-jdk7:1.8.22'\n" +
                "        }\n" +
                "    }\n" +
                "}\n\n" +
                "plugins {\n}\n\n" +
                "tasks.register('clean', Delete) {\n}\n";
            string updated = (string)inject.Invoke(
                null,
                new object[] { legacy });

            Assert.That(updated, Does.StartWith("plugins {"));
            Assert.That(updated, Does.Not.Contain(
                "kotlin-stdlib-jdk7:1.8.22"));
            Assert.That(updated, Does.Contain(
                "exclude group: 'org.jetbrains.kotlin', module: 'kotlin-stdlib-jdk7'"));
            Assert.That(
                updated.IndexOf("plugins {", StringComparison.Ordinal),
                Is.LessThan(updated.IndexOf(
                    "// Color Gate Runner Kotlin dependency alignment",
                    StringComparison.Ordinal)));
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
