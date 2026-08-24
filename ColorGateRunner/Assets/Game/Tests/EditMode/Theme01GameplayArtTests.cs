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
    }
}
