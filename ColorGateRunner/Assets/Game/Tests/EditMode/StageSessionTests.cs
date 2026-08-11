using System;
using System.Collections.Generic;
using ColorGateRunner.Core;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class StageSessionTests
    {
        [Test]
        public void StageCatalog_ContainsTwentyValidStages()
        {
            Assert.That(StageCatalog.Count, Is.EqualTo(20));
            for (int index = 0; index < StageCatalog.Count; index++)
            {
                Assert.That(StageCatalog.GetByIndex(index).IsValid(), Is.True);
            }
        }

        [Test]
        public void StageIds_AreUnique()
        {
            HashSet<string> ids = new HashSet<string>();
            for (int index = 0; index < StageCatalog.Count; index++)
            {
                Assert.That(ids.Add(StageCatalog.GetByIndex(index).StageId), Is.True);
            }
        }

        [Test]
        public void StageNumbers_AreSequential()
        {
            for (int index = 0; index < StageCatalog.Count; index++)
            {
                Assert.That(
                    StageCatalog.GetByIndex(index).DisplayNumber,
                    Is.EqualTo(index + 1));
            }
        }

        [Test]
        public void StagesOneToThree_DoNotAllowGreen()
        {
            for (int index = 0; index < 3; index++)
            {
                Assert.That(
                    StageCatalog.GetByIndex(index).AllowsColor(RunnerColor.Green),
                    Is.False);
            }
        }

        [Test]
        public void StagesFourAndFive_AllowGreen()
        {
            Assert.That(
                StageCatalog.GetByDisplayNumber(4).AllowsColor(RunnerColor.Green),
                Is.True);
            Assert.That(
                StageCatalog.GetByDisplayNumber(5).AllowsColor(RunnerColor.Green),
                Is.True);
        }

        [Test]
        public void EveryStage_HasPositiveTargetAndFinalSection()
        {
            for (int index = 0; index < StageCatalog.Count; index++)
            {
                StageDefinition stage = StageCatalog.GetByIndex(index);
                Assert.That(stage.TargetGateCount, Is.Positive);
                Assert.That(stage.FinalPressureGateCount, Is.Positive);
                Assert.That(
                    stage.FinalPressureGateCount,
                    Is.LessThan(stage.TargetGateCount));
                Assert.That(stage.CadenceEnd, Is.GreaterThanOrEqualTo(0.75f));
            }
        }

        [Test]
        public void Booster_IsShorterThanEstimatedStageDistance()
        {
            for (int index = 0; index < StageCatalog.Count; index++)
            {
                StageDefinition stage = StageCatalog.GetByIndex(index);
                float estimatedDistance =
                    stage.TargetGateCount *
                    stage.StartingSpeed *
                    stage.CadenceEnd;
                Assert.That(stage.BoosterDistance, Is.LessThan(estimatedDistance));
            }
        }

        [Test]
        public void NoSelection_InitializesWithoutItems()
        {
            StageSession session = CreatePlaying(default);

            Assert.That(session.ShieldActive, Is.False);
            Assert.That(session.BoosterActive, Is.False);
        }

        [Test]
        public void ShieldSelection_ActivatesAfterCountdown()
        {
            StageSession session = CreatePlaying(new StartItemSelection(true, false));

            Assert.That(session.ShieldActive, Is.True);
            Assert.That(session.BoosterActive, Is.False);
        }

        [Test]
        public void BoosterSelection_ActivatesAfterCountdown()
        {
            StageSession session = CreatePlaying(new StartItemSelection(false, true));

            Assert.That(session.BoosterActive, Is.True);
            Assert.That(session.CurrentSpeed, Is.EqualTo(session.Stage.BoosterSpeed));
        }

        [Test]
        public void BoosterBypass_DoesNotConsumeShield()
        {
            StageSession session = CreatePlaying(new StartItemSelection(true, true));
            RunnerColor mismatch = session.CurrentColor == RunnerColor.Red
                ? RunnerColor.Blue
                : RunnerColor.Red;

            Assert.That(session.ResolveGate(mismatch), Is.EqualTo(GateOutcome.Boosted));
            Assert.That(session.ShieldActive, Is.True);
        }

        [Test]
        public void BoosterEnd_UsesSpeedAtCurrentStageProgress()
        {
            StageSession session = CreatePlaying(new StartItemSelection(false, true));
            while (session.BoosterDistanceRemaining > 0f)
            {
                GatePlan plan = session.GetNextGatePlan();
                session.ResolveGate(plan.Color);
                session.Advance(0.1f, plan.Spacing);
            }

            float expected =
                session.Stage.GetBaseSpeed(session.Progress);
            Assert.That(session.CurrentSpeed, Is.GreaterThan(expected));
            session.Advance(StageSession.BoosterExitDuration, 0f);
            Assert.That(session.CurrentSpeed, Is.EqualTo(expected).Within(0.0001f));
        }

        [Test]
        public void FinalGate_EntersStageFinishing()
        {
            StageSession session = CreatePlaying(default);
            ResolveAllGates(session);

            Assert.That(session.FlowState, Is.EqualTo(StageFlowState.StageFinishing));
            Assert.That(session.RemainingGates, Is.Zero);
        }

        [Test]
        public void Goal_ClearsOnlyOnce()
        {
            StageSession session = CreatePlaying(default);
            ResolveAllGates(session);

            Assert.That(session.ReachGoal(), Is.True);
            Assert.That(session.ReachGoal(), Is.False);
            Assert.That(session.IsComplete, Is.True);
        }

        [Test]
        public void StageFinishing_IgnoresFailureJudgment()
        {
            StageSession session = CreatePlaying(default);
            ResolveAllGates(session);

            Assert.That(
                session.ResolveGate(RunnerColor.Green),
                Is.EqualTo(GateOutcome.Ignored));
            Assert.That(session.FlowState, Is.EqualTo(StageFlowState.StageFinishing));
        }

        [Test]
        public void Clear_UnlocksOnlyNextStage()
        {
            Assert.That(
                StageProgress.HighestUnlockedAfterClear(1, 1),
                Is.EqualTo(2));
            Assert.That(
                StageProgress.HighestUnlockedAfterClear(2, 1),
                Is.EqualTo(2));
            Assert.That(
                StageProgress.HighestUnlockedAfterClear(5, 5),
                Is.EqualTo(6));
            Assert.That(
                StageProgress.HighestUnlockedAfterClear(11, 11),
                Is.EqualTo(12));
            Assert.That(
                StageProgress.HighestUnlockedAfterClear(20, 20),
                Is.EqualTo(20));
        }

        [Test]
        public void BetterLowerTime_ReplacesBestTime()
        {
            StageRecord record = StageProgress.RecordClear(
                default,
                35f,
                new StartItemSelection(true, false));
            record = StageProgress.RecordClear(
                record,
                31f,
                new StartItemSelection(false, true));

            Assert.That(record.BestTime, Is.EqualTo(31f));
            Assert.That(record.ClearCount, Is.EqualTo(2));
        }

        [Test]
        public void BoosterClear_DoesNotOverwriteNoItemBest()
        {
            StageRecord record = StageProgress.RecordClear(default, 40f, default);
            record = StageProgress.RecordClear(
                record,
                25f,
                new StartItemSelection(false, true));

            Assert.That(record.BestTime, Is.EqualTo(25f));
            Assert.That(record.BestNoItemTime, Is.EqualTo(40f));
        }

        [Test]
        public void CorruptPersistence_FallsBackSafely()
        {
            StageRecord record = StageProgress.Parse("not|a|stage|record");

            Assert.That(record.Cleared, Is.False);
            Assert.That(record.BestTime, Is.Zero);
            Assert.That(record.BestNoItemTime, Is.Zero);
            Assert.That(record.ClearCount, Is.Zero);
        }

        [Test]
        public void Persistence_RoundTrips()
        {
            StageRecord expected = new StageRecord(true, 31.25f, 35.5f, 4);

            StageRecord actual = StageProgress.Parse(StageProgress.Serialize(expected));

            Assert.That(actual.Cleared, Is.True);
            Assert.That(actual.BestTime, Is.EqualTo(31.25f));
            Assert.That(actual.BestNoItemTime, Is.EqualTo(35.5f));
            Assert.That(actual.ClearCount, Is.EqualTo(4));
        }

        [Test]
        public void StageSequence_IsDeterministic()
        {
            StageDefinition stage = StageCatalog.GetByDisplayNumber(5);
            DeterministicStageGateSequence first =
                new DeterministicStageGateSequence(stage);
            DeterministicStageGateSequence second =
                new DeterministicStageGateSequence(stage);

            for (int index = 0; index < stage.TargetGateCount; index++)
            {
                GatePlan a = first.GetPlan(index);
                GatePlan b = second.GetPlan(index);
                Assert.That(a.Color, Is.EqualTo(b.Color));
                Assert.That(a.Spacing, Is.EqualTo(b.Spacing));
                Assert.That(a.Pattern, Is.EqualTo(b.Pattern));
            }
        }

        [Test]
        public void StageFour_IntroducesGreenAtConfiguredGate()
        {
            StageDefinition stage = StageCatalog.GetByDisplayNumber(4);
            DeterministicStageGateSequence sequence =
                new DeterministicStageGateSequence(stage);
            for (int index = 0; index < stage.IntroGateCount; index++)
            {
                Assert.That(
                    sequence.GetPlan(index).Color,
                    Is.Not.EqualTo(RunnerColor.Green));
            }

            Assert.That(
                sequence.GetPlan(stage.IntroGateCount).Color,
                Is.EqualTo(RunnerColor.Green));
        }

        [Test]
        public void Retry_RetainsSelectedItemsAndReplaysLayout()
        {
            StageSession session = CreatePlaying(new StartItemSelection(true, true));
            GatePlan first = session.GetNextGatePlan();
            session.ResolveGate(first.Color);

            session.RetryToSelection();
            GatePlan replay = session.GetNextGatePlan();

            Assert.That(session.Items.Shield, Is.True);
            Assert.That(session.Items.Booster, Is.True);
            Assert.That(replay.Color, Is.EqualTo(first.Color));
            Assert.That(replay.Spacing, Is.EqualTo(first.Spacing));
        }

        [Test]
        public void NegativeAdvance_IsRejected()
        {
            StageSession session = CreatePlaying(default);
            Assert.Throws<ArgumentOutOfRangeException>(
                () => session.Advance(-0.1f, 0f));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => session.Advance(0.1f, -1f));
        }

        [Test]
        public void Countdown_DoesNotAdvanceElapsedTime()
        {
            StageSession session =
                new StageSession(StageCatalog.GetByDisplayNumber(1));
            session.BeginCountdown();
            session.Advance(1f, 5f);

            Assert.That(session.ElapsedPlayingSeconds, Is.Zero);
        }

        private static StageSession CreatePlaying(StartItemSelection items)
        {
            int stageNumber =
                items.Shield || items.Booster ? 8 : 1;
            StageSession session =
                new StageSession(
                    StageCatalog.GetByDisplayNumber(stageNumber));
            session.SelectItems(items);
            session.BeginCountdown();
            session.CompleteCountdown();
            return session;
        }

        private static void ResolveAllGates(StageSession session)
        {
            while (session.RemainingGates > 0)
            {
                GatePlan plan = session.GetNextGatePlan();
                while (session.CurrentColor != plan.Color)
                {
                    session.TryToggleColor();
                }

                Assert.That(
                    session.ResolveGate(plan.Color),
                    Is.EqualTo(GateOutcome.Matched));
            }
        }
    }
}
