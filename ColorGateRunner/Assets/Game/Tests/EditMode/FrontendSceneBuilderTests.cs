using System;
using ColorGateRunner.Editor;
using ColorGateRunner.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class FrontendSceneBuilderTests
    {
        [Test]
        public void CampaignSelection_UsesFirstActiveNonBootNonFrontendScene()
        {
            var scenes = new[]
            {
                new EditorBuildSettingsScene(
                    "Assets/Scenes/Boot.unity",
                    true),
                new EditorBuildSettingsScene(
                    "Assets/Scenes/Frontend.unity",
                    true),
                new EditorBuildSettingsScene(
                    "Assets/Scenes/Disabled.unity",
                    false),
                new EditorBuildSettingsScene(
                    "Assets/Scenes/MainCampaign.unity",
                    true)
            };

            Assert.That(
                FrontendSceneBuilder.SelectCampaignScenePath(scenes),
                Is.EqualTo("Assets/Scenes/MainCampaign.unity"));
        }

        [Test]
        public void CampaignSelection_HasNoSampleSceneNameDependency()
        {
            var scenes = new[]
            {
                new EditorBuildSettingsScene(
                    "Assets/Scenes/CampaignProduction.unity",
                    true)
            };

            string path = FrontendSceneBuilder.SelectCampaignScenePath(scenes);

            Assert.That(path, Is.EqualTo(
                "Assets/Scenes/CampaignProduction.unity"));
            Assert.That(path, Does.Not.Contain("SampleScene"));
        }

        [Test]
        public void BuildSceneList_IsExactlyBootFrontendCampaign()
        {
            EditorBuildSettingsScene[] scenes =
                FrontendSceneBuilder.CreateFrontendSceneList(
                    "Assets/Scenes/MainCampaign.unity");

            Assert.That(scenes.Length, Is.EqualTo(3));
            Assert.That(scenes[0].path,
                Is.EqualTo("Assets/Scenes/Boot.unity"));
            Assert.That(scenes[1].path,
                Is.EqualTo("Assets/Scenes/Frontend.unity"));
            Assert.That(scenes[2].path,
                Is.EqualTo("Assets/Scenes/MainCampaign.unity"));
            Assert.That(scenes, Has.All.Matches<EditorBuildSettingsScene>(
                scene => scene.enabled));
        }

        [TestCase("Assets/Scenes/Boot.unity")]
        [TestCase("Assets/Scenes/Frontend.unity")]
        [TestCase("")]
        public void BuildSceneList_RejectsInvalidCampaign(string path)
        {
            Assert.Throws<InvalidOperationException>(() =>
                FrontendSceneBuilder.CreateFrontendSceneList(path));
        }

        [Test]
        public void CampaignBuilder_SelectsSerializedFrontendWithoutNameHardcode()
        {
            var scenes = new[]
            {
                new EditorBuildSettingsScene(
                    "Assets/Scenes/Boot.unity",
                    true),
                new EditorBuildSettingsScene(
                    "Assets/Scenes/ProductShell.unity",
                    true),
                new EditorBuildSettingsScene(
                    GrayboxSceneBuilder.ScenePath,
                    true)
            };

            string path = GrayboxSceneBuilder.SelectFrontendScenePath(scenes);

            Assert.That(path, Is.EqualTo("Assets/Scenes/ProductShell.unity"));
            Assert.That(path, Does.Not.Contain("Frontend"));
        }

        [Test]
        public void CampaignBuilder_RejectsBootOrSelfAsFrontend()
        {
            var scenes = new[]
            {
                new EditorBuildSettingsScene(
                    "Assets/Scenes/Boot.unity",
                    true),
                new EditorBuildSettingsScene(
                    GrayboxSceneBuilder.ScenePath,
                    true)
            };

            Assert.Throws<InvalidOperationException>(() =>
                GrayboxSceneBuilder.SelectFrontendScenePath(scenes));
        }

        [Test]
        public void LobbyThemeCatalog_HasCompleteThemeOneAndFutureFallbacks()
        {
            LobbyThemeVisualCatalog catalog =
                AssetDatabase.LoadAssetAtPath<LobbyThemeVisualCatalog>(
                    LobbyThemeVisualCatalogBuilder.AssetPath);

            Assert.That(catalog, Is.Not.Null);
            Assert.That(catalog.Count, Is.EqualTo(3));
            Assert.That(catalog.TryGet(
                0,
                out LobbyThemeVisualDefinition themeOne), Is.True);
            Assert.That(themeOne.ThemeId, Is.EqualTo("color-courtyard"));
            Assert.That(themeOne.DisplayName,
                Is.EqualTo("COLOR COURTYARD"));
            Assert.That(themeOne.HasCompleteArtwork, Is.True);
            Assert.That(AssetDatabase.GetAssetPath(themeOne.Background),
                Is.EqualTo(LobbyThemeVisualCatalogBuilder.BackgroundPath));
            Assert.That(AssetDatabase.GetAssetPath(themeOne.Midground),
                Is.EqualTo(LobbyThemeVisualCatalogBuilder.ReactorPath));
            Assert.That(AssetDatabase.GetAssetPath(themeOne.Foreground),
                Is.EqualTo(LobbyThemeVisualCatalogBuilder.AmbientPath));

            Assert.That(catalog.TryGet(
                1,
                out LobbyThemeVisualDefinition themeTwo), Is.True);
            Assert.That(themeTwo.HasCompleteArtwork, Is.False);
            Assert.That(catalog.TryGet(
                2,
                out LobbyThemeVisualDefinition themeThree), Is.True);
            Assert.That(themeThree.HasCompleteArtwork, Is.False);

            AssertSpriteImporter(
                LobbyThemeVisualCatalogBuilder.BackgroundPath,
                false);
            AssertSpriteImporter(
                LobbyThemeVisualCatalogBuilder.ReactorPath,
                true);
            AssertSpriteImporter(
                LobbyThemeVisualCatalogBuilder.AmbientPath,
                true);
        }

        private static void AssertSpriteImporter(
            string path,
            bool expectedTransparency)
        {
            TextureImporter importer =
                AssetImporter.GetAtPath(path) as TextureImporter;

            Assert.That(importer, Is.Not.Null);
            Assert.That(importer.textureType,
                Is.EqualTo(TextureImporterType.Sprite));
            Assert.That(importer.mipmapEnabled, Is.False);
            Assert.That(importer.alphaIsTransparency,
                Is.EqualTo(expectedTransparency));
            Assert.That(importer.wrapMode, Is.EqualTo(TextureWrapMode.Clamp));
        }
    }
}
