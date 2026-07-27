using System;

namespace ColorGateRunner.Core
{
    public static class GameRules
    {
        public const uint DefaultSeed = 12345u;
        public const int InitialScore = 0;
        public const float InitialSpeed = 6f;
        public const float ScoreAcceleration = 0.01f;
        public const float MaximumSpeed = 15f;
        public const float MinimumReactionTime = 0.75f;
        public const float GateSafetyMargin = 0.5f;
        public const float ShieldRecoveryDuration = 1f;
        public const float ShieldRecoveryInitialSpeedMultiplier = 0.7f;
        public const float DefaultCountdownDuration = 3f;
        public const int ThirdColorScoreMilestone = 15;
        public const int ThirdColorTutorialGateCount = 2;
        public const int MaximumConsecutiveGateColors = 4;
        public const float MinimumGateDistance = 5.5f;
        public const int AllowedGateSpacingCount = 4;

        public static float CalculateSpeed(int score, float elapsedPlayingSeconds)
        {
            if (score < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(score));
            }

            if (elapsedPlayingSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(elapsedPlayingSeconds));
            }

            float timeSpeed = InterpolateProgression(
                elapsedPlayingSeconds,
                6f,
                9f,
                10.5f,
                12.5f,
                14f,
                15f);
            float calculatedSpeed = timeSpeed + (score * ScoreAcceleration);
            return Math.Min(MaximumSpeed, calculatedSpeed);
        }

        public static float CalculateTargetEncounterInterval(
            float elapsedPlayingSeconds)
        {
            if (elapsedPlayingSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(elapsedPlayingSeconds));
            }

            return InterpolateProgression(
                elapsedPlayingSeconds,
                1.65f,
                1.32f,
                1.15f,
                1f,
                0.87f,
                0.82f);
        }

        public static float CalculatePatternEncounterInterval(
            float targetEncounterInterval,
            float beatMultiplier)
        {
            if (targetEncounterInterval < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(targetEncounterInterval));
            }

            if (beatMultiplier <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(beatMultiplier));
            }

            return Math.Max(
                MinimumReactionTime,
                targetEncounterInterval * beatMultiplier);
        }

        public static int CalculateFirstShieldPickupGateIndex(uint seed)
        {
            uint normalized = DeterministicGateSequence.NormalizeSeed(seed);
            return 6 + (int)(normalized % 3u);
        }

        public static float GetAllowedGateSpacing(int index)
        {
            switch (index)
            {
                case 0:
                    return 5.5f;
                case 1:
                    return 6f;
                case 2:
                    return 7f;
                case 3:
                    return 8f;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        public static bool IsAllowedGateSpacing(float spacing)
        {
            for (int index = 0; index < AllowedGateSpacingCount; index++)
            {
                if (spacing == GetAllowedGateSpacing(index))
                {
                    return true;
                }
            }

            return false;
        }

        public static float CalculateMinimumSafeSpacing(float speed)
        {
            if (speed < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(speed));
            }

            return (speed * MinimumReactionTime) + GateSafetyMargin;
        }

        public static SpeedPresentation GetSpeedPresentation(
            float elapsedPlayingSeconds,
            float currentSpeed)
        {
            if (elapsedPlayingSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(elapsedPlayingSeconds));
            }

            if (currentSpeed < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(currentSpeed));
            }

            SpeedStage stage;
            if (elapsedPlayingSeconds < 5f)
            {
                stage = SpeedStage.Start;
            }
            else if (elapsedPlayingSeconds < 15f)
            {
                stage = SpeedStage.Accelerating;
            }
            else if (elapsedPlayingSeconds < 30f)
            {
                stage = SpeedStage.Fast;
            }
            else
            {
                stage = SpeedStage.MaximumPressure;
            }

            float intensity = Math.Max(
                0f,
                Math.Min(
                    1f,
                    (currentSpeed - InitialSpeed) /
                    (MaximumSpeed - InitialSpeed)));

            switch (stage)
            {
                case SpeedStage.Start:
                    return new SpeedPresentation(stage, intensity, 60f, 0.1f, 0f, 1f);
                case SpeedStage.Accelerating:
                    return new SpeedPresentation(stage, intensity, 65f, 0.35f, 18f, 1.25f);
                case SpeedStage.Fast:
                    return new SpeedPresentation(stage, intensity, 70f, 0.7f, 40f, 1.55f);
                default:
                    return new SpeedPresentation(stage, intensity, 74f, 1f, 70f, 1.9f);
            }
        }

        private static float InterpolateProgression(
            float elapsed,
            float atZero,
            float atFive,
            float atTen,
            float atTwenty,
            float atThirty,
            float atFortyFive)
        {
            if (elapsed <= 5f)
            {
                return Lerp(atZero, atFive, elapsed / 5f);
            }

            if (elapsed <= 10f)
            {
                return Lerp(atFive, atTen, (elapsed - 5f) / 5f);
            }

            if (elapsed <= 20f)
            {
                return Lerp(atTen, atTwenty, (elapsed - 10f) / 10f);
            }

            if (elapsed <= 30f)
            {
                return Lerp(atTwenty, atThirty, (elapsed - 20f) / 10f);
            }

            if (elapsed <= 45f)
            {
                return Lerp(atThirty, atFortyFive, (elapsed - 30f) / 15f);
            }

            return atFortyFive;
        }

        private static float Lerp(float from, float to, float t)
        {
            return from + ((to - from) * Math.Max(0f, Math.Min(1f, t)));
        }
    }
}
