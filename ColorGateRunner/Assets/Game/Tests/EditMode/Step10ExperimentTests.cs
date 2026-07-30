using ColorGateRunner.Core;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class Step10ExperimentTests
    {
        [TestCase(RunnerColor.Red, RunnerColorSymbol.Circle)]
        [TestCase(RunnerColor.Blue, RunnerColorSymbol.Square)]
        [TestCase(RunnerColor.Green, RunnerColorSymbol.Triangle)]
        [TestCase(RunnerColor.Yellow, RunnerColorSymbol.Star)]
        [TestCase(RunnerColor.Purple, RunnerColorSymbol.Diamond)]
        [TestCase(RunnerColor.Cyan, RunnerColorSymbol.Hexagon)]
        public void SixColorIdentityAndSymbolMapping_IsStable(
            RunnerColor color,
            RunnerColorSymbol symbol)
        {
            Assert.That(MobileUiPolicy.GetSymbol(color), Is.EqualTo(symbol));
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        public void ColorCycle_IsCorrectForTwoThroughSixColors(int count)
        {
            RunnerColor[] colors = Colors(count);
            for (int current = 0; current < count; current++)
            {
                for (int target = 0; target < count; target++)
                {
                    Assert.That(
                        MobileUiPolicy.GetRequiredTapCount(
                            colors,
                            colors[current],
                            colors[target]),
                        Is.EqualTo((target - current + count) % count));
                }
            }
        }

        [Test]
        public void ExperimentDefinitions_RemainDeterministic()
        {
            ExperimentDefinition definition = ExperimentCatalog.Get(
                6,
                MechanicExperimentType.None);
            DeterministicExperimentGateSequence first =
                new DeterministicExperimentGateSequence(definition);
            DeterministicExperimentGateSequence second =
                new DeterministicExperimentGateSequence(definition);

            for (int index = 0; index < definition.GateCount; index++)
            {
                ExperimentGatePlan a = first.GetPlan(index);
                ExperimentGatePlan b = second.GetPlan(index);
                Assert.That(a.Color, Is.EqualTo(b.Color));
                Assert.That(a.RequiredTapCount, Is.EqualTo(b.RequiredTapCount));
                Assert.That(a.Spacing, Is.EqualTo(b.Spacing));
            }
        }

        [Test]
        public void ExperimentRestart_ReplaysPlansAndColor()
        {
            ExperimentSession session = new ExperimentSession(
                ExperimentCatalog.Get(5, MechanicExperimentType.None));
            ExperimentGatePlan first = session.GetNextPlan();
            session.Restart();
            ExperimentGatePlan replay = session.GetNextPlan();

            Assert.That(replay.Color, Is.EqualTo(first.Color));
            Assert.That(replay.RequiredTapCount,
                Is.EqualTo(first.RequiredTapCount));
            Assert.That(session.CurrentColor, Is.EqualTo(RunnerColor.Red));
        }

        [Test]
        public void Camouflage_RevealsAfterOneInterveningGate()
        {
            ExperimentDefinition definition = ExperimentCatalog.Get(
                4,
                MechanicExperimentType.Camouflage);
            DeterministicExperimentGateSequence sequence =
                new DeterministicExperimentGateSequence(definition);
            ExperimentGatePlan camouflage = default;
            for (int index = 0; index < definition.GateCount; index++)
            {
                ExperimentGatePlan plan = sequence.GetPlan(index);
                if (plan.IsCamouflage)
                {
                    camouflage = plan;
                    break;
                }
            }

            Assert.That(
                camouflage.IsCamouflageRevealed(camouflage.GateIndex - 2),
                Is.False);
            Assert.That(
                camouflage.IsCamouflageRevealed(camouflage.GateIndex - 1),
                Is.True);
        }

        [Test]
        public void Fog_ExposesExactlyTwoNearestUnpassedGates()
        {
            ExperimentDefinition definition = ExperimentCatalog.Get(
                4,
                MechanicExperimentType.Fog);
            DeterministicExperimentGateSequence sequence =
                new DeterministicExperimentGateSequence(definition);
            int visible = 0;
            for (int index = 0; index <= definition.FogStartGate + 3; index++)
            {
                ExperimentGatePlan plan = sequence.GetPlan(index);
                if (index >= definition.FogStartGate &&
                    plan.IsFullyVisibleInFog(definition.FogStartGate))
                {
                    visible++;
                }
            }

            Assert.That(visible, Is.EqualTo(2));
        }

        [Test]
        public void Fog_DoesNotChangeGatePlans()
        {
            ExperimentDefinition normal = ExperimentCatalog.Get(
                5,
                MechanicExperimentType.None);
            ExperimentDefinition fog = ExperimentCatalog.Get(
                5,
                MechanicExperimentType.Fog);
            DeterministicExperimentGateSequence normalSequence =
                new DeterministicExperimentGateSequence(normal);
            DeterministicExperimentGateSequence fogSequence =
                new DeterministicExperimentGateSequence(fog);

            for (int index = 0; index < normal.GateCount; index++)
            {
                ExperimentGatePlan a = normalSequence.GetPlan(index);
                ExperimentGatePlan b = fogSequence.GetPlan(index);
                Assert.That(b.Color, Is.EqualTo(a.Color));
                Assert.That(b.RequiredTapCount,
                    Is.EqualTo(a.RequiredTapCount));
            }
        }

        [Test]
        public void Ice_ChangesSpeedButPreservesColorJudgment()
        {
            ExperimentDefinition definition = ExperimentCatalog.Get(
                4,
                MechanicExperimentType.Ice);
            ExperimentSession session = new ExperimentSession(definition);
            session.CompleteCountdown();
            ExperimentGatePlan ice = default;
            for (int index = 0; index <= definition.IceStartGate; index++)
            {
                ice = session.GetNextPlan();
                while (session.CurrentColor != ice.Color)
                {
                    session.TryCycleColor();
                }
                if (index < definition.IceStartGate)
                {
                    Assert.That(session.Resolve(ice), Is.True);
                }
            }

            Assert.That(ice.IsIce, Is.True);
            Assert.That(session.GetSpeedForPlan(ice),
                Is.GreaterThan(session.CurrentSpeed));
            session.TryCycleColor();
            Assert.That(session.Resolve(ice), Is.False);
            Assert.That(session.Failed, Is.True);
        }

        [Test]
        public void Ice_ExitRestoresCorrectNormalSpeed()
        {
            ExperimentDefinition definition = ExperimentCatalog.Get(
                3,
                MechanicExperimentType.Ice);
            ExperimentSession session = new ExperimentSession(definition);
            session.CompleteCountdown();
            ExperimentGatePlan plan = default;
            for (int index = 0; index <= definition.IceEndGate + 1; index++)
            {
                plan = session.GetNextPlan();
                while (session.CurrentColor != plan.Color)
                {
                    session.TryCycleColor();
                }
                Assert.That(session.Resolve(plan), Is.True);
            }

            Assert.That(plan.IsIce, Is.False);
            Assert.That(session.GetSpeedForPlan(plan),
                Is.EqualTo(session.CurrentSpeed));
        }

        [Test]
        public void RepeatedTapPlayerProfiles_AreValid()
        {
            for (int index = 0; index < 5; index++)
            {
                SimulatedPlayerProfile profile =
                    SimulatedPlayerProfile.Get((SimulatedPlayerKind)index);
                Assert.That(profile.IsValid(), Is.True);
                Assert.That(profile.MaximumComfortableTapBurst,
                    Is.GreaterThanOrEqualTo(1));
            }
        }

        [Test]
        public void TapWindowMetrics_HandleZeroAndRepeatedTaps()
        {
            TapWindowMetrics zero = TapWindowMetrics.Calculate(
                0, 0f, 1f, 0.2f, 0.15f, 0.1f, 0.08f);
            TapWindowMetrics three = TapWindowMetrics.Calculate(
                3, 0f, 1f, 0.2f, 0.15f, 0.1f, 0.08f);

            Assert.That(float.IsPositiveInfinity(
                zero.RequiredTapInterval), Is.True);
            Assert.That(three.RequiredTapInterval,
                Is.EqualTo(0.62f / 3f).Within(0.0001f));
            Assert.That(three.PostTapMargin,
                Is.EqualTo(0.4f).Within(0.0001f));
        }

        [Test]
        public void RiskClassification_AppliesDocumentedThresholds()
        {
            ExperimentSimulationResult averageHardExclusion =
                new ExperimentSimulationResult
                {
                    Profile = SimulatedPlayerKind.Average,
                    RunCount = 1000,
                    CompletionRate = 0.1f,
                    P10RequiredTapInterval = 0.3f
                };
            ExperimentSimulationResult expertHardExclusion =
                new ExperimentSimulationResult
                {
                    Profile = SimulatedPlayerKind.Expert,
                    RunCount = 1000,
                    CompletionRate = 0.49f,
                    P10RequiredTapInterval = 0.3f
                };
            ExperimentSimulationResult oneCaution =
                new ExperimentSimulationResult
                {
                    Profile = SimulatedPlayerKind.Average,
                    RunCount = 1000,
                    CompletionRate = 0.5f,
                    P10RequiredTapInterval = 0.1f
                };
            ExperimentSimulationResult multipleCautions =
                new ExperimentSimulationResult
                {
                    Profile = SimulatedPlayerKind.Average,
                    RunCount = 1000,
                    CompletionRate = 0.5f,
                    P10RequiredTapInterval = 0.1f
                };
            multipleCautions.TapDistribution[3] = 300;
            multipleCautions.FailureCounts[
                (int)ExperimentFailureCategory.RepeatedTapBurstTooSlow] = 700;

            Assert.That(
                ExperimentSimulationRunner.Classify(averageHardExclusion),
                Is.EqualTo(ExperimentRisk.HighRisk));
            Assert.That(
                ExperimentSimulationRunner.Classify(expertHardExclusion),
                Is.EqualTo(ExperimentRisk.HighRisk));
            Assert.That(
                ExperimentSimulationRunner.Classify(oneCaution),
                Is.EqualTo(ExperimentRisk.Caution));
            Assert.That(
                ExperimentSimulationRunner.Classify(multipleCautions),
                Is.EqualTo(ExperimentRisk.HighRisk));
        }

        [Test]
        public void ConditionRisk_CombinesAverageAndExpertHardExclusions()
        {
            ExperimentSimulationResult average =
                new ExperimentSimulationResult
                {
                    Profile = SimulatedPlayerKind.Average,
                    CompletionRate = 0.4f,
                    P10RequiredTapInterval = 0.3f
                };
            ExperimentSimulationResult expert =
                new ExperimentSimulationResult
                {
                    Profile = SimulatedPlayerKind.Expert,
                    CompletionRate = 0.49f,
                    P10RequiredTapInterval = 0.3f
                };

            ExperimentRiskAssessment assessment =
                ExperimentSimulationRunner.AssessCondition(average, expert);

            Assert.That(assessment.ExpertCompletionHardExclusion, Is.True);
            Assert.That(assessment.CautionFlagCount, Is.Zero);
            Assert.That(assessment.Risk, Is.EqualTo(ExperimentRisk.HighRisk));
        }

        [Test]
        public void SixColorBoundary_IsHighRiskAndBoundaryOnly()
        {
            ExperimentSimulationBatch batch =
                ExperimentSimulationRunner.RunFullMatrix(2);
            ExperimentSimulationResult result = null;
            for (int index = 0; index < batch.Results.Count; index++)
            {
                ExperimentSimulationResult candidate = batch.Results[index];
                if (candidate.ExperimentId ==
                        ExperimentShortlist.BoundaryOnlyExperimentId &&
                    candidate.Profile == SimulatedPlayerKind.Average)
                {
                    result = candidate;
                    break;
                }
            }

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Risk, Is.EqualTo(ExperimentRisk.HighRisk));
            Assert.That(
                ExperimentShortlist.IsBoundaryOnly(result.ExperimentId),
                Is.True);
            Assert.That(
                ExperimentReportFormatter.ToColorCapacityMarkdown(batch),
                Does.Contain("|HighRisk|AverageCompletionBelow20"));
            Assert.That(
                ExperimentReportFormatter.ToColorCapacityMarkdown(batch),
                Does.Contain("|BoundaryOnly|"));
        }

        [Test]
        public void ShortlistGeneration_IsDeterministic()
        {
            ExperimentSimulationBatch batch =
                ExperimentSimulationRunner.RunFullMatrix(2);
            string[] selected = ExperimentShortlist.Select(batch);

            Assert.That(
                selected,
                Is.EqualTo(ExperimentShortlist.Select(batch)));
            Assert.That(
                selected[selected.Length - 1],
                Is.EqualTo(ExperimentShortlist.BoundaryOnlyExperimentId));
        }

        private static RunnerColor[] Colors(int count)
        {
            RunnerColor[] result = new RunnerColor[count];
            for (int index = 0; index < count; index++)
            {
                result[index] = (RunnerColor)index;
            }
            return result;
        }
    }
}
