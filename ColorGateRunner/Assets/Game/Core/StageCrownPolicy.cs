using System;

namespace ColorGateRunner.Core
{
    // Per-Stage achievements earned by clearing runs. Clean, Combo and
    // FullCombo are the three displayed crowns; Comeback is a history badge
    // stored with them for the later Collection page.
    [Flags]
    public enum StageCrown
    {
        None = 0,
        Clean = 1,
        Combo = 2,
        FullCombo = 4,
        Comeback = 8
    }

    // One clearing run's facts, independent of judgment and rewards.
    public readonly struct StageClearRun
    {
        public StageClearRun(
            bool continued,
            int shieldAbsorbCount,
            int maxCombo,
            int comboHitCount,
            int comboBreakCount,
            int comboTarget)
        {
            Continued = continued;
            ShieldAbsorbCount = Math.Max(0, shieldAbsorbCount);
            MaxCombo = Math.Max(0, maxCombo);
            ComboHitCount = Math.Max(0, comboHitCount);
            ComboBreakCount = Math.Max(0, comboBreakCount);
            ComboTarget = Math.Max(1, comboTarget);
        }

        public bool Continued { get; }
        public int ShieldAbsorbCount { get; }
        public int MaxCombo { get; }
        // Perfect and Great gates, the ones that continue the combo.
        public int ComboHitCount { get; }
        public int ComboBreakCount { get; }
        // Max combo the cleared Stage asks for its Combo crown.
        public int ComboTarget { get; }
    }

    public static class StageCrownPolicy
    {
        // Combo crown targets rise with Stage difficulty.
        public const int NormalComboTarget = 10;
        public const int HardComboTarget = 20;
        public const int VeryHardComboTarget = 30;
        // Seven failed attempts since the last clear, then a clear.
        public const int ComebackAttempt = 8;
        public const StageCrown DisplayedCrowns =
            StageCrown.Clean | StageCrown.Combo | StageCrown.FullCombo;
        public const StageCrown AllCrowns =
            DisplayedCrowns | StageCrown.Comeback;

        public static StageCrown Evaluate(
            StageClearRun run,
            int attemptsSinceClear)
        {
            StageCrown crowns = StageCrown.None;
            bool clean = !run.Continued && run.ShieldAbsorbCount == 0;
            if (clean)
            {
                crowns |= StageCrown.Clean;
            }
            if (run.MaxCombo >= run.ComboTarget)
            {
                crowns |= StageCrown.Combo;
            }
            if (clean && run.ComboHitCount > 0 && run.ComboBreakCount == 0)
            {
                crowns |= StageCrown.FullCombo;
            }
            if (attemptsSinceClear >= ComebackAttempt)
            {
                crowns |= StageCrown.Comeback;
            }
            return crowns;
        }

        public static int ComboTargetFor(StageDifficulty difficulty) =>
            difficulty switch
            {
                StageDifficulty.Hard => HardComboTarget,
                StageDifficulty.VeryHard => VeryHardComboTarget,
                _ => NormalComboTarget
            };

        public static bool IsValid(int flags)
        {
            return flags >= 0 && (flags & ~(int)AllCrowns) == 0;
        }
    }
}
