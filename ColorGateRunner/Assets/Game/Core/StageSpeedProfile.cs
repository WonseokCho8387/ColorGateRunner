using System;

namespace ColorGateRunner.Core
{
    public sealed class StageSpeedProfile
    {
        private readonly float[] _normalizedSamples;
        private readonly bool _isLinear;

        public StageSpeedProfile(
            float startingSpeed,
            float maximumSpeed,
            float[] normalizedSamples)
        {
            if (startingSpeed <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(startingSpeed));
            }
            if (maximumSpeed < startingSpeed)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumSpeed));
            }
            if (normalizedSamples == null || normalizedSamples.Length < 2)
            {
                throw new ArgumentException(
                    "At least two speed-curve samples are required.",
                    nameof(normalizedSamples));
            }

            StartingSpeed = startingSpeed;
            MaximumSpeed = maximumSpeed;
            _normalizedSamples = new float[normalizedSamples.Length];
            for (int index = 0; index < normalizedSamples.Length; index++)
            {
                _normalizedSamples[index] = Clamp01(normalizedSamples[index]);
            }
            _isLinear = _normalizedSamples.Length == 2 &&
                _normalizedSamples[0] == 0f &&
                _normalizedSamples[1] == 1f;
        }

        public float StartingSpeed { get; }
        public float MaximumSpeed { get; }
        public int SampleCount => _normalizedSamples.Length;
        public bool IsLinear => _isLinear;

        public float Evaluate(float normalizedProgress)
        {
            float progress = Clamp01(normalizedProgress);
            if (_isLinear)
            {
                return StartingSpeed +
                    ((MaximumSpeed - StartingSpeed) * progress);
            }

            float samplePosition = progress * (_normalizedSamples.Length - 1);
            int lowerIndex = (int)samplePosition;
            int upperIndex = lowerIndex + 1;
            if (upperIndex >= _normalizedSamples.Length)
            {
                upperIndex = _normalizedSamples.Length - 1;
            }

            float fraction = samplePosition - lowerIndex;
            float normalizedSpeed =
                _normalizedSamples[lowerIndex] +
                ((_normalizedSamples[upperIndex] -
                  _normalizedSamples[lowerIndex]) * fraction);
            return StartingSpeed +
                ((MaximumSpeed - StartingSpeed) * normalizedSpeed);
        }

        public float GetNormalizedSample(int index)
        {
            return _normalizedSamples[index];
        }

        public static StageSpeedProfile Linear(
            float startingSpeed,
            float maximumSpeed)
        {
            return new StageSpeedProfile(
                startingSpeed,
                maximumSpeed,
                new[] { 0f, 1f });
        }

        private static float Clamp01(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }
            return value > 1f ? 1f : value;
        }
    }
}
