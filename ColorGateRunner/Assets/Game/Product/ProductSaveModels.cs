using System;

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
    public sealed class LocalSaveData
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public long SaveRevision;
        public LocalProfileData Profile;
        public LocalSettingsData Settings;
        public string LastWriteUtc;

        public static LocalSaveData CreateEmpty()
        {
            return new LocalSaveData
            {
                SchemaVersion = CurrentSchemaVersion,
                SaveRevision = 0,
                Profile = null,
                Settings = null,
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
            LastWriteUtc = source.LastWriteUtc;
        }
    }
}
