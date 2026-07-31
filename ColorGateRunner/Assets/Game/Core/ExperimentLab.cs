using System;
using System.Collections.Generic;

namespace ColorGateRunner.Core
{
    public enum MechanicExperimentType
    {
        None = 0,
        Camouflage = 1,
        Fog = 2,
        Ice = 3,
        Echo = 4,
        Hidden = 5,
        Flicker = 6
    }

    public enum ExperimentRuntimeFailureCause
    {
        None,
        StandardGateMiss
    }

    public enum ExperimentGateResolution
    {
        None,
        PlayerColorMatch,
        EchoColorMatch,
        ShieldDefense,
        BoosterDefense,
        Failure
    }

    public enum ExperimentRisk
    {
        Safe,
        Caution,
        HighRisk
    }

    public readonly struct ExperimentRiskAssessment
    {
        public ExperimentRiskAssessment(
            ExperimentRisk risk,
            bool rapidTapRisk,
            bool controlFatigueRisk,
            bool averageCompletionHardExclusion,
            bool interactionCostDominance,
            bool expertCompletionHardExclusion)
        {
            Risk = risk;
            RapidTapRisk = rapidTapRisk;
            ControlFatigueRisk = controlFatigueRisk;
            AverageCompletionHardExclusion =
                averageCompletionHardExclusion;
            InteractionCostDominance = interactionCostDominance;
            ExpertCompletionHardExclusion =
                expertCompletionHardExclusion;
        }

        public ExperimentRisk Risk { get; }
        public bool RapidTapRisk { get; }
        public bool ControlFatigueRisk { get; }
        public bool AverageCompletionHardExclusion { get; }
        public bool InteractionCostDominance { get; }
        public bool ExpertCompletionHardExclusion { get; }
        public bool HasHardExclusion =>
            AverageCompletionHardExclusion ||
            ExpertCompletionHardExclusion;
        public int CautionFlagCount =>
            (RapidTapRisk ? 1 : 0) +
            (ControlFatigueRisk ? 1 : 0) +
            (InteractionCostDominance ? 1 : 0);
    }

    public enum ExperimentFailureCategory
    {
        None,
        RecognitionDelay,
        FirstTapDelay,
        RepeatedTapBurstTooSlow,
        MissedTap,
        ExtraTap,
        WrongFinalColor,
        MechanicTiming,
        PatternPressure,
        Fatigue
    }

    public sealed class ExperimentDefinition
    {
        private readonly RunnerColor[] _activeColors;

        public ExperimentDefinition(
            int colorCount,
            MechanicExperimentType mechanic,
            uint seed,
            int gateCount = 40,
            EchoSettings echoSettings = null,
            CamouflageSettings camouflageSettings = null,
            HiddenSettings hiddenSettings = null,
            FlickerSettings flickerSettings = null)
        {
            if (colorCount < 3 || colorCount > 6)
            {
                throw new ArgumentOutOfRangeException(nameof(colorCount));
            }
            if (gateCount < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(gateCount));
            }

            ColorCount = colorCount;
            Mechanic = mechanic;
            Seed = seed;
            Id =
                $"step10-{colorCount}-colors-" +
                mechanic.ToString().ToLowerInvariant();
            GateCount = gateCount;
            Echo = mechanic == MechanicExperimentType.Echo
                ? echoSettings ?? EchoSettings.CreateDefault()
                : EchoSettings.Disabled();
            Camouflage = camouflageSettings ??
                CamouflageSettings.CreateDefault();
            Hidden = mechanic == MechanicExperimentType.Hidden
                ? hiddenSettings ?? HiddenSettings.CreateDefault()
                : HiddenSettings.Disabled();
            Flicker = mechanic == MechanicExperimentType.Flicker
                ? flickerSettings ?? FlickerSettings.CreateDefault()
                : FlickerSettings.Disabled();
            if (mechanic == MechanicExperimentType.Flicker)
            {
                Flicker.ValidateForActiveColorCount(colorCount);
            }
            StartingSpeed = 16f;
            MaximumSpeed = 22f;
            CadenceStart = 1.45f;
            CadenceEnd = 0.95f;
            WarmUpGateCount = 8;
            PressureGateCount = 24;
            FinalSectionGateCount = 8;
            MaximumPermittedRequiredTaps = colorCount - 1;
            _activeColors = new RunnerColor[colorCount];
            for (int index = 0; index < colorCount; index++)
            {
                _activeColors[index] = (RunnerColor)index;
            }
        }

        public string Id { get; }
        public int ColorCount { get; }
        public MechanicExperimentType Mechanic { get; }
        public uint Seed { get; }
        public int GateCount { get; }
        public EchoSettings Echo { get; }
        public CamouflageSettings Camouflage { get; }
        public HiddenSettings Hidden { get; }
        public FlickerSettings Flicker { get; }
        public float StartingSpeed { get; }
        public float MaximumSpeed { get; }
        public float CadenceStart { get; }
        public float CadenceEnd { get; }
        public int WarmUpGateCount { get; }
        public int PressureGateCount { get; }
        public int FinalSectionGateCount { get; }
        public int MaximumPermittedRequiredTaps { get; }
        public int IceStartGate => 12;
        public int IceEndGate => 19;
        public float IceSpeedMultiplier => 1.45f;
        public float IceSpacingMultiplier => 1.3f;
        public float IceAccelerationSeconds => 0.3f;
        public float IceDecelerationSeconds => 0.35f;
        public int FogStartGate => 10;
        public int FogEndGate => 21;

        public RunnerColor GetColor(int index)
        {
            if (index < 0 || index >= _activeColors.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return _activeColors[index];
        }

        public RunnerColor[] CopyColors()
        {
            RunnerColor[] result = new RunnerColor[_activeColors.Length];
            Array.Copy(_activeColors, result, result.Length);
            return result;
        }

        public float GetBaseSpeed(int gateIndex)
        {
            ValidateGateIndex(gateIndex);
            float progress = gateIndex /
                (float)Math.Max(1, GateCount - 1);
            return Lerp(StartingSpeed, MaximumSpeed, progress);
        }

        public float GetCadence(int gateIndex)
        {
            ValidateGateIndex(gateIndex);
            float progress = gateIndex /
                (float)Math.Max(1, GateCount - 1);
            return Lerp(CadenceStart, CadenceEnd, progress);
        }

        public float GetSpacing(int gateIndex)
        {
            float spacing = GetBaseSpeed(gateIndex) *
                GetCadence(gateIndex);
            bool ice =
                Mechanic == MechanicExperimentType.Ice &&
                gateIndex >= IceStartGate &&
                gateIndex <= IceEndGate;
            return spacing * (ice ? IceSpacingMultiplier : 1f);
        }

        private void ValidateGateIndex(int gateIndex)
        {
            if (gateIndex < 0 || gateIndex >= GateCount)
            {
                throw new ArgumentOutOfRangeException(nameof(gateIndex));
            }
        }

        private static float Lerp(float start, float end, float amount)
        {
            return start + ((end - start) * amount);
        }
    }

