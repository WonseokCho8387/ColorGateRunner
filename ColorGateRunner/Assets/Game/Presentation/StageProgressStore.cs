using System;
using ColorGateRunner.Core;
using ColorGateRunner.Product;
using UnityEngine;

namespace ColorGateRunner.Presentation
{
    internal interface IStageProgressReader
    {
        int LoadHighestUnlocked();
        StageRecord LoadRecord(int stageNumber);
    }

    internal interface IStageProgressStore : IStageProgressReader
    {
        void SaveHighestUnlocked(int stageNumber);
        void SaveRecord(int stageNumber, StageRecord record);
        void ClearGameplayProgress();
    }

    internal interface IAtomicStageProgressStore
    {
        ProductMutationResult SaveClearResult(
            int stageNumber,
            StageRecord record,
            int highestUnlocked,
            string heartRefundToken);
    }

    internal sealed class PlayerPrefsStageProgressStore : IStageProgressStore
    {
        private const string UnlockKey = "ColorGateRunner.Stage.HighestUnlocked";
        private const string RecordPrefix = "ColorGateRunner.Stage.Record.";

        public int LoadHighestUnlocked()
        {
            return Mathf.Clamp(PlayerPrefs.GetInt(UnlockKey, 1), 1, StageCatalog.Count);
        }

        public StageRecord LoadRecord(int stageNumber)
        {
            return StageProgress.Parse(
                PlayerPrefs.GetString(RecordPrefix + stageNumber, string.Empty));
        }

        public void SaveHighestUnlocked(int stageNumber)
        {
            PlayerPrefs.SetInt(
                UnlockKey,
                Mathf.Clamp(stageNumber, 1, StageCatalog.Count));
            PlayerPrefs.Save();
        }

        public void SaveRecord(int stageNumber, StageRecord record)
        {
            PlayerPrefs.SetString(
                RecordPrefix + stageNumber,
                StageProgress.Serialize(record));
            PlayerPrefs.Save();
        }

        public void ClearGameplayProgress()
        {
            PlayerPrefs.DeleteKey(UnlockKey);
            for (int stageNumber = 1;
                stageNumber <= StageCatalog.Count;
                stageNumber++)
            {
                PlayerPrefs.DeleteKey(RecordPrefix + stageNumber);
            }
            PlayerPrefs.Save();
        }
    }

    internal sealed class ProductStageProgressStore :
        IStageProgressStore,
        IAtomicStageProgressStore
    {
        private readonly LocalProductSession _session;
        private readonly ProgressionService _progression;
        private readonly IStageCatalog _catalog;

        public ProductStageProgressStore(
            LocalProductSession session,
            ProgressionService progression,
            IStageCatalog catalog)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _progression = progression ??
                throw new ArgumentNullException(nameof(progression));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        public int LoadHighestUnlocked()
        {
            string stageId =
                _progression.Campaign?.HighestUnlockedStageId;
            for (int index = 0; index < _catalog.Count; index++)
            {
                StageDefinition stage = _catalog.GetByIndex(index);
                if (stage.StageId == stageId)
                {
                    return stage.DisplayNumber;
                }
            }
            return 1;
        }

        public StageRecord LoadRecord(int stageNumber)
        {
            StageDefinition stage = _catalog.GetByDisplayNumber(stageNumber);
            LocalStageProgressData data =
                _progression.GetStageRecord(stage.StageId);
            return data == null
                ? default
                : new StageRecord(
                    data.Cleared,
                    data.BestTime,
                    data.BestNoItemTime,
                    data.ClearCount,
                    data.ContinuedClearCount);
        }

        public void SaveHighestUnlocked(int stageNumber)
        {
            StageDefinition stage = _catalog.GetByDisplayNumber(stageNumber);
            ProductMutationResult result =
                _session.SetHighestUnlockedForDevelopment(stage.StageId);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(result.Error.Diagnostic);
            }
        }

        public void SaveRecord(int stageNumber, StageRecord record)
        {
            throw new InvalidOperationException(
                "Product progression requires one atomic clear result.");
        }

        public ProductMutationResult SaveClearResult(
            int stageNumber,
            StageRecord record,
            int highestUnlocked,
            string heartRefundToken)
        {
            StageDefinition stage = _catalog.GetByDisplayNumber(stageNumber);
            StageDefinition highest =
                _catalog.GetByDisplayNumber(highestUnlocked);
            return _session.RecordStageClear(
                new StageClearProgressRequest(
                    stageNumber,
                    stage.StageId,
                    highest.StageId,
                    new LocalStageProgressData
                    {
                        StageId = stage.StageId,
                        Cleared = record.Cleared,
                        BestTime = record.BestTime,
                        BestNoItemTime = record.BestNoItemTime,
                        ClearCount = record.ClearCount,
                        ContinuedClearCount = record.ContinuedClearCount
                    },
                    MapDifficulty(stage.Difficulty),
                    heartRefundToken));
        }

        private static StageRewardDifficulty MapDifficulty(
            StageDifficulty difficulty)
        {
            return difficulty switch
            {
                StageDifficulty.Normal => StageRewardDifficulty.Normal,
                StageDifficulty.Hard => StageRewardDifficulty.Hard,
                StageDifficulty.VeryHard => StageRewardDifficulty.VeryHard,
                _ => throw new ArgumentOutOfRangeException(nameof(difficulty))
            };
        }

        public void ClearGameplayProgress()
        {
            ProductMutationResult result =
                _session.ResetProgressForDevelopment();
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(result.Error.Diagnostic);
            }
        }
    }
}
