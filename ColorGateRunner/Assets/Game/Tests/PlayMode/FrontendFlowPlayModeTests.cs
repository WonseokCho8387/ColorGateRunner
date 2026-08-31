using System;
using System.Collections;
using ColorGateRunner.Core;
using ColorGateRunner.Presentation;
using ColorGateRunner.Product;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ColorGateRunner.Tests.PlayMode
{
    public sealed class FrontendFlowPlayModeTests
    {
        private const string BootPath = "Assets/Scenes/Boot.unity";
        private const string FrontendPath = "Assets/Scenes/Frontend.unity";
        private const string CampaignPath = "Assets/Scenes/SampleScene.unity";
        private const string UnlockKey =
            "ColorGateRunner.Stage.HighestUnlocked";
        private const string RecordKey =
            "ColorGateRunner.Stage.Record.6";
        private CampaignPlayerPrefsSnapshot _campaignProgressSnapshot;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _campaignProgressSnapshot =
                CampaignPlayerPrefsSnapshot.Capture();
            bool completed = false;
            try
            {
                yield return DestroyAllAppRoots();
                AppRoot.ClearTestState();
                completed = true;
            }
            finally
            {
                if (!completed)
                {
                    RestoreCampaignProgress();
                }
            }
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            try
            {
                yield return DestroyAllAppRoots();
                AppRoot.ClearTestState();
            }
            finally
            {
                RestoreCampaignProgress();
            }
        }

        [UnityTest]
        public IEnumerator BootToFrontend_StartsAtAccountChoiceWithOneAppRootAndEventSystem()
        {
            yield return LoadFrontendThroughBoot();

            FrontendSceneController controller = RequireController();
            Assert.That(controller.HasRequiredReferences(), Is.True);
            Assert.That(controller.Router.CurrentPage,
                Is.EqualTo(FrontendPage.AccountChoice));
            Assert.That(controller.ActivePrimaryPageCount(), Is.EqualTo(1));
            Assert.That(controller.TitlePageRoot.activeSelf, Is.True);
            Assert.That(controller.LobbyPageRoot.activeSelf, Is.False);
            Assert.That(controller.ProductReady, Is.True);
            Assert.That(controller.TitleProfileText.text, Is.EqualTo("GUEST"));
            Assert.That(controller.TitleAccountText.text, Is.EqualTo("GUEST"));
            Assert.That(controller.TitleVersionText.text, Does.StartWith("v"));
            Assert.That(controller.TitleLegalRoot.activeSelf, Is.False);
            Assert.That(
                Object.FindObjectsByType<AppRoot>().Length,
                Is.EqualTo(1));
            Assert.That(
                Object.FindObjectsByType<EventSystem>().Length,
                Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator ThemeOneUiSkin_UsesSlicedButtonsAndResourceIcons()
        {
            yield return LoadFrontendThroughBoot();

            FrontendSceneController controller = RequireController();
            Image startImage = controller.TitleStartButton.GetComponent<Image>();
            Assert.That(startImage.type, Is.EqualTo(Image.Type.Sliced));
            Assert.That(startImage.sprite, Is.Not.Null);
            Assert.That(startImage.sprite.name, Is.EqualTo("ButtonPrimary"));
            Image startIcon = controller.TitleStartButton.transform
                .Find("ThemeIcon")?.GetComponent<Image>();
            Assert.That(startIcon, Is.Not.Null);
            Assert.That(startIcon.raycastTarget, Is.False);

            controller.TitleStartButton.onClick.Invoke();
            yield return null;
            Transform currency = controller.CurrencySlotRoot.transform;
            Image coin = currency.Find("LobbyCoinIcon")?.GetComponent<Image>();
            Image heart = currency.Find("LobbyHeartIcon")?.GetComponent<Image>();
            Assert.That(coin, Is.Not.Null);
            Assert.That(heart, Is.Not.Null);
            Assert.That(coin.raycastTarget, Is.False);
            Assert.That(heart.raycastTarget, Is.False);
        }

        [UnityTest]
        public IEnumerator GuestChoicePersistsThenLobbyBackRequestsExit()
        {
            yield return LoadFrontendThroughBoot();
            FrontendSceneController controller = RequireController();

            controller.TitleStartButton.onClick.Invoke();
            controller.TitleStartButton.onClick.Invoke();
            yield return null;

            Assert.That(controller.Router.CurrentPage,
                Is.EqualTo(FrontendPage.Lobby));
            Assert.That(controller.ActivePrimaryPageCount(), Is.EqualTo(1));
            Assert.That(controller.LobbyProfileText.text, Is.EqualTo("GUEST"));
            Assert.That(controller.CurrencySlotRoot.activeSelf, Is.True);
            Assert.That(controller.LobbyThemeRoot.activeSelf, Is.True);
            Assert.That(controller.LobbyProgressionPanel, Is.Not.Null);
            Assert.That(controller.LobbyProgressionPanel.HeartText.text,
                Is.EqualTo("HEARTS 5/5"));
            Assert.That(GameObject.Find("LobbyTitle"), Is.Null);
            Assert.That(GameObject.Find("LobbyAccountText"), Is.Null);
            Assert.That(controller.LobbyStageMechanicText.text,
                Does.EndWith("NORMAL"));
            Assert.That(controller.LobbyPlayButton
                    .GetComponentInChildren<Text>().text,
                Is.EqualTo("PLAY"));
            Assert.That(controller.LobbyBackButton
                    .GetComponent<RectTransform>().anchorMin.y,
                Is.GreaterThanOrEqualTo(0.84f));
            Assert.That(controller.LobbySettingsButton
                    .GetComponent<RectTransform>().anchorMin.y,
                Is.GreaterThanOrEqualTo(0.84f));
            Assert.That(controller.LobbyPlayButton
                    .GetComponent<RectTransform>().anchorMax.y,
                Is.LessThanOrEqualTo(0.20f));
            Assert.That(controller.EventModuleSlotRoot.activeSelf, Is.False);
            Assert.That(controller.LobbyPlayButton.gameObject.activeSelf,
                Is.True);
            Assert.That(controller.TitleAccountButton.gameObject.activeSelf,
                Is.False);
            Assert.That(
                AppRoot.TryGetActive(out AppRoot activeRoot),
                Is.True);
            Assert.That(activeRoot.Graph.Profile.Current.ProfileId,
                Is.EqualTo("frontend-playmode-guest"));
            Assert.That(
                activeRoot.Graph.Profile.Current.AccountChoiceCompleted,
                Is.True);
            Assert.That(
                _campaignProgressSnapshot.MatchesCurrentState(),
                Is.True);

            controller.HandleBack();
            yield return null;
            Assert.That(controller.Router.CurrentPage,
                Is.EqualTo(FrontendPage.Lobby));
            Assert.That(controller.Router.CurrentModal,
                Is.EqualTo(FrontendModal.ExitConfirmation));
            Assert.That(controller.ActivePrimaryPageCount(), Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator LobbyProgression_ShowsEconomyThemeAndAcknowledgesReward()
        {
            AppServiceGraph graph = null;
            yield return LoadFrontendThroughBoot(() =>
            {
                graph = CreateGraph(new ProgressedGuestSaveService());
                return graph;
            });
            FrontendSceneController controller = RequireController();
            LobbyProgressionPanel panel = controller.LobbyProgressionPanel;

            Assert.That(controller.Router.CurrentPage,
                Is.EqualTo(FrontendPage.Lobby));
            Assert.That(panel.CoinText.text, Is.EqualTo("COINS 950"));
            Assert.That(panel.HeartText.text, Is.EqualTo("HEARTS 5/5"));
            Assert.That(panel.InventoryText.text,
                Is.EqualTo("SHIELD 2   BOOSTER 1"));
            Assert.That(panel.ThemeText.text,
                Is.EqualTo("COLOR COURTYARD"));
            Assert.That(panel.NextUpgradeText.text,
                Is.EqualTo("LOBBY 2/18   NEXT STAGE 6"));
            Assert.That(panel.RewardText.text,
                Does.Contain("2 UPGRADES"));
            Assert.That(panel.ActiveUpgradeVisualCount(), Is.EqualTo(2));
            Assert.That(graph.Progression.Lobby.PresentedMilestoneCount,
                Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator LobbyThemeOne_ShowsLayeredNonBlockingArtwork()
        {
            yield return LoadFrontendThroughBoot();
            FrontendSceneController controller = RequireController();
            controller.TitleStartButton.onClick.Invoke();
            yield return null;

            LobbyProgressionPanel panel = controller.LobbyProgressionPanel;
            Assert.That(panel.ThemeVisualCatalog, Is.Not.Null);
            Assert.That(panel.ThemeArtworkBackground.sprite, Is.Not.Null);
            Assert.That(panel.ThemeArtworkMidground.sprite, Is.Not.Null);
            Assert.That(panel.ThemeArtworkForeground.sprite, Is.Not.Null);
            Assert.That(panel.ThemeArtworkBackground.gameObject.activeSelf,
                Is.True);
            Assert.That(panel.ThemeArtworkMidground.gameObject.activeSelf,
                Is.True);
            Assert.That(panel.ThemeArtworkForeground.gameObject.activeSelf,
                Is.True);
            Assert.That(panel.ThemeArtworkBackground.raycastTarget, Is.False);
            Assert.That(panel.ThemeArtworkMidground.raycastTarget, Is.False);
            Assert.That(panel.ThemeArtworkForeground.raycastTarget, Is.False);
            Assert.That(
                AppRoot.TryGetActive(out AppRoot activeRoot),
                Is.True);
            int applied = Mathf.Clamp(
                activeRoot.Graph.Progression.Lobby.AppliedMilestoneCount,
                0,
                18);
            int expectedVisible = applied == 18 ? 6 : applied % 6;
            Assert.That(panel.ActiveUpgradeVisualCount(),
                Is.EqualTo(expectedVisible));
            Assert.That(controller.LobbyPlayButton.gameObject.activeSelf,
                Is.True);
            Assert.That(controller.LobbyPlayButton.interactable, Is.True);
        }

        [UnityTest]
        public IEnumerator CompletedAccountChoice_StartsDirectlyAtLobby()
        {
            yield return LoadFrontendThroughBoot(() =>
                CreateGraph(new ExistingGuestSaveService(true)));

            FrontendSceneController controller = RequireController();
            Assert.That(controller.Router.CurrentPage,
                Is.EqualTo(FrontendPage.Lobby));
            Assert.That(controller.TitlePageRoot.activeSelf, Is.False);
            Assert.That(controller.LobbyPageRoot.activeSelf, Is.True);
        }

        [UnityTest]
        public IEnumerator AccountChoiceSaveFailure_RemainsOnChoicePage()
        {
            AppServiceGraph graph = null;
            var save = new ArmableSaveFailureService();
            yield return LoadFrontendThroughBoot(() =>
            {
                graph = CreateGraph(save);
                return graph;
            });

            FrontendSceneController controller = RequireController();
            save.ArmFailure();
            controller.TitleStartButton.onClick.Invoke();
            yield return null;

            Assert.That(controller.Router.CurrentPage,
                Is.EqualTo(FrontendPage.AccountChoice));
            Assert.That(controller.Router.CurrentModal,
                Is.EqualTo(FrontendModal.SaveError));
            Assert.That(
                graph.Profile.Current.AccountChoiceCompleted,
                Is.False);
        }

        [UnityTest]
        public IEnumerator SharedSettings_SaveTogglesAndPreserveMasterVolume()
        {
            float originalVolume = AudioListener.volume;
            try
            {
                yield return LoadFrontendThroughBoot();
                FrontendSceneController controller = RequireController();
                controller.TitleStartButton.onClick.Invoke();
                controller.LobbySettingsButton.onClick.Invoke();
                yield return null;

                SettingsPanelController panel = controller.SettingsPanel;
                SetAllToggles(panel, true);
                AssertTogglePresentation(panel, true, true, true, true);
                SetAllToggles(panel, false);
                AssertTogglePresentation(panel, false, false, false, false);
                panel.NotificationToggle.isOn = true;
                panel.MusicToggle.isOn = false;
                panel.SfxToggle.isOn = false;
                panel.VibrationToggle.isOn = false;
                AssertTogglePresentation(panel, true, false, false, false);
                panel.ApplyButton.onClick.Invoke();
                yield return null;

                Assert.That(controller.Router.CurrentModal,
                    Is.EqualTo(FrontendModal.None));
                Assert.That(AudioListener.volume, Is.EqualTo(1f));
                Assert.That(
                    AppRoot.TryGetActive(out AppRoot root),
                    Is.True);
                Assert.That(root.CurrentSettings.MusicVolume,
                    Is.Zero);
                Assert.That(root.CurrentSettings.SfxVolume,
                    Is.Zero);
                Assert.That(root.CurrentSettings.LastNonZeroMusicVolume,
                    Is.EqualTo(1f));
                Assert.That(root.CurrentSettings.LastNonZeroSfxVolume,
                    Is.EqualTo(1f));
                Assert.That(root.CurrentSettings.NotificationEnabled,
                    Is.True);
                Assert.That(root.VibrationEnabled, Is.False);
                Assert.That(panel.NotificationNoticeText.text,
                    Does.Contain("NOT SENT YET"));
                Assert.That(panel.TermsButton.interactable, Is.False);
                Assert.That(panel.PrivacyButton.interactable, Is.False);
                Assert.That(panel.SupportButton.interactable, Is.False);
                Assert.That(panel.LinkStatusText.text,
                    Is.EqualTo("URL NOT CONFIGURED"));

                controller.LobbySettingsButton.onClick.Invoke();
                AssertTogglePresentation(panel, true, false, false, false);
                SetAllToggles(panel, true);
                panel.CancelButton.onClick.Invoke();
                controller.LobbySettingsButton.onClick.Invoke();
                AssertTogglePresentation(panel, true, false, false, false);
                panel.MusicToggle.isOn = true;
                panel.SfxToggle.isOn = true;
                panel.ApplyButton.onClick.Invoke();
                yield return null;
                Assert.That(root.CurrentSettings.MusicVolume,
                    Is.EqualTo(1f));
                Assert.That(root.CurrentSettings.SfxVolume,
                    Is.EqualTo(1f));
                Assert.That(root.CurrentSettings.MasterVolume,
                    Is.EqualTo(1f));
                Assert.That(
                    _campaignProgressSnapshot.MatchesCurrentState(),
                    Is.True);
            }
            finally
            {
                AudioListener.volume = originalVolume;
            }
        }

        [UnityTest]
        public IEnumerator AccountAndSettingsModal_AreTruthfulAndConsumeBack()
        {
            yield return LoadFrontendThroughBoot();
            FrontendSceneController controller = RequireController();

            controller.TitleAccountButton.onClick.Invoke();
            yield return null;
            Assert.That(controller.Router.CurrentModal,
                Is.EqualTo(FrontendModal.AccountUnavailable));
            Assert.That(controller.ModalMessageText.text,
                Does.Contain("NOT AVAILABLE"));
            Assert.That(controller.ModalMessageText.text,
                Does.Not.Contain("LINKED"));

            controller.HandleBack();
            Assert.That(controller.Router.CurrentModal,
                Is.EqualTo(FrontendModal.None));
            Assert.That(controller.Router.CurrentPage,
                Is.EqualTo(FrontendPage.AccountChoice));

            controller.TitleStartButton.onClick.Invoke();
            controller.HandleBack();
            Assert.That(controller.Router.CurrentPage,
                Is.EqualTo(FrontendPage.Lobby));
            Assert.That(controller.Router.CurrentModal,
                Is.EqualTo(FrontendModal.ExitConfirmation));
        }

        [UnityTest]
        public IEnumerator TitleBack_RequestsExitOnlyAfterConfirmation()
        {
            yield return LoadFrontendThroughBoot();
            FrontendSceneController controller = RequireController();
            var exit = new RecordingExitHandler();
            controller.SetExitHandlerForTests(exit);

            controller.HandleBack();
            Assert.That(controller.Router.CurrentModal,
                Is.EqualTo(FrontendModal.ExitConfirmation));
            Assert.That(exit.RequestCount, Is.Zero);

            controller.ModalConfirmButton.onClick.Invoke();
            Assert.That(exit.RequestCount, Is.EqualTo(1));
            Assert.That(controller.Router.CurrentModal,
                Is.EqualTo(FrontendModal.None));
        }

        [UnityTest]
        public IEnumerator DirectFrontendWithoutAppRoot_ShowsBootRequiredAndBlocksCampaign()
        {
            yield return SceneManager.LoadSceneAsync(
                FrontendPath,
                LoadSceneMode.Single);
            yield return null;

            FrontendSceneController controller = RequireController();
            var loader = new RecordingSceneLoader(
                SceneTransitionResult.Success());
            controller.SetSceneLoaderForTests(loader);
            Assert.That(controller.ProductReady, Is.False);
            Assert.That(controller.Router.CurrentModal,
                Is.EqualTo(FrontendModal.BootRequired));
            Assert.That(controller.LobbyPlayButton.interactable, Is.False);
            Assert.That(controller.PopupRoot.activeSelf, Is.True);

            controller.LobbyPlayButton.onClick.Invoke();
            Assert.That(loader.LoadCount, Is.Zero);
        }

        [UnityTest]
        public IEnumerator FailedCampaignLoad_ClearsBlockerAndShowsErrorOnce()
        {
            yield return LoadFrontendThroughBoot();
            FrontendSceneController controller = RequireController();
            var loader = new RecordingSceneLoader(
                SceneTransitionResult.Failure("PLANNED LOAD FAILURE"));
            controller.SetSceneLoaderForTests(loader);
            controller.TitleStartButton.onClick.Invoke();

            controller.LobbyPlayButton.onClick.Invoke();
            controller.LobbyPlayButton.onClick.Invoke();
            yield return null;

            Assert.That(loader.LoadCount, Is.EqualTo(1));
            Assert.That(controller.Router.IsTransitioning, Is.False);
            Assert.That(controller.TransitionBlockerRoot.activeSelf, Is.False);
            Assert.That(controller.LoadingRoot.activeSelf, Is.False);
            Assert.That(controller.Router.CurrentModal,
                Is.EqualTo(FrontendModal.SceneLoadError));
            Assert.That(controller.ModalMessageText.text,
                Is.EqualTo("PLANNED LOAD FAILURE"));
            Assert.That(
                AppRoot.TryGetActive(out AppRoot activeRoot),
                Is.True);
            Assert.That(activeRoot.TryConsumeCampaignLaunch(out _), Is.False);
        }

        [UnityTest]
        public IEnumerator StartStage_BypassesCampaignLobbyAndPreservesProgress()
        {
            const string record = "1|12.34|11.11|4|2";
            for (int stage = 1;
                stage <= CampaignPlayerPrefsSnapshot.StageCount;
                stage++)
            {
                PlayerPrefs.DeleteKey(
                    CampaignPlayerPrefsSnapshot.RecordKey(stage));
            }
            PlayerPrefs.SetInt(UnlockKey, 8);
            for (int stage = 1; stage <= 5; stage++)
            {
                PlayerPrefs.SetString(
                    "ColorGateRunner.Stage.Record." + stage,
                    "1|20|19|1|0");
            }
            PlayerPrefs.SetString(RecordKey, record);
            PlayerPrefs.Save();
            yield return LoadFrontendThroughBoot();
            FrontendSceneController controller = RequireController();

            controller.TitleStartButton.onClick.Invoke();
            Assert.That(controller.LobbyStageText.text,
                Is.EqualTo("STAGE 7"));
            Assert.That(controller.LobbyStageTitleText.text,
                Is.EqualTo("BOOSTER TIMING"));
            controller.LobbyPlayButton.onClick.Invoke();
            yield return WaitForScene(CampaignPath);
            yield return null;

            StageSceneController campaign =
                Object.FindFirstObjectByType<StageSceneController>();
            Assert.That(campaign, Is.Not.Null);
            Assert.That(campaign.LobbyRoot.activeSelf, Is.False);
            Assert.That(campaign.PreRunRoot.activeSelf, Is.True);
            Assert.That(campaign.UiFlow, Is.EqualTo(MobileUiFlow.PreRun));
            Assert.That(campaign.SelectedStageNumber, Is.EqualTo(7));
            Assert.That(PlayerPrefs.GetInt(UnlockKey, -1), Is.EqualTo(8));
            Assert.That(PlayerPrefs.GetString(RecordKey, string.Empty),
                Is.EqualTo(record));
            Assert.That(
                Object.FindObjectsByType<AppRoot>().Length,
                Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator FrontendPreRunBack_ReturnsDirectlyToFrontendLobby()
        {
            yield return LoadFrontendThroughBoot(() =>
                CreateGraph(new ExistingGuestSaveService(true)));
            FrontendSceneController frontend = RequireController();
            Assert.That(frontend.Router.CurrentPage,
                Is.EqualTo(FrontendPage.Lobby));

            frontend.LobbyPlayButton.onClick.Invoke();
            yield return WaitForScene(CampaignPath);
            StageSceneController campaign =
                Object.FindFirstObjectByType<StageSceneController>();
            Assert.That(campaign, Is.Not.Null);
            Assert.That(campaign.EnteredFromFrontendLaunch, Is.True);
            Assert.That(campaign.LobbyRoot.activeSelf, Is.False);
            Assert.That(campaign.PreRunRoot.activeSelf, Is.True);
            Assert.That(
                AppRoot.TryGetActive(out AppRoot root),
                Is.True);
            Assert.That(root.TryConsumeCampaignLaunch(out _), Is.False);

            campaign.PreRunBackButton.onClick.Invoke();
            float timeout = Time.realtimeSinceStartup + 5f;
            while (SceneManager.GetActiveScene().path == CampaignPath &&
                Time.realtimeSinceStartup < timeout)
            {
                Assert.That(campaign.LobbyRoot.activeSelf, Is.False);
                yield return null;
            }

            Assert.That(SceneManager.GetActiveScene().path,
                Is.EqualTo(FrontendPath));
            FrontendSceneController returned = RequireController();
            Assert.That(returned.Router.CurrentPage,
                Is.EqualTo(FrontendPage.Lobby));
            Assert.That(returned.LobbyPageRoot.activeSelf, Is.True);
            Assert.That(returned.ActivePrimaryPageCount(), Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator FrontendPreRunBack_FailureStaysAndAllowsOneRetry()
        {
            yield return LoadFrontendThroughBoot(() =>
                CreateGraph(new ExistingGuestSaveService(true)));
            RequireController().LobbyPlayButton.onClick.Invoke();
            yield return WaitForScene(CampaignPath);
            StageSceneController campaign =
                Object.FindFirstObjectByType<StageSceneController>();
            var loader = new DeferredSceneLoader();
            campaign.SetSceneTransitionLoaderForTests(loader);

            campaign.PreRunBackButton.onClick.Invoke();
            campaign.HandlePreRunBack();

            Assert.That(loader.LoadCount, Is.EqualTo(1));
            Assert.That(loader.LastPath, Is.EqualTo(campaign.FrontendScenePath));
            Assert.That(campaign.PreRunReturnTransitioning, Is.True);
            Assert.That(campaign.PreRunBackButton.interactable, Is.False);
            Assert.That(campaign.LobbyRoot.activeSelf, Is.False);

            loader.Complete(SceneTransitionResult.Failure("PLANNED"));

            Assert.That(campaign.UiFlow, Is.EqualTo(MobileUiFlow.PreRun));
            Assert.That(campaign.PreRunRoot.activeSelf, Is.True);
            Assert.That(campaign.LobbyRoot.activeSelf, Is.False);
            Assert.That(campaign.PreRunReturnTransitioning, Is.False);
            Assert.That(campaign.PreRunBackButton.interactable, Is.True);
            Assert.That(campaign.PreRunReturnError, Is.EqualTo("PLANNED"));

            campaign.PreRunBackButton.onClick.Invoke();
            Assert.That(loader.LoadCount, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator FrontendStageClear_ReturnsToLobbyAndShowsReward()
        {
            PlayerPrefs.SetInt(UnlockKey, 1);
            for (int stage = 1;
                stage <= CampaignPlayerPrefsSnapshot.StageCount;
                stage++)
            {
                PlayerPrefs.DeleteKey(
                    CampaignPlayerPrefsSnapshot.RecordKey(stage));
            }
            PlayerPrefs.Save();
            var save = new RoundtripSaveService();
            yield return LoadFrontendThroughBoot(() => CreateGraph(save));
            RequireController().LobbyPlayButton.onClick.Invoke();
            yield return WaitForScene(CampaignPath);
            StageSceneController campaign =
                Object.FindFirstObjectByType<StageSceneController>();

            ClearCurrentStage(campaign);
            campaign.Tick(1.3f);
            Assert.That(campaign.ClearPanel.activeSelf, Is.True);
            campaign.ClearLobbyButton.onClick.Invoke();
            yield return WaitForScene(FrontendPath);
            FrontendSceneController returned = RequireController();

            Assert.That(returned.Router.CurrentPage,
                Is.EqualTo(FrontendPage.Lobby));
            Assert.That(returned.LobbyProgressionPanel.CoinText.text,
                Is.EqualTo("COINS 100"));
            Assert.That(returned.LobbyProgressionPanel.NextUpgradeText.text,
                Is.EqualTo("LOBBY 0/18   NEXT STAGE 2"));
        }

        [UnityTest]
        public IEnumerator TogglePresentation_MatchesAfterSaveReloadAndInPause()
        {
            var save = new RoundtripSaveService();
            yield return LoadFrontendThroughBoot(() => CreateGraph(save));
            FrontendSceneController frontend = RequireController();
            frontend.LobbySettingsButton.onClick.Invoke();
            SettingsPanelController panel = frontend.SettingsPanel;
            panel.NotificationToggle.isOn = true;
            panel.MusicToggle.isOn = false;
            panel.SfxToggle.isOn = true;
            panel.VibrationToggle.isOn = false;
            AssertTogglePresentation(panel, true, false, true, false);
            panel.ApplyButton.onClick.Invoke();

            yield return DestroyAllAppRoots();
            AppRoot.ClearTestState();
            yield return LoadFrontendThroughBoot(() => CreateGraph(save));
            frontend = RequireController();
            Assert.That(frontend.Router.CurrentPage,
                Is.EqualTo(FrontendPage.Lobby));
            frontend.LobbySettingsButton.onClick.Invoke();
            panel = frontend.SettingsPanel;
            AssertTogglePresentation(panel, true, false, true, false);
            panel.CancelButton.onClick.Invoke();

            frontend.LobbyPlayButton.onClick.Invoke();
            yield return WaitForScene(CampaignPath);
            StageSceneController campaign =
                Object.FindFirstObjectByType<StageSceneController>();
            campaign.StartSelectedStage();
            campaign.RequestPause();
            campaign.RequestPauseSettings();

            Assert.That(campaign.PauseSettingsPanel.gameObject.activeSelf,
                Is.True);
            AssertTogglePresentation(
                campaign.PauseSettingsPanel,
                true,
                false,
                true,
                false);
        }

        [UnityTest]
        public IEnumerator CampaignStart_UsesProductInventoryAndPersistsSpend()
        {
            var save = new StockedCampaignSaveService();
            yield return LoadFrontendThroughBoot(() => CreateGraph(save));
            RequireController().LobbyPlayButton.onClick.Invoke();
            yield return WaitForScene(CampaignPath);
            StageSceneController campaign =
                Object.FindFirstObjectByType<StageSceneController>();

            Assert.That(campaign.SelectedStageNumber, Is.EqualTo(8));
            Assert.That(campaign.ShieldToggleText.text,
                Is.EqualTo("SHIELD x2: OFF"));
            Assert.That(campaign.BoosterToggleText.text,
                Is.EqualTo("BOOSTER x1: OFF"));
            int savesBeforeStart = save.SaveCount;
            campaign.ToggleShieldSelection();
            campaign.ToggleBoosterSelection();
            campaign.StartSelectedStage();

            Assert.That(campaign.Session.FlowState,
                Is.EqualTo(StageFlowState.Countdown));
            Assert.That(save.Stored.Economy.ShieldCount, Is.EqualTo(1));
            Assert.That(save.Stored.Economy.BoosterCount, Is.Zero);
            Assert.That(save.Stored.Economy.HeartCount,
                Is.EqualTo(HeartStatePolicy.MaximumHearts - 1));
            Assert.That(save.SaveCount, Is.EqualTo(savesBeforeStart + 1));
        }

        [UnityTest]
        public IEnumerator LastHeart_ClearRefundsAndShowsFirstClearRewards()
        {
            var save = new StockedCampaignSaveService(heartCount: 1);
            yield return LoadFrontendThroughBoot(() => CreateGraph(save));
            RequireController().LobbyPlayButton.onClick.Invoke();
            yield return WaitForScene(CampaignPath);
            StageSceneController campaign =
                Object.FindFirstObjectByType<StageSceneController>();

            ClearCurrentStage(campaign);
            campaign.Tick(1.3f);

            Assert.That(save.Stored.Economy.HeartCount, Is.EqualTo(1));
            Assert.That(save.Stored.Economy.Coins, Is.EqualTo(300));
            Assert.That(campaign.ClearDetailsText.text,
                Does.Contain("STAGE 8 COMPLETE"));
            Assert.That(campaign.ClearDetailsText.text,
                Does.Contain("TIME"));
            campaign.HandleGameplayTap();
            campaign.Tick(2f);
            Assert.That(campaign.ResultSequenceView.VisibleRewardRowCount,
                Is.EqualTo(4));
            Assert.That(campaign.ReplayButton.gameObject.activeSelf, Is.False);
        }

        [UnityTest]
        public IEnumerator FailureExitConfirmation_DoesNotSpendASecondHeart()
        {
            var save = new StockedCampaignSaveService(heartCount: 2);
            yield return LoadFrontendThroughBoot(() => CreateGraph(save));
            RequireController().LobbyPlayButton.onClick.Invoke();
            yield return WaitForScene(CampaignPath);
            StageSceneController campaign =
                Object.FindFirstObjectByType<StageSceneController>();
            campaign.SelectStage(1);
            campaign.StartSelectedStage();
            campaign.Tick(3.1f);
            campaign.Session.Advance(1f, 0f);
            StageGateView gate = FindCurrentGate(campaign);
            Assert.That(gate, Is.Not.Null);
            while (campaign.Session.CurrentColor == gate.AssignedColor)
            {
                campaign.HandleGameplayTap();
            }

            Assert.That(gate.TryResolveCrossing(), Is.True);
            Assert.That(campaign.Session.FlowState,
                Is.EqualTo(StageFlowState.Failed));
            campaign.Tick(campaign.FailurePanelDelaySeconds + 0.1f);
            Assert.That(save.Stored.Economy.HeartCount, Is.EqualTo(1));

            campaign.RequestFailureExit();
            campaign.ConfirmFailureExit();

            Assert.That(campaign.ResultPage,
                Is.EqualTo(CampaignResultPage.FailureFinalChoice));
            Assert.That(save.Stored.Economy.HeartCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator FrontendReentry_DoesNotDuplicatePersistentOrSceneObjects()
        {
            yield return LoadFrontendThroughBoot();
            RequireController().TitleStartButton.onClick.Invoke();
            yield return null;
            yield return SceneManager.LoadSceneAsync(
                FrontendPath,
                LoadSceneMode.Single);
            yield return null;

            FrontendSceneController controller = RequireController();
            Assert.That(controller.Router.CurrentPage,
                Is.EqualTo(FrontendPage.Lobby));
            Assert.That(controller.ActivePrimaryPageCount(), Is.EqualTo(1));
            Assert.That(
                Object.FindObjectsByType<AppRoot>().Length,
                Is.EqualTo(1));
            Assert.That(
                Object.FindObjectsByType<EventSystem>().Length,
                Is.EqualTo(1));

            for (int index = 0; index < 5; index++)
            {
                controller.LobbySettingsButton.onClick.Invoke();
                controller.HandleBack();
            }
            Assert.That(controller.Router.CurrentPage,
                Is.EqualTo(FrontendPage.Lobby));
            Assert.That(controller.ActivePrimaryPageCount(), Is.EqualTo(1));
        }

        private static IEnumerator LoadFrontendThroughBoot(
            Func<AppServiceGraph> graphFactory = null)
        {
            AppRoot.SetTestGraphFactory(graphFactory ?? CreateGraph);
            yield return SceneManager.LoadSceneAsync(
                BootPath,
                LoadSceneMode.Single);
            yield return WaitForScene(FrontendPath);
            yield return null;
        }

        private static FrontendSceneController RequireController()
        {
            FrontendSceneController controller =
                Object.FindFirstObjectByType<FrontendSceneController>();
            Assert.That(controller, Is.Not.Null);
            return controller;
        }

        private static void SetAllToggles(
            SettingsPanelController panel,
            bool value)
        {
            panel.NotificationToggle.isOn = value;
            panel.MusicToggle.isOn = value;
            panel.SfxToggle.isOn = value;
            panel.VibrationToggle.isOn = value;
        }

        private static void AssertTogglePresentation(
            SettingsPanelController panel,
            bool notifications,
            bool music,
            bool sfx,
            bool vibration)
        {
            AssertTogglePresentation(
                panel.NotificationToggle,
                panel.NotificationStateText,
                notifications);
            AssertTogglePresentation(
                panel.MusicToggle,
                panel.MusicStateText,
                music);
            AssertTogglePresentation(
                panel.SfxToggle,
                panel.SfxStateText,
                sfx);
            AssertTogglePresentation(
                panel.VibrationToggle,
                panel.VibrationStateText,
                vibration);
        }

        private static void AssertTogglePresentation(
            Toggle toggle,
            Text stateText,
            bool expected)
        {
            Assert.That(toggle.isOn, Is.EqualTo(expected));
            Assert.That(stateText.text, Is.EqualTo(expected ? "ON" : "OFF"));
            Assert.That(toggle.transform.Find("StateLabel"),
                Is.SameAs(stateText.transform));
            Assert.That(toggle.transform.Find("OnLabel"), Is.Null);
            Assert.That(toggle.transform.Find("OffLabel"), Is.Null);
            int stateLabelCount = 0;
            Text[] labels = toggle.GetComponentsInChildren<Text>(true);
            for (int index = 0; index < labels.Length; index++)
            {
                if (labels[index].name == "StateLabel")
                {
                    stateLabelCount++;
                }
            }
            Assert.That(stateLabelCount, Is.EqualTo(1));
        }

        private static IEnumerator WaitForScene(string path)
        {
            float timeout = Time.realtimeSinceStartup + 5f;
            while (SceneManager.GetActiveScene().path != path &&
                Time.realtimeSinceStartup < timeout)
            {
                yield return null;
            }
            Assert.That(SceneManager.GetActiveScene().path, Is.EqualTo(path));
        }

        private static void ClearCurrentStage(StageSceneController controller)
        {
            controller.StartSelectedStage();
            controller.Tick(3.1f);
            controller.Session.Advance(1f, 0f);
            while (controller.Session.RemainingGates > 0)
            {
                StageGateView gate = FindCurrentGate(controller);
                Assert.That(gate, Is.Not.Null);
                while (controller.Session.CurrentColor != gate.AssignedColor)
                {
                    controller.HandleGameplayTap();
                }
                Assert.That(gate.TryResolveCrossing(), Is.True);
            }
            float remaining = controller.GoalPathDistance -
                controller.CampaignDistance + 1f;
            controller.TickMovement(
                remaining / controller.Session.CurrentSpeed);
            Assert.That(controller.Session.FlowState,
                Is.EqualTo(StageFlowState.StageCleared));
        }

        private static StageGateView FindCurrentGate(
            StageSceneController controller)
        {
            for (int index = 0; index < controller.GatePoolSize; index++)
            {
                StageGateView gate = controller.GetGate(index);
                if (gate.gameObject.activeSelf && !gate.HasResolved &&
                    gate.PlanIndex == controller.Session.GatesPassed)
                {
                    return gate;
                }
            }
            return null;
        }

        private static IEnumerator DestroyAllAppRoots()
        {
            AppRoot[] roots = Object.FindObjectsByType<AppRoot>(
                FindObjectsInactive.Include);
            for (int index = 0; index < roots.Length; index++)
            {
                Object.Destroy(roots[index].gameObject);
            }
            yield return null;
        }

        private void RestoreCampaignProgress()
        {
            _campaignProgressSnapshot?.Dispose();
            _campaignProgressSnapshot = null;
        }

        private static AppServiceGraph CreateGraph()
        {
            return CreateGraph(new ExistingGuestSaveService());
        }

        private static AppServiceGraph CreateGraph(ILocalSaveService save)
        {
            var clock = new FixedClock();
            var profile = new ProfileService(clock, new FixedIdGenerator());
            var account = new LocalAccountService();
            var settings = new SettingsService();
            return new AppServiceGraph(
                clock,
                profile,
                account,
                settings,
                save,
                new AppInitializationPipeline(
                    save,
                    profile,
                    settings,
                    account),
                true);
        }

        private sealed class FixedClock : IClockService
        {
            public DateTime UtcNow =>
                new DateTime(2026, 8, 2, 0, 0, 0, DateTimeKind.Utc);
        }

        private sealed class FixedIdGenerator : IProfileIdGenerator
        {
            public string CreateProfileId() => "frontend-playmode-guest";
        }

        private sealed class ExistingGuestSaveService : ILocalSaveService
        {
            private readonly bool _accountChoiceCompleted;

            public ExistingGuestSaveService(
                bool accountChoiceCompleted = false)
            {
                _accountChoiceCompleted = accountChoiceCompleted;
            }

            public LocalSaveLoadResult Load()
            {
                const string utc = "2026-08-02T00:00:00.0000000Z";
                return LocalSaveLoadResult.Success(
                    new LocalSaveData
                    {
                        SchemaVersion = LocalSaveData.CurrentSchemaVersion,
                        SaveRevision = 1,
                        LastWriteUtc = utc,
                        Settings = LocalSettingsData.CreateDefaults(),
                        Profile = new LocalProfileData
                        {
                            ProfileId = "frontend-playmode-guest",
                            CreatedUtc = utc,
                            LastPlayedUtc = utc,
                            DisplayName = "GUEST",
                            AccountState = LocalAccountState.Guest,
                            AccountChoiceCompleted = _accountChoiceCompleted,
                            SaveRevision = 1
                        }
                    },
                    false,
                    false);
            }

            public LocalSaveWriteResult Save(LocalSaveData data) =>
                LocalSaveWriteResult.Success(
                    SaveReplacementResult.Recoverable);
        }

        private sealed class ArmableSaveFailureService : ILocalSaveService
        {
            private bool _failWrites;

            public LocalSaveLoadResult Load() =>
                new ExistingGuestSaveService().Load();

            public void ArmFailure() => _failWrites = true;

            public LocalSaveWriteResult Save(LocalSaveData data)
            {
                if (_failWrites)
                {
                    return LocalSaveWriteResult.Failure(
                        new ProductError(
                            ProductErrorCode.SaveWrite,
                            "PLANNED ACCOUNT SAVE FAILURE",
                            true));
                }

                data.SaveRevision++;
                data.Profile.SaveRevision = data.SaveRevision;
                return LocalSaveWriteResult.Success(
                    SaveReplacementResult.Recoverable);
            }
        }

        private sealed class ProgressedGuestSaveService : ILocalSaveService
        {
            public LocalSaveLoadResult Load()
            {
                LocalSaveData data =
                    new ExistingGuestSaveService(true).Load().Data;
                data.CampaignProgress.LegacyMigrationCompleted = true;
                data.CampaignProgress.HighestUnlockedStageId = "stage-05";
                data.Economy.Coins = 950;
                data.Economy.ShieldCount = 2;
                data.Economy.BoosterCount = 1;
                data.LobbyProgress.AppliedMilestoneCount = 2;
                data.LobbyProgress.PresentedMilestoneCount = 0;
                return LocalSaveLoadResult.Success(data, false, false);
            }

            public LocalSaveWriteResult Save(LocalSaveData data) =>
                LocalSaveWriteResult.Success(
                    SaveReplacementResult.Recoverable);
        }

        private sealed class RecordingSceneLoader : ISceneTransitionLoader
        {
            private readonly SceneTransitionResult _result;

            public RecordingSceneLoader(SceneTransitionResult result)
            {
                _result = result;
            }

            public int LoadCount { get; private set; }

            public void LoadScene(
                string scenePath,
                Action<SceneTransitionResult> completed)
            {
                LoadCount++;
                completed(_result);
            }
        }

        private sealed class DeferredSceneLoader : ISceneTransitionLoader
        {
            private Action<SceneTransitionResult> _completed;

            public int LoadCount { get; private set; }
            public string LastPath { get; private set; }

            public void LoadScene(
                string scenePath,
                Action<SceneTransitionResult> completed)
            {
                LoadCount++;
                LastPath = scenePath;
                _completed = completed;
            }

            public void Complete(SceneTransitionResult result)
            {
                Action<SceneTransitionResult> completed = _completed;
                _completed = null;
                completed?.Invoke(result);
            }
        }

        private sealed class RoundtripSaveService : ILocalSaveService
        {
            private LocalSaveData _data;

            public RoundtripSaveService()
            {
                _data = new ExistingGuestSaveService(true).Load().Data.Clone();
            }

            public LocalSaveLoadResult Load() =>
                LocalSaveLoadResult.Success(
                    _data.Clone(),
                    false,
                    false);

            public LocalSaveWriteResult Save(LocalSaveData data)
            {
                _data = data.Clone();
                return LocalSaveWriteResult.Success(
                    SaveReplacementResult.Recoverable);
            }
        }

        private sealed class StockedCampaignSaveService : ILocalSaveService
        {
            private LocalSaveData _data;

            internal StockedCampaignSaveService(
                int heartCount = HeartStatePolicy.MaximumHearts)
            {
                _data = new ExistingGuestSaveService(true).Load().Data.Clone();
                _data.CampaignProgress.LegacyMigrationCompleted = true;
                _data.CampaignProgress.HighestUnlockedStageId = "stage-08";
                for (int stage = 1; stage <= 7; stage++)
                {
                    _data.CampaignProgress.StageRecords.Add(
                        new LocalStageProgressData
                        {
                            StageId = $"stage-{stage:00}",
                            Cleared = true,
                            BestTime = 10f,
                            BestNoItemTime = 10f,
                            ClearCount = 1
                        });
                }
                _data.Economy.ShieldCount = 2;
                _data.Economy.BoosterCount = 1;
                _data.Economy.HeartCount = heartCount;
            }

            internal LocalSaveData Stored => _data;
            internal int SaveCount { get; private set; }

            public LocalSaveLoadResult Load() =>
                LocalSaveLoadResult.Success(_data.Clone(), false, false);

            public LocalSaveWriteResult Save(LocalSaveData data)
            {
                SaveCount++;
                data.SaveRevision++;
                data.Profile.SaveRevision = data.SaveRevision;
                _data = data.Clone();
                return LocalSaveWriteResult.Success(
                    SaveReplacementResult.Recoverable);
            }
        }

        private sealed class RecordingExitHandler : IFrontendExitHandler
        {
            public int RequestCount { get; private set; }

            public void RequestExit()
            {
                RequestCount++;
            }
        }
    }
}
