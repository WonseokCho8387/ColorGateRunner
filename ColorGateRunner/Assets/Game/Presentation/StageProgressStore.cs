using ColorGateRunner.Core;
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
}
