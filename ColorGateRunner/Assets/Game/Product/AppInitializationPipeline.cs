using System;
using System.Collections.Generic;

namespace ColorGateRunner.Product
{
    public sealed class AppInitializationPipeline
    {
        private readonly ILocalSaveService _saveService;
        private readonly ProfileService _profileService;
        private readonly SettingsService _settingsService;
        private readonly LocalAccountService _accountService;

        public AppInitializationPipeline(
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
            Session = new LocalProductSession(
                _saveService,
                _profileService,
                _settingsService,
                _accountService);
        }

        public int AttemptCount { get; private set; }
        public LocalProductSession Session { get; }

        public AppInitializationResult Initialize()
        {
            AttemptCount++;
            var steps = new List<InitializationStep>(6)
            {
                InitializationStep.ClockAndPaths,
                InitializationStep.SaveLoad
            };

            LocalSaveLoadResult load = _saveService.Load();
            if (!load.Succeeded)
            {
                return AppInitializationResult.Failure(steps, load.Error);
            }

            LocalSaveData data = load.Data;
            try
            {
                steps.Add(InitializationStep.Profile);
                bool profileDirty =
                    _profileService.LoadOrCreateGuest(data);

                steps.Add(InitializationStep.Settings);
                bool settingsDirty =
                    _settingsService.LoadOrCreateDefaults(data);
                _accountService.Load(_profileService.Current);

                if (load.Dirty || profileDirty || settingsDirty)
                {
                    steps.Add(InitializationStep.PersistDirtyData);
                    LocalSaveWriteResult write = _saveService.Save(data);
                    if (!write.Succeeded)
                    {
                        return AppInitializationResult.Failure(
                            steps,
                            write.Error);
                    }
                }

                Session.Bind(data);

                steps.Add(InitializationStep.Complete);
                return AppInitializationResult.Success(data, steps);
            }
            catch (Exception exception)
            {
                return AppInitializationResult.Failure(
                    steps,
                    new ProductError(
                        ProductErrorCode.Initialization,
                        exception.Message,
                        true));
            }
        }
    }
}
