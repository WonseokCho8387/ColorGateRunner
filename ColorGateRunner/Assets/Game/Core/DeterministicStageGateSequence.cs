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

        public int Cursor { get; private set; }

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
            StageSectionInfo section =
                StageSectionCatalog.FindSection(_stage.DisplayNumber, gateIndex);
            if (!string.IsNullOrEmpty(section.Name))
            {
                pattern = section.Pattern;
                cadence = Math.Max(
                    GameRules.MinimumReactionTime,
                    cadence * section.CadenceMultiplier);
            }
            float speed = Lerp(_stage.StartingSpeed, _stage.MaximumSpeed, progress);
            float spacing = speed * cadence;
            Cursor++;

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
            Cursor = 0;
        }

        private RunnerColor ChooseColor(int gateIndex, uint raw)
        {
            if (_stage.DisplayNumber == 3)
            {
                return RecordColor(ChooseStageThreeColor(gateIndex));
            }

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

        private static RunnerColor ChooseStageThreeColor(int gateIndex)
        {
            RunnerColor[] authored =
            {
                RunnerColor.Red, RunnerColor.Red, RunnerColor.Red, RunnerColor.Red,
                RunnerColor.Blue, RunnerColor.Red, RunnerColor.Red, RunnerColor.Red,
                RunnerColor.Blue, RunnerColor.Blue, RunnerColor.Blue, RunnerColor.Red,
                RunnerColor.Blue, RunnerColor.Blue,
                RunnerColor.Red, RunnerColor.Red, RunnerColor.Red, RunnerColor.Red,
                RunnerColor.Blue, RunnerColor.Red, RunnerColor.Red, RunnerColor.Red,
                RunnerColor.Blue, RunnerColor.Blue, RunnerColor.Blue, RunnerColor.Red,
                RunnerColor.Blue, RunnerColor.Blue, RunnerColor.Red, RunnerColor.Blue
            };
            return authored[gateIndex % authored.Length];
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
