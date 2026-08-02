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

            public LocalSaveLoadResult Load() =>
                LocalSaveLoadResult.Success(Stored.Clone(), false, false);

            public LocalSaveWriteResult Save(LocalSaveData data)
            {
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
