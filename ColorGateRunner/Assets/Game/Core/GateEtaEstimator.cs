using System;

namespace ColorGateRunner.Core
{
    public static class GateEtaEstimator
    {
        public static float EstimateSeconds(
            float remainingDistance,
            float effectiveSpeed)
        {
            if (remainingDistance <= 0f)
            {
                return 0f;
            }
            if (effectiveSpeed <= 0f)
            {
                return float.PositiveInfinity;
            }

            return remainingDistance / effectiveSpeed;
        }

        public static bool ShouldStartReveal(
            float estimatedArrivalSeconds,
            CamouflageSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            return estimatedArrivalSeconds <=
                settings.RevealLeadTimeSeconds;
        }
    }
}
