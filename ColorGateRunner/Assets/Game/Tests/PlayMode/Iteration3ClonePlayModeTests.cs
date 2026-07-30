using System.Collections;
using ColorGateRunner.Core;
using ColorGateRunner.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ColorGateRunner.Tests.PlayMode
{
    public sealed class Iteration3ClonePlayModeTests
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
        public void CloneLauncher_StartsStandaloneConditionWithoutBooster()
        {
            _controller.OpenExperimentLab();
            ExperimentLauncher launcher = FindLauncher();
            launcher.ToggleBooster();
            SelectClone(launcher);

            Assert.That(launcher.Booster, Is.False);
            Assert.That(launcher.Label, Does.Contain("CLONE"));
            Assert.That(launcher.Label, Does.Contain("NO ITEMS"));

            launcher.StartExperiment();

            Assert.That(_controller.ExperimentActive, Is.True);
            Assert.That(
                _controller.ExperimentSession.FlowState,
                Is.EqualTo(StageFlowState.Countdown));
            Assert.That(
                _controller.ExperimentSession.Definition.CloneEnabled,
                Is.True);
            Assert.That(
                _controller.ExperimentSession.Definition.GateCount,
                Is.EqualTo(42));
            Assert.That(
                _controller.ExperimentSession.Items.Booster,
                Is.False);
            Assert.That(_controller.GatePoolSize, Is.EqualTo(6));
        }

        [Test]
        public void CloneRuntime_CountdownBlocksThenJudgesSourceAndClone()
        {
            StartClone(out _);
            StageGateView first = FindGate(0);

            Assert.That(first.TryResolveCrossing(), Is.False);
            Assert.That(_controller.ExperimentSession.GatesPassed, Is.Zero);

            EnterPlaying();
            PassCurrent();
            PassCurrent();
            StageGateView source = FindGate(2);
            Assert.That(source.ActiveExperimentPlan.IsSource, Is.True);
            RunnerColor sourceColor = source.AssignedColor;
            float sourceZ = source.transform.position.z;
            Pass(source);

            StageGateView clone = FindGate(3);
            Assert.That(clone.ActiveExperimentPlan.IsClone, Is.True);
            Assert.That(clone.AssignedColor, Is.EqualTo(sourceColor));
            Assert.That(
                clone.transform.position.z - sourceZ,
                Is.EqualTo(clone.ActiveExperimentPlan.Spacing)
                    .Within(0.0001f));
            Assert.That(clone.CloneVisualActive, Is.True);
            Assert.That(clone.SymbolAlpha, Is.LessThan(1f));

            Pass(clone);

            Assert.That(_controller.ExperimentSession.GatesPassed,
                Is.EqualTo(4));
            Assert.That(
                _controller.ExperimentSession.FlowState,
                Is.EqualTo(StageFlowState.Playing));
        }

        [Test]
        public void CloneRuntime_FailureShowsCauseAndRetryReplaysLayout()
        {
            StartClone(out _);
            EnterPlaying();
            PassCurrent();
            PassCurrent();
            PassCurrent();
            StageGateView clone = FindGate(3);
            float cloneZ = clone.transform.position.z;
            MismatchCurrent(clone);

            Assert.That(
                _controller.ExperimentSession.LastFailureCause,
                Is.EqualTo(ExperimentRuntimeFailureCause.CloneGateMiss));
            _controller.Tick(1.1f);
            Assert.That(_controller.FailedResultRoot.activeSelf, Is.True);
            Assert.That(
                _controller.FailDetailsText.text,
                Does.Contain("CLONE MISS"));

            _controller.RetryToItemSelection();

            Assert.That(
                _controller.ExperimentSession.FlowState,
                Is.EqualTo(StageFlowState.Countdown));
            Assert.That(_controller.ExperimentSession.GatesPassed, Is.Zero);
            Assert.That(
                _controller.ExperimentSession.LastFailureCause,
                Is.EqualTo(ExperimentRuntimeFailureCause.None));
            StageGateView replayClone = FindGate(3);
            Assert.That(replayClone.transform.position.z,
                Is.EqualTo(cloneZ).Within(0.0001f));
            Assert.That(replayClone.CloneVisualActive, Is.True);
            Assert.That(replayClone.ReactionActive, Is.False);
        }

        [Test]
        public void CloneRuntime_ShieldConsumesOnceAndKeepsPlaying()
        {
            StartDefinition(
                ExperimentCatalog.Get(4, MechanicExperimentType.Clone),
                new StartItemSelection(true, false));
            EnterPlaying();
            PassCurrent();
            PassCurrent();
            PassCurrent();
            StageGateView clone = FindGate(3);
            MismatchCurrent(clone);

            Assert.That(
                _controller.ExperimentSession.FlowState,
                Is.EqualTo(StageFlowState.Playing));
            Assert.That(_controller.ExperimentSession.GatesPassed,
                Is.EqualTo(4));
            Assert.That(
                _controller.ExperimentSession.ShieldActive,
                Is.False);
            Assert.That(_controller.ShieldVisual.activeSelf, Is.False);
        }

        [Test]
        public void CloneCamouflage_HidesRevealsJudgesAndResets()
        {
            ExperimentDefinition definition = new ExperimentDefinition(
                4,
                MechanicExperimentType.Camouflage,
                ExperimentCatalog.DefaultSeed,
                CloneSettings.CreateApproved());
            StartDefinition(
                definition,
                new StartItemSelection(false, false));
            StageGateView clone = FindGate(3);
            Assert.That(clone.ActiveExperimentPlan.IsClone, Is.True);
            Assert.That(clone.ActiveExperimentPlan.IsCamouflage, Is.True);
            Assert.That(clone.SymbolVisible, Is.False);

            EnterPlaying();
            PassCurrent();
            PassCurrent();

            StageGateView source = FindGate(2);
            clone = FindGate(3);
            Assert.That(clone.SymbolVisible, Is.True);
            Assert.That(clone.AssignedColor, Is.EqualTo(source.AssignedColor));
            Pass(source);
            MismatchCurrent(clone);

            Assert.That(
                _controller.ExperimentSession.FlowState,
                Is.EqualTo(StageFlowState.Failed));
            Assert.That(
                _controller.ExperimentSession.LastFailureCause,
                Is.EqualTo(ExperimentRuntimeFailureCause.CloneGateMiss));

            _controller.RetryToItemSelection();

            StageGateView replayClone = FindGate(3);
            Assert.That(replayClone.SymbolVisible, Is.False);
            Assert.That(replayClone.CloneVisualActive, Is.True);
        }

        [Test]
        public void CloneRuntime_CompletionReplayAndLabReentryDoNotGrowPools()
        {
            StartClone(out ExperimentLauncher launcher);
            EnterPlaying();
            float firstCloneZ = FindGate(3).transform.position.z;

            while (_controller.ExperimentSession.FlowState ==
                StageFlowState.Playing)
            {
                PassCurrent();
            }
            _controller.Tick(1.3f);

            Assert.That(
                _controller.ExperimentSession.FlowState,
                Is.EqualTo(StageFlowState.StageCleared));
            Assert.That(_controller.ClearResultRoot.activeSelf, Is.True);

            _controller.RetryToItemSelection();

            Assert.That(
                _controller.ExperimentSession.FlowState,
                Is.EqualTo(StageFlowState.Countdown));
            Assert.That(FindGate(3).transform.position.z,
                Is.EqualTo(firstCloneZ).Within(0.0001f));
            _controller.BackToExperimentLab();
            launcher.StartExperiment();

            Assert.That(_controller.GatePoolSize, Is.EqualTo(6));
            Assert.That(_controller.TrackPool.SegmentCount, Is.EqualTo(6));
            Assert.That(
                Object.FindObjectsByType<ExperimentLauncher>(
                    FindObjectsInactive.Include).Length,
                Is.EqualTo(1));
        }

        private void StartClone(out ExperimentLauncher launcher)
        {
            _controller.OpenExperimentLab();
            launcher = FindLauncher();
            SelectClone(launcher);
            launcher.StartExperiment();
        }

        private void StartDefinition(
            ExperimentDefinition definition,
            StartItemSelection items)
        {
            _controller.StartDevelopmentExperiment(definition, items);
        }

        private void EnterPlaying()
        {
            _controller.Tick(3.1f);
            Assert.That(
                _controller.ExperimentSession.FlowState,
                Is.EqualTo(StageFlowState.Playing));
        }

        private void PassCurrent()
        {
            Pass(FindGate(_controller.ExperimentSession.GatesPassed));
        }

        private void Pass(StageGateView gate)
        {
            MatchColor(gate.AssignedColor);
            Assert.That(gate.TryResolveCrossing(), Is.True);
        }

        private void MismatchCurrent(StageGateView gate)
        {
            if (_controller.ExperimentSession.CurrentColor ==
                gate.AssignedColor)
            {
                _controller.HandleGameplayTap();
            }
            Assert.That(
                _controller.ExperimentSession.CurrentColor,
                Is.Not.EqualTo(gate.AssignedColor));
            Assert.That(gate.TryResolveCrossing(), Is.True);
        }

        private void MatchColor(RunnerColor color)
        {
            int safety =
                _controller.ExperimentSession.Definition.ColorCount;
            while (_controller.ExperimentSession.CurrentColor != color &&
                safety-- > 0)
            {
                _controller.HandleGameplayTap();
            }
            Assert.That(
                _controller.ExperimentSession.CurrentColor,
                Is.EqualTo(color));
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

        private static void SelectClone(ExperimentLauncher launcher)
        {
            while (launcher.Mechanic != MechanicExperimentType.Clone)
            {
                launcher.NextMechanic();
            }
            Assert.That(launcher.SelectedDefinition.CloneEnabled, Is.True);
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
                return 5;
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
