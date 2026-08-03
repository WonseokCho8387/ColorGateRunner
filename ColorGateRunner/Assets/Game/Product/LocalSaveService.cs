using System;
using System.Globalization;

namespace ColorGateRunner.Product
{
    public sealed class LocalSaveService : ILocalSaveService
    {
        private readonly ISaveDocumentSerializer _serializer;
        private readonly ILocalSaveFileSystem _fileSystem;
        private readonly IClockService _clock;
        private readonly LocalSavePaths _paths;
        private readonly SaveReplacementPolicy _replacementPolicy;

        public LocalSaveService(
            ISaveDocumentSerializer serializer,
            ILocalSaveFileSystem fileSystem,
            IClockService clock,
            LocalSavePaths paths,
            SaveReplacementPolicy replacementPolicy)
        {
            _serializer = serializer ??
                throw new ArgumentNullException(nameof(serializer));
            _fileSystem = fileSystem ??
                throw new ArgumentNullException(nameof(fileSystem));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _paths = paths;
            _replacementPolicy = replacementPolicy;
        }

        public LocalSaveLoadResult Load()
        {
            if (!_fileSystem.FileExists(_paths.Primary))
            {
                if (_fileSystem.FileExists(_paths.Backup))
                {
                    return LoadBackupAndRestorePrimary();
                }

                return LocalSaveLoadResult.Success(
                    LocalSaveData.CreateEmpty(),
                    true,
                    false);
            }

            DecodeResult primary = DecodeFile(_paths.Primary);
            if (primary.Status == DecodeStatus.Success)
            {
                return LocalSaveLoadResult.Success(
                    primary.Data,
                    primary.Dirty,
                    false);
            }
            if (primary.Status == DecodeStatus.FutureVersion)
            {
                return LocalSaveLoadResult.Failure(primary.Error);
            }

            if (!_fileSystem.FileExists(_paths.Backup))
            {
                return LocalSaveLoadResult.Failure(
                    new ProductError(
                        ProductErrorCode.CorruptPrimaryAndBackup,
                        "The primary save is invalid and no backup exists. " +
                        primary.Error.Diagnostic,
                        false));
            }

            return LoadBackupAndRestorePrimary(primary.Error.Diagnostic);
        }

        public LocalSaveWriteResult Save(LocalSaveData data)
        {
            if (data == null)
            {
                return LocalSaveWriteResult.Failure(
                    new ProductError(
                        ProductErrorCode.SaveWrite,
                        "A save snapshot is required.",
                        false));
            }

            LocalSaveData candidate = data.Clone();
            candidate.SchemaVersion = LocalSaveData.CurrentSchemaVersion;
            candidate.SaveRevision = Math.Max(0, candidate.SaveRevision) + 1;
            candidate.LastWriteUtc = ToUtcString(_clock.UtcNow);
            if (candidate.Profile != null)
            {
                candidate.Profile.SaveRevision = candidate.SaveRevision;
            }

            ProductError validationError =
                SaveDocumentPolicy.ValidateCurrent(candidate);
            if (validationError.IsError)
            {
                return LocalSaveWriteResult.Failure(validationError);
            }

            try
            {
                string serialized = _serializer.Serialize(candidate);
                _fileSystem.WriteAllTextDurable(
                    _paths.Temporary,
                    serialized);

                DecodeResult temporary = DecodeFile(_paths.Temporary);
                if (temporary.Status != DecodeStatus.Success || temporary.Dirty)
                {
                    throw new InvalidOperationException(
                        "The temporary save did not pass current-schema " +
                        "validation. " + temporary.Error.Diagnostic);
                }

                SaveReplacementResult replacement = ReplacePrimary();
                data.CopyFrom(candidate);
                return LocalSaveWriteResult.Success(replacement);
            }
            catch (Exception exception)
            {
                TryRestorePrimaryAfterFailure();
                TryDelete(_paths.Temporary);
                return LocalSaveWriteResult.Failure(
                    new ProductError(
                        ProductErrorCode.SaveWrite,
                        exception.Message,
                        true));
            }
        }

