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
        public void GuideRun_OwnMusicClockStopsDuringTheHoldAndFollowsAfter()
        {
            // The WebGL path: the track counts its own playback.
            StageMusicPlayer music = _controller.StageMusic;
            music.SetCountOwnClockForTests(true);
            StartStage(1);
            TickUntilFrozen();
            float held = music.PlaybackSeconds;
            _controller.Tick(0.5f);
            Assert.That(music.PlaybackSeconds, Is.EqualTo(held));

            if (_controller.Guide.Step != TutorialGuideStep.HoldSameColor)
            {
                _controller.HandleGameplayTap();
            }
            for (int frame = 0; frame < 200 && _controller.Session.GatesPassed == 0; frame++)
            {
                _controller.Tick(0.05f);
            }
            for (int frame = 0; frame < 10; frame++)
            {
                _controller.Tick(0.05f);
            }
            Assert.That(_controller.Session.GatesPassed, Is.GreaterThanOrEqualTo(1));
            Assert.That(music.PlaybackSeconds, Is.EqualTo(
                music.Timeline.TimeAtDistance(_controller.CampaignDistance))
                .Within(0.25f));
        }

        [Test]
        public void GuideRun_ShowsTheTapLineDuringTheCountdown()
        {
            _store.HighestUnlocked = StageCatalog.Count;
            _controller.SetProgressStoreForTests(_store);
            _controller.SetTutorialsEnabledForTests(true);
            _controller.SelectStage(1);
            _controller.StartSelectedStage();
            _controller.Tick(1f);
            Assert.That(_controller.Session.FlowState, Is.EqualTo(StageFlowState.Countdown));
            Assert.That(_controller.TutorialGuideText.gameObject.activeSelf, Is.True);
            Assert.That(_controller.TutorialGuideText.text, Does.Contain("TAP ANYWHERE"));
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

            _controller.SelectStage(7);
            Assert.That(_controller.TutorialCard.IsOpen, Is.True);
            Assert.That(_controller.TutorialCard.Title.text, Is.EqualTo("CAMOUFLAGE"));
            _controller.TutorialCard.PrimaryButton.onClick.Invoke();
            Assert.That(_controller.TutorialCard.IsOpen, Is.False);

            _controller.SelectStage(7);
            Assert.That(_controller.TutorialCard.IsOpen, Is.False, "Shown once per visit.");

            _controller.SelectStage(8);
            Assert.That(_controller.TutorialCard.IsOpen, Is.False, "Practice Stage has no intro.");

            _controller.SelectStage(11);
            Assert.That(_controller.TutorialCard.Title.text, Is.EqualTo("ICE"));
            _controller.TutorialCard.Close();
            _controller.SelectStage(7);
            Assert.That(_controller.TutorialCard.IsOpen, Is.False, "Remembered across Stages.");

            _store.Records[8] = new StageRecord(false, 0f, 0f, 0, 0, 0, StageCrown.None, 1);
            _controller.SelectStage(9);
            Assert.That(_controller.TutorialCard.IsOpen, Is.False, "Already attempted.");
        }

        [Test]
        public void MechanicGuide_HoldsTheFirstGreenGate()
        {
            StageGateView gate = RunToMechanicHold(13);
            Assert.That(gate.ActivePlan.Color, Is.EqualTo(RunnerColor.Green));
            Assert.That(_controller.Guide.GateColor, Is.EqualTo(RunnerColor.Green));
            Assert.That(gate.PlanIndex, Is.EqualTo(_controller.Session.GatesPassed));
            Assert.That(_controller.TutorialGuideText.text, Does.Contain("GREEN"));
            AssertWaitsForMatchThenPasses(gate);
        }

        [Test]
        public void MechanicGuide_HoldsCamouflageAfterTheReveal()
        {
            StageGateView gate = RunToMechanicHold(7);
            Assert.That(gate.ActivePlan.Modifier.IsCamouflage, Is.True);
            Assert.That(gate.CamouflageRevealProgress, Is.EqualTo(1f).Within(0.001f));
            Assert.That(_controller.TutorialGuideText.text, Does.Contain("COLOR REVEALED"));
            AssertWaitsForMatchThenPasses(gate);
        }

        [Test]
        public void MechanicGuide_HoldsTheFirstIceGate()
        {
            StageGateView gate = RunToMechanicHold(11);
            Assert.That(gate.ActivePlan.Modifier.IsIce, Is.True);
            Assert.That(_controller.TutorialGuideText.text, Does.Contain("ICE"));
            AssertWaitsForMatchThenPasses(gate);
        }

        [Test]
        public void MechanicGuide_HoldsTheEchoGateThenShowsTheStoredColor()
        {
            StageGateView gate = RunToMechanicHold(18);
            Assert.That(gate.ActivePlan.Modifier.IsEchoProvider, Is.True);
            Assert.That(_controller.TutorialGuideText.text, Does.Contain("ECHO GATE"));
            AssertWaitsForMatchThenPasses(gate);
            Assert.That(_controller.Session.EchoActive, Is.True);
            Assert.That(_controller.TutorialGuideText.text, Does.Contain("ECHO STORED"));
        }

        [Test]
        public void MechanicGuide_HoldsHiddenWhileItsColorShowsThenResumes()
        {
            StageGateView gate = RunToMechanicHold(21);
            Assert.That(gate.ActivePlan.Modifier.IsHidden, Is.True);
            Assert.That(gate.HiddenHideStarted, Is.False);
            Assert.That(_controller.Guide.Step, Is.EqualTo(TutorialGuideStep.HoldTimed));
            Assert.That(_controller.TutorialGuideText.text,
                Does.Contain("REMEMBER THIS GATE: " + gate.ActivePlan.Color.ToString().ToUpperInvariant()));
            RunnerColor before = _controller.Session.CurrentColor;
            _controller.HandleGameplayTap();
            Assert.That(_controller.Session.CurrentColor, Is.EqualTo(before), "Taps wait during the hold.");
            for (int frame = 0; frame < 300 && !gate.HiddenHideStarted; frame++)
            {
                StageGateView next = FindActive(_controller.Session.GatesPassed);
                if (next != null) AutoMatch(next);
                _controller.Tick(0.02f);
            }
            Assert.That(_controller.Guide.Frozen, Is.False);
            Assert.That(gate.HiddenHideStarted, Is.True);
        }

        [Test]
        public void MechanicGuide_HoldsFlickerAfterItLocks()
        {
            StageGateView gate = RunToMechanicHold(24);
            Assert.That(gate.ActivePlan.Modifier.IsFlicker, Is.True);
            Assert.That(gate.FlickerWindow.IsLocked, Is.True);
            Assert.That(gate.FlickerWindow.Accepts(_controller.Guide.GateColor), Is.True);
            Assert.That(_controller.TutorialGuideText.text, Does.Contain("LOCKED ON"));
            AssertWaitsForMatchThenPasses(gate);
        }

        [Test]
        public void MechanicGuide_IsSkippedOnceTheStageIsCleared()
        {
            _store.Records[6] = new StageRecord(true, 40f, 40f, 1);
            StartStage(7);
            Assert.That(_controller.GuideRunActive, Is.False);
        }

        // Plays the Stage, matching every gate except the guide's target,
        // until the guide holds the run.
        private StageGateView RunToMechanicHold(int stageNumber)
        {
            StartStage(stageNumber);
            Assert.That(_controller.GuideRunActive, Is.True);
            TutorialScript script = _controller.Guide.Script;
            for (int frame = 0; frame < 3000 && !_controller.Guide.Frozen; frame++)
            {
                StageGateView next = FindActive(_controller.Session.GatesPassed);
                if (next != null && !script.Matches(next.ActivePlan, RunnerColor.Green))
                {
                    AutoMatch(next);
                }
                _controller.Tick(0.02f);
                Assert.That(_controller.Session.FlowState,
                    Is.Not.EqualTo(StageFlowState.Failed), $"stage {stageNumber} frame {frame}");
            }
            Assert.That(_controller.Guide.Frozen, Is.True, $"stage {stageNumber} never held");
            for (int slot = 0; slot < 6; slot++)
            {
                StageGateView gate = _controller.GetGate(slot);
                if (gate.gameObject.activeSelf && gate.PlanIndex == _controller.Guide.HeldGateIndex)
                {
                    return gate;
                }
            }
            Assert.Fail("Held gate is not active.");
            return null;
        }

        private void AssertWaitsForMatchThenPasses(StageGateView gate)
        {
            float distance = _controller.CampaignDistance;
            _controller.Tick(0.5f);
            Assert.That(_controller.CampaignDistance, Is.EqualTo(distance));
            Assert.That(_controller.StageMusic.IsPaused, Is.True);
            int index = gate.PlanIndex;
            for (int tap = 0; tap < 3 &&
                _controller.Guide.Step == TutorialGuideStep.HoldNeedsTap; tap++)
            {
                _controller.HandleGameplayTap();
            }
            Assert.That(_controller.Guide.Step,
                Is.EqualTo(TutorialGuideStep.HoldMatched).Or.EqualTo(TutorialGuideStep.HoldSameColor));
            for (int frame = 0; frame < 300 && _controller.Session.GatesPassed <= index; frame++)
            {
                _controller.Tick(0.02f);
            }
            Assert.That(_controller.Session.GatesPassed, Is.GreaterThan(index));
            Assert.That(_controller.Session.FlowState, Is.Not.EqualTo(StageFlowState.Failed));
        }

        private void AutoMatch(StageGateView gate)
        {
            if (!_controller.Guide.AcceptsTap) return;
            for (int tap = 0; tap < 3 && !Accepts(gate, _controller.Session.CurrentColor); tap++)
            {
                _controller.HandleGameplayTap();
            }
        }

        private static bool Accepts(StageGateView gate, RunnerColor color) =>
            gate.ActivePlan.Modifier.IsFlicker
                ? gate.FlickerWindow.Accepts(color)
                : gate.ActivePlan.Color == color;

        private StageGateView FindActive(int planIndex)
        {
            for (int slot = 0; slot < 6; slot++)
            {
                StageGateView gate = _controller.GetGate(slot);
                if (gate.gameObject.activeSelf && !gate.HasResolved && gate.PlanIndex == planIndex)
                {
                    return gate;
                }
            }
            return null;
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
