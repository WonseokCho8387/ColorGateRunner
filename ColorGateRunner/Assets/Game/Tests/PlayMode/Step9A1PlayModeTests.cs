using System.Collections;
using ColorGateRunner.Core;
using ColorGateRunner.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ColorGateRunner.Tests.PlayMode
{
    public sealed class Step9A1PlayModeTests
    {
        private StageSceneController _controller;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
            yield return null;
            _controller = Object.FindAnyObjectByType<StageSceneController>();
            Assert.That(_controller, Is.Not.Null);
            _controller.SetProgressStoreForTests(
                new InMemoryStageProgressStore());
            _controller.SetStartItemInventoryForTests(
                new StartItemInventoryTestGateway());
            _controller.SetContinueServicesForTests(
                new ContinueEconomyTestGateway(),
                new RewardedAdTestService());
        }

        [Test]
        public void BoosterExit_ActiveGatePositionsDoNotJump()
        {
            StartPlaying(booster: true);
            float[] before = CaptureGatePositions();
            int cursor = _controller.Session.SequenceCursor;

            EndBooster();

            Assert.That(CaptureGatePositions(), Is.EqualTo(before));
            Assert.That(_controller.Session.SequenceCursor,
                Is.EqualTo(cursor));
        }

        [Test]
        public void BoosterExit_DoesNotIntroduceEmptyGateInterval()
        {
            StartPlaying(booster: true);
            float before = MaximumAdjacentGateGap();

            EndBooster();

            Assert.That(MaximumAdjacentGateGap(),
                Is.EqualTo(before).Within(0.001f));
        }

        [Test]
        public void BoosterExit_FirstGateUsesCurrentPlayerColor()
        {
            StartPlaying(booster: true);
            EndBooster();

            StageGateView first = FindGateByPlanIndex(
                _controller.Session.GatesPassed);
            Assert.That(first, Is.Not.Null);
            Assert.That(first.AssignedColor,
                Is.EqualTo(_controller.Session.CurrentColor));
        }

        [Test]
        public void BoosterExit_OriginalPatternResumesAfterShortOverride()
        {
            StartPlaying(booster: true);
            GatePlan authoredThird = FindGateByPlanIndex(2).ActivePlan;

            EndBooster();

            GatePlan third = FindGateByPlanIndex(2).ActivePlan;
            Assert.That(third.Color, Is.EqualTo(authoredThird.Color));
            Assert.That(third.Pattern, Is.EqualTo(authoredThird.Pattern));
            Assert.That(third.HasTemporaryColorOverride, Is.False);
        }

        [Test]
        public void Continue_FreezesTrackDuringCleanRespawn()
        {
            StartPlaying();
            Fail();
            Vector3[] tracks = CaptureTrackPositions();

            _controller.RequestCoinContinue();

            Assert.That(CaptureTrackPositions(), Is.EqualTo(tracks));
        }

        [Test]
        public void Continue_CountdownUsesSameGateLayout()
        {
            StartPlaying();
            Fail();
            _controller.RequestCoinContinue();
            float[] gates = CaptureGatePositions();

            _controller.Tick(0.25f);

            Assert.That(_controller.CountdownPanel.activeSelf, Is.True);
            Assert.That(_controller.CountdownText.text, Is.EqualTo("READY"));
            Assert.That(CaptureGatePositions(), Is.EqualTo(gates));
        }

        [Test]
        public void Continue_ReadyThenGoResumesWithoutBlockingGameplay()
        {
            StartPlaying();
            Fail();
            _controller.RequestCoinContinue();
            float progress = _controller.Session.Progress;
            float distance = _controller.CampaignDistance;

            _controller.Tick(0.25f);

            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.Countdown));
            Assert.That(_controller.ContinueReadyActive, Is.True);
            Assert.That(_controller.CountdownText.text, Is.EqualTo("READY"));
            Assert.That(_controller.Session.Progress, Is.EqualTo(progress));

            _controller.Tick(0.26f);

            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.Playing));
            Assert.That(_controller.ContinueReadyActive, Is.False);
            Assert.That(_controller.ContinueGoFlashActive, Is.True);
            Assert.That(_controller.CountdownText.text, Is.EqualTo("GO!"));

            _controller.Tick(0.1f);

            Assert.That(_controller.CampaignDistance,
                Is.GreaterThan(distance));
            Assert.That(_controller.ContinueGoFlashActive, Is.True);

            _controller.Tick(0.16f);

            Assert.That(_controller.ContinueGoFlashActive, Is.False);
            Assert.That(_controller.CountdownPanel.activeSelf, Is.False);
            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.Playing));
        }

        [Test]
        public void Continue_UnaffectedGateTransformsRemainUnchanged()
        {
            StartPlaying();
            Fail();
            ContinueSnapshot snapshot = _controller.FailureSnapshot;

            _controller.RequestCoinContinue();

            for (int index = 0; index < snapshot.ActiveGates.Length; index++)
            {
                ActiveGateSnapshot expected = snapshot.ActiveGates[index];
                if (expected.PlanIndex == snapshot.FailedGateIndex)
                {
                    continue;
                }
                StageGateView gate =
                    _controller.GetGate(expected.PoolIdentity);
                Assert.That(gate.transform.position,
                    Is.EqualTo(expected.Position));
                Assert.That(gate.transform.rotation,
                    Is.EqualTo(expected.Rotation));
            }
        }

        [Test]
        public void Continue_FailedGateCannotImmediatelyFailAgain()
        {
            StartPlaying();
            Fail();
            int failedIndex = _controller.FailureSnapshot.FailedGateIndex;
            StageGateView failed = FindGateByPlanIndex(failedIndex);

            _controller.RequestCoinContinue();
            _controller.Tick(3.1f);

            Assert.That(FindGateByPlanIndex(failedIndex), Is.Null);
            Assert.That(failed.gameObject.activeSelf, Is.True);
            Assert.That(failed.PlanIndex,
                Is.GreaterThan(_controller.Session.GatesPassed));
            Assert.That(_controller.Session.FlowState,
                Is.Not.EqualTo(StageFlowState.Failed));
            Assert.That(
                _controller.Session.ContinuedFailedGateResolutionCount,
                Is.EqualTo(1));
        }

        [Test]
        public void Continue_ResumesSameTimerProgressSpeedColorAndCursor()
        {
            StartPlaying();
            _controller.TickMovement(0.5f);
            Fail();
            ContinueSnapshot snapshot = _controller.FailureSnapshot;

            _controller.RequestCoinContinue();

            Assert.That(_controller.Session.ElapsedPlayingSeconds,
                Is.EqualTo(snapshot.ElapsedPlayingSeconds));
            Assert.That(_controller.Session.Progress,
                Is.EqualTo(
                    snapshot.NormalizedProgress +
                    (1f / _controller.Session.Stage.TargetGateCount)));
            Assert.That(_controller.Session.CurrentSpeed,
                Is.EqualTo(snapshot.NormalSpeed));
            Assert.That(_controller.Session.CurrentColor,
                Is.EqualTo(snapshot.PlayerColor));
            Assert.That(_controller.Session.SequenceCursor,
                Is.EqualTo(snapshot.SequenceCursor + 1));
        }

        [Test]
        public void Continue_ResumesSameActiveSequence()
        {
            StartPlaying();
            Fail();
            ContinueSnapshot snapshot = _controller.FailureSnapshot;

            _controller.RequestCoinContinue();
            _controller.Tick(3.1f);

            Assert.That(_controller.Session.SequenceCursor,
                Is.EqualTo(snapshot.SequenceCursor + 1));
            for (int index = 0; index < snapshot.ActiveGates.Length; index++)
            {
                ActiveGateSnapshot expected = snapshot.ActiveGates[index];
                if (expected.PlanIndex == snapshot.FailedGateIndex)
                {
                    StageGateView replacement =
                        _controller.GetGate(expected.PoolIdentity);
                    Assert.That(replacement.gameObject.activeSelf, Is.True);
                    Assert.That(replacement.PlanIndex,
                        Is.GreaterThan(snapshot.FailedGateIndex));
                    continue;
                }
                StageGateView gate =
                    _controller.GetGate(expected.PoolIdentity);
                Assert.That(gate.PlanIndex,
                    Is.EqualTo(expected.PlanIndex));
            }
        }

        [Test]
        public void Continue_ClearsCameraShakeAndDoesNotShowItemSelection()
        {
            StartPlaying();
            Fail();
            ContinueSnapshot snapshot = _controller.FailureSnapshot;

            _controller.RequestCoinContinue();
            _controller.Tick(0.4f);

            Assert.That(_controller.GameplayCamera.transform.rotation,
                Is.EqualTo(snapshot.CameraRotation));
            Assert.That(_controller.ItemPanel.activeSelf, Is.False);
        }

        [Test]
        public void Continue_RecyclesFailedSlotWithoutResettingPools()
        {
            StartPlaying();
            Fail();
            int gateCount = Object.FindObjectsByType<StageGateView>(
                FindObjectsInactive.Include).Length;
            int trackCount = Object.FindObjectsByType<TrackSegmentView>(
                FindObjectsInactive.Include).Length;
            ContinueSnapshot snapshot = _controller.FailureSnapshot;
            Vector3[] tracks = CaptureTrackPositions();

            _controller.RequestCoinContinue();
            _controller.Tick(3.1f);

            Assert.That(Object.FindObjectsByType<StageGateView>(
                FindObjectsInactive.Include).Length, Is.EqualTo(gateCount));
            Assert.That(Object.FindObjectsByType<TrackSegmentView>(
                FindObjectsInactive.Include).Length, Is.EqualTo(trackCount));
            Assert.That(CaptureTrackPositions(), Is.EqualTo(tracks));
            for (int index = 0; index < snapshot.ActiveGates.Length; index++)
            {
                ActiveGateSnapshot expected = snapshot.ActiveGates[index];
                StageGateView gate =
                    _controller.GetGate(expected.PoolIdentity);
                if (expected.PlanIndex == snapshot.FailedGateIndex)
                {
                    Assert.That(gate.gameObject.activeSelf, Is.True);
                    Assert.That(gate.PlanIndex,
                        Is.GreaterThan(expected.PlanIndex));
                    continue;
                }
                Assert.That(gate.transform.position,
                    Is.EqualTo(expected.Position));
            }
        }

        [Test]
        public void Continue_DoesNotRestoreShieldOrBooster()
        {
            StartPlaying(shield: true, booster: true);
            EndBooster();
            StageGateView first = FindGateByPlanIndex(0);
            Mismatch(first.AssignedColor);
            Assert.That(first.TryResolveCrossing(), Is.True);
            Assert.That(_controller.Session.ShieldActive, Is.False);
            _controller.Session.Advance(
                GameRules.ShieldRecoveryDuration,
                0f);
            StageGateView second = FindGateByPlanIndex(1);
            Mismatch(second.AssignedColor);
            Assert.That(second.TryResolveCrossing(), Is.True);
            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.Failed));

            _controller.RequestCoinContinue();
            _controller.Tick(3.1f);

            Assert.That(_controller.Session.ShieldActive, Is.False);
            Assert.That(_controller.Session.BoosterActive, Is.False);
            Assert.That(_controller.Session.BoosterDistanceRemaining,
                Is.Zero);
            Assert.That(_controller.ShieldVisual.activeSelf, Is.False);
            Assert.That(_controller.BoosterMeterRoot.activeSelf, Is.False);
            Assert.That(_controller.BoosterWarning.activeSelf, Is.False);
            Assert.That(_controller.SpeedLines.isPlaying, Is.False);
        }

        [Test]
        public void ContinueCountdown_AppliesCamouflageBeforeFirstFrame()
        {
            var store = new InMemoryStageProgressStore
            {
                HighestUnlocked = 9
            };
            _controller.SetProgressStoreForTests(store);
            _controller.SelectStage(9);
            _controller.StartSelectedStage();
            _controller.Tick(3.1f);
            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.Playing));

            StageGateView probe = _controller.GetGate(5);
            GatePlan original = probe.ActivePlan;
            GatePlan camouflage = new GatePlan(
                original.GateId,
                original.Color,
                original.Spacing,
                original.TimeToGate,
                original.BeatMultiplier,
                original.Pattern,
                original.IndexInPattern,
                original.HasShieldPickupBefore,
                new GateModifier(GateModifierType.Camouflage));
            probe.Activate(
                camouflage,
                _controller.Session.GatesPassed + 5,
                _controller.GetPresentationMaterial(camouflage.Color),
                10000f);

            StageGateView failureGate = FindGateByPlanIndex(
                _controller.Session.GatesPassed);
            Mismatch(failureGate.AssignedColor);
            failureGate.TryResolveCrossing();
            _controller.RequestCoinContinue();

            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.Countdown));

            Assert.That(probe.SymbolVisible, Is.False);
            Assert.That(probe.DisplayMaterial,
                Is.EqualTo(_controller.TrackPool
                    .GetSegment(0).SurfaceMaterial));
        }

        [UnityTest]
        public IEnumerator StageEleven_RepeatedContinuesPastGateThirtyFourReachGoal()
        {
            var store = new InMemoryStageProgressStore
            {
                HighestUnlocked = 11
            };
            _controller.SetProgressStoreForTests(store);
            _controller.SetContinueServicesForTests(
                new ContinueEconomyTestGateway(100000),
                new RewardedAdTestService());
            _controller.SelectStage(11);
            _controller.StartSelectedStage();
            _controller.Tick(3.1f);
            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.Playing));

            int continueCount = 0;
            int targetGateCount = _controller.Session.Stage.TargetGateCount;
            for (int resolutionCount = 0;
                resolutionCount < targetGateCount;
                resolutionCount++)
            {
                if (_controller.Session.FlowState ==
                    StageFlowState.StageFinishing)
                {
                    break;
                }
                int planIndex = _controller.Session.GatesPassed;
                StageGateView gate = FindGateByPlanIndex(planIndex);
                Assert.That(gate, Is.Not.Null,
                    $"Gate {planIndex + 1} must stay in the fixed pool.");

                if (planIndex >= 28)
                {
                    RunnerColor judgmentColor =
                        gate.ActivePlan.GetJudgmentColor(
                            _controller.Session.ElapsedPlayingSeconds);
                    Mismatch(judgmentColor);
                    Assert.That(gate.TryResolveCrossing(), Is.True);
                    Assert.That(_controller.Session.FlowState,
                        Is.EqualTo(StageFlowState.Failed));
                    _controller.RequestCoinContinue();
                    continueCount++;
                    Assert.That(_controller.Session.GatesPassed,
                        Is.EqualTo(planIndex + 1));
                    if (_controller.Session.FlowState ==
                        StageFlowState.Countdown)
                    {
                        _controller.Tick(3.1f);
                        _controller.Session.Advance(1.1f, 0f);
                    }
                    yield return null;
                    continue;
                }

                int tapCount = 0;
                while (_controller.Session.CurrentColor != gate.AssignedColor &&
                    tapCount < _controller.Session.Stage.AllowedColorCount)
                {
                    _controller.HandleGameplayTap();
                    tapCount++;
                }
                Assert.That(_controller.Session.CurrentColor,
                    Is.EqualTo(gate.AssignedColor));
                Assert.That(gate.TryResolveCrossing(), Is.True);
                yield return null;
            }

            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.StageFinishing));
            Assert.That(continueCount, Is.GreaterThan(6));
            Assert.That(_controller.Session.GatesPassed,
                Is.EqualTo(_controller.Session.Stage.TargetGateCount));
            float remaining = _controller.GoalPathDistance -
                _controller.CampaignDistance + 1f;
            _controller.TickMovement(
                remaining / _controller.Session.CurrentSpeed);
            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.StageCleared));
        }

        [Test]
        public void ThirtyRepeatedBoosterContinueTransitions_HaveNoTransformDrift()
        {
            int gateCount = Object.FindObjectsByType<StageGateView>(
                FindObjectsInactive.Include).Length;
            int trackCount = Object.FindObjectsByType<TrackSegmentView>(
                FindObjectsInactive.Include).Length;

            for (int iteration = 0; iteration < 30; iteration++)
            {
                StartPlaying(booster: true);
                EndBooster();
                StageGateView first = FindGateByPlanIndex(
                    _controller.Session.GatesPassed);
                Mismatch(first.AssignedColor);
                first.TryResolveCrossing();
                _controller.RequestCoinContinue();
                _controller.Tick(3.1f);
                _controller.RetryToItemSelection();
                if (_controller.BoosterSelected)
                {
                    _controller.ToggleBoosterSelection();
                }
                _controller.ShowLobby();
            }

            Assert.That(Object.FindObjectsByType<StageGateView>(
                FindObjectsInactive.Include).Length, Is.EqualTo(gateCount));
            Assert.That(Object.FindObjectsByType<TrackSegmentView>(
                FindObjectsInactive.Include).Length, Is.EqualTo(trackCount));
            for (int index = 0; index < _controller.GatePoolSize; index++)
            {
                Transform gate = _controller.GetGate(index).transform;
                Assert.That(gate.localRotation,
                    Is.EqualTo(Quaternion.identity));
                Assert.That(gate.localScale, Is.EqualTo(Vector3.one));
            }
        }

        private void StartPlaying(
            bool shield = false,
            bool booster = false)
        {
            if (shield || booster)
            {
                InMemoryStageProgressStore store =
                    new InMemoryStageProgressStore
                    {
                        HighestUnlocked = 8
                    };
                _controller.SetProgressStoreForTests(store);
                _controller.SelectStage(8);
            }
            else
            {
                _controller.PlayFromLobby();
            }
            if (shield)
            {
                _controller.ToggleShieldSelection();
            }
            if (booster)
            {
                _controller.ToggleBoosterSelection();
            }
            _controller.StartSelectedStage();
            _controller.Tick(3.1f);
            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.Playing));
        }

        private void EndBooster()
        {
            _controller.Session.Advance(
                0f,
                _controller.Session.Stage.BoosterDistance - 1f);
            _controller.Tick(
                1.01f / _controller.Session.Stage.BoosterSpeed);
            Assert.That(_controller.Session.BoosterActive, Is.False);
            Assert.That(_controller.BoosterExitOverridesApplied, Is.True);
        }

        private void Fail()
        {
            StageGateView gate = FindGateByPlanIndex(
                _controller.Session.GatesPassed);
            Mismatch(gate.AssignedColor);
            gate.TryResolveCrossing();
            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.Failed));
        }

        private void Mismatch(RunnerColor color)
        {
            if (_controller.Session.CurrentColor == color)
            {
                _controller.HandleGameplayTap();
            }
            Assert.That(_controller.Session.CurrentColor,
                Is.Not.EqualTo(color));
        }

        private StageGateView FindGateByPlanIndex(int planIndex)
        {
            for (int index = 0; index < _controller.GatePoolSize; index++)
            {
                StageGateView gate = _controller.GetGate(index);
                if (gate.gameObject.activeSelf &&
                    gate.PlanIndex == planIndex)
                {
                    return gate;
                }
            }
            return null;
        }

        private float[] CaptureGatePositions()
        {
            float[] result = new float[_controller.GatePoolSize];
            for (int index = 0; index < result.Length; index++)
            {
                result[index] =
                    _controller.GetGate(index).PathDistance;
            }
            return result;
        }

        private float MaximumAdjacentGateGap()
        {
            float maximum = 0f;
            for (int index = 1; index < _controller.GatePoolSize; index++)
            {
                float gap =
                    _controller.GetGate(index).PathDistance -
                    _controller.GetGate(index - 1).PathDistance;
                maximum = Mathf.Max(maximum, gap);
            }
            return maximum;
        }

        private static Vector3[] CaptureTrackPositions()
        {
            TrackSegmentView[] tracks =
                Object.FindObjectsByType<TrackSegmentView>(
                    FindObjectsInactive.Include);
            Vector3[] result = new Vector3[tracks.Length];
            for (int index = 0; index < tracks.Length; index++)
            {
                result[index] = tracks[index].transform.position;
            }
            return result;
        }

        private sealed class InMemoryStageProgressStore :
            IStageProgressStore
        {
            private readonly StageRecord[] _records =
                new StageRecord[StageCatalog.Count];

            public int HighestUnlocked { get; set; } = 1;

            public int LoadHighestUnlocked() => HighestUnlocked;
            public StageRecord LoadRecord(int stageNumber) =>
                _records[stageNumber - 1];
            public void SaveHighestUnlocked(int stageNumber)
            {
                HighestUnlocked = stageNumber;
            }
            public void SaveRecord(int stageNumber, StageRecord record) =>
                _records[stageNumber - 1] = record;

            public void ClearGameplayProgress()
            {
                System.Array.Clear(_records, 0, _records.Length);
                HighestUnlocked = 1;
            }
        }
    }
}
