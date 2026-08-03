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

        internal void Bind(LocalProfileData profile)
        {
            Current = profile ?? throw new ArgumentNullException(nameof(profile));
        }
    }

    public sealed class LocalAccountService
    {
        private readonly IAccountProviderAvailability _providerAvailability;

        public LocalAccountService(
            IAccountProviderAvailability providerAvailability = null)
        {
            _providerAvailability = providerAvailability ??
                new UnavailableAccountProviderAvailability();
        }

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

        public bool IsProviderAvailable(string provider)
        {
            return _providerAvailability.IsAvailable(provider);
        }
    }

    public sealed class UnavailableAccountProviderAvailability :
        IAccountProviderAvailability
    {
        public bool IsAvailable(string provider)
        {
            return false;
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

        internal void Bind(LocalSettingsData settings)
        {
            Current = settings ?? throw new ArgumentNullException(nameof(settings));
        }
    }


    public sealed class LocalProductSession
    {
        private readonly ILocalSaveService _saveService;
        private readonly ProfileService _profileService;
        private readonly SettingsService _settingsService;
        private readonly LocalAccountService _accountService;
        private LocalSaveData _current;

        public LocalProductSession(
            ILocalSaveService saveService,
            ProfileService profileService,
            SettingsService settingsService,
            LocalAccountService accountService)
        {
            _saveService = saveService ??
                throw new ArgumentNullException(nameof(saveService));
            _profileService = profileService ??
                throw new ArgumentNullException(nameof(profileService));
            _settingsService = settingsService ??
                throw new ArgumentNullException(nameof(settingsService));
            _accountService = accountService ??
                throw new ArgumentNullException(nameof(accountService));
        }

        public bool IsReady => _current != null;

        internal void Bind(LocalSaveData data)
        {
            _current = data ?? throw new ArgumentNullException(nameof(data));
            RebindServices();
        }

        public ProductMutationResult CompleteGuestAccountChoice()
        {
            if (!TryGetReady(out ProductMutationResult failure))
            {
                return failure;
            }
            if (_current.Profile.AccountChoiceCompleted)
            {
                return ProductMutationResult.Success(false);
            }

            LocalSaveData candidate = _current.Clone();
            candidate.Profile.AccountChoiceCompleted = true;
            return Commit(candidate);
        }

        public ProductMutationResult ApplySettings(
            float masterVolume,
            float musicVolume,
            float sfxVolume,
            bool vibration)
        {
            bool notification =
                _current?.Settings?.NotificationEnabled ?? false;
            return ApplySettings(
                masterVolume,
                musicVolume,
                sfxVolume,
                vibration,
                notification);
        }

        public ProductMutationResult ApplySettings(
            float masterVolume,
            float musicVolume,
            float sfxVolume,
            bool vibration,
            bool notificationEnabled)
        {
            if (!TryGetReady(out ProductMutationResult failure))
            {
                return failure;
            }
            if (!IsFinite(masterVolume) || !IsFinite(musicVolume) ||
                !IsFinite(sfxVolume))
            {
                return ProductMutationResult.Failure(
                    new ProductError(
                        ProductErrorCode.SaveValidation,
                        "Settings volumes must be finite values.",
                        true));
            }

            float master = Clamp01(masterVolume);
            float music = Clamp01(musicVolume);
            float sfx = Clamp01(sfxVolume);
            LocalSettingsData current = _current.Settings;
            if (current.MasterVolume == master &&
                current.MusicVolume == music &&
                current.SfxVolume == sfx &&
                current.Vibration == vibration &&
                current.NotificationEnabled == notificationEnabled)
            {
                return ProductMutationResult.Success(false);
            }

            LocalSaveData candidate = _current.Clone();
            candidate.Settings.MasterVolume = master;
            if (music > 0f)
            {
                candidate.Settings.LastNonZeroMusicVolume = music;
            }
            else if (current.MusicVolume > 0f)
            {
                candidate.Settings.LastNonZeroMusicVolume =
                    current.MusicVolume;
            }
            if (sfx > 0f)
            {
                candidate.Settings.LastNonZeroSfxVolume = sfx;
            }
            else if (current.SfxVolume > 0f)
            {
                candidate.Settings.LastNonZeroSfxVolume =
                    current.SfxVolume;
            }
            candidate.Settings.MusicVolume = music;
            candidate.Settings.SfxVolume = sfx;
            candidate.Settings.Vibration = vibration;
            candidate.Settings.NotificationEnabled = notificationEnabled;
            return Commit(candidate);
        }

        private ProductMutationResult Commit(LocalSaveData candidate)
        {
            LocalSaveWriteResult write = _saveService.Save(candidate);
            if (!write.Succeeded)
            {
                return ProductMutationResult.Failure(write.Error);
            }

            _current.CopyFrom(candidate);
            RebindServices();
            return ProductMutationResult.Success(true);
        }

        private bool TryGetReady(out ProductMutationResult failure)
        {
            if (_current != null && _current.Profile != null &&
                _current.Settings != null)
            {
                failure = default;
                return true;
            }

            failure = ProductMutationResult.Failure(
                new ProductError(
                    ProductErrorCode.Initialization,
                    "The local product session is not initialized.",
                    true));
            return false;
        }

        private void RebindServices()
        {
            _profileService.Bind(_current.Profile);
            _settingsService.Bind(_current.Settings);
            _accountService.Load(_current.Profile);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static float Clamp01(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }
            return value > 1f ? 1f : value;
        }
    }
}
