using System;
using System.Collections.Generic;
using ColorGateRunner.Core;
using ColorGateRunner.Product;
using UnityEngine;

namespace ColorGateRunner.Presentation
{
    public sealed class AppRoot : MonoBehaviour,
        IProductSettingsHost,
        ICampaignLaunchHost
    {
        internal const string SaveFileName = "product-save.json";

        private static AppRoot _active;
        private static Func<AppServiceGraph> _testGraphFactory;

        private AppServiceGraph _graph;
        private readonly CampaignLaunchContext _campaignLaunch =
            new CampaignLaunchContext();
        private bool _ownsActiveSlot;

        public bool IsPrimary => _active == this;
        public AppServiceGraph Graph => _graph;
        public LocalSettingsData CurrentSettings => _graph?.Settings?.Current;
        public bool AccountChoiceCompleted =>
            _graph?.Profile?.Current?.AccountChoiceCompleted == true;
        public bool VibrationEnabled => CurrentSettings?.Vibration ?? true;

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

            AppInitializationResult result =
                _graph.Initialization.Initialize();
            if (result.Succeeded)
            {
                ProductMutationResult migration =
                    MigrateLegacyCampaignIfRequired();
                if (!migration.Succeeded)
                {
                    return AppInitializationResult.Failure(
                        result.Steps,
                        migration.Error);
                }
                UnityProductSettingsRuntime.ApplyMasterVolume(
                    _graph.Settings.Current);
            }
            return result;
        }

        public ProductMutationResult CompleteGuestAccountChoice()
        {
            return _graph == null
                ? ProductMutationResult.Failure(
                    new ProductError(
                        ProductErrorCode.Initialization,
                        "The AppRoot service graph is unavailable.",
                        true))
                : _graph.ProductSession.CompleteGuestAccountChoice();
        }

        public ProductMutationResult ApplySettings(
            float masterVolume,
            float musicVolume,
            float sfxVolume,
            bool vibration,
            bool notificationEnabled)
        {
            if (_graph == null)
            {
                return ProductMutationResult.Failure(
                    new ProductError(
                        ProductErrorCode.Initialization,
                        "The AppRoot service graph is unavailable.",
                        true));
            }

            ProductMutationResult result = _graph.ProductSession.ApplySettings(
                masterVolume,
                musicVolume,
                sfxVolume,
                vibration,
                notificationEnabled);
            if (result.Succeeded)
            {
                UnityProductSettingsRuntime.ApplyMasterVolume(
                    _graph.Settings.Current);
            }
            return result;
        }

        public ProductMutationResult AcknowledgeLobbyMilestones()
        {
            return _graph == null
                ? ProductMutationResult.Failure(
                    new ProductError(
                        ProductErrorCode.Initialization,
                        "The AppRoot service graph is unavailable.",
                        true))
                : _graph.ProductSession.AcknowledgeLobbyMilestones();
        }

        public bool IsAccountProviderAvailable(string provider)
        {
            return _graph != null &&
                _graph.Account.IsProviderAvailable(provider);
        }

        internal bool TryQueueCampaignLaunch(
            string stageId,
            CampaignRunKind runKind = CampaignRunKind.Authored)
        {
            return IsPrimary && _campaignLaunch.TrySet(
                stageId,
                false,
                runKind);
        }

#if UNITY_EDITOR
        internal bool TryQueueDevelopmentCampaignLaunch(string stageId)
        {
            return IsPrimary && _campaignLaunch.TrySet(stageId, true);
        }

        internal static AppServiceGraph CreateDevelopmentGraph()
        {
            return CreateProductionGraph();
        }
#endif

        internal bool TryConsumeCampaignLaunch(
            out CampaignLaunchRequest request)
        {
            if (!IsPrimary)
            {
                request = default;
                return false;
            }

            return _campaignLaunch.TryConsume(out request);
        }

        internal bool TryCancelCampaignLaunch(string stageId)
        {
            return IsPrimary && _campaignLaunch.TryCancel(stageId);
        }

        bool ICampaignLaunchHost.TryQueueCampaignLaunch(
            string stageId,
            CampaignRunKind runKind)
        {
            return TryQueueCampaignLaunch(stageId, runKind);
        }

        bool ICampaignLaunchHost.TryCancelCampaignLaunch(string stageId)
        {
            return TryCancelCampaignLaunch(stageId);
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
            var progression = new ProgressionService();
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
            var initialization = new AppInitializationPipeline(
                save,
                profile,
                settings,
                account,
                progression);
            return new AppServiceGraph(
                clock,
                profile,
                account,
                settings,
                save,
                initialization,
                true);
        }

        private ProductMutationResult MigrateLegacyCampaignIfRequired()
        {
            if (_graph == null || !_graph.LegacyCampaignMigrationEnabled ||
                _graph.Progression.Campaign.LegacyMigrationCompleted)
            {
                return ProductMutationResult.Success(false);
            }

            StageCatalogProvider.EnsureConfigured();
            IStageCatalog catalog = StageCatalog.Current;
            var legacy = new PlayerPrefsStageProgressStore();
            int highestNumber = legacy.LoadHighestUnlocked();
            StageDefinition highest =
                catalog.GetByDisplayNumber(highestNumber);
            var entries = new List<CampaignProgressImportEntry>(catalog.Count);
            for (int index = 0; index < catalog.Count; index++)
            {
                StageDefinition stage = catalog.GetByIndex(index);
                StageRecord record = legacy.LoadRecord(stage.DisplayNumber);
                entries.Add(new CampaignProgressImportEntry(
                    stage.DisplayNumber,
                    new LocalStageProgressData
                    {
                        StageId = stage.StageId,
                        Cleared = record.Cleared,
                        BestTime = record.BestTime,
                        BestNoItemTime = record.BestNoItemTime,
                        ClearCount = record.ClearCount,
                        ContinuedClearCount = record.ContinuedClearCount
                    },
                    stage.Difficulty switch
                    {
                        StageDifficulty.Normal => StageRewardDifficulty.Normal,
                        StageDifficulty.Hard => StageRewardDifficulty.Hard,
                        StageDifficulty.VeryHard => StageRewardDifficulty.VeryHard,
                        _ => throw new ArgumentOutOfRangeException()
                    }));
            }
            return _graph.ProductSession.ImportLegacyCampaign(
                highest.StageId,
                entries);
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
            : this(
                clock,
                profile,
                account,
                settings,
                save,
                initialization,
                false)
        {
        }

        public AppServiceGraph(
            IClockService clock,
            ProfileService profile,
            LocalAccountService account,
            SettingsService settings,
            ILocalSaveService save,
            AppInitializationPipeline initialization,
            bool legacyCampaignMigrationEnabled)
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
            LegacyCampaignMigrationEnabled = legacyCampaignMigrationEnabled;
        }

        public IClockService Clock { get; }
        public ProfileService Profile { get; }
        public LocalAccountService Account { get; }
        public SettingsService Settings { get; }
        public ILocalSaveService Save { get; }
        public AppInitializationPipeline Initialization { get; }
        public LocalProductSession ProductSession => Initialization.Session;
        public ProgressionService Progression => Initialization.Progression;
        public bool LegacyCampaignMigrationEnabled { get; }
    }
}
