namespace ColorGateRunner.Core
{
    public readonly struct GatePlan
    {
        public GatePlan(
            RunnerColor color,
            float spacing,
            float timeToGate,
            float beatMultiplier,
            GatePatternType pattern,
            int indexInPattern,
            bool hasShieldPickupBefore)
            : this(
                color,
                color,
                spacing,
                timeToGate,
                beatMultiplier,
                pattern,
                indexInPattern,
                hasShieldPickupBefore,
                false)
        {
        }

        private GatePlan(
            RunnerColor color,
            RunnerColor plannedColor,
            float spacing,
            float timeToGate,
            float beatMultiplier,
            GatePatternType pattern,
            int indexInPattern,
            bool hasShieldPickupBefore,
            bool hasTemporaryColorOverride)
        {
            Color = color;
            PlannedColor = plannedColor;
            Spacing = spacing;
            TimeToGate = timeToGate;
            BeatMultiplier = beatMultiplier;
            Pattern = pattern;
            IndexInPattern = indexInPattern;
            HasShieldPickupBefore = hasShieldPickupBefore;
            HasTemporaryColorOverride = hasTemporaryColorOverride;
        }

        public RunnerColor Color { get; }
        public RunnerColor PlannedColor { get; }
        public float Spacing { get; }
        public float TimeToGate { get; }
        public float BeatMultiplier { get; }
        public GatePatternType Pattern { get; }
        public int IndexInPattern { get; }
        public bool HasShieldPickupBefore { get; }
        public bool HasTemporaryColorOverride { get; }
        public bool IsPatternStart => IndexInPattern == 0;

        public GatePlan WithTemporaryColorOverride(RunnerColor color)
        {
            return new GatePlan(
                color,
                PlannedColor,
                Spacing,
                TimeToGate,
                BeatMultiplier,
                Pattern,
                IndexInPattern,
                HasShieldPickupBefore,
                color != PlannedColor);
        }
    }
}
