using System;
using System.Collections.Generic;

namespace ColorGateRunner.Product
{
    public enum LocalAccountState
    {
        Guest = 0,
        LocalError = 1
    }

    [Serializable]
    public sealed class LocalProfileData
    {
        public string ProfileId;
        public string CreatedUtc;
        public string LastPlayedUtc;
        public string DisplayName;
        public LocalAccountState AccountState;
        public bool AccountChoiceCompleted;
        public long SaveRevision;

        public LocalProfileData Clone()
        {
            return new LocalProfileData
            {
                ProfileId = ProfileId,
                CreatedUtc = CreatedUtc,
                LastPlayedUtc = LastPlayedUtc,
                DisplayName = DisplayName,
                AccountState = AccountState,
                AccountChoiceCompleted = AccountChoiceCompleted,
                SaveRevision = SaveRevision
            };
        }
    }

    [Serializable]
    public sealed class LocalSettingsData
    {
        public float MasterVolume = 1f;
        public float MusicVolume = 1f;
        public float SfxVolume = 1f;
        public float LastNonZeroMusicVolume = 1f;
        public float LastNonZeroSfxVolume = 1f;
        public bool Vibration = true;
        public bool NotificationEnabled;
        public string Language = "system";

        public static LocalSettingsData CreateDefaults()
        {
            return new LocalSettingsData();
        }

        public LocalSettingsData Clone()
        {
            return new LocalSettingsData
            {
                MasterVolume = MasterVolume,
                MusicVolume = MusicVolume,
                SfxVolume = SfxVolume,
                LastNonZeroMusicVolume = LastNonZeroMusicVolume,
                LastNonZeroSfxVolume = LastNonZeroSfxVolume,
                Vibration = Vibration,
                NotificationEnabled = NotificationEnabled,
                Language = Language
            };
        }
    }

    [Serializable]
    public sealed class LocalStageProgressData
    {
        public string StageId;
        public bool Cleared;
        public float BestTime;
        public float BestNoItemTime;
        public int ClearCount;
        public int ContinuedClearCount;

        public LocalStageProgressData Clone()
        {
            return new LocalStageProgressData
            {
                StageId = StageId,
                Cleared = Cleared,
                BestTime = BestTime,
                BestNoItemTime = BestNoItemTime,
                ClearCount = ClearCount,
                ContinuedClearCount = ContinuedClearCount
            };
        }
    }

    [Serializable]
    public sealed class LocalCampaignProgressData
    {
        public bool LegacyMigrationCompleted;
        public string HighestUnlockedStageId = "stage-01";
        public List<LocalStageProgressData> StageRecords = new();

        public static LocalCampaignProgressData CreateDefaults()
        {
            return new LocalCampaignProgressData();
        }

        public LocalCampaignProgressData Clone()
        {
            var clone = new LocalCampaignProgressData
            {
                LegacyMigrationCompleted = LegacyMigrationCompleted,
                HighestUnlockedStageId = HighestUnlockedStageId,
                StageRecords = new List<LocalStageProgressData>(
                    StageRecords?.Count ?? 0)
            };
            if (StageRecords != null)
            {
                for (int index = 0; index < StageRecords.Count; index++)
                {
                    clone.StageRecords.Add(StageRecords[index]?.Clone());
                }
            }
            return clone;
        }
    }

    [Serializable]
    public sealed class LocalEconomyData
    {
        public int Coins;
        public int ShieldCount;
        public int BoosterCount;
        public List<string> AppliedTransactionIds = new();

        public static LocalEconomyData CreateDefaults()
        {
            return new LocalEconomyData();
        }

        public LocalEconomyData Clone()
        {
            return new LocalEconomyData
            {
                Coins = Coins,
                ShieldCount = ShieldCount,
                BoosterCount = BoosterCount,
                AppliedTransactionIds = AppliedTransactionIds == null
                    ? new List<string>()
                    : new List<string>(AppliedTransactionIds)
            };
        }
    }

    [Serializable]
    public sealed class LocalLobbyProgressData
    {
        public int AppliedMilestoneCount;
        public int PresentedMilestoneCount;

        public static LocalLobbyProgressData CreateDefaults()
        {
            return new LocalLobbyProgressData();
        }

        public LocalLobbyProgressData Clone()
        {
            return new LocalLobbyProgressData
            {
                AppliedMilestoneCount = AppliedMilestoneCount,
                PresentedMilestoneCount = PresentedMilestoneCount
            };
        }
    }

    [Serializable]
    public sealed class LocalSaveData
    {
        public const int CurrentSchemaVersion = 2;

        public int SchemaVersion = CurrentSchemaVersion;
        public long SaveRevision;
        public LocalProfileData Profile;
        public LocalSettingsData Settings;
        public LocalCampaignProgressData CampaignProgress =
            LocalCampaignProgressData.CreateDefaults();
        public LocalEconomyData Economy = LocalEconomyData.CreateDefaults();
        public LocalLobbyProgressData LobbyProgress =
            LocalLobbyProgressData.CreateDefaults();
        public string LastWriteUtc;

        public static LocalSaveData CreateEmpty()
        {
            return new LocalSaveData
            {
                SchemaVersion = CurrentSchemaVersion,
                SaveRevision = 0,
                Profile = null,
                Settings = null,
                CampaignProgress =
                    LocalCampaignProgressData.CreateDefaults(),
                Economy = LocalEconomyData.CreateDefaults(),
                LobbyProgress = LocalLobbyProgressData.CreateDefaults(),
                LastWriteUtc = string.Empty
            };
        }

        public LocalSaveData Clone()
        {
            return new LocalSaveData
            {
                SchemaVersion = SchemaVersion,
                SaveRevision = SaveRevision,
                Profile = Profile?.Clone(),
                Settings = Settings?.Clone(),
                CampaignProgress = CampaignProgress?.Clone(),
                Economy = Economy?.Clone(),
                LobbyProgress = LobbyProgress?.Clone(),
                LastWriteUtc = LastWriteUtc
            };
        }

        public void CopyFrom(LocalSaveData source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            SchemaVersion = source.SchemaVersion;
            SaveRevision = source.SaveRevision;
            Profile = source.Profile?.Clone();
            Settings = source.Settings?.Clone();
            CampaignProgress = source.CampaignProgress?.Clone();
            Economy = source.Economy?.Clone();
            LobbyProgress = source.LobbyProgress?.Clone();
            LastWriteUtc = source.LastWriteUtc;
        }
    }
}
