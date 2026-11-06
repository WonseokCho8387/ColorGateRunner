using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ColorGateRunner.Core;

namespace ColorGateRunner.Presentation
{
    // One row of the Campaign ranking; Rank counts from 1.
    public readonly struct RankingEntry
    {
        public RankingEntry(int rank, string name, int score)
        {
            Rank = rank;
            Name = string.IsNullOrWhiteSpace(name) ? "PLAYER" : name;
            Score = score;
        }

        public int Rank { get; }
        public string Name { get; }
        public int Score { get; }
    }

    public sealed class RankingBoard
    {
        public RankingBoard(
            IReadOnlyList<RankingEntry> top,
            bool hasPlayer,
            RankingEntry player)
        {
            Top = top ?? Array.Empty<RankingEntry>();
            HasPlayer = hasPlayer;
            Player = player;
        }

        public IReadOnlyList<RankingEntry> Top { get; }
        public bool HasPlayer { get; }
        public RankingEntry Player { get; }
    }

    // The online ranking service. Any failure is an exception; callers
    // show a truthful offline state and the game keeps working.
    public interface IRankingGateway
    {
        // Signs in when needed and returns the shown player name.
        Task<string> SignInAsync();
        Task SubmitTotalAsync(int total);
        Task<RankingBoard> LoadAsync(int topCount);
    }

    public static class CampaignRankingScore
    {
        // Every Stage best is between zero and what a perfect clear of that
        // Stage can score; the total is their sum. A basic range check
        // until scores are verified on a server.
        internal static int Total(IStageProgressReader progress, IStageCatalog catalog)
        {
            if (progress == null || catalog == null) return 0;
            long total = 0;
            for (int index = 0; index < catalog.Count; index++)
            {
                StageDefinition stage = catalog.GetByIndex(index);
                int best = progress.LoadRecord(stage.DisplayNumber).BestScore;
                total += Math.Max(0, Math.Min(best, MaximumFor(stage)));
            }
            return (int)Math.Min(int.MaxValue, total);
        }

        public static int MaximumFor(StageDefinition stage) =>
            StageScorePolicy.Evaluate(
                stage.Difficulty,
                StageCrown.Clean | StageCrown.Combo | StageCrown.FullCombo,
                stage.TargetGateCount,
                0,
                0).Total;
    }
}
