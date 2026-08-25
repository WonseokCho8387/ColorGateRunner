using System.Collections.Generic;
using ColorGateRunner.Core;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class Iteration3EchoTests
    {
        [Test]
        public void EchoExperiment_DoesNotIncreaseGateCount()
        {
            ExperimentDefinition definition =
                ExperimentCatalog.Get(4, MechanicExperimentType.Echo);
            Assert.That(definition.GateCount, Is.EqualTo(40));

            DeterministicExperimentGateSequence sequence =
                new DeterministicExperimentGateSequence(definition);
            for (int index = 0; index < definition.GateCount; index++)
            {
                ExperimentGatePlan plan = sequence.GetPlan(index);
                Assert.That(plan.GateId, Is.EqualTo(index));
                Assert.That(plan.GenerationOrder, Is.EqualTo(index));
            }
            Assert.That(sequence.Cursor, Is.EqualTo(40));
        }

        [Test]
        public void EchoExperiment_FirstProviderIsDeterministic()
        {
            ExperimentSession first = CreatePlayingEcho(false);
            ExperimentSession second = CreatePlayingEcho(false);
            List<int> firstOffers = GenerateProviderIds(first, 10);
            List<int> secondOffers = GenerateProviderIds(second, 10);

            Assert.That(firstOffers.Count, Is.EqualTo(1));
            Assert.That(secondOffers, Is.EqualTo(firstOffers));
            Assert.That(firstOffers[0] / 39f, Is.InRange(0.1f, 0.2f));
        }

        [Test]
        public void ShieldedAttempt_HasNoEchoProviderForEntireRun()
        {
            ExperimentSession session = CreatePlayingEcho(true);
            for (int index = 0; index < session.Definition.GateCount; index++)
            {
                ExperimentGatePlan plan = session.GetPlan(index);
                Assert.That(plan.IsEchoProvider, Is.False);
                MatchColor(session, plan.Color);
                Assert.That(session.Resolve(plan), Is.True);
            }

            Assert.That(session.EchoActive, Is.False);
            Assert.That(session.EchoAcquisitionCount, Is.Zero);
        }

        [Test]
        public void ProviderAcquisition_StoresEffectiveColor()
        {
            ExperimentSession session = CreatePlayingEcho(false);
            ExperimentGatePlan provider = AdvanceToProvider(session);
            MatchColor(session, provider.Color);

            Assert.That(session.Resolve(provider), Is.True);
            Assert.That(session.EchoActive, Is.True);
            Assert.That(session.EchoColor, Is.EqualTo(provider.Color));
            Assert.That(session.EchoAcquisitionCount, Is.EqualTo(1));
        }

        [Test]
        public void ResolutionPriority_PlayerThenEcho()
        {
            ExperimentSession session = CreatePlayingEcho(false);
            ExperimentGatePlan provider = AdvanceToProvider(session);
            MatchColor(session, provider.Color);
            Assert.That(session.Resolve(provider), Is.True);
            RunnerColor stored = session.EchoColor;
            RunnerColor player = session.CurrentColor;

            ExperimentGatePlan playerMatch = CreatePlan(
                session.GatesPassed,
                player);
            Assert.That(session.Resolve(playerMatch), Is.True);
            Assert.That(
                session.LastResolution,
                Is.EqualTo(ExperimentGateResolution.PlayerColorMatch));
            Assert.That(session.EchoActive, Is.True);
            Assert.That(session.ShieldActive, Is.False);

            SetDifferentColor(session, stored);
            ExperimentGatePlan echoMatch = CreatePlan(
                session.GatesPassed,
                stored);
            Assert.That(session.Resolve(echoMatch), Is.True);
            Assert.That(
                session.LastResolution,
                Is.EqualTo(ExperimentGateResolution.EchoColorMatch));
            Assert.That(session.EchoActive, Is.False);
            Assert.That(session.ShieldActive, Is.False);
        }

        [Test]
        public void EchoUse_DoesNotIncreaseCompletionRequirement()
        {
            ExperimentSession session = CreatePlayingEcho(false);
            for (int index = 0; index < session.Definition.GateCount; index++)
            {
                ExperimentGatePlan plan = session.GetPlan(index);
                MatchColor(session, plan.Color);
                Assert.That(session.Resolve(plan), Is.True);
            }

            Assert.That(session.GatesPassed, Is.EqualTo(40));
            Assert.That(
                session.FlowState,
                Is.EqualTo(StageFlowState.StageCleared));
        }

        [Test]
        public void Retry_ClearsEchoStateAndReproducesProvider()
        {
            ExperimentSession session = CreatePlayingEcho(false);
            ExperimentGatePlan provider = AdvanceToProvider(session);
            int providerId = provider.GateId;
            MatchColor(session, provider.Color);
            session.Resolve(provider);
            Assert.That(session.EchoActive, Is.True);

            session.Restart();
            session.CompleteCountdown();
            ExperimentGatePlan replayProvider = AdvanceToProvider(session);

            Assert.That(session.EchoActive, Is.False);
            Assert.That(session.EchoAcquisitionCount, Is.Zero);
            Assert.That(replayProvider.GateId, Is.EqualTo(providerId));
        }

        [Test]
        public void EchoProviderCanAlsoCarryCamouflageWithoutColorLeak()
        {
            GateModifier modifier =
                new GateModifier(GateModifierType.Camouflage)
                    .With(GateModifierType.EchoProvider);
            ExperimentGatePlan plan = new ExperimentGatePlan(
                4,
                RunnerColor.Green,
                1,
                18f,
                1.2f,
                21.6f,
                MechanicExperimentType.Echo,
                modifier);

            Assert.That(plan.IsCamouflage, Is.True);
            Assert.That(plan.IsEchoProvider, Is.True);
            Assert.That(plan.Color, Is.EqualTo(RunnerColor.Green));
        }

        private static ExperimentSession CreatePlayingEcho(bool shield)
        {
            ExperimentSession session = new ExperimentSession(
                ExperimentCatalog.Get(4, MechanicExperimentType.Echo),
                new StartItemSelection(shield, false));
            session.CompleteCountdown();
            return session;
        }

        private static List<int> GenerateProviderIds(
            ExperimentSession session,
            int count)
        {
            List<int> result = new List<int>();
            for (int index = 0; index < count; index++)
            {
                ExperimentGatePlan plan = session.GetPlan(index);
                if (plan.IsEchoProvider)
                {
                    result.Add(plan.GateId);
                }
            }
            return result;
        }

        private static ExperimentGatePlan AdvanceToProvider(
            ExperimentSession session)
        {
            while (session.GatesPassed < session.Definition.GateCount)
            {
                ExperimentGatePlan plan =
                    session.GetPlan(session.GatesPassed);
                if (plan.IsEchoProvider)
                {
                    return plan;
                }
                MatchColor(session, plan.Color);
                Assert.That(session.Resolve(plan), Is.True);
            }
            Assert.Fail("No Echo provider was generated.");
            return default;
        }

        private static ExperimentGatePlan CreatePlan(
            int gateIndex,
            RunnerColor color)
        {
            return new ExperimentGatePlan(
                gateIndex,
                color,
                0,
                18f,
                1f,
                18f,
                MechanicExperimentType.Echo,
                GateModifier.None);
        }

        private static void MatchColor(
            ExperimentSession session,
            RunnerColor target)
        {
            int safety = session.Definition.ColorCount;
            while (session.CurrentColor != target && safety-- > 0)
            {
                session.TryCycleColor();
            }
            Assert.That(session.CurrentColor, Is.EqualTo(target));
        }

        private static void SetDifferentColor(
            ExperimentSession session,
            RunnerColor target)
        {
            if (session.CurrentColor == target)
            {
                session.TryCycleColor();
            }
            Assert.That(session.CurrentColor, Is.Not.EqualTo(target));
        }

        private static RunnerColor NextColor(
            ExperimentSession session,
            RunnerColor current)
        {
            for (int index = 0;
                index < session.Definition.ColorCount;
                index++)
            {
                RunnerColor color = session.Definition.GetColor(index);
                if (color != current)
                {
                    return color;
                }
            }
            return current;
        }
    }
}
