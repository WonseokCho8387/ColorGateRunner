using System;
using System.Collections.Generic;
using System.Reflection;
using ColorGateRunner.Core;
using ColorGateRunner.Presentation;
using NUnit.Framework;
using UnityEngine.Serialization;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class HiddenModifierTests
    {
        [Test]
        public void HiddenMigration_PreservesFormerFlickerEnumValues()
        {
            Assert.That((int)GateModifierType.Hidden, Is.EqualTo(1 << 4));
            Assert.That((int)MechanicExperimentType.Hidden, Is.EqualTo(5));
            Assert.That((int)GateModifierType.Flicker, Is.EqualTo(1 << 5));
            Assert.That((int)MechanicExperimentType.Flicker, Is.EqualTo(6));
            Assert.That(
                (GateModifierType)(1 << 4),
                Is.EqualTo(GateModifierType.Hidden));
            Assert.That(
                (MechanicExperimentType)5,
                Is.EqualTo(MechanicExperimentType.Hidden));
        }

        [Test]
        public void HiddenMigration_FormerSerializedNamesMapOnlyToHidden()
        {
            string[,] mappings =
            {
                { "hiddenEnabled", "flickerEnabled" },
                {
                    "hiddenEligibleStartProgress",
                    "flickerEligibleStartProgress"
                },
                {
                    "hiddenEligibleEndProgress",
                    "flickerEligibleEndProgress"
                },
                {
                    "hiddenOccurrenceChance",
                    "flickerOccurrenceChance"
                },
                {
                    "hiddenMinimumGateCooldown",
                    "flickerMinimumGateCooldown"
                },
                {
                    "hiddenRevealDurationSeconds",
                    "flickerRevealDurationSeconds"
                },
                {
                    "hiddenHideLeadTimeSeconds",
                    "flickerHideLeadTimeSeconds"
                },
                {
                    "hiddenTransitionSeconds",
                    "flickerTransitionSeconds"
                },
                {
                    "hiddenMaxOccurrences",
                    "flickerMaxOccurrences"
                },
                {
                    "hiddenFirstOccurrenceGuaranteed",
                    "flickerFirstOccurrenceGuaranteed"
                }
            };
            BindingFlags fields =
                BindingFlags.Instance | BindingFlags.NonPublic;

            for (int index = 0; index < mappings.GetLength(0); index++)
            {
                string currentName = mappings[index, 0];
                string formerName = mappings[index, 1];
                FieldInfo hiddenField = typeof(ExperimentLauncher).GetField(
                    currentName,
                    fields);
                Assert.That(hiddenField, Is.Not.Null, currentName);
                object[] attributes = hiddenField.GetCustomAttributes(
                    typeof(FormerlySerializedAsAttribute),
                    false);
                Assert.That(attributes.Length, Is.EqualTo(1), currentName);
                Assert.That(
                    ((FormerlySerializedAsAttribute)attributes[0]).oldName,
                    Is.EqualTo(formerName));
                Assert.That(
                    typeof(ExperimentLauncher).GetField(formerName, fields),
                    Is.Null,
                    formerName + " must not be reused by new Flicker.");
            }

            Assert.That(
                typeof(ExperimentLauncher).GetField(
                    "colorCycleFlickerOccurrenceChance",
                    fields),
                Is.Not.Null);
        }

        [TestCase(12345u, new int[] { 6, 11, 18, 23 })]
        [TestCase(98765u, new int[] { 6, 9, 13, 20 })]
        public void HiddenMigration_PreservesFormerFlickerSeedSelections(
            uint seed,
            int[] expectedGateIds)
        {
            ExperimentDefinition definition = ExperimentCatalog.Get(
                4,
                MechanicExperimentType.Hidden,
                seed);

            Assert.That(
                GetHiddenGateIds(definition),
                Is.EqualTo(expectedGateIds));
        }

        [Test]
        public void HiddenSettings_DefaultsMatchApprovedContract()
        {
            HiddenSettings settings = HiddenSettings.CreateDefault();

            Assert.That(settings.Enabled, Is.True);
            Assert.That(settings.EligibleStartProgress, Is.EqualTo(0.15f));
            Assert.That(settings.EligibleEndProgress, Is.EqualTo(0.85f));
            Assert.That(settings.OccurrenceChance, Is.EqualTo(0.35f));
            Assert.That(settings.MinimumGateCooldown, Is.EqualTo(2));
            Assert.That(settings.RevealDurationSeconds, Is.EqualTo(1f));
            Assert.That(settings.HideLeadTimeSeconds, Is.EqualTo(0.85f));
            Assert.That(settings.TransitionSeconds, Is.EqualTo(0.12f));
            Assert.That(settings.MaxOccurrences, Is.EqualTo(4));
            Assert.That(settings.FirstOccurrenceGuaranteed, Is.True);
        }

        [Test]
        public void HiddenSettings_RejectInvalidValues()
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
        public void HiddenSelection_IsDeterministicAndRespectsBoundsCooldownAndMaximum()
        {
            ExperimentDefinition definition = ExperimentCatalog.Get(
                4,
                MechanicExperimentType.Hidden,
                98765u);
            List<int> first = GetHiddenGateIds(definition);
            List<int> second = GetHiddenGateIds(definition);

            Assert.That(first, Is.EqualTo(second));
            Assert.That(first.Count, Is.InRange(1, 4));
            for (int index = 0; index < first.Count; index++)
            {
                float progress = first[index] /
                    (float)(definition.GateCount - 1);
                Assert.That(
                    progress,
                    Is.InRange(
                        definition.Hidden.EligibleStartProgress,
                        definition.Hidden.EligibleEndProgress));
                if (index > 0)
                {
                    Assert.That(
                        first[index] - first[index - 1],
                        Is.GreaterThan(
                            definition.Hidden.MinimumGateCooldown));
                }
            }
        }

        [Test]
        public void FirstOccurrenceGuaranteed_SelectsOneWhenChanceIsZero()
        {
            HiddenSettings settings = CreateSettings(
                chance: 0f,
                maximum: 4,
                guaranteed: true);
            ExperimentDefinition definition = ExperimentCatalog.Get(
                4,
                MechanicExperimentType.Hidden,
                5u,
                settings);

            Assert.That(GetHiddenGateIds(definition).Count, Is.EqualTo(1));
        }

        [Test]
        public void NoEligibleGate_IsHandledWithoutFallbackSelection()
        {
            ExperimentDefinition definition = new ExperimentDefinition(
                4,
                MechanicExperimentType.Hidden,
                5u,
                gateCount: 1,
                hiddenSettings: HiddenSettings.CreateDefault());

            Assert.That(GetHiddenGateIds(definition), Is.Empty);
        }

        [Test]
        public void HiddenSelection_DoesNotIncreaseGateCountOrMixModifiers()
        {
            ExperimentDefinition definition = ExperimentCatalog.Get(
                4,
                MechanicExperimentType.Hidden);
            DeterministicExperimentGateSequence sequence =
                new DeterministicExperimentGateSequence(definition);
            int hiddenCount = 0;

            for (int index = 0; index < definition.GateCount; index++)
            {
                ExperimentGatePlan plan = sequence.GetPlan(index);
                if (plan.IsHidden)
                {
                    hiddenCount++;
                    Assert.That(
                        plan.Modifier.Types,
                        Is.EqualTo(GateModifierType.Hidden));
                }
            }

            Assert.That(sequence.Cursor, Is.EqualTo(definition.GateCount));
            Assert.That(hiddenCount, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void HiddenVisibility_RequiresObservationAndEtaThenNeverReappears()
        {
            HiddenSettings settings = HiddenSettings.CreateDefault();
            HiddenVisibilityState state = new HiddenVisibilityState();

            state.Advance(0.99f, 0.5f, settings);
            Assert.That(state.HideStarted, Is.False);

            state.Advance(0.01f, 0.8501f, settings);
            Assert.That(state.HideStarted, Is.False);

            state.Advance(0f, 0.85f, settings);
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
        public void HiddenVisibility_ResetClearsRuntimeState()
        {
            HiddenSettings settings = new HiddenSettings(
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
            HiddenVisibilityState state = new HiddenVisibilityState();
            state.Advance(0f, 1f, settings);
            Assert.That(state.HideStarted, Is.True);

            state.Reset();

            Assert.That(state.VisibleElapsed, Is.Zero);
            Assert.That(state.HideStarted, Is.False);
            Assert.That(state.TransitionProgress, Is.Zero);
            Assert.That(state.HideStartCount, Is.Zero);
        }

        [Test]
        public void HiddenPlan_UsesOrdinaryPlayerShieldAndFailurePriority()
        {
            ExperimentSession shielded = CreatePlayingHidden(shield: true);
            ExperimentGatePlan shieldPlan = CreateCurrentHiddenPlan(
                shielded,
                NextColor(shielded, shielded.CurrentColor));

            Assert.That(shielded.Resolve(shieldPlan), Is.True);
            Assert.That(
                shielded.LastResolution,
                Is.EqualTo(ExperimentGateResolution.ShieldDefense));
            Assert.That(shielded.ShieldActive, Is.False);

            ExperimentSession failed = CreatePlayingHidden(shield: false);
            ExperimentGatePlan failPlan = CreateCurrentHiddenPlan(
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
        public void HeldEcho_ResolvesHiddenUsingStoredColor()
        {
            ExperimentSession session = new ExperimentSession(
                ExperimentCatalog.Get(4, MechanicExperimentType.Echo),
                new StartItemSelection(false, false));
            session.CompleteCountdown();
            ExperimentGatePlan provider = AdvanceToProvider(session);
            MatchColor(session, provider.Color);
            Assert.That(session.Resolve(provider), Is.True);
            RunnerColor echoColor = session.EchoColor;
            if (session.CurrentColor == echoColor)
            {
                session.TryCycleColor();
            }
            ExperimentGatePlan hidden = new ExperimentGatePlan(
                session.GatesPassed,
                echoColor,
                0,
                session.CurrentSpeed,
                1f,
                session.CurrentSpeed,
                MechanicExperimentType.Hidden,
                new GateModifier(GateModifierType.Hidden));

            Assert.That(session.Resolve(hidden), Is.True);
            Assert.That(
                session.LastResolution,
                Is.EqualTo(ExperimentGateResolution.EchoColorMatch));
            Assert.That(session.EchoActive, Is.False);
            Assert.That(session.ShieldActive, Is.False);
        }

        private static HiddenSettings CreateSettings(
            bool enabled = true,
            float start = 0.15f,
            float end = 0.85f,
            float chance = 0.35f,
            int cooldown = 2,
            float reveal = 1f,
            float lead = 0.85f,
            float transition = 0.12f,
            int maximum = 4,
            bool guaranteed = true)
        {
            return new HiddenSettings(
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

        private static List<int> GetHiddenGateIds(
            ExperimentDefinition definition)
        {
            List<int> result = new List<int>();
            DeterministicExperimentGateSequence sequence =
                new DeterministicExperimentGateSequence(definition);
            for (int index = 0; index < definition.GateCount; index++)
            {
                ExperimentGatePlan plan = sequence.GetPlan(index);
                if (plan.IsHidden)
                {
                    result.Add(plan.GateId);
                }
            }
            return result;
        }

        private static ExperimentSession CreatePlayingHidden(bool shield)
        {
            ExperimentSession session = new ExperimentSession(
                ExperimentCatalog.Get(4, MechanicExperimentType.Hidden),
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

        private static ExperimentGatePlan CreateCurrentHiddenPlan(
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
                MechanicExperimentType.Hidden,
                new GateModifier(GateModifierType.Hidden));
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
