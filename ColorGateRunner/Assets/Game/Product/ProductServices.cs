using System;

namespace ColorGateRunner.Product
{
    public sealed class ProfileService
    {
        private readonly IClockService _clock;
        private readonly IProfileIdGenerator _idGenerator;

        public ProfileService(
            IClockService clock,
            IProfileIdGenerator idGenerator)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _idGenerator = idGenerator ??
                throw new ArgumentNullException(nameof(idGenerator));
        }

        public LocalProfileData Current { get; private set; }

        public bool LoadOrCreateGuest(LocalSaveData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            string now = LocalSaveService.ToUtcString(_clock.UtcNow);
            bool created = data.Profile == null;
            if (created)
            {
                string profileId = _idGenerator.CreateProfileId();
                if (string.IsNullOrWhiteSpace(profileId))
                {
                    throw new InvalidOperationException(
                        "The profile ID generator returned an empty ID.");
                }

                data.Profile = new LocalProfileData
                {
                    ProfileId = profileId,
                    CreatedUtc = now,
                    LastPlayedUtc = now,
                    DisplayName = "GUEST",
                    AccountState = LocalAccountState.Guest,
                    SaveRevision = data.SaveRevision
                };
            }
            else
            {
                data.Profile.LastPlayedUtc = now;
            }

            Current = data.Profile;
            return true;
        }
    }

    public sealed class LocalAccountService
    {
        public LocalAccountState CurrentState { get; private set; } =
            LocalAccountState.Guest;

        public string CurrentProfileId { get; private set; } = string.Empty;

        public void Load(LocalProfileData profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            CurrentState = profile.AccountState;
            CurrentProfileId = profile.ProfileId;
        }

        public ProductError LinkAccount(string provider)
        {
            return new ProductError(
                ProductErrorCode.UnsupportedAccountOperation,
                "External account providers are deferred.",
                false);
        }
    }

    public sealed class SettingsService
    {
        public LocalSettingsData Current { get; private set; }

        public bool LoadOrCreateDefaults(LocalSaveData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            bool created = data.Settings == null;
            data.Settings ??= LocalSettingsData.CreateDefaults();
            Current = data.Settings;
            return created;
        }
    }
}
