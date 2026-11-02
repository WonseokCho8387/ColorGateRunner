using System.Collections.Generic;
using System.IO;
using ColorGateRunner.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class NeonUiDesignSystemTests
    {
        private const string BootScenePath = "Assets/Scenes/Boot.unity";
        private const string FrontendScenePath = "Assets/Scenes/Frontend.unity";
        private const string CampaignScenePath = "Assets/Scenes/SampleScene.unity";
        private const string PlayerFacingSymbols = "·•—∞";

        private static readonly string[] SurfaceAssets =
        {
            "Panel", "Modal", "ItemCard", "ButtonPrimary",
            "ButtonSecondary", "ButtonDanger", "ResourceChip"
        };

        private static readonly HashSet<string> ButtonSurfaceNames =
            new HashSet<string>
            {
                "ButtonPrimary", "ButtonSecondary", "ButtonDanger"
            };

        [Test]
        public void NeonFont_CoversPrintableAsciiAndPlayerFacingSymbols()
        {
            Font font = LoadNeonFont();
            for (char character = ' '; character <= '~'; character++)
            {
                Assert.That(font.HasCharacter(character), Is.True,
                    $"Missing glyph U+{(int)character:X4}");
            }

            foreach (char character in PlayerFacingSymbols)
            {
                Assert.That(font.HasCharacter(character), Is.True,
                    $"Missing glyph U+{(int)character:X4}");
            }
        }

        [Test]
        public void GeneratedScenes_UseNeonFontWithoutSyntheticBold()
        {
            Font font = LoadNeonFont();
            string[] scenes =
            {
                BootScenePath, FrontendScenePath, CampaignScenePath
            };
            for (int sceneIndex = 0; sceneIndex < scenes.Length; sceneIndex++)
            {
                WithScene(scenes[sceneIndex], scene =>
                {
                    int count = 0;
                    foreach (Text text in FindAll<Text>(scene))
                    {
                        count++;
                        string path = $"{scene.name}/{text.name}";
                        Assert.That(text.font, Is.SameAs(font), path);
                        Assert.That(text.fontStyle,
                            Is.EqualTo(FontStyle.Normal), path);
                    }

                    Assert.That(count, Is.GreaterThan(0), scene.name);
                });
            }
        }

        [Test]
        public void NeonSurfaces_KeepChamferedTransparentCornersAndSolidCenter()
        {
            for (int index = 0; index < SurfaceAssets.Length; index++)
            {
                string path = Theme01UiSkinBuilder.ArtFolder + "/" +
                    SurfaceAssets[index] + ".png";
                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                try
                {
                    Assert.That(ImageConversion.LoadImage(
                        texture, File.ReadAllBytes(path)), Is.True, path);
                    Assert.That(texture.width, Is.EqualTo(256), path);
                    Assert.That(texture.height, Is.EqualTo(256), path);
                    // Unity rows start at the bottom: (9, 246) is the image's
                    // top-left large chamfer and (246, 9) its bottom-right.
                    Assert.That(texture.GetPixel(0, 0).a, Is.LessThan(0.06f), path);
                    Assert.That(texture.GetPixel(9, 246).a, Is.LessThan(0.06f), path);
                    Assert.That(texture.GetPixel(246, 9).a, Is.LessThan(0.06f), path);
                    Assert.That(texture.GetPixel(128, 128).a,
                        Is.GreaterThan(0.75f), path);
                }
                finally
                {
                    Object.DestroyImmediate(texture);
                }
            }
        }

        [Test]
        public void LobbyStageCardCopy_StaysInsideCardAndAbovePlay()
        {
            WithScene(FrontendScenePath, scene =>
            {
                RectTransform card = FindRect(scene, "LobbyStageCard");
                RectTransform play = FindRect(scene, "LobbyPlayCampaignButton");
                string[] rows =
                {
                    "LobbyRecommendedStageText", "LobbyStageTitleText",
                    "LobbyStageMechanicText"
                };
                float previousBottom = float.MaxValue;
                for (int index = 0; index < rows.Length; index++)
                {
                    RectTransform row = FindRect(scene, rows[index]);
                    Assert.That(row.anchorMax.y,
                        Is.LessThanOrEqualTo(card.anchorMax.y), rows[index]);
                    Assert.That(row.anchorMax.y,
                        Is.LessThanOrEqualTo(previousBottom + 0.0001f),
                        rows[index]);
                    Assert.That(row.anchorMin.y,
                        Is.GreaterThan(play.anchorMax.y), rows[index]);
                    previousBottom = row.anchorMin.y;
                }
            });
        }

        [Test]
        public void NavigationSelectionFrame_StaysAboveTabLabel()
        {
            WithScene(FrontendScenePath, scene =>
            {
                string[] tabs =
                {
                    "Shop", "Leaderboard", "Home", "Journey", "Collection"
                };
                for (int index = 0; index < tabs.Length; index++)
                {
                    RectTransform button =
                        FindRect(scene, tabs[index] + "NavigationButton");
                    RectTransform selection =
                        FindRect(scene, tabs[index] + "NavigationSelection");
                    RectTransform label =
                        button.Find("Label") as RectTransform;
                    Assert.That(label, Is.Not.Null, tabs[index]);
                    float buttonHeight = button.anchorMax.y - button.anchorMin.y;
                    float labelTop = button.anchorMin.y +
                        buttonHeight * label.anchorMax.y;
                    Assert.That(selection.anchorMin.y,
                        Is.GreaterThanOrEqualTo(labelTop), tabs[index]);
                }
            });
        }

        [Test]
        public void SkinnedButtonLabels_AreInsetInsideNeonStroke()
        {
            string[] scenes =
            {
                BootScenePath, FrontendScenePath, CampaignScenePath
            };
            for (int sceneIndex = 0; sceneIndex < scenes.Length; sceneIndex++)
            {
                WithScene(scenes[sceneIndex], scene =>
                {
                    int checkedCount = 0;
                    foreach (Button button in FindAll<Button>(scene))
                    {
                        Image image = button.GetComponent<Image>();
                        RectTransform label =
                            button.transform.Find("Label") as RectTransform;
                        if (image == null || image.sprite == null ||
                            image.color.a < 0.01f ||
                            label == null ||
                            button.transform.Find("ThemeIcon") != null ||
                            !ButtonSurfaceNames.Contains(image.sprite.name))
                        {
                            continue;
                        }

                        checkedCount++;
                        string path = $"{scene.name}/{button.name}";
                        Assert.That(label.offsetMin.x,
                            Is.GreaterThanOrEqualTo(12f), path);
                        Assert.That(label.offsetMax.x,
                            Is.LessThanOrEqualTo(-12f), path);
                    }

                    if (scenes[sceneIndex] == FrontendScenePath)
                    {
                        Assert.That(checkedCount, Is.GreaterThan(0));
                    }
                });
            }
        }

        private static Font LoadNeonFont()
        {
            Font font = AssetDatabase.LoadAssetAtPath<Font>(
                Theme01UiSkinBuilder.FontPath);
            Assert.That(font, Is.Not.Null, Theme01UiSkinBuilder.FontPath);
            return font;
        }

        private static void WithScene(string path, System.Action<Scene> body)
        {
            Scene scene = EditorSceneManager.OpenScene(
                path,
                OpenSceneMode.Additive);
            try
            {
                body(scene);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static IEnumerable<T> FindAll<T>(Scene scene)
            where T : Component
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int index = 0; index < roots.Length; index++)
            {
                T[] components = roots[index].GetComponentsInChildren<T>(true);
                for (int item = 0; item < components.Length; item++)
                {
                    yield return components[item];
                }
            }
        }

        private static RectTransform FindRect(Scene scene, string name)
        {
            foreach (RectTransform rect in FindAll<RectTransform>(scene))
            {
                if (rect.name == name)
                {
                    return rect;
                }
            }

            Assert.Fail($"{scene.name} is missing {name}.");
            return null;
        }
    }
}
