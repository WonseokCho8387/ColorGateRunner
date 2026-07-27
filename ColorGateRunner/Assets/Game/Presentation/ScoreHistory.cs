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
                if (!inserted && (source >= existingCount || score >= existing[source]))
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
