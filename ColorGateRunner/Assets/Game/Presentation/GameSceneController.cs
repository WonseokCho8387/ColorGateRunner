using System;
using ColorGateRunner.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    public sealed class GameSceneController : MonoBehaviour
    {
        private const float ScorePulseDuration = 0.2f;
        private const float SuccessPulseDuration = 0.25f;
        private const float FailureShakeDuration = 0.22f;
        private const float FailurePresentationDelay = 0.9f;
        private const float FailureShakeAmplitude = 0.14f;
        private const float ReadyPulseSpeed = 3f;
        private const float ShieldHitStopDuration = 0.12f;
        private const float ShieldMessageDuration = 1.1f;
        private const float CountdownGoWindow = 0.25f;
        private const float RankPulseDuration = 0.6f;
        private const float ThirdColorMessageDuration = 1.4f;
        private const float DiagnosticsRefreshInterval = 0.25f;
        private const float ResultScorePulseSpeed = 3.5f;

        [SerializeField] private uint configuredSeed = GameRules.DefaultSeed;
        [SerializeField] private Transform player;
        [SerializeField] private Renderer playerRenderer;
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private Material redMaterial;
        [SerializeField] private Material blueMaterial;
        [SerializeField] private Material greenMaterial;
        [SerializeField] private Material failureMaterial;
        [SerializeField] private GameObject scorePanel;
        [SerializeField] private Text scoreLabel;
        [SerializeField] private Text scoreText;
        [SerializeField] private RectTransform scorePulseTarget;
        [SerializeField] private Text shieldText;
        [SerializeField] private GameObject shieldVisual;
        [SerializeField] private Text shieldMessageText;
        [SerializeField] private ShieldPickupView shieldPickup;
        [SerializeField] private Text colorCycleText;
        [SerializeField] private Text thirdColorMessageText;
        [SerializeField] private Text speedStageText;
        [SerializeField] private GameObject diagnosticsPanel;
        [SerializeField] private Text diagnosticsText;
        [SerializeField] private bool diagnosticsEnabled;
        [SerializeField] private TrailRenderer playerTrail;
        [SerializeField] private ParticleSystem speedLines;
        [SerializeField] private ParticleSystem shieldParticles;
        [SerializeField] private TrackPoolController trackPool;
        [SerializeField] private GameObject readyOverlay;
        [SerializeField] private Text readyTitleText;
        [SerializeField] private Text readyInstructionText;
        [SerializeField] private Text readyTapText;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject resultCard;
        [SerializeField] private Text thisRunLabel;
        [SerializeField] private Text gameOverScoreText;
        [SerializeField] private Text bestScoreText;
        [SerializeField] private Text topScoresText;
        [SerializeField] private GameObject topScoresPanel;
        [SerializeField] private GameObject bestBadge;
        [SerializeField] private Text newBestText;
        [SerializeField] private GameObject[] topScoreRows;
        [SerializeField] private Text[] topScoreRankTexts;
        [SerializeField] private Text[] topScoreValueTexts;
        [SerializeField] private Text[] topScoreMarkerTexts;
        [SerializeField] private Button restartButton;
        [SerializeField] private GameplayTapSurface gameplayTapSurface;
        [SerializeField] private ParticleSystem successParticles;
        [SerializeField] private GateView[] gates;
        [SerializeField] private GameObject countdownPanel;
        [SerializeField] private Text countdownText;
        [SerializeField] private float countdownDuration =
            GameRules.DefaultCountdownDuration;

        private GameSession _session;
        private IBestScoreStore _bestScoreStore;
        private int _bestScore;
        private IScoreHistoryStore _scoreHistoryStore;
        private int[] _topScores;
        private Vector3 _playerStartPosition;
        private Vector3 _playerStartScale;
        private Vector3 _cameraStartPosition;
        private Quaternion _cameraStartRotation;
        private Vector3 _failureCameraBasePosition;
        private float _nextGateZ;
        private float _scorePulseRemaining;
        private float _successPulseRemaining;
        private float _failureShakeRemaining;
        private float _failurePresentationRemaining;
        private bool _gameOverReady;
        private float _readyPulseElapsed;
        private float _shieldHitStopRemaining;
        private float _shieldMessageRemaining;
        private float _countdownRemaining;
        private Vector3 _failurePlayerPosition;
        private Quaternion _failurePlayerRotation;
        private Vector3 _failurePlayerScale;
        private int _completedRunRank = -1;
        private bool _newBest;
        private SpeedStage _displayedSpeedStage;
        private float _rankPulseRemaining;
        private float _thirdColorMessageRemaining;
        private float _diagnosticsRefreshRemaining;
        private float _resultPulseElapsed;
        private GatePlan _lastGatePlan;

        internal GameSession Session => _session;
        internal Transform PlayerTransform => player;
        internal Renderer PlayerRenderer => playerRenderer;
        internal Camera GameplayCamera => gameplayCamera;
        internal GameObject ScorePanel => scorePanel;
        internal Text ScoreLabel => scoreLabel;
        internal Text ScoreText => scoreText;
        internal RectTransform ScorePulseTarget => scorePulseTarget;
        internal GameObject ReadyOverlay => readyOverlay;
        internal Text ReadyTitleText => readyTitleText;
        internal Text ReadyInstructionText => readyInstructionText;
        internal Text ReadyTapText => readyTapText;
        internal GameObject GameOverPanel => gameOverPanel;
        internal Text GameOverScoreText => gameOverScoreText;
        internal Text BestScoreText => bestScoreText;
        internal Text TopScoresText => topScoresText;
        internal Text ShieldText => shieldText;
        internal GameObject ShieldVisual => shieldVisual;
        internal Text ShieldMessageText => shieldMessageText;
        internal ShieldPickupView ShieldPickup => shieldPickup;
        internal Text ColorCycleText => colorCycleText;
        internal Text ThirdColorMessageText => thirdColorMessageText;
        internal Text SpeedStageText => speedStageText;
        internal GameObject DiagnosticsPanel => diagnosticsPanel;
        internal Text DiagnosticsText => diagnosticsText;
        internal ParticleSystem SpeedLines => speedLines;
        internal ParticleSystem ShieldParticles => shieldParticles;
        internal TrailRenderer PlayerTrail => playerTrail;
        internal TrackPoolController TrackPool => trackPool;
        internal GameObject CountdownPanel => countdownPanel;
        internal Text CountdownText => countdownText;
        internal GameObject TopScoresPanel => topScoresPanel;
        internal GameObject ResultCard => resultCard;
        internal Text ThisRunLabel => thisRunLabel;
        internal GameObject BestBadge => bestBadge;
        internal Text NewBestText => newBestText;
        internal Button RestartButton => restartButton;
        internal GameplayTapSurface TapSurface => gameplayTapSurface;
        internal ParticleSystem SuccessParticles => successParticles;
        internal int GatePoolSize => gates == null ? 0 : gates.Length;
        internal int BestScore => _bestScore;
        internal bool IsScorePulsing => _scorePulseRemaining > 0f;
        internal bool IsSuccessFeedbackActive => _successPulseRemaining > 0f;
        internal bool IsFailureFeedbackActive => _failureShakeRemaining > 0f;
        internal float CountdownDuration => countdownDuration;
        internal float CountdownRemaining => _countdownRemaining;
        internal int CompletedRunRank => _completedRunRank;
        internal bool IsNewBest => _newBest;
        internal int TopScoreRowCount =>
            topScoreRows == null ? 0 : topScoreRows.Length;
        internal bool DiagnosticsEnabled => diagnosticsPanel.activeSelf;
        internal GatePlan LastGatePlan => _lastGatePlan;

        internal GameObject GetTopScoreRow(int index)
        {
            return topScoreRows[index];
        }

        internal Text GetTopScoreValueText(int index)
        {
            return topScoreValueTexts[index];
        }

        internal Text GetTopScoreMarkerText(int index)
        {
            return topScoreMarkerTexts[index];
        }

        private void Awake()
        {
            ValidateRequiredReferences();

            _session = new GameSession(configuredSeed);
            _bestScoreStore = new PlayerPrefsBestScoreStore();
            _bestScore = _bestScoreStore.Load();
            _scoreHistoryStore = new PlayerPrefsScoreHistoryStore();
            _topScores = _scoreHistoryStore.Load();
            SynchronizeBestScoreWithHistory();
            _playerStartPosition = player.position;
            _playerStartScale = player.localScale;
            _cameraStartPosition = gameplayCamera.transform.position;
            _cameraStartRotation = gameplayCamera.transform.rotation;

            restartButton.onClick.AddListener(RestartGame);
            ResetPresentation();
        }

        private void OnDestroy()
        {
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(RestartGame);
            }
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        internal void Tick(float deltaTime)
        {
            if (_session.CurrentState == RunState.Countdown)
            {
                TickCountdown(deltaTime);
                TickFeedback(deltaTime);
                return;
            }

            if (_shieldHitStopRemaining > 0f)
            {
                _shieldHitStopRemaining =
                    Mathf.Max(0f, _shieldHitStopRemaining - deltaTime);
                TickFeedback(deltaTime);
                ApplySpeedPresentation(deltaTime);
                return;
            }

            _session.Advance(deltaTime);
            TickFeedback(deltaTime);
            TickMovement(deltaTime);
            ApplySpeedPresentation(deltaTime);
            trackPool.Tick(player.position.z);
        }

        internal void HandleGameplayTap()
        {
            if (_session.CurrentState == RunState.Ready)
            {
                if (_session.BeginCountdown())
                {
                    _countdownRemaining = countdownDuration;
                }
            }
            else if (_session.CurrentState == RunState.Playing)
            {
                _session.TryToggleColor();
            }
            else if (_session.CurrentState == RunState.ShieldRecovery)
            {
                _session.TryToggleColor();
            }

            SynchronizeViews();
        }

        internal GateOutcome HandleGateCrossed(GateView gate)
        {
            bool hadShield = _session.ShieldActive;
            bool hadThirdColor = _session.ThirdColorIntroduced;
            GateOutcome outcome = _session.ResolveGate(gate.AssignedColor);

            if (outcome == GateOutcome.Matched)
            {
                SynchronizeViews();
                TriggerSuccessFeedback(gate);
                if (!hadShield && _session.ShieldActive)
                {
                    TriggerShieldAcquiredFeedback();
                }
                if (!hadThirdColor && _session.ThirdColorIntroduced)
                {
                    TriggerThirdColorIntroduction();
                }
                RecycleGate(gate);
            }
            else if (outcome == GateOutcome.Mismatched)
            {
                RecordCompletedScore();
                UpdateBestScore();
                TriggerFailureFeedback(gate);
                SynchronizeViews();
            }
            else if (outcome == GateOutcome.Shielded)
            {
                SynchronizeViews();
                TriggerShieldBreakFeedback(gate);
                RecycleGate(gate);
            }
            else if (outcome == GateOutcome.Invulnerable)
            {
                gate.ShowFailure(failureMaterial);
                RecycleGate(gate);
            }

            return outcome;
        }

        internal bool HandleShieldPickupCollected(ShieldPickupView pickup)
        {
            if (pickup != shieldPickup || !_session.CollectShieldPickup())
            {
                return false;
            }

            pickup.Deactivate();
            TriggerShieldAcquiredFeedback();
            SynchronizeViews();
            return true;
        }

        internal void TickMovement(float deltaTime)
        {
            if (_session == null ||
                (_session.CurrentState != RunState.Playing &&
                _session.CurrentState != RunState.ShieldRecovery))
            {
                return;
            }

            float distance = _session.CurrentSpeed * deltaTime;
            Vector3 forwardMovement = Vector3.forward * distance;
            player.position += forwardMovement;
            gameplayCamera.transform.position += forwardMovement;
        }

        internal bool IsPlayerCollider(Collider other)
        {
            Transform otherTransform = other.transform;
            return otherTransform == player || otherTransform.IsChildOf(player);
        }

        internal GateView GetGate(int index)
        {
            return gates[index];
        }

        internal void SetBestScoreStoreForTests(IBestScoreStore store)
        {
            _bestScoreStore = store ?? throw new ArgumentNullException(nameof(store));
            _bestScore = Mathf.Max(0, _bestScoreStore.Load());
            SynchronizeViews();
        }

        internal void SetScoreHistoryStoreForTests(IScoreHistoryStore store)
        {
            _scoreHistoryStore = store ?? throw new ArgumentNullException(nameof(store));
            _topScores = _scoreHistoryStore.Load();
            SynchronizeBestScoreWithHistory();
            SynchronizeViews();
        }

        internal void SetDiagnosticsEnabledForTests(bool enabled)
        {
            diagnosticsEnabled = enabled;
            diagnosticsPanel.SetActive(
                enabled &&
                (Application.isEditor || Debug.isDebugBuild));
            _diagnosticsRefreshRemaining = 0f;
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
                shieldVisual == null ||
                shieldMessageText == null ||
                shieldPickup == null ||
                !shieldPickup.HasRequiredReferences() ||
                colorCycleText == null ||
                thirdColorMessageText == null ||
                speedStageText == null ||
                diagnosticsPanel == null ||
                diagnosticsText == null ||
                playerTrail == null ||
                speedLines == null ||
                shieldParticles == null ||
                trackPool == null ||
                !trackPool.HasRequiredReferences() ||
                scorePanel == null ||
                scoreLabel == null ||
                scoreText == null ||
                scorePulseTarget == null ||
                shieldText == null ||
                readyOverlay == null ||
                readyTitleText == null ||
                readyInstructionText == null ||
                readyTapText == null ||
                gameOverPanel == null ||
                resultCard == null ||
                thisRunLabel == null ||
                gameOverScoreText == null ||
                bestScoreText == null ||
                topScoresText == null ||
                topScoresPanel == null ||
                bestBadge == null ||
                newBestText == null ||
                restartButton == null ||
                gameplayTapSurface == null ||
                successParticles == null ||
                countdownPanel == null ||
                countdownText == null ||
                gates == null ||
                gates.Length < 3 ||
                topScoreRows == null ||
                topScoreRankTexts == null ||
                topScoreValueTexts == null ||
                topScoreMarkerTexts == null ||
                topScoreRows.Length != ScoreHistory.Capacity ||
                topScoreRankTexts.Length != ScoreHistory.Capacity ||
                topScoreValueTexts.Length != ScoreHistory.Capacity ||
                topScoreMarkerTexts.Length != ScoreHistory.Capacity)
            {
                return false;
            }

            for (int index = 0; index < ScoreHistory.Capacity; index++)
            {
                if (topScoreRows[index] == null ||
                    topScoreRankTexts[index] == null ||
                    topScoreValueTexts[index] == null ||
                    topScoreMarkerTexts[index] == null)
                {
                    return false;
                }
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

        private void RestartGame()
        {
            if (_session.CurrentState == RunState.Countdown)
            {
                return;
            }

            _session.Restart();
            ResetPresentation();
            _session.BeginCountdown();
            _countdownRemaining = countdownDuration;
            SynchronizeViews();
        }

        private void ResetPresentation()
        {
            _scorePulseRemaining = 0f;
            _successPulseRemaining = 0f;
            _failureShakeRemaining = 0f;
            _failurePresentationRemaining = 0f;
            _gameOverReady = false;
            _readyPulseElapsed = 0f;
            _shieldHitStopRemaining = 0f;
            _shieldMessageRemaining = 0f;
            _countdownRemaining = 0f;
            _completedRunRank = -1;
            _newBest = false;
            _rankPulseRemaining = 0f;
            _thirdColorMessageRemaining = 0f;
            _diagnosticsRefreshRemaining = 0f;
            _resultPulseElapsed = 0f;
            player.position = _playerStartPosition;
            player.localScale = _playerStartScale;
            player.localRotation = Quaternion.identity;
            gameplayCamera.transform.SetPositionAndRotation(
                _cameraStartPosition,
                _cameraStartRotation);
            gameplayCamera.fieldOfView = 60f;
            scorePulseTarget.localScale = Vector3.one;
            readyTapText.rectTransform.localScale = Vector3.one;
            topScoresText.rectTransform.localScale = Vector3.one;
            gameOverScoreText.rectTransform.localScale = Vector3.one;
            newBestText.rectTransform.localScale = Vector3.one;
            successParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            shieldParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            speedLines.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            playerRenderer.enabled = true;
            shieldVisual.SetActive(false);
            shieldMessageText.gameObject.SetActive(false);
            thirdColorMessageText.gameObject.SetActive(false);
            shieldPickup.Deactivate();
            countdownPanel.SetActive(false);
            diagnosticsPanel.SetActive(
                diagnosticsEnabled &&
                (Application.isEditor || Debug.isDebugBuild));
            trackPool.ResetPool();

            _nextGateZ = _playerStartPosition.z;
            for (int index = 0; index < gates.Length; index++)
            {
                GatePlan plan = _session.GetNextGatePlan();
                ActivateGateFromPlan(gates[index], plan);
            }

            SynchronizeViews();
        }

        private void RecycleGate(GateView gate)
        {
            GatePlan plan = _session.GetNextGatePlan();
            ActivateGateFromPlan(gate, plan);
        }

        private void ActivateGateFromPlan(GateView gate, GatePlan plan)
        {
            float previousGateZ = _nextGateZ;
            _nextGateZ += plan.Spacing;
            if (plan.HasShieldPickupBefore &&
                !_session.ShieldActive &&
                !_session.ShieldPickupCollected)
            {
                shieldPickup.Activate(
                    Mathf.Lerp(previousGateZ, _nextGateZ, 0.5f));
            }

            gate.Activate(plan.Color, GetMaterial(plan.Color), _nextGateZ);
            _lastGatePlan = plan;
        }

        private Material GetMaterial(RunnerColor color)
        {
            if (color == RunnerColor.Red)
            {
                return redMaterial;
            }

            return color == RunnerColor.Blue
                ? blueMaterial
                : greenMaterial;
        }

        private void SynchronizeViews()
        {
            if (_session.CurrentState != RunState.Dead)
            {
                playerRenderer.sharedMaterial = GetMaterial(_session.CurrentColor);
            }

            scoreText.text = _session.CurrentScore.ToString();
            bool showingGameOver =
                _session.CurrentState == RunState.Dead && _gameOverReady;
            scorePanel.SetActive(!showingGameOver);
            shieldText.text =
                _session.CurrentState == RunState.ShieldRecovery
                    ? "SHIELD  BROKEN"
                    : _session.ShieldActive
                        ? "SHIELD  READY"
                        : "SHIELD  EMPTY";
            shieldVisual.SetActive(_session.ShieldActive);
            colorCycleText.text = _session.ActiveColorCount == 3
                ? "RED  >  BLUE  >  GREEN"
                : "RED  >  BLUE";
            readyOverlay.SetActive(_session.CurrentState == RunState.Ready);
            countdownPanel.SetActive(_session.CurrentState == RunState.Countdown);
            gameOverPanel.SetActive(showingGameOver);
            resultCard.SetActive(showingGameOver);
            gameOverScoreText.text = $"CURRENT  {_session.CurrentScore}";
            bestScoreText.text = $"★  BEST  {_bestScore}";
            bestBadge.SetActive(_bestScore > 0);
            bool enteredTopFive = _completedRunRank >= 0;
            newBestText.gameObject.SetActive(_newBest || enteredTopFive);
            newBestText.text = _newBest
                ? $"NEW BEST!  •  RANK {_completedRunRank + 1}"
                : enteredTopFive
                    ? $"TOP 5  •  RANK {_completedRunRank + 1}"
                    : string.Empty;
            topScoresText.text = FormatTopScores();
            gameOverScoreText.text = _session.CurrentScore.ToString();
            bestScoreText.text = $"BEST  {_bestScore}";
            newBestText.gameObject.SetActive(_newBest);
            newBestText.text = _newBest ? "NEW BEST!" : string.Empty;
            topScoresText.gameObject.SetActive(false);
            SynchronizeTopScoreRows();
        }

        private void SynchronizeTopScoreRows()
        {
            for (int index = 0; index < ScoreHistory.Capacity; index++)
            {
                bool hasScore =
                    _topScores != null && index < _topScores.Length;
                topScoreRankTexts[index].text = (index + 1).ToString();
                topScoreValueTexts[index].text =
                    hasScore ? _topScores[index].ToString() : "--";
                bool isCurrent = index == _completedRunRank;
                topScoreMarkerTexts[index].text =
                    isCurrent ? "YOU" : string.Empty;
                Image rowBackground =
                    topScoreRows[index].GetComponent<Image>();
                if (rowBackground != null)
                {
                    rowBackground.color = isCurrent
                        ? new Color(0.2f, 0.55f, 0.95f, 0.55f)
                        : new Color(1f, 1f, 1f, 0.055f);
                }
            }
        }

        private void TriggerSuccessFeedback(GateView gate)
        {
            _scorePulseRemaining = ScorePulseDuration;
            _successPulseRemaining = SuccessPulseDuration;
            scorePulseTarget.localScale = Vector3.one * 1.18f;
            player.localScale = _playerStartScale * 1.12f;
            successParticles.transform.position =
                gate.transform.position + (Vector3.up * 1.5f);
            successParticles.Emit(10);
        }

        private void TriggerFailureFeedback(GateView gate)
        {
            _successPulseRemaining = 0f;
            _failureShakeRemaining = FailureShakeDuration;
            _failurePresentationRemaining = FailurePresentationDelay;
            _gameOverReady = false;
            _failureCameraBasePosition = gameplayCamera.transform.position;
            _failurePlayerPosition = player.position;
            _failurePlayerRotation = player.rotation;
            _failurePlayerScale = player.localScale;
            playerRenderer.sharedMaterial = failureMaterial;
            gate.ShowFailure(failureMaterial);
        }

        private void TriggerShieldBreakFeedback(GateView gate)
        {
            _successPulseRemaining = 0f;
            _shieldHitStopRemaining = ShieldHitStopDuration;
            _shieldMessageRemaining = ShieldMessageDuration;
            player.localScale = _playerStartScale * 1.2f;
            gate.ShowFailure(failureMaterial);
            shieldParticles.transform.position =
                gate.transform.position + (Vector3.up * 1.5f);
            shieldParticles.Emit(32);
            shieldMessageText.text = "SHIELD BREAK";
            shieldMessageText.gameObject.SetActive(true);
        }

        private void TriggerShieldAcquiredFeedback()
        {
            _shieldMessageRemaining = ShieldMessageDuration;
            shieldMessageText.text = "SHIELD";
            shieldMessageText.gameObject.SetActive(true);
            shieldVisual.SetActive(true);
            shieldParticles.transform.position =
                player.position + (Vector3.up * 0.5f);
            shieldParticles.Emit(24);
            shieldText.rectTransform.localScale = Vector3.one * 1.25f;
        }

        private void TriggerThirdColorIntroduction()
        {
            _thirdColorMessageRemaining = ThirdColorMessageDuration;
            thirdColorMessageText.text = "NEW COLOR";
            thirdColorMessageText.color =
                greenMaterial.HasProperty("_BaseColor")
                    ? greenMaterial.GetColor("_BaseColor")
                    : greenMaterial.color;
            thirdColorMessageText.gameObject.SetActive(true);
            thirdColorMessageText.rectTransform.localScale =
                Vector3.one * 1.18f;
        }

        private void UpdateBestScore()
        {
            if (_session.CurrentScore <= _bestScore)
            {
                return;
            }

            _bestScore = _session.CurrentScore;
            _bestScoreStore.Save(_bestScore);
        }

        private void RecordCompletedScore()
        {
            ScoreHistoryUpdate update =
                ScoreHistory.InsertWithResult(_topScores, _session.CurrentScore);
            _topScores = update.Scores;
            _completedRunRank = update.InsertedRank;
            _newBest = update.IsNewBest;
            _scoreHistoryStore.Save(_topScores);
        }

        private void SynchronizeBestScoreWithHistory()
        {
            if (_topScores != null && _topScores.Length > 0)
            {
                _bestScore = Mathf.Max(_bestScore, _topScores[0]);
            }
        }

        private string FormatTopScores()
        {
            if (_topScores == null || _topScores.Length == 0)
            {
                return "--";
            }

            string result = string.Empty;
            for (int index = 0; index < _topScores.Length; index++)
            {
                string marker = index == _completedRunRank ? "►" : " ";
                result +=
                    $"{marker}  {index + 1}.          {_topScores[index]}";
                if (index < _topScores.Length - 1)
                {
                    result += "\n";
                }
            }
            return result;
        }

        private void TickFeedback(float deltaTime)
        {
            if (shieldPickup.IsAvailable)
            {
                shieldPickup.Tick(deltaTime);
            }

            if (_thirdColorMessageRemaining > 0f)
            {
                _thirdColorMessageRemaining = Mathf.Max(
                    0f,
                    _thirdColorMessageRemaining - deltaTime);
                float progress =
                    1f -
                    (_thirdColorMessageRemaining /
                    ThirdColorMessageDuration);
                float scale =
                    1f + (Mathf.Sin(progress * Mathf.PI) * 0.18f);
                thirdColorMessageText.rectTransform.localScale =
                    Vector3.one * scale;
                if (_thirdColorMessageRemaining == 0f)
                {
                    thirdColorMessageText.gameObject.SetActive(false);
                    thirdColorMessageText.rectTransform.localScale =
                        Vector3.one;
                }
            }

            if (_gameOverReady &&
                _session.CurrentState == RunState.Dead)
            {
                _resultPulseElapsed += deltaTime;
                float resultScale =
                    1f +
                    (Mathf.Sin(
                        _resultPulseElapsed *
                        ResultScorePulseSpeed) *
                    0.035f);
                gameOverScoreText.rectTransform.localScale =
                    Vector3.one * resultScale;
                if (_newBest)
                {
                    float bestScale =
                        1f +
                        (Mathf.Sin(
                            _resultPulseElapsed *
                            ResultScorePulseSpeed *
                            1.4f) *
                        0.08f);
                    newBestText.rectTransform.localScale =
                        Vector3.one * bestScale;
                }
            }
            else
            {
                gameOverScoreText.rectTransform.localScale = Vector3.one;
                newBestText.rectTransform.localScale = Vector3.one;
            }

            if (shieldVisual.activeSelf)
            {
                shieldVisual.transform.Rotate(
                    0f,
                    90f * deltaTime,
                    0f,
                    Space.Self);
            }

            if (_shieldMessageRemaining > 0f)
            {
                _shieldMessageRemaining =
                    Mathf.Max(0f, _shieldMessageRemaining - deltaTime);
                float pulse =
                    1f + ((_shieldMessageRemaining / ShieldMessageDuration) * 0.2f);
                shieldMessageText.rectTransform.localScale = Vector3.one * pulse;
                shieldText.rectTransform.localScale = Vector3.one * pulse;
                if (_shieldMessageRemaining == 0f)
                {
                    shieldMessageText.gameObject.SetActive(false);
                    shieldText.rectTransform.localScale = Vector3.one;
                }
            }

            if (_rankPulseRemaining > 0f)
            {
                _rankPulseRemaining =
                    Mathf.Max(0f, _rankPulseRemaining - deltaTime);
                float progress =
                    1f - (_rankPulseRemaining / RankPulseDuration);
                float scale =
                    1f + (Mathf.Sin(progress * Mathf.PI) * 0.08f);
                topScoresText.rectTransform.localScale = Vector3.one * scale;
                if (_rankPulseRemaining == 0f)
                {
                    topScoresText.rectTransform.localScale = Vector3.one;
                }
            }

            if (_session.CurrentState == RunState.ShieldRecovery)
            {
                float phase =
                    _session.ShieldRecoveryRemaining * 12f;
                playerRenderer.enabled = Mathf.FloorToInt(phase) % 2 == 0;
            }
            else
            {
                playerRenderer.enabled = true;
            }

            if (_session.CurrentState == RunState.Ready)
            {
                _readyPulseElapsed += deltaTime;
                float pulse =
                    1f + (Mathf.Sin(_readyPulseElapsed * ReadyPulseSpeed) * 0.035f);
                readyTapText.rectTransform.localScale = Vector3.one * pulse;
            }
            else if (readyTapText.rectTransform.localScale != Vector3.one)
            {
                readyTapText.rectTransform.localScale = Vector3.one;
            }

            if (_scorePulseRemaining > 0f)
            {
                _scorePulseRemaining =
                    Mathf.Max(0f, _scorePulseRemaining - deltaTime);
                float normalized = _scorePulseRemaining / ScorePulseDuration;
                scorePulseTarget.localScale =
                    Vector3.one * (1f + (normalized * 0.18f));
                if (_scorePulseRemaining == 0f)
                {
                    scorePulseTarget.localScale = Vector3.one;
                }
            }

            if (_successPulseRemaining > 0f)
            {
                _successPulseRemaining =
                    Mathf.Max(0f, _successPulseRemaining - deltaTime);
                float normalized = _successPulseRemaining / SuccessPulseDuration;
                player.localScale =
                    _playerStartScale * (1f + (normalized * 0.12f));
                if (_successPulseRemaining == 0f)
                {
                    player.localScale = _playerStartScale;
                }
            }

            if (_failureShakeRemaining > 0f)
            {
                _failureShakeRemaining =
                    Mathf.Max(0f, _failureShakeRemaining - deltaTime);
                if (_failureShakeRemaining == 0f)
                {
                    gameplayCamera.transform.position =
                        _failureCameraBasePosition;
                }
                else
                {
                    float progress =
                        1f - (_failureShakeRemaining / FailureShakeDuration);
                    float offset =
                        Mathf.Sin(progress * Mathf.PI * 4f) *
                        (1f - progress) *
                        FailureShakeAmplitude;
                    gameplayCamera.transform.position =
                        _failureCameraBasePosition +
                        (Vector3.right * offset);
                }
            }

            if (_failurePresentationRemaining > 0f)
            {
                _failurePresentationRemaining =
                    Mathf.Max(0f, _failurePresentationRemaining - deltaTime);
                float progress =
                    1f -
                    (_failurePresentationRemaining / FailurePresentationDelay);
                float eased = progress * progress * (3f - (2f * progress));
                player.position =
                    _failurePlayerPosition +
                    (Vector3.forward *
                    (0.7f * (1f - ((1f - progress) * (1f - progress))))) +
                    (Vector3.down * (0.55f * eased));
                player.rotation = Quaternion.Slerp(
                    _failurePlayerRotation,
                    Quaternion.Euler(0f, 0f, 68f),
                    eased);
                player.localScale = Vector3.Lerp(
                    _failurePlayerScale,
                    _failurePlayerScale * 0.88f,
                    eased);
                if (_failurePresentationRemaining == 0f)
                {
                    _gameOverReady = true;
                    _rankPulseRemaining =
                        _completedRunRank >= 0 ? RankPulseDuration : 0f;
                    SynchronizeViews();
                }
            }

            TickDiagnostics(deltaTime);
        }

        private void TickDiagnostics(float deltaTime)
        {
            if (!diagnosticsPanel.activeSelf)
            {
                return;
            }

            _diagnosticsRefreshRemaining -= deltaTime;
            if (_diagnosticsRefreshRemaining > 0f)
            {
                return;
            }

            _diagnosticsRefreshRemaining = DiagnosticsRefreshInterval;
            diagnosticsText.text =
                $"MOVE {_session.CurrentSpeed:0.00}\n" +
                $"INTERVAL {_session.TargetEncounterInterval:0.00}\n" +
                $"STAGE {_session.GetSpeedPresentation().Stage}\n" +
                $"PATTERN {_lastGatePlan.Pattern}\n" +
                $"COLORS {_session.ActiveColorCount}\n" +
                $"SHIELD {_session.CurrentState}";
        }

        private void TickCountdown(float deltaTime)
        {
            _countdownRemaining = Mathf.Max(0f, _countdownRemaining - deltaTime);
            if (_countdownRemaining <= CountdownGoWindow)
            {
            countdownText.text = "GO";
            }
            else
            {
                countdownText.text =
                    Mathf.CeilToInt(_countdownRemaining).ToString();
            }

            float phase = _countdownRemaining - Mathf.Floor(_countdownRemaining);
            countdownText.rectTransform.localScale =
                Vector3.one * (1f + (phase * 0.12f));

            if (_countdownRemaining == 0f)
            {
                _session.CompleteCountdown();
                SynchronizeViews();
            }
        }

        private void ApplySpeedPresentation(float deltaTime)
        {
            SpeedPresentation presentation = _session.GetSpeedPresentation();
            gameplayCamera.fieldOfView = Mathf.MoveTowards(
                gameplayCamera.fieldOfView,
                presentation.CameraFieldOfView,
                16f * deltaTime);
            playerTrail.time =
                Mathf.Lerp(0.08f, 0.6f, presentation.TrailIntensity);
            playerTrail.widthMultiplier =
                Mathf.Lerp(0.15f, 0.42f, presentation.TrailIntensity);

            ParticleSystem.EmissionModule emission = speedLines.emission;
            emission.rateOverTime = presentation.SpeedLineRate;
            if (presentation.SpeedLineRate > 0f && !speedLines.isPlaying)
            {
                speedLines.Play();
            }

            if (_displayedSpeedStage != presentation.Stage ||
                string.IsNullOrEmpty(speedStageText.text))
            {
                _displayedSpeedStage = presentation.Stage;
                switch (presentation.Stage)
                {
                    case SpeedStage.Start:
                        speedStageText.text = "SPEED  I";
                        break;
                    case SpeedStage.Accelerating:
                        speedStageText.text = "SPEED  II";
                        break;
                    case SpeedStage.Fast:
                        speedStageText.text = "SPEED  III";
                        break;
                    default:
                        speedStageText.text = "SPEED  MAX";
                        break;
                }
            }
        }

        private void ValidateRequiredReferences()
        {
            if (!HasRequiredReferences())
            {
                enabled = false;
                throw new InvalidOperationException(
                    "GameSceneController has missing required scene references or fewer than three gates.");
            }
        }

        internal void Configure(
            uint seed,
            Transform playerTransform,
            Renderer playerVisual,
            Camera sceneCamera,
            Material red,
            Material blue,
            Material green,
            Material failure,
            GameObject hudPanel,
            Text hudLabel,
            Text hudScore,
            RectTransform hudPulseTarget,
            Text shieldIndicator,
            Text colorCycleIndicator,
            GameObject startOverlay,
            Text startTitle,
            Text startInstruction,
            Text startTap,
            GameObject gameOverUi,
            Text gameOverScore,
            Text bestScore,
            Text topScores,
            Button restartUi,
            GameplayTapSurface tapSurface,
            ParticleSystem particles,
            ParticleSystem speedLineParticles,
            ParticleSystem shieldBurstParticles,
            TrailRenderer trail,
            GameObject activeShieldVisual,
            ShieldPickupView pickup,
            Text shieldMessage,
            Text thirdColorMessage,
            Text speedStage,
            GameObject developerDiagnosticsPanel,
            Text developerDiagnosticsText,
            TrackPoolController trackPoolController,
            GameObject countdownUi,
            Text countdownValue,
            GameObject centralResultCard,
            Text runLabel,
            GameObject topFivePanel,
            GameObject bestScoreBadge,
            Text newBestLabel,
            GameObject[] rankRows,
            Text[] rankTexts,
            Text[] rankScoreTexts,
            Text[] rankMarkerTexts,
            GateView[] gatePool)
        {
            configuredSeed = seed;
            player = playerTransform;
            playerRenderer = playerVisual;
            gameplayCamera = sceneCamera;
            redMaterial = red;
            blueMaterial = blue;
            greenMaterial = green;
            failureMaterial = failure;
            scorePanel = hudPanel;
            scoreLabel = hudLabel;
            scoreText = hudScore;
            scorePulseTarget = hudPulseTarget;
            shieldText = shieldIndicator;
            colorCycleText = colorCycleIndicator;
            readyOverlay = startOverlay;
            readyTitleText = startTitle;
            readyInstructionText = startInstruction;
            readyTapText = startTap;
            gameOverPanel = gameOverUi;
            gameOverScoreText = gameOverScore;
            bestScoreText = bestScore;
            topScoresText = topScores;
            restartButton = restartUi;
            gameplayTapSurface = tapSurface;
            successParticles = particles;
            speedLines = speedLineParticles;
            shieldParticles = shieldBurstParticles;
            playerTrail = trail;
            shieldVisual = activeShieldVisual;
            shieldPickup = pickup;
            shieldMessageText = shieldMessage;
            thirdColorMessageText = thirdColorMessage;
            speedStageText = speedStage;
            diagnosticsPanel = developerDiagnosticsPanel;
            diagnosticsText = developerDiagnosticsText;
            trackPool = trackPoolController;
            countdownPanel = countdownUi;
            countdownText = countdownValue;
            resultCard = centralResultCard;
            thisRunLabel = runLabel;
            topScoresPanel = topFivePanel;
            bestBadge = bestScoreBadge;
            newBestText = newBestLabel;
            topScoreRows = rankRows;
            topScoreRankTexts = rankTexts;
            topScoreValueTexts = rankScoreTexts;
            topScoreMarkerTexts = rankMarkerTexts;
            gates = gatePool;
        }
    }
}
