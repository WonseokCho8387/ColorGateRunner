using ColorGateRunner.Core;
using UnityEngine;

namespace ColorGateRunner.Presentation
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class StageGateView : MonoBehaviour
    {
        private const float ReactionDuration = 0.2f;

        [SerializeField] private StageSceneController controller;
        [SerializeField] private Renderer[] gateRenderers;
        [SerializeField] private TextMesh colorSymbol;

        private bool _resolved;
        private float _reactionRemaining;
        private Vector3 _baseScale;
        private Material _assignedMaterial;
        private Vector3[] _partPositions;
        private Quaternion[] _partRotations;
        private Vector3[] _partScales;
        private bool _boosterDestroyed;
        private int _planIndex = -1;
        private ExperimentGatePlan _experimentPlan;
        private bool _hasExperimentPlan;
        private bool _experimentWasHidden;
        private bool _echoProviderVisualActive;
        private bool _camouflageRevealStarted;
        private float _camouflageRevealProgress;
        private HiddenVisibilityState _hiddenVisibility;
        private long _flickerPhaseIndex;
        private float _flickerTransitionPulse;
        private MaterialPropertyBlock _visibilityPropertyBlock;

        internal RunnerColor AssignedColor { get; private set; }
        internal GatePlan ActivePlan { get; private set; }
        internal int PlanIndex => _planIndex;
        internal bool HasResolved => _resolved;
        internal bool ReactionActive => _reactionRemaining > 0f;
        internal bool BoosterDestroyed => _boosterDestroyed;
        internal int PartCount => gateRenderers == null ? 0 : gateRenderers.Length;
        internal bool HasExperimentPlan => _hasExperimentPlan;
        internal ExperimentGatePlan ActiveExperimentPlan => _experimentPlan;
        internal bool SymbolVisible => colorSymbol.gameObject.activeSelf;
        internal string SymbolText => colorSymbol.text;
        internal bool EchoProviderVisualActive => _echoProviderVisualActive;
        internal float SymbolAlpha => colorSymbol.color.a;
        internal float CamouflageRevealProgress =>
            _camouflageRevealProgress;
        internal float HiddenVisibleElapsed =>
            _hiddenVisibility.VisibleElapsed;
        internal bool HiddenHideStarted =>
            _hiddenVisibility.HideStarted;
        internal float HiddenTransitionProgress =>
            _hiddenVisibility.TransitionProgress;
        internal float HiddenTargetAlpha => _hiddenVisibility.TargetAlpha;
        internal int HiddenHideStartCount =>
            _hiddenVisibility.HideStartCount;
        internal long FlickerPhaseIndex => _flickerPhaseIndex;
        internal float FlickerTransitionPulse => _flickerTransitionPulse;

        private void Awake()
        {
            _visibilityPropertyBlock = new MaterialPropertyBlock();
            _hiddenVisibility = new HiddenVisibilityState();
            CaptureParts();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (controller.IsPlayerCollider(other))
            {
                TryResolveCrossing();
            }
        }

        internal bool TryResolveCrossing()
        {
            if (_resolved || !gameObject.activeSelf)
            {
                return false;
            }
            if (_hasExperimentPlan &&
                !controller.CanResolveExperimentGate())
            {
                return false;
            }

            _resolved = true;
            controller.HandleGateCrossed(this);
            return true;
        }

        internal void Activate(
            GatePlan plan,
            int planIndex,
            Material material,
            float worldZ)
        {
            ActivePlan = plan;
            _planIndex = planIndex;
            AssignedColor = plan.Color;
            _assignedMaterial = material;
            _resolved = false;
            _reactionRemaining = 0f;
            _boosterDestroyed = false;
            ResetEchoProviderPresentation();
            _experimentWasHidden = false;
            _camouflageRevealStarted = !plan.Modifier.IsCamouflage;
            _camouflageRevealProgress =
                plan.Modifier.IsCamouflage ? 0f : 1f;
            ResetHiddenState();
            ResetFlickerState();
            gameObject.SetActive(true);
            Vector3 position = transform.position;
            position.z = worldZ;
            transform.position = position;
            _baseScale = Vector3.one;
            transform.localScale = _baseScale;
            ResetParts();
            ApplyMaterial(material);
            ApplyColorSymbol();
            if (plan.Modifier.IsEchoProvider)
            {
                ApplyEchoProviderPresentation();
            }
        }

        internal void ApplyTemporaryPlan(
            GatePlan plan,
            Material material)
        {
            ActivePlan = plan;
            AssignedColor = plan.Color;
            _assignedMaterial = material;
            ApplyMaterial(material);
            ApplyColorSymbol();
        }

        internal void ActivateExperiment(
            ExperimentGatePlan plan,
            Material colorMaterial,
            Material neutralMaterial,
            float worldZ,
            int passedGateCount,
            float estimatedArrivalSeconds,
            CamouflageSettings camouflageSettings,
            HiddenSettings hiddenSettings,
            FlickerSettings flickerSettings,
            float gameplayTimeSeconds)
        {
            _experimentPlan = plan;
            _hasExperimentPlan = true;
            _experimentWasHidden = false;
            _camouflageRevealStarted = !plan.IsCamouflage;
            _camouflageRevealProgress = plan.IsCamouflage ? 0f : 1f;
            ResetHiddenState();
            ResetFlickerState();
            Activate(
                new GatePlan(
                    plan.GateId,
                    plan.Color,
                    plan.Spacing,
                    plan.Cadence,
                    1f,
                    GatePatternType.Steady,
                    plan.GateIndex,
                    false,
                    plan.Modifier),
                plan.GateIndex,
                colorMaterial,
                worldZ);
            if (plan.IsEchoProvider)
            {
                ApplyEchoProviderPresentation();
            }
            if (plan.IsHidden)
            {
                ApplyMaterial(_assignedMaterial);
                ApplyHiddenSymbol(0f);
                return;
            }
            UpdateExperimentVisibility(
                passedGateCount,
                estimatedArrivalSeconds,
                neutralMaterial,
                camouflageSettings,
                hiddenSettings,
                flickerSettings,
                gameplayTimeSeconds,
                0f);
        }

        internal void UpdateExperimentVisibility(
            int passedGateCount,
            float estimatedArrivalSeconds,
            Material neutralMaterial,
            CamouflageSettings camouflageSettings,
            HiddenSettings hiddenSettings,
            FlickerSettings flickerSettings,
            float gameplayTimeSeconds,
            float deltaSeconds)
        {
            if (!_hasExperimentPlan)
            {
                return;
            }
            UpdateModifierVisibility(
                _experimentPlan.IsCamouflage,
                _experimentPlan.IsFullyVisibleInFog(passedGateCount),
                _experimentPlan.IsEchoProvider,
                _experimentPlan.IsHidden,
                _experimentPlan.IsFlicker,
                estimatedArrivalSeconds,
                neutralMaterial,
                camouflageSettings,
                hiddenSettings,
                flickerSettings,
                gameplayTimeSeconds,
                deltaSeconds);
        }

        internal void UpdateCampaignVisibility(
            int passedGateCount,
            float estimatedArrivalSeconds,
            Material neutralMaterial,
            CamouflageSettings camouflageSettings,
            float deltaSeconds)
        {
            bool fogVisible =
                !ActivePlan.Modifier.IsFog ||
                _planIndex - passedGateCount < 2;
            UpdateModifierVisibility(
                ActivePlan.Modifier.IsCamouflage,
                fogVisible,
                ActivePlan.Modifier.IsEchoProvider,
                false,
                false,
                estimatedArrivalSeconds,
                neutralMaterial,
                camouflageSettings,
                null,
                null,
                0f,
                deltaSeconds);
        }

        private void UpdateModifierVisibility(
            bool isCamouflage,
            bool fogVisible,
            bool isEchoProvider,
            bool isHidden,
            bool isFlicker,
            float estimatedArrivalSeconds,
            Material neutralMaterial,
            CamouflageSettings camouflageSettings,
            HiddenSettings hiddenSettings,
            FlickerSettings flickerSettings,
            float gameplayTimeSeconds,
            float deltaSeconds)
        {
            if (isHidden)
            {
                UpdateHiddenVisibility(
                    estimatedArrivalSeconds,
                    neutralMaterial,
                    hiddenSettings,
                    deltaSeconds);
                return;
            }
            if (isFlicker)
            {
                UpdateFlickerCycle(gameplayTimeSeconds);
                return;
            }

            if (isCamouflage &&
                !_camouflageRevealStarted &&
                GateEtaEstimator.ShouldStartReveal(
                    estimatedArrivalSeconds,
                    camouflageSettings))
            {
                _camouflageRevealStarted = true;
            }
            if (_camouflageRevealStarted &&
                _camouflageRevealProgress < 1f)
            {
                _camouflageRevealProgress =
                    camouflageSettings.RevealTransitionSeconds <= 0f
                        ? 1f
                        : Mathf.Min(
                            1f,
                            _camouflageRevealProgress +
                            (Mathf.Max(0f, deltaSeconds) /
                             camouflageSettings.RevealTransitionSeconds));
            }
            bool camouflageHidden =
                isCamouflage &&
                !_camouflageRevealStarted;
            bool fogObscured = !fogVisible;
            bool hidden = camouflageHidden || fogObscured;
            if (hidden)
            {
                ApplyMaterial(neutralMaterial);
            }
            else if (isCamouflage &&
                _camouflageRevealProgress < 1f)
            {
                ApplyRevealBlend(
                    neutralMaterial,
                    _camouflageRevealProgress);
            }
            else
            {
                ApplyMaterial(_assignedMaterial);
            }
            if (hidden)
            {
                colorSymbol.gameObject.SetActive(
                    isEchoProvider);
                if (isEchoProvider)
                {
                    colorSymbol.text = "ECHO";
                    colorSymbol.color = Color.white;
                }
            }
            else
            {
                colorSymbol.gameObject.SetActive(true);
                ApplyColorSymbol();
                if (isEchoProvider)
                {
                    ApplyEchoProviderPresentation();
                }
                if (isCamouflage)
                {
                    Color symbolColor = colorSymbol.color;
                    symbolColor.a = _camouflageRevealProgress;
                    colorSymbol.color = symbolColor;
                }
            }
            if (_experimentWasHidden && !hidden)
            {
                _reactionRemaining = ReactionDuration;
            }
            _experimentWasHidden = hidden;
        }

        private void UpdateHiddenVisibility(
            float estimatedArrivalSeconds,
            Material neutralMaterial,
            HiddenSettings settings,
            float deltaSeconds)
        {
            if (settings == null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            _hiddenVisibility.Advance(
                Mathf.Max(0f, deltaSeconds),
                estimatedArrivalSeconds,
                settings);
            if (_hiddenVisibility.HideStarted)
            {
                ApplyRevealBlend(
                    neutralMaterial,
                    _hiddenVisibility.TargetAlpha);
            }
            else
            {
                ApplyMaterial(_assignedMaterial);
            }
            ApplyHiddenSymbol(
                _hiddenVisibility.TransitionProgress);
        }

        private void ApplyHiddenSymbol(float hideProgress)
        {
            colorSymbol.richText = true;
            colorSymbol.gameObject.SetActive(true);
            colorSymbol.color = Color.white;
            int alpha = Mathf.RoundToInt(
                255f * (1f - Mathf.Clamp01(hideProgress)));
            string targetSymbol = GetSymbolText(
                MobileUiPolicy.GetSymbol(AssignedColor));
            colorSymbol.text =
                "HIDDEN\n<color=#FFFFFF" +
                alpha.ToString("X2") +
                ">" +
                targetSymbol +
                "</color>";
        }

        private void UpdateFlickerCycle(float gameplayTimeSeconds)
        {
            FlickerCycleSample sample =
                _experimentPlan.GetFlickerSample(gameplayTimeSeconds);
            RunnerColor activeColor = _experimentPlan.GetCycleColor(
                sample.CycleColorIndex);
            _flickerPhaseIndex = sample.PhaseIndex;
            _flickerTransitionPulse = sample.TransitionPulse;
            AssignedColor = activeColor;
            _assignedMaterial =
                controller.GetPresentationMaterial(activeColor);
            ApplyMaterial(_assignedMaterial);
            ApplyFlickerPulse(sample.TransitionPulse);
            ApplyFlickerSymbol();
        }

        private void ApplyFlickerSymbol()
        {
            colorSymbol.richText = true;
            colorSymbol.gameObject.SetActive(true);
            colorSymbol.color = Color.white;
            colorSymbol.text =
                "FLICKER\n" +
                GetSymbolText(MobileUiPolicy.GetSymbol(AssignedColor));
        }

        private void ApplyFlickerPulse(float pulse)
        {
            if (pulse <= 0f)
            {
                return;
            }
            if (_visibilityPropertyBlock == null)
            {
                _visibilityPropertyBlock = new MaterialPropertyBlock();
            }
            Color baseColor = GetMaterialColor(_assignedMaterial);
            Color pulsed = Color.Lerp(
                baseColor,
                Color.white,
                Mathf.Clamp01(pulse) * 0.35f);
            _visibilityPropertyBlock.Clear();
            _visibilityPropertyBlock.SetColor("_BaseColor", pulsed);
            _visibilityPropertyBlock.SetColor("_Color", pulsed);
            for (int index = 0; index < gateRenderers.Length; index++)
            {
                gateRenderers[index].SetPropertyBlock(
                    _visibilityPropertyBlock);
            }
        }

        private void ResetHiddenState()
        {
            if (_hiddenVisibility == null)
            {
                _hiddenVisibility = new HiddenVisibilityState();
            }
            _hiddenVisibility.Reset();
        }

        private void ResetFlickerState()
        {
            _flickerPhaseIndex = 0;
            _flickerTransitionPulse = 0f;
        }

        internal Transform GetPartTransform(int index)
        {
            return gateRenderers[index].transform;
        }

        internal void ShowSuccess()
        {
            _reactionRemaining = ReactionDuration;
        }

        internal void ShowFailure(Material failureMaterial)
        {
            _reactionRemaining = ReactionDuration;
            ApplyMaterial(failureMaterial);
        }

        internal void ShowBoosterImpact(Material flashMaterial)
        {
            _boosterDestroyed = true;
            _reactionRemaining = ReactionDuration;
            ApplyMaterial(flashMaterial);
            if (gateRenderers.Length >= 3)
            {
                gateRenderers[0].transform.localPosition +=
                    new Vector3(-1.1f, 0.35f, 0f);
                gateRenderers[0].transform.localRotation =
                    Quaternion.Euler(0f, 0f, 22f);
                gateRenderers[1].transform.localPosition +=
                    new Vector3(1.1f, 0.35f, 0f);
                gateRenderers[1].transform.localRotation =
                    Quaternion.Euler(0f, 0f, -22f);
                gateRenderers[2].transform.localPosition +=
                    new Vector3(0f, 1f, 0.5f);
                gateRenderers[2].transform.localRotation =
                    Quaternion.Euler(25f, 0f, 12f);
            }
        }

        internal void Tick(float deltaTime)
        {
            if (_reactionRemaining <= 0f)
            {
                return;
            }

            _reactionRemaining = Mathf.Max(0f, _reactionRemaining - deltaTime);
            float phase = _reactionRemaining / ReactionDuration;
            transform.localScale = _baseScale * (1f + (0.15f * phase));
            if (_reactionRemaining <= 0f)
            {
                transform.localScale = _baseScale;
                ApplyMaterial(_assignedMaterial);
            }
        }

        internal void Deactivate()
        {
            _resolved = false;
            _reactionRemaining = 0f;
            _boosterDestroyed = false;
            ActivePlan = default;
            _planIndex = -1;
            _hasExperimentPlan = false;
            _experimentWasHidden = false;
            _camouflageRevealStarted = false;
            _camouflageRevealProgress = 0f;
            ResetHiddenState();
            ResetFlickerState();
            ResetEchoProviderPresentation();
            transform.localScale = Vector3.one;
            ResetParts();
            colorSymbol.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }

        internal bool HasRequiredReferences()
        {
            if (controller == null || colorSymbol == null ||
                gateRenderers == null || gateRenderers.Length == 0)
            {
                return false;
            }

            for (int index = 0; index < gateRenderers.Length; index++)
            {
                if (gateRenderers[index] == null)
                {
                    return false;
                }
            }

            BoxCollider trigger = GetComponent<BoxCollider>();
            return trigger != null && trigger.isTrigger;
        }

        internal void Configure(
            StageSceneController sceneController,
            Renderer[] renderers,
            TextMesh symbol)
        {
            controller = sceneController;
            gateRenderers = renderers;
            colorSymbol = symbol;
            CaptureParts();
        }

        private void CaptureParts()
        {
            if (gateRenderers == null)
            {
                return;
            }
            _partPositions = new Vector3[gateRenderers.Length];
            _partRotations = new Quaternion[gateRenderers.Length];
            _partScales = new Vector3[gateRenderers.Length];
            for (int index = 0; index < gateRenderers.Length; index++)
            {
                Transform part = gateRenderers[index].transform;
                _partPositions[index] = part.localPosition;
                _partRotations[index] = part.localRotation;
                _partScales[index] = part.localScale;
            }
        }

        private void ApplyMaterial(Material material)
        {
            for (int index = 0; index < gateRenderers.Length; index++)
            {
                gateRenderers[index].SetPropertyBlock(null);
                gateRenderers[index].sharedMaterial = material;
            }
        }

        private void ApplyRevealBlend(
            Material neutralMaterial,
            float progress)
        {
            if (_visibilityPropertyBlock == null)
            {
                _visibilityPropertyBlock = new MaterialPropertyBlock();
            }
            Color neutral = GetMaterialColor(neutralMaterial);
            Color target = GetMaterialColor(_assignedMaterial);
            Color blended = Color.Lerp(neutral, target, progress);
            _visibilityPropertyBlock.Clear();
            _visibilityPropertyBlock.SetColor("_BaseColor", blended);
            _visibilityPropertyBlock.SetColor("_Color", blended);
            for (int index = 0; index < gateRenderers.Length; index++)
            {
                gateRenderers[index].sharedMaterial = _assignedMaterial;
                gateRenderers[index].SetPropertyBlock(
                    _visibilityPropertyBlock);
            }
        }

        private static Color GetMaterialColor(Material material)
        {
            if (material == null)
            {
                return Color.white;
            }
            if (material.HasProperty("_BaseColor"))
            {
                return material.GetColor("_BaseColor");
            }
            if (material.HasProperty("_Color"))
            {
                return material.GetColor("_Color");
            }
            return Color.white;
        }

        private void ApplyColorSymbol()
        {
            RunnerColorSymbol symbol = MobileUiPolicy.GetSymbol(AssignedColor);
            colorSymbol.text = GetSymbolText(symbol);
            colorSymbol.color = Color.white;
        }

        private void ApplyEchoProviderPresentation()
        {
            _echoProviderVisualActive = true;
            colorSymbol.text = "ECHO\n" + colorSymbol.text;
            colorSymbol.color = Color.white;
        }

        private void ResetEchoProviderPresentation()
        {
            _echoProviderVisualActive = false;
            if (colorSymbol != null)
            {
                colorSymbol.color = Color.white;
            }
        }

        private static string GetSymbolText(RunnerColorSymbol symbol)
        {
            switch (symbol)
            {
                case RunnerColorSymbol.Circle:
                    return "●";
                case RunnerColorSymbol.Square:
                    return "■";
                case RunnerColorSymbol.Triangle:
                    return "▲";
                case RunnerColorSymbol.Star:
                    return "★";
                case RunnerColorSymbol.Diamond:
                    return "◆";
                default:
                    return "HEX";
            }
        }

        private void ResetParts()
        {
            if (_partPositions == null)
            {
                return;
            }

            for (int index = 0; index < gateRenderers.Length; index++)
            {
                Transform part = gateRenderers[index].transform;
                part.localPosition = _partPositions[index];
                part.localRotation = _partRotations[index];
                part.localScale = _partScales[index];
            }
        }
    }
}