        private LocalSaveLoadResult LoadBackupAndRestorePrimary(
            string primaryDiagnostic = "")
        {
            DecodeResult backup = DecodeFile(_paths.Backup);
            if (backup.Status == DecodeStatus.FutureVersion)
            {
                return LocalSaveLoadResult.Failure(backup.Error);
            }
            if (backup.Status != DecodeStatus.Success)
            {
                return LocalSaveLoadResult.Failure(
                    new ProductError(
                        ProductErrorCode.CorruptPrimaryAndBackup,
                        "Primary: " + primaryDiagnostic +
                        " Backup: " + backup.Error.Diagnostic,
                        false));
            }

            try
            {
                _fileSystem.CopyFile(
                    _paths.Backup,
                    _paths.Temporary,
                    true);
                if (_fileSystem.FileExists(_paths.Primary))
                {
                    _fileSystem.DeleteFile(_paths.Primary);
                }
                _fileSystem.MoveFile(_paths.Temporary, _paths.Primary);
            }
            catch (Exception exception)
            {
                TryDelete(_paths.Temporary);
                return LocalSaveLoadResult.Failure(
                    new ProductError(
                        ProductErrorCode.SaveWrite,
                        "Backup was valid but primary restoration failed: " +
                        exception.Message,
                        true));
            }

            return LocalSaveLoadResult.Success(
                backup.Data,
                backup.Dirty,
                true);
        }

        private SaveReplacementResult ReplacePrimary()
        {
            if (!_fileSystem.FileExists(_paths.Primary))
            {
                _fileSystem.MoveFile(_paths.Temporary, _paths.Primary);
                return _replacementPolicy ==
                    SaveReplacementPolicy.AtomicPreferred
                    ? SaveReplacementResult.Atomic
                    : SaveReplacementResult.Recoverable;
            }

            if (_replacementPolicy == SaveReplacementPolicy.AtomicPreferred)
            {
                try
                {
                    _fileSystem.ReplaceFileAtomically(
                        _paths.Temporary,
                        _paths.Primary,
                        _paths.Backup);
                    return SaveReplacementResult.Atomic;
                }
                catch (PlatformNotSupportedException)
                {
                    // The platform explicitly cannot promise atomic replace.
                    // Fall through to the recoverable backup policy.
                }
                catch (NotSupportedException)
                {
                    // The platform explicitly cannot promise atomic replace.
                    // Fall through to the recoverable backup policy.
                }
            }

            _fileSystem.CopyFile(_paths.Primary, _paths.Backup, true);
            _fileSystem.DeleteFile(_paths.Primary);
            try
            {
                _fileSystem.MoveFile(_paths.Temporary, _paths.Primary);
            }
            catch
            {
                TryRestorePrimaryAfterFailure();
                throw;
            }
            return SaveReplacementResult.Recoverable;
        }

        private DecodeResult DecodeFile(string path)
        {
            try
            {
                string serialized = _fileSystem.ReadAllText(path);
                if (!_serializer.TryDeserialize(
                    serialized,
                    out LocalSaveData data,
                    out string diagnostic))
                {
                    return DecodeResult.Corrupt(
                        new ProductError(
                            ProductErrorCode.SaveRead,
                            diagnostic,
                            false));
                }

                return SaveDocumentPolicy.PrepareForLoad(data);
            }
            catch (Exception exception)
            {
                return DecodeResult.Corrupt(
                    new ProductError(
                        ProductErrorCode.SaveRead,
                        exception.Message,
                        false));
            }
        }

        private void TryRestorePrimaryAfterFailure()
        {
            try
            {
                if (!_fileSystem.FileExists(_paths.Primary) &&
                    _fileSystem.FileExists(_paths.Backup))
                {
                    _fileSystem.CopyFile(
                        _paths.Backup,
                        _paths.Primary,
                        true);
                }
            }
            catch
            {
                // The original typed save error remains authoritative.
            }
        }

        private void TryDelete(string path)
        {
            try
            {
                _fileSystem.DeleteFile(path);
            }
            catch
            {
                // Temporary cleanup cannot replace the original error.
            }
        }

        internal static string ToUtcString(DateTime value)
        {
            DateTime utc = value.Kind == DateTimeKind.Utc
                ? value
                : value.ToUniversalTime();
            return utc.ToString("O", CultureInfo.InvariantCulture);
        }
    }

    internal enum DecodeStatus
    {
        Success = 0,
        Corrupt = 1,
        FutureVersion = 2
    }

    internal readonly struct DecodeResult
    {
        private DecodeResult(
            DecodeStatus status,
            LocalSaveData data,
            bool dirty,
            ProductError error)
        {
            Status = status;
            Data = data;
            Dirty = dirty;
            Error = error;
        }

        internal DecodeStatus Status { get; }
        internal LocalSaveData Data { get; }
        internal bool Dirty { get; }
        internal ProductError Error { get; }

        internal static DecodeResult Success(LocalSaveData data, bool dirty)
        {
            return new DecodeResult(
                DecodeStatus.Success,
                data,
                dirty,
                ProductError.None);
        }

        internal static DecodeResult Corrupt(ProductError error)
        {
            return new DecodeResult(
                DecodeStatus.Corrupt,
                null,
                false,
                error);
        }

        internal static DecodeResult Future(ProductError error)
        {
            return new DecodeResult(
                DecodeStatus.FutureVersion,
                null,
                false,
                error);
        }
    }

