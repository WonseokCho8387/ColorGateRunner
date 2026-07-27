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
                GatePlan a = first.GetNext(10f);
                GatePlan b = second.GetNext(10f);
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
                GatePlan a = first.GetNext(10f);
                GatePlan b = second.GetNext(10f);
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
                GatePlan plan = sequence.GetNext(12f);
                patterns.Add(plan.Pattern);
                times.Add(plan.TimeToGate);
                Assert.That(plan.TimeToGate, Is.GreaterThanOrEqualTo(0.85f));
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

            Assert.That(patterns.Count, Is.GreaterThanOrEqualTo(6));
            Assert.That(times.Count, Is.GreaterThanOrEqualTo(6));
        }

        [Test]
        public void PatternSequence_ColorRunNeverExceedsFour()
        {
            var sequence =
                new DeterministicGatePatternSequence(GameRules.DefaultSeed);
            RunnerColor previous = sequence.GetNext(10f).Color;
            int run = 1;

            for (int index = 1; index < 1000; index++)
            {
                RunnerColor color = sequence.GetNext(10f).Color;
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
                GatePlan plan = sequence.GetNext(10f);
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
                GatePlan plan = sequence.GetNext(speed);
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
    }
}
