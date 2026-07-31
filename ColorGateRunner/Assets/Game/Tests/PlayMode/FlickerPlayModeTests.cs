using System.Collections;
using ColorGateRunner.Core;
using ColorGateRunner.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ColorGateRunner.Tests.PlayMode
{
    public sealed class FlickerPlayModeTests
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
        public void FlickerLauncher_DisablesBoosterAndUsesFixedPool()
        {
            _controller.OpenExperimentLab();
            ExperimentLauncher launcher = FindLauncher();
            launcher.ToggleBooster();
            Assert.That(launcher.Booster, Is.True);
            SelectFlicker(launcher);

            Assert.That(launcher.Booster, Is.False);
            Assert.That(launcher.Label, Does.Contain("BOOSTER DISABLED"));
            launcher.ToggleBooster();
            Assert.That(launcher.Booster, Is.False);

            launcher.StartExperiment();

            Assert.That(_controller.ExperimentActive, Is.True);
            Assert.That(
                _controller.ExperimentSession.Definition.Mechanic,
                Is.EqualTo(MechanicExperimentType.Flicker));
            Assert.That(_controller.ExperimentSession.Items.Booster, Is.False);
            Assert.That(_controller.GatePoolSize, Is.EqualTo(6));
        }

        [Test]
        public void Countdown_DoesNotAdvanceFlickerObservation()
        {
            StartFlicker(
                CreateFastFlickerSettings(
                    revealDuration: 1f,
                    transitionDuration: 0.2f),
                shield: false);
            StageGateView gate = FindGate(0);

            _controller.Tick(1f);
            Assert.That(
                _controller.ExperimentSession.FlowState,
                Is.EqualTo(StageFlowState.Countdown));
            Assert.That(gate.FlickerVisibleElapsed, Is.Zero);
            Assert.That(gate.FlickerHideStarted, Is.False);

            _controller.Tick(2.1f);
            Assert.That(
                _controller.ExperimentSession.FlowState,
                Is.EqualTo(StageFlowState.Playing));
            Assert.That(gate.FlickerVisibleElapsed, Is.Zero);
            Assert.That(gate.FlickerHideStarted, Is.False);
        }

        [Test]
        public void Flicker_HidesTargetOnlyAfterObservationAndEtaConditions()
        {
            FlickerSettings settings = CreateFastFlickerSettings(
                revealDuration: 1f,
                transitionDuration: 0.2f);
            StartFlicker(settings, shield: false);
            EnterPlaying();
            StageGateView gate = FindGate(0);
            Vector3 position = gate.transform.position;
            RunnerColor targetColor = gate.AssignedColor;

            Assert.That(gate.SymbolText, Does.Contain("FLICKER"));
            Assert.That(gate.FlickerTargetAlpha, Is.EqualTo(1f));

            _controller.Tick(0.99f);
            Assert.That(gate.FlickerHideStarted, Is.False);

            _controller.Tick(0.01f);
            Assert.That(gate.FlickerHideStarted, Is.True);
            Assert.That(gate.FlickerTransitionProgress, Is.GreaterThan(0f));
            Assert.That(gate.FlickerHideStartCount, Is.EqualTo(1));

            _controller.Tick(0.19f);
            Assert.That(gate.FlickerTransitionProgress, Is.EqualTo(1f));
            Assert.That(gate.FlickerTargetAlpha, Is.Zero);
            Assert.That(gate.SymbolVisible, Is.True);
            Assert.That(gate.SymbolText, Does.Contain("FLICKER"));
            Assert.That(gate.AssignedColor, Is.EqualTo(targetColor));
            Assert.That(gate.transform.position, Is.EqualTo(position));
            Assert.That(gate.GetComponent<BoxCollider>().enabled, Is.True);
        }

        [Test]
        public void HiddenFlicker_UsesOrdinaryShieldAndFailureFlows()
        {
            FlickerSettings settings = CreateFastFlickerSettings(
                revealDuration: 0f,
                transitionDuration: 0f);
            StartFlicker(settings, shield: true);
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

            StartFlicker(settings, shield: false);
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
        public void HiddenFlicker_UsesHeldEchoWithoutConsumingShield()
        {
            _controller.StartDevelopmentExperiment(
                ExperimentCatalog.Get(4, MechanicExperimentType.Echo),
                new StartItemSelection(true, false));
            EnterPlaying();
            StageGateView provider = PassUntilProvider();
            MatchColor(provider.AssignedColor);
            Assert.That(provider.TryResolveCrossing(), Is.True);
            RunnerColor echoColor =
                _controller.ExperimentSession.EchoColor;
            StageGateView flickerGate = FindGate(
                _controller.ExperimentSession.GatesPassed);
            FlickerSettings settings = CreateFastFlickerSettings(
                revealDuration: 0f,
                transitionDuration: 0f);
            ExperimentGatePlan original =
                flickerGate.ActiveExperimentPlan;
            ExperimentGatePlan flicker = new ExperimentGatePlan(
                original.GateIndex,
                echoColor,
                original.RequiredTapCount,
                original.BaseSpeed,
                original.Cadence,
                original.Spacing,
                MechanicExperimentType.Flicker,
                new GateModifier(GateModifierType.Flicker));
            Material neutral =
                _controller.TrackPool.GetSegment(0).SurfaceMaterial;
            flickerGate.ActivateExperiment(
                flicker,
                _controller.GetPresentationMaterial(echoColor),
                neutral,
                flickerGate.transform.position.z,
                _controller.ExperimentSession.GatesPassed,
                0f,
                _controller.ExperimentSession.Definition.Camouflage,
                settings);
            flickerGate.UpdateExperimentVisibility(
                _controller.ExperimentSession.GatesPassed,
                0f,
                neutral,
                _controller.ExperimentSession.Definition.Camouflage,
                settings,
                0f);
            SetDifferentColor(echoColor);

            Assert.That(flickerGate.FlickerTargetAlpha, Is.Zero);
            Assert.That(flickerGate.TryResolveCrossing(), Is.True);
            Assert.That(
                _controller.ExperimentSession.LastResolution,
                Is.EqualTo(ExperimentGateResolution.EchoColorMatch));
            Assert.That(_controller.ExperimentSession.EchoActive, Is.False);
            Assert.That(_controller.ExperimentSession.ShieldActive, Is.True);
        }

        [Test]
        public void Retry_ReproducesFlickerSelectionAndResetsVisibility()
        {
            FlickerSettings settings = CreateFastFlickerSettings(
                revealDuration: 0f,
                transitionDuration: 0f);
            StartFlicker(settings, shield: false);
            EnterPlaying();
            StageGateView original = FindGate(0);
            _controller.Tick(0f);
            int gateId = original.ActiveExperimentPlan.GateId;
            Assert.That(original.FlickerHideStarted, Is.True);

            _controller.RestartDevelopmentExperiment();
            StageGateView replay = FindGate(0);

            Assert.That(
                replay.ActiveExperimentPlan.GateId,
                Is.EqualTo(gateId));
            Assert.That(replay.ActiveExperimentPlan.IsFlicker, Is.True);
            Assert.That(replay.FlickerVisibleElapsed, Is.Zero);
            Assert.That(replay.FlickerHideStarted, Is.False);
            Assert.That(replay.FlickerTransitionProgress, Is.Zero);
            Assert.That(replay.FlickerTargetAlpha, Is.EqualTo(1f));
            Assert.That(replay.SymbolText, Does.Contain("FLICKER"));
        }

        private void StartFlicker(
            FlickerSettings settings,
            bool shield)
        {
            ExperimentDefinition definition = ExperimentCatalog.Get(
                4,
                MechanicExperimentType.Flicker,
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

        private static FlickerSettings CreateFastFlickerSettings(
            float revealDuration,
            float transitionDuration)
        {
            return new FlickerSettings(
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

        private static void SelectFlicker(ExperimentLauncher launcher)
        {
            while (launcher.Mechanic != MechanicExperimentType.Flicker)
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
