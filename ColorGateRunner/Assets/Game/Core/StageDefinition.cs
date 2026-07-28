using System;

namespace ColorGateRunner.Core
{
    public sealed class StageDefinition
    {
        private readonly RunnerColor[] _allowedColors;
        private readonly GatePatternType[] _allowedPatterns;

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
            uint seed)
        {
            StageId = stageId ?? throw new ArgumentNullException(nameof(stageId));
            DisplayNumber = displayNumber;
            Title = title ?? string.Empty;
            Description = description ?? string.Empty;
            TargetGateCount = targetGateCount;
            _allowedColors = allowedColors ??
                throw new ArgumentNullException(nameof(allowedColors));
            StartingSpeed = startingSpeed;
            MaximumSpeed = maximumSpeed;
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
        }

        public string StageId { get; }
        public int DisplayNumber { get; }
        public string Title { get; }
        public string Description { get; }
        public int TargetGateCount { get; }
        public float StartingSpeed { get; }
        public float MaximumSpeed { get; }
        public float CadenceStart { get; }
        public float CadenceEnd { get; }
        public int IntroGateCount { get; }
        public int FinalPressureGateCount { get; }
        public float BoosterDistance { get; }
        public float BoosterSpeed { get; }
        public bool ShieldAllowed { get; }
        public bool BoosterAllowed { get; }
        public uint Seed { get; }
        public int AllowedColorCount => _allowedColors.Length;
        public int AllowedPatternCount => _allowedPatterns.Length;

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
                IntroGateCount >= 0 &&
                FinalPressureGateCount > 0 &&
                FinalPressureGateCount < TargetGateCount &&
                BoosterDistance > 0f &&
                BoosterSpeed > MaximumSpeed;
        }
    }
}
