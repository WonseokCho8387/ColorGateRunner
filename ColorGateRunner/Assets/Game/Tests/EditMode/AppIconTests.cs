using ColorGateRunner.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEngine;

namespace ColorGateRunner.Tests.EditMode
{
    // The Player settings must carry the generated app icon, not Unity's
    // default, in every Android slot (AppIconBuilder.EnsureConfigured).
    public sealed class AppIconTests
    {
        [Test]
        public void AndroidIconSlots_UseTheGeneratedAppIcon()
        {
            Texture2D background = Load(AppIconBuilder.BackgroundPath, 432);
            Texture2D foreground = Load(AppIconBuilder.ForegroundPath, 432);
            Texture2D legacy = Load(AppIconBuilder.LegacyPath, 512);

            AssertSlots(AndroidPlatformIconKind.Adaptive, background, foreground);
            AssertSlots(AndroidPlatformIconKind.Round, legacy);
            AssertSlots(AndroidPlatformIconKind.Legacy, legacy);
        }

        private static Texture2D Load(string path, int size)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            Assert.That(texture, Is.Not.Null, path);
            Assert.That(texture.width, Is.EqualTo(size), path);
            Assert.That(texture.height, Is.EqualTo(size), path);
            return texture;
        }

        private static void AssertSlots(
            PlatformIconKind kind,
            params Texture2D[] layers)
        {
            PlatformIcon[] icons = PlayerSettings.GetPlatformIcons(
                NamedBuildTarget.Android,
                kind);
            Assert.That(icons, Is.Not.Empty, kind.ToString());
            foreach (PlatformIcon icon in icons)
            {
                for (int layer = 0; layer < layers.Length; layer++)
                {
                    Assert.That(icon.GetTexture(layer), Is.SameAs(layers[layer]),
                        $"{kind} {icon.width}px layer {layer}");
                }
            }
        }
    }
}
