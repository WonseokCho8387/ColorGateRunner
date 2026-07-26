using System;

namespace ColorGateRunner.Core
{
    public static class GameRules
    {
        public const uint DefaultSeed = 12345u;
        public const int InitialScore = 0;
        public const float InitialSpeed = 4f;
        public const float SpeedIncrease = 0.15f;
        public const int PointsPerSpeedIncrease = 5;
        public const float MaximumSpeed = 9f;
        public const int MaximumConsecutiveGateColors = 4;
        public const float MinimumGateDistance = 6f;

        public static float CalculateSpeed(int score)
        {
            if (score < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(score));
            }

            int completedThresholds = score / PointsPerSpeedIncrease;
            float calculatedSpeed = InitialSpeed + (completedThresholds * SpeedIncrease);
            return Math.Min(MaximumSpeed, calculatedSpeed);
        }
    }
}
