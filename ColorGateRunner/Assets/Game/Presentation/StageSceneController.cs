using System;
using ColorGateRunner.Core;
using ColorGateRunner.Product;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    public sealed class StageSceneController : MonoBehaviour
    {
        private const float CountdownDuration = 3f;
        private const float CampaignInitialGateLeadDistance = 48f;
        private const float ExperimentInitialGateLeadDistance = 24f;
        private const float CampaignGoalDistance = 40f;
        private const float FailurePanelDelay = 1f;
        private const float ClearPanelDelay = 1.2f;
        private const float BoosterWarningThreshold = 0.2f;
        private const float BoosterFov = 78f;
        private const float NormalFov = 60f;
        private const float BoosterCameraBlendIn = 0.22f;
        private const float BoosterCameraBlendOut = 0.35f;
        private const float ColorStackTransitionDuration = 0.12f;
        private static readonly Vector3 BoosterCameraFollowOffset =
            new Vector3(0f, 5.4f, -7.4f);
        private static readonly Quaternion BoosterCameraRotation =
            Quaternion.Euler(14f, 0f, 0f);
        private static readonly float[] ColorStackPositions =
            { -34f, -91f, -137f, -176f, -211f, -243f };

        [SerializeField] private StageCatalogAsset stageCatalogAsset;
        [SerializeField] private Transform player;
        [SerializeField] private Renderer playerRenderer;
        [SerializeField] private Rigidbody playerBody;
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private Material redMaterial;
        [SerializeField] private Material blueMaterial;
        [SerializeField] private Material greenMaterial;
        [SerializeField] private Material yellowMaterial;
        [SerializeField] private Material purpleMaterial;
        [SerializeField] private Material cyanMaterial;
        [SerializeField] private Material failureMaterial;
        [SerializeField] private GameplayTapSurface tapSurface;
        [SerializeField] private TrackPoolController trackPool;
        [SerializeField] private StageGateView[] gates;
        [SerializeField] private GameObject goal;
        [SerializeField] private GameObject shieldVisual;
        [SerializeField] private GameObject echoShellVisual;
        [SerializeField] private Renderer[] echoShellRenderers;
        [SerializeField] private ParticleSystem successParticles;
        [SerializeField] private ParticleSystem speedLines;
        [SerializeField] private TrailRenderer playerTrail;
        [SerializeField] private bool developmentTelemetryEnabled;
        [SerializeField] private GameObject[] uiFlowRoots;

        [SerializeField] private GameObject lobbyPanel;
        [SerializeField] private Text lobbyStageText;
        [SerializeField] private Text lobbyStageTitleText;
        [SerializeField] private Text lobbyStageDescriptionText;
        [SerializeField] private Text lobbyProgressText;
        [SerializeField] private Text lobbyTierText;
        [SerializeField] private Button lobbyPlayButton;
        [SerializeField] private Button experimentLabButton;
        [SerializeField] private GameObject[] lobbyTierRoots;
        [SerializeField] private Button resetProgressButton;
        [SerializeField] private GameObject resetProgressConfirmation;
        [SerializeField] private Button confirmResetProgressButton;
        [SerializeField] private Button cancelResetProgressButton;

        [SerializeField] private GameObject stageSelectPanel;
        [SerializeField] private Button[] stageButtons;
        [SerializeField] private Text[] stageSummaryTexts;
        [SerializeField] private Button developerUnlockAllButton;

        [SerializeField] private GameObject itemPanel;
        [SerializeField] private Text selectedStageText;
        [SerializeField] private Button shieldToggleButton;
        [SerializeField] private Text shieldToggleText;
        [SerializeField] private Button boosterToggleButton;
        [SerializeField] private Text boosterToggleText;
        [SerializeField] private Text preRunStatusText;
        [SerializeField] private Button startButton;
        [SerializeField] private Button backButton;

        [SerializeField] private GameObject countdownPanel;
        [SerializeField] private Text countdownText;
        [SerializeField] private GameObject stageHud;
        [SerializeField] private Text stageHudText;
        [SerializeField] private Text progressText;
        [SerializeField] private Image progressFill;
        [SerializeField] private GameObject shieldIcon;
        [SerializeField] private GameObject boosterMeterRoot;
        [SerializeField] private Image boosterMeterFill;
        [SerializeField] private GameObject boosterWarning;
        [SerializeField] private GameObject colorHudPanel;
        [SerializeField] private GameObject[] colorTiles;
        [SerializeField] private Image[] colorTileImages;
        [SerializeField] private Text[] colorTileSymbols;
        [SerializeField] private GameObject[] nextColorMarkers;

        [SerializeField] private GameObject clearPanel;
        [SerializeField] private Text clearTitleText;
        [SerializeField] private Text clearDetailsText;
        [SerializeField] private Button clearContinueButton;
        [SerializeField] private Button replayButton;
        [SerializeField] private Button clearLobbyButton;

        [SerializeField] private GameObject failPanel;
        [SerializeField] private Text failTitleText;
        [SerializeField] private Text failDetailsText;
        [SerializeField] private Text failContinueStatusText;
        [SerializeField] private Button coinContinueButton;
        [SerializeField] private Button rewardedContinueButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button failLobbyButton;

        [SerializeField] private Button pauseButton;
        [SerializeField] private GameObject pauseOverlayRoot;
        [SerializeField] private Image pauseDim;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button pauseResumeButton;
        [SerializeField] private Button pauseRestartButton;
        [SerializeField] private Button pauseSettingsButton;
        [SerializeField] private Button pauseLobbyButton;
        [SerializeField] private GameObject pauseModalRoot;
        [SerializeField] private Text pauseModalTitleText;
        [SerializeField] private Text pauseModalMessageText;
        [SerializeField] private Button pauseModalConfirmButton;
        [SerializeField] private Text pauseModalConfirmText;
        [SerializeField] private Button pauseModalCancelButton;
        [SerializeField] private SettingsPanelController pauseSettingsPanel;
        [SerializeField] private GameObject pauseTransitionBlocker;
        [SerializeField] private ParticleSystem[] attemptEffects;
        [SerializeField] private string frontendScenePath;

        private StageSession _session;
        private IStageProgressStore _progressStore;
        private IStartItemInventoryGateway _startItemInventory;
        private IContinueEconomyGateway _continueEconomy;
        private IRewardedAdService _rewardedAdService;
        private AttemptContinuePolicy _attemptContinuePolicy;
        private StartItemInventorySnapshot _startItemInventorySnapshot;
        private string _startItemStartError = string.Empty;
        private string _clearSaveError = string.Empty;
        private IHapticFeedback _haptics;
        private DevelopmentTelemetry _telemetry;
        private int _highestUnlocked;
        private int _selectedStageNumber = 1;
        private string _selectedStageId;
        private int _lobbyTier;
        private bool _shieldSelected;
        private bool _boosterSelected;
        private float _countdownRemaining;
        private float _nextGateZ;
        private int _nextPlanIndex;
        private float _cameraShakeRemaining;
        private float _failureDelayRemaining;
        private float _clearDelayRemaining;
        private float _boosterLaunchPulse;
        private bool _wasBoosterActive;
        private Vector3 _playerStartPosition;
        private Vector3 _cameraStartPosition;
        private Quaternion _cameraStartRotation;
        private Vector3 _cameraFollowOffset;
        private bool _clearRecorded;
        private bool _boosterExitOverridesApplied;
        private StageGateView _failedGate;
        private ContinueSnapshot _continueSnapshot;
        private string _continueAttemptId = string.Empty;
        private string _continueStatus = string.Empty;
        private int _continueRequestGeneration;
        private bool _continueRequestPending;
        private MobileUiFlow _uiFlow;
        private ExperimentSession _experimentSession;
        private bool _experimentActive;
        private Material _normalTrackMaterial;
        private float _boosterCameraBlend;
        private RunnerColor _lastStackColor;
        private int _lastStackActiveCount;
        private bool _colorStackInitialized;
        private float _colorStackTransitionRemaining;
        private Vector2[] _colorStackStartPositions;
        private Vector3[] _colorStackStartScales;
        private UnityAction[] _stageButtonListeners;
        private GameplayPauseCoordinator _pauseCoordinator;
        private ISceneTransitionLoader _sceneTransitionLoader;
        private bool[] _pausedEffectWasPlaying;
        private string _pauseSceneLoadError = string.Empty;
        private bool _enteredFromFrontendLaunch;
        private bool _preRunReturnTransitioning;
        private string _preRunReturnError = string.Empty;

        internal StageSession Session => _session;
        internal int HighestUnlocked => _highestUnlocked;
        internal int SelectedStageNumber => _selectedStageNumber;
        internal int LobbyTier => _lobbyTier;
        internal bool ShieldSelected => _shieldSelected;
        internal bool BoosterSelected => _boosterSelected;
        internal GameObject LobbyPanel => lobbyPanel;
        internal GameObject StageSelectPanel => stageSelectPanel;
        internal GameObject ItemPanel => itemPanel;
        internal GameObject CountdownPanel => countdownPanel;
        internal GameObject StageHud => stageHud;
        internal GameObject ClearPanel => clearPanel;
        internal GameObject FailPanel => failPanel;
        internal GameObject Goal => goal;
        internal Button ExperimentLabButton => experimentLabButton;
        internal GameObject ShieldVisual => shieldVisual;
        internal GameObject EchoShellVisual => echoShellVisual;
        internal GameObject BoosterWarning => boosterWarning;
        internal GameObject BoosterMeterRoot => boosterMeterRoot;
        internal Image BoosterMeterFill => boosterMeterFill;
        internal Text LobbyStageText => lobbyStageText;
        internal Text LobbyStageTitleText => lobbyStageTitleText;
        internal Text LobbyStageDescriptionText => lobbyStageDescriptionText;
        internal ParticleSystem SpeedLines => speedLines;
        internal TrailRenderer PlayerTrail => playerTrail;
        internal Text CountdownText => countdownText;
        internal Text ProgressText => progressText;
        internal Text ClearDetailsText => clearDetailsText;
        internal Text FailDetailsText => failDetailsText;
        internal Button ShieldToggleButton => shieldToggleButton;
        internal Text ShieldToggleText => shieldToggleText;
        internal Button BoosterToggleButton => boosterToggleButton;
        internal Text BoosterToggleText => boosterToggleText;
        internal Text PreRunStatusText => preRunStatusText;
        internal Button CoinContinueButton => coinContinueButton;
        internal Button RewardedContinueButton => rewardedContinueButton;
        internal Text FailContinueStatusText => failContinueStatusText;
        internal Button RetryButton => retryButton;
        internal Button ReplayButton => replayButton;
        internal Button ClearLobbyButton => clearLobbyButton;
        internal Button FailLobbyButton => failLobbyButton;
        internal Transform PlayerTransform => player;
        internal Renderer PlayerRenderer => playerRenderer;
        internal Camera GameplayCamera => gameplayCamera;
        internal int GatePoolSize => gates == null ? 0 : gates.Length;
        internal float FailurePanelDelaySeconds => FailurePanelDelay;
        internal float ClearPanelDelaySeconds => ClearPanelDelay;
        internal int NextPlanIndex => _nextPlanIndex;
        internal float NextGateZ => _nextGateZ;
        internal ContinueSnapshot FailureSnapshot => _continueSnapshot;
        internal TrackPoolController TrackPool => trackPool;
        internal bool BoosterExitOverridesApplied =>
            _boosterExitOverridesApplied;
        internal MobileUiFlow UiFlow => _uiFlow;
        internal GameObject LobbyRoot => uiFlowRoots[0];
        internal GameObject PreRunRoot => uiFlowRoots[1];
        internal GameObject GameplayHudRoot => uiFlowRoots[2];
        internal GameObject CountdownRoot => uiFlowRoots[3];
        internal GameObject ClearResultRoot => uiFlowRoots[4];
        internal GameObject FailedResultRoot => uiFlowRoots[5];
        internal GameObject DevelopmentDebugRoot => uiFlowRoots[6];
        internal int ActiveColorTileCount
        {
            get
            {
                int count = 0;
                for (int index = 0; index < colorTiles.Length; index++)
                {
                    count += colorTiles[index].activeSelf ? 1 : 0;
                }
                return count;
            }
        }

        internal int ActiveGateCount
        {
            get
            {
                int count = 0;
                for (int index = 0; index < gates.Length; index++)
                {
                    if (gates[index].gameObject.activeSelf)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        internal StageGateView GetGate(int index) => gates[index];
        internal Button GetStageButton(int index) => stageButtons[index];
        internal GameObject GetColorTile(int index) => colorTiles[index];
        internal Image GetColorTileImage(int index) => colorTileImages[index];
        internal Text GetColorTileSymbol(int index) => colorTileSymbols[index];
        internal GameObject GetNextColorMarker(int index) =>
            nextColorMarkers[index];
        internal Material GetPresentationMaterial(RunnerColor color) =>
            GetMaterial(color);
        internal GameObject ResetProgressConfirmation =>
            resetProgressConfirmation;
        internal float BoosterCameraBlend => _boosterCameraBlend;
        internal bool ExperimentActive => _experimentActive;
        internal ExperimentSession ExperimentSession => _experimentSession;
        internal Vector3 NormalCameraPosition => _cameraStartPosition;
        internal Quaternion NormalCameraRotation => _cameraStartRotation;
        internal GameplayPauseCoordinator PauseCoordinator => _pauseCoordinator;
        internal GameObject PauseOverlayRoot => pauseOverlayRoot;
        internal Image PauseDim => pauseDim;
        internal Button PauseButton => pauseButton;
        internal SettingsPanelController PauseSettingsPanel => pauseSettingsPanel;
        internal string FrontendScenePath => frontendScenePath;
        internal Button PreRunBackButton => backButton;
        internal bool EnteredFromFrontendLaunch => _enteredFromFrontendLaunch;
        internal bool PreRunReturnTransitioning => _preRunReturnTransitioning;
        internal string PreRunReturnError => _preRunReturnError;

        internal bool IsPlayerCollider(Collider other)
        {
            return other.transform == player ||
                other.transform.IsChildOf(player);
        }

        private static bool IsVibrationEnabled()
        {
            return !AppRoot.TryGetActive(out AppRoot root) ||
                root.VibrationEnabled;
        }

        private void Awake()
        {
            if (stageCatalogAsset != null)
            {
                StageCatalogProvider.Configure(stageCatalogAsset);
            }
            else
            {
                StageCatalogProvider.EnsureConfigured();
            }
            ValidateRequiredReferences();
            _pauseCoordinator = new GameplayPauseCoordinator();
            _sceneTransitionLoader ??= new UnitySceneTransitionLoader();
            _pausedEffectWasPlaying = new bool[attemptEffects.Length];
            _progressStore = AppRoot.TryGetActive(out AppRoot appRoot) &&
                appRoot.Graph?.Progression?.Campaign != null &&
                appRoot.Graph.Progression.Campaign.LegacyMigrationCompleted
                    ? new ProductStageProgressStore(
                        appRoot.Graph.ProductSession,
                        appRoot.Graph.Progression,
                        StageCatalog.Current)
                    : new PlayerPrefsStageProgressStore();
            _startItemInventory = appRoot != null &&
                appRoot.Graph?.ProductSession?.IsReady == true &&
                appRoot.Graph.Progression?.Economy != null
                    ? new ProductStartItemInventoryGateway(
                        appRoot.Graph.ProductSession,
                        appRoot.Graph.Progression)
                    : new UnavailableStartItemInventoryGateway();
            _continueEconomy = appRoot != null &&
                appRoot.Graph?.ProductSession?.IsReady == true &&
                appRoot.Graph.Progression?.Economy != null
                    ? new ProductContinueEconomyGateway(
                        appRoot.Graph.ProductSession,
                        appRoot.Graph.Progression)
                    : new UnavailableContinueEconomyGateway();
            _rewardedAdService = new UnavailableRewardedAdService();
            _attemptContinuePolicy = new AttemptContinuePolicy();
            RefreshStartItemInventory();
            _haptics = new UnityHapticFeedback(IsVibrationEnabled);
            _telemetry = new DevelopmentTelemetry(developmentTelemetryEnabled);
            _highestUnlocked = _progressStore.LoadHighestUnlocked();
            _selectedStageId =
                StageCatalog.GetByDisplayNumber(_selectedStageNumber).StageId;
            _playerStartPosition = player.position;
            _cameraStartPosition = gameplayCamera.transform.position;
            _cameraStartRotation = gameplayCamera.transform.rotation;
            _cameraFollowOffset = _cameraStartPosition - _playerStartPosition;
            _normalTrackMaterial = trackPool.GetSegment(0).SurfaceMaterial;
            _colorStackStartPositions = new Vector2[colorTiles.Length];
            _colorStackStartScales = new Vector3[colorTiles.Length];
            AddListeners();
            if (!TryEnterExternalCampaignLaunch())
            {
                ShowLobby();
            }
        }

        private void OnDestroy()
        {
            RemoveListeners();
            _telemetry?.Flush();
        }

        private void Update()
        {
            if (Keyboard.current != null &&
                Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                HandleBack();
            }
            Tick(Time.deltaTime);
        }

        internal void Tick(float deltaTime)
        {
            if (_pauseCoordinator != null && _pauseCoordinator.IsPaused)
            {
                return;
            }
            TickGateReactions(deltaTime);
            TickOutcomePresentation(deltaTime);
            TickColorStackAnimation(deltaTime);

            if (_experimentActive)
            {
                TickExperimentRuntime(deltaTime);
                TickCamera(deltaTime);
                return;
            }

            if (_session == null)
            {
                TickCamera(deltaTime);
                return;
            }

            if (_session.FlowState == StageFlowState.Countdown)
            {
                _countdownRemaining = Mathf.Max(
                    0f,
                    _countdownRemaining - deltaTime);
                UpdateCountdownText();
                shieldVisual.SetActive(
                    _shieldSelected && !_session.ContinueUsed ||
                    _session.ShieldActive);
                if (_countdownRemaining <= 0f)
                {
                    bool continued = _session.ContinueUsed;
                    _session.CompleteCountdown();
                    ApplyUiFlow(MobileUiFlow.Gameplay);
                    if (continued)
                    {
                        ApplySafeOverridesToActiveGates();
                    }
                    else
                    {
                        ApplyItemPresentation();
                    }
                    SynchronizeViews();
                }
                TickCamera(deltaTime);
                return;
            }

            bool boosterBefore = _session.BoosterActive;
            TickMovement(deltaTime);
            if (boosterBefore && !_session.BoosterActive)
            {
                _boosterExitOverridesApplied = true;
                ApplySafeOverridesToActiveGates();
            }
            SynchronizeViews();
            TickCamera(deltaTime);
        }

        internal void TickMovement(float deltaTime)
        {
            if ((_pauseCoordinator != null && _pauseCoordinator.IsPaused) ||
                _session == null ||
                (_session.FlowState != StageFlowState.Playing &&
                _session.FlowState != StageFlowState.ShieldRecovery &&
                _session.FlowState != StageFlowState.StageFinishing))
            {
                return;
            }

            StageGateView upcoming =
                FindActiveGate(_session.GatesPassed);
            float effectiveSpeed = upcoming == null
                ? _session.CurrentSpeed
                : _session.GetSpeedForPlan(upcoming.ActivePlan);
            float distance = effectiveSpeed * deltaTime;
            if (_session.FlowState != StageFlowState.StageFinishing)
            {
                _session.Advance(deltaTime, distance);
            }
            if (_session.FlowState == StageFlowState.Failed)
            {
                return;
            }

            player.position += Vector3.forward * distance;
            ResolveCrossedGatePlanes();
            if (_session.FlowState == StageFlowState.Failed)
            {
                return;
            }
            trackPool.Tick(player.position.z);
            trackPool.SetSurfaceMaterial(
                upcoming != null && upcoming.ActivePlan.Modifier.IsIce
                    ? cyanMaterial
                    : _normalTrackMaterial);
            RecycleResolvedGatesBehindPlayer();
            UpdateCampaignGateVisibility(deltaTime);
            if (_session.FlowState == StageFlowState.StageFinishing &&
                goal.activeSelf &&
                player.position.z >= goal.transform.position.z)
            {
                CompleteStageAtGoal();
            }
        }

        internal void HandleGameplayTap()
        {
            if (_pauseCoordinator != null && _pauseCoordinator.IsPaused)
            {
                return;
            }
            if (_experimentActive)
            {
                if (_experimentSession.TryCycleColor())
                {
                    SynchronizeExperimentViews();
                }
                return;
            }
            if (_session == null)
            {
                return;
            }
            if (_session.TryToggleColor())
            {
                _telemetry.Record(
                    "tap",
                    _session,
                    _session.GatesPassed,
                    default,
                    "color-change",
                    0f);
            }
            SynchronizeViews();
        }

        internal void RequestPause()
        {
            if (_pauseCoordinator == null ||
                !_pauseCoordinator.TryPause(GetCurrentFlowState()))
            {
                return;
            }

            PauseAttemptEffects();
            ApplyPausePresentation();
        }

        internal void RequestResume()
        {
            if (_pauseCoordinator == null || !_pauseCoordinator.TryResume())
            {
                return;
            }

            ResumeAttemptEffects();
            ApplyPausePresentation();
        }

        internal void HandleBack()
        {
            if (_pauseCoordinator == null)
            {
                return;
            }

            bool wasPaused = _pauseCoordinator.IsPaused;
            GameplayPauseBackResult result =
                _pauseCoordinator.HandleBack(GetCurrentFlowState());
            if (result == GameplayPauseBackResult.Paused && !wasPaused)
            {
                PauseAttemptEffects();
            }
            else if (result == GameplayPauseBackResult.Resumed)
            {
                ResumeAttemptEffects();
            }
            ApplyPausePresentation();
        }

        internal void RequestPauseRestart()
        {
            if (_pauseCoordinator != null &&
                _pauseCoordinator.TryShowModal(
                    GameplayPauseModal.RestartConfirmation))
            {
                ApplyPausePresentation();
            }
        }

        internal void RequestPauseSettings()
        {
            if (_pauseCoordinator == null ||
                !_pauseCoordinator.TryShowModal(GameplayPauseModal.Settings))
            {
                return;
            }

            pauseSettingsPanel.Open(
                AppRoot.TryGetActive(out AppRoot root) ? root : null);
            ApplyPausePresentation();
        }

        internal void RequestPauseLobby()
        {
            if (_pauseCoordinator != null &&
                _pauseCoordinator.TryShowModal(
                    GameplayPauseModal.LeaveConfirmation))
            {
                ApplyPausePresentation();
            }
        }

        internal void ConfirmPauseModal()
        {
            if (_pauseCoordinator == null)
            {
                return;
            }

            if (_pauseCoordinator.CurrentModal ==
                GameplayPauseModal.RestartConfirmation)
            {
                _pauseCoordinator.Clear();
                RetryToItemSelection();
                ApplyPausePresentation();
                return;
            }
            if (_pauseCoordinator.CurrentModal ==
                GameplayPauseModal.LeaveConfirmation &&
                _pauseCoordinator.TryBeginTransition())
            {
                _pauseSceneLoadError = string.Empty;
                ApplyPausePresentation();
                _sceneTransitionLoader.LoadScene(
                    frontendScenePath,
                    OnFrontendSceneLoadCompleted);
            }
        }

        internal void CancelPauseModal()
        {
            if (_pauseCoordinator != null &&
                _pauseCoordinator.TryCloseModal())
            {
                ApplyPausePresentation();
            }
        }

        private StageFlowState GetCurrentFlowState()
        {
            if (_experimentActive && _experimentSession != null)
            {
                return _experimentSession.FlowState;
            }
            return _session?.FlowState ?? StageFlowState.Lobby;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                RequestPause();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                RequestPause();
            }
        }

        internal void PlayFromLobby()
        {
            SelectStageById(_selectedStageId);
        }

        internal void SelectStage(int displayNumber)
        {
            if (displayNumber < 1)
            {
                return;
            }
            StageDefinition definition;
            try
            {
                definition =
                    StageCatalog.GetByDisplayNumber(displayNumber);
            }
            catch (ArgumentOutOfRangeException)
            {
                return;
            }
            SelectStageById(definition.StageId);
        }

        internal void SelectStageById(string stageId)
        {
            TrySelectStageById(stageId, false);
        }

        private bool TrySelectStageById(
            string stageId,
            bool enteredFromFrontendLaunch)
        {
            StageDefinition definition;
            try
            {
                definition = StageCatalog.GetById(stageId);
            }
            catch (ArgumentException)
            {
                return false;
            }
            if (definition.DisplayNumber > _highestUnlocked)
            {
                return false;
            }
            int displayNumber = definition.DisplayNumber;
            _selectedStageNumber = displayNumber;
            _selectedStageId = definition.StageId;
            _enteredFromFrontendLaunch = enteredFromFrontendLaunch;
            _preRunReturnTransitioning = false;
            _preRunReturnError = string.Empty;
            _session = new StageSession(definition);
            _shieldSelected = false;
            _boosterSelected = false;
            _startItemStartError = string.Empty;
            RefreshStartItemInventory();
            ApplyUiFlow(MobileUiFlow.PreRun);
            selectedStageText.text =
                $"STAGE {displayNumber}\n{_session.Stage.Title}";
            SynchronizeItemSelection();
            return true;
        }

        private bool TryEnterExternalCampaignLaunch()
        {
            if (!AppRoot.TryGetActive(out AppRoot root) ||
                !root.TryConsumeCampaignLaunch(
                    out CampaignLaunchRequest request))
            {
                return false;
            }

            return TrySelectStageById(request.StageId, true);
        }

        internal void HandlePreRunBack()
        {
            if (_uiFlow != MobileUiFlow.PreRun ||
                _preRunReturnTransitioning)
            {
                return;
            }

            if (!_enteredFromFrontendLaunch)
            {
                ShowLobby();
                return;
            }

            _preRunReturnTransitioning = true;
            _preRunReturnError = string.Empty;
            backButton.interactable = false;
            _sceneTransitionLoader.LoadScene(
                frontendScenePath,
                OnPreRunFrontendLoadCompleted);
        }

        private void OnPreRunFrontendLoadCompleted(
            SceneTransitionResult result)
        {
            if (this == null || result.Succeeded)
            {
                return;
            }

            _preRunReturnTransitioning = false;
            _preRunReturnError = string.IsNullOrWhiteSpace(result.Error)
                ? "FRONTEND LOAD FAILED"
                : result.Error;
            backButton.interactable = true;
            selectedStageText.text =
                $"STAGE {_selectedStageNumber}\n{_session.Stage.Title}\n" +
                _preRunReturnError;
        }

        internal void ToggleShieldSelection()
        {
            if (_session == null ||
                _session.FlowState != StageFlowState.PreRunSelection ||
                _session.StageProvidesShield ||
                !_session.Stage.ShieldAllowed ||
                _startItemInventorySnapshot.ShieldCount <= 0)
            {
                return;
            }
            _shieldSelected = !_shieldSelected;
            SynchronizeItemSelection();
        }

        internal void ToggleBoosterSelection()
        {
            if (_session == null ||
                _session.FlowState != StageFlowState.PreRunSelection ||
                _session.StageProvidesBooster ||
                !_session.Stage.BoosterAllowed ||
                _startItemInventorySnapshot.BoosterCount <= 0)
            {
                return;
            }
            _boosterSelected = !_boosterSelected;
            SynchronizeItemSelection();
        }

        internal void StartSelectedStage()
        {
            if (_session == null ||
                _session.FlowState != StageFlowState.PreRunSelection)
            {
                return;
            }
            ProductMutationResult consumption = _startItemInventory.Consume(
                _shieldSelected,
                _boosterSelected);
            if (!consumption.Succeeded)
            {
                _startItemStartError =
                    consumption.Error.Code ==
                        ProductErrorCode.InsufficientInventory
                        ? "NOT ENOUGH START ITEMS"
                        : "ITEM SAVE FAILED";
                RefreshStartItemInventory();
                NormalizeSelectedItemsToInventory();
                SynchronizeItemSelection();
                return;
            }
            _startItemStartError = string.Empty;
            RefreshStartItemInventory();
            _session.SelectItems(
                new StartItemSelection(_shieldSelected, _boosterSelected));
            if (!_session.BeginCountdown())
            {
                return;
            }
            ResetRunPresentation();
            _attemptContinuePolicy = new AttemptContinuePolicy();
            _continueAttemptId = Guid.NewGuid().ToString("N");
            BuildInitialGatePool();
            PlacePlannedGoal();
            ApplyUiFlow(MobileUiFlow.Countdown);
            shieldVisual.SetActive(_shieldSelected);
            _countdownRemaining = CountdownDuration;
            UpdateCountdownText();
            SynchronizeViews();
            _telemetry.Record(
                "run-start",
                _session,
                0,
                default,
                "countdown",
                0f);
        }

        internal GateOutcome HandleGateCrossed(StageGateView gate)
        {
            if (!CanResolveGate())
            {
                return GateOutcome.Invulnerable;
            }
            if (_experimentActive && gate.HasExperimentPlan)
            {
                return HandleExperimentGateCrossed(gate);
            }
            GatePlan plan = gate.ActivePlan;
            int gateIndex = gate.PlanIndex;
            GateOutcome outcome = _session.ResolveGate(
                plan,
                _session.ElapsedPlayingSeconds);
            if (outcome == GateOutcome.Matched ||
                outcome == GateOutcome.Invulnerable ||
                outcome == GateOutcome.Echoed)
            {
                gate.ShowSuccess();
                successParticles.transform.position = gate.transform.position;
                successParticles.Play();
                RecycleOrDeactivate(gate);
            }
            else if (outcome == GateOutcome.Boosted)
            {
                gate.ShowBoosterImpact(failureMaterial);
            }
            else if (outcome == GateOutcome.Shielded)
            {
                gate.ShowFailure(failureMaterial);
                RecycleOrDeactivate(gate);
            }
            else if (outcome == GateOutcome.Mismatched)
            {
                gate.ShowFailure(failureMaterial);
                TriggerFailure(gate);
            }
            _telemetry.Record(
                "gate",
                _session,
                gateIndex,
                plan,
                outcome.ToString(),
                0f);
            UpdateCampaignGateVisibility();
            SynchronizeViews();
            return outcome;
        }

        internal bool CanResolveExperimentGate()
        {
            return (_pauseCoordinator == null || !_pauseCoordinator.IsPaused) &&
                _experimentActive &&
                _experimentSession != null &&
                _experimentSession.FlowState == StageFlowState.Playing;
        }

        internal bool CanResolveGate()
        {
            if (_pauseCoordinator != null && _pauseCoordinator.IsPaused)
            {
                return false;
            }
            if (_experimentActive)
            {
                return CanResolveExperimentGate();
            }
            return _session != null &&
                (_session.FlowState == StageFlowState.Playing ||
                 _session.FlowState == StageFlowState.ShieldRecovery);
        }

        internal void RequestCoinContinue()
        {
            if (!CanRequestContinue() || !_continueEconomy.IsAvailable)
            {
                return;
            }
            int count = _session.ContinueUseCount;
            AttemptContinuePolicyResult offer =
                _attemptContinuePolicy.GetCoinOffer(
                    count,
                    _continueEconomy.CoinBalance);
            if (!offer.Authorized)
            {
                _continueStatus = "NOT ENOUGH COINS";
                SynchronizeContinueOffers();
                return;
            }

            _continueRequestPending = true;
            SynchronizeContinueOffers();
            string transactionId =
                $"continue:{_continueAttemptId}:coin:" +
                _attemptContinuePolicy.CoinContinueCount;
            ProductMutationResult spend =
                _continueEconomy.Spend(transactionId, offer.CoinCost);
            _continueRequestPending = false;
            if (!spend.Succeeded)
            {
                _continueStatus = spend.Error.Code ==
                    ProductErrorCode.InsufficientFunds
                        ? "NOT ENOUGH COINS"
                        : "CONTINUE SAVE FAILED";
                SynchronizeContinueOffers();
                return;
            }
            AttemptContinuePolicyResult authorization =
                _attemptContinuePolicy.ConfirmCoinContinue(
                    count,
                    offer.CoinCost);
            if (!authorization.Authorized || !ResumeAuthorizedContinue())
            {
                _continueStatus = "CONTINUE UNAVAILABLE";
                SynchronizeContinueOffers();
            }
        }

        internal void RequestRewardedContinue()
        {
            if (!CanRequestContinue() ||
                !_attemptContinuePolicy.CanRequestRewardedAd(
                    _session.ContinueUseCount,
                    _rewardedAdService))
            {
                return;
            }
            _continueRequestPending = true;
            _continueStatus = "WAITING FOR AD";
            int generation = ++_continueRequestGeneration;
            StageSession session = _session;
            int continueCount = session.ContinueUseCount;
            SynchronizeContinueOffers();
            _rewardedAdService.Show(result => OnRewardedContinueCompleted(
                generation,
                session,
                continueCount,
                result));
        }

        private void OnRewardedContinueCompleted(
            int generation,
            StageSession session,
            int continueCount,
            RewardedAdResult result)
        {
            if (this == null || generation != _continueRequestGeneration ||
                !_continueRequestPending || _session != session ||
                _session.FlowState != StageFlowState.Failed ||
                _session.ContinueUseCount != continueCount)
            {
                return;
            }
            _continueRequestPending = false;
            AttemptContinuePolicyResult authorization =
                _attemptContinuePolicy.ApplyRewardedAdResult(
                    continueCount,
                    result);
            if (authorization.Authorized && ResumeAuthorizedContinue())
            {
                return;
            }
            _continueStatus = result switch
            {
                RewardedAdResult.Cancelled => "AD CANCELLED",
                RewardedAdResult.Unavailable => "AD UNAVAILABLE",
                RewardedAdResult.Failed => "AD FAILED",
                _ => "CONTINUE UNAVAILABLE"
            };
            SynchronizeContinueOffers();
        }

        private bool CanRequestContinue()
        {
            return !_experimentActive && !_continueRequestPending &&
                _session != null &&
                _session.FlowState == StageFlowState.Failed &&
                _session.ContinueAvailable &&
                _attemptContinuePolicy != null;
        }

        private bool ResumeAuthorizedContinue()
        {
            if (_session == null || !_session.ContinueAfterFailure())
            {
                return false;
            }
            failPanel.SetActive(false);
            _failureDelayRemaining = 0f;
            _continueStatus = string.Empty;
            PrepareCleanContinueRespawn();
            ApplySafeOverridesToActiveGates();
            ApplyUiFlow(MobileUiFlow.Countdown);
            _countdownRemaining = CountdownDuration;
            UpdateCountdownText();
            return true;
        }

        internal void RetryToItemSelection()
        {
            if (_experimentActive)
            {
                RestartDevelopmentExperiment();
                return;
            }
            if (_session == null)
            {
                return;
            }
            _session.RetryToSelection();
            ResetRunPresentation();
            _attemptContinuePolicy = new AttemptContinuePolicy();
            failPanel.SetActive(false);
            clearPanel.SetActive(false);
            _startItemStartError = string.Empty;
            RefreshStartItemInventory();
            NormalizeSelectedItemsToInventory();
            ApplyUiFlow(MobileUiFlow.PreRun);
            selectedStageText.text =
                $"STAGE {_selectedStageNumber}\n{_session.Stage.Title}";
            SynchronizeItemSelection();
        }

        internal void ShowLobby()
        {
            _enteredFromFrontendLaunch = false;
            _preRunReturnTransitioning = false;
            _preRunReturnError = string.Empty;
            backButton.interactable = true;
            _experimentActive = false;
            _experimentSession = null;
            if (_normalTrackMaterial != null)
            {
                trackPool.SetSurfaceMaterial(_normalTrackMaterial);
            }
            _session = null;
            ResetRunPresentation();
            bool[] cleared = LoadClearedStages();
            _highestUnlocked = _progressStore.LoadHighestUnlocked();
            _selectedStageNumber = LobbyProgression.SelectCurrentStage(
                _highestUnlocked,
                cleared);
            _lobbyTier = LobbyProgression.GetVisualTier(cleared);
            ApplyLobbyTier();
            StageDefinition stage =
                StageCatalog.GetByDisplayNumber(_selectedStageNumber);
            _selectedStageId = stage.StageId;
            lobbyStageText.text = $"STAGE {_selectedStageNumber}";
            lobbyStageTitleText.text = stage.Title;
            lobbyStageDescriptionText.text = stage.Description;
            int clearedCount = 0;
            for (int index = 0; index < cleared.Length; index++)
            {
                if (cleared[index])
                {
                    clearedCount++;
                }
            }
            lobbyProgressText.text = $"{clearedCount} / {StageCatalog.Count} CLEARED";
            lobbyTierText.text = $"LOBBY LEVEL {_lobbyTier}";
            resetProgressConfirmation.SetActive(false);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            resetProgressButton.gameObject.SetActive(true);
            experimentLabButton.gameObject.SetActive(true);
#else
            resetProgressButton.gameObject.SetActive(false);
            experimentLabButton.gameObject.SetActive(false);
#endif
            ApplyUiFlow(MobileUiFlow.Lobby);
            RefreshStageButtons();
        }

        internal void RequestProgressReset()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            resetProgressConfirmation.SetActive(true);
#endif
        }

        internal void CancelProgressReset()
        {
            resetProgressConfirmation.SetActive(false);
        }

        internal void ConfirmProgressReset()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _progressStore.ClearGameplayProgress();
            _shieldSelected = false;
            _boosterSelected = false;
            ShowLobby();
#endif
        }

        internal void ShowStageSelect()
        {
            ShowLobby();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            ApplyUiFlow(MobileUiFlow.Development);
            stageSelectPanel.SetActive(true);
#endif
        }

        internal void OpenExperimentLab()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _experimentActive = false;
            _experimentSession = null;
            _session = null;
            if (_normalTrackMaterial != null)
            {
                trackPool.SetSurfaceMaterial(_normalTrackMaterial);
            }
            ResetRunPresentation();
            ApplyUiFlow(MobileUiFlow.Development);
            stageSelectPanel.SetActive(false);
#endif
        }

        internal static bool ShouldShowExperimentLab(
            bool isEditor,
            bool isDevelopmentBuild)
        {
            return isEditor || isDevelopmentBuild;
        }

        internal void SetProgressStoreForTests(IStageProgressStore store)
        {
            _progressStore = store ?? throw new ArgumentNullException(nameof(store));
            _highestUnlocked = Mathf.Clamp(
                _progressStore.LoadHighestUnlocked(),
                1,
                StageCatalog.Count);
            ShowLobby();
        }

        internal void SetStartItemInventoryForTests(
            IStartItemInventoryGateway gateway)
        {
            _startItemInventory = gateway ??
                throw new ArgumentNullException(nameof(gateway));
            RefreshStartItemInventory();
            NormalizeSelectedItemsToInventory();
            if (_session != null &&
                _session.FlowState == StageFlowState.PreRunSelection)
            {
                SynchronizeItemSelection();
            }
        }

        internal void SetContinueServicesForTests(
            IContinueEconomyGateway economy,
            IRewardedAdService rewardedAds)
        {
            _continueEconomy = economy ??
                throw new ArgumentNullException(nameof(economy));
            _rewardedAdService = rewardedAds ??
                throw new ArgumentNullException(nameof(rewardedAds));
            SynchronizeContinueOffers();
        }

        internal void SetHapticsForTests(IHapticFeedback haptics)
        {
            _haptics = haptics;
        }

        internal void SetSceneTransitionLoaderForTests(
            ISceneTransitionLoader loader)
        {
            _sceneTransitionLoader = loader ??
                throw new ArgumentNullException(nameof(loader));
        }

        internal void ConfigurePause(
            Button hudPauseButton,
            GameObject overlayRoot,
            Image fullScreenDim,
            GameObject panel,
            Button resume,
            Button restart,
            Button settings,
            Button lobby,
            GameObject modalRoot,
            Text modalTitle,
            Text modalMessage,
            Button modalConfirm,
            Text modalConfirmLabel,
            Button modalCancel,
            SettingsPanelController sharedSettingsPanel,
            GameObject transitionBlocker,
            ParticleSystem[] registeredAttemptEffects,
            string frontendPath)
        {
            pauseButton = hudPauseButton;
            pauseOverlayRoot = overlayRoot;
            pauseDim = fullScreenDim;
            pausePanel = panel;
            pauseResumeButton = resume;
            pauseRestartButton = restart;
            pauseSettingsButton = settings;
            pauseLobbyButton = lobby;
            pauseModalRoot = modalRoot;
            pauseModalTitleText = modalTitle;
            pauseModalMessageText = modalMessage;
            pauseModalConfirmButton = modalConfirm;
            pauseModalConfirmText = modalConfirmLabel;
            pauseModalCancelButton = modalCancel;
            pauseSettingsPanel = sharedSettingsPanel;
            pauseTransitionBlocker = transitionBlocker;
            attemptEffects = registeredAttemptEffects;
            frontendScenePath = frontendPath;
        }

        internal void StartDevelopmentExperiment(
            ExperimentDefinition definition,
            StartItemSelection items)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _session = null;
            _experimentSession = new ExperimentSession(definition, items);
            _experimentActive = true;
            ResetRunPresentation();
            ApplyUiFlow(MobileUiFlow.Countdown);
            BuildInitialExperimentGatePool();
            _colorStackInitialized = false;
            _countdownRemaining = CountdownDuration;
            UpdateCountdownText();
            SynchronizeExperimentViews();
