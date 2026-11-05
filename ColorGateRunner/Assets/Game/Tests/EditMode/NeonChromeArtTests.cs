using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace ColorGateRunner.Tests.EditMode
{
    // Iteration 66: Gold is reserved for currency, so the Frontend frames and
    // Theme01 icons recolored by Tools/Recolor-NeonChrome.mjs keep no gold
    // chrome while the coin stays gold.
    public sealed class NeonChromeArtTests
    {
        private const string Theme = "Assets/Game/Art/UI/Theme01/";

        private static readonly string[] Recolored =
        {
            "Frontend/TopResourceBar", "Frontend/BottomNavigationDock",
            "Frontend/NavigationSelection", "Frontend/LobbyStageCard",
            "Frontend/PageHeader",
            "Navigation/IconNavShop", "Navigation/IconNavRank",
            "Navigation/IconNavHome", "Navigation/IconNavJourney",
            "Navigation/IconNavCollection",
            "IconHeart", "IconProfile", "IconBooster", "IconShield",
            "IconBack", "IconPause", "IconPlay", "IconRetry", "IconSettings",
        };

        // The pre-recolor art held 4-21 percent gold; the trim, its bronze
        // shadows and yellow-green highlights must all be gone.
        [TestCaseSource(nameof(Recolored))]
        public void ChromeAndIcons_CarryNoGold(string name)
        {
            Assert.That(GoldShare(name), Is.LessThan(0.001f), name);
        }

        [Test]
        public void Coin_StaysGold()
        {
            Assert.That(GoldShare("IconCoin"), Is.GreaterThan(0.3f));
        }

        // Share of visible pixels in the gold hue band, matching the
        // recolor tool's full-weight thresholds.
        private static float GoldShare(string name)
        {
            string path = Path.GetFullPath(Theme + name + ".png");
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                Assert.That(texture.LoadImage(File.ReadAllBytes(path)), Is.True);
                Color32[] pixels = texture.GetPixels32();
                int visible = 0;
                int gold = 0;
                for (int index = 0; index < pixels.Length; index++)
                {
                    Color32 pixel = pixels[index];
                    if (pixel.a < 40)
                    {
                        continue;
                    }
                    visible++;
                    Color.RGBToHSV(pixel, out float hue, out float saturation,
                        out float value);
                    float degrees = hue * 360f;
                    if (degrees >= 26f && degrees <= 80f &&
                        saturation > 0.22f && value > 0.14f)
                    {
                        gold++;
                    }
                }
                return visible == 0 ? 0f : (float)gold / visible;
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }
    }
}
