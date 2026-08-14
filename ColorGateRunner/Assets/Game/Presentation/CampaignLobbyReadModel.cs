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
            int totalStageCount)
        {
            StageId = stageId;
            DisplayNumber = displayNumber;
            Title = title;
            MechanicLabel = mechanicLabel;
            DifficultyLabel = difficultyLabel;
            ClearedCount = clearedCount;
            TotalStageCount = totalStageCount;
        }

        internal string StageId { get; }
        internal int DisplayNumber { get; }
        internal string Title { get; }
        internal string MechanicLabel { get; }
        internal string DifficultyLabel { get; }
        internal int ClearedCount { get; }
        internal int TotalStageCount { get; }
    }

    internal sealed class FrontendCampaignProgressReader
    {
        private readonly IStageProgressReader _progress;
        private readonly IStageCatalog _catalog;

        internal FrontendCampaignProgressReader(
            IStageProgressReader progress,
            IStageCatalog catalog)
        {
            _progress = progress ??
                throw new ArgumentNullException(nameof(progress));
            _catalog = catalog ??
                throw new ArgumentNullException(nameof(catalog));
        }

        internal CampaignLobbyReadModel Read()
        {
            if (_catalog.Count <= 0)
            {
                throw new InvalidOperationException(
                    "The Stage Catalog contains no stages.");
            }

            bool[] cleared = new bool[_catalog.Count];
            int clearedCount = 0;
            for (int index = 0; index < cleared.Length; index++)
            {
                bool isCleared =
                    _progress.LoadRecord(index + 1).Cleared;
                cleared[index] = isCleared;
                if (isCleared)
                {
                    clearedCount++;
                }
            }

            int displayNumber = LobbyProgression.SelectCurrentStage(
                _progress.LoadHighestUnlocked(),
                cleared);
            StageDefinition stage =
                _catalog.GetByDisplayNumber(displayNumber);
            return new CampaignLobbyReadModel(
                stage.StageId,
                stage.DisplayNumber,
                stage.Title,
                GetMechanicLabel(stage.PrimaryMechanic),
                GetDifficultyLabel(stage.Difficulty),
                clearedCount,
                _catalog.Count);
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
