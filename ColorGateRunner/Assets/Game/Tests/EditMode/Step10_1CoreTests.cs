using ColorGateRunner.Core;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class Step10_1CoreTests
    {
        [Test]
        public void Stage4_ActiveColorsAreCompleteAtRunStart()
        {
            StageDefinition stage = StageCatalog.GetByDisplayNumber(4);
            StageSession session = CreatePlaying(stage);

            Assert.That(stage.ActiveColorsFromStart, Is.True);
            Assert.That(MobileUiPolicy.GetActiveColorCount(stage, 0),
                Is.EqualTo(3));
            session.TryToggleColor();
            session.TryToggleColor();
            Assert.That(session.CurrentColor, Is.EqualTo(RunnerColor.Green));
        }

        [Test]
        public void Stage4_GreenGatesBeginAtConfiguredLaterIndex()
        {
            StageDefinition stage = StageCatalog.GetByDisplayNumber(4);
            DeterministicStageGateSequence sequence =
                new DeterministicStageGateSequence(stage);
            int firstGreen = stage.GetFirstGateIndex(RunnerColor.Green);

            for (int index = 0; index < firstGreen; index++)
            {
                Assert.That(sequence.GetPlan(index).Color,
                    Is.Not.EqualTo(RunnerColor.Green));
            }
            Assert.That(sequence.GetPlan(firstGreen).Color,
                Is.EqualTo(RunnerColor.Green));
            Assert.That(firstGreen, Is.EqualTo(8));
        }

        [Test]
        public void Stage4_ColorCycleDoesNotMutateMidRun()
        {
            StageDefinition stage = StageCatalog.GetByDisplayNumber(4);
            StageSession session = CreatePlaying(stage);

            AssertCycleHasThreeColors(session);
            for (int index = 0; index < stage.IntroGateCount; index++)
            {
                GatePlan plan = session.GetNextGatePlan();
                MatchCurrentColor(session, plan.Color);
                Assert.That(session.ResolveGate(plan.Color),
                    Is.EqualTo(GateOutcome.Matched));
            }
            AssertCycleHasThreeColors(session);
        }

        [Test]
        public void Stage4_EarlyTutorialPrimarilyRequiresZeroOrOneTap()
        {
            StageDefinition stage = StageCatalog.GetByDisplayNumber(4);
            DeterministicStageGateSequence sequence =
                new DeterministicStageGateSequence(stage);
            RunnerColor current = RunnerColor.Red;
            int easyCount = 0;

            for (int index = 0; index < stage.IntroGateCount; index++)
            {
                RunnerColor target = sequence.GetPlan(index).Color;
                int taps = RequiredTaps(stage, current, target);
                if (taps <= 1)
                {
                    easyCount++;
                }
                current = target;
            }

            Assert.That(easyCount, Is.GreaterThanOrEqualTo(7));
        }

        [Test]
        public void Stage4_PerfectFirstAttemptUsesRuntimeColorCycle()
        {
            StageDefinition stage = StageCatalog.GetByDisplayNumber(4);
            StageSimulationResult result = StageSimulationRunner.Run(
                stage,
                new StartItemSelection(false, false),
                SimulatedPlayerProfile.Get(SimulatedPlayerKind.Perfect),
                new GameplaySimulationSettings(1, 12345u, false));

            Assert.That(result.FirstAttemptClearCount, Is.EqualTo(1));
            Assert.That(result.FirstAttemptClearRate, Is.EqualTo(1f));
            Assert.That(result.FailureByGate[5], Is.Zero);
        }

        [Test]
        public void Stage4_SharedTapCalculationMatchesRuntimeCycle()
        {
            StageDefinition stage = StageCatalog.GetByDisplayNumber(4);
            StageSession session = CreatePlaying(stage);
            DeterministicStageGateSequence sequence =
                new DeterministicStageGateSequence(stage);

            for (int index = 0; index <= 5; index++)
            {
                GatePlan plan = sequence.GetPlan(index);
                int required = stage.GetRequiredTapCount(
                    session.GatesPassed,
                    session.CurrentColor,
                    plan.Color);
                for (int tap = 0; tap < required; tap++)
                {
                    Assert.That(session.TryToggleColor(), Is.True);
                }
                Assert.That(session.CurrentColor, Is.EqualTo(plan.Color));
                Assert.That(session.ResolveGate(plan.Color),
                    Is.EqualTo(GateOutcome.Matched));
            }
        }

        [Test]
        public void ExperimentCountdown_BlocksInputProgressAndJudgment()
        {
            ExperimentSession session = new ExperimentSession(
                ExperimentCatalog.Get(4, MechanicExperimentType.None));
            ExperimentGatePlan first = session.GetNextPlan();

            Assert.That(session.FlowState,
                Is.EqualTo(StageFlowState.Countdown));
            Assert.That(session.TryCycleColor(), Is.False);
            session.Advance(1f);
            Assert.That(session.ElapsedPlayingSeconds, Is.Zero);
            Assert.That(session.Resolve(first), Is.False);
            Assert.That(session.GatesPassed, Is.Zero);
        }

        [Test]
        public void ExperimentCountdown_CompletesOnceIntoPlaying()
        {
            ExperimentSession session = new ExperimentSession(
                ExperimentCatalog.Get(4, MechanicExperimentType.None));

            Assert.That(session.CompleteCountdown(), Is.True);
            Assert.That(session.FlowState,
                Is.EqualTo(StageFlowState.Playing));
            Assert.That(session.CompleteCountdown(), Is.False);
        }

        [Test]
        public void ExperimentMismatch_EntersFailedOnce()
        {
            ExperimentSession session = CreatePlayingExperiment(
                ExperimentCatalog.Get(4, MechanicExperimentType.None));
            ExperimentGatePlan first = session.GetNextPlan();
            while (session.CurrentColor == first.Color)
            {
                session.TryCycleColor();
            }

            Assert.That(session.Resolve(first), Is.False);
            Assert.That(session.FlowState,
                Is.EqualTo(StageFlowState.Failed));
            Assert.That(session.Resolve(first), Is.False);
            Assert.That(session.GatesPassed, Is.Zero);
        }

        [Test]
        public void ExperimentFinalGate_EntersCompletedOnce()
        {
            ExperimentSession session = CreatePlayingExperiment(
                ExperimentCatalog.Get(3, MechanicExperimentType.None));

            for (int index = 0;
                index < session.Definition.GateCount;
                index++)
            {
                ExperimentGatePlan plan = session.GetNextPlan();
                MatchExperimentColor(session, plan.Color);
                Assert.That(session.Resolve(plan), Is.True);
            }

            Assert.That(session.FlowState,
                Is.EqualTo(StageFlowState.StageCleared));
            Assert.That(session.Completed, Is.True);
            Assert.That(session.TryCycleColor(), Is.False);
        }

        [Test]
        public void ExperimentRestart_PreservesConditionAndResetsRuntime()
        {
            StartItemSelection items = new StartItemSelection(true, true);
            ExperimentSession session = new ExperimentSession(
                ExperimentCatalog.Get(
                    4,
                    MechanicExperimentType.Camouflage,
                    9876u),
                items);
            ExperimentGatePlan first = session.GetNextPlan();
            session.CompleteCountdown();
            session.Advance(2f);
            MatchExperimentColor(session, first.Color);
            session.Resolve(first);

            session.Restart();
            ExperimentGatePlan replay = session.GetNextPlan();

            Assert.That(session.Definition.ColorCount, Is.EqualTo(4));
            Assert.That(session.Definition.Mechanic,
                Is.EqualTo(MechanicExperimentType.Camouflage));
            Assert.That(session.Definition.Seed, Is.EqualTo(9876u));
            Assert.That(session.Items, Is.EqualTo(items));
            Assert.That(session.FlowState,
                Is.EqualTo(StageFlowState.Countdown));
            Assert.That(session.CurrentColor, Is.EqualTo(RunnerColor.Red));
            Assert.That(session.GatesPassed, Is.Zero);
            Assert.That(session.ElapsedPlayingSeconds, Is.Zero);
            Assert.That(session.ShieldActive, Is.False);
            Assert.That(session.BoosterActive, Is.False);
            Assert.That(replay.Color, Is.EqualTo(first.Color));
            Assert.That(replay.Spacing, Is.EqualTo(first.Spacing));
        }

        [Test]
        public void Goal_IsPartOfDeterministicStagePlanning()
        {
            StageDefinition stage = StageCatalog.GetByDisplayNumber(4);

            StageGoalPlan first = StageGoalPlanner.Create(stage, 48f, 40f);
            StageGoalPlan replay = StageGoalPlanner.Create(stage, 48f, 40f);

            Assert.That(first.DistanceFromPlayer,
                Is.EqualTo(replay.DistanceFromPlayer));
            Assert.That(first.GateCount, Is.EqualTo(stage.TargetGateCount));
            Assert.That(first.DistanceFromPlayer, Is.GreaterThan(48f));
        }

        [Test]
        public void Continue_ConsumesFailedGateImmediately()
        {
            StageSession session = CreatePlaying(
                StageCatalog.GetByDisplayNumber(1));
            GatePlan failed = session.GetNextGatePlan();
            MatchDifferentColor(session, failed.Color);
            Assert.That(session.ResolveGate(failed.Color),
                Is.EqualTo(GateOutcome.Mismatched));

            Assert.That(session.ContinueAfterFailure(), Is.True);

            Assert.That(session.FailedGatePendingForContinue, Is.False);
            Assert.That(session.ContinuedFailedGateResolutionCount,
                Is.EqualTo(1));
            Assert.That(session.GatesPassed, Is.EqualTo(1));
        }

        [Test]
        public void Continue_SelectsGateAfterConsumedFailure()
        {
            StageDefinition stage = StageCatalog.GetByDisplayNumber(1);
            StageSession session = CreatePlaying(stage);
            GatePlan failed = session.GetNextGatePlan();
            MatchDifferentColor(session, failed.Color);
            session.ResolveGate(failed.Color);

            session.ContinueAfterFailure();
            GatePlan next = session.GetNextGatePlan();
            DeterministicStageGateSequence reference =
                new DeterministicStageGateSequence(stage);
            reference.GetPlan(0);
            GatePlan plannedNext = reference.GetPlan(1);

            Assert.That(next.IndexInPattern, Is.EqualTo(1));
            Assert.That(next.Color, Is.EqualTo(session.CurrentColor));
            Assert.That(next.PlannedColor, Is.EqualTo(plannedNext.Color));
        }

        [TestCase(4, MechanicExperimentType.None)]
        [TestCase(4, MechanicExperimentType.Camouflage)]
        [TestCase(4, MechanicExperimentType.Fog)]
        [TestCase(4, MechanicExperimentType.Ice)]
        [TestCase(5, MechanicExperimentType.None)]
        [TestCase(6, MechanicExperimentType.None)]
        public void ExperimentLauncherOptions_MapToExistingDefinitions(
            int colorCount,
            MechanicExperimentType mechanic)
        {
            ExperimentDefinition definition =
                ExperimentCatalog.Get(colorCount, mechanic);

            Assert.That(definition.ColorCount, Is.EqualTo(colorCount));
            Assert.That(definition.Mechanic, Is.EqualTo(mechanic));
            Assert.That(definition.Id, Is.Not.Empty);
        }

        private static StageSession CreatePlaying(StageDefinition stage)
        {
            StageSession session = new StageSession(stage);
            session.BeginCountdown();
            session.CompleteCountdown();
            return session;
        }

        private static ExperimentSession CreatePlayingExperiment(
            ExperimentDefinition definition)
        {
            ExperimentSession session = new ExperimentSession(definition);
            Assert.That(session.CompleteCountdown(), Is.True);
            return session;
        }

        private static void MatchExperimentColor(
            ExperimentSession session,
            RunnerColor color)
        {
            while (session.CurrentColor != color)
            {
                Assert.That(session.TryCycleColor(), Is.True);
            }
        }

        private static void AssertCycleHasThreeColors(StageSession session)
        {
            RunnerColor start = session.CurrentColor;
            Assert.That(session.TryToggleColor(), Is.True);
            Assert.That(session.TryToggleColor(), Is.True);
            Assert.That(session.TryToggleColor(), Is.True);
            Assert.That(session.CurrentColor, Is.EqualTo(start));
        }

        private static void MatchCurrentColor(
            StageSession session,
            RunnerColor color)
        {
            while (session.CurrentColor != color)
            {
                session.TryToggleColor();
            }
        }

        private static void MatchDifferentColor(
            StageSession session,
            RunnerColor color)
        {
            if (session.CurrentColor == color)
            {
                session.TryToggleColor();
            }
        }

        private static int RequiredTaps(
            StageDefinition stage,
            RunnerColor current,
            RunnerColor target)
        {
            return stage.GetRequiredTapCount(0, current, target);
        }
    }
}
