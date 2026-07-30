using System;

namespace ColorGateRunner.Core
{
    public interface IStageCatalog
    {
        int Count { get; }
        StageDefinition GetByIndex(int index);
        StageDefinition GetByDisplayNumber(int displayNumber);
        StageDefinition GetById(string stageId);
    }

    public sealed class InMemoryStageCatalog : IStageCatalog
    {
        private readonly StageDefinition[] _stages;

        public InMemoryStageCatalog(StageDefinition[] stages)
        {
            if (stages == null)
            {
                throw new ArgumentNullException(nameof(stages));
            }

            _stages = new StageDefinition[stages.Length];
            Array.Copy(stages, _stages, stages.Length);
        }

        public int Count => _stages.Length;

        public StageDefinition GetByIndex(int index)
        {
            if (index < 0 || index >= _stages.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return _stages[index];
        }

        public StageDefinition GetByDisplayNumber(int displayNumber)
        {
            for (int index = 0; index < _stages.Length; index++)
            {
                if (_stages[index].DisplayNumber == displayNumber)
                {
                    return _stages[index];
                }
            }

            throw new ArgumentOutOfRangeException(nameof(displayNumber));
        }

        public StageDefinition GetById(string stageId)
        {
            if (string.IsNullOrEmpty(stageId))
            {
                throw new ArgumentException(
                    "Stage ID is required.",
                    nameof(stageId));
            }
            for (int index = 0; index < _stages.Length; index++)
            {
                if (_stages[index].StageId == stageId)
                {
                    return _stages[index];
                }
            }

            throw new ArgumentException("Unknown stage ID.", nameof(stageId));
        }
    }

    // Compatibility facade for existing runtime and simulation call sites.
    // Production stage values are supplied by StageCatalogAsset through an
    // IStageCatalog adapter; no stage content is embedded in Core.
    public static class StageCatalog
    {
        private static IStageCatalog _current;

        public static bool IsConfigured => _current != null;
        public static int Count => Current.Count;

        public static IStageCatalog Current
        {
            get
            {
                if (_current == null)
                {
                    throw new InvalidOperationException(
                        "StageCatalog has not been configured.");
                }

                return _current;
            }
        }

        public static void Configure(IStageCatalog catalog)
        {
            _current = catalog ??
                throw new ArgumentNullException(nameof(catalog));
        }

        public static StageDefinition GetByIndex(int index)
        {
            return Current.GetByIndex(index);
        }

        public static StageDefinition GetByDisplayNumber(int displayNumber)
        {
            return Current.GetByDisplayNumber(displayNumber);
        }

        public static StageDefinition GetById(string stageId)
        {
            return Current.GetById(stageId);
        }
    }
}
