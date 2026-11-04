using System;
using System.Collections.Generic;
using ColorGateRunner.Core;

namespace ColorGateRunner.Presentation
{
    public enum StageCollectionRowState
    {
        Cleared = 0,
        Next = 1,
        Locked = 2
    }

    public sealed class StageCollectionRowModel
    {
        internal StageCollectionRowModel(
            string stageId,
            int displayNumber,
            string title,
            StageCollectionRowState state,
            StageCrown crowns,
            float bestTime,
            int bestMaxCombo,
            bool unlocked,
            int comboTarget)
        {
            StageId = stageId;
            DisplayNumber = displayNumber;
            Title = title;
            State = state;
            Crowns = crowns;
            BestTime = bestTime;
            BestMaxCombo = bestMaxCombo;
            Unlocked = unlocked;
            ComboTarget = comboTarget;
        }

        public string StageId { get; }
        public int DisplayNumber { get; }
        public string Title { get; }
        public StageCollectionRowState State { get; }
        public StageCrown Crowns { get; }
        public float BestTime { get; }
        public int BestMaxCombo { get; }
        public bool Unlocked { get; }
        // This Stage's Combo crown target.
        public int ComboTarget { get; }
        public bool CanReplay =>
            State == StageCollectionRowState.Cleared && Unlocked;
        public bool Comeback => (Crowns & StageCrown.Comeback) != 0;

        public bool Has(StageCrown crown) => (Crowns & crown) == crown;
    }

    public sealed class StageCollectionModel
    {
        internal StageCollectionModel(
            IReadOnlyList<StageCollectionRowModel> rows,
            int crownCount,
            int cleanCount,
            int comboCount,
            int fullComboCount,
            int comebackCount)
        {
            Rows = rows;
            CrownCount = crownCount;
            CleanCount = cleanCount;
            ComboCount = comboCount;
            FullComboCount = fullComboCount;
            ComebackCount = comebackCount;
        }

        public IReadOnlyList<StageCollectionRowModel> Rows { get; }
        public int StageCount => Rows.Count;
        public int MaxCrownCount => Rows.Count * StageCollectionPresentation.CrownsPerStage;
        public int CrownCount { get; }
        public int CleanCount { get; }
        public int ComboCount { get; }
        public int FullComboCount { get; }
        public int ComebackCount { get; }
    }

    internal static class StageCollectionPresentation
    {
        internal const int CrownsPerStage = 3;

        internal static StageCollectionModel Create(
            IStageProgressReader progress,
            IStageCatalog catalog)
        {
            if (progress == null)
            {
                throw new ArgumentNullException(nameof(progress));
            }
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            int highestUnlocked = progress.LoadHighestUnlocked();
            var rows = new List<StageCollectionRowModel>(catalog.Count);
            int clean = 0;
            int combo = 0;
            int fullCombo = 0;
            int comeback = 0;
            for (int index = 0; index < catalog.Count; index++)
            {
                StageDefinition definition = catalog.GetByIndex(index);
                StageRecord record = progress.LoadRecord(definition.DisplayNumber);
                StageCollectionRowState state;
                StageCrown crowns = StageCrown.None;
                if (record.Cleared)
                {
                    state = StageCollectionRowState.Cleared;
                    crowns = record.Crowns & StageCrownPolicy.AllCrowns;
                }
                else
                {
                    state = definition.DisplayNumber <= highestUnlocked
                        ? StageCollectionRowState.Next
                        : StageCollectionRowState.Locked;
                }

                var row = new StageCollectionRowModel(
                    definition.StageId,
                    definition.DisplayNumber,
                    definition.Title,
                    state,
                    crowns,
                    record.Cleared ? Math.Max(0f, record.BestTime) : 0f,
                    record.Cleared ? Math.Max(0, record.BestMaxCombo) : 0,
                    definition.DisplayNumber <= highestUnlocked,
                    StageCrownPolicy.ComboTargetFor(definition.Difficulty));
                clean += row.Has(StageCrown.Clean) ? 1 : 0;
                combo += row.Has(StageCrown.Combo) ? 1 : 0;
                fullCombo += row.Has(StageCrown.FullCombo) ? 1 : 0;
                comeback += row.Comeback ? 1 : 0;
                rows.Add(row);
            }

            return new StageCollectionModel(
                rows,
                clean + combo + fullCombo,
                clean,
                combo,
                fullCombo,
                comeback);
        }

        internal static string FormatTitle(StageCollectionRowModel row) =>
            $"STAGE {row.DisplayNumber:00}  {row.Title}";

        internal static string FormatDetail(StageCollectionRowModel row) =>
            row.State switch
            {
                StageCollectionRowState.Cleared =>
                    $"BEST {row.BestTime:0.00}s  ·  COMBO {row.BestMaxCombo}" +
                    (row.Comeback ? "  ·  COMEBACK" : string.Empty),
                StageCollectionRowState.Next => "NOT CLEARED YET",
                _ => "LOCKED"
            };

        internal static string FormatCrownTotal(StageCollectionModel model) =>
            $"CROWNS {model.CrownCount} / {model.MaxCrownCount}";

        internal static string FormatBreakdown(StageCollectionModel model) =>
            $"CLEAN {model.CleanCount}  ·  " +
            $"COMBO {model.ComboCount}  ·  " +
            $"FULL COMBO {model.FullComboCount}  /  {model.StageCount}";

        internal static string FormatComeback(StageCollectionModel model) =>
            $"COMEBACK CLEARS {model.ComebackCount}";
    }
}
