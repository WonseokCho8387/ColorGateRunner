using System;
using System.Collections;
using ColorGateRunner.Core;
using ColorGateRunner.Presentation;
using ColorGateRunner.Product;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ColorGateRunner.Tests.PlayMode
{
    public sealed class CampaignProgressRestorePlayModeTests
    {
        private const string BootPath = "Assets/Scenes/Boot.unity";
        private const string FrontendPath = "Assets/Scenes/Frontend.unity";
        private const string CampaignPath = "Assets/Scenes/SampleScene.unity";

        private CampaignPlayerPrefsSnapshot _editorProgressSnapshot;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _editorProgressSnapshot =
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
                    RestoreEditorProgress();
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
                RestoreEditorProgress();
            }
        }

        [UnityTest]
        public IEnumerator ExistingStageElevenProgress_RestoresSameStableStage()
        {
            SeedStageElevenProgress();
            StageSceneController first = null;
            yield return LoadCampaign(controller => first = controller);
            string expectedStageId =
                StageCatalog.GetByDisplayNumber(11).StageId;

            AssertLobbyStage(first, 11);
            first.PlayFromLobby();
            Assert.That(first.Session.FlowState,
                Is.EqualTo(StageFlowState.PreRunSelection));
            Assert.That(first.Session.Stage.StageId, Is.EqualTo(expectedStageId));

            first.StartSelectedStage();
            first.Tick(3.1f);
            Assert.That(first.Session.FlowState,
                Is.EqualTo(StageFlowState.Playing));
            Assert.That(first.Session.Stage.StageId, Is.EqualTo(expectedStageId));

            StageSceneController reentered = null;
            yield return LoadCampaign(controller => reentered = controller);
            AssertLobbyStage(reentered, 11);
            reentered.PlayFromLobby();
            Assert.That(reentered.Session.Stage.StageId,
                Is.EqualTo(expectedStageId));
        }

        [UnityTest]
        public IEnumerator FreshCampaign_ClearAndReinitialize_RestoresStageTwo()
        {
            ClearCampaignProgressForTest();
            StageSceneController first = null;
            yield return LoadCampaign(controller => first = controller);
            AssertLobbyStage(first, 1);

            ClearCurrentStage(first);
            Assert.That(
                PlayerPrefs.GetInt(
                    CampaignPlayerPrefsSnapshot.HighestUnlockedKey,
                    -1),
                Is.EqualTo(2));
            Assert.That(
                StageProgress.Parse(PlayerPrefs.GetString(
                    CampaignPlayerPrefsSnapshot.RecordKey(1),
                    string.Empty)).Cleared,
                Is.True);

            StageSceneController reinitialized = null;
            yield return LoadCampaign(controller => reinitialized = controller);
            AssertLobbyStage(reinitialized, 2);
            Assert.That(reinitialized.SelectedStageNumber, Is.Not.EqualTo(11));
            reinitialized.PlayFromLobby();
            Assert.That(reinitialized.Session.Stage.StageId,
                Is.EqualTo(StageCatalog.GetByDisplayNumber(2).StageId));
        }

        [UnityTest]
        public IEnumerator NewProductGuest_PreservesExistingCampaignProgress()
        {
            SeedStageElevenProgress();
            using CampaignPlayerPrefsSnapshot seededProgress =
                CampaignPlayerPrefsSnapshot.Capture();
            var save = new NewGuestSaveService();
            AppRoot.SetTestGraphFactory(() => CreateGraph(save));

            yield return SceneManager.LoadSceneAsync(
                BootPath,
                LoadSceneMode.Single);
            yield return WaitForScene(FrontendPath);

            AppRoot root = Object.FindFirstObjectByType<AppRoot>();
            Assert.That(root, Is.Not.Null);
            Assert.That(root.Graph.Profile.Current.ProfileId,
                Is.EqualTo("campaign-restore-new-guest"));
            Assert.That(seededProgress.MatchesCurrentState(), Is.True);

            StageSceneController first = null;
            yield return LoadCampaign(controller => first = controller);
            AssertLobbyStage(first, 11);
            Assert.That(seededProgress.MatchesCurrentState(), Is.True);

            StageSceneController reentered = null;
            yield return LoadCampaign(controller => reentered = controller);
            AssertLobbyStage(reentered, 11);
            Assert.That(seededProgress.MatchesCurrentState(), Is.True);
            Assert.That(root.Graph.Profile.Current.ProfileId,
                Is.EqualTo("campaign-restore-new-guest"));
        }

        [UnityTest]
        public IEnumerator MissingOrCorruptHighest_FallsBackWithoutDeletingRecords()
        {
            ClearCampaignProgressForTest();
            const string preservedRecord = "1|25|26|2|0";
            string recordKey = CampaignPlayerPrefsSnapshot.RecordKey(5);
            PlayerPrefs.SetString(recordKey, preservedRecord);
            PlayerPrefs.Save();

            StageSceneController missing = null;
            yield return LoadCampaign(controller => missing = controller);
            AssertLobbyStage(missing, 1);
            Assert.That(PlayerPrefs.GetString(recordKey, string.Empty),
                Is.EqualTo(preservedRecord));

            PlayerPrefs.SetString(
                CampaignPlayerPrefsSnapshot.HighestUnlockedKey,
                "not-an-integer");
            PlayerPrefs.Save();
            StageSceneController corrupt = null;
            yield return LoadCampaign(controller => corrupt = controller);
            AssertLobbyStage(corrupt, 1);
            Assert.That(PlayerPrefs.GetString(recordKey, string.Empty),
                Is.EqualTo(preservedRecord));
        }

        private static void SeedStageElevenProgress()
        {
            ClearCampaignProgressForTest();
            for (int stageNumber = 1; stageNumber <= 10; stageNumber++)
            {
                PlayerPrefs.SetString(
                    CampaignPlayerPrefsSnapshot.RecordKey(stageNumber),
                    StageProgress.Serialize(new StageRecord(
                        true,
                        30f + stageNumber,
                        0f,
                        1)));
            }
            PlayerPrefs.SetInt(
                CampaignPlayerPrefsSnapshot.HighestUnlockedKey,
                11);
            PlayerPrefs.Save();
        }

        private static void ClearCampaignProgressForTest()
        {
            PlayerPrefs.DeleteKey(
                CampaignPlayerPrefsSnapshot.HighestUnlockedKey);
            for (int stageNumber = 1;
                stageNumber <= CampaignPlayerPrefsSnapshot.StageCount;
                stageNumber++)
            {
                PlayerPrefs.DeleteKey(
                    CampaignPlayerPrefsSnapshot.RecordKey(stageNumber));
            }
            PlayerPrefs.Save();
        }

        private static IEnumerator LoadCampaign(
            Action<StageSceneController> loaded)
        {
            yield return SceneManager.LoadSceneAsync(
                CampaignPath,
                LoadSceneMode.Single);
            yield return null;
            StageSceneController controller =
                Object.FindFirstObjectByType<StageSceneController>();
            Assert.That(controller, Is.Not.Null);
            loaded(controller);
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

        private static void AssertLobbyStage(
            StageSceneController controller,
            int expectedDisplayNumber)
        {
            StageDefinition expected =
                StageCatalog.GetByDisplayNumber(expectedDisplayNumber);
            Assert.That(controller.SelectedStageNumber,
                Is.EqualTo(expectedDisplayNumber));
            Assert.That(controller.LobbyStageText.text,
                Does.Contain($"STAGE {expectedDisplayNumber}"));
            Assert.That(controller.LobbyStageTitleText.text,
                Is.EqualTo(expected.Title));
            Assert.That(controller.UiFlow, Is.EqualTo(MobileUiFlow.Lobby));
        }

        private static void ClearCurrentStage(StageSceneController controller)
        {
            controller.PlayFromLobby();
            controller.StartSelectedStage();
            controller.Tick(3.1f);
            Assert.That(controller.Session.FlowState,
                Is.EqualTo(StageFlowState.Playing));
            controller.Session.Advance(1f, 0f);

            while (controller.Session.RemainingGates > 0)
            {
                StageGateView gate = FindCurrentGate(controller);
                Assert.That(gate, Is.Not.Null);
                Match(controller, gate.AssignedColor);
                Assert.That(gate.TryResolveCrossing(), Is.True);
            }

            Vector3 position = controller.PlayerTransform.position;
            position.z = controller.Goal.transform.position.z - 1f;
            controller.PlayerTransform.position = position;
            controller.TickMovement(2f);
            Assert.That(controller.Session.FlowState,
                Is.EqualTo(StageFlowState.StageCleared));
        }

        private static StageGateView FindCurrentGate(
            StageSceneController controller)
        {
            for (int index = 0; index < controller.GatePoolSize; index++)
            {
                StageGateView gate = controller.GetGate(index);
                if (gate.gameObject.activeSelf &&
                    !gate.HasResolved &&
                    gate.PlanIndex == controller.Session.GatesPassed)
                {
                    return gate;
                }
            }
            return null;
        }

        private static void Match(
            StageSceneController controller,
            RunnerColor color)
        {
            for (int index = 0;
                index < 3 && controller.Session.CurrentColor != color;
                index++)
            {
                controller.HandleGameplayTap();
            }
            Assert.That(controller.Session.CurrentColor, Is.EqualTo(color));
        }

        private static AppServiceGraph CreateGraph(ILocalSaveService save)
        {
            var clock = new FixedClock();
            var profile = new ProfileService(clock, new FixedIdGenerator());
            var settings = new SettingsService();
            var account = new LocalAccountService();
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

        private void RestoreEditorProgress()
        {
            _editorProgressSnapshot?.Dispose();
            _editorProgressSnapshot = null;
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

        private sealed class FixedClock : IClockService
        {
            public DateTime UtcNow =>
                new DateTime(2026, 8, 2, 0, 0, 0, DateTimeKind.Utc);
        }

        private sealed class FixedIdGenerator : IProfileIdGenerator
        {
            public string CreateProfileId() =>
                "campaign-restore-new-guest";
        }

        private sealed class NewGuestSaveService : ILocalSaveService
        {
            private readonly LocalSaveData _data = LocalSaveData.CreateEmpty();

            public LocalSaveLoadResult Load()
            {
                return LocalSaveLoadResult.Success(_data, true, false);
            }

            public LocalSaveWriteResult Save(LocalSaveData data)
            {
                data.SaveRevision++;
                data.Profile.SaveRevision = data.SaveRevision;
                return LocalSaveWriteResult.Success(
                    SaveReplacementResult.Recoverable);
            }
        }
    }
}
