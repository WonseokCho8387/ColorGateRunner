using ColorGateRunner.Core;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class StageScoreTests
    {
        [Test]
        public void Score_WeighsCrownsAndComboByDifficultyAndTakesExtraTaps()
        {
            StageCrown all = StageCrown.Clean | StageCrown.Combo | StageCrown.FullCombo;
            StageScore normal = StageScorePolicy.Evaluate(
                StageDifficulty.Normal, all, 24, 15, 12);
            Assert.That(normal.CrownPoints, Is.EqualTo(600));
            Assert.That(normal.ComboPoints, Is.EqualTo(240));
            Assert.That(normal.ExtraTaps, Is.EqualTo(3));
            Assert.That(normal.Total, Is.EqualTo(780));

            StageScore hard = StageScorePolicy.Evaluate(
                StageDifficulty.Hard, all, 24, 12, 12);
            Assert.That(hard.Total, Is.EqualTo(1680));
            StageScore veryHard = StageScorePolicy.Evaluate(
                StageDifficulty.VeryHard, StageCrown.Clean, 10, 12, 12);
            Assert.That(veryHard.Total, Is.EqualTo(300 + 300));
        }

        [Test]
        public void Score_IgnoresComebackAndNeverGoesBelowZero()
        {
            StageScore comeback = StageScorePolicy.Evaluate(
                StageDifficulty.Normal, StageCrown.Comeback, 0, 0, 0);
            Assert.That(comeback.Total, Is.Zero);
            StageScore spam = StageScorePolicy.Evaluate(
                StageDifficulty.Normal, StageCrown.None, 2, 80, 10);
            Assert.That(spam.ExtraTaps, Is.EqualTo(70));
            Assert.That(spam.Total, Is.Zero);
            StageScore fewer = StageScorePolicy.Evaluate(
                StageDifficulty.Normal, StageCrown.Clean, 0, 3, 5);
            Assert.That(fewer.ExtraTaps, Is.Zero, "An Echo save can need fewer taps.");
        }

        [Test]
        public void Session_CountsTapsAndTheFewestTheJudgedGatesNeeded()
        {
            var session = new StageSession(AuthoredStage.Get(1));
            session.BeginCountdown();
            session.CompleteCountdown();
            Assert.That(session.CurrentColor, Is.EqualTo(RunnerColor.Red));

            session.TryToggleColor();
            Assert.That(session.ResolveGate(RunnerColor.Blue), Is.EqualTo(GateOutcome.Matched));
            Assert.That(session.ResolveGate(RunnerColor.Blue), Is.EqualTo(GateOutcome.Matched));
            session.TryToggleColor();
            session.TryToggleColor();
            session.TryToggleColor();
            Assert.That(session.ResolveGate(RunnerColor.Red), Is.EqualTo(GateOutcome.Matched));

            Assert.That(session.TapCount, Is.EqualTo(4));
            Assert.That(session.MinimumTapCount, Is.EqualTo(2));
        }

        [Test]
        public void Session_ThreeColorsCountTheCycleDistance()
        {
            var session = new StageSession(AuthoredStage.Get(5));
            session.BeginCountdown();
            session.CompleteCountdown();
            RunnerColor start = session.CurrentColor;
            RunnerColor next = session.Stage.GetNextActiveColor(0, start);
            RunnerColor back = session.Stage.GetNextActiveColor(0, next);
            Assert.That(back, Is.Not.EqualTo(start));

            session.TryToggleColor();
            session.TryToggleColor();
            Assert.That(session.ResolveGate(back), Is.EqualTo(GateOutcome.Matched));
            Assert.That(session.MinimumTapCount, Is.EqualTo(2),
                "Going to the color behind takes two taps.");
        }

        [Test]
        public void Session_BoosterGatesNeedNoTaps()
        {
            var session = new StageSession(AuthoredStage.Get(8));
            session.SelectItems(new StartItemSelection(false, true));
            session.BeginCountdown();
            session.CompleteCountdown();
            Assert.That(session.BoosterActive, Is.True);
            RunnerColor other = session.Stage.GetNextActiveColor(0, session.CurrentColor);
            Assert.That(session.ResolveGate(other), Is.EqualTo(GateOutcome.Boosted));
            Assert.That(session.MinimumTapCount, Is.Zero);
        }

        [Test]
        public void Record_KeepsTheBestScoreAcrossClears()
        {
            var record = new StageRecord(true, 10f, 10f, 1, 0, 12, StageCrown.Clean, 0, 500);
            Assert.That(record.WithBestScore(300).BestScore, Is.EqualTo(500));
            Assert.That(record.WithBestScore(900).BestScore, Is.EqualTo(900));
            StageRecord cleared = StageProgress.RecordClear(
                record, 9f, default,
                new StageClearRun(false, 0, 14, 14, 0, 10), out _);
            Assert.That(cleared.BestScore, Is.EqualTo(500), "A clear keeps the stored best.");
        }
    }
}