    public static class ExperimentCatalog
    {
        public const uint DefaultSeed = 12345u;

        public static int Count => 16;

        public static ExperimentDefinition Get(
            int colorCount,
            MechanicExperimentType mechanic,
            uint seed = DefaultSeed,
            HiddenSettings hiddenSettings = null,
            FlickerSettings flickerSettings = null)
        {
            return new ExperimentDefinition(
                colorCount,
                mechanic,
                seed,
                hiddenSettings: hiddenSettings,
                flickerSettings: flickerSettings);
        }

        public static ExperimentDefinition GetByIndex(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            int colorCount = 3 + (index % 4);
            MechanicExperimentType mechanic =
                (MechanicExperimentType)(index / 4);
            return Get(colorCount, mechanic);
        }
    }

    public readonly struct ExperimentGatePlan
    {
        private readonly RunnerColor[] _cycleColors;

        public ExperimentGatePlan(
            int gateIndex,
            RunnerColor color,
            int requiredTapCount,
            float baseSpeed,
            float cadence,
            float spacing,
            MechanicExperimentType mechanic,
            GateModifier modifier,
            RunnerColor[] cycleColors = null,
            float switchIntervalSeconds = 0f,
            float phaseOffsetSeconds = 0f,
            float transitionPulseSeconds = 0f,
            uint selectionSeed = 0u)
        {
            GateId = gateIndex;
            GateIndex = gateIndex;
            Color = color;
            RequiredTapCount = requiredTapCount;
            BaseSpeed = baseSpeed;
            Cadence = cadence;
            Spacing = spacing;
            Mechanic = mechanic;
            Modifier = modifier;
            _cycleColors = cycleColors == null
                ? Array.Empty<RunnerColor>()
                : (RunnerColor[])cycleColors.Clone();
            SwitchIntervalSeconds = switchIntervalSeconds;
            PhaseOffsetSeconds = phaseOffsetSeconds;
            TransitionPulseSeconds = transitionPulseSeconds;
            SelectionSeed = selectionSeed;
            if (modifier.IsFlicker)
            {
                if (_cycleColors.Length < 2)
                {
                    throw new ArgumentException(
                        "Flicker plans require at least two cycle colors.",
                        nameof(cycleColors));
                }
                if (switchIntervalSeconds <= 0f)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(switchIntervalSeconds));
                }
                if (phaseOffsetSeconds < 0f ||
                    phaseOffsetSeconds >= switchIntervalSeconds)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(phaseOffsetSeconds));
                }
                if (transitionPulseSeconds < 0f)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(transitionPulseSeconds));
                }
            }
        }

        public int GateId { get; }
        public int GateIndex { get; }
        public int GenerationOrder => GateIndex;
        public RunnerColor Color { get; }
        public int RequiredTapCount { get; }
        public float BaseSpeed { get; }
        public float Cadence { get; }
        public float Spacing { get; }
        public MechanicExperimentType Mechanic { get; }
        public GateModifier Modifier { get; }
        public int CycleColorCount => _cycleColors == null
            ? 0
            : _cycleColors.Length;
        public float SwitchIntervalSeconds { get; }
        public float PhaseOffsetSeconds { get; }
        public float TransitionPulseSeconds { get; }
        public uint SelectionSeed { get; }
        public bool IsCamouflage => Modifier.IsCamouflage;
        public bool IsFog => Modifier.IsFog;
        public bool IsIce => Modifier.IsIce;
        public bool IsEchoProvider => Modifier.IsEchoProvider;
        public bool IsHidden => Modifier.IsHidden;
        public bool IsFlicker => Modifier.IsFlicker;

        public RunnerColor GetCycleColor(int index)
        {
            if (index < 0 || index >= CycleColorCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return _cycleColors[index];
        }

        public RunnerColor[] CopyCycleColors()
        {
            RunnerColor[] result = new RunnerColor[CycleColorCount];
            if (CycleColorCount > 0)
            {
                Array.Copy(_cycleColors, result, CycleColorCount);
            }
            return result;
        }

        public FlickerCycleSample GetFlickerSample(float gameplayTimeSeconds)
        {
            if (!IsFlicker)
            {
                return new FlickerCycleSample(0, 0, 0f);
            }
            return FlickerCycleCalculator.Calculate(
                gameplayTimeSeconds,
                PhaseOffsetSeconds,
                SwitchIntervalSeconds,
                TransitionPulseSeconds,
                CycleColorCount);
        }

        public RunnerColor GetJudgmentColor(float gameplayTimeSeconds)
        {
            if (!IsFlicker)
            {
                return Color;
            }
            FlickerCycleSample sample =
                GetFlickerSample(gameplayTimeSeconds);
            return GetCycleColor(sample.CycleColorIndex);
        }

        public ExperimentGatePlan WithModifier(GateModifier modifier)
        {
            return new ExperimentGatePlan(
                GateIndex,
                Color,
                RequiredTapCount,
                BaseSpeed,
                Cadence,
                Spacing,
                Mechanic,
                modifier,
                _cycleColors,
                SwitchIntervalSeconds,
                PhaseOffsetSeconds,
                TransitionPulseSeconds,
                SelectionSeed);
        }

        public bool ShouldRevealCamouflage(
            float estimatedArrivalSeconds,
            CamouflageSettings settings)
        {
            return !IsCamouflage ||
                GateEtaEstimator.ShouldStartReveal(
                    estimatedArrivalSeconds,
                    settings);
        }

        public bool IsFullyVisibleInFog(int passedGateCount)
        {
            return !IsFog || GateIndex - passedGateCount < 2;
        }
    }

    public sealed class DeterministicExperimentGateSequence
    {
        private const int VisibleGatePoolCount = 6;
        private readonly ExperimentDefinition _definition;
        private readonly bool[] _hiddenGateMask;
        private readonly bool[] _flickerGateMask;
        private uint _state;
        private int _plannedColorIndex;

        public DeterministicExperimentGateSequence(
            ExperimentDefinition definition)
        {
            _definition = definition ??
                throw new ArgumentNullException(nameof(definition));
            _hiddenGateMask = new bool[_definition.GateCount];
            _flickerGateMask = new bool[_definition.GateCount];
            Reset();
        }

        public int Cursor { get; private set; }

        public ExperimentGatePlan GetPlan(int gateIndex)
        {
            if (gateIndex != Cursor || gateIndex < 0 ||
                gateIndex >= _definition.GateCount)
            {
                throw new ArgumentOutOfRangeException(nameof(gateIndex));
            }

            int authoredGateIndex = Cursor;
            _state = DeterministicGateSequence.AdvanceXorshift32(_state);
            int maximumTaps =
                authoredGateIndex < _definition.WarmUpGateCount
                ? 1
                : Math.Min(3, _definition.MaximumPermittedRequiredTaps);
            int taps = (int)(_state % (uint)(maximumTaps + 1));
            if (authoredGateIndex >= _definition.PressureGateCount &&
                authoredGateIndex % 5 == 0)
            {
                taps = _definition.MaximumPermittedRequiredTaps;
            }

            _plannedColorIndex =
                (_plannedColorIndex + taps) % _definition.ColorCount;
            float speed = _definition.GetBaseSpeed(authoredGateIndex);
            float cadence = _definition.GetCadence(authoredGateIndex);
            bool camouflage =
                _definition.Mechanic == MechanicExperimentType.Camouflage &&
                authoredGateIndex >= _definition.WarmUpGateCount &&
                authoredGateIndex % 5 == 2 &&
                (authoredGateIndex >= 20 || taps <= 1);
            bool fog =
                _definition.Mechanic == MechanicExperimentType.Fog &&
                authoredGateIndex >= _definition.FogStartGate &&
                authoredGateIndex <= _definition.FogEndGate;
            bool ice =
                _definition.Mechanic == MechanicExperimentType.Ice &&
                authoredGateIndex >= _definition.IceStartGate &&
                authoredGateIndex <= _definition.IceEndGate;
            float spacing = _definition.GetSpacing(authoredGateIndex);
            GateModifier modifier = GateModifier.None;
            if (camouflage)
            {
                modifier = modifier.With(GateModifierType.Camouflage);
            }
            if (fog)
            {
                modifier = modifier.With(GateModifierType.Fog);
            }
            if (ice)
            {
                modifier = modifier.With(GateModifierType.Ice);
            }
            if (_hiddenGateMask[authoredGateIndex])
            {
                modifier = modifier.With(GateModifierType.Hidden);
            }
            RunnerColor baseColor =
                _definition.GetColor(_plannedColorIndex);
            RunnerColor[] cycleColors = null;
            float phaseOffset = 0f;
            if (_flickerGateMask[authoredGateIndex])
            {
                modifier = modifier.With(GateModifierType.Flicker);
                cycleColors = CreateFlickerCycleColors(
                    authoredGateIndex,
                    baseColor);
                phaseOffset = CreateFlickerPhaseOffset(
                    authoredGateIndex);
            }
            ExperimentGatePlan plan = new ExperimentGatePlan(
                gateIndex,
                baseColor,
                taps,
                speed,
                cadence,
                spacing,
                _definition.Mechanic,
                modifier,
                cycleColors,
                _flickerGateMask[authoredGateIndex]
                    ? _definition.Flicker.SwitchIntervalSeconds
                    : 0f,
                phaseOffset,
                _flickerGateMask[authoredGateIndex]
                    ? _definition.Flicker.TransitionPulseSeconds
                    : 0f,
                _definition.Seed);
            Cursor++;
            return plan;
        }

        public void Reset()
        {
            _state = DeterministicGateSequence.NormalizeSeed(_definition.Seed);
            _plannedColorIndex = 0;
            Cursor = 0;
            BuildHiddenGateMask();
            BuildFlickerGateMask();
        }

        private void BuildHiddenGateMask()
        {
            Array.Clear(
                _hiddenGateMask,
                0,
                _hiddenGateMask.Length);
            HiddenSettings settings = _definition.Hidden;
            if (_definition.Mechanic != MechanicExperimentType.Hidden ||
                !settings.Enabled ||
                settings.MaxOccurrences == 0)
            {
                return;
            }

            uint selectionState = DeterministicGateSequence.NormalizeSeed(
                _definition.Seed ^ 0xF11C4E2Du);
            int selectedCount = 0;
            int lastSelectedGate = int.MinValue;
            for (int gateIndex = 0;
                gateIndex < _definition.GateCount;
                gateIndex++)
            {
                float progress = gateIndex /
                    (float)Math.Max(1, _definition.GateCount - 1);
                if (progress < settings.EligibleStartProgress ||
                    progress > settings.EligibleEndProgress)
                {
                    continue;
                }
                if (lastSelectedGate != int.MinValue &&
                    gateIndex - lastSelectedGate <=
                    settings.MinimumGateCooldown)
                {
                    continue;
                }

                selectionState =
                    DeterministicGateSequence.AdvanceXorshift32(
                        selectionState);
                bool guaranteed =
                    settings.FirstOccurrenceGuaranteed &&
                    selectedCount == 0;
                float roll =
                    (selectionState & 0x00FFFFFFu) / 16777216f;
                if (!guaranteed && roll >= settings.OccurrenceChance)
                {
                    continue;
                }

                _hiddenGateMask[gateIndex] = true;
                selectedCount++;
                lastSelectedGate = gateIndex;
                if (selectedCount >= settings.MaxOccurrences)
                {
                    break;
                }
            }
        }

        private void BuildFlickerGateMask()
        {
            Array.Clear(
                _flickerGateMask,
                0,
                _flickerGateMask.Length);
            FlickerSettings settings = _definition.Flicker;
            if (_definition.Mechanic != MechanicExperimentType.Flicker ||
                !settings.Enabled ||
                settings.MaxOccurrences == 0)
            {
                return;
            }

            uint selectionState = DeterministicGateSequence.NormalizeSeed(
                _definition.Seed ^ 0xC01C1E5Fu);
            int selectedCount = 0;
            int lastSelectedGate = int.MinValue;
            for (int gateIndex = 0;
                gateIndex < _definition.GateCount;
                gateIndex++)
            {
                float progress = gateIndex /
                    (float)Math.Max(1, _definition.GateCount - 1);
                if (progress < settings.EligibleStartProgress ||
                    progress > settings.EligibleEndProgress ||
                    !MeetsMinimumVisibleCycles(gateIndex, settings))
                {
                    continue;
                }
                if (lastSelectedGate != int.MinValue &&
                    gateIndex - lastSelectedGate <=
                    settings.MinimumGateCooldown)
                {
                    continue;
                }

                selectionState =
                    DeterministicGateSequence.AdvanceXorshift32(
                        selectionState);
                bool guaranteed =
                    settings.FirstOccurrenceGuaranteed &&
                    selectedCount == 0;
                float roll =
                    (selectionState & 0x00FFFFFFu) / 16777216f;
                if (!guaranteed && roll >= settings.OccurrenceChance)
                {
                    continue;
                }

                _flickerGateMask[gateIndex] = true;
                selectedCount++;
                lastSelectedGate = gateIndex;
                if (selectedCount >= settings.MaxOccurrences)
                {
                    break;
                }
            }
        }

        private bool MeetsMinimumVisibleCycles(
            int gateIndex,
            FlickerSettings settings)
        {
            int firstVisibleGate = Math.Max(
                0,
                gateIndex - VisibleGatePoolCount + 1);
            float visibleDistance = 0f;
            for (int index = firstVisibleGate;
                index <= gateIndex;
                index++)
            {
                visibleDistance += _definition.GetSpacing(index);
            }
            float expectedVisibleSeconds =
                GateEtaEstimator.EstimateSeconds(
                    visibleDistance,
                    _definition.MaximumSpeed);
            return expectedVisibleSeconds >=
                settings.SwitchIntervalSeconds *
                settings.MinimumCyclesVisible;
        }

        private RunnerColor[] CreateFlickerCycleColors(
            int gateId,
            RunnerColor baseColor)
        {
            int count = _definition.Flicker.CycleColorCount;
            RunnerColor[] result = new RunnerColor[count];
            result[0] = baseColor;
            RunnerColor[] candidates =
                new RunnerColor[_definition.ColorCount - 1];
            int candidateCount = 0;
            for (int index = 0;
                index < _definition.ColorCount;
                index++)
            {
                RunnerColor candidate = _definition.GetColor(index);
                if (candidate != baseColor)
                {
                    candidates[candidateCount++] = candidate;
                }
            }

            uint state = DeterministicGateSequence.NormalizeSeed(
                _definition.Seed ^
                ((uint)(gateId + 1) * 0x9E3779B9u) ^
                0xC1C1E123u);
            for (int index = 1; index < count; index++)
            {
                state = DeterministicGateSequence.AdvanceXorshift32(state);
                int selected = (int)(state % (uint)candidateCount);
                result[index] = candidates[selected];
                candidateCount--;
                candidates[selected] = candidates[candidateCount];
            }
            return result;
        }

        private float CreateFlickerPhaseOffset(int gateId)
        {
            FlickerSettings settings = _definition.Flicker;
            if (!settings.RandomizePhaseOffset)
            {
                return 0f;
            }
            uint state = DeterministicGateSequence.NormalizeSeed(
                _definition.Seed ^
                ((uint)(gateId + 1) * 0x85EBCA6Bu) ^
                0xF1A5E0FFu);
            state = DeterministicGateSequence.AdvanceXorshift32(state);
            float unit = (state & 0x00FFFFFFu) / 16777216f;
            return unit * settings.SwitchIntervalSeconds;
        }
    }

    public sealed class ExperimentSession
    {
        private readonly DeterministicExperimentGateSequence _sequence;
        private readonly EchoOfferCoordinator _echoCoordinator;
        private bool _shieldActive;
        private float _boosterDistanceRemaining;

        public ExperimentSession(ExperimentDefinition definition)
            : this(definition, new StartItemSelection(false, false))
        {
        }

        public ExperimentSession(
            ExperimentDefinition definition,
            StartItemSelection items)
        {
            Definition = definition ??
                throw new ArgumentNullException(nameof(definition));
            _sequence = new DeterministicExperimentGateSequence(definition);
            _echoCoordinator = new EchoOfferCoordinator(
                definition.Echo,
                definition.Seed,
                definition.GateCount);
            Items = items;
            Restart();
        }

        public ExperimentDefinition Definition { get; }
        public RunnerColor CurrentColor { get; private set; }
        public int GatesPassed { get; private set; }
        public float CurrentSpeed { get; private set; }
        public float ElapsedPlayingSeconds { get; private set; }
        public StartItemSelection Items { get; }
        public bool ShieldActive => _shieldActive;
        public float BoosterDistanceRemaining => _boosterDistanceRemaining;
        public bool BoosterActive => _boosterDistanceRemaining > 0f;
        public StageFlowState FlowState { get; private set; }
        public bool Failed => FlowState == StageFlowState.Failed;
        public bool Completed => FlowState == StageFlowState.StageCleared;
        public int SequenceCursor => _sequence.Cursor;
        public ExperimentRuntimeFailureCause LastFailureCause { get; private set; }
        public ExperimentGateResolution LastResolution { get; private set; }
        public bool EchoActive => _echoCoordinator.EchoActive;
        public RunnerColor EchoColor => _echoCoordinator.EchoColor;
        public int EchoAcquisitionCount =>
            _echoCoordinator.EchoAcquisitionCount;
        public int EchoCooldownRemaining =>
            _echoCoordinator.EchoCooldownRemaining;
        public bool EchoOfferPending =>
            _echoCoordinator.EchoOfferPending;
        public int ActiveEchoOfferGateId =>
            _echoCoordinator.ActiveEchoOfferGateId;
        public RunnerColor LastJudgmentColor { get; private set; }
        public bool LastJudgedGateWasFlicker { get; private set; }

        public bool CompleteCountdown()
        {
            if (FlowState != StageFlowState.Countdown)
            {
                return false;
            }

            FlowState = StageFlowState.Playing;
            _shieldActive = Items.Shield;
            _boosterDistanceRemaining = Items.Booster
                ? ExperimentItemRules.BoosterDistance
                : 0f;
            UpdateSpeed();
            return true;
        }

        public bool TryCycleColor()
        {
            if (FlowState != StageFlowState.Playing)
            {
                return false;
            }
            int current = 0;
            while (Definition.GetColor(current) != CurrentColor)
            {
                current++;
            }
            CurrentColor = Definition.GetColor(
                (current + 1) % Definition.ColorCount);
            return true;
        }

        public ExperimentGatePlan GetNextPlan()
        {
            return GetPlan(GatesPassed);
        }

        public ExperimentGatePlan GetPlan(int gateIndex)
        {
            ExperimentGatePlan plan = _sequence.GetPlan(gateIndex);
            if (Definition.Mechanic != MechanicExperimentType.Echo)
            {
                return plan;
            }
            float progress = plan.GateIndex /
                (float)Math.Max(1, Definition.GateCount - 1);
            GateModifier modifier = _echoCoordinator.RegisterGate(
                plan.GateId,
                progress,
                false,
                plan.Modifier);
            return plan.WithModifier(modifier);
        }

        public bool Resolve(ExperimentGatePlan plan)
        {
            return Resolve(plan, ElapsedPlayingSeconds);
        }

        public bool Resolve(
            ExperimentGatePlan plan,
            float gameplayTimeSeconds)
        {
            if (FlowState != StageFlowState.Playing ||
                plan.GateIndex != GatesPassed)
            {
                return false;
            }
            RunnerColor judgmentColor =
                plan.GetJudgmentColor(gameplayTimeSeconds);
            LastJudgmentColor = judgmentColor;
            LastJudgedGateWasFlicker = plan.IsFlicker;
            bool playerMatch = CurrentColor == judgmentColor;
            if (playerMatch)
            {
                LastResolution = ExperimentGateResolution.PlayerColorMatch;
                if (plan.IsEchoProvider)
                {
                    _echoCoordinator.TryAcquire(
                        plan.GateId,
                        judgmentColor,
                        true);
                }
            }
            else if (_echoCoordinator.TryConsume(judgmentColor))
            {
                LastResolution = ExperimentGateResolution.EchoColorMatch;
            }
            else if (_boosterDistanceRemaining > 0f)
            {
                LastResolution = ExperimentGateResolution.BoosterDefense;
            }
            else if (_shieldActive)
            {
                _shieldActive = false;
                LastResolution = ExperimentGateResolution.ShieldDefense;
            }
            else
            {
                LastResolution = ExperimentGateResolution.Failure;
                LastFailureCause =
                    ExperimentRuntimeFailureCause.StandardGateMiss;
                _echoCoordinator.OnGateResolved(plan.GateId);
                FlowState = StageFlowState.Failed;
                CurrentSpeed = 0f;
                return false;
            }
            _echoCoordinator.OnGateResolved(plan.GateId);
            GatesPassed++;
            ExperimentItemRules.AdvanceDistance(
                ref _boosterDistanceRemaining,
                plan.Spacing);
            UpdateSpeed();
            if (GatesPassed >= Definition.GateCount)
            {
                FlowState = StageFlowState.StageCleared;
                CurrentSpeed = 0f;
            }
            return true;
        }

        public float GetSpeedForPlan(ExperimentGatePlan plan)
        {
            if (FlowState != StageFlowState.Playing)
            {
                return 0f;
            }
            if (BoosterActive)
            {
                return ExperimentItemRules.BoosterSpeed;
            }
            return CurrentSpeed *
                (plan.IsIce ? Definition.IceSpeedMultiplier : 1f);
        }

        public void Advance(float deltaSeconds)
        {
            if (deltaSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            }
            if (FlowState == StageFlowState.Playing)
            {
                ElapsedPlayingSeconds += deltaSeconds;
            }
        }

        public void Restart()
        {
            _sequence.Reset();
            _echoCoordinator.Restart();
            CurrentColor = Definition.GetColor(0);
            GatesPassed = 0;
            CurrentSpeed = Definition.StartingSpeed;
            ElapsedPlayingSeconds = 0f;
            _shieldActive = false;
            _boosterDistanceRemaining = 0f;
            LastFailureCause = ExperimentRuntimeFailureCause.None;
            LastResolution = ExperimentGateResolution.None;
            LastJudgmentColor = Definition.GetColor(0);
            LastJudgedGateWasFlicker = false;
            FlowState = StageFlowState.Countdown;
        }

        private void UpdateSpeed()
        {
            float progress = (float)GatesPassed / Definition.GateCount;
            CurrentSpeed = Definition.StartingSpeed +
                ((Definition.MaximumSpeed - Definition.StartingSpeed) *
                progress);
        }
    }

    public static class ExperimentItemRules
    {
        public const float BoosterDistance = 160f;
        public const float BoosterSpeed = 44f;

        public static bool TryProtectMismatch(
            ref bool shieldActive,
            float boosterDistanceRemaining)
        {
            if (boosterDistanceRemaining > 0f)
            {
                return true;
            }
            if (!shieldActive)
            {
                return false;
            }
            shieldActive = false;
            return true;
        }

        public static void AdvanceDistance(
            ref float boosterDistanceRemaining,
            float distance)
        {
            boosterDistanceRemaining = Math.Max(
                0f,
                boosterDistanceRemaining - Math.Max(0f, distance));
        }
    }

    public readonly struct TapWindowMetrics
    {
        public TapWindowMetrics(
            float recognitionStartTime,
            float gateArrivalTime,
            float firstTapAvailableTime,
            float availableRepeatedTapWindow,
            float requiredTapInterval,
            float finalTapCompletionTime,
            float postTapMargin)
        {
            RecognitionStartTime = recognitionStartTime;
            GateArrivalTime = gateArrivalTime;
            FirstTapAvailableTime = firstTapAvailableTime;
            AvailableRepeatedTapWindow = availableRepeatedTapWindow;
            RequiredTapInterval = requiredTapInterval;
            FinalTapCompletionTime = finalTapCompletionTime;
            PostTapMargin = postTapMargin;
        }

        public float RecognitionStartTime { get; }
        public float GateArrivalTime { get; }
        public float FirstTapAvailableTime { get; }
        public float AvailableRepeatedTapWindow { get; }
        public float RequiredTapInterval { get; }
        public float FinalTapCompletionTime { get; }
        public float PostTapMargin { get; }

        public static TapWindowMetrics Calculate(
            int requiredTapCount,
            float recognitionStartTime,
            float gateArrivalTime,
            float firstTapReaction,
            float repeatedTapInterval,
            float confirmationTime,
            float safetyMargin)
        {
            if (requiredTapCount < 0 || gateArrivalTime < recognitionStartTime ||
                firstTapReaction < 0f || repeatedTapInterval < 0f ||
                confirmationTime < 0f || safetyMargin < 0f)
            {
                throw new ArgumentOutOfRangeException();
            }
            float first = recognitionStartTime + firstTapReaction;
            float available = Math.Max(
                0f,
                gateArrivalTime - first - confirmationTime - safetyMargin);
            float requiredInterval = requiredTapCount == 0
                ? float.PositiveInfinity
                : available / requiredTapCount;
            float final = requiredTapCount == 0
                ? recognitionStartTime
                : first + ((requiredTapCount - 1) * repeatedTapInterval);
            return new TapWindowMetrics(
                recognitionStartTime,
                gateArrivalTime,
                first,
                available,
                requiredInterval,
                final,
                gateArrivalTime - final - confirmationTime);
        }
    }

    public sealed class ExperimentSimulationResult
    {
        public string ExperimentId;
        public int ColorCount;
        public MechanicExperimentType Mechanic;
        public SimulatedPlayerKind Profile;
        public bool Shield;
        public bool Booster;
        public int RunCount;
        public int CompletedCount;
        public float CompletionRate;
        public float MedianCompletionTime;
        public float AverageRequiredTaps;
        public int MaximumRequiredTaps;
        public readonly int[] TapDistribution = new int[5];
        public float AverageRequiredTapInterval;
        public float P10RequiredTapInterval;
        public float MinimumRequiredTapInterval;
        public float PeakRequiredTapsPerSecond;
        public int LongestTapBurst;
        public readonly int[] FailureCounts =
            new int[Enum.GetValues(typeof(ExperimentFailureCategory)).Length];
        public float FinalSectionReachRate;
        public float CamouflageAverageRevealMargin;
        public float FogAverageVisibleGateCount;
        public float IceAverageReactionMargin;
        public float FirstThreeAfterEntryFailureRate;
        public float FirstThreeAfterExitFailureRate;
        public float MechanicTimingFailureRate;
        public ExperimentRisk Risk;
    }

    public sealed class ExperimentSimulationBatch
    {
        public readonly List<ExperimentSimulationResult> Results =
            new List<ExperimentSimulationResult>();
        public int TotalRuns;
        public readonly List<ExperimentSimulationResult> CandidateItemResults =
            new List<ExperimentSimulationResult>();
        public int CandidateItemRuns;
    }

    public static class ExperimentSimulationRunner
    {
        public static ExperimentSimulationBatch RunFullMatrix(
            int stochasticRuns)
        {
            if (stochasticRuns < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(stochasticRuns));
            }
            ExperimentSimulationBatch batch = new ExperimentSimulationBatch();
            for (int condition = 0;
                condition < ExperimentCatalog.Count;
                condition++)
            {
                ExperimentDefinition definition =
                    ExperimentCatalog.GetByIndex(condition);
                for (int profileIndex = 0; profileIndex < 5; profileIndex++)
                {
                    SimulatedPlayerKind kind =
                        (SimulatedPlayerKind)profileIndex;
                    int runs = kind == SimulatedPlayerKind.Perfect
                        ? 1
                        : stochasticRuns;
                    ExperimentSimulationResult result = Run(
                        definition,
                        SimulatedPlayerProfile.Get(kind),
                        runs,
                        definition.Seed + (uint)(profileIndex * 100003));
                    batch.Results.Add(result);
                    batch.TotalRuns += runs;
                }
            }
            ApplyConditionRisks(batch);
            return batch;
        }

        public static ExperimentSimulationResult Run(
            ExperimentDefinition definition,
            SimulatedPlayerProfile profile,
            int runs,
            uint seed)
        {
            return Run(
                definition,
                profile,
                runs,
                seed,
                new StartItemSelection(false, false));
        }

        public static ExperimentSimulationResult Run(
            ExperimentDefinition definition,
            SimulatedPlayerProfile profile,
            int runs,
            uint seed,
            StartItemSelection items)
        {
            if (runs < 1 || !profile.IsValid())
            {
                throw new ArgumentOutOfRangeException(nameof(runs));
            }
            ExperimentSimulationResult result =
                CreateResult(definition, profile, runs, items);
            List<float> completionTimes = new List<float>(runs);
            List<float> intervals =
                new List<float>(runs * definition.GateCount);
            float totalTaps = 0f;
            float totalRevealMargin = 0f;
            int revealSamples = 0;
            float totalVisible = 0f;
            int fogSamples = 0;
            float totalIceMargin = 0f;
            int iceSamples = 0;
            int finalReached = 0;
            int entryOpportunities = 0;
            int entryFailures = 0;
            int exitOpportunities = 0;
            int exitFailures = 0;
            int mechanicFailures = 0;

            for (int run = 0; run < runs; run++)
            {
                uint random = DeterministicGateSequence.NormalizeSeed(
                    seed + (uint)(run * 7919));
                DeterministicExperimentGateSequence sequence =
                    new DeterministicExperimentGateSequence(definition);
                float time = 0f;
                bool failed = false;
                RunnerColor simulatedColor = definition.GetColor(0);
                bool shieldActive = items.Shield;
                float boosterDistanceRemaining = items.Booster
                    ? ExperimentItemRules.BoosterDistance
                    : 0f;
                for (int gate = 0; gate < definition.GateCount; gate++)
                {
                    ExperimentGatePlan plan = sequence.GetPlan(gate);
                    if (gate >= definition.GateCount -
                        definition.FinalSectionGateCount)
                    {
                        finalReached++;
                    }
                    float speed = Lerp(
                        definition.StartingSpeed,
                        definition.MaximumSpeed,
                        (float)gate / Math.Max(1, definition.GateCount - 1));
                    if (plan.IsIce)
                    {
                        speed *= definition.IceSpeedMultiplier;
                    }
                    float arrival = plan.Spacing / speed;
                    float recognitionStart = 0f;
                    if (plan.IsCamouflage)
                    {
                        arrival += plan.Cadence;
                        totalRevealMargin += arrival - recognitionStart;
                        revealSamples++;
                    }
                    if (plan.IsFog)
                    {
                        totalVisible += 2f;
                        fogSamples++;
                    }
                    RunnerColor judgmentColor =
                        plan.GetJudgmentColor(time + arrival);
                    int requiredTapCount = CalculateRequiredTaps(
                        definition,
                        simulatedColor,
                        judgmentColor);
                    float firstReaction = Math.Max(
                        0f,
                        profile.FirstTapReactionTimeMean +
                        (NextSigned(ref random) *
                        profile.FirstTapReactionTimeVariance));
                    float repeat = Math.Max(
                        0f,
                        profile.RepeatedTapIntervalMean +
                        (NextSigned(ref random) *
                        profile.RepeatedTapIntervalVariance));
                    TapWindowMetrics metric = TapWindowMetrics.Calculate(
                        requiredTapCount,
                        recognitionStart,
                        arrival,
                        firstReaction,
                        repeat,
                        profile.PostTapConfirmationTime,
                        0.08f);
                    if (!float.IsPositiveInfinity(metric.RequiredTapInterval))
                    {
                        intervals.Add(metric.RequiredTapInterval);
                    }
                    if (plan.IsIce)
                    {
                        totalIceMargin += metric.PostTapMargin;
                        iceSamples++;
                    }
                    totalTaps += requiredTapCount;
                    result.MaximumRequiredTaps = Math.Max(
                        result.MaximumRequiredTaps,
                        requiredTapCount);
                    result.LongestTapBurst = Math.Max(
                        result.LongestTapBurst,
                        requiredTapCount);
                    result.TapDistribution[
                        Math.Min(4, requiredTapCount)]++;
                    result.PeakRequiredTapsPerSecond = Math.Max(
                        result.PeakRequiredTapsPerSecond,
                        arrival <= 0f ? 0f : requiredTapCount / arrival);

                    ExperimentFailureCategory failure = DetermineFailure(
                        profile,
                        plan,
                        requiredTapCount,
                        metric,
                        gate,
                        ref random);
                    bool entryWindow = IsEntryWindow(definition, gate);
                    bool exitWindow = IsExitWindow(definition, gate);
                    entryOpportunities += entryWindow ? 1 : 0;
                    exitOpportunities += exitWindow ? 1 : 0;
                    if (failure != ExperimentFailureCategory.None &&
                        ExperimentItemRules.TryProtectMismatch(
                            ref shieldActive,
                            boosterDistanceRemaining))
                    {
                        failure = ExperimentFailureCategory.None;
                    }
                    ExperimentItemRules.AdvanceDistance(
                        ref boosterDistanceRemaining,
                        plan.Spacing);
                    time += arrival;
                    simulatedColor = judgmentColor;
                    if (failure != ExperimentFailureCategory.None)
                    {
                        entryFailures += entryWindow ? 1 : 0;
                        exitFailures += exitWindow ? 1 : 0;
                        if ((plan.IsCamouflage || plan.IsFog || plan.IsIce) &&
                            (failure ==
                            ExperimentFailureCategory.RecognitionDelay ||
                            failure ==
                            ExperimentFailureCategory.RepeatedTapBurstTooSlow))
                        {
                            mechanicFailures++;
                        }
                        result.FailureCounts[(int)failure]++;
                        failed = true;
                        break;
                    }
                }
                if (!failed)
                {
                    result.CompletedCount++;
                    completionTimes.Add(time);
                }
            }

            result.CompletionRate = (float)result.CompletedCount / runs;
            result.MedianCompletionTime = Percentile(completionTimes, 0.5f);
            result.AverageRequiredTaps =
                totalTaps / Math.Max(1, runs * definition.GateCount);
            result.AverageRequiredTapInterval = Average(intervals);
            result.MinimumRequiredTapInterval = Minimum(intervals);
            result.P10RequiredTapInterval = Percentile(intervals, 0.1f);
            result.FinalSectionReachRate = (float)finalReached /
                Math.Max(1, runs * definition.FinalSectionGateCount);
            result.CamouflageAverageRevealMargin =
                totalRevealMargin / Math.Max(1, revealSamples);
            result.FogAverageVisibleGateCount =
                totalVisible / Math.Max(1, fogSamples);
            result.IceAverageReactionMargin =
                totalIceMargin / Math.Max(1, iceSamples);
            result.FirstThreeAfterEntryFailureRate =
                (float)entryFailures / Math.Max(1, entryOpportunities);
            result.FirstThreeAfterExitFailureRate =
                (float)exitFailures / Math.Max(1, exitOpportunities);
            result.MechanicTimingFailureRate =
                (float)mechanicFailures / Math.Max(1, runs);
            result.Risk = Classify(result);
            return result;
        }

        public static void RunCandidateItemMatrix(
            ExperimentSimulationBatch batch,
            int stochasticRuns)
        {
            string[] shortlist = ExperimentShortlist.Select(batch);
            StartItemSelection[] items =
            {
                new StartItemSelection(true, false),
                new StartItemSelection(false, true),
                new StartItemSelection(true, true)
            };
            for (int candidate = 0;
                candidate < shortlist.Length;
                candidate++)
            {
                ExperimentDefinition definition =
                    FindDefinition(shortlist[candidate]);
                for (int item = 0; item < items.Length; item++)
                {
                    for (int profileIndex = 0;
                        profileIndex < 5;
                        profileIndex++)
                    {
                        SimulatedPlayerKind kind =
                            (SimulatedPlayerKind)profileIndex;
                        int runs = kind == SimulatedPlayerKind.Perfect
                            ? 1
                            : stochasticRuns;
                        batch.CandidateItemResults.Add(Run(
                            definition,
                            SimulatedPlayerProfile.Get(kind),
                            runs,
                            definition.Seed +
                                (uint)(profileIndex * 100003 + item * 170003),
                            items[item]));
                        batch.CandidateItemRuns += runs;
                    }
                }
            }
        }

        public static ExperimentRisk Classify(
            ExperimentSimulationResult result)
        {
            bool averageHardExclusion =
                result.Profile == SimulatedPlayerKind.Average &&
                result.CompletionRate < 0.2f;
            bool expertHardExclusion =
                result.Profile == SimulatedPlayerKind.Expert &&
                result.CompletionRate < 0.5f;
            return ResolveRisk(
                averageHardExclusion || expertHardExclusion,
                CountCautionFlags(result));
        }

        public static ExperimentRiskAssessment AssessCondition(
            ExperimentSimulationResult average,
            ExperimentSimulationResult expert)
        {
            if (average == null)
            {
                throw new ArgumentNullException(nameof(average));
            }
            if (expert == null)
            {
                throw new ArgumentNullException(nameof(expert));
            }
            if (average.Profile != SimulatedPlayerKind.Average)
            {
                throw new ArgumentException(
                    "The condition assessment requires an Average result.",
                    nameof(average));
            }
            if (expert.Profile != SimulatedPlayerKind.Expert)
            {
                throw new ArgumentException(
                    "The condition assessment requires an Expert result.",
                    nameof(expert));
            }

            bool rapidTapRisk =
                average.P10RequiredTapInterval < 0.14f;
            bool controlFatigueRisk =
                CalculateHighTapShare(average) > 0.2f;
            bool averageCompletionHardExclusion =
                average.CompletionRate < 0.2f;
            bool interactionCostDominance =
                CalculateBurstFailureShare(average) > 0.5f;
            bool expertCompletionHardExclusion =
                expert.CompletionRate < 0.5f;
            int cautionFlags =
                (rapidTapRisk ? 1 : 0) +
                (controlFatigueRisk ? 1 : 0) +
                (interactionCostDominance ? 1 : 0);
            ExperimentRisk risk = ResolveRisk(
                averageCompletionHardExclusion ||
                expertCompletionHardExclusion,
                cautionFlags);
            return new ExperimentRiskAssessment(
                risk,
                rapidTapRisk,
                controlFatigueRisk,
                averageCompletionHardExclusion,
                interactionCostDominance,
                expertCompletionHardExclusion);
        }

        public static void ApplyConditionRisks(
            ExperimentSimulationBatch batch)
        {
            if (batch == null)
            {
                throw new ArgumentNullException(nameof(batch));
            }
            for (int condition = 0;
                condition < ExperimentCatalog.Count;
                condition++)
            {
                string experimentId =
                    ExperimentCatalog.GetByIndex(condition).Id;
                ExperimentSimulationResult average = null;
                ExperimentSimulationResult expert = null;
                for (int index = 0; index < batch.Results.Count; index++)
                {
                    ExperimentSimulationResult result =
                        batch.Results[index];
                    if (result.ExperimentId != experimentId)
                    {
                        continue;
                    }
                    if (result.Profile == SimulatedPlayerKind.Average)
                    {
                        average = result;
                    }
                    else if (result.Profile == SimulatedPlayerKind.Expert)
                    {
                        expert = result;
                    }
                }
                ExperimentRisk risk =
                    AssessCondition(average, expert).Risk;
                for (int index = 0; index < batch.Results.Count; index++)
                {
                    if (batch.Results[index].ExperimentId == experimentId)
                    {
                        batch.Results[index].Risk = risk;
                    }
                }
            }
        }

        private static int CountCautionFlags(
            ExperimentSimulationResult result)
        {
            int flags = 0;
            flags += result.P10RequiredTapInterval < 0.14f ? 1 : 0;
            flags += CalculateHighTapShare(result) > 0.2f ? 1 : 0;
            flags += CalculateBurstFailureShare(result) > 0.5f ? 1 : 0;
            return flags;
        }

        private static float CalculateHighTapShare(
            ExperimentSimulationResult result)
        {
            int highTapGates = 0;
            int totalGates = 0;
            for (int index = 0; index < result.TapDistribution.Length; index++)
            {
                totalGates += result.TapDistribution[index];
                if (index >= 3)
                {
                    highTapGates += result.TapDistribution[index];
                }
            }
            return (float)highTapGates / Math.Max(1, totalGates);
        }

        private static float CalculateBurstFailureShare(
            ExperimentSimulationResult result)
        {
            int failures = 0;
            for (int index = 1; index < result.FailureCounts.Length; index++)
            {
                failures += result.FailureCounts[index];
            }
            return failures == 0
                ? 0f
                : (float)result.FailureCounts[
                    (int)ExperimentFailureCategory.RepeatedTapBurstTooSlow] /
                    failures;
        }

        private static ExperimentRisk ResolveRisk(
            bool hardExclusion,
            int cautionFlags)
        {
            if (hardExclusion || cautionFlags >= 2)
            {
                return ExperimentRisk.HighRisk;
            }
            return cautionFlags == 1
                ? ExperimentRisk.Caution
                : ExperimentRisk.Safe;
        }

        private static ExperimentSimulationResult CreateResult(
            ExperimentDefinition definition,
            SimulatedPlayerProfile profile,
            int runs,
            StartItemSelection items)
        {
            return new ExperimentSimulationResult
            {
                ExperimentId = definition.Id,
                ColorCount = definition.ColorCount,
                Mechanic = definition.Mechanic,
                Profile = profile.Kind,
                RunCount = runs,
                Shield = items.Shield,
                Booster = items.Booster
            };
        }

        private static ExperimentDefinition FindDefinition(string id)
        {
            for (int index = 0; index < ExperimentCatalog.Count; index++)
            {
                ExperimentDefinition definition =
                    ExperimentCatalog.GetByIndex(index);
                if (definition.Id == id)
                {
                    return definition;
                }
            }
            throw new ArgumentException("Unknown experiment ID.", nameof(id));
        }

        private static ExperimentFailureCategory DetermineFailure(
            SimulatedPlayerProfile profile,
            ExperimentGatePlan plan,
            int requiredTapCount,
            TapWindowMetrics metric,
            int gate,
            ref uint random)
        {
            if (profile.Kind == SimulatedPlayerKind.Perfect)
            {
                return ExperimentFailureCategory.None;
            }
            if (metric.FirstTapAvailableTime >= metric.GateArrivalTime &&
                requiredTapCount > 0)
            {
                return plan.IsCamouflage
                    ? ExperimentFailureCategory.RecognitionDelay
                    : ExperimentFailureCategory.FirstTapDelay;
            }
            if (metric.PostTapMargin < 0f)
            {
                return ExperimentFailureCategory.RepeatedTapBurstTooSlow;
            }
            float fatigue = gate * profile.FatiguePenalty * 0.001f;
            int excess = Math.Max(
                0,
                requiredTapCount - profile.MaximumComfortableTapBurst);
            float burst = excess * profile.BurstErrorGrowth;
            float roll = Next01(ref random);
            if (roll < profile.BaseMissChance + burst)
            {
                return excess > 0
                    ? ExperimentFailureCategory.RepeatedTapBurstTooSlow
                    : ExperimentFailureCategory.MissedTap;
            }
            if (roll < profile.BaseMissChance + burst +
                profile.WrongInputChance)
            {
                return ExperimentFailureCategory.ExtraTap;
            }
            if (roll < profile.BaseMissChance + burst +
                profile.WrongInputChance + fatigue)
            {
                return ExperimentFailureCategory.Fatigue;
            }
            return ExperimentFailureCategory.None;
        }

        private static int CalculateRequiredTaps(
            ExperimentDefinition definition,
            RunnerColor current,
            RunnerColor target)
        {
            int currentIndex = -1;
            int targetIndex = -1;
            for (int index = 0; index < definition.ColorCount; index++)
            {
                RunnerColor color = definition.GetColor(index);
                if (color == current)
                {
                    currentIndex = index;
                }
                if (color == target)
                {
                    targetIndex = index;
                }
            }
            if (currentIndex < 0 || targetIndex < 0)
            {
                throw new ArgumentException(
                    "Simulation colors must belong to the active palette.");
            }
            return (targetIndex - currentIndex + definition.ColorCount) %
                definition.ColorCount;
        }

        private static bool IsEntryWindow(
            ExperimentDefinition definition,
            int gate)
        {
            int start = definition.Mechanic == MechanicExperimentType.Fog
                ? definition.FogStartGate
                : definition.Mechanic == MechanicExperimentType.Ice
                    ? definition.IceStartGate
                    : definition.WarmUpGateCount;
            return gate >= start && gate < start + 3;
        }

        private static bool IsExitWindow(
            ExperimentDefinition definition,
            int gate)
        {
            int start = definition.Mechanic == MechanicExperimentType.Fog
                ? definition.FogEndGate + 1
                : definition.Mechanic == MechanicExperimentType.Ice
                    ? definition.IceEndGate + 1
                    : definition.PressureGateCount;
            return gate >= start && gate < start + 3;
        }

        private static float Next01(ref uint state)
        {
            state = DeterministicGateSequence.AdvanceXorshift32(state);
            return (state & 0x00FFFFFFu) / 16777216f;
        }

        private static float NextSigned(ref uint state)
        {
            return (Next01(ref state) * 2f) - 1f;
        }

        private static float Lerp(float start, float end, float amount)
        {
            return start + ((end - start) * amount);
        }

        private static float Average(List<float> values)
        {
            if (values.Count == 0)
            {
                return 0f;
            }
            float sum = 0f;
            for (int index = 0; index < values.Count; index++)
            {
                sum += values[index];
            }
            return sum / values.Count;
        }

        private static float Minimum(List<float> values)
        {
            if (values.Count == 0)
            {
                return 0f;
            }
            float minimum = values[0];
            for (int index = 1; index < values.Count; index++)
            {
                minimum = Math.Min(minimum, values[index]);
            }
            return minimum;
        }

        private static float Percentile(List<float> values, float percentile)
        {
            if (values.Count == 0)
            {
                return 0f;
            }
            values.Sort();
            int index = (int)Math.Round(
                (values.Count - 1) * percentile,
                MidpointRounding.AwayFromZero);
            return values[Math.Max(0, Math.Min(values.Count - 1, index))];
        }
    }

    public static class ExperimentShortlist
    {
        public const string BoundaryOnlyExperimentId =
            "step10-6-colors-none";

        public static bool IsBoundaryOnly(string experimentId)
        {
            return experimentId == BoundaryOnlyExperimentId;
        }

        public static string[] Select(ExperimentSimulationBatch batch)
        {
            if (batch == null)
            {
                throw new ArgumentNullException(nameof(batch));
            }
            string[] result = new string[5];
            MechanicExperimentType[] mechanics =
            {
                MechanicExperimentType.None,
                MechanicExperimentType.Camouflage,
                MechanicExperimentType.Fog,
                MechanicExperimentType.Ice,
                MechanicExperimentType.None
            };
            int[] targetColors = { 5, 4, 4, 4, 6 };
            for (int mechanic = 0; mechanic < mechanics.Length; mechanic++)
            {
                ExperimentSimulationResult best = null;
                for (int index = 0; index < batch.Results.Count; index++)
                {
                    ExperimentSimulationResult candidate =
                        batch.Results[index];
                    if (candidate.Profile != SimulatedPlayerKind.Average ||
                        candidate.Mechanic != mechanics[mechanic] ||
                        candidate.ColorCount > targetColors[mechanic])
                    {
                        continue;
                    }
                    bool boundary = mechanic == mechanics.Length - 1;
                    if (best == null ||
                        (boundary &&
                        candidate.ColorCount > best.ColorCount) ||
                        (!boundary &&
                        candidate.Risk != ExperimentRisk.HighRisk &&
                        candidate.ColorCount > best.ColorCount) ||
                        (!boundary &&
                        best.Risk == ExperimentRisk.HighRisk &&
                        candidate.Risk < best.Risk))
                    {
                        best = candidate;
                    }
                }
                result[mechanic] = best == null
                    ? string.Empty
                    : best.ExperimentId;
            }
            return result;
        }
    }
}
