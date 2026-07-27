using ColorGateRunner.Core;
using UnityEngine;

namespace ColorGateRunner.Presentation
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class GateView : MonoBehaviour
    {
        [SerializeField] private GameSceneController controller;
        [SerializeField] private Renderer[] gateRenderers;

        private bool _hasResolved;

        public RunnerColor AssignedColor { get; private set; }

        internal bool HasResolved => _hasResolved;

        private void OnTriggerEnter(Collider other)
        {
            if (controller.IsPlayerCollider(other))
            {
                TryResolveCrossing();
            }
        }

        internal bool TryResolveCrossing()
        {
            if (_hasResolved)
            {
                return false;
            }

            _hasResolved = true;
            controller.HandleGateCrossed(this);
            return true;
        }

        internal void Activate(RunnerColor color, Material material, float worldZ)
        {
            AssignedColor = color;
            _hasResolved = false;

            Vector3 position = transform.position;
            position.z = worldZ;
            transform.position = position;

            for (int index = 0; index < gateRenderers.Length; index++)
            {
                gateRenderers[index].sharedMaterial = material;
            }
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
            GameSceneController sceneController,
            Renderer[] renderers)
        {
            controller = sceneController;
            gateRenderers = renderers;
        }
    }
}
