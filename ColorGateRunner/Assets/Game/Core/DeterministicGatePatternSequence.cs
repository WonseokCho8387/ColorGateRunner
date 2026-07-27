using System;

namespace ColorGateRunner.Core
{
    public sealed class DeterministicGatePatternSequence
    {
        private uint _state;
        private GatePatternType _currentPattern;
        private GatePatternType _previousPattern;
        private GatePatternType _patternBeforePrevious;
        private int _patternIndex;
        private int _patternLength;
        private int _generatedGateCount;
        private RunnerColor _baseColor;
        private RunnerColor _previousPatternBaseColor;
        private RunnerColor _previousColor;
        private int _colorRunLength;
        private bool _hasPattern;
        private bool _hasPreviousPatternBaseColor;
        private bool _hasPreviousColor;
        private int _shieldPickupGateIndex;

        public DeterministicGatePatternSequence(uint seed)
        {
            Reset(seed);
        }

        public GatePlan GetNext(float currentSpeed)
        {
            return GetNext(
                currentSpeed,
                GameRules.CalculateTargetEncounterInterval(0f),
                2,
                -1);
        }

        public GatePlan GetNext(
            float movementSpeed,
            float targetEncounterInterval,
            int activeColorCount,
            int tutorialIndex)
        {
            if (movementSpeed < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(movementSpeed));
            }

            if (targetEncounterInterval < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(targetEncounterInterval));
            }

