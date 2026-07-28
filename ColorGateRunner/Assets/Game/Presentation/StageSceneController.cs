using System;
using ColorGateRunner.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    public sealed class StageSceneController : MonoBehaviour
    {
        private const float CountdownDuration = 3f;
        private const float GoalDistance = 10f;
        private const float FailurePanelDelay = 1f;
        private const float ClearPanelDelay = 1.2f;
        private const float BoosterWarningThreshold = 0.2f;
        private const float BoosterFov = 74f;
        private const float NormalFov = 60f;

        [SerializeField] private Transform player;
        [SerializeField] private Renderer playerRenderer;
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private Material redMaterial;
        [SerializeField] private Material blueMaterial;
        [SerializeField] private Material greenMaterial;
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

        [SerializeField] private GameObject lobbyPanel;
        [SerializeField] private Text lobbyStageText;
        [SerializeField] private Text lobbyProgressText;
        [SerializeField] private Text lobbyTierText;
        [SerializeField] private Button lobbyPlayButton;
        [SerializeField] private GameObject[] lobbyTierRoots;

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
        [SerializeField] private Text shieldStatusText;
        [SerializeField] private Text boosterStatusText;
        [SerializeField] private Text currentColorText;
        [SerializeField] private Text cycleOrderText;
        [SerializeField] private Text nextColorText;
        [SerializeField] private Image boosterMeterFill;
        [SerializeField] private GameObject boosterWarning;

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
        private DeterministicStageGateSequence _layoutSequence;
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
        internal Image BoosterMeterFill => boosterMeterFill;
        internal Text CurrentColorText => currentColorText;
        internal Text CycleOrderText => cycleOrderText;
        internal Text NextColorText => nextColorText;
        internal Text LobbyStageText => lobbyStageText;
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
            TickCamera(deltaTime);
            TickOutcomePresentation(deltaTime);

            if (_session == null)
            {
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
                    _session.CompleteCountdown();
                    countdownPanel.SetActive(false);
                    stageHud.SetActive(true);
                    ApplyItemPresentation();
                    SynchronizeViews();
                }
                return;
            }

            bool boosterBefore = _session.BoosterActive;
            TickMovement(deltaTime);
            if (boosterBefore && !_session.BoosterActive)
            {
                RebuildUpcomingGatesForSafeTransition();
            }
            SynchronizeViews();
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
            gameplayCamera.transform.position =
                player.position + _cameraFollowOffset;
            trackPool.Tick(player.position.z);
            if (_session.FlowState == StageFlowState.StageFinishing &&
                goal.activeSelf &&
                player.position.z >= goal.transform.position.z)
            {
                CompleteStageAtGoal();
            }
        }

        internal void HandleGameplayTap()
        {
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
            lobbyPanel.SetActive(false);
            stageSelectPanel.SetActive(false);
            itemPanel.SetActive(true);
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
            itemPanel.SetActive(false);
            countdownPanel.SetActive(true);
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
            GatePlan plan = new GatePlan(
                gate.AssignedColor,
                0f,
                0f,
                1f,
                GatePatternType.Steady,
                _session.GatesPassed,
                false);
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
                TriggerFailure();
            }
            if (_session.FlowState == StageFlowState.StageFinishing)
            {
                ShowGoalAfter(gate.transform.position.z);
            }
            _telemetry.Record(
                "gate",
                _session,
                _session.GatesPassed,
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
            player.localRotation = Quaternion.identity;
            player.localScale = Vector3.one;
            playerRenderer.sharedMaterial = GetMaterial(_session.CurrentColor);
            shieldVisual.SetActive(_session.ShieldActive);
            ResetBoosterPresentation();
            RebuildUpcomingGatesForSafeTransition();
            countdownPanel.SetActive(true);
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
            itemPanel.SetActive(true);
            selectedStageText.text =
                $"STAGE {_selectedStageNumber}\n{_session.Stage.Title}";
            SynchronizeItemSelection();
        }

        internal void ShowLobby()
        {
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
            lobbyStageText.text = LobbyProgression.IsPrototypeComplete(cleared)
                ? $"PROTOTYPE COMPLETE\nSTAGE 5 · {stage.Title}"
                : $"STAGE {_selectedStageNumber}\n{stage.Title}";
            int clearedCount = 0;
            for (int index = 0; index < cleared.Length; index++)
            {
                if (cleared[index])
                {
                    clearedCount++;
                }
            }
            lobbyProgressText.text = $"{clearedCount} / {StageCatalog.Count} CLEARED";
            lobbyTierText.text = $"LOBBY TIER {_lobbyTier}";
            lobbyPanel.SetActive(true);
            stageSelectPanel.SetActive(false);
            itemPanel.SetActive(false);
            countdownPanel.SetActive(false);
            stageHud.SetActive(false);
            clearPanel.SetActive(false);
            failPanel.SetActive(false);
            RefreshStageButtons();
        }

        internal void ShowStageSelect()
        {
            ShowLobby();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            lobbyPanel.SetActive(false);
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
                failureMaterial == null || tapSurface == null ||
                !tapSurface.HasRequiredReference() || trackPool == null ||
                !trackPool.HasRequiredReferences() || gates == null ||
                gates.Length < 5 || goal == null || shieldVisual == null ||
                successParticles == null || speedLines == null ||
                playerTrail == null || lobbyPanel == null ||
                lobbyStageText == null || lobbyProgressText == null ||
                lobbyTierText == null || lobbyPlayButton == null ||
                lobbyTierRoots == null || lobbyTierRoots.Length != 4 ||
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
                progressFill == null || shieldStatusText == null ||
                boosterStatusText == null || currentColorText == null ||
                cycleOrderText == null || nextColorText == null ||
                boosterMeterFill == null || boosterWarning == null ||
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
            Material failure,
            GameplayTapSurface gameplayTapSurface,
            TrackPoolController pool,
            StageGateView[] gatePool,
            GameObject goalObject,
            GameObject shieldObject,
            ParticleSystem success,
            ParticleSystem boosterLines,
            TrailRenderer trail,
            GameObject lobby,
            Text lobbyStage,
            Text lobbyProgress,
            Text lobbyTierLabel,
            Button lobbyPlay,
            GameObject[] tierRoots,
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
            Text hudShield,
            Text hudBooster,
            Text hudCurrent,
            Text hudCycle,
            Text hudNext,
            Image boosterFill,
            GameObject warning,
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
            failureMaterial = failure;
            tapSurface = gameplayTapSurface;
            trackPool = pool;
            gates = gatePool;
            goal = goalObject;
            shieldVisual = shieldObject;
            successParticles = success;
            speedLines = boosterLines;
            playerTrail = trail;
            lobbyPanel = lobby;
            lobbyStageText = lobbyStage;
            lobbyProgressText = lobbyProgress;
            lobbyTierText = lobbyTierLabel;
            lobbyPlayButton = lobbyPlay;
            lobbyTierRoots = tierRoots;
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
            shieldStatusText = hudShield;
            boosterStatusText = hudBooster;
            currentColorText = hudCurrent;
            cycleOrderText = hudCycle;
            nextColorText = hudNext;
            boosterMeterFill = boosterFill;
            boosterWarning = warning;
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
            _layoutSequence = new DeterministicStageGateSequence(_session.Stage);
            _nextPlanIndex = 0;
            _nextGateZ = player.position.z + 12f;
            for (int index = 0; index < gates.Length; index++)
            {
                ActivateNextGate(gates[index]);
            }
        }

        private void RebuildUpcomingGatesForSafeTransition()
        {
            _layoutSequence = new DeterministicStageGateSequence(_session.Stage);
            for (int index = 0; index < _session.GatesPassed; index++)
            {
                _layoutSequence.GetPlan(index);
            }
            _nextPlanIndex = _session.GatesPassed;
            _nextGateZ = player.position.z + 8f;
            for (int index = 0; index < gates.Length; index++)
            {
                ActivateNextGate(gates[index]);
            }
        }

        private void ActivateNextGate(StageGateView gate)
        {
            if (_nextPlanIndex >= _session.Stage.TargetGateCount)
            {
                gate.Deactivate();
                return;
            }
            GatePlan basePlan = _layoutSequence.GetPlan(_nextPlanIndex);
            int ahead = _nextPlanIndex - _session.GatesPassed;
            GatePlan plan = _session.AdjustForSafeTransition(basePlan, ahead);
            _nextGateZ += plan.Spacing;
            gate.Activate(plan.Color, GetMaterial(plan.Color), _nextGateZ);
            _nextPlanIndex++;
        }

        private void RecycleOrDeactivate(StageGateView gate)
        {
            if (_nextPlanIndex < _session.Stage.TargetGateCount)
            {
                ActivateNextGate(gate);
            }
            else
            {
                gate.Deactivate();
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
            stageHud.SetActive(false);
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

        private void TriggerFailure()
        {
            _cameraShakeRemaining = 0.3f;
            _failureDelayRemaining = FailurePanelDelay;
            playerRenderer.sharedMaterial = failureMaterial;
            player.localRotation = Quaternion.Euler(65f, 0f, 18f);
            player.localScale = Vector3.one * 0.8f;
            shieldVisual.SetActive(false);
            ResetBoosterPresentation();
            stageHud.SetActive(false);
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
            player.position = _playerStartPosition;
            player.localRotation = Quaternion.identity;
            player.localScale = Vector3.one;
            gameplayCamera.transform.SetPositionAndRotation(
                _cameraStartPosition,
                _cameraStartRotation);
            gameplayCamera.fieldOfView = NormalFov;
            playerRenderer.sharedMaterial = redMaterial;
            trackPool.ResetPool();
            goal.SetActive(false);
            shieldVisual.SetActive(false);
            successParticles.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear);
            ResetBoosterPresentation();
            boosterMeterFill.fillAmount = 0f;
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
                playerTrail.emitting = true;
                gameplayCamera.fieldOfView = BoosterFov;
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
            gameplayCamera.fieldOfView = NormalFov;
            boosterWarning.SetActive(false);
            boosterMeterFill.fillAmount = 0f;
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
            shieldStatusText.text = _session.ShieldActive
                ? "SHIELD READY"
                : "SHIELD --";
            currentColorText.text = $"CURRENT\n{_session.CurrentColor.ToString().ToUpperInvariant()}";
            cycleOrderText.text = _session.Stage.AllowedColorCount == 3 &&
                !(_session.Stage.DisplayNumber == 4 &&
                _session.GatesPassed < _session.Stage.IntroGateCount)
                ? "RED  >  BLUE  >  GREEN  >  RED"
                : "RED  >  BLUE  >  RED";
            nextColorText.text =
                $"NEXT: {GetNextColor(_session).ToString().ToUpperInvariant()}";

            float boosterNormalized = _session.Items.Booster
                ? _session.BoosterDistanceRemaining /
                    _session.Stage.BoosterDistance
                : 0f;
            boosterMeterFill.fillAmount = Mathf.Clamp01(boosterNormalized);
            boosterWarning.SetActive(
                _session.BoosterActive &&
                boosterNormalized <= BoosterWarningThreshold);
            boosterStatusText.text = _session.BoosterActive
                ? "BOOSTER"
                : _session.BoosterExitActive ? "BOOST END" : string.Empty;

            if (_session.BoosterPresentationStrength > 0f)
            {
                if (!speedLines.isPlaying)
                {
                    speedLines.Play();
                }
                playerTrail.emitting = true;
                float strength = _session.BoosterPresentationStrength;
                if (_session.BoosterActive &&
                    boosterNormalized <= BoosterWarningThreshold)
                {
                    strength *= 0.75f + (boosterNormalized * 1.25f);
                }
                gameplayCamera.fieldOfView = Mathf.Lerp(
                    NormalFov,
                    BoosterFov,
                    strength);
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
            if (_cameraShakeRemaining <= 0f)
            {
                return;
            }
            _cameraShakeRemaining = Mathf.Max(
                0f,
                _cameraShakeRemaining - deltaTime);
            if (_cameraShakeRemaining <= 0f)
            {
                gameplayCamera.transform.position =
                    player.position + _cameraFollowOffset;
                return;
            }
            float phase = _cameraShakeRemaining * 100f;
            gameplayCamera.transform.position =
                player.position +
                _cameraFollowOffset +
                new Vector3(
                    Mathf.Sin(phase) * 0.15f,
                    Mathf.Cos(phase * 0.7f) * 0.05f,
                    0f);
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
                lobbyTierRoots[index].SetActive(index <= _lobbyTier);
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
            return redMaterial;
        }

        private static RunnerColor GetNextColor(StageSession session)
        {
            int count = session.Stage.DisplayNumber == 4 &&
                session.GatesPassed < session.Stage.IntroGateCount
                ? 2
                : session.Stage.AllowedColorCount;
            for (int index = 0; index < count; index++)
            {
                if (session.Stage.GetAllowedColor(index) == session.CurrentColor)
                {
                    return session.Stage.GetAllowedColor((index + 1) % count);
                }
            }
            return session.Stage.GetAllowedColor(0);
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
