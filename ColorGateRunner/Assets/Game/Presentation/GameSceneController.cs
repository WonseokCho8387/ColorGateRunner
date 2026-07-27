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
        private const float FailurePresentationDelay = 0.85f;
        private const float FailureShakeAmplitude = 0.14f;
        private const float ReadyPulseSpeed = 3f;

        [SerializeField] private uint configuredSeed = GameRules.DefaultSeed;
        [SerializeField] private Transform player;
        [SerializeField] private Renderer playerRenderer;
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private Material redMaterial;
        [SerializeField] private Material blueMaterial;
        [SerializeField] private Material failureMaterial;
        [SerializeField] private GameObject scorePanel;
        [SerializeField] private Text scoreLabel;
        [SerializeField] private Text scoreText;
        [SerializeField] private RectTransform scorePulseTarget;
        [SerializeField] private Text shieldText;
        [SerializeField] private GameObject readyOverlay;
        [SerializeField] private Text readyTitleText;
        [SerializeField] private Text readyInstructionText;
        [SerializeField] private Text readyTapText;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Text gameOverScoreText;
        [SerializeField] private Text bestScoreText;
        [SerializeField] private Text topScoresText;
        [SerializeField] private Button restartButton;
        [SerializeField] private GameplayTapSurface gameplayTapSurface;
        [SerializeField] private ParticleSystem successParticles;
        [SerializeField] private GateView[] gates;

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
        internal Button RestartButton => restartButton;
        internal GameplayTapSurface TapSurface => gameplayTapSurface;
        internal ParticleSystem SuccessParticles => successParticles;
        internal int GatePoolSize => gates == null ? 0 : gates.Length;
        internal int BestScore => _bestScore;
        internal bool IsScorePulsing => _scorePulseRemaining > 0f;
        internal bool IsSuccessFeedbackActive => _successPulseRemaining > 0f;
        internal bool IsFailureFeedbackActive => _failureShakeRemaining > 0f;

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
            _session.Advance(deltaTime);
            TickFeedback(deltaTime);
            TickMovement(deltaTime);
        }

        internal void HandleGameplayTap()
        {
            if (_session.CurrentState == RunState.Ready)
            {
                _session.StartRun();
            }
            else if (_session.CurrentState == RunState.Playing)
            {
                _session.TryToggleColor();
            }

            SynchronizeViews();
        }

        internal GateOutcome HandleGateCrossed(GateView gate)
        {
            GateOutcome outcome = _session.ResolveGate(gate.AssignedColor);

            if (outcome == GateOutcome.Matched)
            {
                SynchronizeViews();
                TriggerSuccessFeedback(gate);
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

            return outcome;
        }

        internal void TickMovement(float deltaTime)
        {
            if (_session == null || _session.CurrentState != RunState.Playing)
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

        internal bool HasRequiredReferences()
        {
            if (player == null ||
                playerRenderer == null ||
                gameplayCamera == null ||
                redMaterial == null ||
                blueMaterial == null ||
                failureMaterial == null ||
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
                gameOverScoreText == null ||
                bestScoreText == null ||
                topScoresText == null ||
                restartButton == null ||
                gameplayTapSurface == null ||
                successParticles == null ||
                gates == null ||
                gates.Length < 3)
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

        private void RestartGame()
        {
            _session.Restart();
            ResetPresentation();
            _session.StartRun();
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
            player.position = _playerStartPosition;
            player.localScale = _playerStartScale;
            player.localRotation = Quaternion.identity;
            gameplayCamera.transform.SetPositionAndRotation(
                _cameraStartPosition,
                _cameraStartRotation);
            scorePulseTarget.localScale = Vector3.one;
            readyTapText.rectTransform.localScale = Vector3.one;
            successParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            _nextGateZ = _playerStartPosition.z;
            for (int index = 0; index < gates.Length; index++)
            {
                _nextGateZ += _session.GetNextGateSpacing();
                RunnerColor gateColor = _session.GetNextGateColor();
                gates[index].Activate(
                    gateColor,
                    GetMaterial(gateColor),
                    _nextGateZ);
            }

            SynchronizeViews();
        }

        private void RecycleGate(GateView gate)
        {
            _nextGateZ += _session.GetNextGateSpacing();
            RunnerColor gateColor = _session.GetNextGateColor();
            gate.Activate(gateColor, GetMaterial(gateColor), _nextGateZ);
        }

        private Material GetMaterial(RunnerColor color)
        {
            return color == RunnerColor.Red ? redMaterial : blueMaterial;
        }

        private void SynchronizeViews()
        {
            if (_session.CurrentState != RunState.Dead)
            {
                playerRenderer.sharedMaterial = GetMaterial(_session.CurrentColor);
            }

            scoreText.text = _session.CurrentScore.ToString();
            shieldText.text = _session.ShieldActive ? "SHIELD  READY" : "SHIELD  EMPTY";
            readyOverlay.SetActive(_session.CurrentState == RunState.Ready);
            gameOverPanel.SetActive(
                _session.CurrentState == RunState.Dead && _gameOverReady);
            gameOverScoreText.text = $"SCORE  {_session.CurrentScore}";
            bestScoreText.text = $"BEST  {_bestScore}";
            topScoresText.text = FormatTopScores();
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
            gameplayCamera.transform.position =
                _failureCameraBasePosition + (Vector3.right * FailureShakeAmplitude);
            player.localScale = _playerStartScale;
            player.localRotation = Quaternion.Euler(0f, 0f, 68f);
            playerRenderer.sharedMaterial = failureMaterial;
            gate.ShowFailure(failureMaterial);
        }

        private void TriggerShieldBreakFeedback(GateView gate)
        {
            _successPulseRemaining = SuccessPulseDuration;
            player.localScale = _playerStartScale * 1.2f;
            gate.ShowFailure(failureMaterial);
            successParticles.transform.position =
                gate.transform.position + (Vector3.up * 1.5f);
            successParticles.Emit(16);
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
            _topScores = ScoreHistory.Insert(_topScores, _session.CurrentScore);
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
                return "TOP SCORES\n--";
            }

            string result = "TOP SCORES";
            for (int index = 0; index < _topScores.Length; index++)
            {
                result += $"\n{index + 1}.  {_topScores[index]}";
            }
            return result;
        }

        private void TickFeedback(float deltaTime)
        {
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
                    float phase =
                        _failureShakeRemaining / FailureShakeDuration;
                    float direction = phase > 0.66f || phase < 0.33f ? 1f : -1f;
                    gameplayCamera.transform.position =
                        _failureCameraBasePosition +
                        (Vector3.right * (FailureShakeAmplitude * direction * phase));
                }
            }

            if (_failurePresentationRemaining > 0f)
            {
                _failurePresentationRemaining =
                    Mathf.Max(0f, _failurePresentationRemaining - deltaTime);
                if (_failurePresentationRemaining == 0f)
                {
                    _gameOverReady = true;
                    SynchronizeViews();
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
            Material failure,
            GameObject hudPanel,
            Text hudLabel,
            Text hudScore,
            RectTransform hudPulseTarget,
            Text shieldIndicator,
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
            GateView[] gatePool)
        {
            configuredSeed = seed;
            player = playerTransform;
            playerRenderer = playerVisual;
            gameplayCamera = sceneCamera;
            redMaterial = red;
            blueMaterial = blue;
            failureMaterial = failure;
            scorePanel = hudPanel;
            scoreLabel = hudLabel;
            scoreText = hudScore;
            scorePulseTarget = hudPulseTarget;
            shieldText = shieldIndicator;
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
            gates = gatePool;
        }
    }
}
