using System;
using System.Collections;
using System.Collections.Generic;
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
    public sealed class ProductBootPlayModeTests
    {
        private const string BootPath = "Assets/Scenes/Boot.unity";
        private const string FrontendPath = "Assets/Scenes/Frontend.unity";
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
        public IEnumerator BootScene_HasRequiredStructureAndOneEventSystem()
        {
            AppRoot.SetTestGraphFactory(() =>
                CreateGraph(new AlwaysFailSaveService()));

            yield return SceneManager.LoadSceneAsync(
                BootPath,
                LoadSceneMode.Single);
            yield return null;

            Assert.That(SceneManager.GetActiveScene().path, Is.EqualTo(BootPath));
            Assert.That(GameObject.Find("ColorGateRunner_Boot"), Is.Not.Null);
            Assert.That(GameObject.Find("AppRoot"), Is.Not.Null);
            Assert.That(
                Object.FindObjectsByType<AppRoot>().Length,
                Is.EqualTo(1));
            Assert.That(
                Object.FindObjectsByType<EventSystem>().Length,
                Is.EqualTo(1));
            Assert.That(
                Object.FindFirstObjectByType<SafeAreaLayout>(),
                Is.Not.Null);

            BootSceneController controller =
                Object.FindFirstObjectByType<BootSceneController>();
            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.DestinationScenePath,
                Is.EqualTo(FrontendPath));
            Assert.That(controller.DestinationScenePath,
                Is.Not.EqualTo(BootPath));
            Assert.That(controller.ErrorPanel.activeSelf, Is.True);
            Assert.That(controller.RetryButton, Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator SuccessfulBoot_LoadsProfileBeforeFrontendTransition()
        {
            var save = new FixedSaveService(CreateSavedProfile());
            AppRoot.SetTestGraphFactory(() => CreateGraph(save));

            yield return SceneManager.LoadSceneAsync(
                BootPath,
                LoadSceneMode.Single);
            yield return WaitForScene(FrontendPath);

            Assert.That(SceneManager.GetActiveScene().path,
                Is.EqualTo(FrontendPath));
            AppRoot root = Object.FindFirstObjectByType<AppRoot>();
            Assert.That(root, Is.Not.Null);
            Assert.That(root.Graph.Profile.Current.ProfileId,
                Is.EqualTo("existing-guest"));
            Assert.That(root.Graph.Account.CurrentProfileId,
                Is.EqualTo("existing-guest"));
            Assert.That(save.LoadCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Retry_ReusesGraphHidesErrorAndDoesNotDuplicateListener()
        {
            var save = new FailOnceThenLoadSaveService();
            int graphCount = 0;
            AppRoot.SetTestGraphFactory(() =>
            {
                graphCount++;
                return CreateGraph(save);
            });

            yield return SceneManager.LoadSceneAsync(
                BootPath,
                LoadSceneMode.Single);
            yield return null;
            BootSceneController controller =
                Object.FindFirstObjectByType<BootSceneController>();
            Assert.That(controller.ErrorPanel.activeSelf, Is.True);
            var loader = new RecordingLoader();
            controller.SetSceneLoaderForTests(loader);

            controller.RetryButton.onClick.Invoke();
            yield return null;

            Assert.That(loader.LoadCount, Is.EqualTo(1));
            Assert.That(loader.LastPath, Is.EqualTo(FrontendPath));
            Assert.That(controller.ErrorPanel.activeSelf, Is.False);
            Assert.That(graphCount, Is.EqualTo(1));
            Assert.That(
                Object.FindObjectsByType<AppRoot>().Length,
                Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator ReturningToBoot_DoesNotCreateSecondServiceGraph()
        {
            int graphCount = 0;
            AppRoot.SetTestGraphFactory(() =>
            {
                graphCount++;
                return CreateGraph(new AlwaysFailSaveService());
            });
            yield return SceneManager.LoadSceneAsync(BootPath);
            yield return null;
            AppRoot first = Object.FindFirstObjectByType<AppRoot>();

            yield return SceneManager.LoadSceneAsync(BootPath);
            yield return null;
            yield return null;

            AppRoot[] roots = Object.FindObjectsByType<AppRoot>();
            Assert.That(roots.Length, Is.EqualTo(1));
            Assert.That(roots[0], Is.SameAs(first));
            Assert.That(graphCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator BootAndGuestCreation_DoNotModifyStageProgressKeys()
        {
            const string record = "1|12.34|11.11|4|2";
            PlayerPrefs.SetInt("ColorGateRunner.Stage.HighestUnlocked", 8);
            PlayerPrefs.SetString("ColorGateRunner.Stage.Record.6", record);
            PlayerPrefs.Save();
            AppRoot.SetTestGraphFactory(() =>
                CreateGraph(new AlwaysFailSaveService()));

            yield return SceneManager.LoadSceneAsync(BootPath);
            yield return null;

            Assert.That(
                PlayerPrefs.GetInt(
                    "ColorGateRunner.Stage.HighestUnlocked",
                    -1),
                Is.EqualTo(8));
            Assert.That(
                PlayerPrefs.GetString(
                    "ColorGateRunner.Stage.Record.6",
                    string.Empty),
                Is.EqualTo(record));
        }

        private static AppServiceGraph CreateGraph(ILocalSaveService save)
        {
            var clock = new FixedClock();
            var profile = new ProfileService(clock, new FixedIdGenerator());
            var settings = new SettingsService();
            var account = new LocalAccountService();
            var initialization = new AppInitializationPipeline(
                save,
                profile,
                settings,
                account);
            return new AppServiceGraph(
                clock,
                profile,
                account,
                settings,
                save,
                initialization);
        }

        private static LocalSaveData CreateSavedProfile()
        {
            const string utc = "2026-08-01T00:00:00.0000000Z";
            return new LocalSaveData
            {
                SchemaVersion = LocalSaveData.CurrentSchemaVersion,
                SaveRevision = 2,
                LastWriteUtc = utc,
                Settings = LocalSettingsData.CreateDefaults(),
                Profile = new LocalProfileData
                {
                    ProfileId = "existing-guest",
                    CreatedUtc = utc,
                    LastPlayedUtc = utc,
                    DisplayName = "GUEST",
                    AccountState = LocalAccountState.Guest,
                    SaveRevision = 2
                }
            };
        }

        private static IEnumerator WaitForScene(string path)
        {
            float timeout = Time.realtimeSinceStartup + 5f;
            while (SceneManager.GetActiveScene().path != path &&
                Time.realtimeSinceStartup < timeout)
            {
                yield return null;
            }
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

        private sealed class FixedClock : IClockService
        {
            public DateTime UtcNow =>
                new DateTime(2026, 8, 1, 1, 0, 0, DateTimeKind.Utc);
        }

        private sealed class FixedIdGenerator : IProfileIdGenerator
        {
            public string CreateProfileId() => "playmode-guest";
        }

        private sealed class AlwaysFailSaveService : ILocalSaveService
        {
            public LocalSaveLoadResult Load() =>
                LocalSaveLoadResult.Failure(new ProductError(
                    ProductErrorCode.SaveRead,
                    "PLANNED LOCAL ERROR",
                    true));

            public LocalSaveWriteResult Save(LocalSaveData data) =>
                LocalSaveWriteResult.Failure(new ProductError(
                    ProductErrorCode.SaveWrite,
                    "PLANNED LOCAL ERROR",
                    true));
        }

        private sealed class FixedSaveService : ILocalSaveService
        {
            private readonly LocalSaveData _data;
            public FixedSaveService(LocalSaveData data) => _data = data;
            public int LoadCount { get; private set; }

            public LocalSaveLoadResult Load()
            {
                LoadCount++;
                return LocalSaveLoadResult.Success(
                    _data.Clone(),
                    false,
                    false);
            }

            public LocalSaveWriteResult Save(LocalSaveData data) =>
                LocalSaveWriteResult.Success(
                    SaveReplacementResult.Recoverable);
        }

        private sealed class FailOnceThenLoadSaveService : ILocalSaveService
        {
            private bool _failed;
            public LocalSaveLoadResult Load()
            {
                if (!_failed)
                {
                    _failed = true;
                    return LocalSaveLoadResult.Failure(new ProductError(
                        ProductErrorCode.SaveRead,
                        "PLANNED FIRST FAILURE",
                        true));
                }

                return LocalSaveLoadResult.Success(
                    LocalSaveData.CreateEmpty(),
                    true,
                    false);
            }

            public LocalSaveWriteResult Save(LocalSaveData data)
            {
                data.SaveRevision++;
                data.Profile.SaveRevision = data.SaveRevision;
                return LocalSaveWriteResult.Success(
                    SaveReplacementResult.Recoverable);
            }
        }

        private sealed class RecordingLoader : IBootSceneLoader
        {
            public int LoadCount { get; private set; }
            public string LastPath { get; private set; }

            public void LoadScene(string scenePath)
            {
                LoadCount++;
                LastPath = scenePath;
            }
        }
    }
}
