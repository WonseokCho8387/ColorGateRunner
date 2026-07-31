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
            Assert.That(gate.AssignedColor, Is.Not.EqualTo(initial));
            Assert.That(gate.SymbolText, Does.Contain("FLICKER"));
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

            Assert.That(plan.CycleColorCount, Is.EqualTo(colorCount));
            for (int index = 0; index < colorCount; index++)
            {
                AssertDisplayedPhase(gate, plan, index);
                _controller.Tick(0.5f);
            }
            AssertDisplayedPhase(gate, plan, 0);
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

            _controller.Tick(0.5f);

            Assert.That(gate.FlickerPhaseIndex, Is.EqualTo(1));
            Assert.That(gate.FlickerTransitionPulse, Is.EqualTo(1f));
            Assert.That(gate.SymbolVisible, Is.True);
            Assert.That(gate.SymbolText, Does.Contain("FLICKER"));
            Assert.That(gate.transform.position, Is.EqualTo(position));
            Assert.That(gate.transform.localScale, Is.EqualTo(scale));
            Assert.That(collider.enabled, Is.True);
            Assert.That(collider.isTrigger, Is.True);

            _controller.Tick(0.1f);
            Assert.That(gate.FlickerTransitionPulse, Is.Zero);
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

            Assert.That(gate.TryResolveCrossing(), Is.True);
            Assert.That(
                _controller.ExperimentSession.LastJudgmentColor,
                Is.EqualTo(crossingColor));
            Assert.That(
                _controller.ExperimentSession.LastResolution,
                Is.EqualTo(ExperimentGateResolution.PlayerColorMatch));
        }

        [Test]
        public void Flicker_EchoMatchConsumesEchoAndPreservesShield()
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
            Assert.That(_controller.ExperimentSession.ShieldActive, Is.True);
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
            Assert.That(FindGate(0).SymbolText, Does.Contain("FLICKER"));

            _controller.BackToExperimentLab();
            ExperimentLauncher launcher = FindLauncher();
            Assert.That(_controller.ExperimentActive, Is.False);
            SelectNone(launcher);
            launcher.StartExperiment();
            StageGateView ordinary = FindGate(0);

            Assert.That(ordinary.ActiveExperimentPlan.IsFlicker, Is.False);
            Assert.That(ordinary.SymbolText, Does.Not.Contain("FLICKER"));
            Assert.That(ordinary.FlickerPhaseIndex, Is.Zero);
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
            Assert.That(
                gate.SymbolText,
                Does.Contain(GetSymbol(expected)));
        }

        private static string GetSymbol(RunnerColor color)
        {
            switch (color)
            {
                case RunnerColor.Red:
                    return "●";
                case RunnerColor.Blue:
                    return "■";
                case RunnerColor.Green:
                    return "▲";
                case RunnerColor.Yellow:
                    return "★";
                case RunnerColor.Purple:
                    return "◆";
                default:
                    return "HEX";
            }
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
