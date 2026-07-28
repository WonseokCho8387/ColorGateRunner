using System;
using System.Globalization;

namespace ColorGateRunner.Core
{
    public readonly struct StageRecord
    {
        public StageRecord(
            bool cleared,
            float bestTime,
            float bestNoItemTime,
            int clearCount)
            : this(cleared, bestTime, bestNoItemTime, clearCount, 0)
        {
        }

        public StageRecord(
            bool cleared,
            float bestTime,
            float bestNoItemTime,
            int clearCount,
            int continuedClearCount)
        {
            Cleared = cleared;
            BestTime = bestTime;
            BestNoItemTime = bestNoItemTime;
            ClearCount = clearCount;
            ContinuedClearCount = continuedClearCount;
        }

        public bool Cleared { get; }
        public float BestTime { get; }
        public float BestNoItemTime { get; }
        public int ClearCount { get; }
        public int ContinuedClearCount { get; }
    }

    public static class StageProgress
    {
        public static StageRecord RecordClear(
            StageRecord current,
            float time,
            StartItemSelection items)
        {
            return RecordClear(current, time, items, false);
        }

        public static StageRecord RecordClear(
            StageRecord current,
            float time,
            StartItemSelection items,
            bool continued)
        {
            if (time <= 0f)
            {
                return current;
            }

            float best = current.BestTime;
            float bestNoItem = current.BestNoItemTime;
            if (!continued)
            {
                best = best <= 0f
                    ? time
                    : Math.Min(best, time);
                if (!items.UsesAnyItem)
                {
                    bestNoItem = bestNoItem <= 0f
                        ? time
                        : Math.Min(bestNoItem, time);
                }
            }

            return new StageRecord(
                true,
                best,
                bestNoItem,
                current.ClearCount + 1,
                current.ContinuedClearCount + (continued ? 1 : 0));
        }

        public static int HighestUnlockedAfterClear(
            int highestUnlocked,
            int clearedDisplayNumber)
        {
            int next = Math.Min(StageCatalog.Count, clearedDisplayNumber + 1);
            return Math.Max(Math.Max(1, highestUnlocked), next);
        }

        public static string Serialize(StageRecord record)
        {
            return string.Join(
                "|",
                record.Cleared ? "1" : "0",
                record.BestTime.ToString("R", CultureInfo.InvariantCulture),
                record.BestNoItemTime.ToString("R", CultureInfo.InvariantCulture),
                Math.Max(0, record.ClearCount).ToString(
                    CultureInfo.InvariantCulture),
                Math.Max(0, record.ContinuedClearCount).ToString(
                    CultureInfo.InvariantCulture));
        }

        public static StageRecord Parse(string serialized)
        {
            if (string.IsNullOrEmpty(serialized))
            {
                return default;
            }

            string[] parts = serialized.Split('|');
            if ((parts.Length != 4 && parts.Length != 5) ||
                (parts[0] != "0" && parts[0] != "1") ||
                !float.TryParse(
                    parts[1],
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out float best) ||
                !float.TryParse(
                    parts[2],
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out float bestNoItem) ||
                !int.TryParse(
                    parts[3],
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out int clearCount) ||
                best < 0f ||
                bestNoItem < 0f ||
                clearCount < 0)
            {
                return default;
            }

            int continuedCount = 0;
            if (parts.Length == 5 &&
                (!int.TryParse(
                    parts[4],
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out continuedCount) ||
                continuedCount < 0))
            {
                return default;
            }

            return new StageRecord(
                parts[0] == "1",
                best,
                bestNoItem,
                clearCount,
                continuedCount);
        }
    }
}
