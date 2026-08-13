using System;
using System.Collections.Generic;

namespace ColorGateRunner.Product
{
    public readonly struct CampaignProgressImportEntry
    {
        public CampaignProgressImportEntry(
            int displayNumber,
            LocalStageProgressData record,
            StageRewardDifficulty rewardDifficulty =
                StageRewardDifficulty.Normal)
        {
            DisplayNumber = displayNumber;
            Record = record ?? throw new ArgumentNullException(nameof(record));
            RewardDifficulty = rewardDifficulty;
        }

        public int DisplayNumber { get; }
        public LocalStageProgressData Record { get; }
        public StageRewardDifficulty RewardDifficulty { get; }
    }

    public readonly struct StageClearProgressRequest
    {
        public StageClearProgressRequest(
            int displayNumber,
            string stageId,
            string highestUnlockedStageId,
            LocalStageProgressData record,
            StageRewardDifficulty rewardDifficulty =
                StageRewardDifficulty.Normal,
            string heartRefundToken = null)
        {
            DisplayNumber = displayNumber;
            StageId = stageId;
            HighestUnlockedStageId = highestUnlockedStageId;
            Record = record ?? throw new ArgumentNullException(nameof(record));
            RewardDifficulty = rewardDifficulty;
            HeartRefundToken = heartRefundToken ?? string.Empty;
        }

        public int DisplayNumber { get; }
        public string StageId { get; }
        public string HighestUnlockedStageId { get; }
        public LocalStageProgressData Record { get; }
        public StageRewardDifficulty RewardDifficulty { get; }
        public string HeartRefundToken { get; }
    }

    public enum StageRewardDifficulty
    {
        Normal = 0,
        Hard = 1,
        VeryHard = 2
    }

    public readonly struct StageStartAuthorizationResult
    {
        public StageStartAuthorizationResult(
            ProductMutationResult mutation,
            bool heartConsumed,
            string heartRefundToken)
        {
            Mutation = mutation;
            HeartConsumed = heartConsumed;
            HeartRefundToken = heartRefundToken ?? string.Empty;
        }

        public ProductMutationResult Mutation { get; }
        public bool Succeeded => Mutation.Succeeded;
        public bool Changed => Mutation.Changed;
        public ProductError Error => Mutation.Error;
        public bool HeartConsumed { get; }
        public string HeartRefundToken { get; }
    }

    public readonly struct StageClearRewardPreview
    {
        internal StageClearRewardPreview(
            int baseCoins,
            int milestoneCoins,
            int shields,
            int boosters)
        {
            BaseCoins = baseCoins;
            MilestoneCoins = milestoneCoins;
            Shields = shields;
            Boosters = boosters;
        }

        public int BaseCoins { get; }
        public int MilestoneCoins { get; }
        public int Shields { get; }
        public int Boosters { get; }
        public int TotalCoins => BaseCoins + MilestoneCoins;
    }

    public static class StageClearRewardPolicy
    {
        public static int GetBaseCoins(StageRewardDifficulty difficulty)
        {
            return difficulty switch
            {
                StageRewardDifficulty.Normal => 100,
                StageRewardDifficulty.Hard => 200,
                StageRewardDifficulty.VeryHard => 500,
                _ => throw new ArgumentOutOfRangeException(nameof(difficulty))
            };
        }

        public static StageClearRewardPreview Preview(
            int displayNumber,
            StageRewardDifficulty difficulty)
        {
            int milestoneCoins = 0;
            int shields = 0;
            int boosters = 0;
            if (LobbyMilestoneRewardPolicy.IsMilestone(displayNumber))
            {
                int milestone = displayNumber / 2;
                milestoneCoins = milestone % 3 == 0 ? 300 : 200;
                if (milestone % 3 == 1)
                {
                    shields = 1;
                }
                else if (milestone % 3 == 2)
                {
                    boosters = 1;
                }
                else
                {
                    shields = 1;
                    boosters = 1;
                }
            }
            return new StageClearRewardPreview(
                GetBaseCoins(difficulty),
                milestoneCoins,
                shields,
                boosters);
        }
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
        internal static bool IsMilestone(int displayNumber)
        {
            return displayNumber >= 2 && displayNumber <= 36 &&
                displayNumber % 2 == 0;
        }

        internal static void Apply(
            int displayNumber,
            LocalEconomyData economy)
        {
            StageClearRewardPreview preview = StageClearRewardPolicy.Preview(
                displayNumber,
                StageRewardDifficulty.Normal);
            economy.Coins += preview.MilestoneCoins;
            economy.ShieldCount += preview.Shields;
            economy.BoosterCount += preview.Boosters;
        }
    }
}
