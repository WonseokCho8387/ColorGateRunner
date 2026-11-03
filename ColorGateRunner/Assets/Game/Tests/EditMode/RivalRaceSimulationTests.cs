using ColorGateRunner.Core;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class RivalRaceSimulationTests
    {
        private const float Lead = 48f;
        private const float GoalOffset = 40f;

        [Test]
        public void GateDistancesAndGoal_MatchStageGoalPlanner()
        {
            StageDefinition stage = StageCatalog.GetByDisplayNumber(5);
            var race = new RivalRaceSimulation(stage, Lead, GoalOffset);
            StageGoalPlan plan = StageGoalPlanner.Create(stage, Lead, GoalOffset);

            Assert.That(race.GateCount, Is.EqualTo(stage.TargetGateCount));
            Assert.That(race.GoalDistance,
                Is.EqualTo(plan.DistanceFromPlayer).Within(0.0001f));
            for (int gate = 1; gate < race.GateCount; gate++)
            {
                Assert.That(race.GetGateDistance(gate),
                    Is.GreaterThan(race.GetGateDistance(gate - 1)));
            }
        }

        [Test]
        public void SameStage_ReplaysIdenticalOutcomesAndProgress()
        {
            StageDefinition stage = StageCatalog.GetByDisplayNumber(8);
            var first = new RivalRaceSimulation(stage, Lead, GoalOffset);
            var replay = new RivalRaceSimulation(stage, Lead, GoalOffset);
            for (int step = 0; step < 600; step++)
            {
                first.Advance(1f / 60f);
                replay.Advance(1f / 60f);
            }

            for (int rival = 0; rival < RivalRaceSimulation.RivalCount; rival++)
            {
                Assert.That(replay.GetDistance(rival),
                    Is.EqualTo(first.GetDistance(rival)));
                Assert.That(replay.GetResolvedGateCount(rival),
                    Is.EqualTo(first.GetResolvedGateCount(rival)));
                for (int gate = 0; gate < first.GateCount; gate++)
                {
                    Assert.That(replay.PassesGate(rival, gate),
                        Is.EqualTo(first.PassesGate(rival, gate)));
                }
            }
        }

        [Test]
        public void GateOutcomes_DoNotDependOnFrameRate()
        {
            StageDefinition stage = StageCatalog.GetByDisplayNumber(10);
            var smooth = new RivalRaceSimulation(stage, Lead, GoalOffset);
            var coarse = new RivalRaceSimulation(stage, Lead, GoalOffset);
            for (int gate = 0; gate < smooth.GateCount; gate++)
            {
                for (int rival = 0; rival < RivalRaceSimulation.RivalCount; rival++)
                {
                    Assert.That(coarse.PassesGate(rival, gate),
                        Is.EqualTo(smooth.PassesGate(rival, gate)));
                }
            }

            RunToFinish(smooth, 1f / 120f);
            RunToFinish(coarse, 1f / 20f);
            for (int rival = 0; rival < RivalRaceSimulation.RivalCount; rival++)
            {
                Assert.That(smooth.HasFinished(rival), Is.True);
                Assert.That(coarse.HasFinished(rival), Is.True);
                Assert.That(smooth.GetResolvedGateCount(rival),
                    Is.EqualTo(smooth.GateCount));
                Assert.That(coarse.GetFinishSeconds(rival),
                    Is.EqualTo(smooth.GetFinishSeconds(rival)).Within(1f));
            }
        }

        [Test]
        public void FailedGate_StumblesAndSlowsTheRival()
        {
            StageDefinition stage = StageCatalog.GetByDisplayNumber(12);
            var race = new RivalRaceSimulation(stage, Lead, GoalOffset);
            int rival = 0;
            int failedGate = -1;
            for (int gate = 0; gate < race.GateCount; gate++)
            {
                if (!race.PassesGate(rival, gate))
                {
                    failedGate = gate;
                    break;
                }
            }
            Assume.That(failedGate, Is.GreaterThanOrEqualTo(0));

            while (race.GetResolvedGateCount(rival) <= failedGate)
            {
                race.Advance(1f / 60f);
            }
            Assert.That(race.IsStumbling(rival), Is.True);
            float before = race.GetDistance(rival);
            race.Advance(0.1f);
            float stumbleStep = race.GetDistance(rival) - before;

            RivalProfile profile = race.GetProfile(rival);
            while (race.IsStumbling(rival))
            {
                race.Advance(1f / 60f);
            }
            before = race.GetDistance(rival);
            race.Advance(0.1f);
            float normalStep = race.GetDistance(rival) - before;
            Assert.That(stumbleStep,
                Is.LessThan(normalStep * (profile.StumbleSpeedFactor + 0.1f)));
        }

        [Test]
        public void ProfilesAndOutcomes_AreBoundedAndVaried()
        {
            StageDefinition stage = StageCatalog.GetByDisplayNumber(20);
            var race = new RivalRaceSimulation(stage, Lead, GoalOffset);
            for (int rival = 0; rival < RivalRaceSimulation.RivalCount; rival++)
            {
                RivalProfile profile = race.GetProfile(rival);
                Assert.That(profile.PaceFactor, Is.InRange(0.9f, 1.1f));
                int fails = 0;
                for (int gate = 0; gate < race.GateCount; gate++)
                {
                    if (!race.PassesGate(rival, gate))
                    {
                        fails++;
                    }
                }
                Assert.That(fails, Is.LessThan(race.GateCount / 2));
            }
        }

        [Test]
        public void PlayerPlace_RanksByFinishTimeThenDistance()
        {
            StageDefinition stage = StageCatalog.GetByDisplayNumber(3);
            var race = new RivalRaceSimulation(stage, Lead, GoalOffset);
            Assert.That(race.GetPlayerPlace(0f, -1f), Is.EqualTo(1));
            race.Advance(2f);
            Assert.That(race.GetPlayerPlace(0f, -1f), Is.EqualTo(3));
            Assert.That(race.GetPlayerPlace(race.GoalDistance, 1f), Is.EqualTo(1));

            RunToFinish(race, 1f / 30f);
            float slowest = System.Math.Max(
                race.GetFinishSeconds(0),
                race.GetFinishSeconds(1));
            Assert.That(race.GetPlayerPlace(race.GoalDistance, slowest + 1f),
                Is.EqualTo(3));
            Assert.That(race.GetPlayerPlace(race.GoalDistance * 0.5f, -1f),
                Is.EqualTo(3));
        }

        [Test]
        public void Rivals_StartFromStageTwoAndLeaveThePlayerSessionUntouched()
        {
            Assert.That(RivalRaceSimulation.IsEnabledFor(
                StageCatalog.GetByDisplayNumber(1)), Is.False);
            StageDefinition stage = StageCatalog.GetByDisplayNumber(2);
            Assert.That(RivalRaceSimulation.IsEnabledFor(stage), Is.True);

            var reference = new StageSession(stage);
            var session = new StageSession(stage);
            var race = new RivalRaceSimulation(stage, Lead, GoalOffset);
            RunToFinish(race, 1f / 60f);
            for (int gate = 0; gate < stage.TargetGateCount; gate++)
            {
                GatePlan expected = reference.GetGatePlan(gate);
                GatePlan actual = session.GetGatePlan(gate);
                Assert.That(actual.Color, Is.EqualTo(expected.Color));
                Assert.That(actual.Spacing, Is.EqualTo(expected.Spacing));
            }
        }

        private static void RunToFinish(RivalRaceSimulation race, float step)
        {
            for (int guard = 0; guard < 200000; guard++)
            {
                if (race.HasFinished(0) && race.HasFinished(1))
                {
                    return;
                }
                race.Advance(step);
            }
            Assert.Fail("Rivals did not finish.");
        }
    }
}
