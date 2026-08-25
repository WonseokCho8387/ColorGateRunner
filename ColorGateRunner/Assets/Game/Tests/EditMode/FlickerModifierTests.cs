using System;
using System.Collections.Generic;
using System.Reflection;
using ColorGateRunner.Core;
using ColorGateRunner.Presentation;
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
            Assert.That(settings.OccurrenceChance, Is.EqualTo(0.30f));
            Assert.That(settings.MinimumGateCooldown, Is.EqualTo(2));
            Assert.That(settings.MaxOccurrences, Is.EqualTo(4));
            Assert.That(settings.FirstOccurrenceGuaranteed, Is.True);
            Assert.That(settings.SwitchIntervalSeconds, Is.EqualTo(0.50f));
            Assert.That(settings.TransitionPulseSeconds, Is.EqualTo(0.10f));
            Assert.That(settings.MinimumCyclesVisible, Is.EqualTo(3));
            Assert.That(settings.RandomizePhaseOffset, Is.True);
        }

        [Test]
        public void FlickerLauncher_HasNoAuthoredCycleCountSetting()
        {
            FieldInfo field = typeof(ExperimentLauncher).GetField(
                "colorCycleFlickerCycleColorCount",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(field, Is.Null);
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
                () => CreateSettings(maximum: -1));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateSettings(interval: 0f));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateSettings(pulse: -0.01f));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateSettings(minimumCycles: 0));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateSettings().ValidateForActiveColorCount(1));
        }

        [Test]
        public void FlickerPlanning_IsDeterministicAndRespectsSelectionRules()
        {
            ExperimentDefinition definition = ExperimentCatalog.Get(
                4,
                MechanicExperimentType.Flicker,
                98765u,
                flickerSettings: FlickerSettings.CreateDefault());
            List<ExperimentGatePlan> first = GetFlickerPlans(definition);
            List<ExperimentGatePlan> second = GetFlickerPlans(definition);

            Assert.That(first.Count, Is.InRange(1, 4));
            Assert.That(second.Count, Is.EqualTo(first.Count));
            for (int index = 0; index < first.Count; index++)
            {
                ExperimentGatePlan a = first[index];
                ExperimentGatePlan b = second[index];
                float progress = a.GateIndex /
                    (float)(definition.GateCount - 1);
                Assert.That(a.GateId, Is.EqualTo(b.GateId));
                Assert.That(
                    a.CopyCycleColors(),
                    Is.EqualTo(b.CopyCycleColors()));
                Assert.That(
                    a.PhaseOffsetSeconds,
                    Is.EqualTo(b.PhaseOffsetSeconds));
                Assert.That(
                    progress,
                    Is.InRange(
                        definition.Flicker.EligibleStartProgress,
                        definition.Flicker.EligibleEndProgress));
                Assert.That(
                    a.Modifier.Types,
                    Is.EqualTo(GateModifierType.Flicker));
                if (index > 0)
                {
                    Assert.That(
                        a.GateId - first[index - 1].GateId,
                        Is.GreaterThan(
                            definition.Flicker.MinimumGateCooldown));
                }
            }
        }

        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        public void FlickerPlanning_UsesFullPaletteInPlayerInputOrder(
            int colorCount)
        {
            ExperimentDefinition definition = ExperimentCatalog.Get(
                colorCount,
                MechanicExperimentType.Flicker,
                12345u,
                flickerSettings: CreateSettings());

            foreach (ExperimentGatePlan plan in GetFlickerPlans(definition))
            {
                Assert.That(
                    plan.CycleColorCount,
                    Is.EqualTo(definition.ColorCount));
                Assert.That(plan.GetCycleColor(0), Is.EqualTo(plan.Color));
                HashSet<RunnerColor> unique = new HashSet<RunnerColor>(
                    plan.CopyCycleColors());
                Assert.That(unique.Count, Is.EqualTo(definition.ColorCount));
                foreach (RunnerColor color in unique)
                {
                    Assert.That(
                        (int)color,
                        Is.InRange(0, definition.ColorCount - 1));
                }
                for (int index = 0;
                    index < plan.CycleColorCount;
                    index++)
                {
                    RunnerColor current = plan.GetCycleColor(index);
                    RunnerColor next = plan.GetCycleColor(
                        (index + 1) % plan.CycleColorCount);
                    Assert.That(
                        next,
                        Is.EqualTo(definition.GetNextColor(current)));
                }
            }
        }

        [Test]
        public void ExperimentPlayerInput_UsesTheSamePaletteOrder()
        {
            ExperimentSession session = new ExperimentSession(
                ExperimentCatalog.Get(6, MechanicExperimentType.Flicker),
                new StartItemSelection(false, false));
            session.CompleteCountdown();

            for (int index = 0;
                index < session.Definition.ColorCount;
                index++)
            {
                RunnerColor current = session.CurrentColor;
                RunnerColor expected =
                    session.Definition.GetNextColor(current);

                Assert.That(session.TryCycleColor(), Is.True);
                Assert.That(session.CurrentColor, Is.EqualTo(expected));
            }

            Assert.That(
                session.CurrentColor,
                Is.EqualTo(session.Definition.GetColor(0)));
        }

        [Test]
        public void FlickerPlanning_FirstOccurrenceGuaranteeAndNoFallbackAreExplicit()
        {
            ExperimentDefinition guaranteed = ExperimentCatalog.Get(
                4,
                MechanicExperimentType.Flicker,
                5u,
                flickerSettings: CreateSettings(
                    chance: 0f,
                    maximum: 1,
                    guaranteed: true));
            ExperimentDefinition insufficientExposure =
                ExperimentCatalog.Get(
                    4,
                    MechanicExperimentType.Flicker,
                    5u,
                    flickerSettings: CreateSettings(
                        chance: 1f,
                        interval: 10f,
                        minimumCycles: 3));

            Assert.That(GetFlickerPlans(guaranteed).Count, Is.EqualTo(1));
            Assert.That(GetFlickerPlans(insufficientExposure), Is.Empty);
        }

        [Test]
        public void FlickerPlanning_DoesNotIncreaseGateCountOrMixModifiers()
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
        public void FlickerCycle_TwoColorsUseExactBoundaryAndRepeat()
        {
            ExperimentGatePlan plan = CreatePlan(
                RunnerColor.Red,
                new[] { RunnerColor.Red, RunnerColor.Blue },
                0.5f,
                0f,
                0.1f);

            Assert.That(
                plan.GetJudgmentColor(0f),
                Is.EqualTo(RunnerColor.Red));
            Assert.That(
                plan.GetJudgmentColor(0.4999f),
                Is.EqualTo(RunnerColor.Red));
            Assert.That(
                plan.GetJudgmentColor(0.5f),
                Is.EqualTo(RunnerColor.Blue));
            Assert.That(
                plan.GetJudgmentColor(1f),
                Is.EqualTo(RunnerColor.Red));
            Assert.That(
                plan.GetJudgmentColor(2.5f),
                Is.EqualTo(RunnerColor.Blue));
        }

        [Test]
        public void FlickerCycle_ThreeColorsUseEveryColorInOrder()
        {
            ExperimentGatePlan plan = CreatePlan(
                RunnerColor.Red,
                new[]
                {
                    RunnerColor.Red,
                    RunnerColor.Blue,
                    RunnerColor.Green
                },
                0.5f,
                0f,
                0.1f);

            Assert.That(
                plan.GetJudgmentColor(0f),
                Is.EqualTo(RunnerColor.Red));
            Assert.That(
                plan.GetJudgmentColor(0.5f),
                Is.EqualTo(RunnerColor.Blue));
            Assert.That(
                plan.GetJudgmentColor(1f),
                Is.EqualTo(RunnerColor.Green));
            Assert.That(
                plan.GetJudgmentColor(1.5f),
                Is.EqualTo(RunnerColor.Red));
        }

        [Test]
        public void FlickerCycle_PhaseOffsetAndPulseAreDeterministic()
        {
            ExperimentDefinition definition = ExperimentCatalog.Get(
                4,
                MechanicExperimentType.Flicker,
                222u,
                flickerSettings: FlickerSettings.CreateDefault());
            ExperimentGatePlan first = GetFlickerPlans(definition)[0];
            ExperimentGatePlan replay = GetFlickerPlans(definition)[0];

            Assert.That(
                first.PhaseOffsetSeconds,
                Is.InRange(0f, first.SwitchIntervalSeconds));
            Assert.That(
                first.PhaseOffsetSeconds,
                Is.EqualTo(replay.PhaseOffsetSeconds));
            Assert.That(
                first.GetFlickerSample(1.25f).CycleColorIndex,
                Is.EqualTo(
                    replay.GetFlickerSample(1.25f).CycleColorIndex));
            Assert.That(
                CreatePlan(
                    RunnerColor.Red,
                    new[] { RunnerColor.Red, RunnerColor.Blue },
                    0.5f,
                    0f,
                    0.1f)
                    .GetFlickerSample(0.5f)
                    .TransitionPulse,
                Is.EqualTo(1f));
        }

        [Test]
        public void FlickerCycle_RandomizePhaseDisabledUsesZero()
        {
            ExperimentDefinition definition = ExperimentCatalog.Get(
                4,
                MechanicExperimentType.Flicker,
                222u,
                flickerSettings: CreateSettings(randomizePhase: false));

            foreach (ExperimentGatePlan plan in GetFlickerPlans(definition))
            {
                Assert.That(plan.PhaseOffsetSeconds, Is.Zero);
            }
        }

        [Test]
        public void FlickerJudgment_UsesCollisionTimeColorInsteadOfBaseColor()
        {
            ExperimentSession session = CreatePlayingSession(shield: false);
            ExperimentGatePlan plan = CreatePlanAtCurrentGate(
                session,
                RunnerColor.Red,
                new[] { RunnerColor.Red, RunnerColor.Blue });
            MatchColor(session, RunnerColor.Blue);

            Assert.That(session.Resolve(plan, 0.5f), Is.True);
            Assert.That(
                session.LastJudgmentColor,
                Is.EqualTo(RunnerColor.Blue));
            Assert.That(session.LastJudgedGateWasFlicker, Is.True);
            Assert.That(
                session.LastResolution,
                Is.EqualTo(ExperimentGateResolution.PlayerColorMatch));
        }

        [Test]
        public void FlickerJudgment_UsesStoredEchoColor()
        {
            ExperimentSession session = AcquireEcho();
            RunnerColor echoColor = session.EchoColor;
            RunnerColor other = GetDifferentColor(session, echoColor);
            ExperimentGatePlan plan = CreatePlanAtCurrentGate(
                session,
                echoColor,
                new[] { echoColor, other });
            MatchColor(session, other);

            Assert.That(session.Resolve(plan, 0f), Is.True);
            Assert.That(
                session.LastResolution,
                Is.EqualTo(ExperimentGateResolution.EchoColorMatch));
            Assert.That(session.EchoActive, Is.False);
            Assert.That(session.ShieldActive, Is.False);
        }

        [Test]
        public void FlickerJudgment_UsesShieldThenOrdinaryFailure()
        {
            ExperimentSession shielded = CreatePlayingSession(shield: true);
            RunnerColor shieldTarget =
                GetDifferentColor(shielded, shielded.CurrentColor);
            ExperimentGatePlan shieldPlan = CreatePlanAtCurrentGate(
                shielded,
                shieldTarget,
                new[] { shieldTarget, shielded.CurrentColor });

            Assert.That(shielded.Resolve(shieldPlan, 0f), Is.True);
            Assert.That(
                shielded.LastResolution,
                Is.EqualTo(ExperimentGateResolution.ShieldDefense));
            Assert.That(shielded.ShieldActive, Is.False);

            ExperimentSession failed = CreatePlayingSession(shield: false);
            RunnerColor failTarget =
                GetDifferentColor(failed, failed.CurrentColor);
            ExperimentGatePlan failPlan = CreatePlanAtCurrentGate(
                failed,
                failTarget,
                new[] { failTarget, failed.CurrentColor });

            Assert.That(failed.Resolve(failPlan, 0f), Is.False);
            Assert.That(
                failed.LastResolution,
                Is.EqualTo(ExperimentGateResolution.Failure));
            Assert.That(
                failed.LastFailureCause,
                Is.EqualTo(ExperimentRuntimeFailureCause.StandardGateMiss));
        }

        [Test]
        public void FlickerRetry_ResetsGameplayTimeAndReplaysPlans()
        {
            ExperimentSession session = CreatePlayingSession(shield: false);
            ExperimentGatePlan first = session.GetPlan(0);
            session.Advance(1.25f);

            session.Restart();
            ExperimentGatePlan replay = session.GetPlan(0);

            Assert.That(session.ElapsedPlayingSeconds, Is.Zero);
            Assert.That(replay.GateId, Is.EqualTo(first.GateId));
            Assert.That(
                replay.CopyCycleColors(),
                Is.EqualTo(first.CopyCycleColors()));
            Assert.That(
                replay.PhaseOffsetSeconds,
                Is.EqualTo(first.PhaseOffsetSeconds));
            Assert.That(
                replay.GetJudgmentColor(0f),
                Is.EqualTo(first.GetJudgmentColor(0f)));
        }

        [Test]
        public void FlickerSimulation_UsesCollisionTimeCycleCalculation()
        {
            ExperimentDefinition definition = ExperimentCatalog.Get(
                4,
                MechanicExperimentType.Flicker,
                12345u,
                flickerSettings: CreateSettings(randomizePhase: false));
            ExperimentSimulationResult result =
                ExperimentSimulationRunner.Run(
                    definition,
                    SimulatedPlayerProfile.Get(
                        SimulatedPlayerKind.Perfect),
                    1,
                    definition.Seed);

            Assert.That(result.CompletedCount, Is.EqualTo(1));
            Assert.That(result.RunCount, Is.EqualTo(1));
        }

        private static FlickerSettings CreateSettings(
            bool enabled = true,
            float start = 0.15f,
            float end = 0.85f,
            float chance = 0.30f,
            int cooldown = 2,
            int maximum = 4,
            bool guaranteed = true,
            float interval = 0.5f,
            float pulse = 0.1f,
            int minimumCycles = 3,
            bool randomizePhase = true)
        {
            return new FlickerSettings(
                enabled,
                start,
                end,
                chance,
                cooldown,
                maximum,
                guaranteed,
                interval,
                pulse,
                minimumCycles,
                randomizePhase);
        }

        private static List<ExperimentGatePlan> GetFlickerPlans(
            ExperimentDefinition definition)
        {
            List<ExperimentGatePlan> result =
                new List<ExperimentGatePlan>();
            DeterministicExperimentGateSequence sequence =
                new DeterministicExperimentGateSequence(definition);
            for (int index = 0; index < definition.GateCount; index++)
            {
                ExperimentGatePlan plan = sequence.GetPlan(index);
                if (plan.IsFlicker)
                {
                    result.Add(plan);
                }
            }
            return result;
        }

        private static ExperimentGatePlan CreatePlan(
            RunnerColor baseColor,
            RunnerColor[] colors,
            float interval,
            float phaseOffset,
            float pulse)
        {
            return new ExperimentGatePlan(
                0,
                baseColor,
                0,
                16f,
                1f,
                16f,
                MechanicExperimentType.Flicker,
                new GateModifier(GateModifierType.Flicker),
                colors,
                interval,
                phaseOffset,
                pulse,
                12345u);
        }

        private static ExperimentSession CreatePlayingSession(bool shield)
        {
            ExperimentSession session = new ExperimentSession(
                ExperimentCatalog.Get(
                    4,
                    MechanicExperimentType.Flicker,
                    flickerSettings: CreateSettings(
                        randomizePhase: false)),
                new StartItemSelection(shield, false));
            session.CompleteCountdown();
            return session;
        }

        private static ExperimentGatePlan CreatePlanAtCurrentGate(
            ExperimentSession session,
            RunnerColor baseColor,
            RunnerColor[] colors)
        {
            return new ExperimentGatePlan(
                session.GatesPassed,
                baseColor,
                0,
                session.CurrentSpeed,
                1f,
                session.CurrentSpeed,
                MechanicExperimentType.Flicker,
                new GateModifier(GateModifierType.Flicker),
                colors,
                0.5f,
                0f,
                0.1f,
                session.Definition.Seed);
        }

        private static ExperimentSession AcquireEcho()
        {
            ExperimentSession session = new ExperimentSession(
                ExperimentCatalog.Get(4, MechanicExperimentType.Echo),
                new StartItemSelection(false, false));
            session.CompleteCountdown();
            while (session.GatesPassed < session.Definition.GateCount)
            {
                ExperimentGatePlan plan =
                    session.GetPlan(session.GatesPassed);
                MatchColor(session, plan.Color);
                Assert.That(session.Resolve(plan), Is.True);
                if (session.EchoActive)
                {
                    return session;
                }
            }
            Assert.Fail("No Echo provider was acquired.");
            return null;
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

        private static RunnerColor GetDifferentColor(
            ExperimentSession session,
            RunnerColor color)
        {
            for (int index = 0;
                index < session.Definition.ColorCount;
                index++)
            {
                RunnerColor candidate = session.Definition.GetColor(index);
                if (candidate != color)
                {
                    return candidate;
                }
            }
            throw new InvalidOperationException(
                "A different active color was required.");
        }
    }
}
