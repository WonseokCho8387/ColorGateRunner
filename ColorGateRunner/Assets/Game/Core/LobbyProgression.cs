using System;

namespace ColorGateRunner.Core
{
    public static class LobbyProgression
    {
        public static int SelectCurrentStage(
            int highestUnlocked,
            bool[] clearedStages)
        {
            if (clearedStages == null ||
                clearedStages.Length != StageCatalog.Count)
            {
                throw new ArgumentException(
                    "One cleared flag is required per stage.",
                    nameof(clearedStages));
            }

            int unlocked = Math.Max(
                1,
                Math.Min(StageCatalog.Count, highestUnlocked));
            for (int index = 0; index < unlocked; index++)
            {
                if (!clearedStages[index])
                {
                    return index + 1;
                }
            }

            return unlocked;
        }

        public static int GetVisualTier(bool[] clearedStages)
        {
            if (clearedStages == null ||
                clearedStages.Length != StageCatalog.Count)
            {
                throw new ArgumentException(
                    "One cleared flag is required per stage.",
                    nameof(clearedStages));
            }

            if (clearedStages[4])
            {
                return 3;
            }
            if (clearedStages[3])
            {
                return 2;
            }
            if (clearedStages[1])
            {
                return 1;
            }
            return 0;
        }

        public static bool IsPrototypeComplete(bool[] clearedStages)
        {
            return clearedStages != null &&
                clearedStages.Length == StageCatalog.Count &&
                clearedStages[StageCatalog.Count - 1];
        }

        public static bool AreAllStagesCleared(bool[] clearedStages)
        {
            if (clearedStages == null || clearedStages.Length == 0)
            {
                return false;
            }
            for (int index = 0; index < clearedStages.Length; index++)
            {
                if (!clearedStages[index])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
