using ColorGateRunner.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    public sealed class SplineTrackLabController : MonoBehaviour
    {
        private const float RunnerHeight = 1f;
        private const float GateHeight = 0f;
        private const float GoalHeight = 0f;
        private const float Speed = 14f;
        private const float FirstGateDistance = 30f;
        private const float GateSpacing = 21f;
        private const float GoalLeadDistance = 22f;
        private static readonly Vector3 CameraLocalOffset =
            new Vector3(0f, 6.2f, -10f);
        private static readonly RunnerColor[] ColorCycle =
        {
            RunnerColor.Red,
            RunnerColor.Blue,
            RunnerColor.Green
        };
        private static readonly RunnerColor[] GateColors =
        {
            RunnerColor.Red,
            RunnerColor.Blue,
            RunnerColor.Green,
            RunnerColor.Blue,
            RunnerColor.Red,
            RunnerColor.Green,
            RunnerColor.Red,
            RunnerColor.Blue,
            RunnerColor.Green,
            RunnerColor.Blue,
            RunnerColor.Red,
            RunnerColor.Green
        };

        [SerializeField] private StageSceneController sceneController;
        [SerializeField] private SplineTrackLabView trackView;
        [SerializeField] private TrackPoolController campaignTrack;
        [SerializeField] private Transform runner;
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private StageGateView[] gates;
        [SerializeField] private GameObject goal;
        [SerializeField] private GateBreakEffectPool gateBreakEffects;
        [SerializeField] private ParticleSystem successParticles;
        [SerializeField] private GameObject hudRoot;
        [SerializeField] private Text titleText;
        [SerializeField] private Text progressText;
        [SerializeField] private Text instructionText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button exitButton;

        private float _distance;
        private int _nextGateIndex;
        private int _nextPlanToActivate;
        private int _colorIndex;
        private bool _active;
        private bool _running;
        private bool _cleared;

        internal bool Active => _active;
        internal bool Running => _active && _running;
        internal bool Cleared => _cleared;
        internal int GatesPassed => _nextGateIndex;
        internal int TotalGateCount => GateColors.Length;
        internal float Distance => _distance;
        internal SplineTrackLabView TrackView => trackView;
        internal GameObject HudRoot => hudRoot;
        internal Button RestartButton => restartButton;
        internal Button ExitButton => exitButton;
        internal RunnerColor CurrentColor => ColorCycle[_colorIndex];
        internal float GoalDistance =>
            FirstGateDistance + ((GateColors.Length - 1) * GateSpacing) +
            GoalLeadDistance;
        internal bool HasRequiredReferences =>
            sceneController != null && trackView != null &&
            trackView.HasRequiredReferences && campaignTrack != null &&
            runner != null && gameplayCamera != null && gates != null &&
            gates.Length >= 5 && goal != null && gateBreakEffects != null &&
            successParticles != null && hudRoot != null && titleText != null &&
            progressText != null && instructionText != null &&
            restartButton != null && exitButton != null;

        internal void Configure(
            StageSceneController controller,
            SplineTrackLabView path,
            TrackPoolController straightTrack,
            Transform player,
            Camera camera,
            StageGateView[] gatePool,
            GameObject goalObject,
            GateBreakEffectPool breakEffects,
            ParticleSystem success,
            GameObject hud,
            Text title,
            Text progress,
            Text instruction,
            Button restart,
            Button exit)
        {
            sceneController = controller;
            trackView = path;
            campaignTrack = straightTrack;
            runner = player;
            gameplayCamera = camera;
            gates = gatePool;
            goal = goalObject;
            gateBreakEffects = breakEffects;
            successParticles = success;
            hudRoot = hud;
            titleText = title;
            progressText = progress;
            instructionText = instruction;
            restartButton = restart;
            exitButton = exit;
        }

        private void Awake()
        {
            restartButton.onClick.AddListener(Restart);
            exitButton.onClick.AddListener(ExitToLobby);
        }

        private void OnDestroy()
        {
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(Restart);
            }
            if (exitButton != null)
            {
                exitButton.onClick.RemoveListener(ExitToLobby);
            }
        }

        internal void Begin()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _active = true;
            campaignTrack.gameObject.SetActive(false);
            trackView.SetVisualsActive(true);
            hudRoot.SetActive(true);
            Restart();
