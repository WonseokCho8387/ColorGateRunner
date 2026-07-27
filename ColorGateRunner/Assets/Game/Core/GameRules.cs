using System;

namespace ColorGateRunner.Core
{
    public static class GameRules
    {
        public const uint DefaultSeed = 12345u;
        public const int InitialScore = 0;
        public const float InitialSpeed = 6f;
        public const float TimeAcceleration = 0.08f;
        public const float ScoreAcceleration = 0.12f;
        public const float MaximumSpeed = 12f;
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

            float calculatedSpeed =
                InitialSpeed +
                (elapsedPlayingSeconds * TimeAcceleration) +
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
    }
}
