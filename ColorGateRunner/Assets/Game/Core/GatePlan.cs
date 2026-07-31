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
                indexInPattern,
                color,
                color,
                spacing,
                timeToGate,
                beatMultiplier,
                pattern,
                indexInPattern,
                hasShieldPickupBefore,
                false,
                GateModifier.None,
                default)
        {
        }

        public GatePlan(
            int gateId,
            RunnerColor color,
            float spacing,
            float timeToGate,
            float beatMultiplier,
            GatePatternType pattern,
            int indexInPattern,
            bool hasShieldPickupBefore,
            GateModifier modifier)
            : this(
                gateId,
                color,
                color,
                spacing,
                timeToGate,
                beatMultiplier,
                pattern,
                indexInPattern,
                hasShieldPickupBefore,
                false,
                modifier,
                default)
        {
        }

        public GatePlan(
            int gateId,
            RunnerColor color,
            float spacing,
            float timeToGate,
            float beatMultiplier,
            GatePatternType pattern,
            int indexInPattern,
            bool hasShieldPickupBefore,
            GateModifier modifier,
            FlickerGatePlan flickerPlan)
            : this(
                gateId,
                color,
                color,
                spacing,
                timeToGate,
                beatMultiplier,
                pattern,
                indexInPattern,
                hasShieldPickupBefore,
                false,
                modifier,
                flickerPlan)
        {
        }

        private GatePlan(
            int gateId,
            RunnerColor color,
            RunnerColor plannedColor,
            float spacing,
            float timeToGate,
            float beatMultiplier,
            GatePatternType pattern,
            int indexInPattern,
            bool hasShieldPickupBefore,
            bool hasTemporaryColorOverride,
            GateModifier modifier,
            FlickerGatePlan flickerPlan)
        {
            GateId = gateId;
            Color = color;
            PlannedColor = plannedColor;
            Spacing = spacing;
            TimeToGate = timeToGate;
            BeatMultiplier = beatMultiplier;
            Pattern = pattern;
            IndexInPattern = indexInPattern;
            HasShieldPickupBefore = hasShieldPickupBefore;
            HasTemporaryColorOverride = hasTemporaryColorOverride;
            Modifier = modifier;
            FlickerPlan = flickerPlan;
        }

        public int GateId { get; }
        public RunnerColor Color { get; }
        public RunnerColor PlannedColor { get; }
        public float Spacing { get; }
        public float TimeToGate { get; }
        public float BeatMultiplier { get; }
        public GatePatternType Pattern { get; }
        public int IndexInPattern { get; }
        public bool HasShieldPickupBefore { get; }
        public bool HasTemporaryColorOverride { get; }
        public GateModifier Modifier { get; }
        public FlickerGatePlan FlickerPlan { get; }
        public bool IsPatternStart => IndexInPattern == 0;

        public RunnerColor GetJudgmentColor(float gameplayTimeSeconds)
        {
            return Modifier.IsFlicker
                ? FlickerPlan.GetActiveColor(gameplayTimeSeconds)
                : Color;
        }

        public GatePlan WithTemporaryColorOverride(RunnerColor color)
        {
            return new GatePlan(
                GateId,
                color,
                PlannedColor,
                Spacing,
                TimeToGate,
                BeatMultiplier,
                Pattern,
                IndexInPattern,
                HasShieldPickupBefore,
                color != PlannedColor,
                Modifier,
                FlickerPlan);
        }

        public GatePlan WithModifier(GateModifier modifier)
        {
            return new GatePlan(
                GateId,
                Color,
                PlannedColor,
                Spacing,
                TimeToGate,
                BeatMultiplier,
                Pattern,
                IndexInPattern,
                HasShieldPickupBefore,
                HasTemporaryColorOverride,
                modifier,
                FlickerPlan);
        }

        public GatePlan WithFlickerPlan(FlickerGatePlan flickerPlan)
        {
            if (!Modifier.IsFlicker || !flickerPlan.IsEnabled)
            {
                throw new System.InvalidOperationException(
                    "A Flicker modifier and enabled plan are required.");
            }

            return new GatePlan(
                GateId,
                Color,
                PlannedColor,
                Spacing,
                TimeToGate,
                BeatMultiplier,
                Pattern,
                IndexInPattern,
                HasShieldPickupBefore,
                HasTemporaryColorOverride,
                Modifier,
                flickerPlan);
        }
    }
}
