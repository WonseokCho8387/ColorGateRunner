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
        public IEnumerator BootToFrontend_StartsAtTitleWithOneAppRootAndEventSystem()
        {
            yield return LoadFrontendThroughBoot();

            FrontendSceneController controller = RequireController();
            Assert.That(controller.HasRequiredReferences(), Is.True);
            Assert.That(controller.Router.CurrentPage,
                Is.EqualTo(FrontendPage.Title));
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
        public IEnumerator TitleLobbyAndBack_KeepOnePrimaryPageAndHideEmptySlots()
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

            controller.HandleBack();
            yield return null;
            Assert.That(controller.Router.CurrentPage,
                Is.EqualTo(FrontendPage.Title));
            Assert.That(controller.ActivePrimaryPageCount(), Is.EqualTo(1));
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
                Is.EqualTo(FrontendPage.Title));

            controller.TitleStartButton.onClick.Invoke();
            controller.HandleBack();
            Assert.That(controller.Router.CurrentPage,
                Is.EqualTo(FrontendPage.Title));
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
                FrontendSceneLoadResult.Success());
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
                FrontendSceneLoadResult.Failure("PLANNED LOAD FAILURE"));
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
        }

        [UnityTest]
        public IEnumerator PlayCampaign_LoadsExistingLobbyAndPreservesProgress()
        {
            const string record = "1|12.34|11.11|4|2";
            PlayerPrefs.SetInt(UnlockKey, 8);
            PlayerPrefs.SetString(RecordKey, record);
            PlayerPrefs.Save();
            yield return LoadFrontendThroughBoot();
            FrontendSceneController controller = RequireController();

            controller.TitleStartButton.onClick.Invoke();
            controller.LobbyPlayButton.onClick.Invoke();
            yield return WaitForScene(CampaignPath);
            yield return null;

            StageSceneController campaign =
                Object.FindFirstObjectByType<StageSceneController>();
            Assert.That(campaign, Is.Not.Null);
            Assert.That(campaign.LobbyRoot.activeSelf, Is.True);
            Assert.That(campaign.UiFlow, Is.EqualTo(MobileUiFlow.Lobby));
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
            yield return SceneManager.LoadSceneAsync(
                FrontendPath,
                LoadSceneMode.Single);
            yield return null;

            FrontendSceneController controller = RequireController();
            Assert.That(controller.Router.CurrentPage,
                Is.EqualTo(FrontendPage.Title));
            Assert.That(controller.ActivePrimaryPageCount(), Is.EqualTo(1));
            Assert.That(
                Object.FindObjectsByType<AppRoot>().Length,
                Is.EqualTo(1));
            Assert.That(
                Object.FindObjectsByType<EventSystem>().Length,
                Is.EqualTo(1));

            for (int index = 0; index < 5; index++)
            {
                controller.TitleStartButton.onClick.Invoke();
                controller.LobbyBackButton.onClick.Invoke();
            }
            Assert.That(controller.Router.CurrentPage,
                Is.EqualTo(FrontendPage.Title));
            Assert.That(controller.ActivePrimaryPageCount(), Is.EqualTo(1));
        }

        private static IEnumerator LoadFrontendThroughBoot()
        {
            AppRoot.SetTestGraphFactory(CreateGraph);
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
            var clock = new FixedClock();
            var profile = new ProfileService(clock, new FixedIdGenerator());
            var account = new LocalAccountService();
            var settings = new SettingsService();
            var save = new ExistingGuestSaveService();
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

        private sealed class RecordingSceneLoader : IFrontendSceneLoader
        {
            private readonly FrontendSceneLoadResult _result;

            public RecordingSceneLoader(FrontendSceneLoadResult result)
            {
                _result = result;
            }

            public int LoadCount { get; private set; }

            public void LoadScene(
                string scenePath,
                Action<FrontendSceneLoadResult> completed)
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
