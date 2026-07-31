using System;
using ColorGateRunner.Product;
using UnityEngine;

namespace ColorGateRunner.Presentation
{
    public sealed class AppRoot : MonoBehaviour
    {
        internal const string SaveFileName = "product-save.json";

        private static AppRoot _active;
        private static Func<AppServiceGraph> _testGraphFactory;

        private AppServiceGraph _graph;
        private bool _ownsActiveSlot;

        public bool IsPrimary => _active == this;
        public AppServiceGraph Graph => _graph;

        private void Awake()
        {
            if (_active != null && _active != this)
            {
                Destroy(gameObject);
                return;
            }

            _active = this;
            _ownsActiveSlot = true;
            _graph = _testGraphFactory != null
                ? _testGraphFactory()
                : CreateProductionGraph();
            if (_graph == null)
            {
                throw new InvalidOperationException(
                    "The AppRoot service graph factory returned null.");
            }

            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (_ownsActiveSlot && _active == this)
            {
                _active = null;
            }
        }

        public AppInitializationResult Initialize()
        {
            if (_graph == null)
            {
                return AppInitializationResult.Failure(
                    Array.Empty<InitializationStep>(),
                    new ProductError(
                        ProductErrorCode.Initialization,
                        "The AppRoot service graph is unavailable.",
                        true));
            }

            return _graph.Initialization.Initialize();
        }

        internal static bool TryGetActive(out AppRoot appRoot)
        {
            appRoot = _active;
            return appRoot != null;
        }

        internal static void SetTestGraphFactory(
            Func<AppServiceGraph> factory)
        {
            _testGraphFactory = factory;
        }

        internal static void ClearTestState()
        {
            _testGraphFactory = null;
            _active = null;
        }

        private static AppServiceGraph CreateProductionGraph()
        {
            var clock = new SystemClockService();
            var profile = new ProfileService(
                clock,
                new GuidProfileIdGenerator());
            var settings = new SettingsService();
            var account = new LocalAccountService();
            var paths = new LocalSavePaths(
                Application.persistentDataPath,
                SaveFileName);
            SaveReplacementPolicy policy =
                Application.platform == RuntimePlatform.WebGLPlayer
                    ? SaveReplacementPolicy.RecoverableOnly
                    : SaveReplacementPolicy.AtomicPreferred;
            var save = new LocalSaveService(
                new UnityJsonSaveDocumentSerializer(),
                new SystemLocalSaveFileSystem(),
                clock,
                paths,
                policy);
            return new AppServiceGraph(
                clock,
                profile,
                account,
                settings,
                save,
                new AppInitializationPipeline(
                    save,
                    profile,
                    settings,
                    account));
        }
    }

    public sealed class AppServiceGraph
    {
        public AppServiceGraph(
            IClockService clock,
            ProfileService profile,
            LocalAccountService account,
            SettingsService settings,
            ILocalSaveService save,
            AppInitializationPipeline initialization)
        {
            Clock = clock ?? throw new ArgumentNullException(nameof(clock));
            Profile = profile ??
                throw new ArgumentNullException(nameof(profile));
            Account = account ??
                throw new ArgumentNullException(nameof(account));
            Settings = settings ??
                throw new ArgumentNullException(nameof(settings));
            Save = save ?? throw new ArgumentNullException(nameof(save));
            Initialization = initialization ??
                throw new ArgumentNullException(nameof(initialization));
        }

        public IClockService Clock { get; }
        public ProfileService Profile { get; }
        public LocalAccountService Account { get; }
        public SettingsService Settings { get; }
        public ILocalSaveService Save { get; }
        public AppInitializationPipeline Initialization { get; }
    }
}
