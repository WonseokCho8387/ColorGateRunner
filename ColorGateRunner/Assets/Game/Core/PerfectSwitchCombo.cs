namespace ColorGateRunner.Core
{
    // How the player's last color change related to a resolved gate.
    public readonly struct GateSwitchTiming
    {
        public GateSwitchTiming(bool switched, float secondsBeforeGate)
        {
            Switched = switched;
            SecondsBeforeGate = secondsBeforeGate;
        }

        public bool Switched { get; }
        public float SecondsBeforeGate { get; }
    }

    // The graded result of one resolved gate for the combo and its flash.
    // Broken carries no grade: the combo resets and nothing is shown.
    public enum ComboStep
    {
        Ignored,
        Perfect,
        Great,
        Good,
        Broken
    }

    // Grades matched gates by the final color change before arrival and
    // counts the combo. Booster and Echo passes are the stage's intended play
    // and always grade Perfect. Perfect and Great continue the combo; Good and
    // every gate without a grade break it. Score only; it never changes
    // judgment, speed, rewards or saves.
    public sealed class PerfectSwitchCombo
    {
        public const float PerfectWindowSeconds = 0.25f;
        public const float GreatWindowSeconds = 0.5f;

        public int Current { get; private set; }
        public int Max { get; private set; }
        public int PerfectCount { get; private set; }
        public int GreatCount { get; private set; }
        public int BreakCount { get; private set; }
        public int ComboHitCount => PerfectCount + GreatCount;

        public ComboStep Record(GateOutcome outcome, GateSwitchTiming timing)
        {
            if (outcome == GateOutcome.Ignored)
            {
                return ComboStep.Ignored;
            }
            ComboStep step;
            switch (outcome)
            {
                case GateOutcome.Matched:
                    step = Grade(timing);
                    break;
                case GateOutcome.Boosted:
                case GateOutcome.Echoed:
                    step = ComboStep.Perfect;
                    break;
                default:
                    step = ComboStep.Broken;
                    break;
            }
            switch (step)
            {
                case ComboStep.Perfect:
                    PerfectCount++;
                    Continue();
                    break;
                case ComboStep.Great:
                    GreatCount++;
                    Continue();
                    break;
                default:
                    // Good, plus Shielded, Invulnerable after Continue and
                    // Mismatched gates.
                    Current = 0;
                    BreakCount++;
                    break;
            }
            return step;
        }

        // Holding the right color through a gate is as good as a switch
        // timed just before it.
        public static ComboStep Grade(GateSwitchTiming timing)
        {
            if (!timing.Switched || timing.SecondsBeforeGate <= PerfectWindowSeconds)
            {
                return ComboStep.Perfect;
            }
            return timing.SecondsBeforeGate <= GreatWindowSeconds
                ? ComboStep.Great
                : ComboStep.Good;
        }

        public void Reset()
        {
            Current = 0;
            Max = 0;
            PerfectCount = 0;
            GreatCount = 0;
            BreakCount = 0;
        }

        private void Continue()
        {
            Current++;
            if (Current > Max)
            {
                Max = Current;
            }
        }
    }
}