#endif
        }

        internal void Restart()
        {
            if (!_active)
            {
                return;
            }
            _distance = 0f;
            _nextGateIndex = 0;
            _nextPlanToActivate = 0;
            _colorIndex = 0;
            _running = true;
            _cleared = false;
            gateBreakEffects.ResetPool();
            successParticles.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear);
            for (int index = 0; index < gates.Length; index++)
            {
                ActivateNextGate(gates[index]);
            }
            PlaceGoal();
            ApplyRunnerPose();
            ApplyCameraPose();
            sceneController.ApplySplineLabPlayerColor(CurrentColor);
            RefreshHud();
        }

        internal void Exit()
        {
            _active = false;
            _running = false;
            _cleared = false;
            hudRoot.SetActive(false);
            trackView.SetVisualsActive(false);
            campaignTrack.gameObject.SetActive(true);
            for (int index = 0; index < gates.Length; index++)
            {
                gates[index].Deactivate();
                gates[index].transform.SetPositionAndRotation(
                    Vector3.zero,
                    Quaternion.identity);
            }
            goal.SetActive(false);
        }

        internal void ExitToLobby()
        {
            sceneController.ShowLobby();
        }

        internal void Tick(float deltaTime)
        {
            if (!Running)
            {
                return;
            }
            _distance = Mathf.Min(
                GoalDistance,
                _distance + (Speed * Mathf.Max(0f, deltaTime)));
            ApplyRunnerPose();
            ApplyCameraPose();
            ResolveCrossedGate();
            if (_running && _nextGateIndex >= GateColors.Length &&
                _distance >= GoalDistance)
            {
                _running = false;
                _cleared = true;
                instructionText.text = "LAB COMPLETE · RESTART OR EXIT";
                restartButton.gameObject.SetActive(true);
                successParticles.transform.position = goal.transform.position;
                successParticles.Play();
            }
        }

        internal void CycleColor()
        {
            if (!Running)
            {
                return;
            }
            _colorIndex = (_colorIndex + 1) % ColorCycle.Length;
            sceneController.ApplySplineLabPlayerColor(CurrentColor);
            RefreshHud();
        }

        internal GateOutcome ResolveGate(StageGateView gate)
        {
            if (!Running || gate.PlanIndex != _nextGateIndex)
            {
                return GateOutcome.Invulnerable;
            }
            GatePlan plan = gate.ActivePlan;
            if (plan.Color != CurrentColor)
            {
                _running = false;
                gate.ShowFailure(sceneController.SplineLabFailureMaterial);
                instructionText.text = "COLOR MISMATCH · RESTART OR EXIT";
                restartButton.gameObject.SetActive(true);
                RefreshHud();
                return GateOutcome.Mismatched;
            }

            gateBreakEffects.Play(
                gate.transform.position,
                sceneController.GetPresentationMaterial(plan.Color));
            gate.ShowSuccess();
            successParticles.transform.position = gate.transform.position;
            successParticles.Play();
            _nextGateIndex++;
            ActivateNextGate(gate);
            RefreshHud();
            return GateOutcome.Matched;
        }

        private void ResolveCrossedGate()
        {
            if (_nextGateIndex >= GateColors.Length)
            {
                return;
            }
            float gateDistance = GetGateDistance(_nextGateIndex);
            if (_distance < gateDistance)
            {
                return;
            }
            for (int index = 0; index < gates.Length; index++)
            {
                if (gates[index].gameObject.activeSelf &&
                    gates[index].PlanIndex == _nextGateIndex)
                {
                    gates[index].TryResolveCrossing();
                    return;
                }
            }
        }

        private void ActivateNextGate(StageGateView gate)
        {
            if (_nextPlanToActivate >= GateColors.Length)
            {
                gate.Deactivate();
                return;
            }
            int planIndex = _nextPlanToActivate++;
            RunnerColor color = GateColors[planIndex];
            GatePlan plan = new GatePlan(
                planIndex,
                color,
                GateSpacing,
                GateSpacing / Speed,
                1f,
                GatePatternType.Steady,
                0,
                false,
                GateModifier.None);
            gate.Activate(
                plan,
                planIndex,
                sceneController.GetPresentationMaterial(color),
                0f);
            trackView.EvaluatePose(
                GetGateDistance(planIndex),
                GateHeight,
                out Vector3 position,
                out Quaternion rotation);
            gate.transform.SetPositionAndRotation(position, rotation);
        }

        private void PlaceGoal()
        {
            trackView.EvaluatePose(
                GoalDistance,
                GoalHeight,
                out Vector3 position,
                out Quaternion rotation);
            goal.transform.SetPositionAndRotation(position, rotation);
            goal.SetActive(true);
        }

        private void ApplyRunnerPose()
        {
            trackView.EvaluatePose(
                _distance,
                RunnerHeight,
                out Vector3 position,
                out Quaternion rotation);
            runner.SetPositionAndRotation(position, rotation);
        }

        private void ApplyCameraPose()
        {
            trackView.EvaluatePose(
                _distance,
                RunnerHeight,
                out Vector3 position,
                out Quaternion pathRotation);
            Vector3 cameraPosition = position +
                (pathRotation * CameraLocalOffset);
            Quaternion cameraRotation = pathRotation *
                Quaternion.Euler(18f, 0f, 0f);
            gameplayCamera.transform.SetPositionAndRotation(
                cameraPosition,
                cameraRotation);
        }

        private void RefreshHud()
        {
            titleText.text = "SPLINE TRACK LAB";
            progressText.text =
                $"{_nextGateIndex} / {GateColors.Length} · " +
                $"{CurrentColor.ToString().ToUpperInvariant()}";
            if (_running)
            {
                instructionText.text = "TAP TO SWITCH COLOR · HORIZONTAL S-CURVE";
                restartButton.gameObject.SetActive(false);
            }
        }

        private static float GetGateDistance(int gateIndex)
        {
            return FirstGateDistance + (gateIndex * GateSpacing);
        }
    }
}
