using System.Linq;
using System.Reflection;
using ColorGateRunner.Editor;
using NUnit.Framework;
using UnityEditor;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class DeveloperConsoleTests
    {
        [Test]
        public void WindowMenu_IsRegisteredOnApprovedPath()
        {
            MethodInfo open = typeof(DeveloperConsoleWindow).GetMethod(
                "Open",
                BindingFlags.Static | BindingFlags.NonPublic);
            MenuItem attribute = open?.GetCustomAttribute<MenuItem>();

            Assert.That(open, Is.Not.Null);
            Assert.That(attribute, Is.Not.Null);
            Assert.That(attribute.menuItem,
                Is.EqualTo("Window/Color Gate Runner/Developer Console"));
        }

        [Test]
        public void BuildSettings_KeepBootFrontendCampaignOrder()
        {
            string[] enabled = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            Assert.That(enabled, Is.EqualTo(new[]
            {
                BootSceneBuilder.ScenePath,
                FrontendSceneBuilder.ScenePath,
                GrayboxSceneBuilder.ScenePath
            }));
        }
    }
}
