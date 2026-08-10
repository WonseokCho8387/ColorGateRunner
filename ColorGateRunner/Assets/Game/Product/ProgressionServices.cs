using System;
using System.Collections.Generic;

namespace ColorGateRunner.Product
{
    public readonly struct CampaignProgressImportEntry
    {
        public CampaignProgressImportEntry(
            int displayNumber,
            LocalStageProgressData record)
        {
            DisplayNumber = displayNumber;
            Record = record ?? throw new ArgumentNullException(nameof(record));
        }

        public int DisplayNumber { get; }
        public LocalStageProgressData Record { get; }
    }

    public readonly struct StageClearProgressRequest
    {
        public StageClearProgressRequest(
            int displayNumber,
            string stageId,
            string highestUnlockedStageId,
            LocalStageProgressData record)
        {
            DisplayNumber = displayNumber;
            StageId = stageId;
            HighestUnlockedStageId = highestUnlockedStageId;
            Record = record ?? throw new ArgumentNullException(nameof(record));
        }

        public int DisplayNumber { get; }
        public string StageId { get; }
        public string HighestUnlockedStageId { get; }
        public LocalStageProgressData Record { get; }
    }

    public sealed class ProgressionService
    {
        public LocalCampaignProgressData Campaign { get; private set; }
        public LocalEconomyData Economy { get; private set; }
        public LocalLobbyProgressData Lobby { get; private set; }

        public bool LoadOrCreateDefaults(LocalSaveData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            bool dirty = data.CampaignProgress == null ||
                data.Economy == null || data.LobbyProgress == null;
            data.CampaignProgress ??=
                LocalCampaignProgressData.CreateDefaults();
            data.Economy ??= LocalEconomyData.CreateDefaults();
            data.LobbyProgress ??= LocalLobbyProgressData.CreateDefaults();
            Bind(data);
            return dirty;
        }

        internal void Bind(LocalSaveData data)
        {
            Campaign = data?.CampaignProgress ??
                throw new ArgumentNullException(nameof(data));
            Economy = data.Economy;
            Lobby = data.LobbyProgress;
        }

        public LocalStageProgressData GetStageRecord(string stageId)
        {
            if (string.IsNullOrWhiteSpace(stageId) ||
                Campaign?.StageRecords == null)
            {
                return null;
            }
            for (int index = 0; index < Campaign.StageRecords.Count; index++)
            {
                LocalStageProgressData record = Campaign.StageRecords[index];
                if (record != null && record.StageId == stageId)
                {
                    return record;
                }
            }
            return null;
        }
    }

    internal static class LobbyMilestoneRewardPolicy
    {
        internal const int FirstClearCoins = 100;

        internal static bool IsMilestone(int displayNumber)
        {
            return displayNumber >= 2 && displayNumber <= 36 &&
                displayNumber % 2 == 0;
        }

        internal static void Apply(
            int displayNumber,
            LocalEconomyData economy)
        {
            int milestone = displayNumber / 2;
            economy.Coins += milestone % 3 == 0 ? 300 : 200;
            if (milestone % 3 == 1)
            {
                economy.ShieldCount++;
            }
            else if (milestone % 3 == 2)
            {
                economy.BoosterCount++;
            }
            else
            {
                economy.ShieldCount++;
                economy.BoosterCount++;
            }
        }
    }
}
