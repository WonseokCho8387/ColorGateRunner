using System;
using ColorGateRunner.Core;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class DeterministicGateSequenceTests
    {
        private const uint TestSeed = GameRules.DefaultSeed;
        private const int ReferenceLength = 32;
        private const uint ReferenceZeroSeedFallback = 0x6D2B79F5u;

        private static readonly RunnerColor[] Seed12345GoldenSequence =
        {
            RunnerColor.Red,
            RunnerColor.Blue,
            RunnerColor.Red,
            RunnerColor.Red,
            RunnerColor.Blue,
            RunnerColor.Red,
            RunnerColor.Red,
            RunnerColor.Red,
            RunnerColor.Red,
            RunnerColor.Blue,
            RunnerColor.Red,
            RunnerColor.Blue,
            RunnerColor.Red,
            RunnerColor.Blue,
            RunnerColor.Blue,
            RunnerColor.Red,
            RunnerColor.Blue,
            RunnerColor.Red,
            RunnerColor.Red,
            RunnerColor.Red,
            RunnerColor.Blue,
            RunnerColor.Blue,
            RunnerColor.Red,
            RunnerColor.Blue,
            RunnerColor.Blue,
            RunnerColor.Blue,
            RunnerColor.Blue,
            RunnerColor.Red,
            RunnerColor.Red,
            RunnerColor.Blue,
            RunnerColor.Blue,
            RunnerColor.Blue
        };

        [Test]
        public void Seed12345_MatchesIndependentReference()
        {
            uint[] expectedRawValues = CalculateReferenceRawValues(TestSeed, ReferenceLength);
            RunnerColor[] expectedColors = ConvertReferenceValuesToColors(expectedRawValues);
            DeterministicGateSequence sequence = new DeterministicGateSequence(TestSeed);
            uint productionState = TestSeed;

            Assert.That(expectedColors, Is.EqualTo(Seed12345GoldenSequence));

            for (int index = 0; index < ReferenceLength; index++)
            {
                productionState = DeterministicGateSequence.AdvanceXorshift32(productionState);
                Assert.That(
                    productionState,
                    Is.EqualTo(expectedRawValues[index]),
                    $"Raw xorshift32 value differed at index {index}.");

                Assert.That(
                    sequence.GetNextColor(),
                    Is.EqualTo(expectedColors[index]),
                    $"Gate color differed at index {index}.");
            }
        }

        [Test]
        public void GeneratedSequence_RunLengthNeverExceedsFour()
        {
            uint[] seeds = { 0u, 1u, TestSeed, 0x80000000u, uint.MaxValue };

            for (int seedIndex = 0; seedIndex < seeds.Length; seedIndex++)
            {
                DeterministicGateSequence sequence = new DeterministicGateSequence(seeds[seedIndex]);
                RunnerColor previousColor = sequence.GetNextColor();
                int runLength = 1;

                for (int index = 1; index < 10000; index++)
                {
                    RunnerColor color = sequence.GetNextColor();
                    if (color == previousColor)
                    {
                        runLength++;
                    }
                    else
                    {
                        previousColor = color;
                        runLength = 1;
                    }

                    Assert.That(
                        runLength,
                        Is.LessThanOrEqualTo(GameRules.MaximumConsecutiveGateColors),
                        $"Seed {seeds[seedIndex]} exceeded the run limit at index {index}.");
                }
            }
        }

        [Test]
        public void SameSeed_ProducesSameSequence()
        {
            DeterministicGateSequence first = new DeterministicGateSequence(TestSeed);
            DeterministicGateSequence second = new DeterministicGateSequence(TestSeed);

            for (int index = 0; index < 1000; index++)
            {
                Assert.That(second.GetNextColor(), Is.EqualTo(first.GetNextColor()));
            }
        }

        [Test]
        public void SeedZero_UsesDocumentedFallback()
        {
            uint normalizedSeed = DeterministicGateSequence.NormalizeSeed(0u);
            uint[] expectedRawValues = CalculateReferenceRawValues(ReferenceZeroSeedFallback, 1);

            Assert.That(normalizedSeed, Is.EqualTo(ReferenceZeroSeedFallback));
            Assert.That(
                DeterministicGateSequence.AdvanceXorshift32(normalizedSeed),
                Is.EqualTo(expectedRawValues[0]));
        }

        [Test]
        public void GateSpacing_MinimumIsSix()
        {
            Assert.That(GameRules.MinimumGateDistance, Is.EqualTo(6f));
        }

        [Test]
        public void CoreAssembly_DoesNotReferenceUnityEngine()
        {
            System.Reflection.AssemblyName[] references =
                typeof(GameSession).Assembly.GetReferencedAssemblies();

            for (int index = 0; index < references.Length; index++)
            {
                Assert.That(
                    references[index].Name.StartsWith("UnityEngine", StringComparison.Ordinal),
                    Is.False,
                    $"Core references {references[index].Name}.");
            }
        }

        private static uint[] CalculateReferenceRawValues(uint seed, int count)
        {
            uint state = seed == 0u ? ReferenceZeroSeedFallback : seed;
            uint[] values = new uint[count];

            for (int index = 0; index < count; index++)
            {
                unchecked
                {
                    state ^= state << 13;
                    state ^= state >> 17;
                    state ^= state << 5;
                }

                values[index] = state;
            }

            return values;
        }

        private static RunnerColor[] ConvertReferenceValuesToColors(uint[] rawValues)
        {
            RunnerColor[] colors = new RunnerColor[rawValues.Length];
            RunnerColor previousColor = RunnerColor.Red;
            int runLength = 0;
            bool hasPreviousColor = false;

            for (int index = 0; index < rawValues.Length; index++)
            {
                RunnerColor color = (rawValues[index] & 1u) == 0u
                    ? RunnerColor.Red
                    : RunnerColor.Blue;

                if (hasPreviousColor && color == previousColor && runLength >= 4)
                {
                    color = color == RunnerColor.Red
                        ? RunnerColor.Blue
                        : RunnerColor.Red;
                }

                if (hasPreviousColor && color == previousColor)
                {
                    runLength++;
                }
                else
                {
                    previousColor = color;
                    runLength = 1;
                    hasPreviousColor = true;
                }

                colors[index] = color;
            }

            return colors;
        }
    }
}
