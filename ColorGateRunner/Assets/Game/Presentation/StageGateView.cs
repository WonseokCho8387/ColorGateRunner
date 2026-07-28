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

        private bool _resolved;
        private float _reactionRemaining;
        private Vector3 _baseScale;
        private Material _assignedMaterial;
        private Vector3[] _partPositions;
        private Quaternion[] _partRotations;
        private Vector3[] _partScales;
        private bool _boosterDestroyed;
        private int _planIndex = -1;

        internal RunnerColor AssignedColor { get; private set; }
        internal GatePlan ActivePlan { get; private set; }
        internal int PlanIndex => _planIndex;
        internal bool HasResolved => _resolved;
        internal bool ReactionActive => _reactionRemaining > 0f;
        internal bool BoosterDestroyed => _boosterDestroyed;
        internal int PartCount => gateRenderers == null ? 0 : gateRenderers.Length;

        private void Awake()
        {
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
            gameObject.SetActive(true);
            Vector3 position = transform.position;
            position.z = worldZ;
            transform.position = position;
            _baseScale = Vector3.one;
            transform.localScale = _baseScale;
            ResetParts();
            ApplyMaterial(material);
        }

        internal void ApplyTemporaryPlan(
            GatePlan plan,
            Material material)
        {
            ActivePlan = plan;
            AssignedColor = plan.Color;
            _assignedMaterial = material;
            ApplyMaterial(material);
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
            transform.localScale = Vector3.one;
            ResetParts();
            gameObject.SetActive(false);
        }

        internal bool HasRequiredReferences()
        {
            if (controller == null || gateRenderers == null || gateRenderers.Length == 0)
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
            Renderer[] renderers)
        {
            controller = sceneController;
            gateRenderers = renderers;
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
