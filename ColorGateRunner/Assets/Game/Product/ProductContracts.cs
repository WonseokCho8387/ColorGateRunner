using System;
using System.Collections.Generic;

namespace ColorGateRunner.Product
{
    public enum ProductErrorCode
    {
        None = 0,
        SaveRead = 1,
        SaveWrite = 2,
        SaveValidation = 3,
        SaveMigration = 4,
        FutureSchemaVersion = 5,
        CorruptPrimaryAndBackup = 6,
        Initialization = 7,
        UnsupportedAccountOperation = 8,
        InsufficientInventory = 9,
        InsufficientFunds = 10
    }

    public readonly struct ProductError
    {
        public ProductError(
            ProductErrorCode code,
            string diagnostic,
            bool recoverable)
        {
            Code = code;
            Diagnostic = diagnostic ?? string.Empty;
            Recoverable = recoverable;
        }

        public ProductErrorCode Code { get; }
        public string Diagnostic { get; }
        public bool Recoverable { get; }
        public bool IsError => Code != ProductErrorCode.None;

        public static ProductError None =>
            new ProductError(ProductErrorCode.None, string.Empty, false);
    }

    public readonly struct ProductMutationResult
    {
        private ProductMutationResult(
            bool succeeded,
            bool changed,
            ProductError error)
        {
            Succeeded = succeeded;
            Changed = changed;
            Error = error;
        }

        public bool Succeeded { get; }
        public bool Changed { get; }
        public ProductError Error { get; }

        public static ProductMutationResult Success(bool changed) =>
            new ProductMutationResult(true, changed, ProductError.None);

        public static ProductMutationResult Failure(ProductError error) =>
            new ProductMutationResult(false, false, error);
    }

    public interface IAccountProviderAvailability
    {
        bool IsAvailable(string provider);
    }

    public interface IClockService
    {
        DateTime UtcNow { get; }
    }

    public interface IProfileIdGenerator
    {
        string CreateProfileId();
    }

    public interface ISaveDocumentSerializer
    {
        string Serialize(LocalSaveData data);

        bool TryDeserialize(
            string serialized,
            out LocalSaveData data,
            out string diagnostic);
    }

    public interface ILocalSaveFileSystem
    {
        bool FileExists(string path);
        string ReadAllText(string path);
        void WriteAllTextDurable(string path, string contents);
        void CopyFile(string source, string destination, bool overwrite);
        void MoveFile(string source, string destination);
        void DeleteFile(string path);
        void ReplaceFileAtomically(
            string source,
            string destination,
            string backup);
    }

    public enum SaveReplacementPolicy
    {
        AtomicPreferred = 0,
        RecoverableOnly = 1
    }

    public enum SaveReplacementResult
    {
        None = 0,
        Atomic = 1,
        Recoverable = 2
    }

    public readonly struct LocalSavePaths
    {
        public LocalSavePaths(string directory, string fileName)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new ArgumentException(
                    "A save directory is required.",
                    nameof(directory));
            }
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException(
                    "A save file name is required.",
                    nameof(fileName));
            }

            Directory = directory;
            Primary = System.IO.Path.Combine(directory, fileName);
            Temporary = Primary + ".tmp";
            Backup = Primary + ".bak";
        }

        public string Directory { get; }
        public string Primary { get; }
        public string Temporary { get; }
        public string Backup { get; }
    }

    public readonly struct LocalSaveLoadResult
    {
        private LocalSaveLoadResult(
            bool succeeded,
            LocalSaveData data,
            bool dirty,
            bool recoveredFromBackup,
            ProductError error)
        {
            Succeeded = succeeded;
            Data = data;
            Dirty = dirty;
            RecoveredFromBackup = recoveredFromBackup;
            Error = error;
        }

        public bool Succeeded { get; }
        public LocalSaveData Data { get; }
        public bool Dirty { get; }
        public bool RecoveredFromBackup { get; }
        public ProductError Error { get; }

        public static LocalSaveLoadResult Success(
            LocalSaveData data,
            bool dirty,
            bool recoveredFromBackup)
        {
            return new LocalSaveLoadResult(
                true,
                data,
                dirty,
                recoveredFromBackup,
                ProductError.None);
        }

        public static LocalSaveLoadResult Failure(ProductError error)
        {
            return new LocalSaveLoadResult(false, null, false, false, error);
        }
    }

    public readonly struct LocalSaveWriteResult
    {
        private LocalSaveWriteResult(
            bool succeeded,
            SaveReplacementResult replacement,
            ProductError error)
        {
            Succeeded = succeeded;
            Replacement = replacement;
            Error = error;
        }

        public bool Succeeded { get; }
        public SaveReplacementResult Replacement { get; }
        public ProductError Error { get; }

        public static LocalSaveWriteResult Success(
            SaveReplacementResult replacement)
        {
            return new LocalSaveWriteResult(
                true,
                replacement,
                ProductError.None);
        }

        public static LocalSaveWriteResult Failure(ProductError error)
        {
            return new LocalSaveWriteResult(
                false,
                SaveReplacementResult.None,
                error);
        }
    }

    public interface ILocalSaveService
    {
        LocalSaveLoadResult Load();
        LocalSaveWriteResult Save(LocalSaveData data);
    }

    public enum InitializationStep
    {
        ClockAndPaths = 0,
        SaveLoad = 1,
        Profile = 2,
        Settings = 3,
        Progression = 4,
        PersistDirtyData = 5,
        Complete = 6
    }

    public readonly struct AppInitializationResult
    {
        private AppInitializationResult(
            bool succeeded,
            LocalSaveData data,
            IReadOnlyList<InitializationStep> steps,
            ProductError error)
        {
            Succeeded = succeeded;
            Data = data;
            Steps = steps;
            Error = error;
        }

        public bool Succeeded { get; }
        public LocalSaveData Data { get; }
        public IReadOnlyList<InitializationStep> Steps { get; }
        public ProductError Error { get; }

        public static AppInitializationResult Success(
            LocalSaveData data,
            IReadOnlyList<InitializationStep> steps)
        {
            return new AppInitializationResult(
                true,
                data,
                steps,
                ProductError.None);
        }

        public static AppInitializationResult Failure(
            IReadOnlyList<InitializationStep> steps,
            ProductError error)
        {
            return new AppInitializationResult(false, null, steps, error);
        }
    }
}
