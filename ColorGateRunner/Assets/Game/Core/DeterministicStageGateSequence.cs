using System;

namespace ColorGateRunner.Core
{
    public sealed class DeterministicStageGateSequence
    {
        private static readonly RunnerColor[] StageFourTutorialColors =
        {
            RunnerColor.Red,
            RunnerColor.Blue,
            RunnerColor.Blue,
            RunnerColor.Blue,
            RunnerColor.Blue,
            RunnerColor.Red,
            RunnerColor.Red,
            RunnerColor.Blue
        };

        private readonly StageDefinition _stage;
        private readonly bool[] _hiddenGateMask;
        private readonly bool[] _flickerGateMask;
        private readonly bool[] _iceDoubleTapMask;
        private uint _state;
        private RunnerColor _previousColor;
        private int _colorRunLength;
        private bool _hasPreviousColor;

        public DeterministicStageGateSequence(StageDefinition stage)
        {
            _stage = stage ?? throw new ArgumentNullException(nameof(stage));
            _hiddenGateMask = new bool[_stage.TargetGateCount];
            _flickerGateMask = new bool[_stage.TargetGateCount];
            _iceDoubleTapMask = new bool[_stage.TargetGateCount];
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
            if (_stage.DisplayNumber == 4 &&
                gateIndex <= _stage.GetFirstGateIndex(RunnerColor.Green))
            {
                cadence = _stage.CadenceStart;
            }
            float speed = _stage.SpeedProfile.IsLinear
                ? Lerp(_stage.StartingSpeed, _stage.MaximumSpeed, progress)
                : _stage.GetBaseSpeed(progress);
            GateModifier modifier =
                GateModifierRules.CreateForStageGate(_stage, gateIndex);
            if (gateIndex < _hiddenGateMask.Length &&
                _hiddenGateMask[gateIndex])
            {
                modifier = modifier.With(GateModifierType.Hidden);
            }
            if (gateIndex < _flickerGateMask.Length &&
                _flickerGateMask[gateIndex])
            {
                modifier = modifier.With(GateModifierType.Flicker);
            }
            float spacing = speed * cadence;
            if (modifier.IsIce)
            {
                spacing *= GateModifierRules.IceSpacingMultiplier;
            }
            Cursor++;

            GatePlan plan = new GatePlan(
                gateIndex,
                color,
                spacing,
                cadence,
                1f,
                pattern,
                gateIndex,
                false,
                modifier);
            if (modifier.IsFlicker)
            {
                RunnerColor[] cycleColors =
                    DeterministicModifierPlanner.CreateCycleColors(
                        color,
                        _stage.GetActiveColorCount(gateIndex),
                        next => _stage.GetNextActiveColor(
                            gateIndex,
                            next));
                FlickerGatePlan flickerPlan = new FlickerGatePlan(
                    cycleColors,
                    _stage.FlickerSettings.SwitchIntervalSeconds,
                    DeterministicModifierPlanner.CreatePhaseOffset(
                        _stage.FlickerSettings,
                        _stage.Seed,
                        gateIndex),
                    _stage.FlickerSettings.TransitionPulseSeconds,
                    _stage.Seed);
                plan = plan.WithFlickerPlan(flickerPlan);
            }
            return plan;
        }

        public void Reset()
        {
            _state = DeterministicGateSequence.NormalizeSeed(_stage.Seed);
            _previousColor = RunnerColor.Red;
            _colorRunLength = 0;
            _hasPreviousColor = false;
            Cursor = 0;
            BuildModifierMasks();
        }

        private void BuildModifierMasks()
        {
            Array.Clear(
                _hiddenGateMask,
                0,
                _hiddenGateMask.Length);
            Array.Clear(
                _flickerGateMask,
                0,
                _flickerGateMask.Length);
            Array.Clear(
                _iceDoubleTapMask,
                0,
                _iceDoubleTapMask.Length);

            BuildIceDoubleTapMask();

            HiddenSettings hidden = _stage.HiddenSettings;
            if ((_stage.GateModifiers & GateModifierType.Hidden) != 0 &&
                hidden.Enabled)
            {
                bool[] mask =
                    DeterministicModifierPlanner.BuildOccurrenceMask(
                        _stage.TargetGateCount,
                        hidden.EligibleStartProgress,
                        hidden.EligibleEndProgress,
                        hidden.OccurrenceChance,
                        hidden.MinimumGateCooldown,
                        hidden.MaxOccurrences,
                        hidden.FirstOccurrenceGuaranteed,
                        _stage.Seed,
                        0xF11C4E2Du);
                Array.Copy(mask, _hiddenGateMask, mask.Length);
            }

            FlickerSettings flicker = _stage.FlickerSettings;
            if ((_stage.GateModifiers & GateModifierType.Flicker) != 0 &&
                flicker.Enabled)
            {
                bool[] mask =
                    DeterministicModifierPlanner.BuildOccurrenceMask(
                        _stage.TargetGateCount,
                        flicker.EligibleStartProgress,
                        flicker.EligibleEndProgress,
                        flicker.OccurrenceChance,
                        flicker.MinimumGateCooldown,
                        flicker.MaxOccurrences,
                        flicker.FirstOccurrenceGuaranteed,
                        _stage.Seed,
                        0xC01C1E5Fu,
                        gateIndex => MeetsMinimumVisibleCycles(
                            gateIndex,
                            flicker));
                Array.Copy(mask, _flickerGateMask, mask.Length);
            }
        }

