using System;

namespace ColorGateRunner.Core
{
    // One clearing run's ranking score: crowns and combo, weighted by how
    // hard the Stage is, minus taps beyond the fewest the run needed.
    public readonly struct StageScore
    {
        public StageScore(int crownPoints, int comboPoints, int extraTaps, int tapPenalty)
        {
            CrownPoints = crownPoints;
            ComboPoints = comboPoints;
            ExtraTaps = extraTaps;
            TapPenalty = tapPenalty;
            Total = Math.Max(0, crownPoints + comboPoints - tapPenalty);
        }

        public int CrownPoints { get; }
        public int ComboPoints { get; }
        public int ExtraTaps { get; }
        public int TapPenalty { get; }
        public int Total { get; }
    }

    public static class StageScorePolicy
    {
        public const int CleanPoints = 100;
        public const int ComboCrownPoints = 200;
        public const int FullComboPoints = 300;
        public const int PointsPerCombo = 10;
        public const int PointsPerExtraTap = 20;

        public static int DifficultyMultiplier(StageDifficulty difficulty) =>
            difficulty switch
            {
                StageDifficulty.Hard => 2,
                StageDifficulty.VeryHard => 3,
                _ => 1
            };

        // earned is what this run qualified for; Comeback scores nothing.
        public static StageScore Evaluate(
            StageDifficulty difficulty,
            StageCrown earned,
            int maxCombo,
            int taps,
            int minimumTaps)
        {
            int multiplier = DifficultyMultiplier(difficulty);
            int crowns =
                ((earned & StageCrown.Clean) != 0 ? CleanPoints : 0) +
                ((earned & StageCrown.Combo) != 0 ? ComboCrownPoints : 0) +
                ((earned & StageCrown.FullCombo) != 0 ? FullComboPoints : 0);
            int extra = Math.Max(0, taps - Math.Max(0, minimumTaps));
            return new StageScore(
                crowns * multiplier,
                Math.Max(0, maxCombo) * PointsPerCombo * multiplier,
                extra,
                extra * PointsPerExtraTap);
        }
    }
}
