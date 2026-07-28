using System;

namespace ColorGateRunner.Core
{
    public sealed class DeterministicStageGateSequence
    {
        private readonly StageDefinition _stage;
        private uint _state;
        private RunnerColor _previousColor;
        private int _colorRunLength;
        private bool _hasPreviousColor;

        public DeterministicStageGateSequence(StageDefinition stage)
        {
            _stage = stage ?? throw new ArgumentNullException(nameof(stage));
            Reset();
        }

        public GatePlan GetPlan(int gateIndex)
        {
            if (gateIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(gateIndex));
            }

            _state = DeterministicGateSequence.AdvanceXorshift32(_state);
            RunnerColor color = ChooseColor(gateIndex, _state);
            int patternIndex = (int)((_state >> 8) % (uint)_stage.AllowedPatternCount);
            GatePatternType pattern = _stage.GetAllowedPattern(patternIndex);
            float progress = _stage.TargetGateCount <= 1
                ? 1f
                : (float)gateIndex / (_stage.TargetGateCount - 1);
            float cadence = Lerp(_stage.CadenceStart, _stage.CadenceEnd, progress);
            float speed = Lerp(_stage.StartingSpeed, _stage.MaximumSpeed, progress);
            float spacing = speed * cadence;

            return new GatePlan(
                color,
                spacing,
                cadence,
                1f,
                pattern,
                gateIndex,
                false);
        }

        public void Reset()
        {
            _state = DeterministicGateSequence.NormalizeSeed(_stage.Seed);
            _previousColor = RunnerColor.Red;
            _colorRunLength = 0;
            _hasPreviousColor = false;
        }

        private RunnerColor ChooseColor(int gateIndex, uint raw)
        {
            int colorCount = _stage.AllowedColorCount;
            if (_stage.DisplayNumber == 4 && gateIndex < _stage.IntroGateCount)
            {
                colorCount = 2;
            }
            else if (_stage.DisplayNumber == 4 &&
                gateIndex == _stage.IntroGateCount)
            {
                return RecordColor(RunnerColor.Green);
            }

            RunnerColor color = _stage.GetAllowedColor((int)(raw % (uint)colorCount));
            if (_hasPreviousColor &&
                color == _previousColor &&
                _colorRunLength >= GameRules.MaximumConsecutiveGateColors)
            {
                int nextIndex = 0;
                while (_stage.GetAllowedColor(nextIndex) == color)
                {
                    nextIndex++;
                }

                color = _stage.GetAllowedColor(nextIndex);
            }

            return RecordColor(color);
        }

        private RunnerColor RecordColor(RunnerColor color)
        {
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

        private static float Lerp(float start, float end, float amount)
        {
            return start + ((end - start) * amount);
        }
    }
}
