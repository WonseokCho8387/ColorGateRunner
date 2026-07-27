namespace ColorGateRunner.Core
{
    public sealed class DeterministicGateSequence
    {
        internal const uint ZeroSeedFallback = 0x6D2B79F5u;

        private uint _state;
        private RunnerColor _previousColor;
        private int _consecutiveColorCount;
        private bool _hasPreviousColor;
        private uint _spacingState;
        private int _previousSpacingBand;
        private int _spacingBandRunLength;

        public DeterministicGateSequence(uint seed)
        {
            Reset(seed);
        }

        public RunnerColor GetNextColor()
        {
            _state = AdvanceXorshift32(_state);
            RunnerColor nextColor = (_state & 1u) == 0u
                ? RunnerColor.Red
                : RunnerColor.Blue;

            if (_hasPreviousColor &&
                nextColor == _previousColor &&
                _consecutiveColorCount >= GameRules.MaximumConsecutiveGateColors)
            {
                nextColor = OppositeOf(nextColor);
            }

            if (_hasPreviousColor && nextColor == _previousColor)
            {
                _consecutiveColorCount++;
            }
            else
            {
                _previousColor = nextColor;
                _consecutiveColorCount = 1;
                _hasPreviousColor = true;
            }

            return nextColor;
        }

        public float GetNextSpacing(float currentSpeed)
        {
            _spacingState = AdvanceXorshift32(_spacingState);
            int band = (int)(_spacingState % 3u);
            if (band == _previousSpacingBand && _spacingBandRunLength >= 2)
            {
                band = (band + 1 + (int)((_spacingState >> 8) & 1u)) % 3;
            }

            if (band == _previousSpacingBand)
            {
                _spacingBandRunLength++;
            }
            else
            {
                _previousSpacingBand = band;
                _spacingBandRunLength = 1;
            }

            float minimum = GameRules.CalculateMinimumSafeSpacing(currentSpeed);
            float quantizedMinimum = (float)System.Math.Ceiling(minimum * 2f) * 0.5f;
            return quantizedMinimum + (band * 1.5f);
        }

        public void Reset(uint seed)
        {
            uint normalizedSeed = NormalizeSeed(seed);
            _state = normalizedSeed;
            _previousColor = RunnerColor.Red;
            _consecutiveColorCount = 0;
            _hasPreviousColor = false;
            _spacingState = normalizedSeed ^ 0x9E3779B9u;
            _previousSpacingBand = -1;
            _spacingBandRunLength = 0;
        }

        internal static uint NormalizeSeed(uint seed)
        {
            return seed == 0u ? ZeroSeedFallback : seed;
        }

        internal static uint AdvanceXorshift32(uint state)
        {
            unchecked
            {
                state ^= state << 13;
                state ^= state >> 17;
                state ^= state << 5;
                return state;
            }
        }

        private static RunnerColor OppositeOf(RunnerColor color)
        {
            return color == RunnerColor.Red
                ? RunnerColor.Blue
                : RunnerColor.Red;
        }
    }
}
