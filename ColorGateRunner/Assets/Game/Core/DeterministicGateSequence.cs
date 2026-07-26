namespace ColorGateRunner.Core
{
    public sealed class DeterministicGateSequence
    {
        internal const uint ZeroSeedFallback = 0x6D2B79F5u;

        private uint _state;
        private RunnerColor _previousColor;
        private int _consecutiveColorCount;
        private bool _hasPreviousColor;

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

        public void Reset(uint seed)
        {
            _state = NormalizeSeed(seed);
            _previousColor = RunnerColor.Red;
            _consecutiveColorCount = 0;
            _hasPreviousColor = false;
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
