using System.Collections.Generic;
using System.Text;
using ColorGateRunner.Core;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class GatePatternSequenceTests
    {
        [Test]
        public void PatternSequence_SameSeedReplaysIdentically()
        {
            var first = new DeterministicGatePatternSequence(GameRules.DefaultSeed);
            var second = new DeterministicGatePatternSequence(GameRules.DefaultSeed);

            for (int index = 0; index < 200; index++)
            {
                GatePlan a = first.GetNext(10f, 1f, 3, -1);
                GatePlan b = second.GetNext(10f, 1f, 3, -1);
                Assert.That(b.Pattern, Is.EqualTo(a.Pattern));
                Assert.That(b.IndexInPattern, Is.EqualTo(a.IndexInPattern));
                Assert.That(b.Color, Is.EqualTo(a.Color));
                Assert.That(b.Spacing, Is.EqualTo(a.Spacing));
            }
        }

        [Test]
        public void PatternSequence_DifferentSeedChangesPlans()
        {
            var first = new DeterministicGatePatternSequence(GameRules.DefaultSeed);
            var second = new DeterministicGatePatternSequence(GameRules.DefaultSeed + 1u);
            bool differs = false;

            for (int index = 0; index < 30; index++)
            {
                GatePlan a = first.GetNext(10f, 1f, 3, -1);
                GatePlan b = second.GetNext(10f, 1f, 3, -1);
                differs |= a.Pattern != b.Pattern || a.Color != b.Color;
            }

            Assert.That(differs, Is.True);
        }

        [Test]
        public void PatternSequence_UsesDistinctFairTimingAndNoImmediateRepeat()
        {
            var sequence =
                new DeterministicGatePatternSequence(GameRules.DefaultSeed);
            var patterns = new HashSet<GatePatternType>();
            var times = new HashSet<float>();
            GatePatternType previousPattern = GatePatternType.Steady;
            bool hasPreviousPattern = false;

            for (int index = 0; index < 100; index++)
            {
                GatePlan plan = sequence.GetNext(12f, 0.82f, 3, -1);
                patterns.Add(plan.Pattern);
                times.Add(plan.TimeToGate);
                Assert.That(
                    plan.TimeToGate,
                    Is.GreaterThanOrEqualTo(GameRules.MinimumReactionTime));
                Assert.That(
                    plan.Spacing,
                    Is.GreaterThanOrEqualTo(
                        (12f * plan.TimeToGate) + GameRules.GateSafetyMargin));

                if (plan.IsPatternStart)
                {
                    if (hasPreviousPattern)
                    {
                        Assert.That(plan.Pattern, Is.Not.EqualTo(previousPattern));
                    }
                    previousPattern = plan.Pattern;
                    hasPreviousPattern = true;
                }
            }

            Assert.That(patterns.Count, Is.GreaterThanOrEqualTo(7));
            Assert.That(times.Count, Is.GreaterThanOrEqualTo(6));
        }

        [Test]
        public void PatternSequence_ColorRunNeverExceedsFour()
        {
            var sequence =
                new DeterministicGatePatternSequence(GameRules.DefaultSeed);
            RunnerColor previous = sequence.GetNext(10f, 0.82f, 3, -1).Color;
            int run = 1;

            for (int index = 1; index < 1000; index++)
            {
                RunnerColor color =
                    sequence.GetNext(10f, 0.82f, 3, -1).Color;
                run = color == previous ? run + 1 : 1;
                previous = color;
                Assert.That(run, Is.LessThanOrEqualTo(4));
            }
        }

        [Test]
        public void SameColorBait_ContainsOneReadableException()
        {
            var sequence =
                new DeterministicGatePatternSequence(GameRules.DefaultSeed);
            var colors = new List<RunnerColor>();
            bool found = false;

            for (int index = 0; index < 300 && !found; index++)
            {
                GatePlan plan = sequence.GetNext(10f, 0.82f, 2, -1);
                if (plan.Pattern != GatePatternType.SameColorBait)
                {
                    continue;
                }

                if (plan.IsPatternStart)
                {
                    colors.Clear();
                }
                colors.Add(plan.Color);
                if (plan.IndexInPattern == 4)
                {
                    found = true;
                }
            }

            Assert.That(found, Is.True);
            Assert.That(colors.Count, Is.EqualTo(5));
            Assert.That(colors[0], Is.EqualTo(colors[1]));
            Assert.That(colors[1], Is.EqualTo(colors[2]));
            Assert.That(colors[3], Is.Not.EqualTo(colors[2]));
            Assert.That(colors[4], Is.EqualTo(colors[2]));
        }

        [Test]
        public void PatternSequence_DiagnosticReportsFirstThirtyAndDistribution()
        {
            var sequence =
                new DeterministicGatePatternSequence(GameRules.DefaultSeed);
            var counts = new Dictionary<GatePatternType, int>();
            var firstThirty = new StringBuilder();
            float elapsed = 0f;

            for (int index = 0; index < 100; index++)
            {
                float speed = GameRules.CalculateSpeed(0, elapsed);
                float target =
                    GameRules.CalculateTargetEncounterInterval(elapsed);
                int colorCount = elapsed >= 15f ? 3 : 2;
                GatePlan plan =
                    sequence.GetNext(speed, target, colorCount, -1);
                counts.TryGetValue(plan.Pattern, out int count);
                counts[plan.Pattern] = count + 1;
                if (index < 30)
                {
                    if (index > 0)
                    {
                        firstThirty.Append(" | ");
                    }
                    firstThirty.Append(plan.Pattern);
                    firstThirty.Append('[');
                    firstThirty.Append(plan.IndexInPattern);
                    firstThirty.Append("] ");
                    firstThirty.Append(plan.TimeToGate.ToString("0.00"));
                    firstThirty.Append("s ");
                    firstThirty.Append(plan.Color);
                }
                elapsed += plan.TimeToGate;
            }

            var distribution = new StringBuilder();
            int total = 0;
            foreach (KeyValuePair<GatePatternType, int> pair in counts)
            {
                if (distribution.Length > 0)
                {
                    distribution.Append(", ");
                }
                distribution.Append(pair.Key);
                distribution.Append('=');
                distribution.Append(pair.Value);
                total += pair.Value;
            }

            TestContext.WriteLine("First30: " + firstThirty);
            TestContext.WriteLine("Distribution100: " + distribution);
            Assert.That(total, Is.EqualTo(100));
        }

        [Test]
        public void PatternSequence_EncounterCadenceDecreasesFromEarlyToLate()
        {
            float earlyTotal = 0f;
            float lateTotal = 0f;
            var early =
                new DeterministicGatePatternSequence(GameRules.DefaultSeed);
            var late =
                new DeterministicGatePatternSequence(GameRules.DefaultSeed);

            for (int index = 0; index < 40; index++)
            {
                earlyTotal +=
                    early.GetNext(6f, 1.65f, 2, -1).TimeToGate;
                lateTotal +=
                    late.GetNext(15f, 0.82f, 3, -1).TimeToGate;
            }

            Assert.That(lateTotal / 40f, Is.LessThan(earlyTotal / 40f));
            Assert.That(
                (earlyTotal - lateTotal) / 40f,
                Is.GreaterThan(0.45f));
        }

        [Test]
        public void PatternSequence_DoesNotRepeatEitherRecentPattern()
        {
            var sequence =
                new DeterministicGatePatternSequence(GameRules.DefaultSeed);
            GatePatternType previous = GatePatternType.ThirdColorTutorial;
            GatePatternType beforePrevious = GatePatternType.ThirdColorTutorial;
            int patternStarts = 0;

            for (int index = 0; index < 300; index++)
            {
                GatePlan plan =
                    sequence.GetNext(14f, 0.82f, 3, -1);
                if (!plan.IsPatternStart)
                {
                    continue;
                }

                if (patternStarts >= 2)
                {
                    Assert.That(plan.Pattern, Is.Not.EqualTo(previous));
                    Assert.That(
                        plan.Pattern,
                        Is.Not.EqualTo(beforePrevious));
                }

                beforePrevious = previous;
                previous = plan.Pattern;
                patternStarts++;
            }

            Assert.That(patternStarts, Is.GreaterThan(20));
        }

        [Test]
        public void PatternSequence_OnlyUsesActiveColors()
        {
            var twoColor =
                new DeterministicGatePatternSequence(GameRules.DefaultSeed);
            var threeColor =
                new DeterministicGatePatternSequence(GameRules.DefaultSeed);
            bool foundGreen = false;

            for (int index = 0; index < 300; index++)
            {
                Assert.That(
                    twoColor.GetNext(10f, 1f, 2, -1).Color,
                    Is.Not.EqualTo(RunnerColor.Green));
                foundGreen |=
                    threeColor.GetNext(14f, 0.82f, 3, -1).Color ==
                    RunnerColor.Green;
            }

            Assert.That(foundGreen, Is.True);
        }

        [Test]
        public void ThreeColorSequence_NeverRequiresMoreThanOneTap()
        {
            var sequence =
                new DeterministicGatePatternSequence(GameRules.DefaultSeed);
            RunnerColor previous =
                sequence.GetNext(14f, 0.82f, 3, -1).Color;

            for (int index = 1; index < 1000; index++)
            {
                RunnerColor color =
                    sequence.GetNext(14f, 0.82f, 3, -1).Color;
                Assert.That(
                    color == previous || color == NextColor(previous),
                    Is.True,
                    $"{previous} to {color} requires more than one tap.");
                previous = color;
            }
        }

        [Test]
        public void ShieldPickupPlan_IsDeterministicAndAppearsInEarlyRange()
        {
            var first =
                new DeterministicGatePatternSequence(GameRules.DefaultSeed);
            var second =
                new DeterministicGatePatternSequence(GameRules.DefaultSeed);
            int firstPickupIndex = -1;

            for (int index = 0; index < 20; index++)
            {
                GatePlan a = first.GetNext(9f, 1.2f, 2, -1);
                GatePlan b = second.GetNext(9f, 1.2f, 2, -1);
                Assert.That(
                    b.HasShieldPickupBefore,
                    Is.EqualTo(a.HasShieldPickupBefore));
                if (a.HasShieldPickupBefore)
                {
                    Assert.That(firstPickupIndex, Is.EqualTo(-1));
                    firstPickupIndex = index;
                }
            }

            Assert.That(firstPickupIndex, Is.InRange(6, 8));
        }

        [Test]
        public void PatternSequence_DiagnosticReportsFirstFortyCadenceValues()
        {
            var sequence =
                new DeterministicGatePatternSequence(GameRules.DefaultSeed);
            var diagnostic = new StringBuilder();
            float elapsed = 0f;

            for (int index = 0; index < 40; index++)
            {
                float speed = GameRules.CalculateSpeed(0, elapsed);
                float target =
                    GameRules.CalculateTargetEncounterInterval(elapsed);
                int colors =
                    elapsed >= GameRules.ThirdColorScoreMilestone ? 3 : 2;
                GatePlan plan =
                    sequence.GetNext(speed, target, colors, -1);
                float actualReaction =
                    (plan.Spacing - GameRules.GateSafetyMargin) / speed;
                diagnostic.Append(index + 1);
                diagnostic.Append(':');
                diagnostic.Append(plan.Color);
                diagnostic.Append('/');
                diagnostic.Append(plan.Pattern);
                diagnostic.Append('/');
                diagnostic.Append(plan.BeatMultiplier.ToString("0.00"));
                diagnostic.Append('/');
                diagnostic.Append(target.ToString("0.00"));
                diagnostic.Append('/');
                diagnostic.Append(plan.Spacing.ToString("0.00"));
                diagnostic.Append('/');
                diagnostic.Append(actualReaction.ToString("0.00"));
                diagnostic.AppendLine();
                elapsed += plan.TimeToGate;
            }

            TestContext.WriteLine("Step7First40:\n" + diagnostic);
            Assert.That(
                diagnostic.ToString().Split('\n').Length,
                Is.GreaterThan(40));
        }

        private static RunnerColor NextColor(RunnerColor color)
        {
            return color == RunnerColor.Red
                ? RunnerColor.Blue
                : color == RunnerColor.Blue
                    ? RunnerColor.Green
                    : RunnerColor.Red;
        }
    }
}
