using System;

namespace ColorGateRunner.Core
{
    public sealed class DeterministicGatePatternSequence
    {
        private uint _state;
        private GatePatternType _currentPattern;
        private GatePatternType _previousPattern;
        private int _patternIndex;
        private int _patternLength;
        private int _generatedGateCount;
        private RunnerColor _baseColor;
        private RunnerColor _previousColor;
        private int _colorRunLength;
        private bool _hasPattern;
        private bool _hasPreviousColor;

        public DeterministicGatePatternSequence(uint seed)
        {
            Reset(seed);
        }

        public GatePlan GetNext(float currentSpeed)
        {
            if (currentSpeed < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(currentSpeed));
            }

            if (!_hasPattern || _patternIndex >= _patternLength)
            {
                SelectNextPattern();
            }

            float timeToGate = GetTimeToGate(_currentPattern, _patternIndex);
            RunnerColor color = GetPatternColor(_currentPattern, _patternIndex);
            color = EnforceColorRunLimit(color);
            float rawSpacing =
                (currentSpeed * timeToGate) + GameRules.GateSafetyMargin;
            float spacing = (float)Math.Ceiling(rawSpacing * 2f) * 0.5f;
            GatePlan plan =
                new GatePlan(
                    color,
                    spacing,
                    timeToGate,
                    _currentPattern,
                    _patternIndex);

            _patternIndex++;
            _generatedGateCount++;
            return plan;
        }

        public void Reset(uint seed)
        {
            _state =
                DeterministicGateSequence.NormalizeSeed(seed) ^ 0xA511E9B3u;
            _currentPattern = GatePatternType.Steady;
            _previousPattern = GatePatternType.Steady;
            _patternIndex = 0;
            _patternLength = 0;
            _generatedGateCount = 0;
            _baseColor = RunnerColor.Red;
            _previousColor = RunnerColor.Red;
            _colorRunLength = 0;
            _hasPattern = false;
            _hasPreviousColor = false;
        }

        private void SelectNextPattern()
        {
            _state = DeterministicGateSequence.AdvanceXorshift32(_state);
            GatePatternType selected;
            if (!_hasPattern)
            {
                selected = GatePatternType.Steady;
            }
            else
            {
                int availablePatternCount =
                    _generatedGateCount < 8 ? 3 :
                    _generatedGateCount < 20 ? 5 : 7;
                selected = (GatePatternType)(_state % (uint)availablePatternCount);
                if (selected == _previousPattern)
                {
                    selected =
                        (GatePatternType)(((int)selected + 1) % availablePatternCount);
                }
            }

            _state = DeterministicGateSequence.AdvanceXorshift32(_state);
            _baseColor = (_state & 1u) == 0u
                ? RunnerColor.Red
                : RunnerColor.Blue;
            _currentPattern = selected;
            _previousPattern = selected;
            _patternIndex = 0;
            _patternLength = GetPatternLength(selected);
            _hasPattern = true;
        }

        private RunnerColor GetPatternColor(GatePatternType pattern, int index)
        {
            bool opposite;
            switch (pattern)
            {
                case GatePatternType.Release:
                    opposite = index == 2;
                    break;
                case GatePatternType.SameColorBait:
                    opposite = index == 3;
                    break;
                case GatePatternType.SingleColorBreak:
                    opposite = index > 0 && index < 4;
                    break;
                default:
                    opposite = (index & 1) == 1;
                    break;
            }

            return opposite ? OppositeOf(_baseColor) : _baseColor;
        }

        private RunnerColor EnforceColorRunLimit(RunnerColor color)
        {
            if (_hasPreviousColor &&
                color == _previousColor &&
                _colorRunLength >= GameRules.MaximumConsecutiveGateColors)
            {
                color = OppositeOf(color);
            }

            if (_hasPreviousColor && color == _previousColor)
            {
                _colorRunLength++;
            }
            else
            {
                _previousColor = color;
                _colorRunLength = 1;
                _hasPreviousColor = true;
            }

            return color;
        }

        private static int GetPatternLength(GatePatternType pattern)
        {
            switch (pattern)
            {
                case GatePatternType.Compression:
                case GatePatternType.Release:
                    return 4;
                case GatePatternType.SameColorBait:
                case GatePatternType.SingleColorBreak:
                    return 5;
                default:
                    return 3;
            }
        }

        private static float GetTimeToGate(GatePatternType pattern, int index)
        {
            switch (pattern)
            {
                case GatePatternType.ShortShortLong:
                    return index < 2 ? 0.9f : 1.9f;
                case GatePatternType.LongShortLong:
                    return index == 1 ? 0.9f : 1.9f;
                case GatePatternType.Compression:
                    return index == 0 ? 1.8f :
                        index == 1 ? 1.45f :
                        index == 2 ? 1.15f : 0.9f;
                case GatePatternType.Release:
                    return index == 0 ? 0.9f :
                        index == 1 ? 1.15f :
                        index == 2 ? 1.45f : 1.9f;
                case GatePatternType.SameColorBait:
                    return index == 3 ? 1.6f : 1.2f;
                case GatePatternType.SingleColorBreak:
                    return index == 4 ? 1.8f : 1.15f;
                default:
                    return 1.3f;
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
