using System;
using System.Collections.Generic;

namespace ColorGateRunner.Core
{
    public static class LeagueStageSelector
    {
        public static int SelectIndex(
            string profileId,
            IReadOnlyList<string> stageIds,
            int replayClearCount)
        {
            Validate(profileId, stageIds, replayClearCount);
            int count = stageIds.Count;
            int cycle = replayClearCount / count;
            int position = replayClearCount % count;
            int[] order = BuildOrder(profileId, stageIds, cycle);
            return order[position];
        }

        public static string SelectStageId(
            string profileId,
            IReadOnlyList<string> stageIds,
            int replayClearCount)
        {
            return stageIds[SelectIndex(
                profileId,
                stageIds,
                replayClearCount)];
        }

        private static int[] BuildOrder(
            string profileId,
            IReadOnlyList<string> stageIds,
            int cycle)
        {
            int[] order = CreateShuffledOrder(profileId, stageIds, cycle);
            if (cycle <= 0 || order.Length <= 1)
            {
                return order;
            }

            int[] previous = CreateShuffledOrder(
                profileId,
                stageIds,
                cycle - 1);
            int previousLast = previous[previous.Length - 1];
            if (order[0] == previousLast)
            {
                int swap = 1;
                while (swap < order.Length && order[swap] == previousLast)
                {
                    swap++;
                }
                if (swap < order.Length)
                {
                    (order[0], order[swap]) = (order[swap], order[0]);
                }
            }
            return order;
        }

        private static int[] CreateShuffledOrder(
            string profileId,
            IReadOnlyList<string> stageIds,
            int cycle)
        {
            int[] order = new int[stageIds.Count];
            for (int index = 0; index < order.Length; index++)
            {
                order[index] = index;
            }

            uint state = CreateSeed(profileId, stageIds, cycle);
            for (int index = order.Length - 1; index > 0; index--)
            {
                state = Next(state);
                int swap = (int)(state % (uint)(index + 1));
                (order[index], order[swap]) = (order[swap], order[index]);
            }
            return order;
        }

        private static uint CreateSeed(
            string profileId,
            IReadOnlyList<string> stageIds,
            int cycle)
        {
            const uint offset = 2166136261u;
            const uint prime = 16777619u;
            uint hash = offset;
            Append(ref hash, profileId, prime);
            for (int index = 0; index < stageIds.Count; index++)
            {
                hash ^= 0xffu;
                hash *= prime;
                Append(ref hash, stageIds[index], prime);
            }
            hash ^= (uint)cycle;
            hash *= prime;
            return hash == 0u ? 0x9E3779B9u : hash;
        }

        private static void Append(ref uint hash, string value, uint prime)
        {
            for (int index = 0; index < value.Length; index++)
            {
                hash ^= value[index];
                hash *= prime;
            }
        }

        private static uint Next(uint state)
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return state == 0u ? 0xA341316Cu : state;
        }

        private static void Validate(
            string profileId,
            IReadOnlyList<string> stageIds,
            int replayClearCount)
        {
            if (string.IsNullOrWhiteSpace(profileId))
            {
                throw new ArgumentException(
                    "A stable Profile ID is required.",
                    nameof(profileId));
            }
            if (stageIds == null || stageIds.Count == 0)
            {
                throw new ArgumentException(
                    "At least one Stage ID is required.",
                    nameof(stageIds));
            }
            if (replayClearCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(replayClearCount));
            }
            for (int index = 0; index < stageIds.Count; index++)
            {
                if (string.IsNullOrWhiteSpace(stageIds[index]))
                {
                    throw new ArgumentException(
                        "Stage IDs cannot be empty.",
                        nameof(stageIds));
                }
            }
        }
    }
}
