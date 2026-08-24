using UnityEngine;

namespace ColorGateRunner.Presentation
{
    public sealed class GateBreakEffectView : MonoBehaviour
    {
        private const float Duration = 0.3f;
        [SerializeField] private Transform[] fragments;
        [SerializeField] private Renderer[] renderers;
        private Vector3[] _startPositions;
        private Vector3[] _velocities;
        private float _elapsed;

        internal bool IsPlaying => gameObject.activeSelf;

        internal void Configure(Transform[] pieces, Renderer[] pieceRenderers)
        {
            fragments = pieces;
            renderers = pieceRenderers;
            EnsureRuntimeState();
            ResetEffect();
        }

        internal bool HasRequiredReferences()
        {
            return fragments != null && renderers != null &&
                fragments.Length == 5 && renderers.Length == 5;
        }

        internal void Play(Vector3 worldPosition, Material colorMaterial)
        {
            EnsureRuntimeState();
            transform.position = worldPosition;
            _elapsed = 0f;
            for (int index = 0; index < fragments.Length; index++)
            {
                fragments[index].localPosition = _startPositions[index];
                fragments[index].localRotation = Quaternion.identity;
                fragments[index].localScale = Vector3.one;
                renderers[index].sharedMaterial = colorMaterial;
            }
            gameObject.SetActive(true);
        }

        internal void ResetEffect()
        {
            _elapsed = 0f;
            if (fragments != null)
            {
                EnsureRuntimeState();
                for (int index = 0; index < fragments.Length; index++)
                {
                    fragments[index].localPosition = _startPositions[index];
                    fragments[index].localRotation = Quaternion.identity;
                    fragments[index].localScale = Vector3.one;
                }
            }
            gameObject.SetActive(false);
        }

        private void EnsureRuntimeState()
        {
            if (_startPositions != null && _startPositions.Length == fragments.Length)
            {
                return;
            }
            _startPositions = new Vector3[fragments.Length];
            _velocities = new Vector3[fragments.Length];
            for (int index = 0; index < fragments.Length; index++)
            {
                _startPositions[index] = fragments[index].localPosition;
                float side = index % 2 == 0 ? -1f : 1f;
                _velocities[index] = new Vector3(
                    side * (2.2f + (index * 0.32f)),
                    1.1f + (index * 0.24f),
                    -0.45f + (index * 0.18f));
            }
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            float normalized = Mathf.Clamp01(_elapsed / Duration);
            float scale = 1f - normalized;
            for (int index = 0; index < fragments.Length; index++)
            {
                Transform fragment = fragments[index];
                fragment.localPosition = _startPositions[index] +
                    (_velocities[index] * _elapsed);
                fragment.localRotation = Quaternion.Euler(
                    normalized * (80f + (index * 17f)),
                    normalized * (110f - (index * 9f)),
                    normalized * (45f + (index * 13f)));
                fragment.localScale = Vector3.one * scale;
            }
            if (_elapsed >= Duration)
            {
                ResetEffect();
            }
        }
    }
}
