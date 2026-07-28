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

        internal RunnerColor AssignedColor { get; private set; }
        internal bool HasResolved => _resolved;
        internal bool ReactionActive => _reactionRemaining > 0f;

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
            RunnerColor color,
            Material material,
            float worldZ)
        {
            AssignedColor = color;
            _assignedMaterial = material;
            _resolved = false;
            _reactionRemaining = 0f;
            gameObject.SetActive(true);
            Vector3 position = transform.position;
            position.z = worldZ;
            transform.position = position;
            _baseScale = Vector3.one;
            transform.localScale = _baseScale;
            ApplyMaterial(material);
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
            transform.localScale = Vector3.one;
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
        }

        private void ApplyMaterial(Material material)
        {
            for (int index = 0; index < gateRenderers.Length; index++)
            {
                gateRenderers[index].sharedMaterial = material;
            }
        }
    }
}