            if (activeColorCount < 2 || activeColorCount > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(activeColorCount));
            }

            if (tutorialIndex >= 0)
            {
                return CreateTutorialPlan(
                    movementSpeed,
                    targetEncounterInterval,
                    tutorialIndex);
            }

            if (!_hasPattern || _patternIndex >= _patternLength)
            {
                SelectNextPattern(activeColorCount, targetEncounterInterval);
            }

            float beatMultiplier =
                GetBeatMultiplier(_currentPattern, _patternIndex);
            float timeToGate = GameRules.CalculatePatternEncounterInterval(
                targetEncounterInterval,
                beatMultiplier);
            RunnerColor color = GetPatternColor(
                _currentPattern,
                _patternIndex,
                activeColorCount);
            color = EnforceColorRunLimit(color, activeColorCount);
            GatePlan plan = CreatePlan(
                color,
                movementSpeed,
                timeToGate,
                beatMultiplier,
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
            _previousPattern = GatePatternType.ThirdColorTutorial;
            _patternBeforePrevious = GatePatternType.ThirdColorTutorial;
            _patternIndex = 0;
            _patternLength = 0;
            _generatedGateCount = 0;
            _baseColor = RunnerColor.Red;
            _previousPatternBaseColor = RunnerColor.Green;
            _previousColor = RunnerColor.Red;
            _colorRunLength = 0;
            _hasPattern = false;
            _hasPreviousPatternBaseColor = false;
            _hasPreviousColor = false;
            _shieldPickupGateIndex =
                GameRules.CalculateFirstShieldPickupGateIndex(seed);
        }

        private GatePlan CreateTutorialPlan(
            float movementSpeed,
            float targetEncounterInterval,
            int tutorialIndex)
        {
            RunnerColor color =
                tutorialIndex == 0 ? RunnerColor.Green : RunnerColor.Red;
            float beatMultiplier = tutorialIndex == 0 ? 1.2f : 1.1f;
            float timeToGate = Math.Max(
                1.35f,
                GameRules.CalculatePatternEncounterInterval(
                    targetEncounterInterval,
                    beatMultiplier));
            color = EnforceColorRunLimit(color, 3, true);
            GatePlan plan = CreatePlan(
                color,
                movementSpeed,
                timeToGate,
                beatMultiplier,
                GatePatternType.ThirdColorTutorial,
                tutorialIndex);
            _generatedGateCount++;
            return plan;
        }

        private GatePlan CreatePlan(
            RunnerColor color,
            float movementSpeed,
            float timeToGate,
            float beatMultiplier,
            GatePatternType pattern,
            int indexInPattern)
        {
            float rawSpacing =
                (movementSpeed * timeToGate) + GameRules.GateSafetyMargin;
            float spacing = (float)Math.Ceiling(rawSpacing * 2f) * 0.5f;
            bool hasShieldPickupBefore =
                _generatedGateCount == _shieldPickupGateIndex;
            return new GatePlan(
                color,
                spacing,
                timeToGate,
                beatMultiplier,
                pattern,
                indexInPattern,
                hasShieldPickupBefore);
        }

        private void SelectNextPattern(
            int activeColorCount,
            float targetEncounterInterval)
        {
            int stage = targetEncounterInterval > 1.15f
                ? 0
                : targetEncounterInterval > 0.9f ? 1 : 2;
            int availablePatternCount =
                stage == 0 ? 3 :
                stage == 1 ? 6 :
                activeColorCount == 3 ? 8 : 7;

            GatePatternType selected;
            do
            {
                _state = DeterministicGateSequence.AdvanceXorshift32(_state);
                selected = GetAvailablePattern(
                    stage,
                    activeColorCount,
                    (int)(_state % (uint)availablePatternCount));
            }
            while (selected == _previousPattern ||
                selected == _patternBeforePrevious);

            _state = DeterministicGateSequence.AdvanceXorshift32(_state);
            _baseColor = ColorFromIndex(
                (int)(_state % (uint)activeColorCount));
            if (selected == GatePatternType.SameColorBait &&
                _hasPreviousPatternBaseColor &&
                _baseColor == _previousPatternBaseColor)
            {
                _baseColor = NextColor(_baseColor, activeColorCount);
            }

            _patternBeforePrevious = _previousPattern;
            _previousPattern = selected;
            _previousPatternBaseColor = _baseColor;
            _hasPreviousPatternBaseColor = true;
            _currentPattern = selected;
            _patternIndex = 0;
            _patternLength = GetPatternLength(selected);
            _hasPattern = true;
        }

        private RunnerColor GetPatternColor(
            GatePatternType pattern,
            int index,
            int activeColorCount)
        {
            RunnerColor next = NextColor(_baseColor, activeColorCount);
            RunnerColor third = NextColor(next, activeColorCount);
            switch (pattern)
            {
                case GatePatternType.Release:
                    return index == 2 ? next : _baseColor;
                case GatePatternType.SameColorBait:
                    return index == 3 ? next : _baseColor;
                case GatePatternType.SingleColorBreak:
                    return index > 0 && index < 4 ? next : _baseColor;
                case GatePatternType.Burst:
                    return index == 2 ? _baseColor : next;
                case GatePatternType.ThreeColorFlow:
                    return index % 3 == 0
                        ? _baseColor
                        : index % 3 == 1 ? next : third;
                default:
                    return (index & 1) == 0 ? _baseColor : next;
            }
        }

        private RunnerColor EnforceColorRunLimit(
            RunnerColor color,
            int activeColorCount,
            bool allowTwoStepTransition = false)
        {
            if (_hasPreviousColor &&
                activeColorCount == 3 &&
                !allowTwoStepTransition &&
                color != _previousColor &&
                color != NextColor(_previousColor, activeColorCount))
            {
                color = NextColor(_previousColor, activeColorCount);
            }

            if (_hasPreviousColor &&
                color == _previousColor &&
                _colorRunLength >= GameRules.MaximumConsecutiveGateColors)
            {
                color = NextColor(color, activeColorCount);
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

        private static GatePatternType GetAvailablePattern(
            int stage,
            int activeColorCount,
            int index)
        {
            if (stage == 0)
            {
                return index == 0
                    ? GatePatternType.Steady
                    : index == 1
                        ? GatePatternType.Compression
                        : GatePatternType.Release;
            }

            if (stage == 1)
            {
                switch (index)
                {
                    case 0: return GatePatternType.Steady;
                    case 1: return GatePatternType.Compression;
                    case 2: return GatePatternType.Release;
                    case 3: return GatePatternType.Syncopation;
                    case 4: return GatePatternType.Burst;
                    default: return GatePatternType.SameColorBait;
                }
            }

            switch (index)
            {
                case 0: return GatePatternType.Steady;
                case 1: return GatePatternType.Compression;
                case 2: return GatePatternType.Release;
                case 3: return GatePatternType.Syncopation;
                case 4: return GatePatternType.Burst;
                case 5: return GatePatternType.SameColorBait;
                case 6: return GatePatternType.SingleColorBreak;
                default:
                    return activeColorCount == 3
                        ? GatePatternType.ThreeColorFlow
                        : GatePatternType.Steady;
            }
        }

        private static int GetPatternLength(GatePatternType pattern)
        {
            switch (pattern)
            {
                case GatePatternType.Burst:
                    return 3;
                case GatePatternType.SameColorBait:
                case GatePatternType.SingleColorBreak:
                    return 5;
                default:
                    return 4;
            }
        }

        private static float GetBeatMultiplier(
            GatePatternType pattern,
            int index)
        {
            switch (pattern)
            {
                case GatePatternType.Compression:
                    return index == 0 ? 1.35f :
                        index == 1 ? 1.15f :
                        index == 2 ? 0.95f : 0.75f;
                case GatePatternType.Release:
                    return index == 0 ? 0.75f :
                        index == 1 ? 0.95f :
                        index == 2 ? 1.15f : 1.35f;
                case GatePatternType.Syncopation:
                    return index == 0 ? 1f :
                        index == 1 ? 0.65f :
                        index == 2 ? 1.35f : 1f;
                case GatePatternType.Burst:
                    return index < 2 ? 0.7f : 1.5f;
                case GatePatternType.SameColorBait:
                    return index == 3 ? 0.8f :
                        index == 4 ? 1.2f : 1f;
                case GatePatternType.SingleColorBreak:
                    return index == 4 ? 1.3f : 0.9f;
                case GatePatternType.ThreeColorFlow:
                    return index == 3 ? 1.15f : 0.9f;
                default:
                    return 1f;
            }
        }

        private static RunnerColor ColorFromIndex(int index)
        {
            return index == 0
                ? RunnerColor.Red
                : index == 1 ? RunnerColor.Blue : RunnerColor.Green;
        }

        private static RunnerColor NextColor(
            RunnerColor color,
            int activeColorCount)
        {
            if (color == RunnerColor.Red)
            {
                return RunnerColor.Blue;
            }

            if (color == RunnerColor.Blue)
            {
                return activeColorCount == 3
                    ? RunnerColor.Green
                    : RunnerColor.Red;
            }

            return RunnerColor.Red;
        }
    }
}
