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
    public sealed class GrayboxScenePlayModeTests
    {
        private StageSceneController _controller;
        private InMemoryStageProgressStore _store;
        private StartItemInventoryTestGateway _inventory;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
            yield return null;
            _controller = Object.FindAnyObjectByType<StageSceneController>();
            Assert.That(_controller, Is.Not.Null);
            _store = new InMemoryStageProgressStore();
            _controller.SetProgressStoreForTests(_store);
            _inventory = new StartItemInventoryTestGateway();
            _controller.SetStartItemInventoryForTests(_inventory);
            _controller.SetContinueServicesForTests(
                new ContinueEconomyTestGateway(),
                new RewardedAdTestService());
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
        public void DirectCampaignPreRunBack_ReturnsToCampaignLobby()
        {
            _controller.PlayFromLobby();

            Assert.That(_controller.EnteredFromFrontendLaunch, Is.False);
            _controller.HandlePreRunBack();

            Assert.That(_controller.UiFlow, Is.EqualTo(MobileUiFlow.Lobby));
            Assert.That(_controller.LobbyRoot.activeSelf, Is.True);
            Assert.That(_controller.PreRunRoot.activeSelf, Is.False);
        }

        [Test]
        public void StagesOneThroughFive_ShowLockedItemsAndRejectToggles()
        {
            _store.HighestUnlocked = 5;
            _controller.SetProgressStoreForTests(_store);

            for (int stageNumber = 1; stageNumber <= 5; stageNumber++)
            {
                _controller.SelectStage(stageNumber);
                Assert.That(
                    _controller.ShieldToggleButton.interactable,
                    Is.False);
                Assert.That(
                    _controller.BoosterToggleButton.interactable,
                    Is.False);
                Assert.That(
                    _controller.ShieldToggleText.text,
                    Is.EqualTo("SHIELD: LOCKED"));
                Assert.That(
                    _controller.BoosterToggleText.text,
                    Is.EqualTo("BOOSTER: LOCKED"));

                _controller.ToggleShieldSelection();
                _controller.ToggleBoosterSelection();
                Assert.That(_controller.ShieldSelected, Is.False);
                Assert.That(_controller.BoosterSelected, Is.False);
            }
        }

        [Test]
        public void StageEight_StartConsumesSelectedInventoryBeforeCountdown()
        {
            _store.HighestUnlocked = 8;
            _controller.SetProgressStoreForTests(_store);
            _inventory = new StartItemInventoryTestGateway(1, 1);
            _controller.SetStartItemInventoryForTests(_inventory);
            _controller.SelectStage(8);

            Assert.That(_controller.ShieldToggleText.text,
                Is.EqualTo("SHIELD x1: OFF"));
            Assert.That(_controller.BoosterToggleText.text,
                Is.EqualTo("BOOSTER x1: OFF"));
            _controller.ToggleShieldSelection();
            _controller.ToggleBoosterSelection();
            _controller.StartSelectedStage();
            _controller.StartSelectedStage();

            Assert.That(_inventory.ShieldCount, Is.Zero);
            Assert.That(_inventory.BoosterCount, Is.Zero);
            Assert.That(_inventory.SelectedConsumptionCount, Is.EqualTo(1));
            Assert.That(_controller.Session.Items.Shield, Is.True);
            Assert.That(_controller.Session.Items.Booster, Is.True);
            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.Countdown));
        }

        [Test]
        public void StageEight_ZeroStockDisablesSelectionAndStartsItemless()
        {
            _store.HighestUnlocked = 8;
            _controller.SetProgressStoreForTests(_store);
            _inventory = new StartItemInventoryTestGateway(0, 0);
            _controller.SetStartItemInventoryForTests(_inventory);
            _controller.SelectStage(8);

            Assert.That(_controller.ShieldToggleButton.interactable, Is.False);
            Assert.That(_controller.BoosterToggleButton.interactable, Is.False);
            Assert.That(_controller.ShieldToggleText.text,
                Is.EqualTo("SHIELD x0: OFF"));
            Assert.That(_controller.BoosterToggleText.text,
                Is.EqualTo("BOOSTER x0: OFF"));
            _controller.ToggleShieldSelection();
            _controller.ToggleBoosterSelection();
            _controller.StartSelectedStage();

            Assert.That(_controller.Session.Items.Shield, Is.False);
            Assert.That(_controller.Session.Items.Booster, Is.False);
            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.Countdown));
        }

        [Test]
        public void EmptyHearts_BlockStageStartAndShowTruthfulError()
        {
            _store.HighestUnlocked = 8;
            _controller.SetProgressStoreForTests(_store);
            _inventory = new StartItemInventoryTestGateway(0, 0, 0);
            _controller.SetStartItemInventoryForTests(_inventory);
            _controller.SelectStage(8);

            _controller.StartSelectedStage();

            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.PreRunSelection));
            Assert.That(_controller.PreRunStatusText.gameObject.activeSelf,
                Is.True);
            Assert.That(_controller.PreRunStatusText.text,
                Is.EqualTo("NOT ENOUGH HEARTS"));
        }

        [Test]
        public void StartItemSaveFailure_BlocksCountdownAndShowsError()
        {
            _store.HighestUnlocked = 8;
            _controller.SetProgressStoreForTests(_store);
            _inventory = new StartItemInventoryTestGateway(1, 0)
            {
                FailWrites = true
            };
            _controller.SetStartItemInventoryForTests(_inventory);
            _controller.SelectStage(8);
            _controller.ToggleShieldSelection();

            _controller.StartSelectedStage();

            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.PreRunSelection));
            Assert.That(_controller.Session.Items.Shield, Is.False);
            Assert.That(_inventory.ShieldCount, Is.EqualTo(1));
            Assert.That(_controller.ShieldSelected, Is.True);
            Assert.That(_controller.PreRunStatusText.gameObject.activeSelf,
                Is.True);
            Assert.That(_controller.PreRunStatusText.text,
                Is.EqualTo("START SAVE FAILED"));
        }

        [Test]
        public void InventoryChangedAfterSelection_BlocksStartAndNormalizes()
        {
            _store.HighestUnlocked = 8;
            _controller.SetProgressStoreForTests(_store);
            _inventory = new StartItemInventoryTestGateway(1, 0);
            _controller.SetStartItemInventoryForTests(_inventory);
            _controller.SelectStage(8);
            _controller.ToggleShieldSelection();
            _inventory.SetCounts(0, 0);

            _controller.StartSelectedStage();

            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.PreRunSelection));
            Assert.That(_controller.ShieldSelected, Is.False);
            Assert.That(_controller.PreRunStatusText.text,
                Is.EqualTo("NOT ENOUGH START ITEMS"));
            Assert.That(_controller.ShieldToggleButton.interactable, Is.False);
        }

        [Test]
        public void BackBeforeStart_DoesNotConsumeSelectedItem()
        {
            _store.HighestUnlocked = 8;
            _controller.SetProgressStoreForTests(_store);
            _inventory = new StartItemInventoryTestGateway(1, 0);
            _controller.SetStartItemInventoryForTests(_inventory);
            _controller.SelectStage(8);
            _controller.ToggleShieldSelection();

            _controller.HandlePreRunBack();

            Assert.That(_inventory.ShieldCount, Is.EqualTo(1));
            Assert.That(_inventory.SelectedConsumptionCount, Is.Zero);
            Assert.That(_controller.UiFlow, Is.EqualTo(MobileUiFlow.Lobby));
        }

        [Test]
        public void Retry_StartConsumesSelectedItemForNewAttempt()
        {
            _store.HighestUnlocked = 8;
            _controller.SetProgressStoreForTests(_store);
            _inventory = new StartItemInventoryTestGateway(2, 0);
            _controller.SetStartItemInventoryForTests(_inventory);
            _controller.SelectStage(8);
            _controller.ToggleShieldSelection();
            _controller.StartSelectedStage();

            _controller.RetryToItemSelection();

            Assert.That(_controller.ShieldSelected, Is.True);
            Assert.That(_controller.ShieldToggleText.text,
                Is.EqualTo("SHIELD x1: ON"));
            _controller.StartSelectedStage();
            Assert.That(_inventory.ShieldCount, Is.Zero);
            Assert.That(_inventory.SelectedConsumptionCount, Is.EqualTo(2));
        }

        [Test]
        public void Retry_NormalizesSelectedItemWhenStockReachesZero()
        {
            _store.HighestUnlocked = 8;
            _controller.SetProgressStoreForTests(_store);
            _inventory = new StartItemInventoryTestGateway(1, 0);
            _controller.SetStartItemInventoryForTests(_inventory);
            _controller.SelectStage(8);
            _controller.ToggleShieldSelection();
            _controller.StartSelectedStage();

            _controller.RetryToItemSelection();

            Assert.That(_controller.ShieldSelected, Is.False);
            Assert.That(_controller.ShieldToggleButton.interactable, Is.False);
            Assert.That(_controller.ShieldToggleText.text,
                Is.EqualTo("SHIELD x0: OFF"));
        }

        [TestCase(6)]
        [TestCase(7)]
        public void ProvidedItem_StartsWithoutProductInventory(int stageNumber)
        {
            _store.HighestUnlocked = 7;
            _controller.SetProgressStoreForTests(_store);
            _controller.SetStartItemInventoryForTests(
                new UnavailableStartItemInventoryGateway());
            _controller.SelectStage(stageNumber);

            _controller.StartSelectedStage();
            _controller.Tick(3.1f);

            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.Playing));
            Assert.That(
                stageNumber == 6
                    ? _controller.Session.ShieldActive
                    : _controller.Session.BoosterActive,
                Is.True);
        }

        [Test]
        public void MissingProductSession_CannotGrantFreeSelectableItem()
        {
            _store.HighestUnlocked = 8;
            _controller.SetProgressStoreForTests(_store);
            _controller.SetStartItemInventoryForTests(
                new UnavailableStartItemInventoryGateway());
            _controller.SelectStage(8);

            _controller.ToggleShieldSelection();
            _controller.ToggleBoosterSelection();

            Assert.That(_controller.ShieldSelected, Is.False);
            Assert.That(_controller.BoosterSelected, Is.False);
            Assert.That(_controller.ShieldToggleButton.interactable, Is.False);
            Assert.That(_controller.BoosterToggleButton.interactable, Is.False);
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
            Assert.That(_controller.GetColorTile(0).transform.localScale.x,
                Is.GreaterThan(_controller.GetColorTile(1).transform.localScale.x));
            Assert.That(_controller.GetNextColorMarker(1).activeSelf, Is.True);
            _controller.HandleGameplayTap();
            _controller.Tick(0.13f);
            Assert.That(_controller.GetColorTile(1).transform.localScale.x,
                Is.GreaterThan(_controller.GetColorTile(0).transform.localScale.x));
            Assert.That(_controller.GetNextColorMarker(0).activeSelf, Is.True);
        }

        [Test]
        public void ThreeColorHud_ShowsFullOrder()
        {
            _store.HighestUnlocked = 5;
            _controller.SetProgressStoreForTests(_store);
            _controller.SelectStage(5);
            _controller.StartSelectedStage();
            _controller.Tick(3.1f);
            Assert.That(_controller.ActiveColorTileCount, Is.EqualTo(3));
            Assert.That(_controller.GetColorTileSymbol(2).text, Is.EqualTo("▲"));
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
            Vector3 baselineOffset =
                _controller.NormalCameraPosition -
                new Vector3(0f, 1f, 0f);
            _controller.Session.Advance(
                1f,
                _controller.Session.Stage.BoosterDistance + 1f);
            _controller.Session.Advance(StageSession.BoosterExitDuration, 0f);
            _controller.Tick(0.36f);
            Assert.That(_controller.GameplayCamera.fieldOfView,
                Is.EqualTo(60f).Within(0.001f));
            Assert.That(
                Quaternion.Angle(
                    _controller.NormalCameraRotation,
                    _controller.GameplayCamera.transform.rotation),
                Is.LessThan(0.001f));
            Assert.That(
                Vector3.Distance(
                    _controller.GameplayCamera.transform.position -
                    _controller.PlayerTransform.position,
                    baselineOffset),
                Is.LessThan(0.001f));
        }

        [Test]
        public void Lobby_VerticalDecorationsAreAbsent()
        {
            Assert.That(CountNamed("LobbyTier_0_Accent_0"), Is.Zero);
        }

        [Test]
        public void VerticalStack_CurrentColorStaysAtTop()
        {
            StartPlaying(false, false);
            float redY = ((RectTransform)_controller.GetColorTile(0).transform)
                .anchoredPosition.y;
            float blueY = ((RectTransform)_controller.GetColorTile(1).transform)
                .anchoredPosition.y;
            Assert.That(redY, Is.GreaterThan(blueY));

            _controller.HandleGameplayTap();
            _controller.Tick(0.13f);
            redY = ((RectTransform)_controller.GetColorTile(0).transform)
                .anchoredPosition.y;
            blueY = ((RectTransform)_controller.GetColorTile(1).transform)
                .anchoredPosition.y;
            Assert.That(blueY, Is.GreaterThan(redY));
        }

        [Test]
        public void VerticalStack_RapidTapsRetargetToAuthoritativeColor()
        {
            StartPlaying(false, false);
            _controller.HandleGameplayTap();
            _controller.Tick(0.03f);
            _controller.HandleGameplayTap();
            _controller.Tick(0.03f);
            _controller.HandleGameplayTap();
            _controller.Tick(0.13f);

            Assert.That(_controller.Session.CurrentColor,
                Is.EqualTo(RunnerColor.Blue));
            Assert.That(
                ((RectTransform)_controller.GetColorTile(1).transform)
                    .anchoredPosition.y,
                Is.GreaterThan(
                    ((RectTransform)_controller.GetColorTile(0).transform)
                        .anchoredPosition.y));
        }

        [Test]
        public void BoosterCamera_LowersAndMovesCloserBehindPlayer()
        {
            StartPlaying(false, true);
            Vector3 offset =
                _controller.GameplayCamera.transform.position -
                _controller.PlayerTransform.position;

            Assert.That(offset.y, Is.LessThan(7f));
            Assert.That(offset.z, Is.GreaterThan(-10f));
            Assert.That(_controller.GameplayCamera.fieldOfView,
                Is.GreaterThan(60f));
        }

        [Test]
        public void ResetProgress_CancellationChangesNothing()
        {
            _store.HighestUnlocked = 4;
            _store.Records[1] = new StageRecord(true, 12f, 13f, 2);
            _controller.SetProgressStoreForTests(_store);

            _controller.RequestProgressReset();
            _controller.CancelProgressReset();

            Assert.That(_store.HighestUnlocked, Is.EqualTo(4));
            Assert.That(_store.Records[1].Cleared, Is.True);
            Assert.That(_controller.ResetProgressConfirmation.activeSelf,
                Is.False);
        }

        [Test]
        public void ResetProgress_ConfirmationReturnsLobbyToStageOne()
        {
            _store.HighestUnlocked = 5;
            _store.Records[0] = new StageRecord(true, 12f, 13f, 2);
            _store.Records[1] = new StageRecord(true, 12f, 13f, 2);
            _controller.SetProgressStoreForTests(_store);

            _controller.RequestProgressReset();
            _controller.ConfirmProgressReset();

            Assert.That(_store.HighestUnlocked, Is.EqualTo(1));
            Assert.That(_controller.SelectedStageNumber, Is.EqualTo(1));
            Assert.That(_controller.LobbyTier, Is.Zero);
            Assert.That(_store.Records[0].Cleared, Is.False);
            Assert.That(_store.UnrelatedSetting, Is.EqualTo(37));
        }

        [Test]
        public void VerticalStack_HasSixReusableSlots()
        {
            for (int index = 0; index < 6; index++)
            {
                Assert.That(_controller.GetColorTile(index), Is.Not.Null);
                Assert.That(_controller.GetColorTileSymbol(index), Is.Not.Null);
            }
        }

        [Test]
        public void ExperimentLauncher_IsHiddenFromNormalFlow()
        {
            ExperimentLauncher launcher = FindExperimentLauncher();

            Assert.That(launcher, Is.Not.Null);
            Assert.That(launcher.gameObject.activeInHierarchy, Is.False);
            Assert.That(_controller.DevelopmentDebugRoot.activeSelf, Is.False);
        }

        [Test]
        public void ExperimentPlay_DoesNotChangeNormalProgression()
        {
            _store.HighestUnlocked = 4;
            _store.Records[1] = new StageRecord(true, 15f, 16f, 2);
            _controller.SetProgressStoreForTests(_store);
            ExperimentLauncher launcher = FindExperimentLauncher();
            launcher.NextColorCount();
            launcher.NextMechanic();
            launcher.StartExperiment();

            Assert.That(launcher.Session, Is.Not.Null);
            Assert.That(launcher.ColorCount, Is.EqualTo(4));
            Assert.That(launcher.Mechanic,
                Is.EqualTo(MechanicExperimentType.Camouflage));
            Assert.That(_store.HighestUnlocked, Is.EqualTo(4));
            Assert.That(_store.Records[1].ClearCount, Is.EqualTo(2));
        }

        [Test]
        public void ExperimentLauncher_StartsCountdownWithFixedPoolSession()
        {
            ExperimentLauncher launcher = FindExperimentLauncher();
            launcher.StartExperiment();

            Assert.That(_controller.ExperimentActive, Is.True);
            Assert.That(_controller.ExperimentSession, Is.Not.Null);
            Assert.That(_controller.ExperimentSession.FlowState,
                Is.EqualTo(StageFlowState.Countdown));
            Assert.That(_controller.GameplayHudRoot.activeSelf, Is.True);
            Assert.That(_controller.ActiveGateCount, Is.EqualTo(6));
            Assert.That(_controller.GatePoolSize, Is.EqualTo(6));
            Assert.That(_controller.TrackPool.SegmentCount, Is.EqualTo(6));
        }

        [Test]
        public void ExperimentColorStack_SupportsThreeThroughSixColors()
        {
            ExperimentLauncher launcher = FindExperimentLauncher();
            for (int count = 3; count <= 6; count++)
            {
                launcher.StartExperiment();
                Assert.That(_controller.ActiveColorTileCount,
                    Is.EqualTo(count));
                launcher.LeaveExperiment();
                if (count < 6)
                {
                    launcher.NextColorCount();
                }
            }
        }

        [Test]
        public void ExperimentRapidTaps_EndAtCorrectCurrentTile()
        {
            ExperimentLauncher launcher = FindExperimentLauncher();
            launcher.PreviousColorCount();
            launcher.StartExperiment();
            _controller.Tick(3.1f);
            for (int index = 0; index < 5; index++)
            {
                _controller.HandleGameplayTap();
                _controller.Tick(0.02f);
            }
            _controller.Tick(0.13f);

            Assert.That(_controller.ExperimentSession.CurrentColor,
                Is.EqualTo(RunnerColor.Cyan));
            Assert.That(
                ((RectTransform)_controller.GetColorTile(5).transform)
                    .anchoredPosition.y,
                Is.GreaterThan(
                    ((RectTransform)_controller.GetColorTile(0).transform)
                        .anchoredPosition.y));
        }

        [Test]
        public void IceGatePositions_RemainContinuousDuringMovement()
        {
            ExperimentLauncher launcher = FindExperimentLauncher();
            launcher.NextColorCount();
            launcher.NextMechanic();
            launcher.NextMechanic();
            launcher.NextMechanic();
            launcher.StartExperiment();
            Vector3[] positions = new Vector3[_controller.GatePoolSize];
            for (int index = 0; index < positions.Length; index++)
            {
                positions[index] = _controller.GetGate(index).transform.position;
            }

            _controller.Tick(0.1f);

            for (int index = 0; index < positions.Length; index++)
            {
                Assert.That(_controller.GetGate(index).transform.position,
                    Is.EqualTo(positions[index]));
            }
            Assert.That(_controller.GatePoolSize, Is.EqualTo(6));
        }

        [Test]
        public void Camouflage_RemainsNeutralThenRevealsWithoutMoving()
        {
            ExperimentDefinition definition = ExperimentCatalog.Get(
                4,
                MechanicExperimentType.Camouflage);
            DeterministicExperimentGateSequence sequence =
                new DeterministicExperimentGateSequence(definition);
            ExperimentGatePlan plan = default;
            for (int index = 0; index < definition.GateCount; index++)
            {
                plan = sequence.GetPlan(index);
                if (plan.IsCamouflage)
                {
                    break;
                }
            }
            StageGateView gate = _controller.GetGate(0);
            Vector3 position = new Vector3(0f, 1f, 50f);
            Material neutral =
                _controller.TrackPool.GetSegment(0).SurfaceMaterial;
            gate.ActivateExperiment(
                plan,
                _controller.GetPresentationMaterial(plan.Color),
                neutral,
                position.z,
                plan.GateIndex - 2,
                definition.Camouflage.RevealLeadTimeSeconds + 1f,
                definition.Camouflage,
                definition.Hidden,
                definition.Flicker,
                0f);
            Vector3 before = gate.transform.position;
            Assert.That(gate.SymbolVisible, Is.False);

            gate.UpdateExperimentVisibility(
                plan.GateIndex - 1,
                definition.Camouflage.RevealLeadTimeSeconds,
                neutral,
                definition.Camouflage,
                definition.Hidden,
                definition.Flicker,
                0f,
                definition.Camouflage.RevealTransitionSeconds);

            Assert.That(gate.SymbolVisible, Is.True);
            Assert.That(gate.transform.position, Is.EqualTo(before));
            Assert.That(gate.ReactionActive, Is.True);

            gate.UpdateExperimentVisibility(
                plan.GateIndex - 2,
                definition.Camouflage.RevealLeadTimeSeconds + 5f,
                neutral,
                definition.Camouflage,
                definition.Hidden,
                definition.Flicker,
                0f,
                0f);
            Assert.That(
                gate.SymbolVisible,
                Is.True,
                "Reveal remains latched when ETA later changes.");
        }

        [Test]
        public void Fog_KeepsExactlyTwoGatesFullyReadableWithoutPoolGrowth()
        {
            int originalCount = _controller.GatePoolSize;
            ExperimentDefinition definition = ExperimentCatalog.Get(
                4,
                MechanicExperimentType.Fog);
            DeterministicExperimentGateSequence sequence =
                new DeterministicExperimentGateSequence(definition);
            ExperimentGatePlan[] plans =
                new ExperimentGatePlan[definition.FogStartGate + 4];
            for (int index = 0; index < plans.Length; index++)
            {
                plans[index] = sequence.GetPlan(index);
            }
            Material neutral =
                _controller.TrackPool.GetSegment(0).SurfaceMaterial;
            int visible = 0;
            for (int offset = 0; offset < 4; offset++)
            {
                ExperimentGatePlan plan =
                    plans[definition.FogStartGate + offset];
                StageGateView gate = _controller.GetGate(offset);
                gate.ActivateExperiment(
                    plan,
                    _controller.GetPresentationMaterial(plan.Color),
                    neutral,
                    50f + offset * 20f,
                    definition.FogStartGate,
                    float.PositiveInfinity,
                    definition.Camouflage,
                    definition.Hidden,
                    definition.Flicker,
                    0f);
                visible += gate.SymbolVisible ? 1 : 0;
            }

            Assert.That(visible, Is.EqualTo(2));
            Assert.That(_controller.GatePoolSize, Is.EqualTo(originalCount));
        }

        [Test]
        public void Ice_ChangesFloorPresentationAndKeepsColorJudgment()
        {
            Material normal =
                _controller.TrackPool.GetSegment(0).SurfaceMaterial;
            Material ice =
                _controller.GetPresentationMaterial(RunnerColor.Cyan);
            _controller.TrackPool.SetSurfaceMaterial(ice);
            for (int index = 0;
                index < _controller.TrackPool.SegmentCount;
                index++)
            {
                Assert.That(
                    _controller.TrackPool.GetSegment(index).SurfaceMaterial,
                    Is.SameAs(ice));
            }

            ExperimentDefinition definition = ExperimentCatalog.Get(
                3,
                MechanicExperimentType.Ice);
            ExperimentSession session = new ExperimentSession(definition);
            session.CompleteCountdown();
            ExperimentGatePlan plan = default;
            for (int index = 0; index <= definition.IceStartGate; index++)
            {
                plan = session.GetNextPlan();
                while (session.CurrentColor != plan.Color)
                {
                    session.TryCycleColor();
                }
                if (index < definition.IceStartGate)
                {
                    session.Resolve(plan);
                }
            }
            Assert.That(session.GetSpeedForPlan(plan),
                Is.GreaterThan(session.CurrentSpeed));
            session.TryCycleColor();
            Assert.That(session.Resolve(plan), Is.False);
            _controller.TrackPool.SetSurfaceMaterial(normal);
        }

        [Test]
        public void RepeatedExperimentStarts_CreateNoDuplicates()
        {
            ExperimentLauncher launcher = FindExperimentLauncher();
            for (int index = 0; index < 30; index++)
            {
                launcher.StartExperiment();
                launcher.LeaveExperiment();
            }

            Assert.That(
                Object.FindObjectsByType<ExperimentLauncher>(
                    FindObjectsInactive.Include).Length,
                Is.EqualTo(1));
            Assert.That(_controller.GatePoolSize, Is.EqualTo(6));
            Assert.That(_controller.TrackPool.SegmentCount, Is.EqualTo(6));
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
            _controller.Session.Advance(
                0f,
                _controller.Session.Stage.BoosterDistance - 1f);
            _controller.Tick(
                1.01f / _controller.Session.Stage.BoosterSpeed);
            StageGateView first = FindGateByPlanIndex(
                _controller.Session.GatesPassed);
            Assert.That(first, Is.Not.Null);
            Assert.That(first.AssignedColor,
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
            Assert.That(CountNamed("TicketContinueButton"), Is.EqualTo(1));
            Assert.That(CountNamed("CoinContinueButton"), Is.EqualTo(1));
            Assert.That(CountNamed("RewardedContinueButton"), Is.EqualTo(1));
            Assert.That(CountNamed("InsufficientCoinsPopup"), Is.EqualTo(1));
            Assert.That(CountNamed("InsufficientCoinsCloseButton"), Is.EqualTo(1));
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
            _controller.RequestCoinContinue();
            Assert.That(_controller.ItemPanel.activeSelf, Is.False);
            Assert.That(_controller.CountdownPanel.activeSelf, Is.True);
            Assert.That(CountNamed("CountdownPanel"), Is.EqualTo(1));
        }

        [Test]
        public void Continue_GrantsTemporaryProtection()
        {
            StartPlaying(false, false);
            Fail();
            _controller.RequestCoinContinue();
            _controller.Tick(3.1f);
            Assert.That(_controller.Session.ContinueProtectionActive, Is.True);
            Assert.That(_controller.Session.SafeGateCountRemaining, Is.EqualTo(2));
        }

        [Test]
        public void SecondFailure_StillOffersContinueBelowCap()
        {
            StartPlaying(false, false);
            Fail();
            _controller.RequestCoinContinue();
            _controller.Tick(3.1f);
            ResolveSafeGates();
            _controller.Session.Advance(1.1f, 0f);
            Fail();
            _controller.Tick(1.1f);
            Assert.That(_controller.CoinContinueButton.gameObject.activeSelf, Is.True);
            Assert.That(_controller.RewardedContinueButton.gameObject.activeSelf, Is.True);
        }

        [Test]
        public void ContinueSources_AdThenCoinKeepsFirstCoinPrice()
        {
            var economy = new ContinueEconomyTestGateway(5000);
            var ads = new RewardedAdTestService();
            _controller.SetContinueServicesForTests(economy, ads);
            StartPlaying(false, false);
            Fail();

            _controller.RequestRewardedContinue();
            Assert.That(_controller.Session.ContinueUseCount, Is.EqualTo(1));
            FailAfterContinue();
            _controller.RequestCoinContinue();

            Assert.That(economy.TotalSpent, Is.EqualTo(900));
            Assert.That(_controller.Session.ContinueUseCount, Is.EqualTo(2));
        }

        [Test]
        public void ContinueTicket_IsOfferedFirstAndSpentWithoutCoinOrAd()
        {
            var economy = new ContinueEconomyTestGateway(
                coinBalance: 2000,
                continueTicketCount: 1);
            var ads = new RewardedAdTestService();
            _controller.SetContinueServicesForTests(economy, ads);
            StartPlaying(false, false);
            Fail();
            _controller.Tick(1.1f);

            Assert.That(_controller.TicketContinueButton.gameObject.activeSelf,
                Is.True);
            Assert.That(_controller.TicketContinueButton.GetComponentInChildren<Text>().text,
                Is.EqualTo("CONTINUE TICKET x1"));
            _controller.RequestTicketContinue();

            Assert.That(economy.TicketSpendCount, Is.EqualTo(1));
            Assert.That(economy.ContinueTicketCount, Is.Zero);
            Assert.That(economy.TotalSpent, Is.Zero);
            Assert.That(ads.ShowCount, Is.Zero);
            Assert.That(_controller.Session.ContinueUseCount, Is.EqualTo(1));
        }

        [Test]
        public void ContinueTicketSaveFailure_KeepsFailureFrozen()
        {
            var economy = new ContinueEconomyTestGateway(
                continueTicketCount: 1)
            {
                FailureCode = ProductErrorCode.SaveWrite
            };
            _controller.SetContinueServicesForTests(
                economy,
                new RewardedAdTestService());
            StartPlaying(false, false);
            Fail();
            Vector3 position = _controller.PlayerTransform.position;

            _controller.RequestTicketContinue();

            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.Failed));
            Assert.That(_controller.Session.ContinueUseCount, Is.Zero);
            Assert.That(economy.ContinueTicketCount, Is.EqualTo(1));
            Assert.That(_controller.PlayerTransform.position, Is.EqualTo(position));
            Assert.That(_controller.FailContinueStatusText.text,
                Does.Contain("CONTINUE SAVE FAILED"));
        }

        [Test]
        public void ContinueSources_CoinAdCoinEscalateWithoutTotalCap()
        {
            var economy = new ContinueEconomyTestGateway(20000);
            var ads = new RewardedAdTestService();
            _controller.SetContinueServicesForTests(economy, ads);
            StartPlaying(false, false);
            Fail();

            _controller.RequestCoinContinue();
            FailAfterContinue();
            _controller.RequestRewardedContinue();
            FailAfterContinue();
            _controller.RequestCoinContinue();
            FailAfterContinue();
            _controller.Tick(1.1f);

            Assert.That(economy.TotalSpent, Is.EqualTo(2800));
            Assert.That(_controller.Session.ContinueUseCount, Is.EqualTo(3));
            Assert.That(_controller.CoinContinueButton.gameObject.activeSelf,
                Is.True);
            Assert.That(_controller.TicketContinueButton.gameObject.activeSelf,
                Is.False);
            Assert.That(_controller.RewardedContinueButton.gameObject.activeSelf,
                Is.False);
            Assert.That(_controller.CoinContinueButton
                .GetComponentInChildren<Text>().text,
                Is.EqualTo("CONTINUE 2900 COINS"));
        }

        [TestCase(ProductErrorCode.InsufficientFunds, "NOT ENOUGH COINS")]
        [TestCase(ProductErrorCode.SaveWrite, "CONTINUE SAVE FAILED")]
        public void CoinContinueFailure_KeepsFailureFrozen(
            ProductErrorCode failure,
            string expectedStatus)
        {
            var economy = new ContinueEconomyTestGateway
            {
                FailureCode = failure
            };
            _controller.SetContinueServicesForTests(
                economy,
                new RewardedAdTestService());
            StartPlaying(false, false);
            Fail();
            Vector3 position = _controller.PlayerTransform.position;

            _controller.RequestCoinContinue();

            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.Failed));
            Assert.That(_controller.Session.ContinueUseCount, Is.Zero);
            Assert.That(_controller.PlayerTransform.position, Is.EqualTo(position));
            Assert.That(_controller.FailContinueStatusText.text,
                Does.Contain(expectedStatus));
        }

        [Test]
        public void RewardedFailureAndStaleDuplicateCallbackAreOneShot()
        {
            var ads = new RewardedAdTestService
            {
                CompleteImmediately = false
            };
            _controller.SetContinueServicesForTests(
                new ContinueEconomyTestGateway(),
                ads);
            StartPlaying(false, false);
            Fail();
            Vector3 position = _controller.PlayerTransform.position;

            _controller.RequestRewardedContinue();
            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.Failed));
            Assert.That(_controller.PlayerTransform.position, Is.EqualTo(position));
            ads.Complete(RewardedAdResult.Failed);
            Assert.That(_controller.Session.ContinueUseCount, Is.Zero);
            Assert.That(_controller.FailContinueStatusText.text,
                Does.Contain("AD FAILED"));

            _controller.RequestRewardedContinue();
            ads.Complete(RewardedAdResult.Completed);
            Assert.That(_controller.Session.ContinueUseCount, Is.EqualTo(1));
            ads.RepeatLast(RewardedAdResult.Completed);
            Assert.That(_controller.Session.ContinueUseCount, Is.EqualTo(1));
        }

        [Test]
        public void RewardedCallbackAfterRetryCannotResumeNewAttempt()
        {
            var ads = new RewardedAdTestService
            {
                CompleteImmediately = false
            };
            _controller.SetContinueServicesForTests(
                new ContinueEconomyTestGateway(),
                ads);
            StartPlaying(false, false);
            Fail();
            _controller.RequestRewardedContinue();

            _controller.RetryToItemSelection();
            ads.Complete(RewardedAdResult.Completed);

            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.PreRunSelection));
            Assert.That(_controller.Session.ContinueUseCount, Is.Zero);
        }

        [Test]
        public void ContinueRequestsBeforeFailureDoNotSpendOrShowAd()
        {
            var economy = new ContinueEconomyTestGateway();
            var ads = new RewardedAdTestService();
            _controller.SetContinueServicesForTests(economy, ads);
            StartPlaying(false, false);

            _controller.RequestCoinContinue();
            _controller.RequestRewardedContinue();

            Assert.That(economy.SpendCount, Is.Zero);
            Assert.That(ads.ShowCount, Is.Zero);
            Assert.That(_controller.Session.ContinueUseCount, Is.Zero);
        }

        [Test]
        public void UnavailableProvidersHideContinueOffersButKeepRetry()
        {
            _controller.SetContinueServicesForTests(
                new UnavailableContinueEconomyGateway(),
                new UnavailableRewardedAdService());
            StartPlaying(false, false);
            Fail();
            _controller.Tick(1.1f);

            Assert.That(_controller.CoinContinueButton.gameObject.activeSelf,
                Is.False);
            Assert.That(_controller.RewardedContinueButton.gameObject.activeSelf,
                Is.False);
            Assert.That(_controller.RetryButton.gameObject.activeSelf, Is.True);
            Assert.That(_controller.FailContinueStatusText.text,
                Is.EqualTo("COINS --  HEARTS --"));
        }

        [Test]
        public void InsufficientCoins_OpensTruthfulPopupAndKeepsFailureFrozen()
        {
            var economy = new ContinueEconomyTestGateway(100);
            _controller.SetContinueServicesForTests(
                economy,
                new UnavailableRewardedAdService());
            StartPlaying(false, false);
            Fail();
            Vector3 position = _controller.PlayerTransform.position;

            _controller.Tick(1.1f);
            Assert.That(_controller.CoinContinueButton.interactable, Is.True);
            _controller.RequestCoinContinue();

            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.Failed));
            Assert.That(_controller.Session.ContinueUseCount, Is.Zero);
            Assert.That(economy.TotalSpent, Is.Zero);
            Assert.That(_controller.PlayerTransform.position,
                Is.EqualTo(position));
            Assert.That(_controller.InsufficientCoinsPopup.activeSelf, Is.True);
            Assert.That(_controller.FailContinueStatusText.text,
                Does.Contain("COINS 100"));
            Assert.That(_controller.FailContinueStatusText.text,
                Does.Contain("HEARTS 5/5"));

            _controller.CloseInsufficientCoinsPopup();
            Assert.That(_controller.InsufficientCoinsPopup.activeSelf, Is.False);
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
            _controller.RequestCoinContinue();
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

        [Test]
        public void CampaignClear_HidesReplayAndNextOpensPreRunDirectly()
        {
            ClearStage();
            _controller.Tick(1.3f);

            Assert.That(_controller.ReplayButton.gameObject.activeSelf, Is.False);
            Assert.That(_controller.ClearContinueButton.gameObject.activeSelf,
                Is.True);
            _controller.OpenNextStagePreRun();

            Assert.That(_controller.SelectedStageNumber, Is.EqualTo(2));
            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.PreRunSelection));
            Assert.That(_controller.ItemPanel.activeSelf, Is.True);
            Assert.That(_controller.LobbyPanel.activeSelf, Is.False);
            Assert.That(_controller.Goal.activeSelf, Is.False);
        }

        [Test]
        public void LobbyPlay_UsesStableIdOfDisplayedProgressionStage()
        {
            _store.HighestUnlocked = 2;
            _store.Records[0] =
                new StageRecord(true, 30f, 32f, 1);
            _controller.SetProgressStoreForTests(_store);

            Assert.That(_controller.SelectedStageNumber, Is.EqualTo(2));
            _controller.PlayFromLobby();
            Assert.That(
                _controller.Session.Stage.DisplayNumber,
                Is.EqualTo(2));

            _store.HighestUnlocked = 3;
            _store.Records[1] =
                new StageRecord(true, 31f, 33f, 1);
            _controller.SetProgressStoreForTests(_store);

            Assert.That(_controller.SelectedStageNumber, Is.EqualTo(3));
            _controller.PlayFromLobby();
            Assert.That(
                _controller.Session.Stage.DisplayNumber,
                Is.EqualTo(3));
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
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        [TestCase(10)]
        [TestCase(11)]
        [TestCase(12)]
        [TestCase(13)]
        [TestCase(14)]
        [TestCase(15)]
        [TestCase(16)]
        [TestCase(17)]
        [TestCase(18)]
        [TestCase(19)]
        [TestCase(20)]
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

                bool stageShield =
                    _controller.Session.StageProvidesShield;
                bool stageBooster =
                    _controller.Session.StageProvidesBooster;
                Assert.That(_controller.Session.Items.Shield,
                    Is.EqualTo(
                        (mask & 1) != 0 &&
                        _controller.Session.Stage.ShieldAllowed &&
                        !stageShield));
                Assert.That(_controller.Session.Items.Booster,
                    Is.EqualTo(
                        (mask & 2) != 0 &&
                        _controller.Session.Stage.BoosterAllowed &&
                        !stageBooster));
                Assert.That(_controller.Session.FlowState,
                    Is.EqualTo(StageFlowState.Countdown));

                _controller.Tick(3.1f);
                Assert.That(_controller.Session.FlowState,
                    Is.EqualTo(StageFlowState.Playing));
                Assert.That(_controller.Session.ShieldActive,
                    Is.EqualTo(
                        ((mask & 1) != 0 &&
                         _controller.Session.Stage.ShieldAllowed) ||
                        stageShield));
                Assert.That(_controller.Session.BoosterActive,
                    Is.EqualTo(
                        ((mask & 2) != 0 &&
                         _controller.Session.Stage.BoosterAllowed &&
                         !stageBooster) ||
                        stageBooster));
                Assert.That(_controller.ShieldVisual.activeSelf,
                    Is.EqualTo(
                        ((mask & 1) != 0 &&
                         _controller.Session.Stage.ShieldAllowed) ||
                        stageShield));
                Assert.That(_controller.GatePoolSize, Is.EqualTo(6));
            }
        }

        [Test]
        public void StageSeven_StartBoosterCrossesEveryGateAndReachesGoal()
        {
            _store.HighestUnlocked = 7;
            _controller.SetProgressStoreForTests(_store);
            _controller.SelectStage(7);

            Assert.That(
                _controller.BoosterToggleText.text,
                Is.EqualTo("BOOSTER: PROVIDED"));
            Assert.That(
                _controller.BoosterToggleButton.interactable,
                Is.False);

            _controller.StartSelectedStage();
            _controller.Tick(3.1f);
            Assert.That(_controller.Session.BoosterActive, Is.True);
            Assert.That(
                _controller.Session.CurrentSpeed,
                Is.EqualTo(_controller.Session.Stage.BoosterSpeed));

            while (_controller.Session.GatesPassed <
                _controller.Session.Stage.TargetGateCount)
            {
                StageGateView gate = FindGateByPlanIndex(
                    _controller.Session.GatesPassed);
                Assert.That(gate, Is.Not.Null);
                Match(gate.AssignedColor);
                float distanceToPastGate =
                    gate.transform.position.z -
                    _controller.PlayerTransform.position.z + 1f;
                float deltaTime = distanceToPastGate /
                    _controller.Session.GetSpeedForPlan(gate.ActivePlan);
                _controller.Tick(deltaTime);
            }

            Assert.That(
                _controller.Session.FlowState,
                Is.EqualTo(StageFlowState.StageFinishing));
            Assert.That(_controller.Goal.activeSelf, Is.True);

            float distanceToPastGoal =
                _controller.Goal.transform.position.z -
                _controller.PlayerTransform.position.z + 1f;
            _controller.TickMovement(
                distanceToPastGoal / _controller.Session.CurrentSpeed);

            Assert.That(
                _controller.Session.FlowState,
                Is.EqualTo(StageFlowState.StageCleared));
        }

        [Test]
        public void StageSelectUi_HasOneButtonPerCatalogEntry()
        {
            for (int index = 0; index < StageCatalog.Count; index++)
            {
                Assert.That(_controller.GetStageButton(index), Is.Not.Null);
            }
        }

        [Test]
        public void StageEight_IsCleanThreeColorItemApplicationRuntime()
        {
            _store.HighestUnlocked = StageCatalog.Count;
            _controller.SetProgressStoreForTests(_store);
            _controller.SelectStage(8);

            Assert.That(_controller.ShieldToggleButton.interactable, Is.True);
            Assert.That(_controller.BoosterToggleButton.interactable, Is.True);
            _controller.ToggleShieldSelection();
            _controller.ToggleBoosterSelection();
            _controller.StartSelectedStage();
            _controller.Tick(3.1f);

            Assert.That(_controller.Session.Stage.AllowedColorCount,
                Is.EqualTo(3));
            Assert.That(_controller.Session.Stage.PrimaryMechanic,
                Is.EqualTo(StagePrimaryMechanic.None));
            Assert.That(_controller.Session.Items.Shield, Is.True);
            Assert.That(_controller.Session.Items.Booster, Is.True);
            Assert.That(_controller.Session.ShieldActive, Is.True);
            Assert.That(_controller.Session.BoosterActive, Is.True);
            for (int gate = 0;
                gate < _controller.Session.Stage.TargetGateCount;
                gate++)
            {
                Assert.That(
                    _controller.Session.GetGatePlan(gate).Modifier.IsNone,
                    Is.True);
            }
        }

        [Test]
        public void CampaignCamouflageGate_RevealsFromEtaAndStaysJudged()
        {
            SelectAndStartStage(9);
            StageGateView camouflage = AdvanceToCampaignModifier(
                GateModifierType.Camouflage);
            Assert.That(camouflage, Is.Not.Null);
            Assert.That(camouflage.ActivePlan.Modifier.IsCamouflage, Is.True);
            Assert.That(camouflage.SymbolVisible, Is.False);

            camouflage.UpdateCampaignVisibility(
                _controller.Session.GatesPassed,
                0f,
                _controller.GetPresentationMaterial(RunnerColor.Red),
                _controller.Session.Stage.CamouflageSettings,
                _controller.Session.Stage.CamouflageSettings
                    .RevealTransitionSeconds);

            Assert.That(camouflage.SymbolVisible, Is.True);
            Assert.That(camouflage.CamouflageRevealProgress, Is.EqualTo(1f));
            Mismatch(camouflage.AssignedColor);
            Assert.That(camouflage.TryResolveCrossing(), Is.True);
            Assert.That(
                _controller.Session.FlowState,
                Is.EqualTo(StageFlowState.Failed));
        }

        [Test]
        public void CampaignEchoProvider_ActivatesColoredPlayerShell()
        {
            SelectAndStartStage(18);
            StageGateView provider = null;
            while (provider == null)
            {
                StageGateView current = FindGateByPlanIndex(
                    _controller.Session.GatesPassed);
                Assert.That(current, Is.Not.Null);
                if (current.ActivePlan.Modifier.IsEchoProvider)
                {
                    provider = current;
                    break;
                }

                Match(current.AssignedColor);
                current.TryResolveCrossing();
            }

            Match(provider.AssignedColor);
            Assert.That(provider.TryResolveCrossing(), Is.True);
            Assert.That(_controller.Session.EchoActive, Is.True);
            Assert.That(_controller.EchoShellVisual.activeSelf, Is.True);
        }

        [TestCase(12, GateModifierType.Fog)]
        [TestCase(15, GateModifierType.Ice)]
        public void CampaignMechanicIntro_BindsModifierToRuntimeGateView(
            int stageNumber,
            GateModifierType modifier)
        {
            SelectAndStartStage(stageNumber);
            Assert.That(_controller.Session.Stage.GateModifiers,
                Is.EqualTo(modifier));

            StageGateView gate = AdvanceToCampaignModifier(modifier);
            Assert.That(gate.ActivePlan.Modifier.Has(modifier), Is.True);
            Assert.That(gate.HasExperimentPlan, Is.False);
            Match(gate.AssignedColor);
            Assert.That(gate.TryResolveCrossing(), Is.True);
            Assert.That(_controller.Session.FlowState,
                Is.Not.EqualTo(StageFlowState.Failed));
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
        public void BuilderOwnedPreRun_ShowsConsumptionCopyAndOneStatusNode()
        {
            Assert.That(CountNamed("PreRunStatusText"), Is.EqualTo(1));
            Text instruction = FindTransform("ChooseItemsText")
                .GetComponent<Text>();
            Assert.That(instruction.text,
                Is.EqualTo("SELECT OWNED START ITEMS\n1 USED WHEN STARTING"));
            Assert.That(instruction.text, Does.Not.Contain("FREE"));
            Assert.That(instruction.text, Does.Not.Contain("UNLIMITED"));
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
            if (shield || booster)
            {
                _store.HighestUnlocked = 8;
                _controller.SetProgressStoreForTests(_store);
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
        }

        [Test]
        public void Lobby_ContainsNoGameplayHudElements()
        {
            Assert.That(_controller.LobbyRoot.activeSelf, Is.True);
            Assert.That(_controller.GameplayHudRoot.activeSelf, Is.False);
            Assert.That(_controller.CountdownRoot.activeSelf, Is.False);
            Assert.That(_controller.SpeedLines.isPlaying, Is.False);
        }

        [Test]
        public void Lobby_ContainsNoBaseOrUpgradeButtons()
        {
            Assert.That(CountNamed("LobbyTierBanner_0"), Is.Zero);
            Assert.That(CountNamed("LobbyTierBanner_1"), Is.Zero);
            Assert.That(CountNamed("LobbyTierBanner_2"), Is.Zero);
            Assert.That(CountNamed("LobbyTierBanner_3"), Is.Zero);
        }

        [Test]
        public void Lobby_HasOneDominantPlayButton()
        {
            Assert.That(CountNamed("LobbyPlayButton"), Is.EqualTo(1));
            RectTransform button = FindTransform("LobbyPlayButton")
                .GetComponent<RectTransform>();
            RectTransform lobby = _controller.LobbyPanel
                .GetComponent<RectTransform>();
            Assert.That(button.anchorMax.x - button.anchorMin.x,
                Is.GreaterThan(0.6f));
            Assert.That(lobby, Is.Not.Null);
        }

        [Test]
        public void Gameplay_HidesLobbyRoot()
        {
            StartPlaying(false, false);

            Assert.That(_controller.LobbyRoot.activeSelf, Is.False);
            AssertPrimaryFlow(MobileUiFlow.Gameplay);
        }

        [Test]
        public void Gameplay_ShowsOneColorHud()
        {
            StartPlaying(false, false);

            Assert.That(CountNamed("ColorHudPanel"), Is.EqualTo(1));
            Assert.That(FindTransform("ColorHudPanel").gameObject.activeInHierarchy,
                Is.True);
        }

        [Test]
        public void CurrentColorHud_UpdatesAfterTap()
        {
            StartPlaying(false, false);
            float redScale = _controller.GetColorTile(0).transform.localScale.x;

            _controller.HandleGameplayTap();
            _controller.Tick(0.13f);

            Assert.That(_controller.GetColorTile(0).transform.localScale.x,
                Is.LessThan(redScale));
            Assert.That(_controller.GetColorTile(1).transform.localScale.x,
                Is.GreaterThan(
                    _controller.GetColorTile(0).transform.localScale.x));
        }

        [Test]
        public void TwoColorStage_ShowsExactlyTwoColorTiles()
        {
            StartPlaying(false, false);
            Assert.That(_controller.ActiveColorTileCount, Is.EqualTo(2));
        }

        [Test]
        public void ThreeColorStage_ShowsExactlyThreeColorTiles()
        {
            _store.HighestUnlocked = 5;
            _controller.SetProgressStoreForTests(_store);
            _controller.SelectStage(5);
            _controller.StartSelectedStage();
            _controller.Tick(3.1f);

            Assert.That(_controller.ActiveColorTileCount, Is.EqualTo(3));
        }

        [Test]
        public void BoosterBar_IsHiddenBeforeBooster()
        {
            StartPlaying(false, false);
            Assert.That(_controller.BoosterMeterRoot.activeSelf, Is.False);
        }

        [Test]
        public void BoosterBar_IsVisibleOnlyDuringBooster()
        {
            StartPlaying(false, true);
            Assert.That(_controller.Session.BoosterActive, Is.True);
            Assert.That(_controller.BoosterMeterRoot.activeSelf, Is.True);

            _controller.Session.Advance(
                0.1f,
                _controller.Session.Stage.BoosterDistance + 1f);
            _controller.Tick(0f);

            Assert.That(_controller.Session.BoosterActive, Is.False);
            Assert.That(_controller.BoosterMeterRoot.activeSelf, Is.False);
        }

        [Test]
        public void BlueHorizontalCenterBar_NoLongerExists()
        {
            Transform meter = FindTransform("BoosterMeter");
            Assert.That(meter.IsChildOf(_controller.StageHud.transform), Is.True);
            RectTransform rect = meter.GetComponent<RectTransform>();
            Assert.That(rect.anchorMin.y, Is.GreaterThanOrEqualTo(0f));
            Assert.That(CountNamed("CurrentColorText"), Is.Zero);
            Assert.That(CountNamed("ColorCycleOrderText"), Is.Zero);
            Assert.That(CountNamed("NextColorText"), Is.Zero);
        }

        [Test]
        public void NormalGameplay_HasNoCentralSpeedLineObstruction()
        {
            StartPlaying(false, false);
            ParticleSystem[] systems =
                _controller.SpeedLines.GetComponentsInChildren<ParticleSystem>(true);

            for (int index = 0; index < systems.Length; index++)
            {
                Assert.That(systems[index].isPlaying, Is.False);
            }
            Assert.That(
                Mathf.Abs(FindTransform("BoosterSpeedLinesLeft").localPosition.x),
                Is.GreaterThanOrEqualTo(3f));
            Assert.That(
                Mathf.Abs(FindTransform("BoosterSpeedLinesRight").localPosition.x),
                Is.GreaterThanOrEqualTo(3f));
        }

        [Test]
        public void BoosterEffects_ClearAfterEnding()
        {
            StartPlaying(false, true);
            _controller.Session.Advance(
                0.1f,
                _controller.Session.Stage.BoosterDistance + 1f);
            _controller.Tick(1f);

            ParticleSystem[] systems =
                _controller.SpeedLines.GetComponentsInChildren<ParticleSystem>(true);
            for (int index = 0; index < systems.Length; index++)
            {
                Assert.That(systems[index].isPlaying, Is.False);
                Assert.That(systems[index].particleCount, Is.Zero);
            }
        }

        [Test]
        public void Continue_RestoresCorrectHudState()
        {
            StartPlaying(false, false);
            Fail();
            _controller.Tick(_controller.FailurePanelDelaySeconds + 0.1f);

            _controller.RequestCoinContinue();

            AssertPrimaryFlow(MobileUiFlow.Countdown);
            Assert.That(_controller.GameplayHudRoot.activeSelf, Is.True);
            Assert.That(_controller.CountdownRoot.activeSelf, Is.True);
        }

        [Test]
        public void Retry_RestoresCorrectHudState()
        {
            StartPlaying(false, false);
            Fail();

            _controller.RetryToItemSelection();

            AssertPrimaryFlow(MobileUiFlow.PreRun);
            Assert.That(_controller.GameplayHudRoot.activeSelf, Is.False);
            Assert.That(_controller.BoosterMeterRoot.activeSelf, Is.False);
        }

        [UnityTest]
        public IEnumerator ReferenceResolutions_PassOverlapBounds()
        {
            Vector2Int[] resolutions =
            {
                new Vector2Int(1080, 1920),
                new Vector2Int(1170, 2532),
                new Vector2Int(1080, 2400),
                new Vector2Int(1440, 3200)
            };
            for (int index = 0; index < resolutions.Length; index++)
            {
                Screen.SetResolution(
                    resolutions[index].x,
                    resolutions[index].y,
                    false);
                yield return null;
                Canvas.ForceUpdateCanvases();
                AssertNoOverlap("LobbyTitle", "LobbyCurrentStage");
                AssertNoOverlap("LobbyCurrentStage", "LobbyStageTitle");
                AssertNoOverlap("LobbyStageTitle", "LobbyStageDescription");
                AssertNoOverlap("LobbyStageDescription", "LobbyProgress");
                AssertNoOverlap("LobbyProgress", "LobbyPlayButton");
            }
        }

        [Test]
        public void PlayerUi_RemainsInsideSimulatedNotchSafeArea()
        {
            SafeAreaLayout layout = FindTransform("SafeAreaRoot")
                .GetComponent<SafeAreaLayout>();
            layout.ApplyForTests(
                new Rect(0f, 120f, 1080f, 2160f),
                1080,
                2400);
            Canvas.ForceUpdateCanvases();

            AssertInsideSafeArea(_controller.LobbyPanel.transform);
            AssertInsideSafeArea(FindTransform("LobbyPlayButton"));
            AssertInsideSafeArea(_controller.StageHud.transform);
            AssertInsideSafeArea(FindTransform("ColorHudPanel"));
            layout.ClearTestOverride();
        }

        [Test]
        public void SceneBuilderRerun_CreatesNoDuplicateRoots()
        {
            string[] roots =
            {
                "LobbyRoot",
                "PreRunRoot",
                "GameplayHudRoot",
                "CountdownRoot",
                "ClearResultRoot",
                "FailedResultRoot",
                "DevelopmentDebugRoot"
            };
            for (int index = 0; index < roots.Length; index++)
            {
                Assert.That(CountNamed(roots[index]), Is.EqualTo(1));
            }
            Assert.That(CountNamed("EventSystem"), Is.EqualTo(1));
            Assert.That(CountNamed("ColorHudPanel"), Is.EqualTo(1));
            Assert.That(CountNamed("BoosterMeter"), Is.EqualTo(1));
            Assert.That(CountNamed("PauseOverlayRoot"), Is.EqualTo(1));
            Assert.That(CountNamed("SettingsPanel"), Is.EqualTo(1));
            Assert.That(CountNamed("PreRunStatusText"), Is.EqualTo(1));
        }

        [Test]
        public void PauseDuringCountdown_FreezesCountdownUntilResume()
        {
            SelectItemsAndStart(false, false);
            _controller.RequestPause();

            _controller.Tick(4f);
            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.Countdown));
            Assert.That(_controller.PauseOverlayRoot.activeSelf, Is.True);
            Assert.That(_controller.PauseDim.color.a, Is.EqualTo(0.85f));

            _controller.RequestResume();
            _controller.Tick(3.1f);
            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.Playing));
        }

        [Test]
        public void PauseButton_IsVisibleClickableInsideSafeAreaAndDoesNotOverlapHud()
        {
            StartPlaying(false, false);
            Canvas.ForceUpdateCanvases();
            Button button = _controller.PauseButton;
            RectTransform buttonRect =
                button.GetComponent<RectTransform>();
            RectTransform safeArea = FindTransform("SafeAreaRoot")
                .GetComponent<RectTransform>();
            RectTransform stageHud = _controller.StageHud
                .GetComponent<RectTransform>();
            RectTransform shield = FindTransform("ShieldIcon")
                .GetComponent<RectTransform>();
            shield.gameObject.SetActive(true);
            Canvas.ForceUpdateCanvases();

            Assert.That(button.gameObject.activeInHierarchy, Is.True);
            Assert.That(buttonRect.rect.width, Is.GreaterThanOrEqualTo(44f));
            Assert.That(buttonRect.rect.height, Is.GreaterThanOrEqualTo(44f));
            Assert.That(IsInside(buttonRect, safeArea), Is.True);
            Assert.That(Overlaps(buttonRect, stageHud), Is.False);
            Assert.That(Overlaps(buttonRect, shield), Is.False);
            RunnerColor before = _controller.Session.CurrentColor;

            var click = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute<IPointerClickHandler>(
                button.gameObject,
                click,
                ExecuteEvents.pointerClickHandler);
            ExecuteEvents.Execute<IPointerClickHandler>(
                button.gameObject,
                click,
                ExecuteEvents.pointerClickHandler);

            Assert.That(_controller.PauseOverlayRoot.activeSelf, Is.True);
            Assert.That(_controller.PauseCoordinator.IsPaused, Is.True);
            Assert.That(_controller.Session.CurrentColor, Is.EqualTo(before));
            Assert.That(CountNamed("PauseOverlayRoot"), Is.EqualTo(1));
        }

        [Test]
        public void PauseDuringPlaying_FreezesTimeMovementInputAndJudgment()
        {
            StartPlaying(false, false);
            float elapsed = _controller.Session.ElapsedPlayingSeconds;
            Vector3 position = _controller.PlayerTransform.position;
            StageGateView gate = _controller.GetGate(0);
            Match(gate.AssignedColor);
            RunnerColor color = _controller.Session.CurrentColor;

            _controller.RequestPause();
            _controller.Tick(1f);
            _controller.HandleGameplayTap();

            Assert.That(_controller.Session.ElapsedPlayingSeconds,
                Is.EqualTo(elapsed));
            Assert.That(_controller.PlayerTransform.position,
                Is.EqualTo(position));
            Assert.That(_controller.Session.CurrentColor,
                Is.EqualTo(color));
            Assert.That(gate.TryResolveCrossing(), Is.False);
            Assert.That(gate.HasResolved, Is.False);

            _controller.RequestResume();
            Assert.That(gate.TryResolveCrossing(), Is.True);
        }

        [Test]
        public void Pause_OnlyPausesRegisteredAttemptEffectAndResumesOnce()
        {
            StartPlaying(false, true);
            Assert.That(_controller.SpeedLines.isPlaying, Is.True);

            _controller.RequestPause();
            _controller.RequestPause();
            Assert.That(_controller.SpeedLines.isPaused, Is.True);

            _controller.RequestResume();
            Assert.That(_controller.SpeedLines.isPlaying, Is.True);
        }

        private static bool IsInside(
            RectTransform child,
            RectTransform parent)
        {
            Rect childRect = WorldRect(child);
            Rect parentRect = WorldRect(parent);
            return parentRect.xMin <= childRect.xMin &&
                parentRect.xMax >= childRect.xMax &&
                parentRect.yMin <= childRect.yMin &&
                parentRect.yMax >= childRect.yMax;
        }

        private static bool Overlaps(
            RectTransform first,
            RectTransform second)
        {
            return WorldRect(first).Overlaps(WorldRect(second));
        }

        private static Rect WorldRect(RectTransform rect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return Rect.MinMaxRect(
                corners[0].x,
                corners[0].y,
                corners[2].x,
                corners[2].y);
        }

        [Test]
        public void PauseRestart_UsesExistingRetryPathAndClearsPause()
        {
            StartPlaying(false, false);
            _controller.RequestPause();
            _controller.RequestPauseRestart();
            _controller.ConfirmPauseModal();

            Assert.That(_controller.PauseCoordinator.IsPaused, Is.False);
            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.PreRunSelection));
            Assert.That(_controller.UiFlow,
                Is.EqualTo(MobileUiFlow.PreRun));
        }

        [Test]
        public void PauseLobbyLoadFailure_StaysPausedAndShowsError()
        {
            StartPlaying(false, false);
            var loader = new FailingSceneTransitionLoader();
            _controller.SetSceneTransitionLoaderForTests(loader);

            _controller.RequestPause();
            _controller.RequestPauseLobby();
            _controller.ConfirmPauseModal();

            Assert.That(loader.LoadCount, Is.EqualTo(1));
            Assert.That(loader.LastPath,
                Is.EqualTo(_controller.FrontendScenePath));
            Assert.That(_controller.PauseCoordinator.IsPaused, Is.True);
            Assert.That(_controller.PauseCoordinator.CurrentModal,
                Is.EqualTo(GameplayPauseModal.SceneLoadError));
            Assert.That(_controller.PauseOverlayRoot.activeSelf, Is.True);
        }

        [Test]
        public void FocusLossPausesOnceAndFocusGainDoesNotAutoResume()
        {
            StartPlaying(false, false);

            _controller.SendMessage("OnApplicationFocus", false);
            _controller.SendMessage("OnApplicationFocus", false);
            Assert.That(_controller.PauseCoordinator.IsPaused, Is.True);

            _controller.SendMessage("OnApplicationFocus", true);
            Assert.That(_controller.PauseCoordinator.IsPaused, Is.True);
        }

        [Test]
        public void GameplayBackPausesAndPauseBackResumesSameAttempt()
        {
            StartPlaying(false, false);
            StageSession session = _controller.Session;

            _controller.HandleBack();
            Assert.That(_controller.PauseCoordinator.IsPaused, Is.True);
            _controller.HandleBack();

            Assert.That(_controller.PauseCoordinator.IsPaused, Is.False);
            Assert.That(_controller.Session, Is.SameAs(session));
            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.Playing));
        }

        private void StartPlaying(bool shield, bool booster)
        {
            SelectItemsAndStart(shield, booster);
            _controller.Tick(3.1f);
            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.Playing));
        }

        private void SelectAndStartStage(int stageNumber)
        {
            _store.HighestUnlocked = StageCatalog.Count;
            _controller.SetProgressStoreForTests(_store);
            _controller.SelectStage(stageNumber);
            _controller.StartSelectedStage();
            _controller.Tick(3.1f);
            Assert.That(
                _controller.Session.FlowState,
                Is.EqualTo(StageFlowState.Playing));
        }

        private StageGateView AdvanceToCampaignModifier(
            GateModifierType modifier)
        {
            while (_controller.Session.GatesPassed <
                _controller.Session.Stage.TargetGateCount)
            {
                StageGateView current = FindGateByPlanIndex(
                    _controller.Session.GatesPassed);
                Assert.That(current, Is.Not.Null);
                if (current.ActivePlan.Modifier.Has(modifier))
                {
                    return current;
                }

                Match(current.AssignedColor);
                Assert.That(current.TryResolveCrossing(), Is.True);
            }

            Assert.Fail($"Campaign modifier {modifier} was not found.");
            return null;
        }

        private static ExperimentLauncher FindExperimentLauncher()
        {
            ExperimentLauncher[] launchers =
                Object.FindObjectsByType<ExperimentLauncher>(
                    FindObjectsInactive.Include);
            return launchers.Length == 0 ? null : launchers[0];
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
            StageGateView gate = FindGateByPlanIndex(
                _controller.Session.GatesPassed);
            Assert.That(gate, Is.Not.Null);
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
            while (_controller.Session.SafeGateCountRemaining > 0)
            {
                StageGateView gate = FindGateByPlanIndex(
                    _controller.Session.GatesPassed);
                Assert.That(gate, Is.Not.Null);
                Match(gate.AssignedColor);
                gate.TryResolveCrossing();
            }
        }

        private void FailAfterContinue()
        {
            _controller.Tick(3.1f);
            ResolveSafeGates();
            _controller.Session.Advance(1.1f, 0f);
            Fail();
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

        private void AssertPrimaryFlow(MobileUiFlow expected)
        {
            Assert.That(_controller.UiFlow, Is.EqualTo(expected));
            int activePrimaryRoots = 0;
            activePrimaryRoots += _controller.LobbyRoot.activeSelf ? 1 : 0;
            activePrimaryRoots += _controller.PreRunRoot.activeSelf ? 1 : 0;
            activePrimaryRoots += _controller.GameplayHudRoot.activeSelf ? 1 : 0;
            activePrimaryRoots += _controller.ClearResultRoot.activeSelf ? 1 : 0;
            activePrimaryRoots += _controller.FailedResultRoot.activeSelf ? 1 : 0;
            activePrimaryRoots += _controller.DevelopmentDebugRoot.activeSelf ? 1 : 0;
            Assert.That(activePrimaryRoots, Is.EqualTo(1));
        }

        private static Transform FindTransform(string value)
        {
            Transform[] transforms =
                Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);
            for (int index = 0; index < transforms.Length; index++)
            {
                if (transforms[index].name == value)
                {
                    return transforms[index];
                }
            }
            Assert.Fail($"Missing transform: {value}");
            return null;
        }

        private static void AssertNoOverlap(string firstName, string secondName)
        {
            Rect first = GetWorldRect(
                FindTransform(firstName).GetComponent<RectTransform>());
            Rect second = GetWorldRect(
                FindTransform(secondName).GetComponent<RectTransform>());
            Assert.That(
                first.Overlaps(second),
                Is.False,
                $"{firstName} overlaps {secondName}.");
        }

        private static void AssertInsideSafeArea(Transform child)
        {
            Rect safe = GetWorldRect(
                FindTransform("SafeAreaRoot").GetComponent<RectTransform>());
            Rect content = GetWorldRect(child.GetComponent<RectTransform>());
            const float tolerance = 1f;
            Assert.That(content.xMin, Is.GreaterThanOrEqualTo(safe.xMin - tolerance));
            Assert.That(content.xMax, Is.LessThanOrEqualTo(safe.xMax + tolerance));
            Assert.That(content.yMin, Is.GreaterThanOrEqualTo(safe.yMin - tolerance));
            Assert.That(content.yMax, Is.LessThanOrEqualTo(safe.yMax + tolerance));
        }

        private static Rect GetWorldRect(RectTransform rect)
        {
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return Rect.MinMaxRect(
                corners[0].x,
                corners[0].y,
                corners[2].x,
                corners[2].y);
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

        private StageGateView FindGateByPlanIndex(int planIndex)
        {
            for (int index = 0; index < _controller.GatePoolSize; index++)
            {
                StageGateView gate = _controller.GetGate(index);
                if (gate.gameObject.activeSelf &&
                    !gate.HasResolved &&
                    gate.PlanIndex == planIndex)
                {
                    return gate;
                }
            }
            return null;
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

        private sealed class FailingSceneTransitionLoader :
            ISceneTransitionLoader
        {
            internal int LoadCount { get; private set; }
            internal string LastPath { get; private set; }

            public void LoadScene(
                string scenePath,
                Action<SceneTransitionResult> completed)
            {
                LoadCount++;
                LastPath = scenePath;
                completed(SceneTransitionResult.Failure("PLANNED"));
            }
        }

        private sealed class InMemoryStageProgressStore : IStageProgressStore
        {
            internal int HighestUnlocked { get; set; } = 1;
            internal StageRecord[] Records { get; } =
                new StageRecord[StageCatalog.Count];
            internal int UnrelatedSetting { get; set; } = 37;

            public int LoadHighestUnlocked() => HighestUnlocked;
            public StageRecord LoadRecord(int stageNumber) =>
                Records[stageNumber - 1];
            public void SaveHighestUnlocked(int stageNumber) =>
                HighestUnlocked = stageNumber;
            public void SaveRecord(int stageNumber, StageRecord record) =>
                Records[stageNumber - 1] = record;

            public void ClearGameplayProgress()
            {
                HighestUnlocked = 1;
                System.Array.Clear(Records, 0, Records.Length);
            }
        }
    }
}
