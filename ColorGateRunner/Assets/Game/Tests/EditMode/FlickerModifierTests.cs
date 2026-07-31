using System;
using System.Collections.Generic;
using ColorGateRunner.Core;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class FlickerModifierTests
    {
        [Test]
        public void FlickerSettings_DefaultsMatchApprovedContract()
        {
            FlickerSettings settings = FlickerSettings.CreateDefault();

            Assert.That(settings.Enabled, Is.True);
            Assert.That(settings.EligibleStartProgress, Is.EqualTo(0.15f));
            Assert.That(settings.EligibleEndProgress, Is.EqualTo(0.85f));
            Assert.That(settings.OccurrenceChance, Is.EqualTo(0.35f));
            Assert.That(settings.MinimumGateCooldown, Is.EqualTo(2));
            Assert.That(settings.RevealDurationSeconds, Is.EqualTo(1f));
            Assert.That(settings.HideLeadTimeSeconds, Is.EqualTo(0.65f));
            Assert.That(settings.TransitionSeconds, Is.EqualTo(0.12f));
            Assert.That(settings.MaxOccurrences, Is.EqualTo(4));
            Assert.That(settings.FirstOccurrenceGuaranteed, Is.True);
        }

        [Test]
        public void FlickerSettings_RejectInvalidValues()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateSettings(start: -0.01f));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateSettings(end: 1.01f));
            Assert.Throws<ArgumentException>(
                () => CreateSettings(start: 0.5f, end: 0.5f));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateSettings(chance: 1.01f));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateSettings(cooldown: -1));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateSettings(reveal: -0.01f));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateSettings(lead: -0.01f));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateSettings(transition: -0.01f));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateSettings(maximum: -1));
        }

        [Test]
        public void FlickerSelection_IsDeterministicAndRespectsBoundsCooldownAndMaximum()
        {
            ExperimentDefinition definition = ExperimentCatalog.Get(
                4,
                MechanicExperimentType.Flicker,
                98765u);
            List<int> first = GetFlickerGateIds(definition);
            List<int> second = GetFlickerGateIds(definition);

            Assert.That(first, Is.EqualTo(second));
            Assert.That(first.Count, Is.InRange(1, 4));
            for (int index = 0; index < first.Count; index++)
            {
                float progress = first[index] /
                    (float)(definition.GateCount - 1);
                Assert.That(
                    progress,
                    Is.InRange(
                        definition.Flicker.EligibleStartProgress,
                        definition.Flicker.EligibleEndProgress));
                if (index > 0)
                {
                    Assert.That(
                        first[index] - first[index - 1],
                        Is.GreaterThan(
                            definition.Flicker.MinimumGateCooldown));
                }
            }
        }

        [Test]
        public void FirstOccurrenceGuaranteed_SelectsOneWhenChanceIsZero()
        {
            FlickerSettings settings = CreateSettings(
                chance: 0f,
                maximum: 4,
                guaranteed: true);
            ExperimentDefinition definition = ExperimentCatalog.Get(
                4,
                MechanicExperimentType.Flicker,
                5u,
                settings);

            Assert.That(GetFlickerGateIds(definition).Count, Is.EqualTo(1));
        }

        [Test]
        public void NoEligibleGate_IsHandledWithoutFallbackSelection()
        {
            ExperimentDefinition definition = new ExperimentDefinition(
                4,
                MechanicExperimentType.Flicker,
                5u,
                gateCount: 1,
                flickerSettings: FlickerSettings.CreateDefault());

            Assert.That(GetFlickerGateIds(definition), Is.Empty);
        }

        [Test]
        public void FlickerSelection_DoesNotIncreaseGateCountOrMixModifiers()
        {
            ExperimentDefinition definition = ExperimentCatalog.Get(
                4,
                MechanicExperimentType.Flicker);
            DeterministicExperimentGateSequence sequence =
                new DeterministicExperimentGateSequence(definition);
            int flickerCount = 0;

            for (int index = 0; index < definition.GateCount; index++)
            {
                ExperimentGatePlan plan = sequence.GetPlan(index);
                if (plan.IsFlicker)
                {
                    flickerCount++;
                    Assert.That(
                        plan.Modifier.Types,
                        Is.EqualTo(GateModifierType.Flicker));
                }
            }

            Assert.That(sequence.Cursor, Is.EqualTo(definition.GateCount));
            Assert.That(flickerCount, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void FlickerVisibility_RequiresObservationAndEtaThenNeverReappears()
        {
            FlickerSettings settings = FlickerSettings.CreateDefault();
            FlickerVisibilityState state = new FlickerVisibilityState();

            state.Advance(0.99f, 0.5f, settings);
            Assert.That(state.HideStarted, Is.False);

            state.Advance(0.01f, 0.66f, settings);
            Assert.That(state.HideStarted, Is.False);

            state.Advance(0f, 0.65f, settings);
            Assert.That(state.HideStarted, Is.True);
            Assert.That(state.HideStartCount, Is.EqualTo(1));

            state.Advance(settings.TransitionSeconds, 10f, settings);
            Assert.That(state.TransitionProgress, Is.EqualTo(1f));
            Assert.That(state.TargetAlpha, Is.Zero);

            state.Advance(5f, float.PositiveInfinity, settings);
            Assert.That(state.HideStarted, Is.True);
            Assert.That(state.HideStartCount, Is.EqualTo(1));
            Assert.That(state.TargetAlpha, Is.Zero);
        }

        [Test]
        public void FlickerVisibility_ResetClearsRuntimeState()
        {
            FlickerSettings settings = new FlickerSettings(
                true,
                0f,
                0.9f,
                1f,
                0,
                0f,
                1f,
                0f,
                1,
                true);
            FlickerVisibilityState state = new FlickerVisibilityState();
            state.Advance(0f, 1f, settings);
            Assert.That(state.HideStarted, Is.True);

            state.Reset();

            Assert.That(state.VisibleElapsed, Is.Zero);
            Assert.That(state.HideStarted, Is.False);
            Assert.That(state.TransitionProgress, Is.Zero);
            Assert.That(state.HideStartCount, Is.Zero);
        }

        [Test]
        public void FlickerPlan_UsesOrdinaryPlayerShieldAndFailurePriority()
        {
            ExperimentSession shielded = CreatePlayingFlicker(shield: true);
            ExperimentGatePlan shieldPlan = CreateCurrentFlickerPlan(
                shielded,
                NextColor(shielded, shielded.CurrentColor));

            Assert.That(shielded.Resolve(shieldPlan), Is.True);
            Assert.That(
                shielded.LastResolution,
                Is.EqualTo(ExperimentGateResolution.ShieldDefense));
            Assert.That(shielded.ShieldActive, Is.False);

            ExperimentSession failed = CreatePlayingFlicker(shield: false);
            ExperimentGatePlan failPlan = CreateCurrentFlickerPlan(
                failed,
                NextColor(failed, failed.CurrentColor));

            Assert.That(failed.Resolve(failPlan), Is.False);
            Assert.That(
                failed.LastResolution,
                Is.EqualTo(ExperimentGateResolution.Failure));
            Assert.That(
                failed.LastFailureCause,
                Is.EqualTo(ExperimentRuntimeFailureCause.StandardGateMiss));
        }

        [Test]
        public void HeldEcho_ResolvesFlickerWithoutConsumingShield()
        {
            ExperimentSession session = new ExperimentSession(
                ExperimentCatalog.Get(4, MechanicExperimentType.Echo),
                new StartItemSelection(true, false));
            session.CompleteCountdown();
            ExperimentGatePlan provider = AdvanceToProvider(session);
            MatchColor(session, provider.Color);
            Assert.That(session.Resolve(provider), Is.True);
            RunnerColor echoColor = session.EchoColor;
            if (session.CurrentColor == echoColor)
            {
                session.TryCycleColor();
            }
            ExperimentGatePlan flicker = new ExperimentGatePlan(
                session.GatesPassed,
                echoColor,
                0,
                session.CurrentSpeed,
                1f,
                session.CurrentSpeed,
                MechanicExperimentType.Flicker,
                new GateModifier(GateModifierType.Flicker));

            Assert.That(session.Resolve(flicker), Is.True);
            Assert.That(
                session.LastResolution,
                Is.EqualTo(ExperimentGateResolution.EchoColorMatch));
            Assert.That(session.EchoActive, Is.False);
            Assert.That(session.ShieldActive, Is.True);
        }

        private static FlickerSettings CreateSettings(
            bool enabled = true,
            float start = 0.15f,
            float end = 0.85f,
            float chance = 0.35f,
            int cooldown = 2,
            float reveal = 1f,
            float lead = 0.65f,
            float transition = 0.12f,
            int maximum = 4,
            bool guaranteed = true)
        {
            return new FlickerSettings(
                enabled,
                start,
                end,
                chance,
                cooldown,
                reveal,
                lead,
                transition,
                maximum,
                guaranteed);
        }

        private static List<int> GetFlickerGateIds(
            ExperimentDefinition definition)
        {
            List<int> result = new List<int>();
            DeterministicExperimentGateSequence sequence =
                new DeterministicExperimentGateSequence(definition);
            for (int index = 0; index < definition.GateCount; index++)
            {
                ExperimentGatePlan plan = sequence.GetPlan(index);
                if (plan.IsFlicker)
                {
                    result.Add(plan.GateId);
                }
            }
            return result;
        }

        private static ExperimentSession CreatePlayingFlicker(bool shield)
        {
            ExperimentSession session = new ExperimentSession(
                ExperimentCatalog.Get(4, MechanicExperimentType.Flicker),
                new StartItemSelection(shield, false));
            session.CompleteCountdown();
            return session;
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

        private static ExperimentGatePlan CreateCurrentFlickerPlan(
            ExperimentSession session,
            RunnerColor color)
        {
            return new ExperimentGatePlan(
                session.GatesPassed,
                color,
                0,
                session.CurrentSpeed,
                1f,
                session.CurrentSpeed,
                MechanicExperimentType.Flicker,
                new GateModifier(GateModifierType.Flicker));
        }

        private static RunnerColor NextColor(
            ExperimentSession session,
            RunnerColor current)
        {
            for (int index = 0; index < session.Definition.ColorCount; index++)
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
