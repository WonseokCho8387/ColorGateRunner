using System;
using System.Collections.Generic;
using System.IO;
using ColorGateRunner.Presentation;
using ColorGateRunner.Product;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class ProductFoundationTests
    {
        private string _directory;
        private LocalSavePaths _paths;
        private readonly DateTime _firstUtc =
            new DateTime(2026, 8, 1, 1, 2, 3, DateTimeKind.Utc);

        [SetUp]
        public void SetUp()
        {
            _directory = Path.Combine(
                Path.GetTempPath(),
                "ColorGateRunnerProductTests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);
            _paths = new LocalSavePaths(_directory, "local-save.json");
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_directory))
            {
                Directory.Delete(_directory, true);
            }
        }

        [Test]
        public void ProductAssembly_RemainsUnityIndependent()
        {
            string assetPath = Path.Combine(
                UnityEngine.Application.dataPath,
                "Game",
                "Product",
                "ColorGateRunner.Product.asmdef");
            string text = File.ReadAllText(assetPath);

            Assert.That(text, Does.Contain("\"noEngineReferences\": true"));
            Assert.That(text, Does.Not.Contain("UnityEngine"));
            Assert.That(text, Does.Contain("\"references\": []"));
        }

        [Test]
        public void FreshInitialization_CreatesOneStableGuestAndOrderedSteps()
        {
            var clock = new MutableClock(_firstUtc);
            var ids = new CountingIdGenerator("guest-fixed");
            AppInitializationPipeline pipeline = CreatePipeline(
                clock,
                ids,
                out ProfileService profile,
                out _,
                out _);

            AppInitializationResult first = pipeline.Initialize();
            clock.UtcNowValue = _firstUtc.AddHours(2);
            AppInitializationResult second = pipeline.Initialize();

            Assert.That(first.Succeeded, Is.True);
            Assert.That(first.Steps, Is.EqualTo(new[]
            {
                InitializationStep.ClockAndPaths,
                InitializationStep.SaveLoad,
                InitializationStep.Profile,
                InitializationStep.Settings,
                InitializationStep.Progression,
                InitializationStep.PersistDirtyData,
                InitializationStep.Complete
            }));
            Assert.That(second.Succeeded, Is.True);
            Assert.That(ids.Count, Is.EqualTo(1));
            Assert.That(profile.Current.ProfileId, Is.EqualTo("guest-fixed"));
            Assert.That(second.Data.Profile.CreatedUtc,
                Is.EqualTo("2026-08-01T01:02:03.0000000Z"));
            Assert.That(second.Data.Profile.LastPlayedUtc,
                Is.EqualTo("2026-08-01T03:02:03.0000000Z"));
            Assert.That(second.Data.Profile.DisplayName, Is.EqualTo("GUEST"));
            Assert.That(second.Data.Profile.ProfileId,
                Does.Not.Contain(SystemInfoDeviceIdentifierMarker));
        }

        [Test]
        public void InitializationFailure_StopsAndRetryUsesSameServices()
        {
            var save = new FailOnceSaveService();
            var clock = new MutableClock(_firstUtc);
            var ids = new CountingIdGenerator("same-service-guest");
            var profile = new ProfileService(clock, ids);
            var settings = new SettingsService();
            var account = new LocalAccountService();
            var pipeline = new AppInitializationPipeline(
                save,
                profile,
                settings,
                account);

            AppInitializationResult failed = pipeline.Initialize();
            Assert.That(failed.Succeeded, Is.False);
            Assert.That(failed.Steps, Is.EqualTo(new[]
            {
                InitializationStep.ClockAndPaths,
                InitializationStep.SaveLoad
            }));
            Assert.That(profile.Current, Is.Null);

            AppInitializationResult retried = pipeline.Initialize();

            Assert.That(retried.Succeeded, Is.True);
            Assert.That(pipeline.AttemptCount, Is.EqualTo(2));
            Assert.That(ids.Count, Is.EqualTo(1));
        }

        [Test]
        public void SaveRoundtrip_PreservesProfileSettingsAndIncrementsRevision()
        {
            var clock = new MutableClock(_firstUtc);
            LocalSaveService save = CreateSave(clock);
            LocalSaveData data = CreateValidData();
            data.Settings.MusicVolume = 0.35f;

            LocalSaveWriteResult write = save.Save(data);
            LocalSaveLoadResult load = save.Load();

            Assert.That(write.Succeeded, Is.True);
            Assert.That(data.SchemaVersion,
                Is.EqualTo(LocalSaveData.CurrentSchemaVersion));
            Assert.That(data.SaveRevision, Is.EqualTo(1));
            Assert.That(data.Profile.SaveRevision, Is.EqualTo(1));
            Assert.That(data.LastWriteUtc,
                Is.EqualTo("2026-08-01T01:02:03.0000000Z"));
            Assert.That(load.Succeeded, Is.True);
            Assert.That(load.Data.Profile.ProfileId, Is.EqualTo("guest-1"));
            Assert.That(load.Data.Settings.MusicVolume, Is.EqualTo(0.35f));
        }

        [Test]
        public void Save_PreparesRevisionAndUtcBeforeTempSerialization()
        {
            var serializer = new InspectingSerializer();
            var fileSystem = new SystemLocalSaveFileSystem();
            var save = new LocalSaveService(
                serializer,
                fileSystem,
                new MutableClock(_firstUtc),
                _paths,
                SaveReplacementPolicy.RecoverableOnly);

            LocalSaveWriteResult result = save.Save(CreateValidData());

            Assert.That(result.Succeeded, Is.True);
            Assert.That(serializer.FirstSerializedRevision, Is.EqualTo(1));
            Assert.That(serializer.FirstSerializedProfileRevision,
                Is.EqualTo(1));
            Assert.That(serializer.FirstSerializedLastWriteUtc,
                Is.EqualTo("2026-08-01T01:02:03.0000000Z"));
            Assert.That(serializer.DeserializeCount, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void FailedReplacement_PreservesPreviousPrimary()
        {
            var clock = new MutableClock(_firstUtc);
            var serializer = new UnityJsonSaveDocumentSerializer();
            LocalSaveData original = CreateValidData();
            LocalSaveService first = CreateSave(clock);
            Assert.That(first.Save(original).Succeeded, Is.True);
            string primaryBefore = File.ReadAllText(_paths.Primary);

            original.Settings.MasterVolume = 0.25f;
            var failing = new LocalSaveService(
                serializer,
                new FailingMoveFileSystem(),
                clock,
                _paths,
                SaveReplacementPolicy.RecoverableOnly);
            LocalSaveWriteResult result = failing.Save(original);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(File.ReadAllText(_paths.Primary), Is.EqualTo(primaryBefore));
        }

        [Test]
        public void CorruptPrimary_RecoversValidBackup()
        {
            var clock = new MutableClock(_firstUtc);
            LocalSaveService save = CreateSave(clock);
            LocalSaveData data = CreateValidData();
            Assert.That(save.Save(data).Succeeded, Is.True);
            clock.UtcNowValue = _firstUtc.AddMinutes(1);
            Assert.That(save.Save(data).Succeeded, Is.True);
            File.WriteAllText(_paths.Primary, "{broken");

            LocalSaveLoadResult load = save.Load();

            Assert.That(load.Succeeded, Is.True);
            Assert.That(load.RecoveredFromBackup, Is.True);
            Assert.That(load.Data.Profile.ProfileId, Is.EqualTo("guest-1"));
            Assert.That(File.ReadAllText(_paths.Primary),
                Is.EqualTo(File.ReadAllText(_paths.Backup)));
        }

        [Test]
        public void BothCorrupt_ProducesTypedBlockingError()
        {
            File.WriteAllText(_paths.Primary, "broken-primary");
            File.WriteAllText(_paths.Backup, "broken-backup");

            LocalSaveLoadResult load = CreateSave(
                new MutableClock(_firstUtc)).Load();

            Assert.That(load.Succeeded, Is.False);
            Assert.That(load.Error.Code,
                Is.EqualTo(ProductErrorCode.CorruptPrimaryAndBackup));
            Assert.That(load.Error.Recoverable, Is.False);
        }

        [Test]
        public void FutureSchema_BlocksWithoutFallbackOrOverwrite()
        {
            string future = "{\"SchemaVersion\":999}";
            File.WriteAllText(_paths.Primary, future);
            File.WriteAllText(_paths.Backup,
                new UnityJsonSaveDocumentSerializer().Serialize(
                    CreateValidData()));

            LocalSaveLoadResult load = CreateSave(
                new MutableClock(_firstUtc)).Load();

            Assert.That(load.Succeeded, Is.False);
            Assert.That(load.Error.Code,
                Is.EqualTo(ProductErrorCode.FutureSchemaVersion));
            Assert.That(File.ReadAllText(_paths.Primary), Is.EqualTo(future));
        }

        [Test]
        public void SupportedSchemaZero_MigratesDeterministically()
        {
            LocalSaveData old = CreateValidData();
            old.SchemaVersion = 0;
            old.Settings = null;
            File.WriteAllText(
                _paths.Primary,
                new UnityJsonSaveDocumentSerializer().Serialize(old));

            LocalSaveLoadResult load = CreateSave(
                new MutableClock(_firstUtc)).Load();

            Assert.That(load.Succeeded, Is.True);
            Assert.That(load.Dirty, Is.True);
            Assert.That(load.Data.SchemaVersion,
                Is.EqualTo(LocalSaveData.CurrentSchemaVersion));
            Assert.That(load.Data.Settings.MasterVolume, Is.EqualTo(1f));
        }

        [Test]
        public void InvalidSettings_ResetToDocumentedDefaults()
        {
            LocalSaveData data = CreateValidData();
            data.Settings.MasterVolume = 2f;
            File.WriteAllText(
                _paths.Primary,
                new UnityJsonSaveDocumentSerializer().Serialize(data));

            LocalSaveLoadResult load = CreateSave(
                new MutableClock(_firstUtc)).Load();

            Assert.That(load.Succeeded, Is.True);
            Assert.That(load.Dirty, Is.True);
            Assert.That(load.Data.Settings.MasterVolume, Is.EqualTo(1f));
            Assert.That(load.Data.Settings.MusicVolume, Is.EqualTo(1f));
            Assert.That(load.Data.Settings.SfxVolume, Is.EqualTo(1f));
            Assert.That(load.Data.Settings.Vibration, Is.True);
            Assert.That(load.Data.Settings.Language, Is.EqualTo("system"));
        }

        [Test]
        public void SaveFiles_StayInsideInjectedDirectory()
        {
            LocalSaveService save = CreateSave(new MutableClock(_firstUtc));
            Assert.That(save.Save(CreateValidData()).Succeeded, Is.True);

            string[] files = Directory.GetFiles(_directory);
            Assert.That(files, Is.Not.Empty);
            Assert.That(files, Has.All.StartsWith(_directory));
        }

        [Test]
        public void RecoverablePolicy_DoesNotClaimAtomicReplacement()
        {
            LocalSaveService save = CreateSave(
                new MutableClock(_firstUtc),
                SaveReplacementPolicy.RecoverableOnly);

            LocalSaveWriteResult first = save.Save(CreateValidData());

            Assert.That(first.Succeeded, Is.True);
            Assert.That(first.Replacement,
                Is.EqualTo(SaveReplacementResult.Recoverable));
        }

        [Test]
        public void AccountLink_IsExplicitlyUnsupported()
        {
            ProductError result = new LocalAccountService()
                .LinkAccount("google");

            Assert.That(result.Code,
                Is.EqualTo(ProductErrorCode.UnsupportedAccountOperation));
            Assert.That(result.IsError, Is.True);
        }

        [Test]
        public void SchemaOneWithoutAccountChoiceField_DefaultsToIncomplete()
        {
            const string json =
                "{\"SchemaVersion\":1,\"SaveRevision\":2," +
                "\"Profile\":{\"ProfileId\":\"legacy\"," +
                "\"CreatedUtc\":\"2026-08-01T00:00:00.0000000Z\"," +
                "\"LastPlayedUtc\":\"2026-08-01T00:00:00.0000000Z\"," +
                "\"DisplayName\":\"GUEST\",\"AccountState\":0," +
                "\"SaveRevision\":2}," +
                "\"Settings\":{\"MasterVolume\":1," +
                "\"MusicVolume\":1,\"SfxVolume\":1," +
                "\"Vibration\":true,\"Language\":\"system\"}," +
                "\"LastWriteUtc\":\"2026-08-01T00:00:00.0000000Z\"}";
            var serializer = new UnityJsonSaveDocumentSerializer();

            bool parsed = serializer.TryDeserialize(
                json,
                out LocalSaveData data,
                out string diagnostic);

            Assert.That(parsed, Is.True, diagnostic);
            Assert.That(data.SchemaVersion, Is.EqualTo(1));
            Assert.That(data.Profile.AccountChoiceCompleted, Is.False);
        }

        [Test]
        public void SchemaOneWithoutUnifiedSettingsFields_UsesSafeDefaults()
        {
            const string json =
                "{\"SchemaVersion\":1,\"SaveRevision\":2," +
                "\"Profile\":{\"ProfileId\":\"legacy\"," +
                "\"CreatedUtc\":\"2026-08-01T00:00:00.0000000Z\"," +
                "\"LastPlayedUtc\":\"2026-08-01T00:00:00.0000000Z\"," +
                "\"DisplayName\":\"GUEST\",\"AccountState\":0," +
                "\"AccountChoiceCompleted\":true,\"SaveRevision\":2}," +
                "\"Settings\":{\"MasterVolume\":0.8," +
                "\"MusicVolume\":0.35,\"SfxVolume\":0," +
                "\"Vibration\":false,\"Language\":\"system\"}," +
                "\"LastWriteUtc\":\"2026-08-01T00:00:00.0000000Z\"}";
            File.WriteAllText(_paths.Primary, json);

            LocalSaveLoadResult load = CreateSave(
                new MutableClock(_firstUtc)).Load();

            Assert.That(load.Succeeded, Is.True);
            Assert.That(load.Dirty, Is.True);
            Assert.That(load.Data.SchemaVersion,
                Is.EqualTo(LocalSaveData.CurrentSchemaVersion));
            Assert.That(load.Data.Settings.MasterVolume, Is.EqualTo(0.8f));
            Assert.That(load.Data.Settings.MusicVolume, Is.EqualTo(0.35f));
            Assert.That(load.Data.Settings.SfxVolume, Is.Zero);
            Assert.That(load.Data.Settings.LastNonZeroMusicVolume,
                Is.EqualTo(1f));
            Assert.That(load.Data.Settings.LastNonZeroSfxVolume,
                Is.EqualTo(1f));
            Assert.That(load.Data.Settings.NotificationEnabled, Is.False);
            Assert.That(load.Data.Settings.Vibration, Is.False);
            Assert.That(load.Data.Profile.AccountChoiceCompleted, Is.True);
            Assert.That(load.Data.Profile.ProfileId, Is.EqualTo("legacy"));
        }

        [Test]
        public void SchemaTwo_MigratesToFullHeartsWithoutChangingWallet()
        {
            LocalSaveData data = CreateValidData();
            data.SchemaVersion = 2;
            data.Economy.Coins = 2600;
            data.Economy.ShieldCount = 4;
            data.Economy.BoosterCount = 4;
            File.WriteAllText(
                _paths.Primary,
                new UnityJsonSaveDocumentSerializer().Serialize(data));

            LocalSaveLoadResult load = CreateSave(
                new MutableClock(_firstUtc)).Load();

            Assert.That(load.Succeeded, Is.True);
            Assert.That(load.Dirty, Is.True);
            Assert.That(load.Data.SchemaVersion, Is.EqualTo(3));
            Assert.That(load.Data.Economy.Coins, Is.EqualTo(2600));
            Assert.That(load.Data.Economy.ShieldCount, Is.EqualTo(4));
            Assert.That(load.Data.Economy.BoosterCount, Is.EqualTo(4));
            Assert.That(load.Data.Economy.HeartCount,
                Is.EqualTo(HeartStatePolicy.MaximumHearts));
            Assert.That(load.Data.Economy.ContinueTicketCount, Is.Zero);
            Assert.That(load.Data.Economy.StarterBundlePurchased, Is.False);
        }

        [Test]
        public void InvalidSettingsReset_DoesNotResetAccountChoice()
        {
            LocalSaveData data = CreateValidData();
            data.Profile.AccountChoiceCompleted = true;
            data.Settings.MasterVolume = 2f;
            File.WriteAllText(
                _paths.Primary,
                new UnityJsonSaveDocumentSerializer().Serialize(data));

            LocalSaveLoadResult load = CreateSave(
                new MutableClock(_firstUtc)).Load();

            Assert.That(load.Succeeded, Is.True);
            Assert.That(load.Data.Settings.MasterVolume, Is.EqualTo(1f));
            Assert.That(load.Data.Profile.AccountChoiceCompleted, Is.True);
        }

        [Test]
        public void ProductSession_SaveFailureLeavesAllPublishedStateUntouched()
        {
            var save = new MutableSessionSaveService(CreateValidData());
            var clock = new MutableClock(_firstUtc);
            var profile = new ProfileService(
                clock,
                new CountingIdGenerator("unused"));
            var settings = new SettingsService();
            var account = new LocalAccountService();
            var pipeline = new AppInitializationPipeline(
                save,
                profile,
                settings,
                account);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);
            LocalProfileData publishedProfile = profile.Current;
            LocalSettingsData publishedSettings = settings.Current;
            save.FailWrites = true;

            ProductMutationResult accountResult =
                pipeline.Session.CompleteGuestAccountChoice();
            ProductMutationResult settingsResult =
                pipeline.Session.ApplySettings(0.2f, 0.3f, 0.4f, false);

            Assert.That(accountResult.Succeeded, Is.False);
            Assert.That(settingsResult.Succeeded, Is.False);
            Assert.That(profile.Current, Is.SameAs(publishedProfile));
            Assert.That(settings.Current, Is.SameAs(publishedSettings));
            Assert.That(profile.Current.AccountChoiceCompleted, Is.False);
            Assert.That(settings.Current.MasterVolume, Is.EqualTo(1f));
            Assert.That(settings.Current.Vibration, Is.True);
            Assert.That(account.CurrentProfileId, Is.EqualTo("guest-1"));
        }

        [Test]
        public void ProductSession_SuccessRebindsOnceAndPersistsClampedSettings()
        {
            var save = new MutableSessionSaveService(CreateValidData());
            var clock = new MutableClock(_firstUtc);
            var profile = new ProfileService(
                clock,
                new CountingIdGenerator("unused"));
            var settings = new SettingsService();
            var account = new LocalAccountService();
            var pipeline = new AppInitializationPipeline(
                save,
                profile,
                settings,
                account);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);

            ProductMutationResult accountResult =
                pipeline.Session.CompleteGuestAccountChoice();
            ProductMutationResult settingsResult =
                pipeline.Session.ApplySettings(-1f, 0.35f, 2f, false);

            Assert.That(accountResult.Succeeded, Is.True);
            Assert.That(settingsResult.Succeeded, Is.True);
            Assert.That(profile.Current.AccountChoiceCompleted, Is.True);
            Assert.That(settings.Current.MasterVolume, Is.Zero);
            Assert.That(settings.Current.MusicVolume, Is.EqualTo(0.35f));
            Assert.That(settings.Current.SfxVolume, Is.EqualTo(1f));
            Assert.That(settings.Current.Vibration, Is.False);
            Assert.That(save.Stored.Profile.ProfileId, Is.EqualTo("guest-1"));
            Assert.That(save.Stored.Profile.AccountChoiceCompleted, Is.True);
            Assert.That(save.Stored.Settings.Vibration, Is.False);
        }

        [Test]
        public void ProductSession_DisablingVolumesPreservesLastNonZeroValues()
        {
            var save = new MutableSessionSaveService(CreateValidData());
            var clock = new MutableClock(_firstUtc);
            var profile = new ProfileService(
                clock,
                new CountingIdGenerator("unused"));
            var settings = new SettingsService();
            var account = new LocalAccountService();
            var pipeline = new AppInitializationPipeline(
                save,
                profile,
                settings,
                account);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);
            Assert.That(
                pipeline.Session.ApplySettings(
                    0.8f,
                    0.35f,
                    0.45f,
                    true,
                    true).Succeeded,
                Is.True);

            ProductMutationResult disabled =
                pipeline.Session.ApplySettings(
                    0.8f,
                    0f,
                    0f,
                    false,
                    true);

            Assert.That(disabled.Succeeded, Is.True);
            Assert.That(settings.Current.MasterVolume, Is.EqualTo(0.8f));
            Assert.That(settings.Current.MusicVolume, Is.Zero);
            Assert.That(settings.Current.SfxVolume, Is.Zero);
            Assert.That(settings.Current.LastNonZeroMusicVolume,
                Is.EqualTo(0.35f));
            Assert.That(settings.Current.LastNonZeroSfxVolume,
                Is.EqualTo(0.45f));
            Assert.That(settings.Current.NotificationEnabled, Is.True);
        }

        [Test]
        public void ProductSession_AccountAndSettingsSurviveSaveReload()
        {
            var clock = new MutableClock(_firstUtc);
            LocalSaveService save = CreateSave(clock);
            var firstProfile = new ProfileService(
                clock,
                new CountingIdGenerator("stable-guest"));
            var firstSettings = new SettingsService();
            var firstAccount = new LocalAccountService();
            var firstPipeline = new AppInitializationPipeline(
                save,
                firstProfile,
                firstSettings,
                firstAccount);
            Assert.That(firstPipeline.Initialize().Succeeded, Is.True);
            Assert.That(
                firstPipeline.Session.CompleteGuestAccountChoice().Succeeded,
                Is.True);
            Assert.That(
                firstPipeline.Session.ApplySettings(
                    0.2f,
                    0.3f,
                    0.4f,
                    false).Succeeded,
                Is.True);

            var reloadedProfile = new ProfileService(
                clock,
                new CountingIdGenerator("must-not-be-used"));
            var reloadedSettings = new SettingsService();
            var reloadedAccount = new LocalAccountService();
            var reloadedPipeline = new AppInitializationPipeline(
                save,
                reloadedProfile,
                reloadedSettings,
                reloadedAccount);

            Assert.That(reloadedPipeline.Initialize().Succeeded, Is.True);
            Assert.That(reloadedProfile.Current.ProfileId,
                Is.EqualTo("stable-guest"));
            Assert.That(
                reloadedProfile.Current.AccountChoiceCompleted,
                Is.True);
            Assert.That(reloadedSettings.Current.MasterVolume,
                Is.EqualTo(0.2f));
            Assert.That(reloadedSettings.Current.MusicVolume,
                Is.EqualTo(0.3f));
            Assert.That(reloadedSettings.Current.SfxVolume,
                Is.EqualTo(0.4f));
            Assert.That(reloadedSettings.Current.Vibration, Is.False);
        }

        [Test]
        public void SchemaOneLoad_UpgradesProgressionEconomyAndLobbyDefaults()
        {
            const string utc = "2026-08-01T00:00:00.0000000Z";
            const string json =
                "{\"SchemaVersion\":1,\"SaveRevision\":2," +
                "\"Profile\":{\"ProfileId\":\"legacy\"," +
                "\"CreatedUtc\":\"" + utc + "\"," +
                "\"LastPlayedUtc\":\"" + utc + "\"," +
                "\"DisplayName\":\"GUEST\",\"AccountState\":0," +
                "\"SaveRevision\":2}," +
                "\"Settings\":{\"MasterVolume\":1," +
                "\"MusicVolume\":1,\"SfxVolume\":1," +
                "\"Vibration\":true,\"Language\":\"system\"}," +
                "\"LastWriteUtc\":\"" + utc + "\"}";
            File.WriteAllText(_paths.Primary, json);

            LocalSaveLoadResult load = CreateSave(
                new MutableClock(_firstUtc)).Load();

            Assert.That(load.Succeeded, Is.True);
            Assert.That(load.Dirty, Is.True);
            Assert.That(load.Data.SchemaVersion,
                Is.EqualTo(LocalSaveData.CurrentSchemaVersion));
            Assert.That(load.Data.CampaignProgress.HighestUnlockedStageId,
                Is.EqualTo("stage-01"));
            Assert.That(load.Data.CampaignProgress.LegacyMigrationCompleted,
                Is.False);
            Assert.That(load.Data.Economy.Coins, Is.Zero);
            Assert.That(load.Data.LobbyProgress.AppliedMilestoneCount, Is.Zero);
        }

        [Test]
        public void LegacyCampaignImport_IsAtomicIdempotentAndBackfillsRewards()
        {
            var save = new MutableSessionSaveService(CreateValidData());
            AppInitializationPipeline pipeline = CreateSessionPipeline(save);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);
            var entries = new[]
            {
                new CampaignProgressImportEntry(
                    1, CreateStageRecord("stage-01", true, 1)),
                new CampaignProgressImportEntry(
                    2, CreateStageRecord("stage-02", true, 1)),
                new CampaignProgressImportEntry(
                    4, CreateStageRecord("stage-04", true, 2))
            };

            ProductMutationResult first = pipeline.Session.ImportLegacyCampaign(
                "stage-05", entries);
            ProductMutationResult second = pipeline.Session.ImportLegacyCampaign(
                "stage-05", entries);

            Assert.That(first.Succeeded, Is.True);
            Assert.That(first.Changed, Is.True);
            Assert.That(second.Succeeded, Is.True);
            Assert.That(second.Changed, Is.False);
            Assert.That(pipeline.Progression.Campaign.LegacyMigrationCompleted,
                Is.True);
            Assert.That(pipeline.Progression.Campaign.HighestUnlockedStageId,
                Is.EqualTo("stage-05"));
            Assert.That(pipeline.Progression.Economy.Coins, Is.EqualTo(700));
            Assert.That(pipeline.Progression.Economy.ShieldCount, Is.EqualTo(1));
            Assert.That(pipeline.Progression.Economy.BoosterCount, Is.EqualTo(1));
            Assert.That(pipeline.Progression.Lobby.AppliedMilestoneCount,
                Is.EqualTo(2));
            Assert.That(save.Stored.Economy.Coins, Is.EqualTo(700));
        }

        [Test]
        public void StageClear_FirstClearRewardsOnceAndAcknowledgesLobby()
        {
            var save = new MutableSessionSaveService(CreateValidData());
            AppInitializationPipeline pipeline = CreateSessionPipeline(save);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);
            Assert.That(pipeline.Session.ImportLegacyCampaign(
                "stage-01",
                Array.Empty<CampaignProgressImportEntry>()).Succeeded,
                Is.True);
            var request = new StageClearProgressRequest(
                2,
                "stage-02",
                "stage-03",
                CreateStageRecord("stage-02", true, 1));

            ProductMutationResult first =
                pipeline.Session.RecordStageClear(request);
            ProductMutationResult repeated =
                pipeline.Session.RecordStageClear(request);
            ProductMutationResult acknowledged =
                pipeline.Session.AcknowledgeLobbyMilestones();

            Assert.That(first.Succeeded, Is.True);
            Assert.That(repeated.Succeeded, Is.True);
            Assert.That(pipeline.Progression.Economy.Coins, Is.EqualTo(300));
            Assert.That(pipeline.Progression.Economy.ShieldCount, Is.EqualTo(1));
            Assert.That(pipeline.Progression.Economy.BoosterCount, Is.Zero);
            Assert.That(pipeline.Progression.Lobby.AppliedMilestoneCount,
                Is.EqualTo(1));
            Assert.That(acknowledged.Succeeded, Is.True);
            Assert.That(pipeline.Progression.Lobby.PresentedMilestoneCount,
                Is.EqualTo(1));
            Assert.That(pipeline.Progression.Campaign.StageRecords.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void StageClearRewardPolicy_UsesDifficultyAndKeepsMilestoneSeparate()
        {
            Assert.That(StageClearRewardPolicy.GetBaseCoins(
                StageRewardDifficulty.Normal), Is.EqualTo(100));
            Assert.That(StageClearRewardPolicy.GetBaseCoins(
                StageRewardDifficulty.Hard), Is.EqualTo(200));
            Assert.That(StageClearRewardPolicy.GetBaseCoins(
                StageRewardDifficulty.VeryHard), Is.EqualTo(500));

            StageClearRewardPreview preview = StageClearRewardPolicy.Preview(
                20,
                StageRewardDifficulty.VeryHard);
            Assert.That(preview.BaseCoins, Is.EqualTo(500));
            Assert.That(preview.MilestoneCoins, Is.EqualTo(200));
            Assert.That(preview.Shields, Is.EqualTo(1));
            Assert.That(preview.Boosters, Is.Zero);
            Assert.That(preview.TotalCoins, Is.EqualTo(700));
        }

        [Test]
        public void StageClear_SaveFailureDoesNotPublishPartialProgression()
        {
            var save = new MutableSessionSaveService(CreateValidData());
            AppInitializationPipeline pipeline = CreateSessionPipeline(save);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);
            Assert.That(pipeline.Session.ImportLegacyCampaign(
                "stage-01",
                Array.Empty<CampaignProgressImportEntry>()).Succeeded,
                Is.True);
            save.FailWrites = true;

            ProductMutationResult result = pipeline.Session.RecordStageClear(
                new StageClearProgressRequest(
                    2,
                    "stage-02",
                    "stage-03",
                    CreateStageRecord("stage-02", true, 1)));

            Assert.That(result.Succeeded, Is.False);
            Assert.That(pipeline.Progression.Economy.Coins, Is.Zero);
            Assert.That(pipeline.Progression.Lobby.AppliedMilestoneCount,
                Is.Zero);
            Assert.That(pipeline.Progression.Campaign.StageRecords,
                Is.Empty);
        }

        [Test]
        public void ConsumeStartItems_BothSelected_DecrementsAtomically()
        {
            var save = new MutableSessionSaveService(CreateValidData());
            AppInitializationPipeline pipeline = CreateSessionPipeline(save);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);
            pipeline.Progression.Economy.ShieldCount = 2;
            pipeline.Progression.Economy.BoosterCount = 3;
            int savesBefore = save.SaveCount;

            ProductMutationResult result =
                pipeline.Session.ConsumeStartItems(true, true);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Changed, Is.True);
            Assert.That(save.SaveCount, Is.EqualTo(savesBefore + 1));
            Assert.That(pipeline.Progression.Economy.ShieldCount, Is.EqualTo(1));
            Assert.That(pipeline.Progression.Economy.BoosterCount, Is.EqualTo(2));
            Assert.That(save.Stored.Economy.ShieldCount, Is.EqualTo(1));
            Assert.That(save.Stored.Economy.BoosterCount, Is.EqualTo(2));
        }

        [Test]
        public void ConsumeStartItems_None_IsNoOpWithoutSave()
        {
            var save = new MutableSessionSaveService(CreateValidData());
            AppInitializationPipeline pipeline = CreateSessionPipeline(save);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);
            int savesBefore = save.SaveCount;

            ProductMutationResult result =
                pipeline.Session.ConsumeStartItems(false, false);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Changed, Is.False);
            Assert.That(save.SaveCount, Is.EqualTo(savesBefore));
        }

        [Test]
        public void ConsumeStartItems_Insufficient_IsAtomicAndDoesNotSave()
        {
            var save = new MutableSessionSaveService(CreateValidData());
            AppInitializationPipeline pipeline = CreateSessionPipeline(save);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);
            pipeline.Progression.Economy.ShieldCount = 1;
            pipeline.Progression.Economy.BoosterCount = 0;
            int savesBefore = save.SaveCount;

            ProductMutationResult result =
                pipeline.Session.ConsumeStartItems(true, true);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Error.Code,
                Is.EqualTo(ProductErrorCode.InsufficientInventory));
            Assert.That(save.SaveCount, Is.EqualTo(savesBefore));
            Assert.That(pipeline.Progression.Economy.ShieldCount, Is.EqualTo(1));
            Assert.That(pipeline.Progression.Economy.BoosterCount, Is.Zero);
        }

        [Test]
        public void ConsumeStartItems_SaveFailure_DoesNotPublishDecrement()
        {
            var save = new MutableSessionSaveService(CreateValidData());
            AppInitializationPipeline pipeline = CreateSessionPipeline(save);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);
            pipeline.Progression.Economy.ShieldCount = 1;
            pipeline.Progression.Economy.BoosterCount = 1;
            save.FailWrites = true;

            ProductMutationResult result =
                pipeline.Session.ConsumeStartItems(true, true);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Error.Code,
                Is.EqualTo(ProductErrorCode.SaveWrite));
            Assert.That(pipeline.Progression.Economy.ShieldCount, Is.EqualTo(1));
            Assert.That(pipeline.Progression.Economy.BoosterCount, Is.EqualTo(1));
        }

        [Test]
        public void ConsumeStartItems_RepeatedAttemptsConsumeAgain()
        {
            var save = new MutableSessionSaveService(CreateValidData());
            AppInitializationPipeline pipeline = CreateSessionPipeline(save);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);
            pipeline.Progression.Economy.ShieldCount = 2;

            ProductMutationResult first =
                pipeline.Session.ConsumeStartItems(true, false);
            ProductMutationResult retry =
                pipeline.Session.ConsumeStartItems(true, false);

            Assert.That(first.Succeeded, Is.True);
            Assert.That(retry.Succeeded, Is.True);
            Assert.That(pipeline.Progression.Economy.ShieldCount, Is.Zero);
        }

        [Test]
        public void SpendContinueCoins_IsAtomicAndIdempotent()
        {
            var save = new MutableSessionSaveService(CreateValidData());
            AppInitializationPipeline pipeline = CreateSessionPipeline(save);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);
            pipeline.Progression.Economy.Coins = 1000;
            int savesBefore = save.SaveCount;

            ProductMutationResult first = pipeline.Session.SpendContinueCoins(
                "continue:attempt-1:coin:0",
                300);
            ProductMutationResult repeated =
                pipeline.Session.SpendContinueCoins(
                    "continue:attempt-1:coin:0",
                    300);

            Assert.That(first.Succeeded, Is.True);
            Assert.That(first.Changed, Is.True);
            Assert.That(repeated.Succeeded, Is.True);
            Assert.That(repeated.Changed, Is.False);
            Assert.That(save.SaveCount, Is.EqualTo(savesBefore + 1));
            Assert.That(pipeline.Progression.Economy.Coins, Is.EqualTo(700));
            Assert.That(save.Stored.Economy.Coins, Is.EqualTo(700));
            Assert.That(
                pipeline.Progression.Economy.AppliedTransactionIds,
                Does.Contain("continue:attempt-1:coin:0"));
        }

        [TestCase(StartItemKind.Shield)]
        [TestCase(StartItemKind.Booster)]
        public void PurchaseStartItemWithCoins_IsAtomicAndIdempotent(
            StartItemKind kind)
        {
            var save = new MutableSessionSaveService(CreateValidData());
            AppInitializationPipeline pipeline = CreateSessionPipeline(save);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);
            pipeline.Progression.Economy.Coins = 1800;
            int savesBefore = save.SaveCount;

            ProductMutationResult first =
                pipeline.Session.PurchaseStartItemWithCoins(kind, "purchase-1");
            ProductMutationResult replay =
                pipeline.Session.PurchaseStartItemWithCoins(kind, "purchase-1");

            Assert.That(first.Succeeded, Is.True);
            Assert.That(first.Changed, Is.True);
            Assert.That(replay.Succeeded, Is.True);
            Assert.That(replay.Changed, Is.False);
            Assert.That(save.SaveCount, Is.EqualTo(savesBefore + 1));
            Assert.That(pipeline.Progression.Economy.Coins, Is.EqualTo(900));
            Assert.That(
                kind == StartItemKind.Shield
                    ? pipeline.Progression.Economy.ShieldCount
                    : pipeline.Progression.Economy.BoosterCount,
                Is.EqualTo(1));
            Assert.That(
                pipeline.Progression.Economy.AppliedTransactionIds,
                Does.Contain("start-item-purchase:purchase-1"));
        }

        [Test]
        public void PurchaseStartItemWithCoins_InsufficientFundsDoesNotSave()
        {
            var save = new MutableSessionSaveService(CreateValidData());
            AppInitializationPipeline pipeline = CreateSessionPipeline(save);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);
            pipeline.Progression.Economy.Coins = 899;
            int savesBefore = save.SaveCount;

            ProductMutationResult result =
                pipeline.Session.PurchaseStartItemWithCoins(
                    StartItemKind.Shield,
                    "purchase-low");

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Error.Code,
                Is.EqualTo(ProductErrorCode.InsufficientFunds));
            Assert.That(save.SaveCount, Is.EqualTo(savesBefore));
            Assert.That(pipeline.Progression.Economy.Coins, Is.EqualTo(899));
            Assert.That(pipeline.Progression.Economy.ShieldCount, Is.Zero);
        }

        [Test]
        public void PurchaseStartItemWithCoins_SaveFailureDoesNotPublish()
        {
            var save = new MutableSessionSaveService(CreateValidData());
            AppInitializationPipeline pipeline = CreateSessionPipeline(save);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);
            pipeline.Progression.Economy.Coins = 900;
            save.FailWrites = true;

            ProductMutationResult result =
                pipeline.Session.PurchaseStartItemWithCoins(
                    StartItemKind.Booster,
                    "purchase-fail");

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Error.Code, Is.EqualTo(ProductErrorCode.SaveWrite));
            Assert.That(pipeline.Progression.Economy.Coins, Is.EqualTo(900));
            Assert.That(pipeline.Progression.Economy.BoosterCount, Is.Zero);
            Assert.That(
                pipeline.Progression.Economy.AppliedTransactionIds,
                Is.Empty);
        }

        [TestCase(null, 300)]
        [TestCase("", 300)]
        [TestCase("   ", 300)]
        [TestCase("continue:attempt-1:coin:0", 0)]
        [TestCase("continue:attempt-1:coin:0", -1)]
        public void SpendContinueCoins_InvalidRequestDoesNotSave(
            string transactionId,
            int amount)
        {
            var save = new MutableSessionSaveService(CreateValidData());
            AppInitializationPipeline pipeline = CreateSessionPipeline(save);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);
            pipeline.Progression.Economy.Coins = 1000;
            int savesBefore = save.SaveCount;

            ProductMutationResult result =
                pipeline.Session.SpendContinueCoins(transactionId, amount);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Error.Code,
                Is.EqualTo(ProductErrorCode.SaveValidation));
            Assert.That(save.SaveCount, Is.EqualTo(savesBefore));
            Assert.That(pipeline.Progression.Economy.Coins, Is.EqualTo(1000));
        }

        [Test]
        public void SpendContinueCoins_InsufficientFundsDoesNotSave()
        {
            var save = new MutableSessionSaveService(CreateValidData());
            AppInitializationPipeline pipeline = CreateSessionPipeline(save);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);
            pipeline.Progression.Economy.Coins = 299;
            int savesBefore = save.SaveCount;

            ProductMutationResult result = pipeline.Session.SpendContinueCoins(
                "continue:attempt-1:coin:0",
                300);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Error.Code,
                Is.EqualTo(ProductErrorCode.InsufficientFunds));
            Assert.That(save.SaveCount, Is.EqualTo(savesBefore));
            Assert.That(pipeline.Progression.Economy.Coins, Is.EqualTo(299));
            Assert.That(
                pipeline.Progression.Economy.AppliedTransactionIds,
                Is.Empty);
        }

        [Test]
        public void SpendContinueCoins_SaveFailureDoesNotPublish()
        {
            var save = new MutableSessionSaveService(CreateValidData());
            AppInitializationPipeline pipeline = CreateSessionPipeline(save);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);
            pipeline.Progression.Economy.Coins = 1000;
            save.FailWrites = true;

            ProductMutationResult result = pipeline.Session.SpendContinueCoins(
                "continue:attempt-1:coin:0",
                300);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Error.Code,
                Is.EqualTo(ProductErrorCode.SaveWrite));
            Assert.That(pipeline.Progression.Economy.Coins, Is.EqualTo(1000));
            Assert.That(
                pipeline.Progression.Economy.AppliedTransactionIds,
                Is.Empty);
        }

        [Test]
        public void SpendContinueCoins_SaveFailureCanRetrySameTransactionOnce()
        {
            var save = new MutableSessionSaveService(CreateValidData());
            AppInitializationPipeline pipeline = CreateSessionPipeline(save);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);
            pipeline.Progression.Economy.Coins = 1000;
            int savesBefore = save.SaveCount;
            save.FailWrites = true;

            ProductMutationResult failed = pipeline.Session.SpendContinueCoins(
                "continue:attempt-1:coin:0",
                300);
            save.FailWrites = false;
            ProductMutationResult retry = pipeline.Session.SpendContinueCoins(
                "continue:attempt-1:coin:0",
                300);

            Assert.That(failed.Succeeded, Is.False);
            Assert.That(retry.Succeeded, Is.True);
            Assert.That(retry.Changed, Is.True);
            Assert.That(save.SaveCount, Is.EqualTo(savesBefore + 2));
            Assert.That(pipeline.Progression.Economy.Coins, Is.EqualTo(700));
            Assert.That(save.Stored.Economy.Coins, Is.EqualTo(700));
            Assert.That(
                pipeline.Progression.Economy.AppliedTransactionIds,
                Is.EqualTo(new[] { "continue:attempt-1:coin:0" }));
        }

        [Test]
        public void SpendContinueCoins_SameTransactionDifferentAmountIsReplay()
        {
            var save = new MutableSessionSaveService(CreateValidData());
            AppInitializationPipeline pipeline = CreateSessionPipeline(save);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);
            pipeline.Progression.Economy.Coins = 1000;
            int savesBefore = save.SaveCount;

            ProductMutationResult first = pipeline.Session.SpendContinueCoins(
                "continue:attempt-1:coin:0",
                300);
            ProductMutationResult replay = pipeline.Session.SpendContinueCoins(
                "continue:attempt-1:coin:0",
                900);

            Assert.That(first.Succeeded, Is.True);
            Assert.That(replay.Succeeded, Is.True);
            Assert.That(replay.Changed, Is.False);
            Assert.That(save.SaveCount, Is.EqualTo(savesBefore + 1));
            Assert.That(pipeline.Progression.Economy.Coins, Is.EqualTo(700));
            Assert.That(save.Stored.Economy.Coins, Is.EqualTo(700));
        }

        [Test]
        public void SpendContinueCoins_ReloadKeepsTransactionIdempotent()
        {
            var save = new MutableSessionSaveService(CreateValidData());
            AppInitializationPipeline first = CreateSessionPipeline(save);
            Assert.That(first.Initialize().Succeeded, Is.True);
            first.Progression.Economy.Coins = 1000;
            Assert.That(first.Session.SpendContinueCoins(
                "continue:attempt-1:coin:0",
                300).Succeeded,
                Is.True);

            AppInitializationPipeline reloaded = CreateSessionPipeline(save);
            Assert.That(reloaded.Initialize().Succeeded, Is.True);
            int savesBeforeReplay = save.SaveCount;
            ProductMutationResult replay =
                reloaded.Session.SpendContinueCoins(
                    "continue:attempt-1:coin:0",
                    300);

            Assert.That(replay.Succeeded, Is.True);
            Assert.That(replay.Changed, Is.False);
            Assert.That(save.SaveCount, Is.EqualTo(savesBeforeReplay));
            Assert.That(reloaded.Progression.Economy.Coins, Is.EqualTo(700));
        }

        [Test]
        public void DevelopmentEconomy_UpdatesExactValuesAndKeepsCommerceLedger()
        {
            LocalSaveData data = CreateValidData();
            data.Economy.Coins = 10;
            data.Economy.AppliedTransactionIds.Add("purchase:kept");
            var save = new MutableSessionSaveService(data);
            var clock = new MutableClock(_firstUtc);
            AppInitializationPipeline pipeline =
                CreateSessionPipeline(save, clock);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);

            ProductMutationResult result =
                pipeline.Session.SetEconomyForDevelopment(
                    25000,
                    7,
                    8,
                    3,
                    2,
                    TimeSpan.FromHours(3));

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Changed, Is.True);
            LocalEconomyData economy = pipeline.Progression.Economy;
            Assert.That(economy.Coins, Is.EqualTo(25000));
            Assert.That(economy.ShieldCount, Is.EqualTo(7));
            Assert.That(economy.BoosterCount, Is.EqualTo(8));
            Assert.That(economy.ContinueTicketCount, Is.EqualTo(3));
            Assert.That(economy.HeartCount, Is.EqualTo(2));
            Assert.That(economy.HeartRechargeAnchorUtc,
                Is.EqualTo("2026-08-01T01:02:03.0000000Z"));
            Assert.That(economy.UnlimitedHeartsUntilUtc,
                Is.EqualTo("2026-08-01T04:02:03.0000000Z"));
            Assert.That(economy.AppliedTransactionIds,
                Is.EqualTo(new[] { "purchase:kept" }));
            Assert.That(save.Stored.Economy.Coins, Is.EqualTo(25000));
        }

        [Test]
        public void DevelopmentEconomy_InvalidValuesDoNotSaveOrPublish()
        {
            var save = new MutableSessionSaveService(CreateValidData());
            AppInitializationPipeline pipeline = CreateSessionPipeline(save);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);
            int savesBefore = save.SaveCount;

            ProductMutationResult result =
                pipeline.Session.SetEconomyForDevelopment(
                    -1,
                    0,
                    0,
                    0,
                    HeartStatePolicy.MaximumHearts + 1,
                    TimeSpan.FromMinutes(-1));

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Error.Code,
                Is.EqualTo(ProductErrorCode.SaveValidation));
            Assert.That(save.SaveCount, Is.EqualTo(savesBefore));
            Assert.That(pipeline.Progression.Economy.Coins, Is.EqualTo(0));
        }

        [Test]
        public void DevelopmentCampaignReset_PreservesEconomyAndResetsLobby()
        {
            LocalSaveData data = CreateValidData();
            data.CampaignProgress.HighestUnlockedStageId = "stage-12";
            data.CampaignProgress.StageRecords.Add(
                CreateStageRecord("stage-01", true, 1));
            data.Economy.Coins = 9000;
            data.Economy.ShieldCount = 4;
            data.LobbyProgress.AppliedMilestoneCount = 6;
            var save = new MutableSessionSaveService(data);
            AppInitializationPipeline pipeline = CreateSessionPipeline(save);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);

            ProductMutationResult result =
                pipeline.Session.ResetCampaignForDevelopment();

            Assert.That(result.Succeeded, Is.True);
            Assert.That(pipeline.Progression.Campaign.HighestUnlockedStageId,
                Is.EqualTo("stage-01"));
            Assert.That(pipeline.Progression.Campaign.LegacyMigrationCompleted,
                Is.True);
            Assert.That(pipeline.Progression.Campaign.StageRecords, Is.Empty);
            Assert.That(pipeline.Progression.Lobby.AppliedMilestoneCount,
                Is.EqualTo(0));
            Assert.That(pipeline.Progression.Economy.Coins, Is.EqualTo(9000));
            Assert.That(pipeline.Progression.Economy.ShieldCount, Is.EqualTo(4));
        }

        [Test]
        public void DevelopmentEconomyReset_PreservesCampaignProgress()
        {
            LocalSaveData data = CreateValidData();
            data.CampaignProgress.HighestUnlockedStageId = "stage-09";
            data.Economy.Coins = 5000;
            data.Economy.ContinueTicketCount = 3;
            var save = new MutableSessionSaveService(data);
            AppInitializationPipeline pipeline = CreateSessionPipeline(save);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);

            ProductMutationResult result =
                pipeline.Session.ResetEconomyForDevelopment();

            Assert.That(result.Succeeded, Is.True);
            Assert.That(pipeline.Progression.Campaign.HighestUnlockedStageId,
                Is.EqualTo("stage-09"));
            Assert.That(pipeline.Progression.Economy.Coins, Is.EqualTo(0));
            Assert.That(pipeline.Progression.Economy.ContinueTicketCount,
                Is.EqualTo(0));
            Assert.That(pipeline.Progression.Economy.HeartCount,
                Is.EqualTo(HeartStatePolicy.MaximumHearts));
        }

        private AppInitializationPipeline CreatePipeline(
            MutableClock clock,
            CountingIdGenerator ids,
            out ProfileService profile,
            out SettingsService settings,
            out LocalAccountService account)
        {
            profile = new ProfileService(clock, ids);
            settings = new SettingsService();
            account = new LocalAccountService();
            LocalSaveService save = CreateSave(clock);
            return new AppInitializationPipeline(
                save,
                profile,
                settings,
                account);
        }

        [Test]
        public void CommerceCatalog_ContainsApprovedConsumableRewards()
        {
            Assert.That(CommerceProductCatalog.All.Count, Is.EqualTo(11));
            foreach (CommerceProductDefinition product in CommerceProductCatalog.All)
            {
                Assert.That(product.ProductType,
                    Is.EqualTo(CommerceProductType.Consumable));
            }
            Assert.That(CommerceProductCatalog.TryGet(
                "coins_100000", out CommerceProductDefinition coins), Is.True);
            Assert.That(coins.Reward.Coins, Is.EqualTo(100000));
            Assert.That(CommerceProductCatalog.TryGet(
                "bundle_starter", out CommerceProductDefinition starter), Is.True);
            Assert.That(starter.AccountLimited, Is.True);
            Assert.That(starter.Reward.Shields, Is.EqualTo(1));
            Assert.That(starter.Reward.Boosters, Is.EqualTo(1));
            Assert.That(starter.Reward.ContinueTickets, Is.EqualTo(1));
            Assert.That(starter.Reward.UnlimitedHeartsDuration,
                Is.EqualTo(TimeSpan.FromMinutes(30)));
            Assert.That(CommerceProductCatalog.TryGet(
                "bundle_xlarge", out CommerceProductDefinition xlarge), Is.True);
            Assert.That(xlarge.Reward.Coins, Is.EqualTo(10000));
            Assert.That(xlarge.Reward.Shields, Is.EqualTo(13));
            Assert.That(xlarge.Reward.Boosters, Is.EqualTo(13));
            Assert.That(xlarge.Reward.ContinueTickets, Is.EqualTo(3));
            Assert.That(xlarge.Reward.UnlimitedHeartsDuration,
                Is.EqualTo(TimeSpan.FromHours(12)));
        }

        [Test]
        public void StageStart_ConsumesHeartAndRechargesOfflineWithoutRollbackGain()
        {
            LocalSaveData data = CreateValidData();
            var save = new MutableSessionSaveService(data);
            var clock = new MutableClock(_firstUtc);
            AppInitializationPipeline pipeline = CreateSessionPipeline(save, clock);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);

            ProductMutationResult start =
                pipeline.Session.AuthorizeStageStart(false, false);
            Assert.That(start.Succeeded, Is.True);
            Assert.That(pipeline.Progression.Economy.HeartCount, Is.EqualTo(4));
            Assert.That(pipeline.Session.GetHeartState().NextHeartAtUtc,
                Is.EqualTo(_firstUtc.AddMinutes(30)));

            clock.UtcNowValue = _firstUtc.AddMinutes(61);
            Assert.That(pipeline.Session.RefreshHeartState().Succeeded, Is.True);
            HeartStateSnapshot recovered = pipeline.Session.GetHeartState();
            Assert.That(recovered.Count, Is.EqualTo(5));

            clock.UtcNowValue = _firstUtc.AddMinutes(1);
            ProductMutationResult noRollbackGain =
                pipeline.Session.RefreshHeartState();
            HeartStateSnapshot rollback = pipeline.Session.GetHeartState();
            Assert.That(noRollbackGain.Succeeded, Is.True);
            Assert.That(noRollbackGain.Changed, Is.False);
            Assert.That(rollback.Count, Is.EqualTo(5));
        }

        [Test]
        public void StageStartReceipt_ClearRefundsConsumedHeartAndRewardsOnce()
        {
            var save = new MutableSessionSaveService(CreateValidData());
            AppInitializationPipeline pipeline = CreateSessionPipeline(save);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);

            StageStartAuthorizationResult start =
                pipeline.Session.AuthorizeStageStartWithReceipt(
                    false,
                    false,
                    "attempt-20");
            StageStartAuthorizationResult duplicateStart =
                pipeline.Session.AuthorizeStageStartWithReceipt(
                    false,
                    false,
                    "attempt-20");
            Assert.That(start.Succeeded, Is.True);
            Assert.That(start.HeartConsumed, Is.True);
            Assert.That(start.HeartRefundToken, Is.EqualTo("attempt-20"));
            Assert.That(duplicateStart.Succeeded, Is.True);
            Assert.That(duplicateStart.Changed, Is.False);
            Assert.That(duplicateStart.HeartConsumed, Is.True);
            Assert.That(pipeline.Progression.Economy.HeartCount, Is.EqualTo(4));

            var request = new StageClearProgressRequest(
                20,
                "stage-20",
                "stage-20",
                CreateStageRecord("stage-20", true, 1),
                StageRewardDifficulty.VeryHard,
                start.HeartRefundToken);
            ProductMutationResult first =
                pipeline.Session.RecordStageClear(request);
            ProductMutationResult replay =
                pipeline.Session.RecordStageClear(request);

            Assert.That(first.Succeeded, Is.True);
            Assert.That(replay.Succeeded, Is.True);
            Assert.That(pipeline.Progression.Economy.HeartCount, Is.EqualTo(5));
            Assert.That(pipeline.Progression.Economy.Coins, Is.EqualTo(700));
            Assert.That(pipeline.Progression.Economy.ShieldCount, Is.EqualTo(1));
        }

        [Test]
        public void StageStartReceipt_UnlimitedOrFreeStartCannotCreateHeartRefund()
        {
            LocalSaveData data = CreateValidData();
            data.Economy.UnlimitedHeartsUntilUtc =
                LocalSaveService.ToUtcString(_firstUtc.AddHours(1));
            var save = new MutableSessionSaveService(data);
            AppInitializationPipeline pipeline = CreateSessionPipeline(save);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);

            StageStartAuthorizationResult unlimited =
                pipeline.Session.AuthorizeStageStartWithReceipt(
                    false,
                    false,
                    "unlimited-attempt");
            StageStartAuthorizationResult free =
                pipeline.Session.AuthorizeStageStartWithReceipt(
                    false,
                    false,
                    "provided-attempt",
                    false);

            Assert.That(unlimited.Succeeded, Is.True);
            Assert.That(unlimited.HeartConsumed, Is.False);
            Assert.That(unlimited.HeartRefundToken, Is.Empty);
            Assert.That(free.Succeeded, Is.True);
            Assert.That(free.Changed, Is.False);
            Assert.That(free.HeartConsumed, Is.False);
            Assert.That(free.HeartRefundToken, Is.Empty);
            Assert.That(pipeline.Progression.Economy.HeartCount,
                Is.EqualTo(HeartStatePolicy.MaximumHearts));
        }

        [Test]
        public void StageClear_HeartRefundAndRewardsAreAtomicOnSaveFailure()
        {
            var save = new MutableSessionSaveService(CreateValidData());
            AppInitializationPipeline pipeline = CreateSessionPipeline(save);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);
            StageStartAuthorizationResult start =
                pipeline.Session.AuthorizeStageStartWithReceipt(
                    false,
                    false,
                    "failed-clear-attempt");
            Assert.That(start.Succeeded, Is.True);
            save.FailWrites = true;

            ProductMutationResult result = pipeline.Session.RecordStageClear(
                new StageClearProgressRequest(
                    11,
                    "stage-11",
                    "stage-12",
                    CreateStageRecord("stage-11", true, 1),
                    StageRewardDifficulty.Hard,
                    start.HeartRefundToken));

            Assert.That(result.Succeeded, Is.False);
            Assert.That(pipeline.Progression.Economy.HeartCount, Is.EqualTo(4));
            Assert.That(pipeline.Progression.Economy.Coins, Is.Zero);
            Assert.That(pipeline.Progression.Campaign.StageRecords, Is.Empty);
        }

        [Test]
        public void StageClear_RejectsHeartRefundTokenWithoutStoredHeartSpend()
        {
            var save = new MutableSessionSaveService(CreateValidData());
            AppInitializationPipeline pipeline = CreateSessionPipeline(save);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);

            ProductMutationResult result = pipeline.Session.RecordStageClear(
                new StageClearProgressRequest(
                    1,
                    "stage-01",
                    "stage-02",
                    CreateStageRecord("stage-01", true, 1),
                    StageRewardDifficulty.Normal,
                    "not-issued"));

            Assert.That(result.Succeeded, Is.False);
            Assert.That(pipeline.Progression.Economy.HeartCount,
                Is.EqualTo(HeartStatePolicy.MaximumHearts));
            Assert.That(pipeline.Progression.Economy.Coins, Is.Zero);
            Assert.That(pipeline.Progression.Campaign.StageRecords, Is.Empty);
        }

        [Test]
        public void StageStart_HeartItemAndSaveAreAtomic()
        {
            LocalSaveData data = CreateValidData();
            data.Economy.HeartCount = 1;
            data.Economy.ShieldCount = 1;
            var save = new MutableSessionSaveService(data);
            AppInitializationPipeline pipeline = CreateSessionPipeline(save);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);
            int savesBefore = save.SaveCount;
            save.FailWrites = true;

            ProductMutationResult failed =
                pipeline.Session.AuthorizeStageStart(true, false);

            Assert.That(failed.Succeeded, Is.False);
            Assert.That(pipeline.Progression.Economy.HeartCount, Is.EqualTo(1));
            Assert.That(pipeline.Progression.Economy.ShieldCount, Is.EqualTo(1));
            Assert.That(save.SaveCount, Is.EqualTo(savesBefore + 1));
            Assert.That(save.Stored.Economy.HeartCount, Is.EqualTo(1));
            Assert.That(save.Stored.Economy.ShieldCount, Is.EqualTo(1));
        }

        [Test]
        public void StageStart_RejectsEmptyHeartsWithoutSaving()
        {
            LocalSaveData data = CreateValidData();
            data.Economy.HeartCount = 0;
            data.Economy.HeartRechargeAnchorUtc =
                LocalSaveService.ToUtcString(_firstUtc);
            var save = new MutableSessionSaveService(data);
            AppInitializationPipeline pipeline = CreateSessionPipeline(save);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);
            int savesBefore = save.SaveCount;

            ProductMutationResult result =
                pipeline.Session.AuthorizeStageStart(false, false);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Error.Code,
                Is.EqualTo(ProductErrorCode.InsufficientHearts));
            Assert.That(save.SaveCount, Is.EqualTo(savesBefore));
        }

        [Test]
        public void CommerceGrant_IsIdempotentAndStarterIsAccountLimited()
        {
            var save = new MutableSessionSaveService(CreateValidData());
            var clock = new MutableClock(_firstUtc);
            AppInitializationPipeline pipeline = CreateSessionPipeline(save, clock);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);

            ProductMutationResult granted = pipeline.Session.GrantCommerceProduct(
                "order-1", "bundle_starter");
            ProductMutationResult replay = pipeline.Session.GrantCommerceProduct(
                "order-1", "bundle_starter");
            ProductMutationResult second = pipeline.Session.GrantCommerceProduct(
                "order-2", "bundle_starter");

            Assert.That(granted.Succeeded, Is.True);
            Assert.That(replay.Succeeded, Is.True);
            Assert.That(replay.Changed, Is.False);
            Assert.That(second.Succeeded, Is.False);
            Assert.That(second.Error.Code, Is.EqualTo(ProductErrorCode.AlreadyOwned));
            Assert.That(pipeline.Progression.Economy.ShieldCount, Is.EqualTo(1));
            Assert.That(pipeline.Progression.Economy.BoosterCount, Is.EqualTo(1));
            Assert.That(pipeline.Progression.Economy.ContinueTicketCount,
                Is.EqualTo(1));
            Assert.That(pipeline.Session.GetHeartState().Unlimited, Is.True);
        }

        [Test]
        public void UnlimitedHearts_StacksAndDoesNotConsumeStoredHeart()
        {
            var save = new MutableSessionSaveService(CreateValidData());
            var clock = new MutableClock(_firstUtc);
            AppInitializationPipeline pipeline = CreateSessionPipeline(save, clock);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);
            Assert.That(pipeline.Session.GrantCommerceProduct(
                "small-1", "bundle_small").Succeeded, Is.True);
            clock.UtcNowValue = _firstUtc.AddMinutes(30);
            Assert.That(pipeline.Session.GrantCommerceProduct(
                "medium-1", "bundle_medium").Succeeded, Is.True);

            DateTime expected = _firstUtc.AddHours(4);
            Assert.That(pipeline.Session.GetHeartState().UnlimitedUntilUtc,
                Is.EqualTo(expected));
            Assert.That(pipeline.Session.AuthorizeStageStart(false, false).Succeeded,
                Is.True);
            Assert.That(pipeline.Progression.Economy.HeartCount,
                Is.EqualTo(HeartStatePolicy.MaximumHearts));
        }

        [Test]
        public void ContinueTicketSpend_IsPersistentAndIdempotent()
        {
            LocalSaveData data = CreateValidData();
            data.Economy.ContinueTicketCount = 1;
            var save = new MutableSessionSaveService(data);
            AppInitializationPipeline pipeline = CreateSessionPipeline(save);
            Assert.That(pipeline.Initialize().Succeeded, Is.True);

            ProductMutationResult first =
                pipeline.Session.SpendContinueTicket("ticket-use-1");
            ProductMutationResult replay =
                pipeline.Session.SpendContinueTicket("ticket-use-1");

            Assert.That(first.Succeeded, Is.True);
            Assert.That(replay.Succeeded, Is.True);
            Assert.That(replay.Changed, Is.False);
            Assert.That(pipeline.Progression.Economy.ContinueTicketCount,
                Is.EqualTo(0));
            Assert.That(save.Stored.Economy.ContinueTicketCount, Is.EqualTo(0));
        }

        private AppInitializationPipeline CreateSessionPipeline(
            ILocalSaveService save)
        {
            return CreateSessionPipeline(save, new MutableClock(_firstUtc));
        }

        private AppInitializationPipeline CreateSessionPipeline(
            ILocalSaveService save,
            MutableClock clock)
        {
            return new AppInitializationPipeline(
                save,
                new ProfileService(
                    clock,
                    new CountingIdGenerator("unused")),
                new SettingsService(),
                new LocalAccountService());
        }

        private static LocalStageProgressData CreateStageRecord(
            string stageId,
            bool cleared,
            int clearCount)
        {
            return new LocalStageProgressData
            {
                StageId = stageId,
                Cleared = cleared,
                BestTime = cleared ? 8.5f : 0f,
                BestNoItemTime = cleared ? 9f : 0f,
                ClearCount = clearCount,
                ContinuedClearCount = 0
            };
        }

        private LocalSaveService CreateSave(
            MutableClock clock,
            SaveReplacementPolicy policy =
                SaveReplacementPolicy.RecoverableOnly)
        {
            return new LocalSaveService(
                new UnityJsonSaveDocumentSerializer(),
                new SystemLocalSaveFileSystem(),
                clock,
                _paths,
                policy);
        }

        private LocalSaveData CreateValidData()
        {
            string utc = "2026-08-01T00:00:00.0000000Z";
            return new LocalSaveData
            {
                SchemaVersion = LocalSaveData.CurrentSchemaVersion,
                SaveRevision = 0,
                LastWriteUtc = utc,
                Profile = new LocalProfileData
                {
                    ProfileId = "guest-1",
                    CreatedUtc = utc,
                    LastPlayedUtc = utc,
                    DisplayName = "GUEST",
                    AccountState = LocalAccountState.Guest,
                    SaveRevision = 0
                },
                Settings = LocalSettingsData.CreateDefaults()
            };
        }

        private const string SystemInfoDeviceIdentifierMarker =
            "deviceUniqueIdentifier";

        private sealed class MutableClock : IClockService
        {
            public MutableClock(DateTime value) => UtcNowValue = value;
            public DateTime UtcNowValue { get; set; }
            public DateTime UtcNow => UtcNowValue;
        }

        private sealed class CountingIdGenerator : IProfileIdGenerator
        {
            private readonly string _id;
            public CountingIdGenerator(string id) => _id = id;
            public int Count { get; private set; }
            public string CreateProfileId()
            {
                Count++;
                return _id;
            }
        }

        private sealed class FailOnceSaveService : ILocalSaveService
        {
            private bool _failed;
            public LocalSaveLoadResult Load()
            {
                if (!_failed)
                {
                    _failed = true;
                    return LocalSaveLoadResult.Failure(
                        new ProductError(
                            ProductErrorCode.SaveRead,
                            "planned",
                            true));
                }

                return LocalSaveLoadResult.Success(
                    LocalSaveData.CreateEmpty(),
                    true,
                    false);
            }

            public LocalSaveWriteResult Save(LocalSaveData data)
            {
                data.SaveRevision++;
                data.Profile.SaveRevision = data.SaveRevision;
                return LocalSaveWriteResult.Success(
                    SaveReplacementResult.Recoverable);
            }
        }

        private sealed class MutableSessionSaveService : ILocalSaveService
        {
            public MutableSessionSaveService(LocalSaveData data)
            {
                Stored = data.Clone();
            }

            public LocalSaveData Stored { get; private set; }
            public bool FailWrites { get; set; }
            public int SaveCount { get; private set; }

            public LocalSaveLoadResult Load() =>
                LocalSaveLoadResult.Success(Stored.Clone(), false, false);

            public LocalSaveWriteResult Save(LocalSaveData data)
            {
                SaveCount++;
                if (FailWrites)
                {
                    return LocalSaveWriteResult.Failure(
                        new ProductError(
                            ProductErrorCode.SaveWrite,
                            "planned",
                            true));
                }

                data.SaveRevision++;
                data.Profile.SaveRevision = data.SaveRevision;
                Stored = data.Clone();
                return LocalSaveWriteResult.Success(
                    SaveReplacementResult.Recoverable);
            }
        }

        private sealed class InspectingSerializer : ISaveDocumentSerializer
        {
            private readonly UnityJsonSaveDocumentSerializer _inner = new();
            public long FirstSerializedRevision { get; private set; } = -1;
            public long FirstSerializedProfileRevision { get; private set; } = -1;
            public string FirstSerializedLastWriteUtc { get; private set; }
            public int DeserializeCount { get; private set; }

            public string Serialize(LocalSaveData data)
            {
                if (FirstSerializedRevision < 0)
                {
                    FirstSerializedRevision = data.SaveRevision;
                    FirstSerializedProfileRevision =
                        data.Profile.SaveRevision;
                    FirstSerializedLastWriteUtc = data.LastWriteUtc;
                }
                return _inner.Serialize(data);
            }

            public bool TryDeserialize(
                string serialized,
                out LocalSaveData data,
                out string diagnostic)
            {
                DeserializeCount++;
                return _inner.TryDeserialize(
                    serialized,
                    out data,
                    out diagnostic);
            }
        }

        private sealed class FailingMoveFileSystem : ILocalSaveFileSystem
        {
            private readonly SystemLocalSaveFileSystem _inner = new();
            public bool FileExists(string path) => _inner.FileExists(path);
            public string ReadAllText(string path) => _inner.ReadAllText(path);
            public void WriteAllTextDurable(string path, string contents) =>
                _inner.WriteAllTextDurable(path, contents);
            public void CopyFile(string source, string destination, bool overwrite) =>
                _inner.CopyFile(source, destination, overwrite);
            public void MoveFile(string source, string destination) =>
                throw new IOException("planned move failure");
            public void DeleteFile(string path) => _inner.DeleteFile(path);
            public void ReplaceFileAtomically(
                string source,
                string destination,
                string backup) =>
                throw new NotSupportedException();
        }
    }
}
