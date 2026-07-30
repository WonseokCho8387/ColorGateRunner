using System.Collections;
using ColorGateRunner.Core;
using ColorGateRunner.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ColorGateRunner.Tests.PlayMode
{
    public sealed class Iteration3EchoPlayModeTests
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
        public void EchoLauncher_StartsFortyGateModifierCondition()
        {
            _controller.OpenExperimentLab();
            ExperimentLauncher launcher = FindLauncher();
            SelectEcho(launcher);
            Assert.That(launcher.Label, Does.Contain("ECHO"));

            launcher.StartExperiment();

            Assert.That(_controller.ExperimentActive, Is.True);
            Assert.That(
                _controller.ExperimentSession.Definition.GateCount,
                Is.EqualTo(40));
            Assert.That(_controller.GatePoolSize, Is.EqualTo(6));
        }

        [UnityTest]
        public IEnumerator ProviderUsesOrdinaryGateAndShowsEchoMarker()
        {
            StartEcho();
            EnterPlaying();
            StageGateView provider = PassUntilProvider();

            Assert.That(provider.ActiveExperimentPlan.IsEchoProvider, Is.True);
            Assert.That(provider.EchoProviderVisualActive, Is.True);
            Assert.That(provider.SymbolVisible, Is.True);
            Assert.That(provider.gameObject.name, Does.StartWith("StageGate_"));
            yield return null;
        }

        [UnityTest]
        public IEnumerator DirectMatchActivatesColoredEchoShell()
        {
            StartEcho();
            EnterPlaying();
            StageGateView provider = PassUntilProvider();
            MatchColor(provider.AssignedColor);

            Assert.That(provider.TryResolveCrossing(), Is.True);

            Assert.That(_controller.ExperimentSession.EchoActive, Is.True);
            Assert.That(_controller.EchoShellVisual.activeSelf, Is.True);
            yield return null;
        }

        [UnityTest]
        public IEnumerator EchoConsumptionHidesShellWithoutUsingShield()
        {
            StartEcho(true);
            EnterPlaying();
            StageGateView provider = PassUntilProvider();
            MatchColor(provider.AssignedColor);
            provider.TryResolveCrossing();
            RunnerColor echoColor =
                _controller.ExperimentSession.EchoColor;
            StageGateView current = FindGate(
                _controller.ExperimentSession.GatesPassed);
            SetDifferentColor(echoColor);
            current.ApplyTemporaryPlan(
                current.ActivePlan.WithTemporaryColorOverride(echoColor),
                _controller.GetPresentationMaterial(echoColor));

            ExperimentGatePlan echoPlan = new ExperimentGatePlan(
                _controller.ExperimentSession.GatesPassed,
                echoColor,
                0,
                current.ActiveExperimentPlan.BaseSpeed,
                current.ActiveExperimentPlan.Cadence,
                current.ActiveExperimentPlan.Spacing,
                MechanicExperimentType.Echo,
                GateModifier.None);
            _controller.ExperimentSession.Resolve(echoPlan);
            _controller.HandleGameplayTap();

            Assert.That(_controller.ExperimentSession.EchoActive, Is.False);
            Assert.That(_controller.ExperimentSession.ShieldActive, Is.True);
            Assert.That(_controller.EchoShellVisual.activeSelf, Is.False);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RetryClearsShellAndReproducesProvider()
        {
            StartEcho();
            EnterPlaying();
            StageGateView provider = PassUntilProvider();
            int providerId = provider.ActiveExperimentPlan.GateId;
            MatchColor(provider.AssignedColor);
            provider.TryResolveCrossing();
            Assert.That(_controller.EchoShellVisual.activeSelf, Is.True);

            _controller.RestartDevelopmentExperiment();
            Assert.That(_controller.EchoShellVisual.activeSelf, Is.False);
            EnterPlaying();
            StageGateView replay = PassUntilProvider();
            Assert.That(replay.ActiveExperimentPlan.GateId, Is.EqualTo(providerId));
            yield return null;
        }

        private void StartEcho(bool shield = false)
        {
            _controller.OpenExperimentLab();
            ExperimentLauncher launcher = FindLauncher();
            SelectEcho(launcher);
            if (shield)
            {
                launcher.ToggleShield();
            }
            launcher.StartExperiment();
        }

        private void EnterPlaying()
        {
            _controller.Tick(3.1f);
            Assert.That(
                _controller.ExperimentSession.FlowState,
                Is.EqualTo(StageFlowState.Playing));
        }

        private StageGateView PassUntilProvider()
        {
            while (true)
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

        private static void SelectEcho(ExperimentLauncher launcher)
        {
            while (launcher.Mechanic != MechanicExperimentType.Echo)
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
