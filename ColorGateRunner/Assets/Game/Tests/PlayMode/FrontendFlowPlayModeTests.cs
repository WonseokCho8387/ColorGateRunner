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
            Assert.That(controller.CurrencySlotRoot.activeSelf, Is.False);
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
            yield return LoadFrontendThroughBoot(() =>
            {
                graph = CreateGraph(new FailSecondWriteSaveService());
                return graph;
            });

            FrontendSceneController controller = RequireController();
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
                panel.NotificationToggle.isOn = true;
                panel.MusicToggle.isOn = false;
                panel.SfxToggle.isOn = false;
                panel.VibrationToggle.isOn = false;
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
                    account));
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

        private sealed class FailSecondWriteSaveService : ILocalSaveService
        {
            private int _writeCount;

            public LocalSaveLoadResult Load() =>
                new ExistingGuestSaveService().Load();

            public LocalSaveWriteResult Save(LocalSaveData data)
            {
                _writeCount++;
                if (_writeCount >= 2)
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
