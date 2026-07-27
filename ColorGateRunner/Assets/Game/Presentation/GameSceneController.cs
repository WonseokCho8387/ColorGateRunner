using System;
using ColorGateRunner.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    public sealed class GameSceneController : MonoBehaviour
    {
        [SerializeField] private uint configuredSeed = GameRules.DefaultSeed;
        [SerializeField] private Transform player;
        [SerializeField] private Renderer playerRenderer;
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private Material redMaterial;
        [SerializeField] private Material blueMaterial;
        [SerializeField] private Text scoreText;
        [SerializeField] private GameObject readyLabel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Button restartButton;
        [SerializeField] private GameplayTapSurface gameplayTapSurface;
        [SerializeField] private GateView[] gates;

        private GameSession _session;
        private Vector3 _playerStartPosition;
        private Vector3 _cameraStartPosition;
        private Quaternion _cameraStartRotation;
        private float _nextGateZ;

        internal GameSession Session => _session;
        internal Transform PlayerTransform => player;
        internal Renderer PlayerRenderer => playerRenderer;
        internal Camera GameplayCamera => gameplayCamera;
        internal Text ScoreText => scoreText;
        internal GameObject GameOverPanel => gameOverPanel;
        internal Button RestartButton => restartButton;
        internal GameplayTapSurface TapSurface => gameplayTapSurface;
        internal int GatePoolSize => gates == null ? 0 : gates.Length;

        private void Awake()
        {
            ValidateRequiredReferences();

            _session = new GameSession(configuredSeed);
            _playerStartPosition = player.position;
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
            TickMovement(Time.deltaTime);
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
            SynchronizeViews();

            if (outcome == GateOutcome.Matched)
            {
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

        internal bool HasRequiredReferences()
        {
            if (player == null ||
                playerRenderer == null ||
                gameplayCamera == null ||
                redMaterial == null ||
                blueMaterial == null ||
                scoreText == null ||
                readyLabel == null ||
                gameOverPanel == null ||
                restartButton == null ||
                gameplayTapSurface == null ||
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
        }

        private void ResetPresentation()
        {
            player.position = _playerStartPosition;
            gameplayCamera.transform.SetPositionAndRotation(
                _cameraStartPosition,
                _cameraStartRotation);

            float firstGateZ = _playerStartPosition.z + GameRules.MinimumGateDistance;
            for (int index = 0; index < gates.Length; index++)
            {
                float gateZ = firstGateZ + (index * GameRules.MinimumGateDistance);
                RunnerColor gateColor = _session.GetNextGateColor();
                gates[index].Activate(gateColor, GetMaterial(gateColor), gateZ);
            }

            _nextGateZ = firstGateZ + (gates.Length * GameRules.MinimumGateDistance);
            SynchronizeViews();
        }

        private void RecycleGate(GateView gate)
        {
            RunnerColor gateColor = _session.GetNextGateColor();
            gate.Activate(gateColor, GetMaterial(gateColor), _nextGateZ);
            _nextGateZ += GameRules.MinimumGateDistance;
        }

        private Material GetMaterial(RunnerColor color)
        {
            return color == RunnerColor.Red ? redMaterial : blueMaterial;
        }

        private void SynchronizeViews()
        {
            playerRenderer.sharedMaterial = GetMaterial(_session.CurrentColor);
            scoreText.text = _session.CurrentScore.ToString();
            readyLabel.SetActive(_session.CurrentState == RunState.Ready);
            gameOverPanel.SetActive(_session.CurrentState == RunState.Dead);
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
            Text hudScore,
            GameObject readyUi,
            GameObject gameOverUi,
            Button restartUi,
            GameplayTapSurface tapSurface,
            GateView[] gatePool)
        {
            configuredSeed = seed;
            player = playerTransform;
            playerRenderer = playerVisual;
            gameplayCamera = sceneCamera;
            redMaterial = red;
            blueMaterial = blue;
            scoreText = hudScore;
            readyLabel = readyUi;
            gameOverPanel = gameOverUi;
            restartButton = restartUi;
            gameplayTapSurface = tapSurface;
            gates = gatePool;
        }
    }
}
