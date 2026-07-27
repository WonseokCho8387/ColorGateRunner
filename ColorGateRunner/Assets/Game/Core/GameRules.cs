using System;

namespace ColorGateRunner.Core
{
    public static class GameRules
    {
        public const uint DefaultSeed = 12345u;
        public const int InitialScore = 0;
        public const float InitialSpeed = 6f;
        public const float SpeedGrowthRate = 0.14f;
        public const float ScoreAcceleration = 0.02f;
        public const float MaximumSpeed = 14f;
        public const float MinimumReactionTime = 0.85f;
        public const float GateSafetyMargin = 0.5f;
        public const int ShieldScoreMilestone = 3;
        public const float ShieldRecoveryDuration = 2f;
        public const float ShieldRecoveryInitialSpeedMultiplier = 0.65f;
        public const float DefaultCountdownDuration = 5f;
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

            double normalizedProgress =
                1d - Math.Exp(-SpeedGrowthRate * elapsedPlayingSeconds);
            float calculatedSpeed = InitialSpeed +
                ((MaximumSpeed - InitialSpeed) * (float)normalizedProgress) +
                (score * ScoreAcceleration);
            return Math.Min(MaximumSpeed, calculatedSpeed);
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
            else if (elapsedPlayingSeconds < 12f)
            {
                stage = SpeedStage.Accelerating;
            }
            else if (elapsedPlayingSeconds < 25f)
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
    }
}
