using ColorGateRunner.Core;
using ColorGateRunner.Product;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class StageCrownTests
    {
        [Test]
        public void CleanRun_EarnsCleanAndFullComboWhenUnbroken()
        {
            StageCrown crowns = StageCrownPolicy.Evaluate(
                new StageClearRun(false, 0, 12, 12, 0),
                1);
            Assert.That(crowns, Is.EqualTo(
                StageCrown.Clean | StageCrown.Combo | StageCrown.FullCombo));
        }

        [Test]
        public void ComboCrown_RequiresTenAndIsIndependentOfClean()
        {
            Assert.That(StageCrownPolicy.Evaluate(
                    new StageClearRun(true, 0, 10, 14, 3), 1),
                Is.EqualTo(StageCrown.Combo));
            Assert.That(StageCrownPolicy.Evaluate(
                    new StageClearRun(false, 0, 9, 9, 0), 1),
                Is.EqualTo(StageCrown.Clean | StageCrown.FullCombo));
        }

        [Test]
        public void ShieldContinueBreakOrNoComboHit_DenyFullCombo()
        {
            Assert.That(StageCrownPolicy.Evaluate(
                    new StageClearRun(false, 1, 4, 8, 1), 1),
                Is.EqualTo(StageCrown.None));
            Assert.That(StageCrownPolicy.Evaluate(
                    new StageClearRun(true, 0, 4, 8, 1), 1),
                Is.EqualTo(StageCrown.None));
            Assert.That(StageCrownPolicy.Evaluate(
                    new StageClearRun(false, 0, 4, 8, 1), 1),
                Is.EqualTo(StageCrown.Clean));
            Assert.That(StageCrownPolicy.Evaluate(
                    new StageClearRun(false, 0, 0, 0, 0), 1),
                Is.EqualTo(StageCrown.Clean));
        }

        [Test]
        public void Comeback_RequiresSevenFailedAttemptsBeforeTheClear()
        {
            var run = new StageClearRun(true, 0, 0, 0, 2);
            Assert.That(StageCrownPolicy.Evaluate(run, 7),
                Is.EqualTo(StageCrown.None));
            Assert.That(StageCrownPolicy.Evaluate(run, 8),
                Is.EqualTo(StageCrown.Comeback));
        }

        [Test]
        public void CrownMask_MatchesProductValidation()
        {
            Assert.That((int)StageCrownPolicy.AllCrowns,
                Is.EqualTo(LocalStageProgressData.KnownCrownFlags));
            Assert.That(StageCrownPolicy.IsValid(15), Is.True);
            Assert.That(StageCrownPolicy.IsValid(16), Is.False);
            Assert.That(StageCrownPolicy.IsValid(-1), Is.False);
        }

        [Test]
        public void RecordClear_AccumulatesCrownsBestComboAndResetsAttempts()
        {
            var current = new StageRecord(
                true, 20f, 20f, 1, 0, 14, StageCrown.Combo, 9);
            StageRecord next = StageProgress.RecordClear(
                current,
                18f,
                default,
                new StageClearRun(false, 0, 6, 6, 0),
                out StageCrown earned);

            Assert.That(earned, Is.EqualTo(
                StageCrown.Clean | StageCrown.FullCombo | StageCrown.Comeback));
            Assert.That(next.Crowns, Is.EqualTo(StageCrownPolicy.AllCrowns));
            Assert.That(next.BestMaxCombo, Is.EqualTo(14));
            Assert.That(next.AttemptsSinceClear, Is.Zero);
            Assert.That(next.ClearCount, Is.EqualTo(2));
            Assert.That(next.BestTime, Is.EqualTo(18f));
        }

        [Test]
        public void LegacyRecordClear_KeepsCrownsWithoutAwardingNewOnes()
        {
            var current = new StageRecord(
                true, 20f, 20f, 1, 0, 14, StageCrown.Combo, 3);
            StageRecord next = StageProgress.RecordClear(current, 18f, default);
            Assert.That(next.Crowns, Is.EqualTo(StageCrown.Combo));
            Assert.That(next.BestMaxCombo, Is.EqualTo(14));
            Assert.That(next.AttemptsSinceClear, Is.Zero);
        }

        [Test]
        public void LegacySerializedRecord_ParsesWithZeroAchievementFields()
        {
            var current = new StageRecord(
                true, 20f, 20f, 1, 0, 14, StageCrown.Combo, 3);
            StageRecord parsed = StageProgress.Parse(
                StageProgress.Serialize(current));
            Assert.That(parsed.ClearCount, Is.EqualTo(1));
            Assert.That(parsed.Crowns, Is.EqualTo(StageCrown.None));
            Assert.That(parsed.BestMaxCombo, Is.Zero);
        }

        [Test]
        public void Combo_CountsBreaksUntilReset()
        {
            var combo = new PerfectSwitchCombo();
            combo.Record(GateOutcome.Matched, new GateSwitchTiming(true, 0f));
            combo.Record(GateOutcome.Matched, new GateSwitchTiming(true, 1f));
            combo.Record(GateOutcome.Shielded, default);
            combo.Record(GateOutcome.Matched, default);
            combo.Record(GateOutcome.Echoed, default);
            Assert.That(combo.BreakCount, Is.EqualTo(3));
            combo.Reset();
            Assert.That(combo.BreakCount, Is.Zero);
        }

        [Test]
        public void Session_CountsShieldAbsorbsAndRetryClearsThem()
        {
            StageDefinition stage = StageCatalog.GetByDisplayNumber(8);
            var session = new StageSession(stage);
            session.SelectItems(new StartItemSelection(true, false));
            Assert.That(session.BeginCountdown(), Is.True);
            Assert.That(session.CompleteCountdown(), Is.True);
            Assert.That(session.ShieldActive, Is.True);
            RunnerColor wrong = session.CurrentColor == RunnerColor.Red
                ? RunnerColor.Blue
                : RunnerColor.Red;
            Assert.That(session.ResolveGate(wrong),
                Is.EqualTo(GateOutcome.Shielded));
            Assert.That(session.ShieldAbsorbCount, Is.EqualTo(1));
            session.RetryToSelection();
            Assert.That(session.ShieldAbsorbCount, Is.Zero);
        }
    }
}
