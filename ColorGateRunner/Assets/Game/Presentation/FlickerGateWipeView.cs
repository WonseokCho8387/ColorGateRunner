using UnityEngine;

namespace ColorGateRunner.Presentation
{
    public sealed class FlickerGateWipeView : MonoBehaviour
    {
        private static readonly int FieldColorId =
            Shader.PropertyToID("_FieldColor");
        private static readonly int FieldPulseId =
            Shader.PropertyToID("_FieldPulse");
        private const float FullWidth = 4.5f;

        [SerializeField] private Renderer wipeRenderer;
        private MaterialPropertyBlock _properties;

        internal bool IsVisible => gameObject.activeSelf;
        internal float Progress { get; private set; }
        internal Color WipeColor { get; private set; } = Color.clear;
        internal bool HasRequiredReferences =>
            wipeRenderer != null &&
            wipeRenderer.sharedMaterial != null &&
            wipeRenderer.sharedMaterial.shader != null &&
            wipeRenderer.sharedMaterial.shader.name ==
                "ColorGateRunner/ProtectionField";

        internal void Configure(Renderer renderer)
        {
            wipeRenderer = renderer;
            SetPresentation(false, Color.clear, 0f);
        }

        internal void SetPresentation(
            bool visible,
            Color color,
            float progress)
        {
            Progress = Mathf.Clamp01(progress);
            WipeColor = color;
            float width = FullWidth * Progress;
            transform.localScale = new Vector3(width, 2.65f, 1f);
            transform.localPosition = new Vector3(
                (-FullWidth * 0.5f) + (width * 0.5f),
                1.55f,
                -0.32f);
            if (wipeRenderer != null)
            {
                _properties ??= new MaterialPropertyBlock();
                _properties.Clear();
                Color fieldColor = color;
                fieldColor.a = 0.78f;
                _properties.SetColor(FieldColorId, fieldColor);
                _properties.SetFloat(FieldPulseId, 1f);
                wipeRenderer.SetPropertyBlock(_properties);
            }
            gameObject.SetActive(visible && Progress > 0f);
        }
    }
}
