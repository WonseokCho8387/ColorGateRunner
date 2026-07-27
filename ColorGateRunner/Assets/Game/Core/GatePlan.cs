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
        {
            Color = color;
            Spacing = spacing;
            TimeToGate = timeToGate;
            BeatMultiplier = beatMultiplier;
            Pattern = pattern;
            IndexInPattern = indexInPattern;
            HasShieldPickupBefore = hasShieldPickupBefore;
        }

        public RunnerColor Color { get; }
        public float Spacing { get; }
        public float TimeToGate { get; }
        public float BeatMultiplier { get; }
        public GatePatternType Pattern { get; }
        public int IndexInPattern { get; }
        public bool HasShieldPickupBefore { get; }
        public bool IsPatternStart => IndexInPattern == 0;
    }
}