        private void BuildIceDoubleTapMask()
        {
            if ((_stage.GateModifiers & GateModifierType.Ice) == 0)
            {
                return;
            }

            int iceGateCount = 0;
            int[] eligibleGateIndices =
                new int[_stage.TargetGateCount];
            int eligibleGateCount = 0;
            for (int gateIndex = 0;
                gateIndex < _stage.TargetGateCount;
                gateIndex++)
            {
                GateModifier modifier =
                    GateModifierRules.CreateForStageGate(
                        _stage,
                        gateIndex);
                if (!modifier.IsIce)
                {
                    continue;
                }

                iceGateCount++;
                if (_stage.GetActiveColorCount(gateIndex) >= 3)
                {
                    eligibleGateIndices[eligibleGateCount++] = gateIndex;
                }
            }

            int doubleTapCount = 0;
            while (doubleTapCount < eligibleGateCount &&
                (doubleTapCount + 1) / (float)iceGateCount <
                GateModifierRules.MaximumIceDoubleTapRate)
            {
                doubleTapCount++;
            }
            uint selectionState =
                DeterministicGateSequence.NormalizeSeed(
                    _stage.Seed ^ 0x1CE7A92Du);
            for (int index = 0; index < doubleTapCount; index++)
            {
                selectionState =
                    DeterministicGateSequence.AdvanceXorshift32(
                        selectionState);
                int selectionIndex = index +
                    (int)(selectionState %
                        (uint)(eligibleGateCount - index));
                int selectedGate = eligibleGateIndices[selectionIndex];
                eligibleGateIndices[selectionIndex] =
                    eligibleGateIndices[index];
                eligibleGateIndices[index] = selectedGate;
                _iceDoubleTapMask[selectedGate] = true;
            }
        }

        private bool MeetsMinimumVisibleCycles(
            int gateIndex,
            FlickerSettings settings)
        {
            const int visibleGatePoolCount = 6;
            int firstVisibleGate = Math.Max(
                0,
                gateIndex - visibleGatePoolCount + 1);
            float visibleDistance = 0f;
            for (int index = firstVisibleGate;
                index <= gateIndex;
                index++)
            {
                visibleDistance += GetSpacingForEligibility(index);
            }
            float fastestSpeed = _stage.BoosterAllowed
                ? Math.Max(_stage.MaximumSpeed, _stage.BoosterSpeed)
                : _stage.MaximumSpeed;
            float expectedVisibleSeconds =
                GateEtaEstimator.EstimateSeconds(
                    visibleDistance,
                    fastestSpeed);
            return expectedVisibleSeconds >=
                settings.SwitchIntervalSeconds *
                settings.MinimumCyclesVisible;
        }

        private float GetSpacingForEligibility(int gateIndex)
        {
            float progress = _stage.TargetGateCount <= 1
                ? 1f
                : (float)gateIndex / (_stage.TargetGateCount - 1);
            float cadence = Lerp(
                _stage.CadenceStart,
                _stage.CadenceEnd,
                progress);
            StageSectionInfo section =
                StageSectionCatalog.FindSection(
                    _stage.DisplayNumber,
                    gateIndex);
            if (!string.IsNullOrEmpty(section.Name))
            {
                cadence = Math.Max(
                    GameRules.MinimumReactionTime,
                    cadence * section.CadenceMultiplier);
            }
            float speed = _stage.SpeedProfile.IsLinear
                ? Lerp(
                    _stage.StartingSpeed,
                    _stage.MaximumSpeed,
                    progress)
                : _stage.GetBaseSpeed(progress);
            float spacing = speed * cadence;
            GateModifier modifier =
                GateModifierRules.CreateForStageGate(_stage, gateIndex);
            if (modifier.IsIce)
            {
                spacing *= GateModifierRules.IceSpacingMultiplier;
            }
            return spacing;
        }

        private RunnerColor ChooseColor(int gateIndex, uint raw)
        {
            if (_stage.DisplayNumber == 3)
            {
                return RecordColor(ChooseStageThreeColor(gateIndex));
            }

            if (_stage.DisplayNumber == 4 &&
                gateIndex < _stage.GetFirstGateIndex(RunnerColor.Green))
            {
                return RecordColor(ChooseStageFourTutorialColor(gateIndex));
            }
            if (_stage.DisplayNumber == 4 &&
                gateIndex == _stage.GetFirstGateIndex(RunnerColor.Green))
            {
                return RecordColor(RunnerColor.Green);
            }

            if (GateModifierRules.CreateForStageGate(
                    _stage,
                    gateIndex).IsIce)
            {
                return RecordColor(ChooseIceColor(gateIndex));
            }

            int colorCount = _stage.GetGateColorCountAt(gateIndex);
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

        private RunnerColor ChooseIceColor(int gateIndex)
        {
            RunnerColor color = _hasPreviousColor
                ? _previousColor
                : _stage.GetAllowedColor(0);
            int tapCount = _iceDoubleTapMask[gateIndex]
                ? GateModifierRules.MaximumIceTapCount
                : GateModifierRules.PreferredIceTapCount;
            for (int tap = 0; tap < tapCount; tap++)
            {
                color = _stage.GetNextActiveColor(gateIndex, color);
            }

            return color;
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

        private static RunnerColor ChooseStageFourTutorialColor(int gateIndex)
        {
            return StageFourTutorialColors[
                gateIndex % StageFourTutorialColors.Length];
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
