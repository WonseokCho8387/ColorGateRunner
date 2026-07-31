using System;
using System.Linq;
using ColorGateRunner.Editor;
using NUnit.Framework;
using UnityEditor;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class BootSceneBuilderTests
    {
        [Test]
        public void Destination_IsFirstActiveNonBootScene()
        {
            var scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/Boot.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Disabled.unity", false),
                new EditorBuildSettingsScene("Assets/Scenes/Campaign.unity", true)
            };

            Assert.That(
                BootSceneBuilder.SelectDestinationScenePath(scenes),
                Is.EqualTo("Assets/Scenes/Campaign.unity"));
        }

        [Test]
        public void Destination_DoesNotDependOnSampleSceneName()
        {
            var scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/MainCampaign.unity", true)
            };

            string destination =
                BootSceneBuilder.SelectDestinationScenePath(scenes);

            Assert.That(destination,
                Is.EqualTo("Assets/Scenes/MainCampaign.unity"));
            Assert.That(destination, Does.Not.Contain("SampleScene"));
        }

        [Test]
        public void Destination_RejectsBootOnlyConfiguration()
        {
            var scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/Boot.unity", true)
            };

            Assert.Throws<InvalidOperationException>(() =>
                BootSceneBuilder.SelectDestinationScenePath(scenes));
        }

        [Test]
        public void BuildSceneList_PutsOneActiveBootAtIndexZero()
        {
            var scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/A.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Boot.unity", false),
                new EditorBuildSettingsScene("Assets/Scenes/B.unity", false),
                new EditorBuildSettingsScene("Assets/Scenes/Boot.unity", true)
            };

            EditorBuildSettingsScene[] result =
                BootSceneBuilder.CreateBootFirstSceneList(scenes);

            Assert.That(result[0].path,
                Is.EqualTo("Assets/Scenes/Boot.unity"));
            Assert.That(result[0].enabled, Is.True);
            Assert.That(result.Count(scene =>
                scene.path == "Assets/Scenes/Boot.unity"), Is.EqualTo(1));
            Assert.That(result.Skip(1).Select(scene => scene.path),
                Is.EqualTo(new[]
                {
                    "Assets/Scenes/A.unity",
                    "Assets/Scenes/B.unity"
                }));
            Assert.That(result[2].enabled, Is.False);
        }

        [Test]
        public void SerializedDestination_RemainsExplicitWhenOrderChanges()
        {
            var initial = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/A.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/B.unity", true)
            };
            string serializedDestination =
                BootSceneBuilder.SelectDestinationScenePath(initial);
            var reordered = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/Boot.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/B.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/A.unity", true)
            };

            Assert.That(serializedDestination,
                Is.EqualTo("Assets/Scenes/A.unity"));
            Assert.That(reordered.Any(scene =>
                scene.enabled && scene.path == serializedDestination), Is.True);
        }
    }
}
