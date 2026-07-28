using System.Collections;
using ColorGateRunner.Core;
using ColorGateRunner.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ColorGateRunner.Tests.PlayMode
{
    public sealed class GrayboxScenePlayModeTests
    {
        private const string SceneName = "SampleScene";
        private const int ExpectedGatePoolSize = 6;

        private StageSceneController _controller;
        private InMemoryStageProgressStore _store;

        [UnitySetUp]
        public IEnumerator LoadStageScene()
        {
            SceneManager.LoadScene(SceneName, LoadSceneMode.Single);
            yield return null;

            _controller = Object.FindAnyObjectByType<StageSceneController>();
            Assert.That(_controller, Is.Not.Null);
            _store = new InMemoryStageProgressStore();
            _controller.SetProgressStoreForTests(_store);
            _controller.ShowStageSelect();
        }

        [Test]
        public void StageSelect_ShowsFiveEntries()
        {
            Assert.That(_controller.StageSelectPanel.activeSelf, Is.True);
            for (int index = 0; index < StageCatalog.Count; index++)
            {
                Assert.That(_controller.GetStageButton(index), Is.Not.Null);
            }
        }

        [Test]
        public void LockedStage_CannotStart()
        {
            _controller.SelectStage(2);

            Assert.That(_controller.Session, Is.Null);
            Assert.That(_controller.StageSelectPanel.activeSelf, Is.True);
        }

        [Test]
        public void SelectedStage_OpensItemScreen()
        {
            _controller.SelectStage(1);

            Assert.That(_controller.ItemPanel.activeSelf, Is.True);
            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.PreRunSelection));
        }

        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public void FourItemCombinations_StartCountdown(bool shield, bool booster)
        {
            SelectItemsAndStart(shield, booster);

            Assert.That(_controller.Session.Items.Shield, Is.EqualTo(shield));
            Assert.That(_controller.Session.Items.Booster, Is.EqualTo(booster));
            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.Countdown));
        }

        [Test]
        public void Start_CreatesExactlyOneCountdown()
        {
            SelectItemsAndStart(false, false);

            Assert.That(CountNamed("CountdownPanel"), Is.EqualTo(1));
            Assert.That(_controller.CountdownPanel.activeSelf, Is.True);
        }

        [Test]
        public void ShieldSelected_ActivatesAfterGo()
        {
            StartPlaying(true, false);

            Assert.That(_controller.Session.ShieldActive, Is.True);
            Assert.That(_controller.ShieldVisual.activeSelf, Is.True);
        }

        [Test]
        public void ShieldUnselected_RemainsInactive()
        {
            StartPlaying(false, false);

            Assert.That(_controller.Session.ShieldActive, Is.False);
            Assert.That(_controller.ShieldVisual.activeSelf, Is.False);
        }

        [Test]
        public void BoosterSelected_ActivatesAfterGo()
        {
            StartPlaying(false, true);

            Assert.That(_controller.Session.BoosterActive, Is.True);
            Assert.That(_controller.SpeedLines.isPlaying, Is.True);
            Assert.That(_controller.PlayerTrail.emitting, Is.True);
        }

        [Test]
        public void Booster_BypassesMismatchedGate()
        {
            StartPlaying(false, true);
            StageGateView gate = _controller.GetGate(0);
            MismatchFrom(gate.AssignedColor);

            Assert.That(gate.TryResolveCrossing(), Is.True);
            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.Playing));
            Assert.That(_controller.Session.GatesPassed, Is.EqualTo(1));
        }

        [Test]
        public void Booster_DoesNotConsumeShield()
        {
            StartPlaying(true, true);
            StageGateView gate = _controller.GetGate(0);
            MismatchFrom(gate.AssignedColor);
            gate.TryResolveCrossing();

            Assert.That(_controller.Session.ShieldActive, Is.True);
        }

        [Test]
        public void Booster_CameraEffectsReturnToNormal()
        {
            StartPlaying(false, true);
            Assert.That(_controller.GameplayCamera.fieldOfView,
                Is.GreaterThan(60f));

            _controller.Session.Advance(
                1f,
                _controller.Session.Stage.BoosterDistance + 1f);
            Assert.That(_controller.Session.BoosterExitActive, Is.True);
            _controller.Session.Advance(StageSession.BoosterExitDuration, 0f);
            _controller.Tick(0f);

            Assert.That(_controller.Session.BoosterActive, Is.False);
            Assert.That(_controller.GameplayCamera.fieldOfView,
                Is.EqualTo(60f).Within(0.01f));
            Assert.That(_controller.PlayerTrail.emitting, Is.False);
        }

        [Test]
        public void GateReaction_ResetsWhenRecycled()
        {
            StartPlaying(false, false);
            StageGateView gate = MatchAndResolve(_controller.GetGate(0));

            Assert.That(gate.HasResolved, Is.False);
            Assert.That(gate.ReactionActive, Is.False);
            Assert.That(gate.gameObject.activeSelf, Is.True);
        }

        [Test]
        public void FinalGate_RevealsGoal()
        {
            StartPlaying(false, false);
            ResolveRemainingStage();

            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.StageFinishing));
            Assert.That(_controller.Goal.activeSelf, Is.True);
        }

        [Test]
        public void GoalCrossing_ClearsStage()
        {
            StartPlaying(false, false);
            ResolveRemainingStage();
            PlacePlayerBeforeGoal();
            _controller.TickMovement(2f);

            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.StageCleared));
            Assert.That(_controller.ClearPanel.activeSelf, Is.True);
        }

        [Test]
        public void NoFailureAfterGoalBecomesAvailable()
        {
            StartPlaying(false, false);
            ResolveRemainingStage();

            Assert.That(
                _controller.Session.ResolveGate(RunnerColor.Green),
                Is.EqualTo(GateOutcome.Ignored));
            Assert.That(_controller.FailPanel.activeSelf, Is.False);
        }

        [Test]
        public void Clear_ShowsOneResultPanel()
        {
            ClearStage();

            Assert.That(CountNamed("StageClearPanel"), Is.EqualTo(1));
            Assert.That(_controller.ClearPanel.activeSelf, Is.True);
            Assert.That(_controller.FailPanel.activeSelf, Is.False);
        }

        [Test]
        public void NextStage_OpensNextItemSelection()
        {
            ClearStage();
            _controller.ShowStageSelect();
            _controller.SelectStage(2);

            Assert.That(_controller.SelectedStageNumber, Is.EqualTo(2));
            Assert.That(_controller.ItemPanel.activeSelf, Is.True);
        }

        [Test]
        public void Failure_RetryReturnsToItemSelection()
        {
            StartPlaying(false, false);
            FailAtFirstGate();

            _controller.RetryToItemSelection();

            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.PreRunSelection));
            Assert.That(_controller.ItemPanel.activeSelf, Is.True);
            Assert.That(_controller.CountdownPanel.activeSelf, Is.False);
        }

        [Test]
        public void Retry_RetainsItemSelection()
        {
            StartPlaying(true, true);
            _controller.Session.Advance(
                1f,
                _controller.Session.Stage.BoosterDistance + 1f);
            FailAtFirstGate();

            _controller.RetryToItemSelection();

            Assert.That(_controller.ShieldSelected, Is.True);
            Assert.That(_controller.BoosterSelected, Is.True);
            Assert.That(_controller.Session.Items.Shield, Is.True);
            Assert.That(_controller.Session.Items.Booster, Is.True);
        }

        [Test]
        public void RuntimeShieldPickup_IsNotExposed()
        {
            Assert.That(
                Object.FindObjectsByType<ShieldPickupView>(
                    FindObjectsInactive.Include).Length,
                Is.Zero);
        }

        [Test]
        public void StageOneToThree_DoNotUnlockGreenByProgress()
        {
            for (int stageNumber = 1; stageNumber <= 3; stageNumber++)
            {
                DeterministicStageGateSequence sequence =
                    new DeterministicStageGateSequence(
                        StageCatalog.GetByDisplayNumber(stageNumber));
                for (int gate = 0;
                    gate < StageCatalog.GetByDisplayNumber(stageNumber)
                        .TargetGateCount;
                    gate++)
                {
                    Assert.That(
                        sequence.GetPlan(gate).Color,
                        Is.Not.EqualTo(RunnerColor.Green));
                }
            }
        }

        [Test]
        public void StageFour_IntroducesGreen()
        {
            DeterministicStageGateSequence sequence =
                new DeterministicStageGateSequence(
                    StageCatalog.GetByDisplayNumber(4));
            bool foundGreen = false;
            for (int gate = 0;
                gate < StageCatalog.GetByDisplayNumber(4).TargetGateCount;
                gate++)
            {
                foundGreen |= sequence.GetPlan(gate).Color == RunnerColor.Green;
            }

            Assert.That(foundGreen, Is.True);
        }

        [Test]
        public void GatePool_ObjectCountDoesNotGrow()
        {
            StartPlaying(false, false);
            int before = CountStageGates();
            for (int index = 0; index < 12; index++)
            {
                MatchAndResolve(_controller.GetGate(index % ExpectedGatePoolSize));
            }

            Assert.That(CountStageGates(), Is.EqualTo(before));
            Assert.That(before, Is.EqualTo(ExpectedGatePoolSize));
        }

        [Test]
        public void TrackPool_ObjectCountDoesNotGrow()
        {
            StartPlaying(false, true);
            int before = Object.FindObjectsByType<TrackSegmentView>(
                FindObjectsInactive.Include).Length;
            _controller.TickMovement(3f);
            int after = Object.FindObjectsByType<TrackSegmentView>(
                FindObjectsInactive.Include).Length;

            Assert.That(after, Is.EqualTo(before));
        }

        [Test]
        public void CameraRotation_RemainsFixedDuringBoosterAndFailure()
        {
            StartPlaying(false, true);
            Quaternion rotation = _controller.GameplayCamera.transform.rotation;
            _controller.TickMovement(0.5f);
            _controller.Session.Advance(
                1f,
                _controller.Session.Stage.BoosterDistance + 1f);
            FailAtFirstGate();
            _controller.Tick(0.3f);

            Assert.That(
                Quaternion.Angle(
                    rotation,
                    _controller.GameplayCamera.transform.rotation),
                Is.LessThan(0.01f));
        }

        [Test]
        public void Failure_StopsMovementImmediately()
        {
            StartPlaying(false, false);
            FailAtFirstGate();
            Vector3 position = _controller.PlayerTransform.position;

            _controller.TickMovement(1f);

            Assert.That(_controller.PlayerTransform.position, Is.EqualTo(position));
        }

        [Test]
        public void StageClear_PersistsPerStageRecordAndUnlock()
        {
            ClearStage();

            Assert.That(_store.LoadRecord(1).Cleared, Is.True);
            Assert.That(_store.LoadRecord(1).ClearCount, Is.EqualTo(1));
            Assert.That(_store.LoadHighestUnlocked(), Is.EqualTo(2));
        }

        [Test]
        public void ItemButtons_AreNotRuntimeConsumables()
        {
            _controller.SelectStage(1);
            _controller.ToggleShieldSelection();
            _controller.ToggleShieldSelection();
            _controller.ToggleShieldSelection();

            Assert.That(_controller.ShieldSelected, Is.True);
        }

        [Test]
        public void StageSelect_DeveloperUnlockIsHiddenByDefault()
        {
            GameObject button = GameObject.Find("DeveloperUnlockAllButton");
            Assert.That(button, Is.Null);
            Assert.That(CountNamed("DeveloperUnlockAllButton"), Is.EqualTo(1));
        }

        [Test]
        public void RequiredSerializedReferences_AreAssigned()
        {
            Assert.That(_controller.HasRequiredReferences(), Is.True);
        }

        [Test]
        public void Scene_HasNoMissingMonoBehaviours()
        {
            Transform[] transforms =
                Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);
            for (int index = 0; index < transforms.Length; index++)
            {
                Component[] components =
                    transforms[index].GetComponents<Component>();
                for (int component = 0;
                    component < components.Length;
                    component++)
                {
                    Assert.That(
                        components[component],
                        Is.Not.Null,
                        $"Missing script on {transforms[index].name}");
                }
            }
        }

        [Test]
        public void GeneratedUi_HasNoDuplicateRoots()
        {
            Assert.That(CountNamed("Canvas"), Is.EqualTo(1));
            Assert.That(CountNamed("StageSelectPanel"), Is.EqualTo(1));
            Assert.That(CountNamed("PreRunItemPanel"), Is.EqualTo(1));
            Assert.That(CountNamed("StageClearPanel"), Is.EqualTo(1));
            Assert.That(CountNamed("StageFailedPanel"), Is.EqualTo(1));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void EveryStage_AllItemCombinationsCanInitialize(int stageNumber)
        {
            _store.HighestUnlocked = 5;
            _controller.SetProgressStoreForTests(_store);
            for (int mask = 0; mask < 4; mask++)
            {
                _controller.ShowStageSelect();
                _controller.SelectStage(stageNumber);
                if ((mask & 1) != 0)
                {
                    _controller.ToggleShieldSelection();
                }
                if ((mask & 2) != 0)
                {
                    _controller.ToggleBoosterSelection();
                }
                _controller.StartSelectedStage();
                _controller.Tick(3.1f);

                Assert.That(_controller.Session.FlowState,
                    Is.EqualTo(StageFlowState.Playing));
                Assert.That(_controller.GatePoolSize,
                    Is.EqualTo(ExpectedGatePoolSize));
            }
        }

        [Test]
        public void Restart_ReplaysIdenticalGateLayout()
        {
            StartPlaying(true, true);
            RunnerColor[] colors = CaptureColors();
            float[] positions = CapturePositions();
            _controller.RetryToItemSelection();
            _controller.StartSelectedStage();

            Assert.That(CaptureColors(), Is.EqualTo(colors));
            Assert.That(CapturePositions(), Is.EqualTo(positions));
        }

        [Test]
        public void FailurePanel_ShowsProgressAndItems()
        {
            StartPlaying(true, false);
            _controller.Session.Advance(1.1f, 0f);
            StageGateView first = _controller.GetGate(0);
            MismatchFrom(first.AssignedColor);
            first.TryResolveCrossing();
            if (_controller.Session.FlowState == StageFlowState.ShieldRecovery)
            {
                _controller.Session.Advance(1.1f, 0f);
                StageGateView second = _controller.GetGate(1);
                MismatchFrom(second.AssignedColor);
                second.TryResolveCrossing();
            }

            Assert.That(_controller.FailDetailsText.text, Does.Contain("PROGRESS"));
            Assert.That(_controller.FailDetailsText.text, Does.Contain("SHIELD"));
        }

        private void SelectItemsAndStart(bool shield, bool booster)
        {
            _controller.SelectStage(1);
            if (shield)
            {
                _controller.ToggleShieldSelection();
            }
            if (booster)
            {
                _controller.ToggleBoosterSelection();
            }
            _controller.StartSelectedStage();
        }

        private void StartPlaying(bool shield, bool booster)
        {
            SelectItemsAndStart(shield, booster);
            _controller.Tick(3.1f);
            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.Playing));
        }

        private StageGateView MatchAndResolve(StageGateView gate)
        {
            MatchColor(gate.AssignedColor);
            Assert.That(gate.TryResolveCrossing(), Is.True);
            return gate;
        }

        private void MatchColor(RunnerColor color)
        {
            for (int index = 0;
                index < 3 && _controller.Session.CurrentColor != color;
                index++)
            {
                _controller.HandleGameplayTap();
            }
            Assert.That(_controller.Session.CurrentColor, Is.EqualTo(color));
        }

        private void MismatchFrom(RunnerColor color)
        {
            if (_controller.Session.CurrentColor == color)
            {
                _controller.HandleGameplayTap();
            }
            Assert.That(_controller.Session.CurrentColor, Is.Not.EqualTo(color));
        }

        private void FailAtFirstGate()
        {
            StageGateView gate = _controller.GetGate(0);
            MismatchFrom(gate.AssignedColor);
            gate.TryResolveCrossing();
            if (_controller.Session.FlowState == StageFlowState.ShieldRecovery)
            {
                _controller.Session.Advance(1.1f, 0f);
                gate = _controller.GetGate(1);
                MismatchFrom(gate.AssignedColor);
                gate.TryResolveCrossing();
            }
            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.Failed));
        }

        private void ResolveRemainingStage()
        {
            int poolIndex = 0;
            while (_controller.Session.RemainingGates > 0)
            {
                MatchAndResolve(
                    _controller.GetGate(poolIndex % ExpectedGatePoolSize));
                poolIndex++;
            }
        }

        private void ClearStage()
        {
            StartPlaying(false, false);
            _controller.Session.Advance(1f, 0f);
            ResolveRemainingStage();
            PlacePlayerBeforeGoal();
            _controller.TickMovement(2f);
            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.StageCleared));
        }

        private void PlacePlayerBeforeGoal()
        {
            Vector3 position = _controller.PlayerTransform.position;
            position.z = _controller.Goal.transform.position.z - 1f;
            _controller.PlayerTransform.position = position;
        }

        private RunnerColor[] CaptureColors()
        {
            RunnerColor[] values = new RunnerColor[ExpectedGatePoolSize];
            for (int index = 0; index < values.Length; index++)
            {
                values[index] = _controller.GetGate(index).AssignedColor;
            }
            return values;
        }

        private float[] CapturePositions()
        {
            float[] values = new float[ExpectedGatePoolSize];
            for (int index = 0; index < values.Length; index++)
            {
                values[index] = _controller.GetGate(index).transform.position.z;
            }
            return values;
        }

        private static int CountStageGates()
        {
            return Object.FindObjectsByType<StageGateView>(
                FindObjectsInactive.Include).Length;
        }

        private static int CountNamed(string objectName)
        {
            int count = 0;
            Transform[] transforms =
                Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);
            for (int index = 0; index < transforms.Length; index++)
            {
                if (transforms[index].name == objectName)
                {
                    count++;
                }
            }
            return count;
        }

        private sealed class InMemoryStageProgressStore : IStageProgressStore
        {
            private readonly StageRecord[] _records =
                new StageRecord[StageCatalog.Count];

            internal int HighestUnlocked { get; set; } = 1;

            public int LoadHighestUnlocked()
            {
                return HighestUnlocked;
            }

            public StageRecord LoadRecord(int stageNumber)
            {
                return _records[stageNumber - 1];
            }

            public void SaveHighestUnlocked(int stageNumber)
            {
                HighestUnlocked = stageNumber;
            }

            public void SaveRecord(int stageNumber, StageRecord record)
            {
                _records[stageNumber - 1] = record;
            }
        }
    }
}
