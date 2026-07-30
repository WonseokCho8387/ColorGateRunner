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
        private bool _cloneVisualActive;
        private MaterialPropertyBlock _clonePropertyBlock;

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
        internal bool CloneVisualActive => _cloneVisualActive;
        internal float SymbolAlpha => colorSymbol.color.a;

        private void Awake()
        {
            _clonePropertyBlock = new MaterialPropertyBlock();
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
            ResetClonePresentation();
            gameObject.SetActive(true);
            Vector3 position = transform.position;
            position.z = worldZ;
            transform.position = position;
            _baseScale = Vector3.one;
            transform.localScale = _baseScale;
            ResetParts();
            ApplyMaterial(material);
            ApplyColorSymbol();
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
            int passedGateCount)
        {
            _experimentPlan = plan;
            _hasExperimentPlan = true;
            _experimentWasHidden = false;
            Activate(
                new GatePlan(
                    plan.Color,
                    plan.Spacing,
                    plan.Cadence,
                    1f,
                    GatePatternType.Steady,
                    plan.GateIndex,
                    false),
                plan.GateIndex,
                colorMaterial,
                worldZ);
            if (plan.IsClone)
            {
                ApplyClonePresentation();
            }
            UpdateExperimentVisibility(passedGateCount, neutralMaterial);
        }

        internal void UpdateExperimentVisibility(
            int passedGateCount,
            Material neutralMaterial)
        {
            if (!_hasExperimentPlan)
            {
                return;
            }
            bool camouflageHidden =
                !_experimentPlan.IsCamouflageRevealed(passedGateCount);
            bool fogObscured =
                !_experimentPlan.IsFullyVisibleInFog(passedGateCount);
            bool hidden = camouflageHidden || fogObscured;
            ApplyMaterial(hidden ? neutralMaterial : _assignedMaterial);
            colorSymbol.gameObject.SetActive(!hidden);
            if (_experimentWasHidden && !hidden)
            {
                _reactionRemaining = ReactionDuration;
            }
            _experimentWasHidden = hidden;
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
            ResetClonePresentation();
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
                gateRenderers[index].sharedMaterial = material;
            }
        }

        private void ApplyColorSymbol()
        {
            RunnerColorSymbol symbol = MobileUiPolicy.GetSymbol(AssignedColor);
            colorSymbol.text = GetSymbolText(symbol);
            colorSymbol.color = Color.white;
        }

        private void ApplyClonePresentation()
        {
            _cloneVisualActive = true;
            if (_clonePropertyBlock == null)
            {
                _clonePropertyBlock = new MaterialPropertyBlock();
            }
            _clonePropertyBlock.Clear();
            Color echoColor = GetAssignedColor();
            echoColor.r *= 0.68f;
            echoColor.g *= 0.68f;
            echoColor.b *= 0.68f;
            echoColor.a = 0.62f;
            _clonePropertyBlock.SetColor("_BaseColor", echoColor);
            _clonePropertyBlock.SetColor("_Color", echoColor);
            for (int index = 0; index < gateRenderers.Length; index++)
            {
                gateRenderers[index].SetPropertyBlock(_clonePropertyBlock);
            }
            colorSymbol.text = "ECHO\n" + colorSymbol.text;
            colorSymbol.color = new Color(1f, 1f, 1f, 0.72f);
        }

        private void ResetClonePresentation()
        {
            _cloneVisualActive = false;
            if (gateRenderers != null)
            {
                for (int index = 0; index < gateRenderers.Length; index++)
                {
                    gateRenderers[index].SetPropertyBlock(null);
                }
            }
            if (colorSymbol != null)
            {
                colorSymbol.color = Color.white;
            }
        }

        private Color GetAssignedColor()
        {
            if (_assignedMaterial == null)
            {
                return Color.white;
            }
            if (_assignedMaterial.HasProperty("_BaseColor"))
            {
                return _assignedMaterial.GetColor("_BaseColor");
            }
            if (_assignedMaterial.HasProperty("_Color"))
            {
                return _assignedMaterial.GetColor("_Color");
            }
            return Color.white;
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
