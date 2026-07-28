using ColorGateRunner.Core;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class Step9A1ContinuityTests
    {
        [Test]
        public void StageCatalog_AllMovementSpeedsAreDoubled()
        {
            float[] starts = { 14f, 16f, 17f, 16f, 18f };
            float[] maximums = { 20f, 22f, 23f, 24f, 27f };
            float[] boosters = { 44f, 46f, 48f, 50f, 52f };
            float[] boosterDistances = { 160f, 180f, 200f, 210f, 230f };

            for (int index = 0; index < StageCatalog.Count; index++)
            {
                StageDefinition stage = StageCatalog.GetByIndex(index);
                Assert.That(stage.StartingSpeed, Is.EqualTo(starts[index]));
                Assert.That(stage.MaximumSpeed, Is.EqualTo(maximums[index]));
                Assert.That(stage.BoosterSpeed, Is.EqualTo(boosters[index]));
                Assert.That(stage.BoosterDistance,
                    Is.EqualTo(boosterDistances[index]));
            }
        }

        [Test]
        public void BoosterExit_PreservesSequenceCursor()
        {
            StageSession session = CreatePlaying(5, booster: true);
            GatePlan first = session.GetGatePlan(0);
            session.GetGatePlan(1);
            session.GetGatePlan(2);
            int cursor = session.SequenceCursor;

            EndBooster(session);
            session.CreateSafeTransitionOverride(first, 0);

            Assert.That(session.SequenceCursor, Is.EqualTo(cursor));
        }

        [Test]
        public void BoosterExit_PreservesGateIndices()
        {
            StageSession session = CreatePlaying(5, booster: true);
            GatePlan authored = session.GetGatePlan(4);
            EndBooster(session);

            GatePlan overridden =
                session.CreateSafeTransitionOverride(authored, 0);

            Assert.That(overridden.IndexInPattern,
                Is.EqualTo(authored.IndexInPattern));
            Assert.That(overridden.Pattern, Is.EqualTo(authored.Pattern));
        }

        [Test]
        public void BoosterExit_OverridesColorWithoutChangingPlannedPosition()
        {
            StageSession session = CreatePlaying(5, booster: true);
            GatePlan authored = session.GetGatePlan(0);
            EndBooster(session);

            GatePlan overridden =
                session.CreateSafeTransitionOverride(authored, 0);

            Assert.That(overridden.Spacing, Is.EqualTo(authored.Spacing));
            Assert.That(overridden.TimeToGate, Is.EqualTo(authored.TimeToGate));
            Assert.That(overridden.BeatMultiplier,
                Is.EqualTo(authored.BeatMultiplier));
            Assert.That(overridden.PlannedColor, Is.EqualTo(authored.Color));
        }

        [Test]
        public void BoosterExit_FirstGateMatchesCurrentColor()
        {
            StageSession session = CreatePlaying(5, booster: true);
            GatePlan authored = session.GetGatePlan(0);
            EndBooster(session);

            GatePlan overridden =
                session.CreateSafeTransitionOverride(authored, 0);

            Assert.That(overridden.Color, Is.EqualTo(session.CurrentColor));
        }

        [Test]
        public void BoosterExit_SecondGateRequiresAtMostOneTap()
        {
            StageSession session = CreatePlaying(5, booster: true);
            GatePlan authored = session.GetGatePlan(1);
            EndBooster(session);

            GatePlan overridden =
                session.CreateSafeTransitionOverride(authored, 1);
            Assert.That(session.TryToggleColor(), Is.True);

            Assert.That(session.CurrentColor, Is.EqualTo(overridden.Color));
        }

        [Test]
        public void BoosterExit_ThirdGateReturnsToAuthoredSequence()
        {
            StageSession session = CreatePlaying(5, booster: true);
            GatePlan authored = session.GetGatePlan(2);
            EndBooster(session);

            GatePlan third = session.AdjustForSafeTransition(authored, 2);

            Assert.That(third.Color, Is.EqualTo(authored.Color));
            Assert.That(third.Pattern, Is.EqualTo(authored.Pattern));
            Assert.That(third.HasTemporaryColorOverride, Is.False);
        }

        [Test]
        public void ContinueSnapshot_PreservesExactStageState()
        {
            StageSession session = CreateFailedWithLookahead(
                out float elapsed,
                out float progress,
                out float speed,
                out RunnerColor color,
                out int cursor);

            Assert.That(session.ContinueAfterFailure(), Is.True);

            Assert.That(session.ElapsedPlayingSeconds, Is.EqualTo(elapsed));
            Assert.That(session.Progress, Is.EqualTo(progress));
            Assert.That(session.CurrentSpeed, Is.EqualTo(speed));
            Assert.That(session.CurrentColor, Is.EqualTo(color));
            Assert.That(session.SequenceCursor, Is.EqualTo(cursor));
        }

        [Test]
        public void Continue_PreservesElapsedTime()
        {
            StageSession session = CreateFailedWithLookahead(
                out float elapsed,
                out _,
                out _,
                out _,
                out _);

            session.ContinueAfterFailure();

            Assert.That(session.ElapsedPlayingSeconds, Is.EqualTo(elapsed));
        }

        [Test]
        public void Continue_PreservesNormalizedProgress()
        {
            StageSession session = CreateFailedWithLookahead(
                out _,
                out float progress,
                out _,
                out _,
                out _);

            session.ContinueAfterFailure();

            Assert.That(session.Progress, Is.EqualTo(progress));
        }

        [Test]
        public void Continue_PreservesCurrentSpeed()
        {
            StageSession session = CreateFailedWithLookahead(
                out _,
                out _,
                out float speed,
                out _,
                out _);

            session.ContinueAfterFailure();

            Assert.That(session.CurrentSpeed, Is.EqualTo(speed));
        }

        [Test]
        public void Continue_PreservesCurrentColor()
        {
            StageSession session = CreateFailedWithLookahead(
                out _,
                out _,
                out _,
                out RunnerColor color,
                out _);

            session.ContinueAfterFailure();

            Assert.That(session.CurrentColor, Is.EqualTo(color));
        }

        [Test]
        public void Continue_PreservesPendingSequenceCursor()
        {
            StageSession session = CreateFailedWithLookahead(
                out _,
                out _,
                out _,
                out _,
                out int cursor);

            session.ContinueAfterFailure();
            session.CompleteCountdown();

            Assert.That(session.SequenceCursor, Is.EqualTo(cursor));
        }

        [Test]
        public void Continue_FailedGateIsResolvedExactlyOnce()
        {
            StageSession session = CreateFailedWithLookahead(
                out _,
                out float progress,
                out _,
                out _,
                out _);

            session.ContinueAfterFailure();
            Assert.That(session.CompleteCountdown(), Is.True);
            Assert.That(session.CompleteCountdown(), Is.False);

            Assert.That(session.ContinuedFailedGateResolutionCount,
                Is.EqualTo(1));
            Assert.That(session.Progress,
                Is.EqualTo(progress + (1f / session.Stage.TargetGateCount)));
        }

        [Test]
        public void Continue_DoesNotRestoreShield()
        {
            StageSession session = CreatePlaying(1, shield: true);
            GatePlan first = session.GetNextGatePlan();
            SetMismatchingColor(session, first.Color);
            Assert.That(session.ResolveGate(first.Color),
                Is.EqualTo(GateOutcome.Shielded));
            session.Advance(GameRules.ShieldRecoveryDuration, 0f);
            GatePlan second = session.GetNextGatePlan();
            SetMismatchingColor(session, second.Color);
            Assert.That(session.ResolveGate(second.Color),
                Is.EqualTo(GateOutcome.Mismatched));

            session.ContinueAfterFailure();
            session.CompleteCountdown();

            Assert.That(session.ShieldActive, Is.False);
        }

        [Test]
        public void Continue_DoesNotRestartBooster()
        {
            StageSession session = CreatePlaying(1, booster: true);
            EndBooster(session);
            session.Advance(StageSession.BoosterExitDuration, 0f);
            GatePlan plan = session.GetNextGatePlan();
            SetMismatchingColor(session, plan.Color);
            Assert.That(session.ResolveGate(plan.Color),
                Is.EqualTo(GateOutcome.Mismatched));

            session.ContinueAfterFailure();
            session.CompleteCountdown();

            Assert.That(session.BoosterActive, Is.False);
            Assert.That(session.BoosterDistanceRemaining, Is.Zero);
        }

        [Test]
        public void Continue_FinalSectionStateSurvives()
        {
            StageSession session = CreatePlaying(1);
            while (!session.IsFinalSection)
            {
                GatePlan plan = session.GetNextGatePlan();
                SetMatchingColor(session, plan.Color);
                session.ResolveGate(plan.Color);
            }
            GatePlan failed = session.GetNextGatePlan();
            SetMismatchingColor(session, failed.Color);
            session.ResolveGate(failed.Color);
            Assert.That(session.IsFinalSection, Is.True);

            session.ContinueAfterFailure();
            session.CompleteCountdown();

            Assert.That(session.IsFinalSection, Is.True);
        }

        [Test]
        public void Simulation_ContinuityMetricsReportNoReset()
        {
            StageSimulationResult result = StageSimulationRunner.Run(
                StageCatalog.GetByDisplayNumber(5),
                new StartItemSelection(false, true),
                SimulatedPlayerProfile.Get(SimulatedPlayerKind.Stress),
                new GameplaySimulationSettings(200, 98765u, true));

            Assert.That(result.MaximumBoosterActiveGateDisplacement,
                Is.Zero.Within(0.0001f));
            Assert.That(result.MaximumContinueUnaffectedGateDisplacement,
                Is.Zero.Within(0.0001f));
            Assert.That(result.BoosterGateIndexGapCount, Is.Zero);
            Assert.That(result.BoosterDuplicateGateIndexCount, Is.Zero);
            Assert.That(result.ContinueSequenceCursorResetCount, Is.Zero);
            Assert.That(result.ContinueGateIndexGapCount, Is.Zero);
            Assert.That(result.ContinueDuplicateGateIndexCount, Is.Zero);
            Assert.That(result.FullPoolResetCount, Is.Zero);
        }

        private static StageSession CreatePlaying(
            int stageNumber,
            bool shield = false,
            bool booster = false)
        {
            StageSession session = new StageSession(
                StageCatalog.GetByDisplayNumber(stageNumber));
            session.SelectItems(new StartItemSelection(shield, booster));
            session.BeginCountdown();
            session.CompleteCountdown();
            return session;
        }

        private static StageSession CreateFailedWithLookahead(
            out float elapsed,
            out float progress,
            out float speed,
            out RunnerColor color,
            out int cursor)
        {
            StageSession session = CreatePlaying(3);
            session.Advance(2.5f, 30f);
            GatePlan failed = session.GetGatePlan(0);
            session.GetGatePlan(1);
            session.GetGatePlan(2);
            SetMismatchingColor(session, failed.Color);
            session.ResolveGate(failed.Color);
            elapsed = session.ElapsedPlayingSeconds;
            progress = session.Progress;
            speed = session.SpeedBeforeFailure;
            color = session.CurrentColor;
            cursor = session.SequenceCursor;
            return session;
        }

        private static void EndBooster(StageSession session)
        {
            session.Advance(1f, session.Stage.BoosterDistance);
            Assert.That(session.BoosterActive, Is.False);
        }

        private static void SetMatchingColor(
            StageSession session,
            RunnerColor color)
        {
            while (session.CurrentColor != color)
            {
                session.TryToggleColor();
            }
        }

        private static void SetMismatchingColor(
            StageSession session,
            RunnerColor color)
        {
            if (session.CurrentColor == color)
            {
                session.TryToggleColor();
            }
        }
    }
}
