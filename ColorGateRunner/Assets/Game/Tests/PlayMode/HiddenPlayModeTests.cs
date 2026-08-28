using System.Collections;
using ColorGateRunner.Core;
using ColorGateRunner.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ColorGateRunner.Tests.PlayMode
{
    public sealed class HiddenPlayModeTests
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
        }

        [Test]
        public void HiddenLauncher_DisablesBoosterAndUsesFixedPool()
        {
            _controller.OpenExperimentLab();
            ExperimentLauncher launcher = FindLauncher();
            launcher.ToggleBooster();
            Assert.That(launcher.Booster, Is.True);
            SelectHidden(launcher);

            Assert.That(launcher.Booster, Is.False);
            Assert.That(
                launcher.SelectedDefinition.Hidden.HideLeadTimeSeconds,
                Is.EqualTo(0.85f));
            Assert.That(launcher.Label, Does.Contain("BOOSTER DISABLED"));
            launcher.ToggleBooster();
            Assert.That(launcher.Booster, Is.False);

            launcher.StartExperiment();

            Assert.That(_controller.ExperimentActive, Is.True);
            Assert.That(
                _controller.ExperimentSession.Definition.Mechanic,
                Is.EqualTo(MechanicExperimentType.Hidden));
            Assert.That(_controller.ExperimentSession.Items.Booster, Is.False);
            Assert.That(_controller.GatePoolSize, Is.EqualTo(6));
        }

        [Test]
        public void Countdown_DoesNotAdvanceHiddenObservation()
        {
            StartHidden(
                CreateFastHiddenSettings(
                    revealDuration: 1f,
                    transitionDuration: 0.2f),
                shield: false);
            StageGateView gate = FindGate(0);

            _controller.Tick(1f);
            Assert.That(
                _controller.ExperimentSession.FlowState,
                Is.EqualTo(StageFlowState.Countdown));
            Assert.That(gate.HiddenVisibleElapsed, Is.Zero);
            Assert.That(gate.HiddenHideStarted, Is.False);

            _controller.Tick(2.1f);
            Assert.That(
                _controller.ExperimentSession.FlowState,
                Is.EqualTo(StageFlowState.Playing));
            Assert.That(gate.HiddenVisibleElapsed, Is.Zero);
            Assert.That(gate.HiddenHideStarted, Is.False);
        }

        [Test]
        public void Pause_FreezesHiddenObservationAndHideTransition()
        {
            StartHidden(
                CreateFastHiddenSettings(
                    revealDuration: 1f,
                    transitionDuration: 0.2f),
                shield: false);
            EnterPlaying();
            StageGateView gate = FindGate(0);
            _controller.Tick(0.4f);
            float visibleElapsed = gate.HiddenVisibleElapsed;
            float transition = gate.HiddenTransitionProgress;

            _controller.RequestPause();
            _controller.Tick(1f);

            Assert.That(gate.HiddenVisibleElapsed,
                Is.EqualTo(visibleElapsed));
            Assert.That(gate.HiddenTransitionProgress,
                Is.EqualTo(transition));
            Assert.That(gate.HiddenHideStarted, Is.False);
        }

        [Test]
        public void Hidden_HidesTargetOnlyAfterObservationAndEtaConditions()
        {
            HiddenSettings settings = CreateFastHiddenSettings(
                revealDuration: 1f,
                transitionDuration: 0.2f);
            StartHidden(settings, shield: false);
            EnterPlaying();
            StageGateView gate = FindGate(0);
            Vector3 position = gate.transform.position;
            RunnerColor targetColor = gate.AssignedColor;

            Assert.That(gate.SymbolVisible, Is.True);
            Assert.That(gate.MarkerVisible, Is.False);
            Assert.That(gate.SymbolText, Is.Empty);
            Assert.That(gate.HiddenTargetAlpha, Is.EqualTo(1f));

            _controller.Tick(0.99f);
            Assert.That(gate.HiddenHideStarted, Is.False);

            _controller.Tick(0.01f);
            Assert.That(gate.HiddenHideStarted, Is.True);
            Assert.That(gate.HiddenTransitionProgress, Is.GreaterThan(0f));
            Assert.That(gate.HiddenHideStartCount, Is.EqualTo(1));

            _controller.Tick(0.19f);
            Assert.That(gate.HiddenTransitionProgress, Is.EqualTo(1f));
            Assert.That(gate.HiddenTargetAlpha, Is.Zero);
            Assert.That(gate.SymbolVisible, Is.False);
            Assert.That(gate.MarkerVisible, Is.False);
            Assert.That(gate.SymbolText, Is.Empty);
            Assert.That(gate.AssignedColor, Is.EqualTo(targetColor));
            Assert.That(gate.transform.position, Is.EqualTo(position));
            Assert.That(gate.GetComponent<BoxCollider>().enabled, Is.True);
        }

        [Test]
        public void Hidden_UsesOrdinaryShieldAndFailureFlows()
        {
            HiddenSettings settings = CreateFastHiddenSettings(
                revealDuration: 0f,
                transitionDuration: 0f);
            StartHidden(settings, shield: true);
            EnterPlaying();
            StageGateView shieldedGate = FindGate(0);
            _controller.Tick(0f);
            SetDifferentColor(shieldedGate.AssignedColor);

            Assert.That(shieldedGate.TryResolveCrossing(), Is.True);
            Assert.That(
                _controller.ExperimentSession.LastResolution,
                Is.EqualTo(ExperimentGateResolution.ShieldDefense));
            Assert.That(
                _controller.ExperimentSession.FlowState,
                Is.EqualTo(StageFlowState.Playing));
            Assert.That(_controller.ExperimentSession.ShieldActive, Is.False);

            StartHidden(settings, shield: false);
            EnterPlaying();
            StageGateView failedGate = FindGate(0);
            _controller.Tick(0f);
            SetDifferentColor(failedGate.AssignedColor);

            Assert.That(failedGate.TryResolveCrossing(), Is.True);
            Assert.That(
                _controller.ExperimentSession.LastResolution,
                Is.EqualTo(ExperimentGateResolution.Failure));
            Assert.That(
                _controller.ExperimentSession.FlowState,
                Is.EqualTo(StageFlowState.Failed));
            Assert.That(
                _controller.ExperimentSession.LastFailureCause,
                Is.EqualTo(ExperimentRuntimeFailureCause.StandardGateMiss));
        }

        [Test]
        public void Hidden_UsesHeldEchoStoredColor()
        {
            _controller.StartDevelopmentExperiment(
                ExperimentCatalog.Get(4, MechanicExperimentType.Echo),
                new StartItemSelection(false, false));
            EnterPlaying();
            StageGateView provider = PassUntilProvider();
            MatchColor(provider.AssignedColor);
            Assert.That(provider.TryResolveCrossing(), Is.True);
            RunnerColor echoColor =
                _controller.ExperimentSession.EchoColor;
            StageGateView hiddenGate = FindGate(
                _controller.ExperimentSession.GatesPassed);
            HiddenSettings settings = CreateFastHiddenSettings(
                revealDuration: 0f,
                transitionDuration: 0f);
            ExperimentGatePlan original =
                hiddenGate.ActiveExperimentPlan;
            ExperimentGatePlan hidden = new ExperimentGatePlan(
                original.GateIndex,
                echoColor,
                original.RequiredTapCount,
                original.BaseSpeed,
                original.Cadence,
                original.Spacing,
                MechanicExperimentType.Hidden,
                new GateModifier(GateModifierType.Hidden));
            Material neutral =
                _controller.TrackPool.GetSegment(0).SurfaceMaterial;
            hiddenGate.ActivateExperiment(
                hidden,
                _controller.GetPresentationMaterial(echoColor),
                neutral,
                hiddenGate.transform.position.z,
                _controller.ExperimentSession.GatesPassed,
                0f,
                _controller.ExperimentSession.Definition.Camouflage,
                settings,
                FlickerSettings.Disabled(),
                _controller.ExperimentSession.ElapsedPlayingSeconds);
            hiddenGate.UpdateExperimentVisibility(
                _controller.ExperimentSession.GatesPassed,
                0f,
                neutral,
                _controller.ExperimentSession.Definition.Camouflage,
                settings,
                FlickerSettings.Disabled(),
                _controller.ExperimentSession.ElapsedPlayingSeconds,
                0f);
            SetDifferentColor(echoColor);

            Assert.That(hiddenGate.HiddenTargetAlpha, Is.Zero);
            Assert.That(hiddenGate.TryResolveCrossing(), Is.True);
            Assert.That(
                _controller.ExperimentSession.LastResolution,
                Is.EqualTo(ExperimentGateResolution.EchoColorMatch));
            Assert.That(_controller.ExperimentSession.EchoActive, Is.False);
            Assert.That(_controller.ExperimentSession.ShieldActive, Is.False);
        }

        [Test]
        public void Retry_ReproducesHiddenSelectionAndResetsVisibility()
        {
            HiddenSettings settings = CreateFastHiddenSettings(
                revealDuration: 0f,
                transitionDuration: 0f);
            StartHidden(settings, shield: false);
            EnterPlaying();
            StageGateView original = FindGate(0);
            _controller.Tick(0f);
            int gateId = original.ActiveExperimentPlan.GateId;
            Assert.That(original.HiddenHideStarted, Is.True);

            _controller.RestartDevelopmentExperiment();
            StageGateView replay = FindGate(0);

            Assert.That(
                replay.ActiveExperimentPlan.GateId,
                Is.EqualTo(gateId));
            Assert.That(replay.ActiveExperimentPlan.IsHidden, Is.True);
            Assert.That(replay.HiddenVisibleElapsed, Is.Zero);
            Assert.That(replay.HiddenHideStarted, Is.False);
            Assert.That(replay.HiddenTransitionProgress, Is.Zero);
            Assert.That(replay.HiddenTargetAlpha, Is.EqualTo(1f));
            Assert.That(replay.SymbolVisible, Is.True);
            Assert.That(replay.MarkerVisible, Is.False);
            Assert.That(replay.SymbolText, Is.Empty);
        }

        private void StartHidden(
            HiddenSettings settings,
            bool shield)
        {
            ExperimentDefinition definition = ExperimentCatalog.Get(
                4,
                MechanicExperimentType.Hidden,
                ExperimentCatalog.DefaultSeed,
                settings);
            _controller.StartDevelopmentExperiment(
                definition,
                new StartItemSelection(shield, false));
        }

        private void EnterPlaying()
        {
            _controller.Tick(3.1f);
            Assert.That(
                _controller.ExperimentSession.FlowState,
                Is.EqualTo(StageFlowState.Playing));
        }

        private StageGateView FindGate(int gateIndex)
        {
            for (int index = 0; index < _controller.GatePoolSize; index++)
            {
                StageGateView gate = _controller.GetGate(index);
                if (gate.gameObject.activeSelf &&
                    gate.HasExperimentPlan &&
                    !gate.HasResolved &&
                    gate.ActiveExperimentPlan.GateIndex == gateIndex)
                {
                    return gate;
                }
            }
            Assert.Fail($"Experiment gate {gateIndex} was not active.");
            return null;
        }

        private StageGateView PassUntilProvider()
        {
            while (_controller.ExperimentSession.GatesPassed <
                _controller.ExperimentSession.Definition.GateCount)
            {
                StageGateView gate = FindGate(
                    _controller.ExperimentSession.GatesPassed);
                if (gate.ActiveExperimentPlan.IsEchoProvider)
                {
                    return gate;
                }
                MatchColor(gate.AssignedColor);
                Assert.That(gate.TryResolveCrossing(), Is.True);
            }
            Assert.Fail("No Echo provider was generated.");
            return null;
        }

        private void MatchColor(RunnerColor target)
        {
            int safety =
                _controller.ExperimentSession.Definition.ColorCount;
            while (_controller.ExperimentSession.CurrentColor != target &&
                safety-- > 0)
            {
                _controller.HandleGameplayTap();
            }
            Assert.That(
                _controller.ExperimentSession.CurrentColor,
                Is.EqualTo(target));
        }

        private void SetDifferentColor(RunnerColor target)
        {
            if (_controller.ExperimentSession.CurrentColor == target)
            {
                _controller.HandleGameplayTap();
            }
            Assert.That(
                _controller.ExperimentSession.CurrentColor,
                Is.Not.EqualTo(target));
        }

        private static HiddenSettings CreateFastHiddenSettings(
            float revealDuration,
            float transitionDuration)
        {
            return new HiddenSettings(
                true,
                0f,
                0.9f,
                0f,
                0,
                revealDuration,
                100f,
                transitionDuration,
                1,
                true);
        }

        private static void SelectHidden(ExperimentLauncher launcher)
        {
            while (launcher.Mechanic != MechanicExperimentType.Hidden)
            {
                launcher.NextMechanic();
            }
        }

        private static ExperimentLauncher FindLauncher()
        {
            ExperimentLauncher[] launchers =
                Object.FindObjectsByType<ExperimentLauncher>(
                    FindObjectsInactive.Include);
            Assert.That(launchers.Length, Is.EqualTo(1));
            return launchers[0];
        }

        private sealed class InMemoryStageProgressStore :
            IStageProgressStore
        {
            private readonly StageRecord[] _records =
                new StageRecord[StageCatalog.Count];

            public int LoadHighestUnlocked()
            {
                return StageCatalog.Count;
            }

            public void SaveHighestUnlocked(int highestUnlocked)
            {
            }

            public StageRecord LoadRecord(int stageNumber)
            {
                return _records[stageNumber - 1];
            }

            public void SaveRecord(int stageNumber, StageRecord record)
            {
                _records[stageNumber - 1] = record;
            }

            public void ClearGameplayProgress()
            {
                for (int index = 0; index < _records.Length; index++)
                {
                    _records[index] = default;
                }
            }
        }
    }
}
