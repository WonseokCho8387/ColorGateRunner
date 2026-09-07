using System;
using ColorGateRunner.Core;

namespace ColorGateRunner.Presentation
{
    internal sealed class CampaignLobbyReadModel
    {
        internal CampaignLobbyReadModel(
            string stageId,
            int displayNumber,
            string title,
            string mechanicLabel,
            string difficultyLabel,
            int clearedCount,
            int totalStageCount,
            CampaignRunKind runKind)
        {
            StageId = stageId;
            DisplayNumber = displayNumber;
            Title = title;
            MechanicLabel = mechanicLabel;
            DifficultyLabel = difficultyLabel;
            ClearedCount = clearedCount;
            TotalStageCount = totalStageCount;
            RunKind = runKind;
        }

        internal string StageId { get; }
        internal int DisplayNumber { get; }
        internal string Title { get; }
        internal string MechanicLabel { get; }
        internal string DifficultyLabel { get; }
        internal int ClearedCount { get; }
        internal int TotalStageCount { get; }
        internal CampaignRunKind RunKind { get; }
        internal bool IsLeague => RunKind == CampaignRunKind.League;
    }

    internal sealed class FrontendCampaignProgressReader
    {
        private readonly IStageProgressReader _progress;
        private readonly IStageCatalog _catalog;
        private readonly string _profileId;

        internal FrontendCampaignProgressReader(
            IStageProgressReader progress,
            IStageCatalog catalog,
            string profileId = "guest")
        {
            _progress = progress ??
                throw new ArgumentNullException(nameof(progress));
            _catalog = catalog ??
                throw new ArgumentNullException(nameof(catalog));
            _profileId = string.IsNullOrWhiteSpace(profileId)
                ? "guest"
                : profileId;
        }

        internal CampaignLobbyReadModel Read()
        {
            if (_catalog.Count <= 0)
            {
                throw new InvalidOperationException(
                    "The Stage Catalog contains no stages.");
            }

            bool[] cleared = new bool[_catalog.Count];
            string[] stageIds = new string[_catalog.Count];
            int clearedCount = 0;
            int replayClearCount = 0;
            for (int index = 0; index < cleared.Length; index++)
            {
                StageDefinition definition = _catalog.GetByIndex(index);
                StageRecord record =
                    _progress.LoadRecord(definition.DisplayNumber);
                bool isCleared = record.Cleared;
                cleared[index] = isCleared;
                stageIds[index] = definition.StageId;
                if (isCleared)
                {
                    clearedCount++;
                    replayClearCount += Math.Max(0, record.ClearCount - 1);
                }
            }

            bool leagueActive = LobbyProgression.AreAllStagesCleared(cleared);
            int displayNumber;
            CampaignRunKind runKind;
            if (leagueActive)
            {
                string stageId = LeagueStageSelector.SelectStageId(
                    _profileId,
                    stageIds,
                    replayClearCount);
                displayNumber = _catalog.GetById(stageId).DisplayNumber;
                runKind = CampaignRunKind.League;
            }
            else
            {
                displayNumber = LobbyProgression.SelectCurrentStage(
                    _progress.LoadHighestUnlocked(),
                    cleared);
                runKind = CampaignRunKind.Authored;
            }
            StageDefinition stage =
                _catalog.GetByDisplayNumber(displayNumber);
            return new CampaignLobbyReadModel(
                stage.StageId,
                stage.DisplayNumber,
                stage.Title,
                GetMechanicLabel(stage.PrimaryMechanic),
                GetDifficultyLabel(stage.Difficulty),
                clearedCount,
                _catalog.Count,
                runKind);
        }

        private static string GetMechanicLabel(
            StagePrimaryMechanic mechanic)
        {
            return mechanic == StagePrimaryMechanic.None
                ? "COLOR MATCH"
                : mechanic.ToString().ToUpperInvariant();
        }

        private static string GetDifficultyLabel(StageDifficulty difficulty)
        {
            return difficulty switch
            {
                StageDifficulty.Hard => "HARD",
                StageDifficulty.VeryHard => "VERY HARD",
                _ => "NORMAL"
            };
        }
    }
}
