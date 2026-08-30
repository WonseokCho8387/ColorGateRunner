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
        public void FlickerLauncher_IsDistinctFromHiddenAndDisablesBooster()
        {
            _controller.OpenExperimentLab();
            ExperimentLauncher launcher = FindLauncher();
            SelectHidden(launcher);
            Assert.That(launcher.Label, Does.Contain("HIDDEN"));
            launcher.NextMechanic();

            Assert.That(
                launcher.Mechanic,
                Is.EqualTo(MechanicExperimentType.Flicker));
            Assert.That(launcher.Label, Does.Contain("FLICKER"));
            Assert.That(launcher.Label, Does.Contain("BOOSTER DISABLED"));
            launcher.ToggleBooster();
            Assert.That(launcher.Booster, Is.False);
            launcher.StartExperiment();

            Assert.That(
                _controller.ExperimentSession.Definition.Mechanic,
                Is.EqualTo(MechanicExperimentType.Flicker));
            Assert.That(_controller.ExperimentSession.Items.Booster, Is.False);
            Assert.That(_controller.GatePoolSize, Is.EqualTo(6));
        }

        [Test]
        public void Countdown_FreezesPhaseAndPlayingStartsPaletteCycle()
        {
            StartFlicker(CreateSettings());
            StageGateView gate = FindGate(0);
            RunnerColor initial = gate.AssignedColor;

            _controller.Tick(2.9f);
            Assert.That(
                _controller.ExperimentSession.ElapsedPlayingSeconds,
                Is.Zero);
            Assert.That(gate.FlickerPhaseIndex, Is.Zero);
            Assert.That(gate.AssignedColor, Is.EqualTo(initial));

            _controller.Tick(0.2f);
            Assert.That(
                _controller.ExperimentSession.FlowState,
                Is.EqualTo(StageFlowState.Playing));
            _controller.Tick(0.5f);

            Assert.That(gate.FlickerPhaseIndex, Is.EqualTo(1));
            Assert.That(gate.AssignedColor, Is.EqualTo(initial));
            Assert.That(gate.FlickerWindow.IsTransitioning, Is.True);
            Assert.That(
                gate.FlickerWindow.Accepts(initial),
                Is.True);
            Assert.That(
                gate.FlickerWindow.Accepts(
                    gate.ActiveExperimentPlan.GetCycleColor(1)),
                Is.True);
            Assert.That(gate.SymbolText, Is.Empty);

            _controller.Tick(0.2f);
            Assert.That(gate.FlickerWindow.IsTransitioning, Is.False);
            Assert.That(gate.AssignedColor, Is.Not.EqualTo(initial));
        }

        [Test]
        public void Pause_FreezesFlickerGameplayTimeAndDisplayedPhase()
        {
            StartFlicker(CreateSettings());
            EnterPlaying();
            StageGateView gate = FindGate(0);
            _controller.Tick(0.2f);
            float elapsed =
                _controller.ExperimentSession.ElapsedPlayingSeconds;
            long phase = gate.FlickerPhaseIndex;
            RunnerColor color = gate.AssignedColor;

            _controller.RequestPause();
            _controller.Tick(1f);

            Assert.That(
                _controller.ExperimentSession.ElapsedPlayingSeconds,
                Is.EqualTo(elapsed));
            Assert.That(gate.FlickerPhaseIndex, Is.EqualTo(phase));
            Assert.That(gate.AssignedColor, Is.EqualTo(color));
        }

        [TestCase(3)]
        [TestCase(6)]
        public void ActivePaletteFlicker_UpdatesColorAndSymbolFromSamePhase(
            int colorCount)
        {
            StartFlicker(CreateSettings(), colorCount: colorCount);
            EnterPlaying();
            StageGateView gate = FindGate(0);
            ExperimentGatePlan plan = gate.ActiveExperimentPlan;
            FlickerGateRuntimeState state = new FlickerGateRuntimeState();

            Assert.That(plan.CycleColorCount, Is.EqualTo(colorCount));
            for (int index = 0; index < colorCount; index++)
            {
                float phaseTime = index * 0.5f;
                FlickerJudgmentWindow window = state.Update(
                    plan.FlickerPlan,
                    phaseTime,
                    30f,
                    0f);
                if (window.IsTransitioning)
                {
                    window = state.Update(
                        plan.FlickerPlan,
                        phaseTime + 0.1f,
                        30f,
                        0.1f);
                }
                gate.ApplyFlickerRuntimeState(window);
                AssertDisplayedPhase(gate, plan, index);
            }
        }

        [Test]
        public void FlickerBoundary_PulsesWithoutMovingOrDisablingGate()
        {
            StartFlicker(CreateSettings());
            EnterPlaying();
            StageGateView gate = FindGate(0);
            Vector3 position = gate.transform.position;
            Vector3 scale = gate.transform.localScale;
            BoxCollider collider = gate.GetComponent<BoxCollider>();
            FlickerGateRuntimeState state = new FlickerGateRuntimeState();
            ExperimentGatePlan plan = gate.ActiveExperimentPlan;
            state.Update(plan.FlickerPlan, 0.49f, 30f, 0f);

            FlickerJudgmentWindow started = state.Update(
                plan.FlickerPlan,
                0.5f,
                30f,
                0f);
            gate.ApplyFlickerRuntimeState(started);

            Assert.That(gate.FlickerPhaseIndex, Is.EqualTo(1));
            Assert.That(gate.FlickerTransitionPulse, Is.EqualTo(1f));
            Assert.That(gate.FlickerFrameTransitioning, Is.True);
            Assert.That(gate.FlickerFrameProgress, Is.Zero);
            Assert.That(gate.FlickerLeftProgress, Is.Zero);
            Assert.That(gate.FlickerTopProgress, Is.Zero);
            Assert.That(gate.FlickerRightProgress, Is.Zero);
            Assert.That(
                Vector3.Dot(gate.FlickerLeftRevealDirection, Vector3.down),
                Is.GreaterThan(0.999f));
            Assert.That(
                Vector3.Dot(gate.FlickerTopRevealDirection, Vector3.right),
                Is.GreaterThan(0.999f));
            Assert.That(
                Vector3.Dot(gate.FlickerRightRevealDirection, Vector3.up),
                Is.GreaterThan(0.999f));
            Assert.That(gate.FlickerSymbolTransitioning, Is.True);
            Assert.That(gate.FlickerNextSymbolVisible, Is.True);
            Assert.That(gate.FlickerSymbolProgress, Is.Zero);
            Assert.That(
                gate.FlickerCurrentSymbol,
                Is.EqualTo(
                    _controller.GetColorEmblemSprite(
                        started.CurrentColor)));
            Assert.That(
                gate.FlickerNextSymbol,
                Is.EqualTo(
                    _controller.GetColorEmblemSprite(
                        started.NextColor)));
            Assert.That(gate.SymbolVisible, Is.True);
            Assert.That(gate.SymbolText, Is.Empty);
            Assert.That(gate.transform.position, Is.EqualTo(position));
            Assert.That(gate.transform.localScale, Is.EqualTo(scale));
            Assert.That(collider.enabled, Is.True);
            Assert.That(collider.isTrigger, Is.True);

            FlickerJudgmentWindow halfway = state.Update(
                plan.FlickerPlan,
                0.55f,
                30f,
                0.05f);
            gate.ApplyFlickerRuntimeState(halfway);
            Assert.That(gate.FlickerFrameTransitioning, Is.True);
            Assert.That(gate.FlickerFrameProgress, Is.EqualTo(0.5f));
            Assert.That(gate.FlickerRightProgress, Is.EqualTo(1f));
            Assert.That(gate.FlickerTopProgress, Is.EqualTo(0.5f)
                .Within(0.001f));
            Assert.That(gate.FlickerLeftProgress, Is.Zero);
            Assert.That(
                gate.DisplayMaterial,
                Is.SameAs(gate.FlickerFrameMaterial));
            Assert.That(gate.FlickerSymbolTransitioning, Is.True);
            Assert.That(gate.FlickerNextSymbolVisible, Is.True);
            Assert.That(gate.FlickerSymbolProgress, Is.EqualTo(0.5f));

            FlickerJudgmentWindow completed = state.Update(
                plan.FlickerPlan,
                0.6f,
                30f,
                0.05f);
            gate.ApplyFlickerRuntimeState(completed);
            Assert.That(gate.FlickerTransitionPulse, Is.Zero);
            Assert.That(gate.FlickerFrameTransitioning, Is.False);
            Assert.That(gate.FlickerSymbolTransitioning, Is.False);
            Assert.That(gate.FlickerNextSymbolVisible, Is.False);
            Assert.That(
                gate.FlickerCurrentSymbol,
                Is.EqualTo(
                    _controller.GetColorEmblemSprite(
                        completed.CurrentColor)));
        }

        [Test]
        public void FlickerFrame_DissolvesInChaseViewLeftTopRightOrder()
        {
            StartFlicker(CreateSettings());
            EnterPlaying();
            StageGateView gate = FindGate(0);
            FlickerGateRuntimeState state = new FlickerGateRuntimeState();
            ExperimentGatePlan plan = gate.ActiveExperimentPlan;
            state.Update(plan.FlickerPlan, 0.49f, 30f, 0f);
            state.Update(plan.FlickerPlan, 0.5f, 30f, 0.01f);

            gate.ApplyFlickerRuntimeState(state.Update(
                plan.FlickerPlan,
                0.525f,
                30f,
                0.025f));
            Assert.That(gate.FlickerRightProgress, Is.GreaterThan(0f));
            Assert.That(gate.FlickerRightProgress, Is.LessThan(1f));
            Assert.That(gate.FlickerTopProgress, Is.Zero);
            Assert.That(gate.FlickerLeftProgress, Is.Zero);

            gate.ApplyFlickerRuntimeState(state.Update(
                plan.FlickerPlan,
                0.575f,
                30f,
                0.05f));
            Assert.That(gate.FlickerRightProgress, Is.EqualTo(1f));
            Assert.That(gate.FlickerTopProgress, Is.EqualTo(1f));
            Assert.That(gate.FlickerLeftProgress, Is.GreaterThan(0f));
            Assert.That(gate.FlickerLeftProgress, Is.LessThan(1f));

            Assert.That(gate.transform.Find("FlickerGatePath"), Is.Null);
            Assert.That(
                gate.GetComponentsInChildren<LineRenderer>(true),
                Is.Empty);
            for (int index = 0; index < gate.PartCount; index++)
            {
                Renderer renderer =
                    gate.GetPartTransform(index).GetComponent<Renderer>();
                Assert.That(
                    renderer.sharedMaterial,
                    Is.SameAs(gate.FlickerFrameMaterial));
            }
        }

        [Test]
        public void Flicker_PlayerPassUsesExactGameplayTimeColor()
        {
            StartFlicker(CreateSettings());
            EnterPlaying();
            StageGateView gate = FindGate(0);
            _controller.Tick(0.5f);
            MatchColor(gate.AssignedColor);
            RunnerColor crossingColor = gate.AssignedColor;
            Material crossingMaterial =
                _controller.GetPresentationMaterial(crossingColor);

            Assert.That(gate.TryResolveCrossing(), Is.True);
            Assert.That(
                _controller.ExperimentSession.LastJudgmentColor,
                Is.EqualTo(crossingColor));
            Assert.That(
                _controller.ExperimentSession.LastResolution,
                Is.EqualTo(ExperimentGateResolution.PlayerColorMatch));
            Assert.That(
                _controller.GateBreakEffects.LastPlayedMaterial,
                Is.SameAs(crossingMaterial));
        }

        [Test]
        public void FlickerBreakMaterial_UsesCurrentCommittedGateColor()
        {
            StartFlicker(CreateSettings());
            EnterPlaying();
            StageGateView gate = FindGate(0);
            RunnerColor authoredColor = gate.AssignedColor;
            RunnerColor currentColor = GetDifferentColor(authoredColor);
            gate.ApplyFlickerRuntimeState(
                new FlickerJudgmentWindow(
                    currentColor,
                    currentColor,
                    false,
                    0f,
                    1,
                    true));

            Assert.That(gate.AssignedColor, Is.EqualTo(currentColor));
            Assert.That(
                _controller.GetGateBreakMaterial(gate),
                Is.SameAs(
                    _controller.GetPresentationMaterial(currentColor)));
            Assert.That(
                _controller.GetGateBreakMaterial(gate),
                Is.Not.SameAs(
                    _controller.GetPresentationMaterial(authoredColor)));
        }

        [Test]
        public void Flicker_EchoMatchConsumesStoredEcho()
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
            RunnerColor other = GetDifferentColor(echoColor);
            StageGateView gate = FindGate(
                _controller.ExperimentSession.GatesPassed);
            BindFlickerPlan(
                gate,
                echoColor,
                new[] { echoColor, other });
            MatchColor(other);

            Assert.That(gate.TryResolveCrossing(), Is.True);
            Assert.That(
                _controller.ExperimentSession.LastResolution,
                Is.EqualTo(ExperimentGateResolution.EchoColorMatch));
            Assert.That(_controller.ExperimentSession.EchoActive, Is.False);
            Assert.That(_controller.ExperimentSession.ShieldActive, Is.False);
        }

        [Test]
        public void Flicker_ShieldAndFailureReuseOrdinaryFlow()
        {
            StartFlicker(CreateSettings(), shield: true);
            EnterPlaying();
            StageGateView shielded = FindGate(0);
            SetDifferentColor(shielded.AssignedColor);

            Assert.That(shielded.TryResolveCrossing(), Is.True);
            Assert.That(
                _controller.ExperimentSession.LastResolution,
                Is.EqualTo(ExperimentGateResolution.ShieldDefense));
            Assert.That(_controller.ExperimentSession.ShieldActive, Is.False);

            StartFlicker(CreateSettings(), shield: false);
            EnterPlaying();
            StageGateView failed = FindGate(0);
            SetDifferentColor(failed.AssignedColor);

            Assert.That(failed.TryResolveCrossing(), Is.True);
            Assert.That(
                _controller.ExperimentSession.FlowState,
                Is.EqualTo(StageFlowState.Failed));
            Assert.That(
                _controller.ExperimentSession.LastResolution,
                Is.EqualTo(ExperimentGateResolution.Failure));
            Assert.That(
                _controller.ExperimentSession.LastJudgmentColor,
                Is.EqualTo(failed.AssignedColor));
        }

        [Test]
        public void Retry_ReplaysCycleAndResetsGameplayPhase()
        {
            StartFlicker(CreateSettings());
            EnterPlaying();
            StageGateView original = FindGate(0);
            RunnerColor[] cycle =
                original.ActiveExperimentPlan.CopyCycleColors();
            float offset =
                original.ActiveExperimentPlan.PhaseOffsetSeconds;
            _controller.Tick(1f);
            Assert.That(original.FlickerPhaseIndex, Is.GreaterThan(0));

            _controller.RestartDevelopmentExperiment();
            StageGateView replay = FindGate(0);

            Assert.That(
                _controller.ExperimentSession.ElapsedPlayingSeconds,
                Is.Zero);
            Assert.That(replay.FlickerPhaseIndex, Is.Zero);
            Assert.That(
                replay.ActiveExperimentPlan.CopyCycleColors(),
                Is.EqualTo(cycle));
            Assert.That(
                replay.ActiveExperimentPlan.PhaseOffsetSeconds,
                Is.EqualTo(offset));
            Assert.That(
                replay.AssignedColor,
                Is.EqualTo(replay.ActiveExperimentPlan.GetCycleColor(0)));
        }

        [Test]
        public void BackToLab_ReentryClearsPooledFlickerState()
        {
            StartFlicker(CreateSettings());
            EnterPlaying();
            _controller.Tick(0.5f);
            Assert.That(FindGate(0).SymbolText, Is.Empty);

            _controller.BackToExperimentLab();
            ExperimentLauncher launcher = FindLauncher();
            Assert.That(_controller.ExperimentActive, Is.False);
            SelectNone(launcher);
            launcher.StartExperiment();
            StageGateView ordinary = FindGate(0);

            Assert.That(ordinary.ActiveExperimentPlan.IsFlicker, Is.False);
            Assert.That(ordinary.SymbolText, Does.Not.Contain("FLICKER"));
            Assert.That(ordinary.FlickerPhaseIndex, Is.Zero);
            Assert.That(ordinary.FlickerFrameTransitioning, Is.False);
            Assert.That(ordinary.FlickerNextSymbolVisible, Is.False);
            Assert.That(_controller.GatePoolSize, Is.EqualTo(6));
        }

        private void StartFlicker(
            FlickerSettings settings,
            bool shield = false,
            int colorCount = 4)
        {
            ExperimentDefinition definition = ExperimentCatalog.Get(
                colorCount,
                MechanicExperimentType.Flicker,
                ExperimentCatalog.DefaultSeed,
                flickerSettings: settings);
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

        private void BindFlickerPlan(
            StageGateView gate,
            RunnerColor baseColor,
            RunnerColor[] cycleColors)
        {
            ExperimentGatePlan original = gate.ActiveExperimentPlan;
            ExperimentGatePlan flicker = new ExperimentGatePlan(
                original.GateIndex,
                baseColor,
                original.RequiredTapCount,
                original.BaseSpeed,
                original.Cadence,
                original.Spacing,
                MechanicExperimentType.Flicker,
                new GateModifier(GateModifierType.Flicker),
                cycleColors,
                0.5f,
                0f,
                0.1f,
                _controller.ExperimentSession.Definition.Seed);
            Material neutral =
                _controller.TrackPool.GetSegment(0).SurfaceMaterial;
            gate.ActivateExperiment(
                flicker,
                _controller.GetPresentationMaterial(baseColor),
                neutral,
                gate.transform.position.z,
                _controller.ExperimentSession.GatesPassed,
                0f,
                _controller.ExperimentSession.Definition.Camouflage,
                HiddenSettings.Disabled(),
                CreateSettings(),
                _controller.ExperimentSession.ElapsedPlayingSeconds);
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

        private RunnerColor GetDifferentColor(RunnerColor target)
        {
            for (int index = 0;
                index < _controller.ExperimentSession.Definition.ColorCount;
                index++)
            {
                RunnerColor color =
                    _controller.ExperimentSession.Definition.GetColor(index);
                if (color != target)
                {
                    return color;
                }
            }
            return target;
        }

        private static void AssertDisplayedPhase(
            StageGateView gate,
            ExperimentGatePlan plan,
            int colorIndex)
        {
            RunnerColor expected = plan.GetCycleColor(colorIndex);
            Assert.That(gate.AssignedColor, Is.EqualTo(expected));
            Assert.That(gate.SymbolVisible, Is.True);
            Assert.That(gate.MarkerVisible, Is.False);
            Assert.That(gate.SymbolText, Is.Empty);
        }

        private static FlickerSettings CreateSettings()
        {
            return new FlickerSettings(
                true,
                0f,
                0.9f,
                1f,
                0,
                1,
                true,
                0.5f,
                0.1f,
                1,
                false);
        }

        private static void SelectHidden(ExperimentLauncher launcher)
        {
            while (launcher.Mechanic != MechanicExperimentType.Hidden)
            {
                launcher.NextMechanic();
            }
        }

        private static void SelectNone(ExperimentLauncher launcher)
        {
            while (launcher.Mechanic != MechanicExperimentType.None)
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
