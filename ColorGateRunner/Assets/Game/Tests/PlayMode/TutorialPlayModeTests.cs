using System.Collections;
using ColorGateRunner.Core;
using ColorGateRunner.Presentation;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ColorGateRunner.Tests.PlayMode
{
    public sealed class TutorialPlayModeTests
    {
        private StageSceneController _controller;
        private MemoryProgressStore _store;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
            yield return null;
            _controller = Object.FindAnyObjectByType<StageSceneController>();
            Assert.That(_controller, Is.Not.Null);
            _store = new MemoryProgressStore();
            _controller.SetProgressStoreForTests(_store);
            _controller.SetStartItemInventoryForTests(
                new StartItemInventoryTestGateway());
            _controller.SetContinueServicesForTests(
                new ContinueEconomyTestGateway(),
                new RewardedAdTestService());
            _controller.SetTutorialsEnabledForTests(true);
        }

        [Test]
        public void GuideRun_HoldsBeforeTheFirstGateUntilTheColorMatches()
        {
            StartStage(1);
            Assert.That(_controller.GuideRunActive, Is.True);
            Assert.That(_controller.TutorialGuideText.gameObject.activeSelf, Is.True);
            Assert.That(_controller.TutorialGuideText.text, Does.Contain("TAP ANYWHERE"));

            TickUntilFrozen();
            StageGateView gate = FindGate(0);
            Assert.That(_controller.Session.GatesPassed, Is.Zero);
            Assert.That(_controller.CampaignDistance, Is.LessThan(gate.PathDistance));
            float distance = _controller.CampaignDistance;
            float elapsed = _controller.Session.ElapsedPlayingSeconds;
            // Shorter than the same-color hold, which resumes by itself.
            _controller.Tick(0.5f);
            Assert.That(_controller.CampaignDistance, Is.EqualTo(distance));
            Assert.That(_controller.Session.ElapsedPlayingSeconds, Is.EqualTo(elapsed));
            Assert.That(_controller.StageMusic.IsPaused, Is.True);

            if (_controller.Guide.Step == TutorialGuideStep.HoldSameColor)
            {
                _controller.HandleGameplayTap();
                Assert.That(_controller.Session.CurrentColor, Is.EqualTo(gate.AssignedColor));
            }
            else
            {
                Assert.That(_controller.TutorialGuideText.text, Does.Contain("TAP TO MATCH"));
                _controller.HandleGameplayTap();
                Assert.That(_controller.Session.CurrentColor, Is.EqualTo(gate.AssignedColor));
                _controller.HandleGameplayTap();
                Assert.That(_controller.Session.CurrentColor, Is.EqualTo(gate.AssignedColor),
                    "A matched hold ignores further taps.");
            }

            for (int frame = 0; frame < 200 && _controller.Session.GatesPassed == 0; frame++)
            {
                _controller.Tick(0.05f);
            }
            Assert.That(_controller.Session.GatesPassed, Is.EqualTo(1));
            Assert.That(_controller.Session.FlowState, Is.EqualTo(StageFlowState.Playing));
            Assert.That(_controller.StageMusic.IsPaused, Is.False);
        }

        [Test]
        public void GuideRun_IsSkippedOnceStageOneIsCleared()
        {
            _store.Records[0] = new StageRecord(true, 30f, 30f, 1);
            StartStage(1);
            Assert.That(_controller.GuideRunActive, Is.False);
            Assert.That(_controller.TutorialGuideText.gameObject.activeSelf, Is.False);
        }

        [Test]
        public void GuideRun_FailureNamesBothColors()
        {
            StartStage(1);
            StageGateView gate = FindGate(0);
            if (_controller.Session.CurrentColor == gate.AssignedColor)
            {
                _controller.HandleGameplayTap();
            }
            gate.TryResolveCrossing();
            Assert.That(_controller.Session.FlowState, Is.EqualTo(StageFlowState.Failed));
            _controller.Tick(3f);
            Assert.That(_controller.FailDetailsText.text, Does.Contain("COLOR MISMATCH"));
            Assert.That(_controller.FailDetailsText.text, Does.Contain("GATES LEFT"));
        }

        [Test]
        public void GuideRun_FirstClearShowsTheCrownCardAfterTheCelebration()
        {
            StartStage(1);
            int index = 0;
            while (_controller.Session.RemainingGates > 0)
            {
                StageGateView gate = _controller.GetGate(index % 6);
                for (int tap = 0; tap < 3 && _controller.Session.CurrentColor != gate.AssignedColor; tap++)
                {
                    _controller.HandleGameplayTap();
                }
                gate.TryResolveCrossing();
                index++;
            }
            float remaining = _controller.GoalPathDistance - _controller.CampaignDistance + 1f;
            _controller.TickMovement(remaining / _controller.Session.CurrentSpeed);
            Assert.That(_controller.Session.FlowState, Is.EqualTo(StageFlowState.StageCleared));

            _controller.Tick(_controller.ClearPanelDelaySeconds + 0.01f);
            Assert.That(_controller.ResultPage, Is.EqualTo(CampaignResultPage.ClearCelebration));
            Assert.That(_controller.TutorialCard.IsOpen, Is.False);
            for (int frame = 0; frame < 100 && !_controller.TutorialCard.IsOpen; frame++)
            {
                _controller.Tick(0.1f);
            }
            Assert.That(_controller.ResultPage, Is.EqualTo(CampaignResultPage.ClearRewards));
            Assert.That(_controller.TutorialCard.IsOpen, Is.True);
            Assert.That(_controller.TutorialCard.Title.text, Is.EqualTo("CROWNS"));
            _controller.HandleBack();
            Assert.That(_controller.TutorialCard.IsOpen, Is.False);
            Assert.That(_controller.ResultPage, Is.EqualTo(CampaignResultPage.ClearRewards));
        }

        [Test]
        public void StageIntro_ShowsOnTheFirstVisitOfANewMechanicOnly()
        {
            _store.HighestUnlocked = StageCatalog.Count;
            _controller.SetProgressStoreForTests(_store);
            _controller.SetTutorialsEnabledForTests(true);

            _controller.SelectStage(9);
            Assert.That(_controller.TutorialCard.IsOpen, Is.True);
            Assert.That(_controller.TutorialCard.Title.text, Is.EqualTo("CAMOUFLAGE"));
            _controller.TutorialCard.PrimaryButton.onClick.Invoke();
            Assert.That(_controller.TutorialCard.IsOpen, Is.False);

            _controller.SelectStage(9);
            Assert.That(_controller.TutorialCard.IsOpen, Is.False, "Shown once per visit.");

            _controller.SelectStage(10);
            Assert.That(_controller.TutorialCard.IsOpen, Is.False, "Practice Stage has no intro.");

            _controller.SelectStage(15);
            Assert.That(_controller.TutorialCard.Title.text, Is.EqualTo("ICE"));
            _controller.TutorialCard.Close();
            _controller.SelectStage(9);
            Assert.That(_controller.TutorialCard.IsOpen, Is.False, "Remembered across Stages.");

            _store.Records[11] = new StageRecord(false, 0f, 0f, 0, 0, 0, StageCrown.None, 1);
            _controller.SelectStage(12);
            Assert.That(_controller.TutorialCard.IsOpen, Is.False, "Already attempted.");
        }

        private void StartStage(int stageNumber)
        {
            _store.HighestUnlocked = StageCatalog.Count;
            _controller.SetProgressStoreForTests(_store);
            _controller.SetTutorialsEnabledForTests(true);
            _controller.SelectStage(stageNumber);
            _controller.StartSelectedStage();
            _controller.Tick(3.1f);
            Assert.That(_controller.Session.FlowState, Is.EqualTo(StageFlowState.Playing));
        }

        private void TickUntilFrozen()
        {
            for (int frame = 0; frame < 400 && !_controller.Guide.Frozen; frame++)
            {
                _controller.Tick(0.02f);
            }
            Assert.That(_controller.Guide.Frozen, Is.True);
        }

        private StageGateView FindGate(int planIndex)
        {
            for (int slot = 0; slot < 6; slot++)
            {
                StageGateView gate = _controller.GetGate(slot);
                if (gate.gameObject.activeSelf && gate.PlanIndex == planIndex)
                {
                    return gate;
                }
            }
            Assert.Fail($"Gate {planIndex} is not active.");
            return null;
        }

        private sealed class MemoryProgressStore : IStageProgressStore
        {
            internal int HighestUnlocked { get; set; } = 1;
            internal StageRecord[] Records { get; } = new StageRecord[StageCatalog.Count];

            public int LoadHighestUnlocked() => HighestUnlocked;
            public StageRecord LoadRecord(int stageNumber) => Records[stageNumber - 1];
            public void SaveHighestUnlocked(int stageNumber) => HighestUnlocked = stageNumber;
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
