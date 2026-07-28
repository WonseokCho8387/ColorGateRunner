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
        private StageSceneController _controller;
        private InMemoryStageProgressStore _store;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
            yield return null;
            _controller = Object.FindAnyObjectByType<StageSceneController>();
            Assert.That(_controller, Is.Not.Null);
            _store = new InMemoryStageProgressStore();
            _controller.SetProgressStoreForTests(_store);
        }

        [Test]
        public void NormalFlow_OpensLobbyRatherThanStageSelect()
        {
            Assert.That(_controller.LobbyPanel.activeSelf, Is.True);
            Assert.That(_controller.StageSelectPanel.activeSelf, Is.False);
            Assert.That(_controller.LobbyStageText.text, Does.Contain("STAGE 1"));
        }

        [Test]
        public void Play_OpensItemSelection()
        {
            _controller.PlayFromLobby();
            Assert.That(_controller.ItemPanel.activeSelf, Is.True);
            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.PreRunSelection));
        }

        [Test]
        public void DebugStagePicker_RemainsHidden()
        {
            Assert.That(_controller.StageSelectPanel.activeSelf, Is.False);
        }

        [Test]
        public void ColorHud_UpdatesCurrentAndNextAfterTap()
        {
            StartPlaying(false, false);
            Assert.That(_controller.CurrentColorText.text, Does.Contain("RED"));
            Assert.That(_controller.NextColorText.text, Does.Contain("BLUE"));
            _controller.HandleGameplayTap();
            Assert.That(_controller.CurrentColorText.text, Does.Contain("BLUE"));
            Assert.That(_controller.NextColorText.text, Does.Contain("RED"));
        }

        [Test]
        public void ThreeColorHud_ShowsFullOrder()
        {
            _store.HighestUnlocked = 5;
            _controller.SetProgressStoreForTests(_store);
            _controller.SelectStage(5);
            _controller.StartSelectedStage();
            _controller.Tick(3.1f);
            Assert.That(_controller.CycleOrderText.text, Does.Contain("GREEN"));
        }

        [Test]
        public void Shield_DoesNotHidePlayerMaterialStructurally()
        {
            Assert.That(_controller.ShieldVisual.GetComponent<Renderer>(), Is.Null);
            Assert.That(
                _controller.ShieldVisual.GetComponentsInChildren<Renderer>(true).Length,
                Is.EqualTo(6));
            Assert.That(_controller.PlayerRenderer, Is.Not.Null);
        }

        [Test]
        public void Shield_AppearsDuringCountdown()
        {
            SelectItemsAndStart(true, false);
            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.Countdown));
            Assert.That(_controller.ShieldVisual.activeSelf, Is.True);
            Assert.That(_controller.Session.ShieldActive, Is.False);
        }

        [Test]
        public void ShieldBreak_EntersRecoveryAndRecoversPresentation()
        {
            StartPlaying(true, false);
            StageGateView gate = _controller.GetGate(0);
            Mismatch(gate.AssignedColor);

            Assert.That(gate.TryResolveCrossing(), Is.True);
            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.ShieldRecovery));
            Assert.That(_controller.Session.ShieldActive, Is.False);
            Assert.That(_controller.ShieldVisual.activeSelf, Is.False);

            _controller.Session.Advance(
                GameRules.ShieldRecoveryDuration,
                0f);
            _controller.Tick(0f);

            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.Playing));
            Assert.That(_controller.ShieldVisual.activeSelf, Is.False);
        }

        [Test]
        public void Booster_LaunchRequestsHapticAndActivatesMeter()
        {
            RecordingHaptics haptics = new RecordingHaptics();
            _controller.SetHapticsForTests(haptics);
            StartPlaying(false, true);
            _controller.Tick(0f);
            Assert.That(haptics.RequestCount, Is.EqualTo(1));
            Assert.That(_controller.BoosterMeterFill.fillAmount, Is.GreaterThan(0f));
            Assert.That(_controller.SpeedLines.isPlaying, Is.True);
        }

        [Test]
        public void Booster_BypassesMismatchedGate()
        {
            StartPlaying(false, true);
            StageGateView gate = _controller.GetGate(0);
            Mismatch(gate.AssignedColor);

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
            Mismatch(gate.AssignedColor);

            Assert.That(gate.TryResolveCrossing(), Is.True);
            Assert.That(_controller.Session.ShieldActive, Is.True);
            Assert.That(_controller.ShieldVisual.activeSelf, Is.True);
        }

        [Test]
        public void Booster_WarningActivatesNearEnd()
        {
            StartPlaying(false, true);
            float remaining = _controller.Session.Stage.BoosterDistance * 0.15f;
            _controller.Session.Advance(
                1f,
                _controller.Session.Stage.BoosterDistance - remaining);
            _controller.Tick(0f);
            Assert.That(_controller.BoosterWarning.activeSelf, Is.True);
        }

        [Test]
        public void Booster_CameraReturnsToExactBaseline()
        {
            StartPlaying(false, true);
            Quaternion rotation = _controller.GameplayCamera.transform.rotation;
            _controller.Session.Advance(
                1f,
                _controller.Session.Stage.BoosterDistance + 1f);
            _controller.Session.Advance(StageSession.BoosterExitDuration, 0f);
            _controller.Tick(0f);
            Assert.That(_controller.GameplayCamera.fieldOfView,
                Is.EqualTo(60f).Within(0.001f));
            Assert.That(
                Quaternion.Angle(
                    rotation,
                    _controller.GameplayCamera.transform.rotation),
                Is.LessThan(0.001f));
        }

        [Test]
        public void FailureCameraShake_RestoresOriginalPositionAndRotation()
        {
            StartPlaying(false, false);
            Vector3 position = _controller.GameplayCamera.transform.position;
            Quaternion rotation = _controller.GameplayCamera.transform.rotation;

            Fail();
            _controller.Tick(0.1f);
            Assert.That(_controller.GameplayCamera.transform.position,
                Is.Not.EqualTo(position));

            _controller.Tick(0.3f);
            Assert.That(_controller.GameplayCamera.transform.position,
                Is.EqualTo(position));
            Assert.That(
                Quaternion.Angle(
                    rotation,
                    _controller.GameplayCamera.transform.rotation),
                Is.LessThan(0.001f));
        }

        [Test]
        public void PostBooster_FirstGateMatchesCurrentColor()
        {
            StartPlaying(false, true);
            _controller.Tick(
                (_controller.Session.Stage.BoosterDistance /
                _controller.Session.Stage.BoosterSpeed) + 0.1f);
            Assert.That(_controller.GetGate(0).AssignedColor,
                Is.EqualTo(_controller.Session.CurrentColor));
        }

        [Test]
        public void Gate_HasLeftRightAndTopParts()
        {
            StageGateView gate = _controller.GetGate(0);
            Assert.That(gate.PartCount, Is.EqualTo(3));
            Assert.That(gate.transform.Find("Left"), Is.Not.Null);
            Assert.That(gate.transform.Find("Right"), Is.Not.Null);
            Assert.That(gate.transform.Find("Top"), Is.Not.Null);
        }

        [Test]
        public void BoosterGateDestruction_ResetsAfterPooling()
        {
            StartPlaying(false, true);
            StageGateView gate = _controller.GetGate(0);
            Vector3 position = gate.transform.Find("Left").localPosition;
            Quaternion rotation = gate.transform.Find("Left").localRotation;
            Vector3 scale = gate.transform.Find("Left").localScale;
            gate.TryResolveCrossing();
            Assert.That(gate.BoosterDestroyed, Is.True);
            Assert.That(gate.transform.Find("Left").localPosition,
                Is.Not.EqualTo(position));
            _controller.Tick(0.25f);
            Assert.That(gate.BoosterDestroyed, Is.False);
            Assert.That(gate.transform.Find("Left").localPosition,
                Is.EqualTo(position));
            Assert.That(gate.transform.Find("Left").localRotation,
                Is.EqualTo(rotation));
            Assert.That(gate.transform.Find("Left").localScale,
                Is.EqualTo(scale));
        }

        [Test]
        public void NormalGate_RemainsIntact()
        {
            StartPlaying(false, false);
            StageGateView gate = _controller.GetGate(0);
            Match(gate.AssignedColor);
            gate.TryResolveCrossing();
            Assert.That(gate.BoosterDestroyed, Is.False);
        }

        [Test]
        public void FailureAnimation_PrecedesFailurePanel()
        {
            StartPlaying(false, false);
            Fail();
            Assert.That(_controller.FailPanel.activeSelf, Is.False);
            Assert.That(_controller.PlayerTransform.localRotation,
                Is.Not.EqualTo(Quaternion.identity));
            _controller.Tick(_controller.FailurePanelDelaySeconds + 0.01f);
            Assert.That(_controller.FailPanel.activeSelf, Is.True);
            Assert.That(_controller.FailDetailsText.text,
                Does.Contain("PROGRESS"));
            Assert.That(_controller.FailDetailsText.text,
                Does.Contain("ITEMS"));
        }

        [Test]
        public void Failure_StopsMovementImmediately()
        {
            StartPlaying(false, false);
            Fail();
            Vector3 position = _controller.PlayerTransform.position;

            _controller.TickMovement(1f);

            Assert.That(_controller.PlayerTransform.position,
                Is.EqualTo(position));
        }

        [Test]
        public void ClearAnimation_PrecedesClearPanel()
        {
            ClearStage();
            Assert.That(_controller.ClearPanel.activeSelf, Is.False);
            Assert.That(_controller.PlayerTransform.localScale.x,
                Is.GreaterThan(1f));
            _controller.Tick(_controller.ClearPanelDelaySeconds + 0.01f);
            Assert.That(_controller.ClearPanel.activeSelf, Is.True);
        }

        [Test]
        public void ClearAndFailurePanels_HaveDistinctRootsAndHierarchy()
        {
            Assert.That(_controller.ClearPanel, Is.Not.SameAs(_controller.FailPanel));
            Assert.That(_controller.ClearPanel.name, Is.EqualTo("StageClearPanel"));
            Assert.That(_controller.FailPanel.name, Is.EqualTo("StageFailedPanel"));
            Assert.That(CountNamed("ClearContinueButton"), Is.EqualTo(1));
            Assert.That(CountNamed("ContinueButton"), Is.EqualTo(1));
        }

        [Test]
        public void FinalGate_RevealsGoalAndGoalCrossingClearsStage()
        {
            StartPlaying(false, false);
            ResolveRemainingGates();

            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.StageFinishing));
            Assert.That(_controller.Goal.activeSelf, Is.True);
            Assert.That(
                _controller.Session.ResolveGate(RunnerColor.Green),
                Is.EqualTo(GateOutcome.Ignored));

            PlacePlayerBeforeGoal();
            _controller.TickMovement(2f);

            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.StageCleared));
            Assert.That(_controller.FailPanel.activeSelf, Is.False);
        }

        [Test]
        public void Continue_BypassesItemSelectionAndStartsOneCountdown()
        {
            StartPlaying(false, false);
            Fail();
            _controller.Tick(1.1f);
            _controller.ContinueAfterFailure();
            Assert.That(_controller.ItemPanel.activeSelf, Is.False);
            Assert.That(_controller.CountdownPanel.activeSelf, Is.True);
            Assert.That(CountNamed("CountdownPanel"), Is.EqualTo(1));
        }

        [Test]
        public void Continue_GrantsTemporaryProtection()
        {
            StartPlaying(false, false);
            Fail();
            _controller.ContinueAfterFailure();
            _controller.Tick(3.1f);
            Assert.That(_controller.Session.ContinueProtectionActive, Is.True);
            Assert.That(_controller.Session.SafeGateCountRemaining, Is.EqualTo(2));
        }

        [Test]
        public void SecondFailure_RemovesContinueOption()
        {
            StartPlaying(false, false);
            Fail();
            _controller.ContinueAfterFailure();
            _controller.Tick(3.1f);
            ResolveSafeGates();
            _controller.Session.Advance(1.1f, 0f);
            Fail();
            _controller.Tick(1.1f);
            Assert.That(_controller.ContinueButton.gameObject.activeSelf, Is.False);
        }

        [Test]
        public void Retry_StillOpensItemSelection()
        {
            StartPlaying(false, false);
            Fail();
            _controller.RetryToItemSelection();
            Assert.That(_controller.ItemPanel.activeSelf, Is.True);
            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.PreRunSelection));
        }

        [Test]
        public void Retry_RetainsItemSelection()
        {
            StartPlaying(true, true);
            _controller.RetryToItemSelection();

            Assert.That(_controller.ShieldSelected, Is.True);
            Assert.That(_controller.BoosterSelected, Is.True);
            Assert.That(_controller.Session.Items.Shield, Is.True);
            Assert.That(_controller.Session.Items.Booster, Is.True);
        }

        [Test]
        public void Restart_ReplaysIdenticalGateLayout()
        {
            StartPlaying(true, true);
            RunnerColor[] colors = CaptureGateColors();
            float[] positions = CaptureGatePositions();

            _controller.RetryToItemSelection();
            _controller.StartSelectedStage();

            Assert.That(CaptureGateColors(), Is.EqualTo(colors));
            Assert.That(CaptureGatePositions(), Is.EqualTo(positions));
        }

        [Test]
        public void StageClear_PersistsRecordAndUnlock()
        {
            ClearStage();
            StageRecord record = _store.LoadRecord(1);

            Assert.That(record.Cleared, Is.True);
            Assert.That(record.ClearCount, Is.EqualTo(1));
            Assert.That(record.ContinuedClearCount, Is.Zero);
            Assert.That(record.BestTime, Is.GreaterThan(0f));
            Assert.That(_store.LoadHighestUnlocked(), Is.EqualTo(2));
        }

        [Test]
        public void ContinuedClear_UnlocksButDoesNotReplaceBestRecords()
        {
            _store.Records[0] =
                new StageRecord(true, 12f, 13f, 2, 0);
            _controller.SetProgressStoreForTests(_store);
            StartPlaying(false, false);
            Fail();
            _controller.ContinueAfterFailure();
            _controller.Tick(3.1f);
            _controller.Session.Advance(1f, 0f);
            FinishCurrentStage();

            StageRecord record = _store.LoadRecord(1);
            Assert.That(record.Cleared, Is.True);
            Assert.That(record.BestTime, Is.EqualTo(12f));
            Assert.That(record.BestNoItemTime, Is.EqualTo(13f));
            Assert.That(record.ClearCount, Is.EqualTo(3));
            Assert.That(record.ContinuedClearCount, Is.EqualTo(1));
            Assert.That(_store.LoadHighestUnlocked(), Is.EqualTo(2));
        }

        [Test]
        public void ClearContinue_ReturnsToUpdatedLobby()
        {
            ClearStage();
            _controller.Tick(1.3f);
            _controller.ShowLobby();
            Assert.That(_controller.LobbyPanel.activeSelf, Is.True);
            Assert.That(_controller.SelectedStageNumber, Is.EqualTo(2));
            Assert.That(_controller.LobbyStageText.text, Does.Contain("STAGE 2"));
        }

        [TestCase(2, 1)]
        [TestCase(4, 2)]
        [TestCase(5, 3)]
        public void LobbyProgression_ChangesAtMilestones(
            int clearedThrough,
            int expectedTier)
        {
            _store.HighestUnlocked = Mathf.Min(5, clearedThrough + 1);
            for (int stage = 1; stage <= clearedThrough; stage++)
            {
                _store.Records[stage - 1] =
                    new StageRecord(true, 30f, 32f, 1);
            }
            _controller.SetProgressStoreForTests(_store);
            Assert.That(_controller.LobbyTier, Is.EqualTo(expectedTier));
        }

        [Test]
        public void ThirtyRepeatedFlows_LeaveNoDuplicateOrStaleVisuals()
        {
            int gates = CountStageGates();
            int tracks = Object.FindObjectsByType<TrackSegmentView>(
                FindObjectsInactive.Include).Length;
            for (int index = 0; index < 30; index++)
            {
                _controller.PlayFromLobby();
                if ((index & 1) == 0)
                {
                    _controller.ToggleShieldSelection();
                }
                if ((index % 3) == 0)
                {
                    _controller.ToggleBoosterSelection();
                }
                _controller.StartSelectedStage();
                _controller.Tick(3.1f);
                _controller.RetryToItemSelection();
                _controller.ShowLobby();
            }
            Assert.That(CountStageGates(), Is.EqualTo(gates));
            Assert.That(Object.FindObjectsByType<TrackSegmentView>(
                FindObjectsInactive.Include).Length, Is.EqualTo(tracks));
            Assert.That(CountNamed("LobbyPanel"), Is.EqualTo(1));
            Assert.That(CountNamed("StageClearPanel"), Is.EqualTo(1));
            Assert.That(CountNamed("StageFailedPanel"), Is.EqualTo(1));
            Assert.That(_controller.GameplayCamera.fieldOfView,
                Is.EqualTo(60f).Within(0.001f));
            Assert.That(_controller.SpeedLines.isPlaying, Is.False);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void EveryStage_AllItemCombinationsCanInitialize(int stageNumber)
        {
            _store.HighestUnlocked = StageCatalog.Count;
            _controller.SetProgressStoreForTests(_store);
            for (int mask = 0; mask < 4; mask++)
            {
                _controller.ShowLobby();
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

                Assert.That(_controller.Session.Items.Shield,
                    Is.EqualTo((mask & 1) != 0));
                Assert.That(_controller.Session.Items.Booster,
                    Is.EqualTo((mask & 2) != 0));
                Assert.That(_controller.Session.FlowState,
                    Is.EqualTo(StageFlowState.Countdown));

                _controller.Tick(3.1f);
                Assert.That(_controller.Session.FlowState,
                    Is.EqualTo(StageFlowState.Playing));
                Assert.That(_controller.Session.ShieldActive,
                    Is.EqualTo((mask & 1) != 0));
                Assert.That(_controller.Session.BoosterActive,
                    Is.EqualTo((mask & 2) != 0));
                Assert.That(_controller.ShieldVisual.activeSelf,
                    Is.EqualTo((mask & 1) != 0));
                Assert.That(_controller.GatePoolSize, Is.EqualTo(6));
            }
        }

        [Test]
        public void RuntimeGateAndTrackPools_DoNotGrow()
        {
            StartPlaying(false, true);
            int gatesBefore = CountStageGates();
            int tracksBefore = Object.FindObjectsByType<TrackSegmentView>(
                FindObjectsInactive.Include).Length;

            for (int index = 0; index < 18; index++)
            {
                StageGateView gate = _controller.GetGate(index % 6);
                gate.TryResolveCrossing();
                _controller.Tick(0.25f);
            }
            _controller.TickMovement(20f);

            Assert.That(CountStageGates(), Is.EqualTo(gatesBefore));
            Assert.That(gatesBefore, Is.EqualTo(6));
            Assert.That(Object.FindObjectsByType<TrackSegmentView>(
                FindObjectsInactive.Include).Length, Is.EqualTo(tracksBefore));
        }

        [Test]
        public void SafeArea_CalculatesNormalizedAnchorsAndContainsPlayerUi()
        {
            SafeAreaLayout.CalculateAnchors(
                new Rect(0f, 100f, 1080f, 2200f),
                1080,
                2400,
                out Vector2 anchorMin,
                out Vector2 anchorMax);

            Assert.That(anchorMin.x, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(anchorMin.y, Is.EqualTo(1f / 24f).Within(0.0001f));
            Assert.That(anchorMax.x, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(anchorMax.y, Is.EqualTo(23f / 24f).Within(0.0001f));

            GameObject safeArea = GameObject.Find("SafeAreaRoot");
            Assert.That(safeArea, Is.Not.Null);
            Assert.That(safeArea.GetComponent<SafeAreaLayout>(), Is.Not.Null);
            Assert.That(_controller.LobbyPanel.transform.IsChildOf(
                safeArea.transform), Is.True);
            Assert.That(_controller.StageHud.transform.IsChildOf(
                safeArea.transform), Is.True);
            Assert.That(_controller.ClearPanel.transform.IsChildOf(
                safeArea.transform), Is.True);
            Assert.That(_controller.FailPanel.transform.IsChildOf(
                safeArea.transform), Is.True);
        }

        [Test]
        public void RequiredReferencesAndPools_AreStable()
        {
            Assert.That(_controller.HasRequiredReferences(), Is.True);
            Assert.That(_controller.GatePoolSize, Is.EqualTo(6));
            Assert.That(Object.FindObjectsByType<ShieldPickupView>(
                FindObjectsInactive.Include).Length, Is.Zero);
        }

        [Test]
        public void Scene_HasNoMissingMonoBehaviours()
        {
            Transform[] transforms =
                Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);
            for (int index = 0; index < transforms.Length; index++)
            {
                Component[] components = transforms[index].GetComponents<Component>();
                for (int component = 0; component < components.Length; component++)
                {
                    Assert.That(components[component], Is.Not.Null);
                }
            }
        }

        private void SelectItemsAndStart(bool shield, bool booster)
        {
            _controller.PlayFromLobby();
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

        private void Match(RunnerColor color)
        {
            for (int index = 0;
                index < 3 && _controller.Session.CurrentColor != color;
                index++)
            {
                _controller.HandleGameplayTap();
            }
        }

        private void Mismatch(RunnerColor color)
        {
            if (_controller.Session.CurrentColor == color)
            {
                _controller.HandleGameplayTap();
            }
            Assert.That(_controller.Session.CurrentColor, Is.Not.EqualTo(color));
        }

        private void Fail()
        {
            StageGateView gate = _controller.GetGate(0);
            if (_controller.Session.CurrentColor == gate.AssignedColor)
            {
                _controller.HandleGameplayTap();
            }
            gate.TryResolveCrossing();
            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.Failed));
        }

        private void ResolveSafeGates()
        {
            int index = 0;
            while (_controller.Session.SafeGateCountRemaining > 0)
            {
                StageGateView gate = _controller.GetGate(index++);
                Match(gate.AssignedColor);
                gate.TryResolveCrossing();
            }
        }

        private void ClearStage()
        {
            StartPlaying(false, false);
            _controller.Session.Advance(1f, 0f);
            FinishCurrentStage();
        }

        private void FinishCurrentStage()
        {
            int index = 0;
            while (_controller.Session.RemainingGates > 0)
            {
                StageGateView gate = _controller.GetGate(index % 6);
                Match(gate.AssignedColor);
                gate.TryResolveCrossing();
                index++;
            }
            Vector3 position = _controller.PlayerTransform.position;
            position.z = _controller.Goal.transform.position.z - 1f;
            _controller.PlayerTransform.position = position;
            _controller.TickMovement(2f);
            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.StageCleared));
        }

        private void ResolveRemainingGates()
        {
            int index = 0;
            while (_controller.Session.RemainingGates > 0)
            {
                StageGateView gate = _controller.GetGate(index % 6);
                Match(gate.AssignedColor);
                gate.TryResolveCrossing();
                index++;
            }
        }

        private void PlacePlayerBeforeGoal()
        {
            Vector3 position = _controller.PlayerTransform.position;
            position.z = _controller.Goal.transform.position.z - 1f;
            _controller.PlayerTransform.position = position;
        }

        private static int CountNamed(string value)
        {
            int count = 0;
            Transform[] transforms =
                Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);
            for (int index = 0; index < transforms.Length; index++)
            {
                if (transforms[index].name == value)
                {
                    count++;
                }
            }
            return count;
        }

        private static int CountStageGates()
        {
            return Object.FindObjectsByType<StageGateView>(
                FindObjectsInactive.Include).Length;
        }

        private RunnerColor[] CaptureGateColors()
        {
            RunnerColor[] result =
                new RunnerColor[_controller.GatePoolSize];
            for (int index = 0; index < result.Length; index++)
            {
                result[index] = _controller.GetGate(index).AssignedColor;
            }
            return result;
        }

        private float[] CaptureGatePositions()
        {
            float[] result = new float[_controller.GatePoolSize];
            for (int index = 0; index < result.Length; index++)
            {
                result[index] = _controller.GetGate(index).transform.position.z;
            }
            return result;
        }

        private sealed class RecordingHaptics : IHapticFeedback
        {
            internal int RequestCount { get; private set; }
            public void RequestBoosterLaunch()
            {
                RequestCount++;
            }
        }

        private sealed class InMemoryStageProgressStore : IStageProgressStore
        {
            internal int HighestUnlocked { get; set; } = 1;
            internal StageRecord[] Records { get; } =
                new StageRecord[StageCatalog.Count];

            public int LoadHighestUnlocked() => HighestUnlocked;
            public StageRecord LoadRecord(int stageNumber) =>
                Records[stageNumber - 1];
            public void SaveHighestUnlocked(int stageNumber) =>
                HighestUnlocked = stageNumber;
            public void SaveRecord(int stageNumber, StageRecord record) =>
                Records[stageNumber - 1] = record;
        }
    }
}
