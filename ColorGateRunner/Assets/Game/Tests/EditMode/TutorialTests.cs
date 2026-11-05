using System.Collections.Generic;
using ColorGateRunner.Core;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class TutorialTests
    {
        [Test]
        public void Guide_MismatchHoldWaitsForTheMatchingTap()
        {
            var guide = new TutorialGuide();
            guide.Begin();
            Assert.That(guide.Message, Does.Contain("TAP ANYWHERE"));
            Assert.That(guide.TryGetHoldDistance(0, 40f, out float lead), Is.True);
            Assert.That(lead, Is.EqualTo(40f * TutorialGuide.HoldLeadSeconds));

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
            Assert.That(guide.TryGetHoldDistance(0, 40f, out _), Is.False);
        }

        [Test]
        public void Guide_SameColorHoldBlocksTapsThenResumes()
        {
            var guide = new TutorialGuide();
            guide.Begin();
            guide.Hold(0, RunnerColor.Red, RunnerColor.Red);
            Assert.That(guide.Step, Is.EqualTo(TutorialGuideStep.HoldSameColor));
            Assert.That(guide.AcceptsTap, Is.False);
            guide.Tick(TutorialGuide.SameColorHoldSeconds - 0.1f);
            Assert.That(guide.Frozen, Is.True);
            guide.Tick(0.2f);
            Assert.That(guide.Frozen, Is.False);
            Assert.That(guide.AcceptsTap, Is.True);
        }

        [Test]
        public void Guide_HoldsOnlyTheFirstGatesThenExplainsGradesAndGoal()
        {
            var guide = new TutorialGuide();
            guide.Begin();
            for (int gate = 0; gate < TutorialGuide.GuidedGateCount; gate++)
            {
                Assert.That(guide.TryGetHoldDistance(gate, 40f, out _), Is.True, $"gate {gate}");
                guide.Hold(gate, RunnerColor.Red, RunnerColor.Red);
                guide.Tick(TutorialGuide.SameColorHoldSeconds + 0.01f);
                guide.OnGateResolved(gate);
            }
            Assert.That(guide.TryGetHoldDistance(TutorialGuide.GuidedGateCount, 40f, out _), Is.False);
            Assert.That(guide.Step, Is.EqualTo(TutorialGuideStep.Grades));
            Assert.That(guide.Message, Does.Contain("PERFECT"));
            guide.Tick(TutorialGuide.GradesSeconds + 0.01f);
            Assert.That(guide.Step, Is.EqualTo(TutorialGuideStep.Goal));
            Assert.That(guide.Message, Does.Contain("GOAL"));
            guide.Tick(TutorialGuide.GoalSeconds + 0.01f);
            Assert.That(guide.Step, Is.EqualTo(TutorialGuideStep.Done));
            Assert.That(guide.Message, Is.Empty);
        }

        [Test]
        public void StageIntros_AppearOnTheFirstStageOfEachMechanic()
        {
            var expected = new Dictionary<int, string>
            {
                { 4, "GREEN" }, { 6, "SHIELD" }, { 7, "BOOSTER" },
                { 8, "START ITEMS" }, { 9, "CAMOUFLAGE" }, { 12, "FOG" },
                { 15, "ICE" }, { 18, "ECHO" }, { 21, "HIDDEN" }, { 24, "FLICKER" }
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
