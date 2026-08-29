using System.IO;
using ColorGateRunner.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class Theme01GameplayArtTests
    {
        private static readonly string[] Models =
        {
            "CyberOrbRunner.fbx",
            "NeonGate.fbx",
            "NeonTrackSegment.fbx",
            "NeonGoalPortal.fbx",
            "NeonCityBackdrop.fbx"
        };

        private static readonly string[] MapSuffixes =
        {
            "BaseColor.png",
            "Emission.png",
            "Normal.png",
            "MetallicSmoothness.png",
            "UVLayout.png"
        };

        [Test]
        public void Theme01GameplayArt_PreservesSourceExportsAndPbrMaps()
        {
            Assert.That(File.Exists(
                "Assets/Game/Art/Gameplay/Theme01/Source/Theme01_Gameplay.blend"),
                Is.True);
            for (int index = 0; index < Models.Length; index++)
            {
                string path = GrayboxSceneBuilder.Theme01ModelsFolder +
                    "/" + Models[index];
                Assert.That(File.Exists(path), Is.True, path);
                Assert.That(AssetDatabase.LoadAssetAtPath<GameObject>(path),
                    Is.Not.Null, path);
            }
            string[] families = { "Runner_", "Gate_", "Environment_" };
            for (int family = 0; family < families.Length; family++)
            {
                for (int map = 0; map < MapSuffixes.Length; map++)
                {
                    string path = "Assets/Game/Art/Gameplay/Theme01/Textures/" +
                        families[family] + MapSuffixes[map];
                    Assert.That(File.Exists(path), Is.True, path);
                    Assert.That(AssetDatabase.LoadAssetAtPath<Texture2D>(path),
                        Is.Not.Null, path);
                }
            }
        }

        [Test]
        public void CyberOrbRunner_HasReadableRearSilhouetteParts()
        {
            GameObject runner = AssetDatabase.LoadAssetAtPath<GameObject>(
                GrayboxSceneBuilder.Theme01ModelsFolder +
                "/CyberOrbRunner.fbx");
            Assert.That(runner, Is.Not.Null);
            string[] parts =
            {
                "HullShell",
                "ColorShell",
                "RearBumper",
                "RearThruster_L",
                "RearThruster_R",
                "RearThrusterGlow_L",
                "RearThrusterGlow_R",
                "RearChevronGlow_L",
                "RearChevronGlow_R",
                "RearLightBar",
                "SideFin_L",
                "SideFin_R"
            };
            for (int index = 0; index < parts.Length; index++)
            {
                Assert.That(
                    FindNamedTransform(runner.transform, parts[index]),
                    Is.Not.Null,
                    parts[index]);
            }
        }

        [Test]
        public void GameplayColorMaterials_EmitWhileDarkAlloyDoesNot()
        {
            string[] emissiveNames =
            {
                "Red",
                "Blue",
                "Green",
                "Yellow",
                "Purple",
                "Cyan",
                "Neutral"
            };
            for (int index = 0; index < emissiveNames.Length; index++)
            {
                Material material = AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/Game/Generated/Materials/" +
                    emissiveNames[index] + ".mat");
                Assert.That(material, Is.Not.Null, emissiveNames[index]);
                Assert.That(
                    material.IsKeywordEnabled("_EMISSION"),
                    Is.True,
                    emissiveNames[index]);
                Assert.That(
                    material.GetColor("_EmissionColor").maxColorComponent,
                    Is.GreaterThan(1f),
                    emissiveNames[index]);
            }

            Material dark = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Game/Generated/Materials/Theme01DarkAlloy.mat");
            Assert.That(dark, Is.Not.Null);
            Assert.That(dark.IsKeywordEnabled("_EMISSION"), Is.False);
            Assert.That(
                dark.GetColor("_EmissionColor").maxColorComponent,
                Is.LessThanOrEqualTo(0.001f));

            Material glass = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Game/Generated/Materials/RunnerGlass.mat");
            Assert.That(glass, Is.Not.Null);
            Assert.That(glass.IsKeywordEnabled("_EMISSION"), Is.False);
            Assert.That(
                glass.GetColor("_EmissionColor").maxColorComponent,
                Is.LessThanOrEqualTo(0.001f));
            Assert.That(glass.GetFloat("_Smoothness"), Is.GreaterThanOrEqualTo(0.9f));
        }

        [Test]
        public void ProtectionFieldShader_HasNoCompilerErrors()
        {
            AssertShaderHasNoCompilerErrors(
                "ProtectionField.shader",
                "ColorGateRunner/ProtectionField");
        }

        [TestCase(
            "FlickerEmblemDissolve.shader",
            "ColorGateRunner/FlickerEmblemDissolve")]
        [TestCase(
            "FlickerGateFrameDissolve.shader",
            "ColorGateRunner/FlickerGateFrameDissolve")]
        public void FlickerShaders_HaveNoCompilerErrors(
            string fileName,
            string shaderName)
        {
            AssertShaderHasNoCompilerErrors(fileName, shaderName);
        }

        private static void AssertShaderHasNoCompilerErrors(
            string fileName,
            string shaderName)
        {
            string path =
                "Assets/Game/Art/Gameplay/Theme01/" + fileName;
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
            Assert.That(shader, Is.Not.Null, path);
            Assert.That(shader.name, Is.EqualTo(shaderName));

            foreach (var message in ShaderUtil.GetShaderMessages(shader))
            {
                if (message.severity.ToString() == "Error")
                {
                    Assert.Fail(
                        $"{path}: {message.message} ({message.platform})");
                }
            }
        }

        private static Transform FindNamedTransform(
            Transform root,
            string name)
        {
            Transform[] transforms =
                root.GetComponentsInChildren<Transform>(true);
            for (int index = 0; index < transforms.Length; index++)
            {
                if (transforms[index].name == name)
                {
                    return transforms[index];
                }
            }
            return null;
        }
    }
}
