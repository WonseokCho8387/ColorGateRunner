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
        [SerializeField] private SpriteRenderer colorEmblem;
        [SerializeField] private TextMesh mechanicMarker;
        [SerializeField] private EchoGateFieldView echoGateField;
        [SerializeField] private FlickerGateFrameView flickerGateFrame;
        [SerializeField] private FlickerGateEmblemView flickerEmblemTransition;

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
        private float _hiddenPowerVisibility = 1f;
        private long _flickerPhaseIndex;
        private float _flickerTransitionPulse;
        private FlickerJudgmentWindow _flickerWindow;
        private MaterialPropertyBlock _visibilityPropertyBlock;

        internal RunnerColor AssignedColor { get; private set; }
        internal GatePlan ActivePlan { get; private set; }
        internal int PlanIndex => _planIndex;
        internal float PathDistance { get; private set; }
        internal bool HasResolved => _resolved;
        internal bool ReactionActive => _reactionRemaining > 0f;
        internal bool BoosterDestroyed => _boosterDestroyed;
        internal int PartCount => gateRenderers == null ? 0 : gateRenderers.Length;
        internal bool HasExperimentPlan => _hasExperimentPlan;
        internal ExperimentGatePlan ActiveExperimentPlan => _experimentPlan;
        internal bool SymbolVisible => colorEmblem.gameObject.activeSelf;
        internal string SymbolText => mechanicMarker.text;
        internal bool MarkerVisible => mechanicMarker.gameObject.activeSelf;
        internal bool EchoProviderVisualActive => _echoProviderVisualActive;
        internal bool EchoFieldVisible =>
            echoGateField != null && echoGateField.IsVisible;
        internal Color EchoFieldColor => echoGateField == null
            ? Color.clear
            : echoGateField.FieldColor;
        internal float SymbolAlpha => colorEmblem.color.a;
        internal float CamouflageRevealProgress =>
            _camouflageRevealProgress;
        internal float HiddenVisibleElapsed =>
            _hiddenVisibility.VisibleElapsed;
        internal bool HiddenHideStarted =>
            _hiddenVisibility.HideStarted;
        internal float HiddenTransitionProgress =>
            _hiddenVisibility.TransitionProgress;
        internal float HiddenTargetAlpha => _hiddenVisibility.TargetAlpha;
        internal float HiddenPowerVisibility => _hiddenPowerVisibility;
        internal int HiddenHideStartCount =>
            _hiddenVisibility.HideStartCount;
        internal long FlickerPhaseIndex => _flickerPhaseIndex;
        internal float FlickerTransitionPulse => _flickerTransitionPulse;
        internal FlickerJudgmentWindow FlickerWindow => _flickerWindow;
        internal bool FlickerFrameTransitioning =>
            flickerGateFrame != null && flickerGateFrame.IsTransitioning;
        internal float FlickerFrameProgress =>
            flickerGateFrame == null ? 0f : flickerGateFrame.Progress;
        internal float FlickerLeftProgress =>
            flickerGateFrame == null ? 0f : flickerGateFrame.LeftProgress;
        internal float FlickerTopProgress =>
            flickerGateFrame == null ? 0f : flickerGateFrame.TopProgress;
        internal float FlickerRightProgress =>
            flickerGateFrame == null ? 0f : flickerGateFrame.RightProgress;
        internal Material FlickerFrameMaterial =>
            flickerGateFrame == null
                ? null
                : flickerGateFrame.TransitionMaterial;
        internal bool FlickerSymbolTransitioning =>
            flickerEmblemTransition != null &&
            flickerEmblemTransition.IsTransitioning;
        internal bool FlickerNextSymbolVisible =>
            flickerEmblemTransition != null &&
            flickerEmblemTransition.NextEmblemVisible;
        internal float FlickerSymbolProgress =>
            flickerEmblemTransition == null
                ? 0f
                : flickerEmblemTransition.Progress;
        internal Sprite FlickerCurrentSymbol =>
            flickerEmblemTransition == null
                ? null
                : flickerEmblemTransition.CurrentSprite;
        internal Sprite FlickerNextSymbol =>
            flickerEmblemTransition == null
                ? null
                : flickerEmblemTransition.NextSprite;
        internal Material DisplayMaterial =>
            gateRenderers != null && gateRenderers.Length > 0
                ? gateRenderers[0].sharedMaterial
                : null;

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
            if (!controller.CanResolveGate())
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
            transform.rotation = Quaternion.identity;
            PathDistance = worldZ;
            _baseScale = Vector3.one;
            transform.localScale = _baseScale;
            ResetParts();
            ApplyMaterial(material);
            ApplyColorEmblem();
            HideMechanicMarker();
            if (plan.Modifier.IsEchoProvider)
            {
                ApplyEchoProviderPresentation();
            }
        }

        internal void ActivateOnPath(
            GatePlan plan,
            int planIndex,
            Material material,
            float pathDistance,
            Vector3 position,
            Quaternion rotation)
        {
            Activate(plan, planIndex, material, pathDistance);
            PathDistance = pathDistance;
            transform.SetPositionAndRotation(position, rotation);
        }

        internal void ApplyTemporaryPlan(
            GatePlan plan,
            Material material)
        {
            ActivePlan = plan;
            AssignedColor = plan.Color;
            _assignedMaterial = material;
            ApplyMaterial(material);
            ApplyColorEmblem();
            HideMechanicMarker();
            ResetEchoProviderPresentation();
            if (plan.Modifier.IsEchoProvider)
            {
                ApplyEchoProviderPresentation();
            }
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
                ApplyHiddenEmblem(0f, 1f);
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
            UpdateCampaignVisibility(
                passedGateCount,
                estimatedArrivalSeconds,
                neutralMaterial,
                camouflageSettings,
                HiddenSettings.Disabled(),
                FlickerSettings.Disabled(),
                0f,
                deltaSeconds);
        }

        internal void UpdateCampaignVisibility(
            int passedGateCount,
            float estimatedArrivalSeconds,
            Material neutralMaterial,
            CamouflageSettings camouflageSettings,
            HiddenSettings hiddenSettings,
            FlickerSettings flickerSettings,
            float gameplayTimeSeconds,
            float deltaSeconds)
        {
            UpdateModifierVisibility(
                ActivePlan.Modifier.IsCamouflage,
                true,
                ActivePlan.Modifier.IsEchoProvider,
                ActivePlan.Modifier.IsHidden,
                ActivePlan.Modifier.IsFlicker,
                estimatedArrivalSeconds,
                neutralMaterial,
                camouflageSettings,
                hiddenSettings,
                flickerSettings,
                gameplayTimeSeconds,
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
                SetEchoFieldPresentation(false, 0f);
                UpdateHiddenVisibility(
                    estimatedArrivalSeconds,
                    neutralMaterial,
                    hiddenSettings,
                    deltaSeconds);
                return;
            }
            if (isFlicker)
            {
                SetEchoFieldPresentation(false, 0f);
                ApplyFlickerRuntimePresentation();
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
                colorEmblem.gameObject.SetActive(false);
                mechanicMarker.gameObject.SetActive(isEchoProvider);
                if (isEchoProvider)
                {
                    mechanicMarker.text = "ECHO";
                    mechanicMarker.color = Color.white;
                }
                SetEchoFieldPresentation(false, 0f);
            }
            else
            {
                ApplyColorEmblem();
                HideMechanicMarker();
                if (isEchoProvider)
                {
                    ApplyEchoProviderPresentation();
                }
                if (isCamouflage)
                {
                    SetEmblemAlpha(_camouflageRevealProgress);
                }
                SetEchoFieldPresentation(
                    isEchoProvider,
                    isCamouflage ? _camouflageRevealProgress : 1f);
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
                _hiddenPowerVisibility = HiddenPowerDownEnvelope.Evaluate(
                    _hiddenVisibility.TransitionProgress);
                ApplyHiddenPowerDown(
                    neutralMaterial,
                    _hiddenPowerVisibility);
            }
            else
            {
                _hiddenPowerVisibility = 1f;
                ApplyMaterial(_assignedMaterial);
            }
            ApplyHiddenEmblem(
                _hiddenVisibility.TransitionProgress,
                _hiddenPowerVisibility);
        }

        private void ApplyHiddenEmblem(
            float hideProgress,
            float visibility)
        {
            float progress = Mathf.Clamp01(hideProgress);
            ApplyColorEmblem(Mathf.Clamp01(visibility));
            colorEmblem.transform.localScale =
                Vector3.one * Mathf.Lerp(1f, 0.72f, progress);
            HideMechanicMarker();
        }

        internal void ApplyFlickerRuntimeState(
            FlickerJudgmentWindow window)
        {
            _flickerWindow = window;
            _flickerPhaseIndex = window.PhaseIndex;
            _flickerTransitionPulse = window.IsTransitioning
                ? 1f - window.TransitionProgress
                : 0f;
            ApplyFlickerRuntimePresentation();
        }

        private void ApplyFlickerRuntimePresentation()
        {
            AssignedColor = _flickerWindow.CurrentColor;
            _assignedMaterial =
                controller.GetPresentationMaterial(AssignedColor);
            Material nextMaterial = controller.GetPresentationMaterial(
                _flickerWindow.NextColor);
            if (_flickerWindow.IsTransitioning &&
                flickerGateFrame != null)
            {
                flickerGateFrame.SetTransition(
                    _assignedMaterial,
                    nextMaterial,
                    _flickerWindow.TransitionProgress);
            }
            else
            {
                flickerGateFrame?.ResetPresentation();
                ApplyMaterial(_assignedMaterial);
            }
            ApplyFlickerPresentation(GetMaterialColor(nextMaterial));
        }

        private void ApplyFlickerPresentation(Color nextColor)
        {
            if (_flickerWindow.IsTransitioning &&
                flickerEmblemTransition != null)
            {
                flickerEmblemTransition.SetTransition(
                    controller.GetColorEmblemSprite(
                        _flickerWindow.CurrentColor),
                    controller.GetColorEmblemSprite(
                        _flickerWindow.NextColor),
                    nextColor,
                    _flickerWindow.TransitionProgress);
            }
            else
            {
                ApplyColorEmblem();
            }
            HideMechanicMarker();
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
            _hiddenPowerVisibility = 1f;
        }

        private void ResetFlickerState()
        {
            _flickerPhaseIndex = 0;
            _flickerTransitionPulse = 0f;
            _flickerWindow = new FlickerJudgmentWindow(
                AssignedColor,
                AssignedColor,
                false,
                0f,
                0,
                false);
            flickerGateFrame?.ResetPresentation();
            if (flickerEmblemTransition != null && colorEmblem != null)
            {
                flickerEmblemTransition.SetStatic(
                    colorEmblem.sprite,
                    colorEmblem.color.a);
            }
        }

        internal Transform GetPartTransform(int index)
        {
            return gateRenderers[index].transform;
        }

        internal void ShowSuccess()
        {
            _reactionRemaining = ReactionDuration;
            SetEchoFieldPresentation(false, 0f);
        }

        internal void ShowFailure(Material failureMaterial)
        {
            _reactionRemaining = ReactionDuration;
            ApplyMaterial(failureMaterial);
            SetEchoFieldPresentation(false, 0f);
        }

        internal void ShowBoosterImpact(Material flashMaterial)
        {
            _boosterDestroyed = true;
            _reactionRemaining = ReactionDuration;
            ApplyMaterial(flashMaterial);
            SetEchoFieldPresentation(false, 0f);
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
            PathDistance = 0f;
            _hasExperimentPlan = false;
            _experimentWasHidden = false;
            _camouflageRevealStarted = false;
            _camouflageRevealProgress = 0f;
            ResetHiddenState();
            ResetFlickerState();
            ResetEchoProviderPresentation();
            transform.localScale = Vector3.one;
            ResetParts();
            colorEmblem.gameObject.SetActive(true);
            colorEmblem.transform.localScale = Vector3.one;
            HideMechanicMarker();
            gameObject.SetActive(false);
        }

        internal bool HasRequiredReferences()
        {
            if (controller == null || colorEmblem == null ||
                mechanicMarker == null ||
                echoGateField == null ||
                !echoGateField.HasRequiredReferences ||
                flickerGateFrame == null ||
                !flickerGateFrame.HasRequiredReferences ||
                flickerEmblemTransition == null ||
                !flickerEmblemTransition.HasRequiredReferences ||
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
            SpriteRenderer emblem,
            TextMesh marker,
            EchoGateFieldView field,
            FlickerGateFrameView frame,
            FlickerGateEmblemView emblemTransition)
        {
            controller = sceneController;
            gateRenderers = renderers;
            colorEmblem = emblem;
            mechanicMarker = marker;
            echoGateField = field;
            flickerGateFrame = frame;
            flickerEmblemTransition = emblemTransition;
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

        private void ApplyHiddenPowerDown(
            Material neutralMaterial,
            float visibility)
        {
            float clampedVisibility = Mathf.Clamp01(visibility);
            if (clampedVisibility <= 0f)
            {
                ApplyMaterial(neutralMaterial);
                return;
            }
            if (_visibilityPropertyBlock == null)
            {
                _visibilityPropertyBlock = new MaterialPropertyBlock();
            }

            Color neutral = GetMaterialColor(neutralMaterial);
            Color target = GetMaterialColor(_assignedMaterial);
            Color neutralEmission = GetMaterialPropertyColor(
                neutralMaterial,
                "_EmissionColor",
                Color.black);
            Color targetEmission = GetMaterialPropertyColor(
                _assignedMaterial,
                "_EmissionColor",
                Color.black);
            Color blended = Color.Lerp(
                neutral,
                target,
                clampedVisibility);
            Color blendedEmission = Color.Lerp(
                neutralEmission,
                targetEmission,
                clampedVisibility);
            _visibilityPropertyBlock.Clear();
            _visibilityPropertyBlock.SetColor("_BaseColor", blended);
            _visibilityPropertyBlock.SetColor("_Color", blended);
            _visibilityPropertyBlock.SetColor(
                "_EmissionColor",
                blendedEmission);
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

        private static Color GetMaterialPropertyColor(
            Material material,
            string propertyName,
            Color fallback)
        {
            if (material != null && material.HasProperty(propertyName))
            {
                return material.GetColor(propertyName);
            }
            return fallback;
        }

        private void ApplyColorEmblem(float alpha = 1f)
        {
            Sprite sprite = controller.GetColorEmblemSprite(AssignedColor);
            colorEmblem.sprite = sprite;
            if (flickerEmblemTransition != null)
            {
                flickerEmblemTransition.SetStatic(sprite, alpha);
            }
            else
            {
                colorEmblem.gameObject.SetActive(alpha > 0f);
                SetEmblemAlpha(alpha);
            }
            colorEmblem.transform.localScale = Vector3.one;
        }

        private void ApplyEchoProviderPresentation()
        {
            _echoProviderVisualActive = true;
            mechanicMarker.gameObject.SetActive(true);
            mechanicMarker.text = "ECHO";
            mechanicMarker.color = Color.white;
            SetEchoFieldPresentation(true, 1f);
        }

        private void ResetEchoProviderPresentation()
        {
            _echoProviderVisualActive = false;
            if (mechanicMarker != null)
            {
                HideMechanicMarker();
            }
            if (echoGateField != null)
            {
                echoGateField.SetPresentation(
                    false,
                    Color.white,
                    0f);
            }
        }

        private void SetEchoFieldPresentation(
            bool visible,
            float alpha)
        {
            if (echoGateField == null)
            {
                return;
            }
            echoGateField.SetPresentation(
                visible && _echoProviderVisualActive && !_resolved,
                GetMaterialColor(_assignedMaterial),
                alpha);
        }

        private void SetEmblemAlpha(float alpha)
        {
            Color color = Color.white;
            color.a = Mathf.Clamp01(alpha);
            colorEmblem.color = color;
        }

        private void HideMechanicMarker()
        {
            mechanicMarker.text = string.Empty;
            mechanicMarker.color = Color.white;
            mechanicMarker.gameObject.SetActive(false);
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
