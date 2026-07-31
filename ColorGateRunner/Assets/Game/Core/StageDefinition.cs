using System;

namespace ColorGateRunner.Core
{
    public sealed class StageDefinition
    {
        private readonly RunnerColor[] _allowedColors;
        private readonly GatePatternType[] _allowedPatterns;
        private readonly int[] _firstGateIndicesByColor;

        public StageDefinition(
            string stageId,
            int displayNumber,
            string title,
            string description,
            int targetGateCount,
            RunnerColor[] allowedColors,
            float startingSpeed,
            float maximumSpeed,
            float cadenceStart,
            float cadenceEnd,
            GatePatternType[] allowedPatterns,
            int introGateCount,
            int finalPressureGateCount,
            float boosterDistance,
            float boosterSpeed,
            bool shieldAllowed,
            bool boosterAllowed,
            uint seed,
            bool activeColorsFromStart = true,
            int[] firstGateIndicesByColor = null,
            StagePrimaryMechanic primaryMechanic = StagePrimaryMechanic.None,
            GateModifierType gateModifiers = GateModifierType.None,
            EchoSettings echoSettings = null,
            CamouflageSettings camouflageSettings = null,
            StageMechanicGrantSettings mechanicGrantSettings = null,
            HiddenSettings hiddenSettings = null,
            FlickerSettings flickerSettings = null)
            : this(
                stageId,
                displayNumber,
                title,
                description,
                targetGateCount,
                allowedColors,
                StageSpeedProfile.Linear(startingSpeed, maximumSpeed),
                cadenceStart,
                cadenceEnd,
                allowedPatterns,
                introGateCount,
                finalPressureGateCount,
                boosterDistance,
                boosterSpeed,
                shieldAllowed,
                boosterAllowed,
                seed,
                activeColorsFromStart,
                firstGateIndicesByColor,
                primaryMechanic,
                gateModifiers,
                echoSettings,
                camouflageSettings,
                mechanicGrantSettings,
                hiddenSettings,
                flickerSettings)
        {
        }

        public StageDefinition(
            string stageId,
            int displayNumber,
            string title,
            string description,
            int targetGateCount,
            RunnerColor[] allowedColors,
            StageSpeedProfile speedProfile,
            float cadenceStart,
            float cadenceEnd,
            GatePatternType[] allowedPatterns,
            int introGateCount,
            int finalPressureGateCount,
            float boosterDistance,
            float boosterSpeed,
            bool shieldAllowed,
            bool boosterAllowed,
            uint seed,
            bool activeColorsFromStart = true,
            int[] firstGateIndicesByColor = null,
            StagePrimaryMechanic primaryMechanic = StagePrimaryMechanic.None,
            GateModifierType gateModifiers = GateModifierType.None,
            EchoSettings echoSettings = null,
            CamouflageSettings camouflageSettings = null,
            StageMechanicGrantSettings mechanicGrantSettings = null,
            HiddenSettings hiddenSettings = null,
            FlickerSettings flickerSettings = null)
        {
            StageId = stageId ?? throw new ArgumentNullException(nameof(stageId));
            DisplayNumber = displayNumber;
            Title = title ?? string.Empty;
            Description = description ?? string.Empty;
            TargetGateCount = targetGateCount;
            _allowedColors = allowedColors ??
                throw new ArgumentNullException(nameof(allowedColors));
            SpeedProfile = speedProfile ??
                throw new ArgumentNullException(nameof(speedProfile));
            CadenceStart = cadenceStart;
            CadenceEnd = cadenceEnd;
            _allowedPatterns = allowedPatterns ??
                throw new ArgumentNullException(nameof(allowedPatterns));
            IntroGateCount = introGateCount;
            FinalPressureGateCount = finalPressureGateCount;
            BoosterDistance = boosterDistance;
            BoosterSpeed = boosterSpeed;
            ShieldAllowed = shieldAllowed;
            BoosterAllowed = boosterAllowed;
            Seed = seed;
            ActiveColorsFromStart = activeColorsFromStart;
            _firstGateIndicesByColor = firstGateIndicesByColor ??
                CreateDefaultFirstGateIndices(_allowedColors.Length);
            PrimaryMechanic = primaryMechanic;
            GateModifiers = gateModifiers;
            EchoSettings = echoSettings ?? EchoSettings.Disabled();
            CamouflageSettings = camouflageSettings ??
                CamouflageSettings.CreateDefault();
            MechanicGrantSettings = mechanicGrantSettings ??
                StageMechanicGrantSettings.Disabled();
            HiddenSettings = hiddenSettings ?? HiddenSettings.Disabled();
            FlickerSettings = flickerSettings ?? FlickerSettings.Disabled();
        }

