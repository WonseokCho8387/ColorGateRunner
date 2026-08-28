using System.IO;
using ColorGateRunner.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class Theme01UiArtTests
    {
        private static readonly string[] SlicedAssets =
        {
            "Panel", "Modal", "ItemCard", "ButtonPrimary",
            "ButtonSecondary", "ButtonDanger", "ResourceChip"
        };

        private static readonly string[] IconAssets =
        {
            "Coin", "Heart", "Shield", "Booster", "Settings",
            "Play", "Back", "Pause", "Retry", "Continue"
        };

        private static readonly string[] ColorEmblemAssets =
        {
            "Red", "Blue", "Green", "Yellow", "Purple", "Cyan"
        };

        [Test]
        public void Theme01UiArt_ImportsTransparentSpritesAndSliceBorders()
        {
            for (int index = 0; index < SlicedAssets.Length; index++)
            {
                string path = Theme01UiSkinBuilder.ArtFolder + "/" +
                    SlicedAssets[index] + ".png";
                AssertSprite(path, true);
            }

            for (int index = 0; index < IconAssets.Length; index++)
            {
                string path = Theme01UiSkinBuilder.ArtFolder + "/Icon" +
                    IconAssets[index] + ".png";
                AssertSprite(path, false);
            }

            AssertHasRealTransparency(
                Theme01UiSkinBuilder.ArtFolder + "/ButtonPrimary.png");
            AssertHasRealTransparency(
                Theme01UiSkinBuilder.ArtFolder + "/IconShield.png");

            for (int index = 0;
                index < ColorEmblemAssets.Length;
                index++)
            {
                string path = Theme01UiSkinBuilder.ArtFolder +
                    "/ColorEmblems/Emblem" +
                    ColorEmblemAssets[index] + ".png";
                AssertSprite(path, false);
                TextureImporter importer =
                    AssetImporter.GetAtPath(path) as TextureImporter;
                Assert.That(
                    importer.spritePixelsPerUnit,
                    Is.EqualTo(512f),
                    path);
                AssertHasRealTransparency(path);
            }
        }

        private static void AssertSprite(string path, bool sliced)
        {
            Assert.That(File.Exists(path), Is.True, path);
            Assert.That(AssetDatabase.LoadAssetAtPath<Sprite>(path),
                Is.Not.Null, path);
            TextureImporter importer = AssetImporter.GetAtPath(path)
                as TextureImporter;
            Assert.That(importer, Is.Not.Null, path);
            Assert.That(importer.textureType,
                Is.EqualTo(TextureImporterType.Sprite), path);
            Assert.That(importer.alphaIsTransparency, Is.True, path);
            Assert.That(importer.mipmapEnabled, Is.False, path);
            Assert.That(importer.spriteBorder,
                Is.EqualTo(sliced
                    ? Vector4.one * Theme01UiSkinBuilder.SliceBorder
                    : Vector4.zero), path);
        }

        private static void AssertHasRealTransparency(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                Assert.That(ImageConversion.LoadImage(texture, bytes),
                    Is.True, path);
                Color32[] pixels = texture.GetPixels32();
                int transparent = 0;
                int opaque = 0;
                for (int index = 0; index < pixels.Length; index++)
                {
                    if (pixels[index].a < 16) transparent++;
                    if (pixels[index].a > 220) opaque++;
                }

                Assert.That(transparent, Is.GreaterThan(0), path);
                Assert.That(opaque, Is.GreaterThan(0), path);
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }
    }
}
