using System;
using System.IO;
using ColorGateRunner.Editor;
using ColorGateRunner.Presentation;
using ColorGateRunner.Product;
using NUnit.Framework;
using UnityEditor;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class FreeEditionBuildTests
    {
        [Test]
        public void FreeEditionBuild_IsNonDevelopmentWebGlWithOnlyItsDefine()
        {
            BuildPlayerOptions options = FreeEditionBuildMenu.CreateBuildOptions();
            Assert.That(options.target, Is.EqualTo(BuildTarget.WebGL));
            Assert.That(options.options, Is.EqualTo(BuildOptions.None));
            Assert.That(options.extraScriptingDefines,
                Is.EqualTo(new[] { "CGR_WEB_FREE" }));
            Assert.That(options.scenes, Is.EqualTo(new[] { "Assets/Scenes/Boot.unity",
                "Assets/Scenes/Frontend.unity", "Assets/Scenes/SampleScene.unity" }));
            Assert.That(options.locationPathName.Replace('\\', '/'),
                Does.EndWith("/Builds/WebFree/" + PlayerSettings.bundleVersion));
            Assert.That(FreeEditionBuildMenu.ZipPath.Replace('\\', '/'),
                Does.EndWith($"/Builds/WebFree/ColorGateRunner-Web-{PlayerSettings.bundleVersion}.zip"));
        }

        [Test]
        public void PortraitTemplate_PersistsTheSaveToIndexedDb()
        {
            string template = File.ReadAllText(Path.GetFullPath(
                "Assets/WebGLTemplates/ColorGateRunnerPortrait/index.html"));
            Assert.That(template, Does.Contain("autoSyncPersistentDataPath: true"));
        }

        [TestCase("index.html", true)]
        [TestCase("Build/v.data.unityweb", true)]
        [TestCase("StreamingAssets/UnityServicesProjectConfiguration.json", true)]
        [TestCase("ColorGateRunner_BurstDebugInformation_DoNotShip/Data/x.txt", false)]
        [TestCase(@"ColorGateRunner_BurstDebugInformation_DoNotShip\lib.txt", false)]
        public void Packaging_DropsDoNotShipFolders(string path, bool shipped)
        {
            Assert.That(WebGlReleaseBuild.ShouldShip(path), Is.EqualTo(shipped));
        }

        [Test]
        public void Packaging_ChecksItchUploadLimits()
        {
            const long MB = 1024L * 1024;
            Assert.That(WebGlReleaseBuild.CheckItchLimits(new[] { 65 * MB, 14 * MB }), Is.Null);
            Assert.That(WebGlReleaseBuild.CheckItchLimits(Array.Empty<long>()), Is.Not.Null);
            Assert.That(WebGlReleaseBuild.CheckItchLimits(new[] { 201 * MB }), Is.Not.Null);
            Assert.That(WebGlReleaseBuild.CheckItchLimits(new[] { 190 * MB, 190 * MB, 190 * MB }),
                Is.Not.Null);
            Assert.That(WebGlReleaseBuild.CheckItchLimits(new long[1001]), Is.Not.Null);
        }

        [Test]
        public void LobbyHeartText_ShowsPermanentWaiverAsInfinity()
        {
            DateTime now = new DateTime(2026, 11, 5, 0, 0, 0, DateTimeKind.Utc);
            Assert.That(LobbyProgressionPanel.FormatHeart(
                new HeartStateSnapshot(5, true, DateTime.MaxValue), now),
                Is.EqualTo("∞"));
            Assert.That(LobbyProgressionPanel.FormatHeart(
                new HeartStateSnapshot(5, true, now.AddMinutes(30)), now),
                Does.StartWith("∞  "));
        }
    }
}
