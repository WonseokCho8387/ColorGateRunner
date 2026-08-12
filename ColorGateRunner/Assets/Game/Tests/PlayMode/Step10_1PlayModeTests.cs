using System.Collections;
using ColorGateRunner.Core;
using ColorGateRunner.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace ColorGateRunner.Tests.PlayMode
{
    public sealed class Step10_1PlayModeTests
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
            _controller.SetContinueServicesForTests(
                new ContinueEconomyTestGateway(),
                new RewardedAdTestService());
        }

        [Test]
        public void Stage4_HudShowsThreeColorsImmediately()
        {
            StartStage(4, completeCountdown: false);

            Assert.That(_controller.ActiveColorTileCount, Is.EqualTo(3));
            Assert.That(_controller.GetColorTile(2).activeSelf, Is.True);
        }

        [Test]
        public void Stage4_EarlyGatesDoNotUseGreen()
        {
            StartStage(4);
            int firstGreen = _controller.Session.Stage.GetFirstGateIndex(
                RunnerColor.Green);

            for (int index = 0; index < firstGreen; index++)
            {
                StageGateView gate = FindGate(index);
                Assert.That(gate, Is.Not.Null);
                Assert.That(gate.AssignedColor, Is.Not.EqualTo(RunnerColor.Green));
                Pass(gate);
            }
        }

        [Test]
        public void Stage4_LaterGateUsesGreen()
        {
            StartStage(4);
            int firstGreen = _controller.Session.Stage.GetFirstGateIndex(
                RunnerColor.Green);

            for (int index = 0; index < firstGreen; index++)
            {
                Pass(FindGate(index));
            }

            Assert.That(FindGate(firstGreen).AssignedColor,
                Is.EqualTo(RunnerColor.Green));
        }

        [Test]
        public void Goal_IsVisibleAtLongRangeAndApproachesContinuously()
        {
            StartStage(4);
            float initialGoalZ = _controller.Goal.transform.position.z;
            float initialDistance =
                initialGoalZ - _controller.PlayerTransform.position.z;

            _controller.TickMovement(0.5f);
            float movedDistance =
                initialGoalZ - _controller.PlayerTransform.position.z;

            Assert.That(_controller.Goal.activeSelf, Is.True);
            Assert.That(initialDistance, Is.GreaterThan(500f));
            Assert.That(_controller.Goal.transform.position.z,
                Is.EqualTo(initialGoalZ));
            Assert.That(movedDistance, Is.LessThan(initialDistance));
        }

        [Test]
        public void Goal_DoesNotPopOrTeleportAtFinalGate()
        {
            StartStage(1);
            Vector3 plannedPosition = _controller.Goal.transform.position;

            for (int index = 0;
                index < _controller.Session.Stage.TargetGateCount;
                index++)
            {
                Pass(FindGate(index));
            }

            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.StageFinishing));
            Assert.That(_controller.Goal.activeSelf, Is.True);
            Assert.That(_controller.Goal.transform.position,
                Is.EqualTo(plannedPosition));
        }

        [Test]
        public void Continue_ResetsPlayerToCleanUprightPose()
        {
            StartStage(1);
            FailAtCurrentGate(out _);
            Rigidbody body =
                _controller.PlayerTransform.GetComponent<Rigidbody>();
            body.linearVelocity = new Vector3(1f, 2f, 3f);
            body.angularVelocity = new Vector3(3f, 2f, 1f);

            _controller.RequestCoinContinue();

            Assert.That(_controller.PlayerTransform.localRotation,
                Is.EqualTo(Quaternion.identity));
            Assert.That(_controller.PlayerTransform.localScale,
                Is.EqualTo(Vector3.one));
            Assert.That(_controller.PlayerRenderer.sharedMaterial,
                Is.EqualTo(_controller.GetPresentationMaterial(
                    _controller.Session.CurrentColor)));
            Assert.That(body.linearVelocity, Is.EqualTo(Vector3.zero));
            Assert.That(body.angularVelocity, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void Continue_ConsumesFailedGateAndLeavesReadableNextGate()
        {
            StartStage(1);
            StageSession sameSession = _controller.Session;
            FailAtCurrentGate(out StageGateView failed);
            Vector3 goalPosition = _controller.Goal.transform.position;

            _controller.RequestCoinContinue();
            StageGateView next = FindGate(_controller.Session.GatesPassed);

            Assert.That(_controller.Session, Is.SameAs(sameSession));
            Assert.That(failed.gameObject.activeSelf, Is.False);
            Assert.That(next, Is.Not.Null);
            Assert.That(
                next.transform.position.z -
                _controller.PlayerTransform.position.z,
                Is.GreaterThanOrEqualTo(47.99f));
            Assert.That(_controller.Goal.transform.position,
                Is.EqualTo(goalPosition));
            Assert.That(_controller.Session.FailedGatePendingForContinue,
                Is.False);
        }

        [Test]
        public void ExperimentLab_OpensFromLobbyInDevelopmentFlow()
        {
            Assert.That(_controller.ExperimentLabButton.gameObject.activeSelf,
                Is.True);

            _controller.OpenExperimentLab();
            ExperimentLauncher launcher = FindLauncher();

            Assert.That(_controller.UiFlow, Is.EqualTo(MobileUiFlow.Development));
            Assert.That(_controller.DevelopmentDebugRoot.activeSelf, Is.True);
            Assert.That(_controller.StageSelectPanel.activeSelf, Is.False);
            Assert.That(launcher.gameObject.activeInHierarchy, Is.True);
        }

        [Test]
        public void ExperimentLab_ShortlistedConditionsLaunchWithoutProgression()
        {
            int[] colors = { 4, 4, 4, 4, 5, 6 };
            MechanicExperimentType[] mechanics =
            {
                MechanicExperimentType.None,
                MechanicExperimentType.Camouflage,
                MechanicExperimentType.Fog,
                MechanicExperimentType.Ice,
                MechanicExperimentType.None,
                MechanicExperimentType.None
            };

            for (int index = 0; index < colors.Length; index++)
            {
                _controller.OpenExperimentLab();
                ExperimentLauncher launcher = FindLauncher();
                SelectCondition(launcher, colors[index], mechanics[index]);
                launcher.StartExperiment();

                Assert.That(_controller.ExperimentActive, Is.True);
                Assert.That(_controller.ExperimentSession.Definition.ColorCount,
                    Is.EqualTo(colors[index]));
                Assert.That(_controller.ExperimentSession.Definition.Mechanic,
                    Is.EqualTo(mechanics[index]));
                Assert.That(launcher.Label,
                    Does.Contain(colors[index] + " COLORS"));
                Assert.That(
                    GameObject.Find("StageHudTitle")
                        .GetComponent<UnityEngine.UI.Text>().text,
                    Does.Contain(colors[index] + " COLORS"));
                launcher.LeaveExperiment();
            }

            Assert.That(_store.HighestUnlocked, Is.EqualTo(5));
            Assert.That(_store.SavedRecordCount, Is.Zero);
        }

        [Test]
        public void ExperimentLab_IsHiddenForReleasePolicy()
        {
            Assert.That(
                StageSceneController.ShouldShowExperimentLab(false, false),
                Is.False);
            Assert.That(
                StageSceneController.ShouldShowExperimentLab(true, false),
                Is.True);
            Assert.That(
                StageSceneController.ShouldShowExperimentLab(false, true),
                Is.True);
        }

        [Test]
        public void ExperimentRuntime_CountdownBlocksProgressThenStartsPlaying()
        {
            StartExperiment(out _);
            Vector3 start = _controller.PlayerTransform.position;
            RunnerColor color = _controller.ExperimentSession.CurrentColor;
            StageGateView first = FindExperimentGate(0);

            Assert.That(_controller.UiFlow,
                Is.EqualTo(MobileUiFlow.Countdown));
            Assert.That(_controller.CountdownRoot.activeSelf, Is.True);
            _controller.HandleGameplayTap();
            Assert.That(first.TryResolveCrossing(), Is.False);
            _controller.Tick(1f);

            Assert.That(_controller.PlayerTransform.position, Is.EqualTo(start));
            Assert.That(_controller.ExperimentSession.CurrentColor,
                Is.EqualTo(color));
            Assert.That(_controller.ExperimentSession.ElapsedPlayingSeconds,
                Is.Zero);

            _controller.Tick(2.1f);
            Assert.That(_controller.ExperimentSession.FlowState,
                Is.EqualTo(StageFlowState.Playing));
            Assert.That(_controller.UiFlow,
                Is.EqualTo(MobileUiFlow.Gameplay));

            _controller.Tick(0.1f);
            Assert.That(_controller.PlayerTransform.position.z,
                Is.GreaterThan(start.z));
            Assert.That(_controller.ExperimentSession.ElapsedPlayingSeconds,
                Is.GreaterThan(0f));
        }

        [Test]
        public void ExperimentRuntime_FailureShowsResultAndRetryReplaysCondition()
        {
            StartExperimentPlaying(out ExperimentLauncher launcher);
            ExperimentDefinition definition =
                _controller.ExperimentSession.Definition;
            RunnerColor firstColor = FindExperimentGate(0).AssignedColor;
            FailExperimentAtCurrentGate();
            Vector3 failedPosition = _controller.PlayerTransform.position;
            RunnerColor failedColor =
                _controller.ExperimentSession.CurrentColor;

            Assert.That(_controller.ExperimentSession.FlowState,
                Is.EqualTo(StageFlowState.Failed));
            Assert.That(_controller.FailedResultRoot.activeSelf, Is.True);
            _controller.Tick(1.1f);
            Assert.That(_controller.FailPanel.activeSelf, Is.True);
            Assert.That(_controller.CoinContinueButton.gameObject.activeSelf,
                Is.False);
            Assert.That(_controller.RewardedContinueButton.gameObject.activeSelf,
                Is.False);
            Assert.That(ButtonLabel(_controller.RetryButton),
                Is.EqualTo("RETRY SAME TEST"));

            _controller.HandleGameplayTap();
            _controller.Tick(0.5f);
            Assert.That(_controller.PlayerTransform.position,
                Is.EqualTo(failedPosition));
            Assert.That(_controller.ExperimentSession.CurrentColor,
                Is.EqualTo(failedColor));

            _controller.RetryButton.onClick.Invoke();

            Assert.That(_controller.ExperimentSession.Definition,
                Is.SameAs(definition));
            Assert.That(_controller.ExperimentSession.Definition.Seed,
                Is.EqualTo(launcher.Seed));
            Assert.That(_controller.ExperimentSession.FlowState,
                Is.EqualTo(StageFlowState.Countdown));
            Assert.That(_controller.ExperimentSession.GatesPassed, Is.Zero);
            Assert.That(_controller.ExperimentSession.ElapsedPlayingSeconds,
                Is.Zero);
            Assert.That(FindExperimentGate(0).AssignedColor,
                Is.EqualTo(firstColor));
            Assert.That(_controller.CountdownRoot.activeSelf, Is.True);
            Assert.That(_controller.FailPanel.activeSelf, Is.False);
        }

        [Test]
        public void ExperimentRuntime_CompletionShowsResultAndReplayRestarts()
        {
            StartExperimentPlaying(out _);
            ExperimentDefinition definition =
                _controller.ExperimentSession.Definition;
            RunnerColor firstColor = FindExperimentGate(0).AssignedColor;

            while (_controller.ExperimentSession.FlowState ==
                StageFlowState.Playing)
            {
                StageGateView gate = FindExperimentGate(
                    _controller.ExperimentSession.GatesPassed);
                MatchExperimentColor(gate.AssignedColor);
                Assert.That(gate.TryResolveCrossing(), Is.True);
            }

            Assert.That(_controller.ExperimentSession.FlowState,
                Is.EqualTo(StageFlowState.StageCleared));
            Assert.That(_controller.ClearResultRoot.activeSelf, Is.True);
            _controller.Tick(1.3f);
            Assert.That(_controller.ClearPanel.activeSelf, Is.True);
            Assert.That(ButtonLabel(_controller.ReplayButton),
                Is.EqualTo("REPLAY"));
            Assert.That(ButtonLabel(_controller.ClearLobbyButton),
                Is.EqualTo("BACK TO LAB"));

            _controller.ReplayButton.onClick.Invoke();

            Assert.That(_controller.ExperimentSession.Definition,
                Is.SameAs(definition));
            Assert.That(_controller.ExperimentSession.FlowState,
                Is.EqualTo(StageFlowState.Countdown));
            Assert.That(_controller.ExperimentSession.GatesPassed, Is.Zero);
            Assert.That(FindExperimentGate(0).AssignedColor,
                Is.EqualTo(firstColor));
        }

        [Test]
        public void ExperimentRuntime_BackToLabDoesNotReadOrWriteProgress()
        {
            int loadCount = _store.LoadCount;
            int highest = _store.HighestUnlocked;
            int saved = _store.SavedRecordCount;
            StartExperimentPlaying(out _);
            FailExperimentAtCurrentGate();
            _controller.Tick(1.1f);

            _controller.FailLobbyButton.onClick.Invoke();

            Assert.That(_controller.ExperimentActive, Is.False);
            Assert.That(_controller.UiFlow,
                Is.EqualTo(MobileUiFlow.Development));
            Assert.That(FindLauncher().gameObject.activeInHierarchy, Is.True);
            Assert.That(_store.LoadCount, Is.EqualTo(loadCount));
            Assert.That(_store.HighestUnlocked, Is.EqualTo(highest));
            Assert.That(_store.SavedRecordCount, Is.EqualTo(saved));
        }

        [Test]
        public void ExperimentRuntime_ReentryKeepsSingleRuntimeObjectGraph()
        {
            int controllerCount = Object.FindObjectsByType<StageSceneController>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None).Length;
            int launcherCount = Object.FindObjectsByType<ExperimentLauncher>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None).Length;
            int gateCount = Object.FindObjectsByType<StageGateView>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None).Length;

            for (int index = 0; index < 5; index++)
            {
                StartExperimentPlaying(out _);
                FailExperimentAtCurrentGate();
                _controller.Tick(1.1f);
                _controller.FailLobbyButton.onClick.Invoke();
            }

            Assert.That(Object.FindObjectsByType<StageSceneController>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None).Length,
                Is.EqualTo(controllerCount));
            Assert.That(Object.FindObjectsByType<ExperimentLauncher>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None).Length,
                Is.EqualTo(launcherCount));
            Assert.That(Object.FindObjectsByType<StageGateView>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None).Length,
                Is.EqualTo(gateCount));
        }

        private void StartStage(int number, bool completeCountdown = true)
        {
            _store.HighestUnlocked = 5;
            _controller.SetProgressStoreForTests(_store);
            _controller.SelectStage(number);
            _controller.StartSelectedStage();
            if (completeCountdown)
            {
                _controller.Tick(3.1f);
                Assert.That(_controller.Session.FlowState,
                    Is.EqualTo(StageFlowState.Playing));
            }
        }

        private void StartExperiment(out ExperimentLauncher launcher)
        {
            _controller.OpenExperimentLab();
            launcher = FindLauncher();
            launcher.StartExperiment();
        }

        private void StartExperimentPlaying(out ExperimentLauncher launcher)
        {
            StartExperiment(out launcher);
            _controller.Tick(3.1f);
            Assert.That(_controller.ExperimentSession.FlowState,
                Is.EqualTo(StageFlowState.Playing));
        }

        private void FailExperimentAtCurrentGate()
        {
            StageGateView gate = FindExperimentGate(
                _controller.ExperimentSession.GatesPassed);
            if (_controller.ExperimentSession.CurrentColor ==
                gate.AssignedColor)
            {
                _controller.HandleGameplayTap();
            }
            Assert.That(gate.TryResolveCrossing(), Is.True);
        }

        private void MatchExperimentColor(RunnerColor color)
        {
            while (_controller.ExperimentSession.CurrentColor != color)
            {
                _controller.HandleGameplayTap();
            }
        }

        private StageGateView FindExperimentGate(int gateIndex)
        {
            for (int index = 0; index < _controller.GatePoolSize; index++)
            {
                StageGateView gate = _controller.GetGate(index);
                if (gate.gameObject.activeSelf &&
                    gate.HasExperimentPlan &&
                    gate.ActiveExperimentPlan.GateIndex == gateIndex)
                {
                    return gate;
                }
            }
            Assert.Fail($"Experiment gate {gateIndex} was not active.");
            return null;
        }

        private static string ButtonLabel(Button button)
        {
            return button.GetComponentInChildren<Text>(true).text;
        }

        private void Pass(StageGateView gate)
        {
            Assert.That(gate, Is.Not.Null);
            while (_controller.Session.CurrentColor != gate.AssignedColor)
            {
                _controller.HandleGameplayTap();
            }
            Assert.That(gate.TryResolveCrossing(), Is.True);
        }

        private void FailAtCurrentGate(out StageGateView failed)
        {
            failed = FindGate(_controller.Session.GatesPassed);
            if (_controller.Session.CurrentColor == failed.AssignedColor)
            {
                _controller.HandleGameplayTap();
            }
            Assert.That(failed.TryResolveCrossing(), Is.True);
            Assert.That(_controller.Session.FlowState,
                Is.EqualTo(StageFlowState.Failed));
        }

        private StageGateView FindGate(int planIndex)
        {
            for (int index = 0; index < _controller.GatePoolSize; index++)
            {
                StageGateView gate = _controller.GetGate(index);
                if (gate.gameObject.activeSelf && gate.PlanIndex == planIndex)
                {
                    return gate;
                }
            }
            return null;
        }

        private static ExperimentLauncher FindLauncher()
        {
            ExperimentLauncher[] launchers =
                Object.FindObjectsByType<ExperimentLauncher>(
                    FindObjectsInactive.Include);
            Assert.That(launchers.Length, Is.EqualTo(1));
            return launchers[0];
        }

        private static void SelectCondition(
            ExperimentLauncher launcher,
            int colorCount,
            MechanicExperimentType mechanic)
        {
            while (launcher.ColorCount != colorCount)
            {
                launcher.NextColorCount();
            }
            while (launcher.Mechanic != mechanic)
            {
                launcher.NextMechanic();
            }
            Assert.That(launcher.SelectedDefinition.ColorCount,
                Is.EqualTo(colorCount));
            Assert.That(launcher.SelectedDefinition.Mechanic,
                Is.EqualTo(mechanic));
        }

        private sealed class InMemoryStageProgressStore :
            IStageProgressStore
        {
            private readonly StageRecord[] _records =
                new StageRecord[StageCatalog.Count];

            public int HighestUnlocked = 5;
            public int SavedRecordCount;
            public int LoadCount;

            public int LoadHighestUnlocked()
            {
                LoadCount++;
                return HighestUnlocked;
            }
            public StageRecord LoadRecord(int stageNumber)
            {
                LoadCount++;
                return _records[stageNumber - 1];
            }
            public void SaveHighestUnlocked(int stageNumber) =>
                HighestUnlocked = stageNumber;
            public void SaveRecord(int stageNumber, StageRecord record)
            {
                _records[stageNumber - 1] = record;
                SavedRecordCount++;
            }
            public void ClearGameplayProgress()
            {
                System.Array.Clear(_records, 0, _records.Length);
                HighestUnlocked = 1;
                SavedRecordCount = 0;
            }
        }
    }
}
