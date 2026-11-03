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

    public enum ComboStep
    {
        Ignored,
        Perfect,
        Held,
        Broken
    }

    // Counts consecutive Perfect Switches: a matched gate whose final color
    // change landed inside the window just before arrival. Score only; it
    // never changes judgment, speed, rewards or saves.
    public sealed class PerfectSwitchCombo
    {
        public const float PerfectWindowSeconds = 0.25f;

        public int Current { get; private set; }
        public int Max { get; private set; }
        public int PerfectCount { get; private set; }

        public ComboStep Record(GateOutcome outcome, GateSwitchTiming timing)
        {
            switch (outcome)
            {
                case GateOutcome.Ignored:
                    return ComboStep.Ignored;
                case GateOutcome.Matched:
                    if (!timing.Switched)
                    {
                        return ComboStep.Held;
                    }
                    if (timing.SecondsBeforeGate <= PerfectWindowSeconds)
                    {
                        Current++;
                        PerfectCount++;
                        if (Current > Max)
                        {
                            Max = Current;
                        }
                        return ComboStep.Perfect;
                    }
                    Current = 0;
                    return ComboStep.Broken;
                case GateOutcome.Echoed:
                case GateOutcome.Boosted:
                    return ComboStep.Held;
                default:
                    // Shielded, Invulnerable after Continue, and Mismatched.
                    Current = 0;
                    return ComboStep.Broken;
            }
        }

        public void Reset()
        {
            Current = 0;
            Max = 0;
            PerfectCount = 0;
        }
    }
}
