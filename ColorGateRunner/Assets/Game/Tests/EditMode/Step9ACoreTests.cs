using ColorGateRunner.Core;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class Step9ACoreTests
    {
        [Test]
        public void Lobby_SelectsLowestUnlockedUnclearedStage()
        {
            bool[] cleared = CreateClearedFlags();
            cleared[0] = true;
            Assert.That(
                LobbyProgression.SelectCurrentStage(3, cleared),
                Is.EqualTo(2));
        }

        [Test]
        public void Lobby_AllClearedKeepsFinalStage()
        {
            bool[] cleared = CreateClearedFlags(true);
            Assert.That(
                LobbyProgression.SelectCurrentStage(13, cleared),
                Is.EqualTo(13));
            Assert.That(LobbyProgression.IsPrototypeComplete(cleared), Is.True);
        }

        [TestCase(0, false, false, false)]
        [TestCase(1, true, false, false)]
        [TestCase(2, true, true, false)]
        [TestCase(3, true, true, true)]
        public void LobbyVisualTier_MapsProgression(
            int expected,
            bool stage2,
            bool stage4,
            bool stage5)
        {
            bool[] cleared = CreateClearedFlags();
            cleared[1] = stage2;
            cleared[3] = stage4;
            cleared[4] = stage5;
            Assert.That(LobbyProgression.GetVisualTier(cleared), Is.EqualTo(expected));
        }

        private static bool[] CreateClearedFlags(bool value = false)
        {
            bool[] result = new bool[StageCatalog.Count];
            if (value)
            {
                for (int index = 0; index < result.Length; index++)
                {
                    result[index] = true;
                }
            }
            return result;
        }

        [Test]
        public void TwoColorCycle_OrderAndNextColorAreStable()
        {
            StageSession session = CreatePlaying(1, default);
            Assert.That(session.CurrentColor, Is.EqualTo(RunnerColor.Red));
            session.TryToggleColor();
            Assert.That(session.CurrentColor, Is.EqualTo(RunnerColor.Blue));
            session.TryToggleColor();
            Assert.That(session.CurrentColor, Is.EqualTo(RunnerColor.Red));
        }

        [Test]
        public void ThreeColorCycle_OrderAndNextColorAreStable()
        {
            StageSession session = CreatePlaying(5, default);
            session.TryToggleColor();
            Assert.That(session.CurrentColor, Is.EqualTo(RunnerColor.Blue));
            session.TryToggleColor();
            Assert.That(session.CurrentColor, Is.EqualTo(RunnerColor.Green));
            session.TryToggleColor();
            Assert.That(session.CurrentColor, Is.EqualTo(RunnerColor.Red));
        }

        [Test]
        public void Continue_HasNoAttemptCapAndRemainsBoundedByFiniteStage()
        {
            StageSession session = CreatePlaying(20, default);
            FailCurrentGate(session);
            const int requestedContinues = 6;
            for (int expectedCount = 1;
                expectedCount <= requestedContinues;
                expectedCount++)
            {
                float elapsedAtFailure = session.ElapsedPlayingSeconds;
                float speedAtFailure = session.SpeedBeforeFailure;
                RunnerColor colorAtFailure = session.CurrentColor;
                int cursorAtFailure = session.SequenceCursor;
                int gatesAtFailure = session.GatesPassed;
                Assert.That(session.ContinueAvailable, Is.True);
                Assert.That(session.ContinueAfterFailure(), Is.True);
                Assert.That(session.ContinueUseCount, Is.EqualTo(expectedCount));
                Assert.That(session.ContinueUsed, Is.True);
                Assert.That(session.ElapsedPlayingSeconds,
                    Is.EqualTo(elapsedAtFailure));
                Assert.That(session.CurrentSpeed, Is.EqualTo(speedAtFailure));
                Assert.That(session.CurrentColor, Is.EqualTo(colorAtFailure));
                Assert.That(session.SequenceCursor, Is.EqualTo(cursorAtFailure));
                Assert.That(session.GatesPassed, Is.EqualTo(gatesAtFailure + 1));
                session.CompleteCountdown();
                if (expectedCount < requestedContinues)
                {
                    FailAfterProtection(session);
                }
            }

            Assert.That(session.ContinueUseCount, Is.EqualTo(requestedContinues));
            Assert.That(session.ContinueUseCount,
                Is.LessThanOrEqualTo(session.Stage.TargetGateCount));
        }

        [Test]
        public void Continue_CountChangesOnlyOnSuccessAndRetryResetsIt()
        {
            StageSession session = CreatePlaying(1, default);
            Assert.That(session.ContinueAfterFailure(), Is.False);
            Assert.That(session.ContinueUseCount, Is.Zero);

            GatePlan plan = session.GetNextGatePlan();
            while (session.CurrentColor == plan.Color)
            {
                session.TryToggleColor();
            }
            session.ResolveGate(plan.Color);
            Assert.That(session.ContinueAfterFailure(), Is.True);
            Assert.That(session.ContinueUseCount, Is.EqualTo(1));

            session.RetryToSelection();
            Assert.That(session.ContinueUseCount, Is.Zero);
            Assert.That(session.ContinueUsed, Is.False);
        }

        [Test]
        public void Continue_DoesNotRestoreConsumedShieldOrBooster()
        {
            StageSession session = CreatePlaying(
                8,
                new StartItemSelection(true, true));
            session.Advance(1f, session.Stage.BoosterDistance + 1f);
            session.Advance(StageSession.BoosterExitDuration, 0f);
            GatePlan first = session.GetNextGatePlan();
            RunnerColor wrong = Other(first.Color);
            while (session.CurrentColor != wrong)
            {
                session.TryToggleColor();
            }
            session.ResolveGate(first.Color);
            session.Advance(1.1f, 0f);
            GatePlan second = session.GetNextGatePlan();
            wrong = Other(second.Color);
            while (session.CurrentColor != wrong)
            {
                session.TryToggleColor();
            }
            session.ResolveGate(second.Color);

            Assert.That(session.ContinueAfterFailure(), Is.True);
            session.CompleteCountdown();
            Assert.That(session.ShieldActive, Is.False);
            Assert.That(session.BoosterActive, Is.False);
        }

        [Test]
        public void Continue_GrantsSafeResumeSequenceAndProtection()
        {
            StageSession session = CreateFailedSession(default);
            RunnerColor color = session.CurrentColor;
            session.ContinueAfterFailure();
            session.CompleteCountdown();

            GatePlan first = session.GetNextGatePlan();
            Assert.That(first.Color, Is.EqualTo(color));
            Assert.That(session.ContinueProtectionActive, Is.True);
            Assert.That(session.SafeGateCountRemaining, Is.EqualTo(2));
        }

        [Test]
        public void ContinuedClear_DoesNotReplaceBestRecords()
        {
            StageRecord current = new StageRecord(true, 40f, 44f, 2);
            StageRecord result = StageProgress.RecordClear(
                current,
                20f,
                default,
                true);

            Assert.That(result.BestTime, Is.EqualTo(40f));
            Assert.That(result.BestNoItemTime, Is.EqualTo(44f));
            Assert.That(result.ClearCount, Is.EqualTo(3));
            Assert.That(result.ContinuedClearCount, Is.EqualTo(1));
            Assert.That(
                StageProgress.HighestUnlockedAfterClear(1, 1),
                Is.EqualTo(2));
        }

        [Test]
        public void PostBooster_FirstGateMatchesAndSecondNeedsAtMostOneTap()
        {
            StageSession session = CreatePlaying(
                8,
                new StartItemSelection(false, true));
            session.Advance(1f, session.Stage.BoosterDistance + 1f);

            GatePlan first = session.GetNextGatePlan();
            Assert.That(first.Color, Is.EqualTo(session.CurrentColor));
            session.ResolveGate(first.Color);
            GatePlan second = session.GetNextGatePlan();
            if (session.CurrentColor != second.Color)
            {
                session.TryToggleColor();
            }
            Assert.That(session.CurrentColor, Is.EqualTo(second.Color));
        }

        [Test]
        public void StageTwo_HasPerceptibleAuthoredCadenceSections()
        {
            Assert.That(StageSectionCatalog.GetSectionCount(2), Is.EqualTo(5));
            Assert.That(
                StageSectionCatalog.GetSection(2, 0).Pattern,
                Is.EqualTo(GatePatternType.Steady));
            Assert.That(
                StageSectionCatalog.GetSection(2, 3).Pattern,
                Is.EqualTo(GatePatternType.Syncopation));
        }

        [Test]
        public void StageThree_HasAuthoredExceptionSections()
        {
            Assert.That(StageSectionCatalog.GetSectionCount(3), Is.EqualTo(5));
            DeterministicStageGateSequence sequence =
                new DeterministicStageGateSequence(
                    StageCatalog.GetByDisplayNumber(3));
            RunnerColor[] expected =
            {
                RunnerColor.Red, RunnerColor.Red, RunnerColor.Red, RunnerColor.Red,
                RunnerColor.Blue, RunnerColor.Red, RunnerColor.Red, RunnerColor.Red
            };
            for (int index = 0; index < expected.Length; index++)
            {
                Assert.That(sequence.GetPlan(index).Color, Is.EqualTo(expected[index]));
            }
        }

        [Test]
        public void SimulationProfiles_AreExplicitAndValid()
        {
            for (int kind = 0; kind <= (int)SimulatedPlayerKind.Stress; kind++)
            {
                Assert.That(
                    SimulatedPlayerProfile.Get((SimulatedPlayerKind)kind).IsValid(),
                    Is.True);
            }
        }

        [Test]
        public void PerfectSimulation_IsDeterministicAndCompletes()
        {
            StageDefinition stage = StageCatalog.GetByDisplayNumber(1);
            GameplaySimulationSettings settings =
                new GameplaySimulationSettings(1, 12345u, true);
            StageSimulationResult first = StageSimulationRunner.Run(
                stage,
                default,
                SimulatedPlayerProfile.Get(SimulatedPlayerKind.Perfect),
                settings);
            StageSimulationResult second = StageSimulationRunner.Run(
                stage,
                default,
                SimulatedPlayerProfile.Get(SimulatedPlayerKind.Perfect),
                settings);

            Assert.That(first.FirstAttemptClearRate, Is.EqualTo(1f));
            Assert.That(first.MedianCompletionTime,
                Is.EqualTo(second.MedianCompletionTime));
        }

        [Test]
        public void StochasticSimulation_ReplaysWithSameSeed()
        {
            StageDefinition stage = StageCatalog.GetByDisplayNumber(3);
            GameplaySimulationSettings settings =
                new GameplaySimulationSettings(100, 98765u, true);
            StageSimulationResult first = StageSimulationRunner.Run(
                stage,
                default,
                SimulatedPlayerProfile.Get(SimulatedPlayerKind.Average),
                settings);
            StageSimulationResult second = StageSimulationRunner.Run(
                stage,
                default,
                SimulatedPlayerProfile.Get(SimulatedPlayerKind.Average),
                settings);

            Assert.That(first.FirstAttemptClearCount,
                Is.EqualTo(second.FirstAttemptClearCount));
            Assert.That(first.MissedInputCount,
                Is.EqualTo(second.MissedInputCount));
            Assert.That(first.MedianFailureProgress,
                Is.EqualTo(second.MedianFailureProgress));
            Assert.That(first.TotalContinueUseCount,
                Is.EqualTo(second.TotalContinueUseCount));
            Assert.That(first.MaximumContinueUseCountObserved,
                Is.EqualTo(second.MaximumContinueUseCountObserved));
            Assert.That(first.ContinueUseHistogram,
                Is.EqualTo(second.ContinueUseHistogram));
        }

        [Test]
        public void Simulation_ContinueUseMetricsAreDeterministicAndStageBounded()
        {
            StageDefinition stage = StageCatalog.GetByDisplayNumber(3);
            GameplaySimulationSettings settings =
                new GameplaySimulationSettings(100, 43210u, true);
            StageSimulationResult result = StageSimulationRunner.Run(
                stage,
                default,
                SimulatedPlayerProfile.Get(SimulatedPlayerKind.Stress),
                settings);

            int histogramRuns = 0;
            int histogramUses = 0;
            for (int count = 0; count < result.ContinueUseHistogram.Length; count++)
            {
                histogramRuns += result.ContinueUseHistogram[count];
                histogramUses += count * result.ContinueUseHistogram[count];
            }

            Assert.That(result.ContinueUseHistogram.Length,
                Is.EqualTo(stage.TargetGateCount + 1));
            Assert.That(histogramRuns, Is.EqualTo(settings.Runs));
            Assert.That(histogramUses, Is.EqualTo(result.TotalContinueUseCount));
            Assert.That(result.MaximumContinueUseCountObserved,
                Is.LessThanOrEqualTo(stage.TargetGateCount));
        }

        [Test]
        public void Simulation_DisabledContinueReportsZeroUseMetrics()
        {
            const int runs = 100;
            StageSimulationResult result = StageSimulationRunner.Run(
                StageCatalog.GetByDisplayNumber(3),
                default,
                SimulatedPlayerProfile.Get(SimulatedPlayerKind.Stress),
                new GameplaySimulationSettings(runs, 24680u, false));

            Assert.That(result.TotalContinueUseCount, Is.Zero);
            Assert.That(result.AverageContinueUseCount, Is.Zero);
            Assert.That(result.MaximumContinueUseCountObserved, Is.Zero);
            Assert.That(result.ContinueUseHistogram[0], Is.EqualTo(runs));
            for (int count = 1; count < result.ContinueUseHistogram.Length; count++)
            {
                Assert.That(result.ContinueUseHistogram[count], Is.Zero);
            }
        }

        [Test]
        public void SimulationReport_EmitsContinueUseMetricsInAllFormats()
        {
            SimulationBatchResult batch = new SimulationBatchResult();
            batch.Results.Add(StageSimulationRunner.Run(
                StageCatalog.GetByDisplayNumber(3),
                default,
                SimulatedPlayerProfile.Get(SimulatedPlayerKind.Stress),
                new GameplaySimulationSettings(10, 13579u, true)));

            string csv = SimulationReportFormatter.ToCsv(batch);
            string json = SimulationReportFormatter.ToJson(batch);
            string markdown = SimulationReportFormatter.ToMarkdown(batch);

            Assert.That(csv, Does.Contain("totalContinueUses"));
            Assert.That(csv, Does.Contain("continueUseHistogram"));
            Assert.That(json, Does.Contain("\"totalContinueUseCount\""));
            Assert.That(json, Does.Contain("\"averageContinueUseCount\""));
            Assert.That(json, Does.Contain("\"maximumContinueUseCountObserved\""));
            Assert.That(json, Does.Contain("\"continueUseHistogram\":["));
            Assert.That(markdown,
                Does.Contain("Uses by count (0..gate count)"));
        }

        [Test]
        public void SimulationReport_ContainsRequiredMechanicalSections()
        {
            SimulationBatchResult batch = new SimulationBatchResult();
            batch.Results.Add(StageSimulationRunner.Run(
                StageCatalog.GetByDisplayNumber(1),
                default,
                SimulatedPlayerProfile.Get(SimulatedPlayerKind.Perfect),
                new GameplaySimulationSettings(1, 1u, true)));

            string report = SimulationReportFormatter.ToMarkdown(batch);
            Assert.That(report, Does.Contain("Stage duration"));
            Assert.That(report, Does.Contain("Stage 2 before/after"));
            Assert.That(report, Does.Contain("Booster exit"));
            Assert.That(report, Does.Contain("Human-only"));
            Assert.That(report, Does.Not.Contain("is fun"));
        }

        private static StageSession CreatePlaying(
            int stage,
            StartItemSelection items)
        {
            StageSession session =
                new StageSession(StageCatalog.GetByDisplayNumber(stage));
            session.SelectItems(items);
            session.BeginCountdown();
            session.CompleteCountdown();
            return session;
        }

        private static StageSession CreateFailedSession(StartItemSelection items)
        {
            StageSession session = CreatePlaying(1, items);
            FailCurrentGate(session);
            return session;
        }

        private static void FailCurrentGate(StageSession session)
        {
            GatePlan plan = session.GetNextGatePlan();
            while (session.CurrentColor == plan.Color)
            {
                session.TryToggleColor();
            }
            session.ResolveGate(plan.Color);
        }

        private static void FailAfterProtection(StageSession session)
        {
            session.Advance(1.1f, 0f);
            while (session.SafeGateCountRemaining > 0)
            {
                GatePlan safe = session.GetNextGatePlan();
                while (session.CurrentColor != safe.Color)
                {
                    session.TryToggleColor();
                }
                session.ResolveGate(safe.Color);
            }
            GatePlan plan = session.GetNextGatePlan();
            while (session.CurrentColor == plan.Color)
            {
                session.TryToggleColor();
            }
            session.ResolveGate(plan.Color);
        }

        private static RunnerColor Other(RunnerColor color)
        {
            return color == RunnerColor.Red
                ? RunnerColor.Blue
                : RunnerColor.Red;
        }
    }
}