#endif
        }

        internal void RestartDevelopmentExperiment()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!_experimentActive || _experimentSession == null)
            {
                return;
            }

            _experimentSession.Restart();
            ResetRunPresentation();
            ApplyUiFlow(MobileUiFlow.Countdown);
            BuildInitialExperimentGatePool();
            _colorStackInitialized = false;
            _countdownRemaining = CountdownDuration;
            UpdateCountdownText();
            SynchronizeExperimentViews();
#endif
        }

        internal void BackToExperimentLab()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            OpenExperimentLab();
#endif
        }

        internal void UnlockAllForDevelopment()
        {
            _highestUnlocked = StageCatalog.Count;
            _progressStore.SaveHighestUnlocked(_highestUnlocked);
            RefreshStageButtons();
        }

        internal bool HasRequiredReferences()
        {
            if (stageCatalogAsset == null ||
                player == null || playerRenderer == null ||
                gameplayCamera == null || playerBody == null ||
                redMaterial == null ||
                blueMaterial == null || greenMaterial == null ||
                yellowMaterial == null || purpleMaterial == null ||
                cyanMaterial == null ||
                failureMaterial == null || tapSurface == null ||
                !tapSurface.HasRequiredReference() || trackPool == null ||
                !trackPool.HasRequiredReferences() || gates == null ||
                gates.Length < 5 || goal == null || shieldVisual == null ||
                echoShellVisual == null || echoShellRenderers == null ||
                echoShellRenderers.Length == 0 ||
                successParticles == null || speedLines == null ||
                playerTrail == null || uiFlowRoots == null ||
                uiFlowRoots.Length != 7 || lobbyPanel == null ||
                lobbyStageText == null || lobbyStageTitleText == null ||
                lobbyStageDescriptionText == null || lobbyProgressText == null ||
                lobbyTierText == null || lobbyPlayButton == null ||
                experimentLabButton == null ||
                lobbyTierRoots == null || lobbyTierRoots.Length != 4 ||
                resetProgressButton == null ||
                resetProgressConfirmation == null ||
                confirmResetProgressButton == null ||
                cancelResetProgressButton == null ||
                stageSelectPanel == null || stageButtons == null ||
                stageSummaryTexts == null ||
                stageButtons.Length != StageCatalog.Count ||
                stageSummaryTexts.Length != StageCatalog.Count ||
                developerUnlockAllButton == null || itemPanel == null ||
                selectedStageText == null || shieldToggleButton == null ||
                shieldToggleText == null || boosterToggleButton == null ||
                boosterToggleText == null || preRunStatusText == null ||
                startButton == null ||
                backButton == null || countdownPanel == null ||
                countdownText == null || stageHud == null ||
                stageHudText == null || progressText == null ||
                progressFill == null || shieldIcon == null ||
                boosterMeterRoot == null || boosterMeterFill == null ||
                boosterWarning == null || colorHudPanel == null ||
                colorTiles == null || colorTiles.Length != 6 ||
                colorTileImages == null || colorTileImages.Length != 6 ||
                colorTileSymbols == null || colorTileSymbols.Length != 6 ||
                nextColorMarkers == null || nextColorMarkers.Length != 6 ||
                clearPanel == null || clearTitleText == null ||
                clearDetailsText == null || clearContinueButton == null ||
                replayButton == null || clearLobbyButton == null ||
                failPanel == null || failTitleText == null ||
                failDetailsText == null || failContinueStatusText == null ||
                coinContinueButton == null || rewardedContinueButton == null ||
                retryButton == null || failLobbyButton == null ||
                pauseButton == null || pauseOverlayRoot == null ||
                pauseDim == null || pausePanel == null ||
                pauseResumeButton == null || pauseRestartButton == null ||
                pauseSettingsButton == null || pauseLobbyButton == null ||
                pauseModalRoot == null || pauseModalTitleText == null ||
                pauseModalMessageText == null ||
                pauseModalConfirmButton == null ||
                pauseModalConfirmText == null ||
                pauseModalCancelButton == null ||
                pauseSettingsPanel == null ||
                !pauseSettingsPanel.HasRequiredReferences() ||
                pauseTransitionBlocker == null || attemptEffects == null ||
                attemptEffects.Length != 2 || attemptEffects[0] == null ||
                attemptEffects[1] == null ||
                string.IsNullOrWhiteSpace(frontendScenePath) ||
                frontendScenePath == gameObject.scene.path)
            {
                return false;
            }
            for (int index = 0; index < gates.Length; index++)
            {
                if (gates[index] == null || !gates[index].HasRequiredReferences())
                {
                    return false;
                }
            }
            return true;
        }

        internal void Configure(
            StageCatalogAsset catalogAsset,
            Transform playerTransform,
            Renderer runnerRenderer,
            Rigidbody runnerBody,
            Camera camera,
            Material red,
            Material blue,
            Material green,
            Material yellow,
            Material purple,
            Material cyan,
            Material failure,
            GameplayTapSurface gameplayTapSurface,
            TrackPoolController pool,
            StageGateView[] gatePool,
            GameObject goalObject,
            GameObject shieldObject,
            GameObject echoShellObject,
            ParticleSystem success,
            ParticleSystem boosterLines,
            TrailRenderer trail,
            GameObject[] flowRoots,
            GameObject lobby,
            Text lobbyStage,
            Text lobbyStageTitle,
            Text lobbyStageDescription,
            Text lobbyProgress,
            Text lobbyTierLabel,
            Button lobbyPlay,
            Button labButton,
            GameObject[] tierRoots,
            Button resetButton,
            GameObject resetConfirmation,
            Button confirmResetButton,
            Button cancelResetButton,
            GameObject stageSelection,
            Button[] selectionButtons,
            Text[] selectionSummaries,
            Button unlockAll,
            GameObject itemSelection,
            Text selectedStage,
            Button shieldButton,
            Text shieldText,
            Button boosterButton,
            Text boosterText,
            Text itemStatus,
            Button start,
            Button back,
            GameObject countdown,
            Text countdownValue,
            GameObject hud,
            Text hudStage,
            Text hudProgress,
            Image hudProgressFill,
            GameObject hudShieldIcon,
            GameObject hudBoosterMeter,
            Image boosterFill,
            GameObject warning,
            GameObject colorHud,
            GameObject[] hudColorTiles,
            Image[] hudColorTileImages,
            Text[] hudColorTileSymbols,
            GameObject[] hudNextMarkers,
            GameObject clear,
            Text clearTitle,
            Text clearDetails,
            Button clearContinue,
            Button replay,
            Button clearLobby,
            GameObject fail,
            Text failTitle,
            Text failDetails,
            Text failContinueStatus,
            Button failCoinContinue,
            Button failRewardedContinue,
            Button retry,
            Button failLobby)
        {
            stageCatalogAsset = catalogAsset;
            player = playerTransform;
            playerRenderer = runnerRenderer;
            playerBody = runnerBody;
            gameplayCamera = camera;
            redMaterial = red;
            blueMaterial = blue;
            greenMaterial = green;
            yellowMaterial = yellow;
            purpleMaterial = purple;
            cyanMaterial = cyan;
            failureMaterial = failure;
            tapSurface = gameplayTapSurface;
            trackPool = pool;
            gates = gatePool;
            goal = goalObject;
            shieldVisual = shieldObject;
            echoShellVisual = echoShellObject;
            echoShellRenderers =
                echoShellObject.GetComponentsInChildren<Renderer>(true);
            successParticles = success;
            speedLines = boosterLines;
            playerTrail = trail;
            uiFlowRoots = flowRoots;
            lobbyPanel = lobby;
            lobbyStageText = lobbyStage;
            lobbyStageTitleText = lobbyStageTitle;
            lobbyStageDescriptionText = lobbyStageDescription;
            lobbyProgressText = lobbyProgress;
            lobbyTierText = lobbyTierLabel;
            lobbyPlayButton = lobbyPlay;
            experimentLabButton = labButton;
            lobbyTierRoots = tierRoots;
            resetProgressButton = resetButton;
            resetProgressConfirmation = resetConfirmation;
            confirmResetProgressButton = confirmResetButton;
            cancelResetProgressButton = cancelResetButton;
            stageSelectPanel = stageSelection;
            stageButtons = selectionButtons;
            stageSummaryTexts = selectionSummaries;
            developerUnlockAllButton = unlockAll;
            itemPanel = itemSelection;
            selectedStageText = selectedStage;
            shieldToggleButton = shieldButton;
            shieldToggleText = shieldText;
            boosterToggleButton = boosterButton;
            boosterToggleText = boosterText;
            preRunStatusText = itemStatus;
            startButton = start;
            backButton = back;
            countdownPanel = countdown;
            countdownText = countdownValue;
            stageHud = hud;
            stageHudText = hudStage;
            progressText = hudProgress;
            progressFill = hudProgressFill;
            shieldIcon = hudShieldIcon;
            boosterMeterRoot = hudBoosterMeter;
            boosterMeterFill = boosterFill;
            boosterWarning = warning;
            colorHudPanel = colorHud;
            colorTiles = hudColorTiles;
            colorTileImages = hudColorTileImages;
            colorTileSymbols = hudColorTileSymbols;
            nextColorMarkers = hudNextMarkers;
            clearPanel = clear;
            clearTitleText = clearTitle;
            clearDetailsText = clearDetails;
            clearContinueButton = clearContinue;
            replayButton = replay;
            clearLobbyButton = clearLobby;
            failPanel = fail;
            failTitleText = failTitle;
            failDetailsText = failDetails;
            failContinueStatusText = failContinueStatus;
            coinContinueButton = failCoinContinue;
            rewardedContinueButton = failRewardedContinue;
            retryButton = retry;
            failLobbyButton = failLobby;
        }

        private void AddListeners()
        {
            lobbyPlayButton.onClick.AddListener(PlayFromLobby);
            experimentLabButton.onClick.AddListener(OpenExperimentLab);
            resetProgressButton.onClick.AddListener(RequestProgressReset);
            confirmResetProgressButton.onClick.AddListener(ConfirmProgressReset);
            cancelResetProgressButton.onClick.AddListener(CancelProgressReset);
            _stageButtonListeners = new UnityAction[stageButtons.Length];
            for (int index = 0; index < stageButtons.Length; index++)
            {
                string stageId = StageCatalog.GetByIndex(index).StageId;
                UnityAction listener = () => SelectStageById(stageId);
                _stageButtonListeners[index] = listener;
                stageButtons[index].onClick.AddListener(listener);
            }
            developerUnlockAllButton.onClick.AddListener(UnlockAllForDevelopment);
            shieldToggleButton.onClick.AddListener(ToggleShieldSelection);
            boosterToggleButton.onClick.AddListener(ToggleBoosterSelection);
            startButton.onClick.AddListener(StartSelectedStage);
            backButton.onClick.AddListener(HandlePreRunBack);
            coinContinueButton.onClick.AddListener(RequestCoinContinue);
            rewardedContinueButton.onClick.AddListener(RequestRewardedContinue);
            retryButton.onClick.AddListener(RetryToItemSelection);
            replayButton.onClick.AddListener(RetryToItemSelection);
            clearContinueButton.onClick.AddListener(LeaveResultFlow);
            clearLobbyButton.onClick.AddListener(LeaveResultFlow);
            failLobbyButton.onClick.AddListener(LeaveResultFlow);
            pauseButton.onClick.AddListener(RequestPause);
            pauseResumeButton.onClick.AddListener(RequestResume);
            pauseRestartButton.onClick.AddListener(RequestPauseRestart);
            pauseSettingsButton.onClick.AddListener(RequestPauseSettings);
            pauseLobbyButton.onClick.AddListener(RequestPauseLobby);
            pauseModalConfirmButton.onClick.AddListener(ConfirmPauseModal);
            pauseModalCancelButton.onClick.AddListener(CancelPauseModal);
            pauseSettingsPanel.ApplySucceeded += ClosePauseSettings;
            pauseSettingsPanel.CancelRequested += ClosePauseSettings;
        }

        private void RemoveListeners()
        {
            if (stageButtons == null)
            {
                return;
            }
            lobbyPlayButton.onClick.RemoveListener(PlayFromLobby);
            experimentLabButton.onClick.RemoveListener(OpenExperimentLab);
            resetProgressButton.onClick.RemoveListener(RequestProgressReset);
            confirmResetProgressButton.onClick.RemoveListener(ConfirmProgressReset);
            cancelResetProgressButton.onClick.RemoveListener(CancelProgressReset);
            if (_stageButtonListeners != null)
            {
                int listenerCount = Mathf.Min(
                    stageButtons.Length,
                    _stageButtonListeners.Length);
                for (int index = 0; index < listenerCount; index++)
                {
                    stageButtons[index].onClick.RemoveListener(
                        _stageButtonListeners[index]);
                }
            }
            developerUnlockAllButton.onClick.RemoveListener(UnlockAllForDevelopment);
            shieldToggleButton.onClick.RemoveListener(ToggleShieldSelection);
            boosterToggleButton.onClick.RemoveListener(ToggleBoosterSelection);
            startButton.onClick.RemoveListener(StartSelectedStage);
            backButton.onClick.RemoveListener(HandlePreRunBack);
            coinContinueButton.onClick.RemoveListener(RequestCoinContinue);
            rewardedContinueButton.onClick.RemoveListener(RequestRewardedContinue);
            retryButton.onClick.RemoveListener(RetryToItemSelection);
            replayButton.onClick.RemoveListener(RetryToItemSelection);
            clearContinueButton.onClick.RemoveListener(LeaveResultFlow);
            clearLobbyButton.onClick.RemoveListener(LeaveResultFlow);
            failLobbyButton.onClick.RemoveListener(LeaveResultFlow);
            pauseButton.onClick.RemoveListener(RequestPause);
            pauseResumeButton.onClick.RemoveListener(RequestResume);
            pauseRestartButton.onClick.RemoveListener(RequestPauseRestart);
            pauseSettingsButton.onClick.RemoveListener(RequestPauseSettings);
            pauseLobbyButton.onClick.RemoveListener(RequestPauseLobby);
            pauseModalConfirmButton.onClick.RemoveListener(ConfirmPauseModal);
            pauseModalCancelButton.onClick.RemoveListener(CancelPauseModal);
            pauseSettingsPanel.ApplySucceeded -= ClosePauseSettings;
            pauseSettingsPanel.CancelRequested -= ClosePauseSettings;
        }

        private void LeaveResultFlow()
        {
            _continueRequestGeneration++;
            _continueRequestPending = false;
            if (_experimentActive)
            {
                BackToExperimentLab();
                return;
            }

            if (_enteredFromFrontendLaunch)
            {
                if (_preRunReturnTransitioning)
                {
                    return;
                }
                _preRunReturnTransitioning = true;
                _preRunReturnError = string.Empty;
                _sceneTransitionLoader.LoadScene(
                    frontendScenePath,
                    OnResultFrontendLoadCompleted);
                return;
            }

            ShowLobby();
        }

        private void OnResultFrontendLoadCompleted(
            SceneTransitionResult result)
        {
            if (this == null || result.Succeeded)
            {
                return;
            }

            _preRunReturnTransitioning = false;
            _preRunReturnError = string.IsNullOrWhiteSpace(result.Error)
                ? "FRONTEND LOAD FAILED"
                : result.Error;
            if (GetCurrentFlowState() == StageFlowState.StageCleared)
            {
                clearDetailsText.text += "\n" + _preRunReturnError;
            }
            else
            {
                failDetailsText.text += "\n" + _preRunReturnError;
            }
        }

        private void BuildInitialGatePool()
        {
            _nextPlanIndex = 0;
            _nextGateZ =
                player.position.z + CampaignInitialGateLeadDistance;
            for (int index = 0; index < gates.Length; index++)
            {
                ActivateNextGate(gates[index]);
            }
            UpdateCampaignGateVisibility();
        }

        private void BuildInitialExperimentGatePool()
        {
            _nextPlanIndex = 0;
            _nextGateZ =
                player.position.z + ExperimentInitialGateLeadDistance;
            for (int index = 0; index < gates.Length; index++)
            {
                ActivateNextExperimentGate(gates[index]);
            }
            UpdateExperimentGateVisibility();
        }

        private void ActivateNextGate(StageGateView gate)
        {
            if (_nextPlanIndex >= _session.Stage.TargetGateCount)
            {
                gate.Deactivate();
                return;
            }
            GatePlan plan = _session.GetGatePlan(_nextPlanIndex);
            _nextGateZ += plan.Spacing;
            gate.Activate(
                plan,
                _nextPlanIndex,
                GetMaterial(plan.Color),
                _nextGateZ);
            gate.UpdateCampaignVisibility(
                _session.GatesPassed,
                EstimateCampaignGateEta(plan, _nextGateZ),
                _normalTrackMaterial,
                _session.Stage.CamouflageSettings,
                _session.Stage.HiddenSettings,
                _session.Stage.FlickerSettings,
                _session.ElapsedPlayingSeconds,
                0f);
            _nextPlanIndex++;
        }

        private void ActivateNextExperimentGate(StageGateView gate)
        {
            if (_nextPlanIndex >= _experimentSession.Definition.GateCount)
            {
                gate.Deactivate();
                return;
            }
            ExperimentGatePlan plan =
                _experimentSession.GetPlan(_nextPlanIndex);
            _nextGateZ += plan.Spacing;
            gate.ActivateExperiment(
                plan,
                GetMaterial(plan.Color),
                _normalTrackMaterial,
                _nextGateZ,
                _experimentSession.GatesPassed,
                EstimateExperimentGateEta(plan, _nextGateZ),
                _experimentSession.Definition.Camouflage,
                _experimentSession.Definition.Hidden,
                _experimentSession.Definition.Flicker,
                _experimentSession.ElapsedPlayingSeconds);
            _nextPlanIndex++;
        }

        private void RecycleOrDeactivate(StageGateView gate)
        {
            if (_experimentActive)
            {
                if (_nextPlanIndex <
                    _experimentSession.Definition.GateCount)
                {
                    ActivateNextExperimentGate(gate);
                }
                else
                {
                    gate.Deactivate();
                }
                return;
            }
            if (_nextPlanIndex < _session.Stage.TargetGateCount)
            {
                ActivateNextGate(gate);
            }
            else
            {
                gate.Deactivate();
            }
        }

        private GateOutcome HandleExperimentGateCrossed(StageGateView gate)
        {
            if (!CanResolveExperimentGate())
            {
                return GateOutcome.Invulnerable;
            }

            ExperimentGatePlan plan = gate.ActiveExperimentPlan;
            bool resolved = _experimentSession.Resolve(
                plan,
                _experimentSession.ElapsedPlayingSeconds);
            GateOutcome outcome;
            if (resolved)
            {
                switch (_experimentSession.LastResolution)
                {
                    case ExperimentGateResolution.EchoColorMatch:
                        outcome = GateOutcome.Echoed;
                        break;
                    case ExperimentGateResolution.BoosterDefense:
                        outcome = GateOutcome.Boosted;
                        break;
                    case ExperimentGateResolution.ShieldDefense:
                        outcome = GateOutcome.Shielded;
                        break;
                    default:
                        outcome = GateOutcome.Matched;
                        break;
                }
                gate.ShowSuccess();
                successParticles.transform.position = gate.transform.position;
                successParticles.Play();
                RecycleOrDeactivate(gate);
                if (_experimentSession.FlowState ==
                    StageFlowState.StageCleared)
                {
                    TriggerExperimentCompleted();
                }
            }
            else
            {
                outcome = GateOutcome.Mismatched;
                gate.ShowFailure(failureMaterial);
                TriggerExperimentFailure();
            }
            UpdateExperimentGateVisibility();
            SynchronizeExperimentViews();
            return outcome;
        }

        private void TickExperimentRuntime(float deltaTime)
        {
            if ((_pauseCoordinator != null && _pauseCoordinator.IsPaused) ||
                _experimentSession == null)
            {
                return;
            }

            if (_experimentSession.FlowState == StageFlowState.Countdown)
            {
                _countdownRemaining = Mathf.Max(
                    0f,
                    _countdownRemaining - deltaTime);
                UpdateCountdownText();
                if (_countdownRemaining <= 0f &&
                    _experimentSession.CompleteCountdown())
                {
                    ApplyUiFlow(MobileUiFlow.Gameplay);
                    SynchronizeExperimentViews();
                }
                return;
            }

            if (_experimentSession.FlowState == StageFlowState.Playing)
            {
                TickExperiment(deltaTime);
            }
        }

        private void TickExperiment(float deltaTime)
        {
            if (_experimentSession == null ||
                _experimentSession.FlowState != StageFlowState.Playing)
            {
                return;
            }
            StageGateView upcoming =
                FindExperimentGate(_experimentSession.GatesPassed);
            if (upcoming == null)
            {
                return;
            }
            float speed = _experimentSession.GetSpeedForPlan(
                upcoming.ActiveExperimentPlan);
            float distance = speed * deltaTime;
            _experimentSession.Advance(deltaTime);
            player.position += Vector3.forward * distance;
            trackPool.Tick(player.position.z);
            bool ice = upcoming.ActiveExperimentPlan.IsIce;
            trackPool.SetSurfaceMaterial(
                ice ? cyanMaterial : _normalTrackMaterial);
            UpdateExperimentGateVisibility(deltaTime);
        }

        private void TriggerExperimentFailure()
        {
            if (_experimentSession == null ||
                _experimentSession.FlowState != StageFlowState.Failed)
            {
                return;
            }

            _failureDelayRemaining = FailurePanelDelay;
            playerRenderer.sharedMaterial = failureMaterial;
            player.localRotation = Quaternion.Euler(65f, 0f, 18f);
            player.localScale = Vector3.one * 0.8f;
            shieldVisual.SetActive(false);
            ResetBoosterPresentation();
            ApplyUiFlow(MobileUiFlow.FailedResult);
            failPanel.SetActive(false);
        }

        private void TriggerExperimentCompleted()
        {
            if (_experimentSession == null ||
                _experimentSession.FlowState != StageFlowState.StageCleared)
            {
                return;
            }

            _clearDelayRemaining = ClearPanelDelay;
            player.localScale = Vector3.one * 1.18f;
            successParticles.transform.position = player.position;
            successParticles.Play();
            shieldVisual.SetActive(false);
            ResetBoosterPresentation();
            ApplyUiFlow(MobileUiFlow.ClearResult);
            clearPanel.SetActive(false);
        }

        private StageGateView FindExperimentGate(int gateIndex)
        {
            for (int index = 0; index < gates.Length; index++)
            {
                StageGateView gate = gates[index];
                if (gate.gameObject.activeSelf &&
                    gate.HasExperimentPlan &&
                    !gate.HasResolved &&
                    gate.ActiveExperimentPlan.GateIndex == gateIndex)
                {
                    return gate;
                }
            }
            return null;
        }

        private void UpdateExperimentGateVisibility(
            float deltaSeconds = 0f)
        {
            for (int index = 0; index < gates.Length; index++)
            {
                StageGateView gate = gates[index];
                if (gate.gameObject.activeSelf && gate.HasExperimentPlan)
                {
                    gate.UpdateExperimentVisibility(
                        _experimentSession.GatesPassed,
                        EstimateExperimentGateEta(
                            gate.ActiveExperimentPlan,
                            gate.transform.position.z),
                        _normalTrackMaterial,
                        _experimentSession.Definition.Camouflage,
                        _experimentSession.Definition.Hidden,
                        _experimentSession.Definition.Flicker,
                        _experimentSession.ElapsedPlayingSeconds,
                        deltaSeconds);
                }
            }
        }

        private float EstimateExperimentGateEta(
            ExperimentGatePlan plan,
            float gateWorldZ)
        {
            float remainingDistance = Mathf.Max(
                0f,
                gateWorldZ - player.position.z);
            float speed = _experimentSession.GetSpeedForPlan(plan);
            return GateEtaEstimator.EstimateSeconds(
                remainingDistance,
                speed);
        }

        private void UpdateCampaignGateVisibility(
            float deltaSeconds = 0f)
        {
            if (_session == null)
            {
                return;
            }

            for (int index = 0; index < gates.Length; index++)
            {
                StageGateView gate = gates[index];
                if (gate.gameObject.activeSelf &&
                    !gate.HasExperimentPlan)
                {
                    gate.UpdateCampaignVisibility(
                        _session.GatesPassed,
                        EstimateCampaignGateEta(
                            gate.ActivePlan,
                            gate.transform.position.z),
                        _normalTrackMaterial,
                        _session.Stage.CamouflageSettings,
                        _session.Stage.HiddenSettings,
                        _session.Stage.FlickerSettings,
                        _session.ElapsedPlayingSeconds,
                        deltaSeconds);
                }
            }
        }

        private float EstimateCampaignGateEta(
            GatePlan plan,
            float gateWorldZ)
        {
            float remainingDistance = Mathf.Max(
                0f,
                gateWorldZ - player.position.z);
            return GateEtaEstimator.EstimateSeconds(
                remainingDistance,
                _session.GetSpeedForPlan(plan));
        }

        private void PlacePlannedGoal()
        {
            StageGoalPlan plan = StageGoalPlanner.Create(
                _session.Stage,
                CampaignInitialGateLeadDistance,
                CampaignGoalDistance);
            goal.transform.position = new Vector3(
                0f,
                0f,
                player.position.z + plan.DistanceFromPlayer);
            goal.SetActive(true);
        }

        private void CompleteStageAtGoal()
        {
            if (!_session.ReachGoal())
            {
                return;
            }
            RecordClear();
            ApplyUiFlow(MobileUiFlow.ClearResult);
            clearPanel.SetActive(false);
            _clearDelayRemaining = ClearPanelDelay;
            player.localScale = Vector3.one * 1.18f;
            successParticles.transform.position = player.position;
            successParticles.Play();
            ResetBoosterPresentation();
            _telemetry.Record(
                "run-end",
                _session,
                _session.GatesPassed,
                default,
                "cleared",
                0f);
        }

        private void RecordClear()
        {
            if (_clearRecorded)
            {
                return;
            }
            _clearRecorded = true;
            _clearSaveError = string.Empty;
            StageRecord record =
                _progressStore.LoadRecord(_selectedStageNumber);
            record = StageProgress.RecordClear(
                record,
                _session.ElapsedPlayingSeconds,
                _session.Items,
                _session.ContinueUsed);
            int highestUnlocked = StageProgress.HighestUnlockedAfterClear(
                _highestUnlocked,
                _selectedStageNumber);
            ProductMutationResult result;
            if (_progressStore is IAtomicStageProgressStore atomicStore)
            {
                result = atomicStore.SaveClearResult(
                    _selectedStageNumber,
                    record,
                    highestUnlocked);
            }
            else
            {
                _progressStore.SaveRecord(_selectedStageNumber, record);
                _progressStore.SaveHighestUnlocked(highestUnlocked);
                result = ProductMutationResult.Success(true);
            }
            if (result.Succeeded)
            {
                _highestUnlocked = highestUnlocked;
            }
            else
            {
                _clearSaveError = string.IsNullOrWhiteSpace(
                    result.Error.Diagnostic)
                        ? "PROGRESS SAVE FAILED"
                        : result.Error.Diagnostic;
                Debug.LogError(_clearSaveError);
            }
        }

        private void TriggerFailure(StageGateView failedGate)
        {
            _failedGate = failedGate;
            _continueSnapshot = CaptureContinueSnapshot(failedGate);
            _cameraShakeRemaining = 0.3f;
            _failureDelayRemaining = FailurePanelDelay;
            playerRenderer.sharedMaterial = failureMaterial;
            player.localRotation = Quaternion.Euler(65f, 0f, 18f);
            player.localScale = Vector3.one * 0.8f;
            shieldVisual.SetActive(false);
            ResetBoosterPresentation();
            ApplyUiFlow(MobileUiFlow.FailedResult);
            failPanel.SetActive(false);
            _telemetry.Record(
                "run-end",
                _session,
                _session.GatesPassed,
                default,
                "failed",
                0f);
        }

        private void TickOutcomePresentation(float deltaTime)
        {
            if (_failureDelayRemaining > 0f)
            {
                _failureDelayRemaining = Mathf.Max(
                    0f,
                    _failureDelayRemaining - deltaTime);
                if (_failureDelayRemaining <= 0f &&
                    _experimentActive &&
                    _experimentSession != null &&
                    _experimentSession.FlowState == StageFlowState.Failed)
                {
                    failPanel.SetActive(true);
                    failTitleText.text = "EXPERIMENT FAILED";
                    failDetailsText.text =
                        FormatExperimentResultDetails();
                    coinContinueButton.gameObject.SetActive(false);
                    rewardedContinueButton.gameObject.SetActive(false);
                    failContinueStatusText.gameObject.SetActive(false);
                    retryButton.gameObject.SetActive(true);
                    failLobbyButton.gameObject.SetActive(true);
                    SetButtonLabel(retryButton, "RETRY SAME TEST");
                    SetButtonLabel(failLobbyButton, "BACK TO LAB");
                }
                else if (_failureDelayRemaining <= 0f && _session != null)
                {
                    failPanel.SetActive(true);
                    failTitleText.text = "STAGE FAILED";
                    failDetailsText.text =
                        $"PROGRESS {_session.GatesPassed}/{_session.Stage.TargetGateCount}\n" +
                        $"ITEMS {FormatItems(_session.Items)}";
                    retryButton.gameObject.SetActive(true);
                    failLobbyButton.gameObject.SetActive(true);
                    SynchronizeContinueOffers();
                    SetButtonLabel(retryButton, "RETRY");
                    SetButtonLabel(failLobbyButton, "LOBBY");
                }
            }
            if (_clearDelayRemaining > 0f)
            {
                _clearDelayRemaining = Mathf.Max(
                    0f,
                    _clearDelayRemaining - deltaTime);
                if (_clearDelayRemaining <= 0f &&
                    _experimentActive &&
                    _experimentSession != null &&
                    _experimentSession.FlowState ==
                    StageFlowState.StageCleared)
                {
                    clearPanel.SetActive(true);
                    clearTitleText.text = "EXPERIMENT COMPLETE";
                    clearDetailsText.text =
                        FormatExperimentResultDetails();
                    clearContinueButton.gameObject.SetActive(false);
                    replayButton.gameObject.SetActive(true);
                    clearLobbyButton.gameObject.SetActive(true);
                    SetButtonLabel(replayButton, "REPLAY");
                    SetButtonLabel(clearLobbyButton, "BACK TO LAB");
                }
                else if (_clearDelayRemaining <= 0f && _session != null)
                {
                    clearPanel.SetActive(true);
                    clearTitleText.text = "STAGE CLEAR";
                    StageRecord record =
                        _progressStore.LoadRecord(_selectedStageNumber);
                    clearDetailsText.text =
                        $"STAGE {_selectedStageNumber} COMPLETE\n" +
                        $"ITEMS {FormatItems(_session.Items)}\n" +
                        $"TIME {_session.ElapsedPlayingSeconds:0.00}s\n" +
                        $"BEST {record.BestTime:0.00}s" +
                        (string.IsNullOrWhiteSpace(_clearSaveError)
                            ? string.Empty
                            : $"\n{_clearSaveError}");
                    clearContinueButton.gameObject.SetActive(true);
                    replayButton.gameObject.SetActive(true);
                    clearLobbyButton.gameObject.SetActive(true);
                    SetButtonLabel(clearContinueButton, "CONTINUE");
                    SetButtonLabel(replayButton, "REPLAY");
                    SetButtonLabel(clearLobbyButton, "LOBBY");
                }
            }
        }

        private string FormatExperimentResultDetails()
        {
            string failureCause =
                _experimentSession.FlowState == StageFlowState.Failed
                    ? "\nCAUSE GATE MISS"
                    : string.Empty;
            return
                $"{_experimentSession.Definition.ColorCount} COLORS · " +
                $"{_experimentSession.Definition.Mechanic.ToString().ToUpperInvariant()}\n" +
                $"SEED {_experimentSession.Definition.Seed}\n" +
                $"PROGRESS {_experimentSession.GatesPassed}/" +
                $"{_experimentSession.Definition.GateCount}\n" +
                $"TIME {_experimentSession.ElapsedPlayingSeconds:0.00}s" +
                failureCause;
        }

        private static void SetButtonLabel(Button button, string value)
        {
            Text label = button.GetComponentInChildren<Text>(true);
            if (label != null)
            {
                label.text = value;
            }
        }

        private void ResetRunPresentation()
        {
            _pauseCoordinator?.Clear();
            _pauseSceneLoadError = string.Empty;
            _clearRecorded = false;
            _cameraShakeRemaining = 0f;
            _failureDelayRemaining = 0f;
            _clearDelayRemaining = 0f;
            _boosterLaunchPulse = 0f;
            _wasBoosterActive = false;
            _boosterCameraBlend = 0f;
            _boosterExitOverridesApplied = false;
            _failedGate = null;
            _continueSnapshot = null;
            _continueRequestGeneration++;
            _continueRequestPending = false;
            _continueStatus = string.Empty;
            player.position = _playerStartPosition;
            player.localRotation = Quaternion.identity;
            player.localScale = Vector3.one;
            gameplayCamera.transform.SetPositionAndRotation(
                _cameraStartPosition,
                _cameraStartRotation);
            gameplayCamera.fieldOfView = NormalFov;
            _colorStackInitialized = false;
            _colorStackTransitionRemaining = 0f;
            playerRenderer.sharedMaterial = redMaterial;
            trackPool.ResetPool();
            goal.SetActive(false);
            shieldVisual.SetActive(false);
            echoShellVisual.SetActive(false);
            successParticles.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear);
            ResetBoosterPresentation();
            boosterMeterFill.fillAmount = 0f;
            SetHorizontalFill(boosterMeterFill, 0f);
            boosterMeterRoot.SetActive(false);
            boosterWarning.SetActive(false);
            for (int index = 0; index < gates.Length; index++)
            {
                gates[index].Deactivate();
            }
            ApplyPausePresentation();
        }

        private void PauseAttemptEffects()
        {
            for (int index = 0; index < attemptEffects.Length; index++)
            {
                ParticleSystem effect = attemptEffects[index];
                _pausedEffectWasPlaying[index] = effect.isPlaying;
                if (_pausedEffectWasPlaying[index])
                {
                    effect.Pause(true);
                }
            }
        }

        private void ResumeAttemptEffects()
        {
            for (int index = 0; index < attemptEffects.Length; index++)
            {
                ParticleSystem effect = attemptEffects[index];
                if (_pausedEffectWasPlaying[index] && effect.isPaused)
                {
                    effect.Play(true);
                }
                _pausedEffectWasPlaying[index] = false;
            }
        }

        private void ApplyPausePresentation()
        {
            if (_pauseCoordinator == null || pauseOverlayRoot == null)
            {
                return;
            }

            bool paused = _pauseCoordinator.IsPaused;
            GameplayPauseModal modal = _pauseCoordinator.CurrentModal;
            pauseOverlayRoot.SetActive(paused);
            pausePanel.SetActive(paused &&
                modal == GameplayPauseModal.None &&
                !_pauseCoordinator.IsTransitioning);
            pauseSettingsPanel.gameObject.SetActive(
                paused && modal == GameplayPauseModal.Settings);
            bool confirmation = paused &&
                (modal == GameplayPauseModal.RestartConfirmation ||
                 modal == GameplayPauseModal.LeaveConfirmation ||
                 modal == GameplayPauseModal.SceneLoadError);
            pauseModalRoot.SetActive(confirmation);
            pauseTransitionBlocker.SetActive(
                paused && _pauseCoordinator.IsTransitioning);

            if (confirmation)
            {
                bool restart =
                    modal == GameplayPauseModal.RestartConfirmation;
                bool leave = modal == GameplayPauseModal.LeaveConfirmation;
                pauseModalTitleText.text = restart
                    ? "RESTART STAGE?"
                    : leave
                        ? "RETURN TO LOBBY?"
                        : "LOBBY LOAD FAILED";
                pauseModalMessageText.text = restart
                    ? "RESTART THIS ATTEMPT USING THE EXISTING RETRY RULES?"
                    : leave
                        ? "CURRENT PLAY WILL NOT BE SAVED. RETURN TO LOBBY?"
                        : string.IsNullOrWhiteSpace(_pauseSceneLoadError)
                            ? "THE ATTEMPT REMAINS PAUSED. RESUME OR TRY AGAIN."
                            : _pauseSceneLoadError;
                pauseModalConfirmButton.gameObject.SetActive(restart || leave);
                pauseModalConfirmText.text = restart ? "RESTART" : "LEAVE";
                SetButtonLabel(pauseModalCancelButton,
                    modal == GameplayPauseModal.SceneLoadError
                        ? "CLOSE"
                        : "CANCEL");
            }

            RefreshPauseButtonVisibility();
        }

        private void RefreshPauseButtonVisibility()
        {
            if (pauseButton == null)
            {
                return;
            }

            pauseButton.gameObject.SetActive(
                _pauseCoordinator != null &&
                !_pauseCoordinator.IsPaused &&
                GameplayPauseCoordinator.IsPauseAllowed(
                    GetCurrentFlowState()) &&
                (_uiFlow == MobileUiFlow.Countdown ||
                 _uiFlow == MobileUiFlow.Gameplay));
        }

        private void ClosePauseSettings()
        {
            CancelPauseModal();
        }

        private void OnFrontendSceneLoadCompleted(SceneTransitionResult result)
        {
            if (this == null)
            {
                return;
            }

            _pauseCoordinator.CompleteTransition(result.Succeeded);
            if (!result.Succeeded &&
                !string.IsNullOrWhiteSpace(result.Error))
            {
                _pauseSceneLoadError = result.Error;
            }
            ApplyPausePresentation();
        }

        private void ApplyItemPresentation()
        {
            shieldVisual.SetActive(_session.ShieldActive);
            if (_session.BoosterActive)
            {
                _haptics.RequestBoosterLaunch();
                _boosterLaunchPulse = 0.35f;
                _cameraShakeRemaining = 0.25f;
                speedLines.Play();
                playerTrail.emitting = false;
                player.localScale = Vector3.one * 1.18f;
            }
            _wasBoosterActive = _session.BoosterActive;
        }

        private void ResetBoosterPresentation()
        {
            speedLines.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear);
            playerTrail.emitting = false;
            boosterWarning.SetActive(false);
            boosterMeterFill.fillAmount = 0f;
            SetHorizontalFill(boosterMeterFill, 0f);
        }

        private void SynchronizeViews()
        {
            if (_session == null)
            {
                return;
            }
            playerRenderer.sharedMaterial =
                _session.FlowState == StageFlowState.Failed
                ? failureMaterial
                : GetMaterial(_session.CurrentColor);
            echoShellVisual.SetActive(_session.EchoActive);
            if (_session.EchoActive)
            {
                Material echoMaterial = GetMaterial(_session.EchoColor);
                for (int index = 0;
                    index < echoShellRenderers.Length;
                    index++)
                {
                    echoShellRenderers[index].sharedMaterial = echoMaterial;
                }
            }
            shieldVisual.SetActive(
                _session.ShieldActive ||
                (_session.FlowState == StageFlowState.Countdown &&
                _shieldSelected && !_session.ContinueUsed));
            stageHudText.text = $"STAGE {_selectedStageNumber}";
            progressText.text =
                $"{_session.GatesPassed} / {_session.Stage.TargetGateCount}";
            progressFill.fillAmount = _session.Progress;
            SetHorizontalFill(progressFill, _session.Progress);
            shieldIcon.SetActive(
                MobileUiPolicy.IsItemIconVisible(
                    _uiFlow,
                    _session.ShieldActive ||
                    (_shieldSelected && !_session.ContinueUsed)));
            SynchronizeColorHud();

            float boosterNormalized =
                (_session.Items.Booster ||
                 _session.StageProvidesBooster)
                ? _session.BoosterDistanceRemaining /
                    _session.Stage.BoosterDistance
                : 0f;
            boosterMeterFill.fillAmount = Mathf.Clamp01(boosterNormalized);
            SetHorizontalFill(boosterMeterFill, boosterNormalized);
            boosterMeterRoot.SetActive(
                MobileUiPolicy.IsBoosterMeterVisible(
                    _uiFlow,
                    _session.BoosterActive));
            boosterWarning.SetActive(
                _session.BoosterActive &&
                boosterNormalized <= BoosterWarningThreshold);
            if (_session.BoosterActive &&
                boosterNormalized <= BoosterWarningThreshold)
            {
                _boosterExitOverridesApplied = true;
                ApplySafeOverridesToActiveGates();
            }
            if (_session.BoosterPresentationStrength > 0f)
            {
                if (!speedLines.isPlaying)
                {
                    speedLines.Play();
                }
                playerTrail.emitting = false;
                float strength = _session.BoosterPresentationStrength;
                if (_session.BoosterActive &&
                    boosterNormalized <= BoosterWarningThreshold)
                {
                    strength *= 0.75f + (boosterNormalized * 1.25f);
                }
            }
            else if (_session.FlowState != StageFlowState.Failed)
            {
                ResetBoosterPresentation();
            }
            if (_boosterLaunchPulse > 0f)
            {
                _boosterLaunchPulse = Mathf.Max(
                    0f,
                    _boosterLaunchPulse - Time.deltaTime);
                player.localScale = Vector3.one *
                    (1f + (_boosterLaunchPulse * 0.5f));
            }
            else if (_session.FlowState != StageFlowState.Failed &&
                _session.FlowState != StageFlowState.StageCleared)
            {
                player.localScale = Vector3.one;
            }
            RefreshPauseButtonVisibility();
        }

        private void SynchronizeItemSelection()
        {
            shieldToggleButton.interactable =
                _session.Stage.ShieldAllowed &&
                !_session.StageProvidesShield &&
                _startItemInventorySnapshot.ShieldCount > 0;
            boosterToggleButton.interactable =
                _session.Stage.BoosterAllowed &&
                !_session.StageProvidesBooster &&
                _startItemInventorySnapshot.BoosterCount > 0;
            shieldToggleText.text = _session.StageProvidesShield
                ? "SHIELD: PROVIDED"
                : !_session.Stage.ShieldAllowed
                    ? "SHIELD: LOCKED"
                    : _shieldSelected
                        ? $"SHIELD x{_startItemInventorySnapshot.ShieldCount}: ON"
                        : $"SHIELD x{_startItemInventorySnapshot.ShieldCount}: OFF";
            boosterToggleText.text = _session.StageProvidesBooster
                ? "BOOSTER: PROVIDED"
                : !_session.Stage.BoosterAllowed
                    ? "BOOSTER: LOCKED"
                    : _boosterSelected
                        ? $"BOOSTER x{_startItemInventorySnapshot.BoosterCount}: ON"
                        : $"BOOSTER x{_startItemInventorySnapshot.BoosterCount}: OFF";
            preRunStatusText.text = _startItemStartError;
            preRunStatusText.gameObject.SetActive(
                !string.IsNullOrWhiteSpace(_startItemStartError));
        }

        private void SynchronizeContinueOffers()
        {
            if (coinContinueButton == null || rewardedContinueButton == null ||
                failContinueStatusText == null)
            {
                return;
            }
            bool campaignFailure = !_experimentActive && _session != null &&
                _session.FlowState == StageFlowState.Failed &&
                _attemptContinuePolicy != null &&
                _attemptContinuePolicy.HasCapacity(_session.ContinueUseCount);
            int price = _attemptContinuePolicy?.CurrentCoinPrice ?? 300;
            bool coinVisible = campaignFailure &&
                _continueEconomy?.IsAvailable == true;
            coinContinueButton.gameObject.SetActive(coinVisible);
            coinContinueButton.interactable = coinVisible &&
                !_continueRequestPending &&
                _continueEconomy.CoinBalance >= price;
            SetButtonLabel(coinContinueButton, $"CONTINUE {price} COINS");

            bool rewardedVisible = campaignFailure &&
                _attemptContinuePolicy.CanRequestRewardedAd(
                    _session.ContinueUseCount,
                    _rewardedAdService);
            rewardedContinueButton.gameObject.SetActive(rewardedVisible);
            rewardedContinueButton.interactable =
                rewardedVisible && !_continueRequestPending;
            SetButtonLabel(rewardedContinueButton, "WATCH AD TO CONTINUE");

            failContinueStatusText.text = _continueStatus;
            failContinueStatusText.gameObject.SetActive(
                !string.IsNullOrWhiteSpace(_continueStatus));
        }

        private void RefreshStartItemInventory()
        {
            _startItemInventorySnapshot =
                _startItemInventory?.Read() ?? default;
        }

        private void NormalizeSelectedItemsToInventory()
        {
            if (_startItemInventorySnapshot.ShieldCount <= 0)
            {
                _shieldSelected = false;
            }
            if (_startItemInventorySnapshot.BoosterCount <= 0)
            {
                _boosterSelected = false;
            }
        }

        private void RefreshStageButtons()
        {
            for (int index = 0; index < StageCatalog.Count; index++)
            {
                StageDefinition definition = StageCatalog.GetByIndex(index);
                int number = definition.DisplayNumber;
                bool unlocked = number <= _highestUnlocked;
                stageButtons[index].interactable = unlocked;
                StageRecord record = _progressStore.LoadRecord(number);
                stageSummaryTexts[index].text =
                    $"STAGE {number}  {(record.Cleared ? "CLEARED" : unlocked ? "OPEN" : "LOCKED")}\n" +
                    definition.Title;
            }
            developerUnlockAllButton.gameObject.SetActive(false);
            stageSelectPanel.SetActive(false);
        }

        private ContinueSnapshot CaptureContinueSnapshot(
            StageGateView failedGate)
        {
            ActiveGateSnapshot[] gateSnapshots =
                new ActiveGateSnapshot[gates.Length];
            for (int index = 0; index < gates.Length; index++)
            {
                StageGateView gate = gates[index];
                Vector3[] partPositions =
                    new Vector3[gate.PartCount];
                Quaternion[] partRotations =
                    new Quaternion[gate.PartCount];
                for (int part = 0; part < gate.PartCount; part++)
                {
                    Transform partTransform = gate.GetPartTransform(part);
                    partPositions[part] = partTransform.localPosition;
                    partRotations[part] = partTransform.localRotation;
                }
                gateSnapshots[index] = new ActiveGateSnapshot(
                    index,
                    gate.gameObject.activeSelf,
                    gate.HasResolved,
                    gate.PlanIndex,
                    gate.ActivePlan,
                    gate.transform.position,
                    gate.transform.rotation,
                    partPositions,
                    partRotations);
            }

            return new ContinueSnapshot(
                _session.Stage.StageId,
                _session.ElapsedPlayingSeconds,
                _session.Progress,
                player.position.z - _playerStartPosition.z,
                _session.SpeedBeforeFailure,
                _session.CurrentColor,
                _session.SequenceCursor,
                failedGate.PlanIndex,
                _session.IsFinalSection,
                goal.activeSelf,
                _session.ContinueUseCount,
                _session.Items.Shield,
                _session.Items.Shield && !_session.ShieldActive,
                _session.Items.Booster,
                _session.Items.Booster && !_session.BoosterActive,
                player.position,
                gameplayCamera.transform.position,
                gameplayCamera.transform.rotation,
                gateSnapshots);
        }

        private void ApplySafeOverridesToActiveGates()
        {
            if (_session == null)
            {
                return;
            }

            int firstUpcomingPlanIndex = _session.GatesPassed;

            for (int offset = 0; offset < 2; offset++)
            {
                int planIndex = firstUpcomingPlanIndex + offset;
                StageGateView gate = FindActiveGate(planIndex);
                if (gate == null)
                {
                    continue;
                }

                GatePlan plan = _session.CreateSafeTransitionOverride(
                    gate.ActivePlan,
                    offset);
                gate.ApplyTemporaryPlan(
                    plan,
                    GetMaterial(plan.Color));
            }
        }

        private StageGateView FindActiveGate(int planIndex)
        {
            for (int index = 0; index < gates.Length; index++)
            {
                StageGateView gate = gates[index];
                if (gate.gameObject.activeSelf &&
                    !gate.HasResolved &&
                    gate.PlanIndex == planIndex)
                {
                    return gate;
                }
            }
            return null;
        }

        private void ResolveCrossedGatePlanes()
        {
            if ((_pauseCoordinator != null && _pauseCoordinator.IsPaused) ||
                _session == null ||
                (_session.FlowState != StageFlowState.Playing &&
                 _session.FlowState != StageFlowState.ShieldRecovery))
            {
                return;
            }

            int maximumResolutions = gates.Length;
            for (int index = 0; index < maximumResolutions; index++)
            {
                StageGateView gate = FindActiveGate(_session.GatesPassed);
                if (gate == null ||
                    gate.transform.position.z > player.position.z ||
                    !gate.TryResolveCrossing())
                {
                    return;
                }
                if (_session.FlowState == StageFlowState.Failed ||
                    _session.FlowState == StageFlowState.StageFinishing)
                {
                    return;
                }
            }
        }

        private void PrepareCleanContinueRespawn()
        {
            StageGateView consumedGate = _failedGate;
            if (consumedGate != null)
            {
                consumedGate.Deactivate();
            }
            _failedGate = null;

            StageGateView nextGate = FindActiveGate(_session.GatesPassed);
            if (nextGate != null)
            {
                Vector3 position = player.position;
                position.x = _playerStartPosition.x;
                position.y = _playerStartPosition.y;
                position.z = Mathf.Min(
                    position.z,
                    nextGate.transform.position.z -
                    CampaignInitialGateLeadDistance);
                player.position = position;
            }

            player.localRotation = Quaternion.identity;
            player.localScale = Vector3.one;
            playerRenderer.sharedMaterial = GetMaterial(_session.CurrentColor);
            playerBody.linearVelocity = Vector3.zero;
            playerBody.angularVelocity = Vector3.zero;
            _cameraShakeRemaining = 0f;
            _boosterCameraBlend = 0f;
            gameplayCamera.transform.SetPositionAndRotation(
                player.position + _cameraFollowOffset,
                _cameraStartRotation);
            gameplayCamera.fieldOfView = NormalFov;
            ResetBoosterPresentation();
        }

        private void RecycleResolvedGatesBehindPlayer()
        {
            for (int index = 0; index < gates.Length; index++)
            {
                StageGateView gate = gates[index];
                if (gate.gameObject.activeSelf &&
                    gate.HasResolved &&
                    !gate.ReactionActive &&
                    !gate.BoosterDestroyed &&
                    gate.transform.position.z < player.position.z)
                {
                    RecycleOrDeactivate(gate);
                }
            }
        }

        private void TickGateReactions(float deltaTime)
        {
            for (int index = 0; index < gates.Length; index++)
            {
                StageGateView gate = gates[index];
                bool destroyed = gate.BoosterDestroyed;
                gate.Tick(deltaTime);
                if (destroyed && !gate.ReactionActive &&
                    _session != null &&
                    _session.FlowState != StageFlowState.Failed)
                {
                    RecycleOrDeactivate(gate);
                }
            }
        }

        private void TickCamera(float deltaTime)
        {
            bool boosterTarget = _session != null &&
                _session.BoosterPresentationStrength > 0f &&
                _session.FlowState != StageFlowState.Failed;
            if (_experimentActive && _experimentSession != null)
            {
                boosterTarget = _experimentSession.BoosterActive &&
                    _experimentSession.FlowState == StageFlowState.Playing;
            }
            float blendDuration = boosterTarget
                ? BoosterCameraBlendIn
                : BoosterCameraBlendOut;
            _boosterCameraBlend = Mathf.MoveTowards(
                _boosterCameraBlend,
                boosterTarget ? 1f : 0f,
                deltaTime / blendDuration);

            Vector3 followOffset = Vector3.Lerp(
                _cameraFollowOffset,
                BoosterCameraFollowOffset,
                _boosterCameraBlend);
            Quaternion rotation = Quaternion.Slerp(
                _cameraStartRotation,
                BoosterCameraRotation,
                _boosterCameraBlend);
            Vector3 cameraPosition = player.position + followOffset;

            if (_cameraShakeRemaining > 0f)
            {
                _cameraShakeRemaining = Mathf.Max(
                    0f,
                    _cameraShakeRemaining - deltaTime);
                if (_cameraShakeRemaining > 0f)
                {
                    float phase = _cameraShakeRemaining * 100f;
                    cameraPosition += new Vector3(
                        Mathf.Sin(phase) * 0.15f,
                        Mathf.Cos(phase * 0.7f) * 0.05f,
                        Mathf.Sin(phase * 0.5f) * 0.06f *
                        _boosterCameraBlend);
                }
            }

            gameplayCamera.transform.SetPositionAndRotation(
                cameraPosition,
                rotation);
            gameplayCamera.fieldOfView = Mathf.Lerp(
                NormalFov,
                BoosterFov,
                _boosterCameraBlend);
        }

        private void UpdateCountdownText()
        {
            if (_countdownRemaining > 2.083f)
            {
                countdownText.text = "3";
            }
            else if (_countdownRemaining > 1.166f)
            {
                countdownText.text = "2";
            }
            else if (_countdownRemaining > 0.25f)
            {
                countdownText.text = "1";
            }
            else
            {
                countdownText.text = "GO!";
            }
        }

        private void ApplyLobbyTier()
        {
            for (int index = 0; index < lobbyTierRoots.Length; index++)
            {
                lobbyTierRoots[index].SetActive(index == _lobbyTier);
            }
        }

        private bool[] LoadClearedStages()
        {
            bool[] result = new bool[StageCatalog.Count];
            for (int index = 0; index < result.Length; index++)
            {
                result[index] = _progressStore.LoadRecord(index + 1).Cleared;
            }
            return result;
        }

        private Material GetMaterial(RunnerColor color)
        {
            if (color == RunnerColor.Blue)
            {
                return blueMaterial;
            }
            if (color == RunnerColor.Green)
            {
                return greenMaterial;
            }
            if (color == RunnerColor.Yellow)
            {
                return yellowMaterial;
            }
            if (color == RunnerColor.Purple)
            {
                return purpleMaterial;
            }
            if (color == RunnerColor.Cyan)
            {
                return cyanMaterial;
            }
            return redMaterial;
        }

        private void ApplyUiFlow(MobileUiFlow flow)
        {
            _uiFlow = flow;
            MobileUiVisibility visibility = MobileUiPolicy.GetVisibility(flow);
            uiFlowRoots[0].SetActive(visibility.Lobby);
            uiFlowRoots[1].SetActive(visibility.PreRun);
            uiFlowRoots[2].SetActive(visibility.GameplayHud);
            uiFlowRoots[3].SetActive(visibility.Countdown);
            uiFlowRoots[4].SetActive(visibility.ClearResult);
            uiFlowRoots[5].SetActive(visibility.FailedResult);
            uiFlowRoots[6].SetActive(visibility.Development);
            lobbyPanel.SetActive(visibility.Lobby);
            itemPanel.SetActive(visibility.PreRun);
            stageHud.SetActive(visibility.GameplayHud);
            countdownPanel.SetActive(visibility.Countdown);
            stageSelectPanel.SetActive(visibility.Development);
            if (!visibility.ClearResult)
            {
                clearPanel.SetActive(false);
            }
            if (!visibility.FailedResult)
            {
                failPanel.SetActive(false);
            }
            RefreshPauseButtonVisibility();
        }

        private void SynchronizeColorHud()
        {
            int activeCount = GetHudActiveColorCount();
            for (int index = 0; index < colorTiles.Length; index++)
            {
                bool active = index < activeCount;
                colorTiles[index].SetActive(active);
                if (!active)
                {
                    nextColorMarkers[index].SetActive(false);
                    continue;
                }

                RunnerColor color = GetHudColorAt(index);
                int slot = GetHudStackSlot(color);
                bool current = slot == 0;
                bool isNext = slot == 1;
                colorTileImages[index].color =
                    GetMaterial(color).color;
                colorTileImages[index].canvasRenderer.SetAlpha(
                    current ? 1f : 0.72f);
                colorTileSymbols[index].text = GetColorSymbol(color);
                nextColorMarkers[index].SetActive(isNext);
            }

            if (!_colorStackInitialized)
            {
                SnapColorStack(activeCount);
                _colorStackInitialized = true;
            }
            else if (_lastStackColor != GetHudCurrentColor() ||
                _lastStackActiveCount != activeCount)
            {
                RetargetColorStack(activeCount);
            }
            _lastStackColor = GetHudCurrentColor();
            _lastStackActiveCount = activeCount;
        }

        private void SynchronizeExperimentViews()
        {
            bool countdown =
                _experimentSession.FlowState == StageFlowState.Countdown;
            playerRenderer.sharedMaterial =
                _experimentSession.Failed
                    ? failureMaterial
                    : GetMaterial(_experimentSession.CurrentColor);
            stageHudText.text = _experimentSession.Failed
                ? "EXPERIMENT FAILED"
                : _experimentSession.Completed
                    ? "EXPERIMENT COMPLETE"
                    : $"{_experimentSession.Definition.ColorCount} COLORS · " +
                    _experimentSession.Definition.Mechanic.ToString().ToUpperInvariant();
            progressText.text =
                $"{_experimentSession.GatesPassed} / " +
                _experimentSession.Definition.GateCount;
            float progress = (float)_experimentSession.GatesPassed /
                _experimentSession.Definition.GateCount;
            SetHorizontalFill(progressFill, progress);
            progressFill.fillAmount = progress;
            bool shieldRelevant =
                (_experimentSession.FlowState == StageFlowState.Playing &&
                _experimentSession.ShieldActive) ||
                (countdown && _experimentSession.Items.Shield);
            shieldVisual.SetActive(shieldRelevant);
            shieldIcon.SetActive(shieldRelevant);
            bool echoRelevant =
                _experimentSession.FlowState == StageFlowState.Playing &&
                _experimentSession.EchoActive;
            echoShellVisual.SetActive(echoRelevant);
            if (echoRelevant)
            {
                Material echoMaterial =
                    GetMaterial(_experimentSession.EchoColor);
                for (int index = 0;
                    index < echoShellRenderers.Length;
                    index++)
                {
                    echoShellRenderers[index].sharedMaterial = echoMaterial;
                }
            }
            boosterMeterRoot.SetActive(_experimentSession.BoosterActive);
            boosterMeterFill.fillAmount = Mathf.Clamp01(
                _experimentSession.BoosterDistanceRemaining / 160f);
            SetHorizontalFill(
                boosterMeterFill,
                boosterMeterFill.fillAmount);
            boosterWarning.SetActive(false);
            SynchronizeColorHud();
            RefreshPauseButtonVisibility();
        }

        private void SnapColorStack(int activeCount)
        {
            for (int index = 0; index < activeCount; index++)
            {
                RunnerColor color = GetHudColorAt(index);
                int slot = GetHudStackSlot(color);
                RectTransform tile = (RectTransform)colorTiles[index].transform;
                tile.anchoredPosition = GetColorStackPosition(slot);
                tile.localScale = Vector3.one * GetColorStackScale(slot);
            }
            _colorStackTransitionRemaining = 0f;
        }

        private void RetargetColorStack(int activeCount)
        {
            for (int index = 0; index < activeCount; index++)
            {
                RectTransform tile = (RectTransform)colorTiles[index].transform;
                _colorStackStartPositions[index] = tile.anchoredPosition;
                _colorStackStartScales[index] = tile.localScale;
            }
            _colorStackTransitionRemaining = ColorStackTransitionDuration;
        }

        private void TickColorStackAnimation(float deltaTime)
        {
            if (_colorStackTransitionRemaining <= 0f ||
                (_session == null && _experimentSession == null))
            {
                return;
            }

            _colorStackTransitionRemaining = Mathf.Max(
                0f,
                _colorStackTransitionRemaining - deltaTime);
            float progress = 1f -
                (_colorStackTransitionRemaining / ColorStackTransitionDuration);
            int activeCount = GetHudActiveColorCount();
            for (int index = 0; index < activeCount; index++)
            {
                RunnerColor color = GetHudColorAt(index);
                int slot = GetHudStackSlot(color);
                RectTransform tile = (RectTransform)colorTiles[index].transform;
                tile.anchoredPosition = Vector2.Lerp(
                    _colorStackStartPositions[index],
                    GetColorStackPosition(slot),
                    progress);
                tile.localScale = Vector3.Lerp(
                    _colorStackStartScales[index],
                    Vector3.one * GetColorStackScale(slot),
                    progress);
            }
        }

        private static Vector2 GetColorStackPosition(int slot)
        {
            return new Vector2(0f, ColorStackPositions[slot]);
        }

        private static float GetColorStackScale(int slot)
        {
            if (slot == 0)
            {
                return 1f;
            }
            return slot == 1 ? 0.82f : Mathf.Max(0.56f, 0.72f - slot * 0.035f);
        }

        private int GetHudActiveColorCount()
        {
            return _experimentActive
                ? _experimentSession.Definition.ColorCount
                : MobileUiPolicy.GetActiveColorCount(
                    _session.Stage,
                    _session.GatesPassed);
        }

        private RunnerColor GetHudColorAt(int index)
        {
            return _experimentActive
                ? _experimentSession.Definition.GetColor(index)
                : MobileUiPolicy.GetColorAt(
                    _session.Stage,
                    _session.GatesPassed,
                    index);
        }

        private RunnerColor GetHudCurrentColor()
        {
            return _experimentActive
                ? _experimentSession.CurrentColor
                : _session.CurrentColor;
        }

        private int GetHudStackSlot(RunnerColor color)
        {
            if (!_experimentActive)
            {
                return MobileUiPolicy.GetStackSlot(
                    _session.Stage,
                    _session.GatesPassed,
                    _session.CurrentColor,
                    color);
            }
            int currentIndex = 0;
            int colorIndex = 0;
            for (int index = 0;
                index < _experimentSession.Definition.ColorCount;
                index++)
            {
                RunnerColor candidate =
                    _experimentSession.Definition.GetColor(index);
                if (candidate == _experimentSession.CurrentColor)
                {
                    currentIndex = index;
                }
                if (candidate == color)
                {
                    colorIndex = index;
                }
            }
            return (colorIndex - currentIndex +
                _experimentSession.Definition.ColorCount) %
                _experimentSession.Definition.ColorCount;
        }

        private static string GetColorSymbol(RunnerColor color)
        {
            RunnerColorSymbol symbol = MobileUiPolicy.GetSymbol(color);
            if (symbol == RunnerColorSymbol.Circle)
            {
                return "●";
            }
            if (symbol == RunnerColorSymbol.Square)
            {
                return "■";
            }
            if (symbol == RunnerColorSymbol.Triangle)
            {
                return "▲";
            }
            if (symbol == RunnerColorSymbol.Star)
            {
                return "★";
            }
            if (symbol == RunnerColorSymbol.Diamond)
            {
                return "◆";
            }
            return "HEX";
        }

        private static void SetHorizontalFill(Image image, float amount)
        {
            RectTransform rect = image.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(Mathf.Clamp01(amount), 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static string FormatItems(StartItemSelection items)
        {
            if (items.Shield && items.Booster)
            {
                return "SHIELD + BOOSTER";
            }
            if (items.Shield)
            {
                return "SHIELD";
            }
            if (items.Booster)
            {
                return "BOOSTER";
            }
            return "NONE";
        }

        private void ValidateRequiredReferences()
        {
            if (!HasRequiredReferences())
            {
                throw new InvalidOperationException(
                    "StageSceneController has missing required references.");
            }
        }
    }
}
