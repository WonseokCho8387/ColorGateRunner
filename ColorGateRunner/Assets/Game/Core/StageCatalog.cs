using System;

namespace ColorGateRunner.Core
{
    public static class StageCatalog
    {
        private static readonly StageDefinition[] Stages =
        {
            Create(
                "stage-01", 1, "TWO-COLOR BASICS",
                "Learn the red and blue match.",
                24, new[] { RunnerColor.Red, RunnerColor.Blue },
                7f, 10f, 1.55f, 1.25f,
                new[] { GatePatternType.Steady, GatePatternType.Release },
                6, 4, 80f, 22f, 10101u),
            Create(
                "stage-02", 2, "RHYTHM CONTRAST",
                "Read changing gate cadence.",
                28, new[] { RunnerColor.Red, RunnerColor.Blue },
                8f, 11f, 1.45f, 1.10f,
                new[]
                {
                    GatePatternType.Steady,
                    GatePatternType.Compression,
                    GatePatternType.Release,
                    GatePatternType.Syncopation
                },
                5, 5, 90f, 23f, 20202u),
            Create(
                "stage-03", 3, "EXCEPTION COLOR",
                "Watch for deliberate color exceptions.",
                30, new[] { RunnerColor.Red, RunnerColor.Blue },
                8.5f, 11.5f, 1.40f, 1.00f,
                new[]
                {
                    GatePatternType.Steady,
                    GatePatternType.SameColorBait,
                    GatePatternType.SingleColorBreak,
                    GatePatternType.Burst
                },
                5, 6, 100f, 24f, 30303u),
            Create(
                "stage-04", 4, "GREEN INTRODUCTION",
                "Meet green safely, then use all three colors.",
                32, new[]
                {
                    RunnerColor.Red,
                    RunnerColor.Blue,
                    RunnerColor.Green
                },
                8f, 12f, 1.45f, 1.00f,
                new[]
                {
                    GatePatternType.Steady,
                    GatePatternType.ThirdColorTutorial,
                    GatePatternType.Compression,
                    GatePatternType.Release
                },
                8, 6, 105f, 25f, 40404u),
            Create(
                "stage-05", 5, "THREE-COLOR CHALLENGE",
                "Master three colors under pressure.",
                36, new[]
                {
                    RunnerColor.Red,
                    RunnerColor.Blue,
                    RunnerColor.Green
                },
                9f, 13.5f, 1.25f, 0.85f,
                new[]
                {
                    GatePatternType.ThreeColorFlow,
                    GatePatternType.Compression,
                    GatePatternType.Syncopation,
                    GatePatternType.SameColorBait,
                    GatePatternType.SingleColorBreak
                },
                4, 8, 115f, 26f, 50505u)
        };

        public static int Count => Stages.Length;

        public static StageDefinition GetByIndex(int index)
        {
            if (index < 0 || index >= Stages.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return Stages[index];
        }

        public static StageDefinition GetByDisplayNumber(int displayNumber)
        {
            return GetByIndex(displayNumber - 1);
        }

        private static StageDefinition Create(
            string id,
            int number,
            string title,
            string description,
            int gateCount,
            RunnerColor[] colors,
            float startingSpeed,
            float maximumSpeed,
            float cadenceStart,
            float cadenceEnd,
            GatePatternType[] patterns,
            int introCount,
            int finalCount,
            float boosterDistance,
            float boosterSpeed,
            uint seed)
        {
            return new StageDefinition(
                id,
                number,
                title,
                description,
                gateCount,
                colors,
                startingSpeed,
                maximumSpeed,
                cadenceStart,
                cadenceEnd,
                patterns,
                introCount,
                finalCount,
                boosterDistance,
                boosterSpeed,
                true,
                true,
                seed);
        }
    }
}
