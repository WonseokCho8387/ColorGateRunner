using UnityEngine;

namespace ColorGateRunner.Presentation
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class ShieldPickupView : MonoBehaviour
    {
        [SerializeField] private GameSceneController controller;
        [SerializeField] private Renderer[] pickupRenderers;

        private float _baseY;
        private float _animationTime;

        internal bool IsAvailable => gameObject.activeSelf;

        private void OnTriggerEnter(Collider other)
        {
            if (controller.IsPlayerCollider(other))
            {
                TryCollect();
            }
        }

        internal bool TryCollect()
        {
            if (!gameObject.activeSelf)
            {
                return false;
            }

            return controller.HandleShieldPickupCollected(this);
        }

        internal void Activate(float worldZ)
        {
            Vector3 position = transform.position;
            position.z = worldZ;
            transform.position = position;
            _baseY = position.y;
            _animationTime = 0f;
            transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
            gameObject.SetActive(true);
        }

        internal void Deactivate()
        {
            gameObject.SetActive(false);
        }

        internal void Tick(float deltaTime)
        {
            if (!gameObject.activeSelf)
            {
                return;
            }

            _animationTime += deltaTime;
            transform.Rotate(0f, 120f * deltaTime, 0f, Space.Self);
            Vector3 position = transform.position;
            position.y = _baseY + (Mathf.Sin(_animationTime * 4f) * 0.18f);
            transform.position = position;
        }

        internal bool HasRequiredReferences()
        {
            if (controller == null ||
                pickupRenderers == null ||
                pickupRenderers.Length == 0)
            {
                return false;
            }

            for (int index = 0; index < pickupRenderers.Length; index++)
            {
                if (pickupRenderers[index] == null)
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
            pickupRenderers = renderers;
        }
    }
}