    internal static class SaveDocumentPolicy
    {
        internal static DecodeResult PrepareForLoad(LocalSaveData data)
        {
            if (data == null)
            {
                return DecodeResult.Corrupt(
                    Error("The save root is missing."));
            }
            if (data.SchemaVersion > LocalSaveData.CurrentSchemaVersion)
            {
                return DecodeResult.Future(
                    new ProductError(
                        ProductErrorCode.FutureSchemaVersion,
                        $"Save schema {data.SchemaVersion} is newer than " +
                        $"supported schema {LocalSaveData.CurrentSchemaVersion}.",
                        false));
            }
            if (data.SchemaVersion < 0)
            {
                return DecodeResult.Corrupt(
                    Error("The save schema version is invalid."));
            }

            bool dirty = false;
            if (data.SchemaVersion == 0)
            {
                data.SchemaVersion = LocalSaveData.CurrentSchemaVersion;
                data.Settings ??= LocalSettingsData.CreateDefaults();
                if (data.Profile != null)
                {
                    data.Profile.SaveRevision = Math.Max(0, data.SaveRevision);
                }
                if (string.IsNullOrWhiteSpace(data.LastWriteUtc))
                {
                    data.LastWriteUtc = data.Profile?.LastPlayedUtc;
                }
                dirty = true;
            }

            if (data.Settings != null)
            {
                if (data.Settings.LastNonZeroMusicVolume == 0f)
                {
                    data.Settings.LastNonZeroMusicVolume =
                        data.Settings.MusicVolume > 0f
                            ? data.Settings.MusicVolume
                            : 1f;
                    dirty = true;
                }
                if (data.Settings.LastNonZeroSfxVolume == 0f)
                {
                    data.Settings.LastNonZeroSfxVolume =
                        data.Settings.SfxVolume > 0f
                            ? data.Settings.SfxVolume
                            : 1f;
                    dirty = true;
                }
            }

            if (!SettingsAreValid(data.Settings))
            {
                data.Settings = LocalSettingsData.CreateDefaults();
                dirty = true;
            }

            ProductError error = ValidateCurrent(data);
            if (error.IsError)
            {
                return DecodeResult.Corrupt(error);
            }

            return DecodeResult.Success(data, dirty);
        }

        internal static ProductError ValidateCurrent(LocalSaveData data)
        {
            if (data == null ||
                data.SchemaVersion != LocalSaveData.CurrentSchemaVersion)
            {
                return Error("The current save schema is invalid.");
            }
            if (data.SaveRevision < 0)
            {
                return Error("SaveRevision cannot be negative.");
            }
            if (data.Profile == null ||
                string.IsNullOrWhiteSpace(data.Profile.ProfileId) ||
                string.IsNullOrWhiteSpace(data.Profile.DisplayName))
            {
                return Error("The local profile identity is invalid.");
            }
            if (data.Profile.AccountState != LocalAccountState.Guest &&
                data.Profile.AccountState != LocalAccountState.LocalError)
            {
                return Error("The local account state is invalid.");
            }
            if (data.Profile.SaveRevision != data.SaveRevision)
            {
                return Error("Profile and root SaveRevision differ.");
            }
            if (!TryParseUtc(data.Profile.CreatedUtc) ||
                !TryParseUtc(data.Profile.LastPlayedUtc) ||
                !TryParseUtc(data.LastWriteUtc))
            {
                return Error("A required UTC timestamp is invalid.");
            }
            if (!SettingsAreValid(data.Settings))
            {
                return Error("The settings section is invalid.");
            }

            return ProductError.None;
        }

        private static bool SettingsAreValid(LocalSettingsData settings)
        {
            return settings != null &&
                IsVolume(settings.MasterVolume) &&
                IsVolume(settings.MusicVolume) &&
                IsVolume(settings.SfxVolume) &&
                IsActiveVolume(settings.LastNonZeroMusicVolume) &&
                IsActiveVolume(settings.LastNonZeroSfxVolume) &&
                !string.IsNullOrWhiteSpace(settings.Language);
        }

        private static bool IsActiveVolume(float value)
        {
            return IsVolume(value) && value > 0f;
        }

        private static bool IsVolume(float value)
        {
            return !float.IsNaN(value) &&
                !float.IsInfinity(value) &&
                value >= 0f &&
                value <= 1f;
        }

        private static bool TryParseUtc(string value)
        {
            return DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out DateTime parsed) &&
                parsed.Kind == DateTimeKind.Utc;
        }

        private static ProductError Error(string diagnostic)
        {
            return new ProductError(
                ProductErrorCode.SaveValidation,
                diagnostic,
                false);
        }
    }
}
