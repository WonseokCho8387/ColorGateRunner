using System.Collections.Generic;
using ColorGateRunner.Core;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class TutorialTests
    {
        private static TutorialGuide BeginBasics()
        {
            var guide = new TutorialGuide();
            guide.Begin(TutorialScripts.For(TutorialTopic.Basics), RunnerColor.Green);
            return guide;
        }

        [Test]
        public void Guide_MismatchHoldWaitsForTheMatchingTap()
        {
            TutorialGuide guide = BeginBasics();
            guide.OnEvent(TutorialEvent.Go);
            Assert.That(guide.Message, Does.Contain("TAP ANYWHERE"));
            Assert.That(guide.TryGetHold(0, default), Is.True);
            Assert.That(
                guide.Script.HoldLeadDistance(40f, 60f, null),
                Is.EqualTo(40f * TutorialGuide.HoldLeadSeconds));

            guide.Hold(0, RunnerColor.Blue, RunnerColor.Red);
            Assert.That(guide.Frozen, Is.True);
            Assert.That(guide.AcceptsTap, Is.True);
            Assert.That(guide.Message, Does.Contain("GATE IS BLUE"));
            Assert.That(guide.Message, Does.Contain("CHECKED AS YOU PASS THROUGH"));
            string held = guide.Message;
            guide.Tick(30f);
            Assert.That(guide.Step, Is.EqualTo(TutorialGuideStep.HoldNeedsTap));
            Assert.That(guide.Message, Is.SameAs(held), "No per-frame rebuild.");

            guide.OnColorChanged(RunnerColor.Blue);
            Assert.That(guide.Step, Is.EqualTo(TutorialGuideStep.HoldMatched));
            Assert.That(guide.AcceptsTap, Is.False);
            guide.Tick(TutorialGuide.MatchedHoldSeconds + 0.01f);
            Assert.That(guide.Frozen, Is.False);
            Assert.That(guide.Step, Is.EqualTo(TutorialGuideStep.Passing));
            Assert.That(guide.AcceptsTap, Is.False,
                "Taps cannot undo the match before the gate.");
            Assert.That(guide.Message, Does.Contain("MATCHED"));
            Assert.That(guide.TryGetHold(1, default), Is.False);

            guide.OnGateResolved(0);
            Assert.That(guide.Step, Is.EqualTo(TutorialGuideStep.Running));
            Assert.That(guide.AcceptsTap, Is.True);
            Assert.That(guide.TryGetHold(0, default), Is.False);
            Assert.That(guide.Message, Is.Empty, "The GO line ends at the first hold.");
        }

        [Test]
        public void Guide_SameColorHoldBlocksTapsThenResumes()
        {
            TutorialGuide guide = BeginBasics();
            guide.Hold(0, RunnerColor.Red, RunnerColor.Red);
            Assert.That(guide.Step, Is.EqualTo(TutorialGuideStep.HoldSameColor));
            Assert.That(guide.AcceptsTap, Is.False);
            guide.Tick(TutorialGuide.SameColorHoldSeconds - 0.1f);
            Assert.That(guide.Frozen, Is.True);
            guide.Tick(0.2f);
            Assert.That(guide.Frozen, Is.False);
            Assert.That(guide.AcceptsTap, Is.False, "Carried through the gate.");
            guide.OnGateResolved(0);
            Assert.That(guide.AcceptsTap, Is.True);
        }

        [Test]
        public void Guide_MechanicNoticeShowsALineWithoutStopping()
        {
            var guide = new TutorialGuide();
            guide.Begin(TutorialScripts.For(TutorialTopic.Ice), RunnerColor.Green);
            guide.Hold(4, RunnerColor.Blue, RunnerColor.Red);
            Assert.That(guide.HoldsDone, Is.EqualTo(1));
            Assert.That(guide.Frozen, Is.False, "The run keeps moving.");
            Assert.That(guide.Step, Is.EqualTo(TutorialGuideStep.Running));
            Assert.That(guide.AcceptsTap, Is.True, "Taps are never blocked.");
            Assert.That(guide.Message, Does.Contain("ICE"));
            Assert.That(guide.Message, Does.Contain("NEW COLOR"));
            Assert.That(guide.TryGetHold(5, default), Is.False, "Only the first Ice gate.");
            guide.OnColorChanged(RunnerColor.Blue);
            guide.OnGateResolved(4);
            Assert.That(guide.Step, Is.EqualTo(TutorialGuideStep.Running));
            Assert.That(guide.Message, Does.Contain("ICE"), "Stays through the gate.");
            guide.Tick(TutorialScripts.NoticeSeconds - 0.1f);
            Assert.That(guide.Message, Does.Contain("ICE"));
            guide.Tick(0.2f);
            Assert.That(guide.Message, Is.Empty);
        }

        [Test]
        public void Guide_HoldsOnlyTheFirstGatesThenExplainsGradesAndGoal()
        {
            TutorialGuide guide = BeginBasics();
            int holds = guide.Script.HoldCount;
            Assert.That(holds, Is.EqualTo(3));
            for (int gate = 0; gate < holds; gate++)
            {
                Assert.That(guide.TryGetHold(gate, default), Is.True, $"gate {gate}");
                guide.Hold(gate, RunnerColor.Red, RunnerColor.Red);
                guide.Tick(TutorialGuide.SameColorHoldSeconds + 0.01f);
                guide.OnGateResolved(gate);
            }
            Assert.That(guide.TryGetHold(holds, default), Is.False);
            Assert.That(guide.Message, Does.Contain("PERFECT"));
            guide.Tick(4.01f);
            Assert.That(guide.Message, Does.Contain("GOAL"));
            guide.Tick(3.01f);
            Assert.That(guide.Message, Is.Empty);
            Assert.That(guide.Step, Is.EqualTo(TutorialGuideStep.Running));
        }

        [Test]
        public void Guide_HiddenNoticeNamesTheColor()
        {
            var guide = new TutorialGuide();
            guide.Begin(TutorialScripts.For(TutorialTopic.Hidden), RunnerColor.Green);
            Assert.That(guide.HoldNeedsNextGate, Is.False);
            guide.Hold(6, RunnerColor.Blue, RunnerColor.Red);
            Assert.That(guide.Frozen, Is.False);
            Assert.That(guide.AcceptsTap, Is.True);
            Assert.That(guide.Message, Does.Contain("REMEMBER THIS GATE: BLUE"));
            Assert.That(guide.TryGetHold(12, default), Is.False, "One notice only.");
        }

        [Test]
        public void Guide_EventBannersShowOnceWithTheirColor()
        {
            var guide = new TutorialGuide();
            guide.Begin(TutorialScripts.For(TutorialTopic.Echo), RunnerColor.Green);
            guide.OnEvent(TutorialEvent.Go);
            Assert.That(guide.Message, Is.Empty, "Echo has no GO line.");
            guide.OnEvent(TutorialEvent.EchoStored, RunnerColor.Blue);
            Assert.That(guide.Message, Does.Contain("ECHO STORED: BLUE"));
            guide.Tick(5f);
            Assert.That(guide.Message, Is.Empty);
            guide.OnEvent(TutorialEvent.EchoStored, RunnerColor.Red);
            Assert.That(guide.Message, Is.Empty, "Each banner shows once.");
        }

        [Test]
        public void Scripts_TeachEachFirstMechanicStage()
        {
            var expected = new Dictionary<int, TutorialTopic>
            {
                { 1, TutorialTopic.Basics }, { 13, TutorialTopic.ThirdColor },
                { 4, TutorialTopic.Shield }, { 5, TutorialTopic.Booster },
                { 6, TutorialTopic.StartItems }, { 7, TutorialTopic.Camouflage },
                { 9, TutorialTopic.Fog }, { 11, TutorialTopic.Ice },
                { 18, TutorialTopic.Echo }, { 21, TutorialTopic.Hidden },
                { 24, TutorialTopic.Flicker }
            };
            for (int number = 1; number <= StageCatalog.Count; number++)
            {
                StageDefinition stage = StageCatalog.GetByDisplayNumber(number);
                TutorialTopic topic = TutorialCards.GetStageTopic(StageCatalog.Current, stage);
                Assert.That(topic,
                    Is.EqualTo(expected.TryGetValue(number, out TutorialTopic t) ? t : TutorialTopic.None),
                    $"stage {number}");
                bool scripted = topic != TutorialTopic.None && topic != TutorialTopic.StartItems;
                Assert.That(TutorialScripts.For(topic) != null, Is.EqualTo(scripted), $"stage {number}");
            }
        }

        [Test]
        public void Scripts_ExplainMechanicsWithoutStopping()
        {
            StageDefinition hidden = StageCatalog.GetByDisplayNumber(21);
            float speed = 40f;
            float spacing = 60f;
            TutorialTopic[] noticed =
            {
                TutorialTopic.Camouflage, TutorialTopic.Ice, TutorialTopic.Echo,
                TutorialTopic.Hidden, TutorialTopic.Flicker
            };
            foreach (TutorialTopic topic in noticed)
            {
                TutorialScript script = TutorialScripts.For(topic);
                Assert.That(script.Notice, Is.True, $"{topic}");
                Assert.That(script.HoldCount, Is.EqualTo(1), $"{topic}");
                Assert.That(script.HoldLeadDistance(speed, spacing, hidden.HiddenSettings),
                    Is.GreaterThanOrEqualTo(spacing), $"{topic} starts by the previous gate");
            }
            Assert.That(TutorialScripts.For(TutorialTopic.Camouflage).NeedsTapMessage,
                Does.Not.Contain("{0}"), "The color is not revealed yet.");

            TutorialScript hide = TutorialScripts.For(TutorialTopic.Hidden);
            float beforeHide = hide.HoldLeadDistance(speed, 2f, hidden.HiddenSettings);
            Assert.That(beforeHide / speed,
                Is.GreaterThan(hidden.HiddenSettings.HideLeadTimeSeconds),
                "Shown while the color still shows.");

            foreach (TutorialTopic topic in new[] { TutorialTopic.Basics, TutorialTopic.ThirdColor })
            {
                TutorialScript script = TutorialScripts.For(topic);
                Assert.That(script.Notice, Is.False, $"{topic} still stops for the match");
                Assert.That(script.HoldLeadDistance(speed, spacing, null),
                    Is.EqualTo(speed * TutorialGuide.HoldLeadSeconds),
                    $"{topic} stops inside the PERFECT window");
            }
        }

        [Test]
        public void StageIntros_AppearOnTheFirstStageOfEachMechanic()
        {
            var expected = new Dictionary<int, string>
            {
                { 13, "GREEN" }, { 4, "SHIELD" }, { 5, "BOOSTER" },
                { 6, "START ITEMS" }, { 7, "CAMOUFLAGE" }, { 9, "FOG" },
                { 11, "ICE" }, { 18, "ECHO" }, { 21, "HIDDEN" }, { 24, "FLICKER" }
            };
            for (int number = 1; number <= StageCatalog.Count; number++)
            {
                StageDefinition stage = StageCatalog.GetByDisplayNumber(number);
                bool found = TutorialCards.TryGetStageIntro(
                    StageCatalog.Current, stage, out TutorialCard card);
                Assert.That(found, Is.EqualTo(expected.ContainsKey(number)), $"stage {number}");
                if (found)
                {
                    Assert.That(card.Title, Is.EqualTo(expected[number]), $"stage {number}");
                    Assert.That(card.Body, Is.Not.Empty);
                }
            }
        }

        [Test]
        public void HowToPlay_CoversControlsJudgmentTimingAndCrowns()
        {
            TutorialCard[] pages = TutorialCards.HowToPlay(StageCrownPolicy.NormalComboTarget);
            Assert.That(pages, Has.Length.EqualTo(4));
            Assert.That(pages[0].Body, Does.Contain("TAP"));
            Assert.That(pages[1].Body, Does.Contain("CHECKED THE MOMENT"));
            Assert.That(pages[2].Body, Does.Contain("0.25s").And.Contain("GREAT").And.Contain("GOOD"));
            Assert.That(pages[3].Title, Is.EqualTo("CROWNS"));
            Assert.That(TutorialCards.Crowns(20).Body, Does.Contain("20 COMBO"));
            Assert.That(TutorialCards.TutorialStageId,
                Is.EqualTo(StageCatalog.GetByDisplayNumber(1).StageId));
        }
    }
}
