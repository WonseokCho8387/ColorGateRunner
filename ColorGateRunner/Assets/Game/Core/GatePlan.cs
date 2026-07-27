namespace ColorGateRunner.Core
{
    public readonly struct GatePlan
    {
        public GatePlan(
            RunnerColor color,
            float spacing,
            float timeToGate,
            GatePatternType pattern,
            int indexInPattern)
        {
            Color = color;
            Spacing = spacing;
            TimeToGate = timeToGate;
            Pattern = pattern;
            IndexInPattern = indexInPattern;
        }

        public RunnerColor Color { get; }
        public float Spacing { get; }
        public float TimeToGate { get; }
        public GatePatternType Pattern { get; }
        public int IndexInPattern { get; }
        public bool IsPatternStart => IndexInPattern == 0;
    }
}
