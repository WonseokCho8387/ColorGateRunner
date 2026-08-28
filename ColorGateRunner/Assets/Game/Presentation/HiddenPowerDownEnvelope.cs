namespace ColorGateRunner.Presentation
{
    internal static class HiddenPowerDownEnvelope
    {
        private static readonly float[] ProgressKeys =
        {
            0f,
            0.14f,
            0.22f,
            0.34f,
            0.44f,
            0.57f,
            0.68f,
            0.78f,
            0.88f,
            1f
        };

        private static readonly float[] VisibilityKeys =
        {
            1f,
            1f,
            0.06f,
            0.88f,
            0.12f,
            0.62f,
            0.05f,
            0.28f,
            0.02f,
            0f
        };

        internal static float Evaluate(float progress)
        {
            if (progress <= 0f)
            {
                return 1f;
            }
            if (progress >= 1f)
            {
                return 0f;
            }

            for (int index = 1; index < ProgressKeys.Length; index++)
            {
                if (progress > ProgressKeys[index])
                {
                    continue;
                }

                float segmentProgress =
                    (progress - ProgressKeys[index - 1]) /
                    (ProgressKeys[index] - ProgressKeys[index - 1]);
                return VisibilityKeys[index - 1] +
                    ((VisibilityKeys[index] - VisibilityKeys[index - 1]) *
                     segmentProgress);
            }

            return 0f;
        }
    }
}