        public string StageId { get; }
        public int DisplayNumber { get; }
        public string Title { get; }
        public string Description { get; }
        public int TargetGateCount { get; }
        public StageSpeedProfile SpeedProfile { get; }
        public float StartingSpeed => SpeedProfile.StartingSpeed;
        public float MaximumSpeed => SpeedProfile.MaximumSpeed;
        public float CadenceStart { get; }
        public float CadenceEnd { get; }
        public int IntroGateCount { get; }
        public int FinalPressureGateCount { get; }
        public float BoosterDistance { get; }
        public float BoosterSpeed { get; }
        public bool ShieldAllowed { get; }
        public bool BoosterAllowed { get; }
        public uint Seed { get; }
        public bool ActiveColorsFromStart { get; }
        public StagePrimaryMechanic PrimaryMechanic { get; }
        public GateModifierType GateModifiers { get; }
        public EchoSettings EchoSettings { get; }
        public CamouflageSettings CamouflageSettings { get; }
        public StageMechanicGrantSettings MechanicGrantSettings { get; }
        public HiddenSettings HiddenSettings { get; }
        public FlickerSettings FlickerSettings { get; }
        public int AllowedColorCount => _allowedColors.Length;
        public int AllowedPatternCount => _allowedPatterns.Length;

        public float GetBaseSpeed(float normalizedProgress)
        {
            return SpeedProfile.Evaluate(normalizedProgress);
        }

        public RunnerColor GetAllowedColor(int index)
        {
            return _allowedColors[index];
        }

        public GatePatternType GetAllowedPattern(int index)
        {
            return _allowedPatterns[index];
        }

        public bool AllowsColor(RunnerColor color)
        {
            for (int index = 0; index < _allowedColors.Length; index++)
            {
                if (_allowedColors[index] == color)
                {
                    return true;
                }
            }

            return false;
        }

        public int GetFirstGateIndex(RunnerColor color)
        {
            for (int index = 0; index < _allowedColors.Length; index++)
            {
                if (_allowedColors[index] == color)
                {
                    return _firstGateIndicesByColor[index];
                }
            }

            throw new ArgumentOutOfRangeException(nameof(color));
        }

        public int GetGateColorCountAt(int gateIndex)
        {
            if (gateIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(gateIndex));
            }

            int count = 0;
            for (int index = 0; index < _firstGateIndicesByColor.Length; index++)
            {
                if (_firstGateIndicesByColor[index] <= gateIndex)
                {
                    count++;
                }
            }
            return count;
        }

        public int GetActiveColorCount(int resolvedGateCount)
        {
            return ActiveColorsFromStart
                ? AllowedColorCount
                : GetGateColorCountAt(resolvedGateCount);
        }

        public RunnerColor GetNextActiveColor(
            int resolvedGateCount,
            RunnerColor currentColor)
        {
            int activeCount = GetActiveColorCount(resolvedGateCount);
            for (int index = 0; index < activeCount; index++)
            {
                if (_allowedColors[index] == currentColor)
                {
                    return _allowedColors[(index + 1) % activeCount];
                }
            }

            return _allowedColors[0];
        }

        public int GetRequiredTapCount(
            int resolvedGateCount,
            RunnerColor currentColor,
            RunnerColor targetColor)
        {
            int activeCount = GetActiveColorCount(resolvedGateCount);
            int currentIndex = -1;
            int targetIndex = -1;
            for (int index = 0; index < activeCount; index++)
            {
                RunnerColor color = _allowedColors[index];
                if (color == currentColor)
                {
                    currentIndex = index;
                }
                if (color == targetColor)
                {
                    targetIndex = index;
                }
            }

            if (currentIndex < 0 || targetIndex < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(targetColor),
                    "Both colors must be active in the current cycle.");
            }

            return (targetIndex - currentIndex + activeCount) % activeCount;
        }

        public bool IsValid()
        {
            return StageId.Length > 0 &&
                DisplayNumber > 0 &&
                TargetGateCount > 0 &&
                _allowedColors.Length >= 2 &&
                StartingSpeed > 0f &&
                MaximumSpeed >= StartingSpeed &&
                CadenceStart >= GameRules.MinimumReactionTime &&
                CadenceEnd >= GameRules.MinimumReactionTime &&
                _allowedPatterns.Length > 0 &&
                _firstGateIndicesByColor.Length == _allowedColors.Length &&
                HasValidFirstGateIndices() &&
                IntroGateCount >= 0 &&
                FinalPressureGateCount > 0 &&
                FinalPressureGateCount < TargetGateCount &&
                BoosterDistance > 0f &&
                BoosterSpeed > MaximumSpeed &&
                (!EchoSettings.Enabled ||
                 (GateModifiers & GateModifierType.EchoProvider) != 0) &&
                (!HiddenSettings.Enabled ||
                 (GateModifiers & GateModifierType.Hidden) != 0) &&
                (!FlickerSettings.Enabled ||
                 ((GateModifiers & GateModifierType.Flicker) != 0 &&
                  AllowedColorCount >= 2));
        }

        private bool HasValidFirstGateIndices()
        {
            for (int index = 0; index < _firstGateIndicesByColor.Length; index++)
            {
                if (_firstGateIndicesByColor[index] < 0 ||
                    _firstGateIndicesByColor[index] >= TargetGateCount)
                {
                    return false;
                }
            }
            return true;
        }

        private static int[] CreateDefaultFirstGateIndices(int count)
        {
            return new int[count];
        }
    }
}
