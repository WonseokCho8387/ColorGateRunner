using System.Collections.Generic;
using ColorGateRunner.Editor;
using ColorGateRunner.Presentation;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class SettingsPanelDesignTests
    {
        private const string FrontendScenePath = "Assets/Scenes/Frontend.unity";
        private const string CampaignScenePath = "Assets/Scenes/SampleScene.unity";

        [Test]
        public void Availability_HidesInertRowsAndShowsHapticsOnlyWhereSupported()
        {
            SettingsAvailability desktop = SettingsAvailability.ForPlatform(false);
            Assert.That(desktop.Notifications, Is.False);
            Assert.That(desktop.Music, Is.True);
            Assert.That(desktop.Sfx, Is.True);
            Assert.That(desktop.Vibration, Is.False);
            Assert.That(desktop.AnyAudio, Is.True);
            Assert.That(desktop.AnyGameplay, Is.False);

            SettingsAvailability handheld = SettingsAvailability.ForPlatform(true);
            Assert.That(handheld.Vibration, Is.True);
            Assert.That(handheld.AnyGameplay, Is.True);
            Assert.That(handheld.Music, Is.True);
            Assert.That(handheld.Sfx, Is.True);
        }

        [Test]
        public void SettingsPanels_UseNeonModalSwitchesAndTextButtons()
        {
            string[] scenes = { FrontendScenePath, CampaignScenePath };
            for (int sceneIndex = 0; sceneIndex < scenes.Length; sceneIndex++)
            {
                WithScene(scenes[sceneIndex], scene =>
                {
                    SettingsPanelController panel =
                        FindSingle<SettingsPanelController>(scene);
                    string path = scene.name;
                    Assert.That(panel.HasRequiredReferences(), Is.True, path);
                    Assert.That(SpriteName(panel.GetComponent<Image>()),
                        Is.EqualTo("Modal"), path);
                    Toggle[] toggles =
                    {
                        panel.NotificationToggle, panel.MusicToggle,
                        panel.SfxToggle, panel.VibrationToggle
                    };
                    for (int index = 0; index < toggles.Length; index++)
                    {
                        SettingsSwitchView view =
                            toggles[index].GetComponent<SettingsSwitchView>();
                        Assert.That(view, Is.Not.Null, toggles[index].name);
                        Assert.That(view.HasRequiredReferences, Is.True,
                            toggles[index].name);
                        Assert.That(
                            SpriteName(toggles[index].GetComponent<Image>()),
                            Is.EqualTo("ItemCard"),
                            toggles[index].name);
                    }

                    Assert.That(SpriteName(panel.ApplyButton.GetComponent<Image>()),
                        Is.EqualTo("ButtonPrimary"), path);
                    Assert.That(SpriteName(panel.CancelButton.GetComponent<Image>()),
                        Is.EqualTo("ButtonSecondary"), path);
                    Button[] textButtons =
                    {
                        panel.ApplyButton, panel.CancelButton,
                        panel.TermsButton, panel.PrivacyButton,
                        panel.SupportButton
                    };
                    for (int index = 0; index < textButtons.Length; index++)
                    {
                        Assert.That(
                            textButtons[index].transform.Find("ThemeIcon"),
                            Is.Null,
                            textButtons[index].name);
                    }

                    Assert.That(FindText(scene,
                            UnifiedSettingsPanelBuilder.FontCredit),
                        Is.True, path);
                });
            }
        }

        [Test]
        public void SettingsOpen_AppliesAvailabilityAndVersion()
        {
            WithScene(FrontendScenePath, scene =>
            {
                SettingsPanelController panel =
                    FindSingle<SettingsPanelController>(scene);
                panel.SetAvailabilityForTests(
                    SettingsAvailability.ForPlatform(true));
                panel.Open(null);

                Assert.That(panel.AudioSection.activeSelf, Is.True);
                Assert.That(panel.MusicToggle.gameObject.activeSelf, Is.True);
                Assert.That(panel.SfxToggle.gameObject.activeSelf, Is.True);
                Assert.That(panel.GameplaySection.activeSelf, Is.True);
                Assert.That(panel.VibrationToggle.gameObject.activeSelf,
                    Is.True);
                Assert.That(panel.NotificationToggle.gameObject.activeSelf,
                    Is.False);
                Assert.That(panel.NotificationNoticeText.gameObject.activeSelf,
                    Is.False);
                Assert.That(panel.VersionText.text,
                    Is.EqualTo("VERSION " + Application.version));
                Assert.That(panel.StatusText.text,
                    Is.EqualTo("SETTINGS UNAVAILABLE"));

                panel.SetAvailabilityForTests(
                    SettingsAvailability.ForPlatform(false));
                panel.Open(null);
                Assert.That(panel.GameplaySection.activeSelf, Is.False);
                Assert.That(panel.VibrationToggle.gameObject.activeSelf,
                    Is.False);
            });
        }

        [Test]
        public void SettingsSwitch_MovesKnobAndUsesMagentaWhenOn()
        {
            WithScene(FrontendScenePath, scene =>
            {
                SettingsPanelController panel =
                    FindSingle<SettingsPanelController>(scene);
                SettingsSwitchView view =
                    panel.VibrationToggle.GetComponent<SettingsSwitchView>();
                view.Show(true);
                Assert.That(view.Knob.anchorMin.x, Is.GreaterThan(0.5f));
                Assert.That(view.Track.color, Is.EqualTo(NeonUiPalette.Magenta));
                view.Show(false);
                Assert.That(view.Knob.anchorMax.x, Is.LessThan(0.5f));
                Assert.That(view.Track.color,
                    Is.Not.EqualTo(NeonUiPalette.Magenta));
            });
        }

        [Test]
        public void RankPage_ShowsTruthfulOfflineStatusCard()
        {
            WithScene(FrontendScenePath, scene =>
            {
                Transform rank = FindNamed(scene, "LeaderboardPageRoot");
                Transform card = rank.Find("ComingSoonStatusCard");
                Assert.That(card, Is.Not.Null);
                Assert.That(SpriteName(card.GetComponent<Image>()),
                    Is.EqualTo("ItemCard"));
                Assert.That(card.Find("ComingSoonStatus")
                    .GetComponent<Text>().text, Is.EqualTo("OFFLINE"));
                Assert.That(card.Find("ComingSoonBody")
                    .GetComponent<Text>().text,
                    Does.Contain("NOT AVAILABLE IN THIS BUILD"));
            });
        }

        private static string SpriteName(Image image)
        {
            Assert.That(image, Is.Not.Null);
            Assert.That(image.sprite, Is.Not.Null, image.name);
            return image.sprite.name;
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

        private static T FindSingle<T>(Scene scene)
            where T : Component
        {
            var found = new List<T>();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int index = 0; index < roots.Length; index++)
            {
                found.AddRange(roots[index].GetComponentsInChildren<T>(true));
            }
            Assert.That(found.Count, Is.EqualTo(1), typeof(T).Name);
            return found[0];
        }

        private static Transform FindNamed(Scene scene, string name)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int index = 0; index < roots.Length; index++)
            {
                Transform[] transforms =
                    roots[index].GetComponentsInChildren<Transform>(true);
                for (int item = 0; item < transforms.Length; item++)
                {
                    if (transforms[item].name == name)
                    {
                        return transforms[item];
                    }
                }
            }
            Assert.Fail($"{scene.name} is missing {name}.");
            return null;
        }

        private static bool FindText(Scene scene, string value)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int index = 0; index < roots.Length; index++)
            {
                Text[] texts = roots[index].GetComponentsInChildren<Text>(true);
                for (int item = 0; item < texts.Length; item++)
                {
                    if (texts[item].text == value)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
