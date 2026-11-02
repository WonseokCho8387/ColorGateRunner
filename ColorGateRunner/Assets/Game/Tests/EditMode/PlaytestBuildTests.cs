using ColorGateRunner.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class PlaytestBuildTests
    {
        [TestCase("WarpCyan")]
        [TestCase("WarpGold")]
        [TestCase("ProtectionPulse")]
        public void AdditiveMaterials_PreserveAuthoredOrderAfterUrpValidation(string name)
        {
            Material asset = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Game/Generated/Materials/" + name + ".mat");
            Assert.That(asset, Is.Not.Null);
            var copy = new Material(asset);
            MaterialEditor editor = null;
            try
            {
                editor = (MaterialEditor)UnityEditor.Editor.CreateEditor(copy);
                Assert.That(editor.customShaderGUI, Is.Not.Null);
                editor.customShaderGUI.ValidateMaterial(copy);
                Assert.That(copy.renderQueue, Is.EqualTo(3010));
            }
            finally
            {
                if (editor != null) Object.DestroyImmediate(editor);
                Object.DestroyImmediate(copy);
            }
        }

        [Test]
        public void ExternalBuild_IsNonDevelopment_AndUsesOnlyBuildScopedPlaytestDefine()
        {
            BuildPlayerOptions options = PlaytestBuildMenu.CreateBuildOptions();
            Assert.That(options.target, Is.EqualTo(BuildTarget.WebGL));
            Assert.That(options.options, Is.EqualTo(BuildOptions.None));
            Assert.That(options.extraScriptingDefines, Is.EqualTo(new[] { "CGR_PLAYTEST" }));
            Assert.That(options.scenes, Is.EqualTo(new[] { "Assets/Scenes/Boot.unity",
                "Assets/Scenes/Frontend.unity", "Assets/Scenes/SampleScene.unity" }));
            Assert.That(options.locationPathName.Replace('\\', '/'), Does.EndWith("/Builds/Playtest/v0.1"));
            Assert.That(TestBuildMenu.CreateBuildOptions(BuildTarget.WebGL, "Builds/Test/WebGL").options,
                Is.EqualTo(BuildOptions.Development));
        }
    }
}
