using System;
using UnityEngine;

namespace ColorGateRunner.Presentation
{
    internal interface IScoreHistoryStore
    {
        int[] Load();
        void Save(int[] scores);
    }

    internal static class ScoreHistory
    {
        internal const int Capacity = 5;

        internal static int[] Parse(string serialized)
        {
            if (string.IsNullOrEmpty(serialized))
            {
                return Array.Empty<int>();
            }

            string[] parts = serialized.Split(',');
            int count = Math.Min(parts.Length, Capacity);
            int[] result = new int[count];
            for (int index = 0; index < count; index++)
            {
                if (!int.TryParse(parts[index], out int score) || score <= 0)
                {
                    return Array.Empty<int>();
                }

                result[index] = score;
            }

            Array.Sort(result);
            Array.Reverse(result);
            return result;
        }

        internal static int[] Insert(int[] existing, int score)
        {
            if (score <= 0)
            {
                return existing ?? Array.Empty<int>();
            }

            int existingCount = existing == null ? 0 : Math.Min(existing.Length, Capacity);
            int[] result = new int[Math.Min(existingCount + 1, Capacity)];
            int source = 0;
            bool inserted = false;
            for (int index = 0; index < result.Length; index++)
            {
                if (!inserted && (source >= existingCount || score > existing[source]))
                {
                    result[index] = score;
                    inserted = true;
                }
                else
                {
                    result[index] = existing[source++];
                }
            }

            return result;
        }

        internal static ScoreHistoryUpdate InsertWithResult(
            int[] existing,
            int score)
        {
            int existingCount =
                existing == null ? 0 : Math.Min(existing.Length, Capacity);
            int rank = -1;
            if (score > 0)
            {
                int insertionIndex = 0;
                while (insertionIndex < existingCount &&
                    score <= existing[insertionIndex])
                {
                    insertionIndex++;
                }

                if (insertionIndex < Capacity)
                {
                    rank = insertionIndex;
                }
            }

            bool isNewBest =
                score > 0 &&
                (existingCount == 0 || score > existing[0]);
            return new ScoreHistoryUpdate(
                Insert(existing, score),
                rank,
                isNewBest);
        }
    }

    internal readonly struct ScoreHistoryUpdate
    {
        internal ScoreHistoryUpdate(int[] scores, int insertedRank, bool isNewBest)
        {
            Scores = scores;
            InsertedRank = insertedRank;
            IsNewBest = isNewBest;
        }

        internal int[] Scores { get; }
        internal int InsertedRank { get; }
        internal bool IsNewBest { get; }
    }

    internal sealed class PlayerPrefsScoreHistoryStore : IScoreHistoryStore
    {
        private const string Key = "ColorGateRunner.TopScores";

        public int[] Load()
        {
            return ScoreHistory.Parse(PlayerPrefs.GetString(Key, string.Empty));
        }

        public void Save(int[] scores)
        {
            PlayerPrefs.SetString(
                Key,
                scores == null ? string.Empty : string.Join(",", scores));
            PlayerPrefs.Save();
        }
    }
}
