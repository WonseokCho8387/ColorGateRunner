using System;
using System.Collections.Generic;
using ColorGateRunner.Core;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class Iteration3CloneTests
    {
        [Test]
        public void CloneSettings_UsesApprovedSourcesAndGap()
        {
            CloneSettings settings = CloneSettings.CreateApproved();

            Assert.That(settings.SourceCount, Is.EqualTo(2));
            Assert.That(settings.GetSourceIndex(0), Is.EqualTo(3));
            Assert.That(settings.GetSourceIndex(1), Is.EqualTo(6));
            Assert.That(settings.GapSeconds, Is.EqualTo(0.45f));
        }

        [Test]
        public void CloneSettings_RejectsDefinitionsWithTooFewSources()
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(
                () => new ExperimentDefinition(
                    4,
                    MechanicExperimentType.Clone,
                    12345u,
                    CloneSettings.CreateApproved(),
                    5));

            Assert.That(
                exception.Message,
                Does.Contain("at least 6 non-Clone judgment gates"));
        }

        [Test]
        public void CloneSequence_InsertsExactlyTwoClonesAfterSourcesThreeAndSix()
        {
            ExperimentGatePlan[] plans = Generate(
                ExperimentCatalog.Get(
                    4,
                    MechanicExperimentType.Clone));
            List<ExperimentGatePlan> sources =
                FindByRole(plans, ExperimentGateRole.Source);
            List<ExperimentGatePlan> clones =
                FindByRole(plans, ExperimentGateRole.Clone);

            Assert.That(plans.Length, Is.EqualTo(42));
            Assert.That(sources.Count, Is.EqualTo(2));
            Assert.That(clones.Count, Is.EqualTo(2));
            AssertSourceClonePair(sources[0], clones[0], 2, 3);
            AssertSourceClonePair(sources[1], clones[1], 6, 7);
            Assert.That(sources[0].NonCloneGateIndex, Is.EqualTo(2));
            Assert.That(sources[1].NonCloneGateIndex, Is.EqualTo(5));
        }

        [Test]
        public void CloneSequence_SameSeedReplaysIdentically()
        {
            ExperimentDefinition definition = ExperimentCatalog.Get(
                5,
                MechanicExperimentType.Clone,
                98765u);
            ExperimentGatePlan[] first = Generate(definition);
            ExperimentGatePlan[] replay = Generate(definition);

            Assert.That(replay.Length, Is.EqualTo(first.Length));
            for (int index = 0; index < first.Length; index++)
            {
                AssertPlanEqual(first[index], replay[index]);
            }
        }

        [Test]
        public void CloneSequence_UsesSourceColorAndConfiguredGap()
        {
            ExperimentGatePlan[] plans = Generate(
                ExperimentCatalog.Get(
                    4,
                    MechanicExperimentType.Clone));

            for (int index = 0; index < plans.Length; index++)
            {
                ExperimentGatePlan clone = plans[index];
                if (!clone.IsClone)
                {
                    continue;
                }
                ExperimentGatePlan source = plans[clone.SourceGateId];
                Assert.That(clone.Color, Is.EqualTo(source.Color));
                Assert.That(clone.RequiredTapCount, Is.Zero);
                Assert.That(clone.BaseSpeed, Is.EqualTo(source.BaseSpeed));
                Assert.That(
                    clone.Spacing,
                    Is.EqualTo(source.BaseSpeed * 0.45f).Within(0.0001f));
                Assert.That(clone.Cadence, Is.EqualTo(0.45f));
            }
        }

        [Test]
        public void CloneSequence_PreservesEveryNonClonePlanAndFollowingGap()
        {
            uint seed = 54321u;
            ExperimentGatePlan[] baseline = Generate(
                ExperimentCatalog.Get(
                    4,
                    MechanicExperimentType.None,
                    seed));
            ExperimentGatePlan[] withClones = Generate(
                ExperimentCatalog.Get(
                    4,
                    MechanicExperimentType.Clone,
                    seed));
            int nonClone = 0;
            for (int index = 0; index < withClones.Length; index++)
            {
                ExperimentGatePlan plan = withClones[index];
                if (plan.IsClone)
                {
                    continue;
                }
                Assert.That(plan.NonCloneGateIndex, Is.EqualTo(nonClone));
                Assert.That(plan.Color, Is.EqualTo(baseline[nonClone].Color));
                Assert.That(
                    plan.RequiredTapCount,
                    Is.EqualTo(baseline[nonClone].RequiredTapCount));
                Assert.That(
                    plan.Spacing,
                    Is.EqualTo(baseline[nonClone].Spacing).Within(0.0001f));
                nonClone++;
            }
            Assert.That(nonClone, Is.EqualTo(baseline.Length));
        }

        [Test]
        public void CloneSession_SourceAndCloneAreIndependentJudgments()
        {
            ExperimentSession session = CreatePlaying(
                MechanicExperimentType.Clone,
                shield: false);
            ExperimentGatePlan source = PassUntilFirstSource(session);

            Assert.That(source.IsSource, Is.True);
            Assert.That(session.GatesPassed, Is.EqualTo(3));
            ExperimentGatePlan clone = session.GetNextPlan();
            Assert.That(clone.IsClone, Is.True);
            Assert.That(clone.SourceGateId, Is.EqualTo(source.GateId));

            session.TryCycleColor();

            Assert.That(session.Resolve(clone), Is.False);
            Assert.That(session.FlowState, Is.EqualTo(StageFlowState.Failed));
            Assert.That(
                session.LastFailureCause,
                Is.EqualTo(ExperimentRuntimeFailureCause.CloneGateMiss));
        }

        [Test]
        public void CloneSession_CloneSuccessCountsTowardCompletion()
        {
            ExperimentSession session = CreatePlaying(
                MechanicExperimentType.Clone,
                shield: false);

            while (session.FlowState == StageFlowState.Playing)
            {
                PassNext(session);
            }

            Assert.That(session.GatesPassed, Is.EqualTo(42));
            Assert.That(
                session.FlowState,
                Is.EqualTo(StageFlowState.StageCleared));
        }

        [Test]
        public void CloneSession_ShieldConsumesOnceAndPassesClone()
        {
            ExperimentSession session = CreatePlaying(
                MechanicExperimentType.Clone,
                shield: true);
            PassUntilFirstSource(session);
            ExperimentGatePlan clone = session.GetNextPlan();
            session.TryCycleColor();

            Assert.That(session.Resolve(clone), Is.True);
            Assert.That(session.ShieldActive, Is.False);
            Assert.That(session.GatesPassed, Is.EqualTo(4));
            Assert.That(session.FlowState, Is.EqualTo(StageFlowState.Playing));
            Assert.That(
                session.LastFailureCause,
                Is.EqualTo(ExperimentRuntimeFailureCause.None));
            Assert.That(session.Resolve(clone), Is.False);
            Assert.That(session.GatesPassed, Is.EqualTo(4));
        }

        [Test]
        public void CloneCamouflage_UsesOrdinaryHideRevealAndJudgment()
        {
            ExperimentDefinition definition = new ExperimentDefinition(
                4,
                MechanicExperimentType.Camouflage,
                ExperimentCatalog.DefaultSeed,
                CloneSettings.CreateApproved());
            ExperimentGatePlan[] plans = Generate(definition);
            ExperimentGatePlan clone =
                FindByRole(plans, ExperimentGateRole.Clone)[0];
            ExperimentGatePlan source = plans[clone.SourceGateId];

            Assert.That(clone.IsCamouflage, Is.True);
            Assert.That(
                clone.IsCamouflageRevealed(clone.GateIndex - 2),
                Is.False);
            Assert.That(
                clone.IsCamouflageRevealed(clone.GateIndex - 1),
                Is.True);
            Assert.That(clone.Color, Is.EqualTo(source.Color));

            ExperimentSession session = new ExperimentSession(definition);
            session.CompleteCountdown();
            PassUntilFirstSource(session);
            clone = session.GetNextPlan();
            session.TryCycleColor();

            Assert.That(session.Resolve(clone), Is.False);
            Assert.That(session.FlowState, Is.EqualTo(StageFlowState.Failed));
        }

        [Test]
        public void CloneRestart_PreservesLayoutAndResetsRuntimeState()
        {
            ExperimentSession session = CreatePlaying(
                MechanicExperimentType.Clone,
                shield: true);
            PassUntilFirstSource(session);
            ExperimentGatePlan clone = session.GetNextPlan();
            session.TryCycleColor();
            Assert.That(session.Resolve(clone), Is.True);
            Assert.That(session.ShieldActive, Is.False);

            session.Restart();

            Assert.That(session.GatesPassed, Is.Zero);
            Assert.That(session.SequenceCursor, Is.Zero);
            Assert.That(
                session.FlowState,
                Is.EqualTo(StageFlowState.Countdown));
            Assert.That(session.ShieldActive, Is.False);
            Assert.That(
                session.LastFailureCause,
                Is.EqualTo(ExperimentRuntimeFailureCause.None));
            Assert.That(session.CompleteCountdown(), Is.True);
            Assert.That(session.ShieldActive, Is.True);
            ExperimentGatePlan replayFirst = session.GetNextPlan();
            ExperimentGatePlan expectedFirst = Generate(
                session.Definition)[0];
            AssertPlanEqual(expectedFirst, replayFirst);
        }

        [Test]
        public void CloneSession_DisablesBoosterSelection()
        {
            ExperimentSession session = new ExperimentSession(
                ExperimentCatalog.Get(
                    4,
                    MechanicExperimentType.Clone),
                new StartItemSelection(true, true));

            Assert.That(session.Items.Shield, Is.True);
            Assert.That(session.Items.Booster, Is.False);
            session.CompleteCountdown();
            Assert.That(session.BoosterActive, Is.False);
        }

        [Test]
        public void CloneJudgment_IsBlockedOutsidePlaying()
        {
            ExperimentSession session = new ExperimentSession(
                ExperimentCatalog.Get(
                    4,
                    MechanicExperimentType.Clone));
            ExperimentGatePlan first = session.GetNextPlan();

            Assert.That(session.Resolve(first), Is.False);
            Assert.That(session.GatesPassed, Is.Zero);
        }

        [Test]
        public void CloneSimulation_SameSeedProducesSameCompletedResult()
        {
            ExperimentDefinition definition = ExperimentCatalog.Get(
                4,
                MechanicExperimentType.Clone,
                24680u);
            SimulatedPlayerProfile perfect = SimulatedPlayerProfile.Get(
                SimulatedPlayerKind.Perfect);

            ExperimentSimulationResult first =
                ExperimentSimulationRunner.Run(
                    definition,
                    perfect,
                    1,
                    13579u);
            ExperimentSimulationResult replay =
                ExperimentSimulationRunner.Run(
                    definition,
                    perfect,
                    1,
                    13579u);

            Assert.That(first.CompletedCount, Is.EqualTo(1));
            Assert.That(replay.CompletedCount,
                Is.EqualTo(first.CompletedCount));
            Assert.That(replay.CompletionRate,
                Is.EqualTo(first.CompletionRate));
            Assert.That(replay.MedianCompletionTime,
                Is.EqualTo(first.MedianCompletionTime));
            Assert.That(replay.AverageRequiredTaps,
                Is.EqualTo(first.AverageRequiredTaps));
        }

        private static ExperimentSession CreatePlaying(
            MechanicExperimentType mechanic,
            bool shield)
        {
            ExperimentSession session = new ExperimentSession(
                ExperimentCatalog.Get(4, mechanic),
                new StartItemSelection(shield, false));
            Assert.That(session.CompleteCountdown(), Is.True);
            return session;
        }

        private static ExperimentGatePlan PassUntilFirstSource(
            ExperimentSession session)
        {
            while (true)
            {
                ExperimentGatePlan plan = session.GetNextPlan();
                CycleTo(session, plan.Color);
                Assert.That(session.Resolve(plan), Is.True);
                if (plan.IsSource)
                {
                    return plan;
                }
            }
        }

        private static void PassNext(ExperimentSession session)
        {
            ExperimentGatePlan plan = session.GetNextPlan();
            CycleTo(session, plan.Color);
            Assert.That(session.Resolve(plan), Is.True);
        }

        private static void CycleTo(
            ExperimentSession session,
            RunnerColor color)
        {
            int safety = session.Definition.ColorCount;
            while (session.CurrentColor != color && safety-- > 0)
            {
                Assert.That(session.TryCycleColor(), Is.True);
            }
            Assert.That(session.CurrentColor, Is.EqualTo(color));
        }

        private static ExperimentGatePlan[] Generate(
            ExperimentDefinition definition)
        {
            DeterministicExperimentGateSequence sequence =
                new DeterministicExperimentGateSequence(definition);
            ExperimentGatePlan[] result =
                new ExperimentGatePlan[definition.GateCount];
            for (int index = 0; index < result.Length; index++)
            {
                result[index] = sequence.GetPlan(index);
            }
            return result;
        }

        private static List<ExperimentGatePlan> FindByRole(
            ExperimentGatePlan[] plans,
            ExperimentGateRole role)
        {
            List<ExperimentGatePlan> result =
                new List<ExperimentGatePlan>();
            for (int index = 0; index < plans.Length; index++)
            {
                if (plans[index].Role == role)
                {
                    result.Add(plans[index]);
                }
            }
            return result;
        }

        private static void AssertSourceClonePair(
            ExperimentGatePlan source,
            ExperimentGatePlan clone,
            int expectedSourceGateId,
            int expectedCloneGateId)
        {
            Assert.That(source.GateId, Is.EqualTo(expectedSourceGateId));
            Assert.That(clone.GateId, Is.EqualTo(expectedCloneGateId));
            Assert.That(
                clone.GateIndex,
                Is.EqualTo(source.GateIndex + 1));
            Assert.That(clone.SourceGateId, Is.EqualTo(source.GateId));
            Assert.That(clone.IsSource, Is.False);
        }

        private static void AssertPlanEqual(
            ExperimentGatePlan expected,
            ExperimentGatePlan actual)
        {
            Assert.That(actual.GateId, Is.EqualTo(expected.GateId));
            Assert.That(actual.GateIndex, Is.EqualTo(expected.GateIndex));
            Assert.That(
                actual.NonCloneGateIndex,
                Is.EqualTo(expected.NonCloneGateIndex));
            Assert.That(actual.Role, Is.EqualTo(expected.Role));
            Assert.That(
                actual.SourceGateId,
                Is.EqualTo(expected.SourceGateId));
            Assert.That(actual.Color, Is.EqualTo(expected.Color));
            Assert.That(
                actual.RequiredTapCount,
                Is.EqualTo(expected.RequiredTapCount));
            Assert.That(
                actual.Spacing,
                Is.EqualTo(expected.Spacing).Within(0.0001f));
            Assert.That(
                actual.Cadence,
                Is.EqualTo(expected.Cadence).Within(0.0001f));
        }
    }
}
