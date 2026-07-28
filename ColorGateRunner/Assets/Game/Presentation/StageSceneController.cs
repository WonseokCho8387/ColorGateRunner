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
        private const float CameraShakeDuration = 0.22f;
        private const float BoosterFov = 70f;
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

        [SerializeField] private GameObject clearPanel;
        [SerializeField] private Text clearTitleText;
        [SerializeField] private Text clearDetailsText;
        [SerializeField] private Button nextStageButton;
        [SerializeField] private Button replayButton;
        [SerializeField] private Button clearStageSelectButton;

        [SerializeField] private GameObject failPanel;
        [SerializeField] private Text failTitleText;
        [SerializeField] private Text failDetailsText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button failStageSelectButton;

        private StageSession _session;
        private IStageProgressStore _progressStore;
        private DeterministicStageGateSequence _layoutSequence;
        private int _highestUnlocked;
        private int _selectedStageNumber = 1;
        private bool _shieldSelected;
        private bool _boosterSelected;
        private float _countdownRemaining;
        private float _nextGateZ;
        private int _nextPlanIndex;
        private float _cameraShakeRemaining;
        private Vector3 _playerStartPosition;
        private Vector3 _cameraStartPosition;
        private Quaternion _cameraStartRotation;
        private Vector3 _cameraFollowOffset;
        private bool _clearRecorded;

        internal StageSession Session => _session;
        internal int HighestUnlocked => _highestUnlocked;
        internal int SelectedStageNumber => _selectedStageNumber;
        internal bool ShieldSelected => _shieldSelected;
        internal bool BoosterSelected => _boosterSelected;
        internal GameObject StageSelectPanel => stageSelectPanel;
        internal GameObject ItemPanel => itemPanel;
        internal GameObject CountdownPanel => countdownPanel;
        internal GameObject StageHud => stageHud;
        internal GameObject ClearPanel => clearPanel;
        internal GameObject FailPanel => failPanel;
        internal GameObject Goal => goal;
        internal GameObject ShieldVisual => shieldVisual;
        internal ParticleSystem SpeedLines => speedLines;
        internal TrailRenderer PlayerTrail => playerTrail;
        internal Text CountdownText => countdownText;
        internal Text ProgressText => progressText;
        internal Text ClearDetailsText => clearDetailsText;
        internal Text FailDetailsText => failDetailsText;
        internal Transform PlayerTransform => player;
        internal Camera GameplayCamera => gameplayCamera;
        internal int GatePoolSize => gates == null ? 0 : gates.Length;
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

        internal StageGateView GetGate(int index)
        {
            return gates[index];
        }

        internal Button GetStageButton(int index)
        {
            return stageButtons[index];
        }

        private void Awake()
        {
            ValidateRequiredReferences();
            _progressStore = new PlayerPrefsStageProgressStore();
            _highestUnlocked = _progressStore.LoadHighestUnlocked();
            _playerStartPosition = player.position;
            _cameraStartPosition = gameplayCamera.transform.position;
            _cameraStartRotation = gameplayCamera.transform.rotation;
            _cameraFollowOffset = _cameraStartPosition - _playerStartPosition;
            AddListeners();
            ShowStageSelect();
        }

        private void OnDestroy()
        {
            RemoveListeners();
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        internal void Tick(float deltaTime)
        {
            TickGateReactions(deltaTime);
            TickCamera(deltaTime);

            if (_session == null)
            {
                return;
            }

            if (_session.FlowState == StageFlowState.Countdown)
            {
                _countdownRemaining = Mathf.Max(0f, _countdownRemaining - deltaTime);
                UpdateCountdownText();
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

            TickMovement(deltaTime);
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

            _session.TryToggleColor();
            SynchronizeViews();
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
            _countdownRemaining = CountdownDuration;
            UpdateCountdownText();
        }

        internal GateOutcome HandleGateCrossed(StageGateView gate)
        {
            GateOutcome outcome = _session.ResolveGate(gate.AssignedColor);
            if (outcome == GateOutcome.Matched)
            {
                gate.ShowSuccess();
                successParticles.transform.position = gate.transform.position;
                successParticles.Play();
                RecycleOrDeactivate(gate);
            }
            else if (outcome == GateOutcome.Boosted)
            {
                gate.ShowSuccess();
                RecycleOrDeactivate(gate);
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

            SynchronizeViews();
            return outcome;
        }

        internal bool IsPlayerCollider(Collider other)
        {
            return other.transform == player || other.transform.IsChildOf(player);
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

        internal void ShowStageSelect()
        {
            _session = null;
            ResetRunPresentation();
            stageSelectPanel.SetActive(true);
            itemPanel.SetActive(false);
            countdownPanel.SetActive(false);
            stageHud.SetActive(false);
            clearPanel.SetActive(false);
            failPanel.SetActive(false);
            RefreshStageButtons();
        }

        internal void SetProgressStoreForTests(IStageProgressStore store)
        {
            _progressStore = store ?? throw new ArgumentNullException(nameof(store));
            _highestUnlocked = Mathf.Clamp(
                _progressStore.LoadHighestUnlocked(),
                1,
                StageCatalog.Count);
            RefreshStageButtons();
        }

        internal void UnlockAllForDevelopment()
        {
            _highestUnlocked = StageCatalog.Count;
            _progressStore.SaveHighestUnlocked(_highestUnlocked);
            RefreshStageButtons();
        }

        internal bool HasRequiredReferences()
        {
            if (player == null ||
                playerRenderer == null ||
                gameplayCamera == null ||
                redMaterial == null ||
                blueMaterial == null ||
                greenMaterial == null ||
                failureMaterial == null ||
                tapSurface == null ||
                !tapSurface.HasRequiredReference() ||
                trackPool == null ||
                !trackPool.HasRequiredReferences() ||
                gates == null ||
                gates.Length < 5 ||
                goal == null ||
                shieldVisual == null ||
                successParticles == null ||
                speedLines == null ||
                playerTrail == null ||
                stageSelectPanel == null ||
                stageButtons == null ||
                stageSummaryTexts == null ||
                stageButtons.Length != StageCatalog.Count ||
                stageSummaryTexts.Length != StageCatalog.Count ||
                developerUnlockAllButton == null ||
                itemPanel == null ||
                selectedStageText == null ||
                shieldToggleButton == null ||
                shieldToggleText == null ||
                boosterToggleButton == null ||
                boosterToggleText == null ||
                startButton == null ||
                backButton == null ||
                countdownPanel == null ||
                countdownText == null ||
                stageHud == null ||
                stageHudText == null ||
                progressText == null ||
                progressFill == null ||
                shieldStatusText == null ||
                boosterStatusText == null ||
                clearPanel == null ||
                clearTitleText == null ||
                clearDetailsText == null ||
                nextStageButton == null ||
                replayButton == null ||
                clearStageSelectButton == null ||
                failPanel == null ||
                failTitleText == null ||
                failDetailsText == null ||
                retryButton == null ||
                failStageSelectButton == null)
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
            GameObject clear,
            Text clearTitle,
            Text clearDetails,
            Button next,
            Button replay,
            Button clearSelect,
            GameObject fail,
            Text failTitle,
            Text failDetails,
            Button retry,
            Button failSelect)
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
            clearPanel = clear;
            clearTitleText = clearTitle;
            clearDetailsText = clearDetails;
            nextStageButton = next;
            replayButton = replay;
            clearStageSelectButton = clearSelect;
            failPanel = fail;
            failTitleText = failTitle;
            failDetailsText = failDetails;
            retryButton = retry;
            failStageSelectButton = failSelect;
        }

        private void AddListeners()
        {
            stageButtons[0].onClick.AddListener(SelectStage1);
            stageButtons[1].onClick.AddListener(SelectStage2);
            stageButtons[2].onClick.AddListener(SelectStage3);
            stageButtons[3].onClick.AddListener(SelectStage4);
            stageButtons[4].onClick.AddListener(SelectStage5);
            developerUnlockAllButton.onClick.AddListener(UnlockAllForDevelopment);
            shieldToggleButton.onClick.AddListener(ToggleShieldSelection);
            boosterToggleButton.onClick.AddListener(ToggleBoosterSelection);
            startButton.onClick.AddListener(StartSelectedStage);
            backButton.onClick.AddListener(ShowStageSelect);
            retryButton.onClick.AddListener(RetryToItemSelection);
            replayButton.onClick.AddListener(RetryToItemSelection);
            nextStageButton.onClick.AddListener(OpenNextStage);
            clearStageSelectButton.onClick.AddListener(ShowStageSelect);
            failStageSelectButton.onClick.AddListener(ShowStageSelect);
        }

        private void RemoveListeners()
        {
            if (stageButtons == null || stageButtons.Length != StageCatalog.Count)
            {
                return;
            }

            stageButtons[0].onClick.RemoveListener(SelectStage1);
            stageButtons[1].onClick.RemoveListener(SelectStage2);
            stageButtons[2].onClick.RemoveListener(SelectStage3);
            stageButtons[3].onClick.RemoveListener(SelectStage4);
            stageButtons[4].onClick.RemoveListener(SelectStage5);
            developerUnlockAllButton.onClick.RemoveListener(UnlockAllForDevelopment);
            shieldToggleButton.onClick.RemoveListener(ToggleShieldSelection);
            boosterToggleButton.onClick.RemoveListener(ToggleBoosterSelection);
            startButton.onClick.RemoveListener(StartSelectedStage);
            backButton.onClick.RemoveListener(ShowStageSelect);
            retryButton.onClick.RemoveListener(RetryToItemSelection);
            replayButton.onClick.RemoveListener(RetryToItemSelection);
            nextStageButton.onClick.RemoveListener(OpenNextStage);
            clearStageSelectButton.onClick.RemoveListener(ShowStageSelect);
            failStageSelectButton.onClick.RemoveListener(ShowStageSelect);
        }

        private void SelectStage1() => SelectStage(1);
        private void SelectStage2() => SelectStage(2);
        private void SelectStage3() => SelectStage(3);
        private void SelectStage4() => SelectStage(4);
        private void SelectStage5() => SelectStage(5);

        private void OpenNextStage()
        {
            int next = Mathf.Min(StageCatalog.Count, _selectedStageNumber + 1);
            ShowStageSelect();
            SelectStage(next);
        }

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

        private void ActivateNextGate(StageGateView gate)
        {
            if (_nextPlanIndex >= _session.Stage.TargetGateCount)
            {
                gate.Deactivate();
                return;
            }

            GatePlan plan = _layoutSequence.GetPlan(_nextPlanIndex);
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
            clearPanel.SetActive(true);
            clearTitleText.text = "STAGE CLEAR";
            StageRecord record =
                _progressStore.LoadRecord(_selectedStageNumber);
            clearDetailsText.text =
                $"STAGE {_selectedStageNumber}\n" +
                $"TIME {_session.ElapsedPlayingSeconds:0.00}s\n" +
                $"ITEMS {FormatItems(_session.Items)}\n" +
                $"BEST {record.BestTime:0.00}s";
            nextStageButton.gameObject.SetActive(
                _selectedStageNumber < StageCatalog.Count);
            ResetBoosterPresentation();
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
                _session.Items);
            _progressStore.SaveRecord(_selectedStageNumber, record);
            _highestUnlocked = StageProgress.HighestUnlockedAfterClear(
                _highestUnlocked,
                _selectedStageNumber);
            _progressStore.SaveHighestUnlocked(_highestUnlocked);
        }

        private void TriggerFailure()
        {
            _cameraShakeRemaining = CameraShakeDuration;
            playerRenderer.sharedMaterial = failureMaterial;
            shieldVisual.SetActive(false);
            ResetBoosterPresentation();
            stageHud.SetActive(false);
            failPanel.SetActive(true);
            failTitleText.text = "STAGE FAILED";
            failDetailsText.text =
                $"PROGRESS {_session.GatesPassed}/{_session.Stage.TargetGateCount}\n" +
                $"ITEMS {FormatItems(_session.Items)}";
        }

        private void ResetRunPresentation()
        {
            _clearRecorded = false;
            _cameraShakeRemaining = 0f;
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
            successParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ResetBoosterPresentation();
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
                speedLines.Play();
                playerTrail.emitting = true;
                gameplayCamera.fieldOfView = BoosterFov;
            }
        }

        private void ResetBoosterPresentation()
        {
            speedLines.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            playerTrail.emitting = false;
            gameplayCamera.fieldOfView = NormalFov;
        }

        private void SynchronizeViews()
        {
            if (_session == null)
            {
                return;
            }

            playerRenderer.sharedMaterial = _session.FlowState == StageFlowState.Failed
                ? failureMaterial
                : GetMaterial(_session.CurrentColor);
            shieldVisual.SetActive(_session.ShieldActive);
            stageHudText.text = $"STAGE {_selectedStageNumber}";
            progressText.text =
                $"{_session.GatesPassed} / {_session.Stage.TargetGateCount}";
            progressFill.fillAmount = _session.Progress;
            shieldStatusText.text = _session.ShieldActive
                ? "SHIELD READY"
                : "SHIELD --";
            boosterStatusText.text = _session.BoosterActive
                ? "BOOST"
                : string.Empty;

            if (_session.BoosterPresentationStrength > 0f)
            {
                if (!speedLines.isPlaying)
                {
                    speedLines.Play();
                }
                playerTrail.emitting = true;
                gameplayCamera.fieldOfView = Mathf.Lerp(
                    NormalFov,
                    BoosterFov,
                    _session.BoosterPresentationStrength);
            }
            else if (_session.FlowState != StageFlowState.Failed)
            {
                ResetBoosterPresentation();
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
                string status = unlocked ? "OPEN" : "LOCKED";
                if (record.Cleared)
                {
                    status = $"CLEARED  {record.BestTime:0.00}s";
                }
                stageSummaryTexts[index].text =
                    $"STAGE {number}  {status}\n" +
                    StageCatalog.GetByDisplayNumber(number).Title;
            }

            developerUnlockAllButton.gameObject.SetActive(false);
        }

        private void TickGateReactions(float deltaTime)
        {
            for (int index = 0; index < gates.Length; index++)
            {
                gates[index].Tick(deltaTime);
            }
        }

        private void TickCamera(float deltaTime)
        {
            if (_cameraShakeRemaining <= 0f)
            {
                return;
            }

            _cameraShakeRemaining =
                Mathf.Max(0f, _cameraShakeRemaining - deltaTime);
            if (_cameraShakeRemaining <= 0f)
            {
                gameplayCamera.transform.position =
                    player.position + _cameraFollowOffset;
                return;
            }

            float phase = _cameraShakeRemaining * 90f;
            gameplayCamera.transform.position =
                player.position +
                _cameraFollowOffset +
                new Vector3(Mathf.Sin(phase) * 0.12f, 0f, 0f);
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
