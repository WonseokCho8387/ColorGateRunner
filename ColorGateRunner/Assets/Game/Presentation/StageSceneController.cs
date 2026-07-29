using System;
using ColorGateRunner.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    public sealed class StageSceneController : MonoBehaviour
    {
        private const float CountdownDuration = 3f;
        private const float InitialGateLeadDistance = 24f;
        private const float GoalDistance = 20f;
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

        [SerializeField] private Transform player;
        [SerializeField] private Renderer playerRenderer;
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
        [SerializeField] private Button continueButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button failLobbyButton;

        private StageSession _session;
        private IStageProgressStore _progressStore;
        private IHapticFeedback _haptics;
        private DevelopmentTelemetry _telemetry;
        private int _highestUnlocked;
        private int _selectedStageNumber = 1;
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
        internal GameObject ShieldVisual => shieldVisual;
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
        internal Button ContinueButton => continueButton;
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

        internal bool IsPlayerCollider(Collider other)
        {
            return other.transform == player ||
                other.transform.IsChildOf(player);
        }

        private void Awake()
        {
            ValidateRequiredReferences();
            _progressStore = new PlayerPrefsStageProgressStore();
            _haptics = new UnityHapticFeedback();
            _telemetry = new DevelopmentTelemetry(developmentTelemetryEnabled);
            _highestUnlocked = _progressStore.LoadHighestUnlocked();
            _playerStartPosition = player.position;
            _cameraStartPosition = gameplayCamera.transform.position;
            _cameraStartRotation = gameplayCamera.transform.rotation;
            _cameraFollowOffset = _cameraStartPosition - _playerStartPosition;
            _normalTrackMaterial = trackPool.GetSegment(0).SurfaceMaterial;
            _colorStackStartPositions = new Vector2[colorTiles.Length];
            _colorStackStartScales = new Vector3[colorTiles.Length];
            AddListeners();
            ShowLobby();
        }

        private void OnDestroy()
        {
            RemoveListeners();
            _telemetry?.Flush();
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        internal void Tick(float deltaTime)
        {
            TickGateReactions(deltaTime);
            TickOutcomePresentation(deltaTime);
            TickColorStackAnimation(deltaTime);

            if (_experimentActive)
            {
                TickExperiment(deltaTime);
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
                        player.localRotation = Quaternion.identity;
                        player.localScale = Vector3.one;
                        playerRenderer.sharedMaterial =
                            GetMaterial(_session.CurrentColor);
                        ApplySafeOverridesToActiveGates();
                        if (_session.FlowState == StageFlowState.StageFinishing &&
                            _failedGate != null)
                        {
                            ShowGoalAfter(_failedGate.transform.position.z);
                        }
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
            if (_session == null ||
                (_session.FlowState != StageFlowState.Playing &&
                _session.FlowState != StageFlowState.ShieldRecovery &&
                _session.FlowState != StageFlowState.StageFinishing))
            {
                return;
            }

            float distance = _session.CurrentSpeed * deltaTime;
            if (_session.FlowState != StageFlowState.StageFinishing)
            {
                _session.Advance(deltaTime, distance);
            }
            if (_session.FlowState == StageFlowState.Failed)
            {
                return;
            }

            player.position += Vector3.forward * distance;
            trackPool.Tick(player.position.z);
            RecycleResolvedGatesBehindPlayer();
            if (_session.FlowState == StageFlowState.StageFinishing &&
                goal.activeSelf &&
                player.position.z >= goal.transform.position.z)
            {
                CompleteStageAtGoal();
            }
        }

        internal void HandleGameplayTap()
        {
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

        internal void PlayFromLobby()
        {
            SelectStage(_selectedStageNumber);
        }

        internal void SelectStage(int displayNumber)
        {
            if (displayNumber < 1 ||
                displayNumber > StageCatalog.Count ||
                displayNumber > _highestUnlocked)
            {
                return;
            }
            _selectedStageNumber = displayNumber;
            _session = new StageSession(
                StageCatalog.GetByDisplayNumber(displayNumber));
            _shieldSelected = false;
            _boosterSelected = false;
            ApplyUiFlow(MobileUiFlow.PreRun);
            selectedStageText.text =
                $"STAGE {displayNumber}\n{_session.Stage.Title}";
            SynchronizeItemSelection();
        }

        internal void ToggleShieldSelection()
        {
            if (_session == null ||
                _session.FlowState != StageFlowState.PreRunSelection)
            {
                return;
            }
            _shieldSelected = !_shieldSelected;
            SynchronizeItemSelection();
        }

        internal void ToggleBoosterSelection()
        {
            if (_session == null ||
                _session.FlowState != StageFlowState.PreRunSelection)
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
            _session.SelectItems(
                new StartItemSelection(_shieldSelected, _boosterSelected));
            if (!_session.BeginCountdown())
            {
                return;
            }
            ResetRunPresentation();
            BuildInitialGatePool();
            ApplyUiFlow(MobileUiFlow.Countdown);
            shieldVisual.SetActive(_shieldSelected);
            _countdownRemaining = CountdownDuration;
            UpdateCountdownText();
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
            if (_experimentActive && gate.HasExperimentPlan)
            {
                return HandleExperimentGateCrossed(gate);
            }
            GatePlan plan = gate.ActivePlan;
            int gateIndex = gate.PlanIndex;
            GateOutcome outcome = _session.ResolveGate(gate.AssignedColor);
            if (outcome == GateOutcome.Matched ||
                outcome == GateOutcome.Invulnerable)
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
            if (_session.FlowState == StageFlowState.StageFinishing)
            {
                ShowGoalAfter(gate.transform.position.z);
            }
            _telemetry.Record(
                "gate",
                _session,
                gateIndex,
                plan,
                outcome.ToString(),
                0f);
            SynchronizeViews();
            return outcome;
        }

        internal void ContinueAfterFailure()
        {
            if (_session == null || !_session.ContinueAfterFailure())
            {
                return;
            }
            failPanel.SetActive(false);
            _failureDelayRemaining = 0f;
            ApplySafeOverridesToActiveGates();
            ApplyUiFlow(MobileUiFlow.Countdown);
            _countdownRemaining = CountdownDuration;
            UpdateCountdownText();
        }

        internal void RetryToItemSelection()
        {
            if (_session == null)
            {
                return;
            }
            _session.RetryToSelection();
            ResetRunPresentation();
            failPanel.SetActive(false);
            clearPanel.SetActive(false);
            ApplyUiFlow(MobileUiFlow.PreRun);
            selectedStageText.text =
                $"STAGE {_selectedStageNumber}\n{_session.Stage.Title}";
            SynchronizeItemSelection();
        }

        internal void ShowLobby()
        {
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
#else
            resetProgressButton.gameObject.SetActive(false);
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

        internal void SetProgressStoreForTests(IStageProgressStore store)
        {
            _progressStore = store ?? throw new ArgumentNullException(nameof(store));
            _highestUnlocked = Mathf.Clamp(
                _progressStore.LoadHighestUnlocked(),
                1,
                StageCatalog.Count);
            ShowLobby();
        }

        internal void SetHapticsForTests(IHapticFeedback haptics)
        {
            _haptics = haptics;
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
            ApplyUiFlow(MobileUiFlow.Gameplay);
            BuildInitialExperimentGatePool();
            _colorStackInitialized = false;
            SynchronizeExperimentViews();
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
            if (player == null || playerRenderer == null ||
                gameplayCamera == null || redMaterial == null ||
                blueMaterial == null || greenMaterial == null ||
                yellowMaterial == null || purpleMaterial == null ||
                cyanMaterial == null ||
                failureMaterial == null || tapSurface == null ||
                !tapSurface.HasRequiredReference() || trackPool == null ||
                !trackPool.HasRequiredReferences() || gates == null ||
                gates.Length < 5 || goal == null || shieldVisual == null ||
                successParticles == null || speedLines == null ||
                playerTrail == null || uiFlowRoots == null ||
                uiFlowRoots.Length != 7 || lobbyPanel == null ||
                lobbyStageText == null || lobbyStageTitleText == null ||
                lobbyStageDescriptionText == null || lobbyProgressText == null ||
                lobbyTierText == null || lobbyPlayButton == null ||
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
                boosterToggleText == null || startButton == null ||
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
                failDetailsText == null || continueButton == null ||
                retryButton == null || failLobbyButton == null)
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
            Transform playerTransform,
            Renderer runnerRenderer,
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
            Button failContinue,
            Button retry,
            Button failLobby)
        {
            player = playerTransform;
            playerRenderer = runnerRenderer;
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
            continueButton = failContinue;
            retryButton = retry;
            failLobbyButton = failLobby;
        }

        private void AddListeners()
        {
            lobbyPlayButton.onClick.AddListener(PlayFromLobby);
            resetProgressButton.onClick.AddListener(RequestProgressReset);
            confirmResetProgressButton.onClick.AddListener(ConfirmProgressReset);
            cancelResetProgressButton.onClick.AddListener(CancelProgressReset);
            stageButtons[0].onClick.AddListener(SelectStage1);
            stageButtons[1].onClick.AddListener(SelectStage2);
            stageButtons[2].onClick.AddListener(SelectStage3);
            stageButtons[3].onClick.AddListener(SelectStage4);
            stageButtons[4].onClick.AddListener(SelectStage5);
            developerUnlockAllButton.onClick.AddListener(UnlockAllForDevelopment);
            shieldToggleButton.onClick.AddListener(ToggleShieldSelection);
            boosterToggleButton.onClick.AddListener(ToggleBoosterSelection);
            startButton.onClick.AddListener(StartSelectedStage);
            backButton.onClick.AddListener(ShowLobby);
            continueButton.onClick.AddListener(ContinueAfterFailure);
            retryButton.onClick.AddListener(RetryToItemSelection);
            replayButton.onClick.AddListener(RetryToItemSelection);
            clearContinueButton.onClick.AddListener(ShowLobby);
            clearLobbyButton.onClick.AddListener(ShowLobby);
            failLobbyButton.onClick.AddListener(ShowLobby);
        }

        private void RemoveListeners()
        {
            if (stageButtons == null || stageButtons.Length != StageCatalog.Count)
            {
                return;
            }
            lobbyPlayButton.onClick.RemoveListener(PlayFromLobby);
            resetProgressButton.onClick.RemoveListener(RequestProgressReset);
            confirmResetProgressButton.onClick.RemoveListener(ConfirmProgressReset);
            cancelResetProgressButton.onClick.RemoveListener(CancelProgressReset);
            stageButtons[0].onClick.RemoveListener(SelectStage1);
            stageButtons[1].onClick.RemoveListener(SelectStage2);
            stageButtons[2].onClick.RemoveListener(SelectStage3);
            stageButtons[3].onClick.RemoveListener(SelectStage4);
            stageButtons[4].onClick.RemoveListener(SelectStage5);
            developerUnlockAllButton.onClick.RemoveListener(UnlockAllForDevelopment);
            shieldToggleButton.onClick.RemoveListener(ToggleShieldSelection);
            boosterToggleButton.onClick.RemoveListener(ToggleBoosterSelection);
            startButton.onClick.RemoveListener(StartSelectedStage);
            backButton.onClick.RemoveListener(ShowLobby);
            continueButton.onClick.RemoveListener(ContinueAfterFailure);
            retryButton.onClick.RemoveListener(RetryToItemSelection);
            replayButton.onClick.RemoveListener(RetryToItemSelection);
            clearContinueButton.onClick.RemoveListener(ShowLobby);
            clearLobbyButton.onClick.RemoveListener(ShowLobby);
            failLobbyButton.onClick.RemoveListener(ShowLobby);
        }

        private void SelectStage1() => SelectStage(1);
        private void SelectStage2() => SelectStage(2);
        private void SelectStage3() => SelectStage(3);
        private void SelectStage4() => SelectStage(4);
        private void SelectStage5() => SelectStage(5);

        private void BuildInitialGatePool()
        {
            _nextPlanIndex = 0;
            _nextGateZ =
                player.position.z + InitialGateLeadDistance;
            for (int index = 0; index < gates.Length; index++)
            {
                ActivateNextGate(gates[index]);
            }
        }

        private void BuildInitialExperimentGatePool()
        {
            _nextPlanIndex = 0;
            _nextGateZ = player.position.z + InitialGateLeadDistance;
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
                _experimentSession.GatesPassed);
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
            ExperimentGatePlan plan = gate.ActiveExperimentPlan;
            bool matched =
                _experimentSession.CurrentColor == plan.Color;
            bool booster = _experimentSession.BoosterActive;
            bool shield = _experimentSession.ShieldActive;
            bool resolved = _experimentSession.Resolve(plan);
            GateOutcome outcome;
            if (resolved)
            {
                outcome = matched
                    ? GateOutcome.Matched
                    : booster
                        ? GateOutcome.Boosted
                        : shield
                            ? GateOutcome.Shielded
                            : GateOutcome.Matched;
                gate.ShowSuccess();
                successParticles.transform.position = gate.transform.position;
                successParticles.Play();
                RecycleOrDeactivate(gate);
            }
            else
            {
                outcome = GateOutcome.Mismatched;
                gate.ShowFailure(failureMaterial);
                stageHudText.text = "EXPERIMENT FAILED";
            }
            UpdateExperimentGateVisibility();
            SynchronizeExperimentViews();
            return outcome;
        }

        private void TickExperiment(float deltaTime)
        {
            if (_experimentSession == null ||
                _experimentSession.Failed ||
                _experimentSession.Completed)
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
            player.position += Vector3.forward * distance;
            trackPool.Tick(player.position.z);
            bool ice = upcoming.ActiveExperimentPlan.IsIce;
            trackPool.SetSurfaceMaterial(
                ice ? cyanMaterial : _normalTrackMaterial);
            UpdateExperimentGateVisibility();
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

        private void UpdateExperimentGateVisibility()
        {
            for (int index = 0; index < gates.Length; index++)
            {
                StageGateView gate = gates[index];
                if (gate.gameObject.activeSelf && gate.HasExperimentPlan)
                {
                    gate.UpdateExperimentVisibility(
                        _experimentSession.GatesPassed,
                        _normalTrackMaterial);
                }
            }
        }

        private void ShowGoalAfter(float gateZ)
        {
            goal.transform.position = new Vector3(0f, 1.5f, gateZ + GoalDistance);
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
            StageRecord record =
                _progressStore.LoadRecord(_selectedStageNumber);
            record = StageProgress.RecordClear(
                record,
                _session.ElapsedPlayingSeconds,
                _session.Items,
                _session.ContinueUsed);
            _progressStore.SaveRecord(_selectedStageNumber, record);
            _highestUnlocked = StageProgress.HighestUnlockedAfterClear(
                _highestUnlocked,
                _selectedStageNumber);
            _progressStore.SaveHighestUnlocked(_highestUnlocked);
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
                if (_failureDelayRemaining <= 0f && _session != null)
                {
                    failPanel.SetActive(true);
                    failTitleText.text = "STAGE FAILED";
                    failDetailsText.text =
                        $"PROGRESS {_session.GatesPassed}/{_session.Stage.TargetGateCount}\n" +
                        $"ITEMS {FormatItems(_session.Items)}";
                    continueButton.gameObject.SetActive(
                        _session.ContinueAvailable);
                }
            }
            if (_clearDelayRemaining > 0f)
            {
                _clearDelayRemaining = Mathf.Max(
                    0f,
                    _clearDelayRemaining - deltaTime);
                if (_clearDelayRemaining <= 0f && _session != null)
                {
                    clearPanel.SetActive(true);
                    clearTitleText.text = "STAGE CLEAR";
                    StageRecord record =
                        _progressStore.LoadRecord(_selectedStageNumber);
                    clearDetailsText.text =
                        $"STAGE {_selectedStageNumber} COMPLETE\n" +
                        $"ITEMS {FormatItems(_session.Items)}\n" +
                        $"TIME {_session.ElapsedPlayingSeconds:0.00}s\n" +
                        $"BEST {record.BestTime:0.00}s";
                }
            }
        }

        private void ResetRunPresentation()
        {
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

            float boosterNormalized = _session.Items.Booster
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
        }

        private void SynchronizeItemSelection()
        {
            shieldToggleText.text = _shieldSelected
                ? "SHIELD: ON"
                : "SHIELD: OFF";
            boosterToggleText.text = _boosterSelected
                ? "BOOSTER: ON"
                : "BOOSTER: OFF";
        }

        private void RefreshStageButtons()
        {
            for (int index = 0; index < StageCatalog.Count; index++)
            {
                int number = index + 1;
                bool unlocked = number <= _highestUnlocked;
                stageButtons[index].interactable = unlocked;
                StageRecord record = _progressStore.LoadRecord(number);
                stageSummaryTexts[index].text =
                    $"STAGE {number}  {(record.Cleared ? "CLEARED" : unlocked ? "OPEN" : "LOCKED")}\n" +
                    StageCatalog.GetByDisplayNumber(number).Title;
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
                _session.ContinueUsed,
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
            if (_session.FlowState == StageFlowState.Countdown &&
                _session.FailedGatePendingForContinue)
            {
                firstUpcomingPlanIndex++;
            }

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
                    !_experimentSession.Failed;
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
            shieldIcon.SetActive(_experimentSession.ShieldActive);
            boosterMeterRoot.SetActive(_experimentSession.BoosterActive);
            boosterMeterFill.fillAmount = Mathf.Clamp01(
                _experimentSession.BoosterDistanceRemaining / 160f);
            SetHorizontalFill(
                boosterMeterFill,
                boosterMeterFill.fillAmount);
            boosterWarning.SetActive(false);
            SynchronizeColorHud();
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
