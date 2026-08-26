using UnityEngine;

namespace ColorGateRunner.Presentation
{
    public sealed class EchoGateFieldView : MonoBehaviour
    {
        private static readonly int FieldColorId =
            Shader.PropertyToID("_FieldColor");
        private static readonly int FieldPulseId =
            Shader.PropertyToID("_FieldPulse");

        [SerializeField] private Renderer fieldRenderer;
        private MaterialPropertyBlock _properties;
        private Color _fieldColor = Color.white;

        internal bool IsVisible => gameObject.activeSelf;
        internal Color FieldColor => _fieldColor;
        internal Renderer FieldRenderer => fieldRenderer;
        internal bool HasRequiredReferences =>
            fieldRenderer != null &&
            fieldRenderer.sharedMaterial != null &&
            fieldRenderer.sharedMaterial.shader != null &&
            fieldRenderer.sharedMaterial.shader.name ==
                "ColorGateRunner/ProtectionField";

        internal void Configure(Renderer renderer)
        {
            fieldRenderer = renderer;
            SetPresentation(false, Color.white, 0f);
        }

        internal void SetPresentation(
            bool visible,
            Color color,
            float alpha = 1f)
        {
            _fieldColor = new Color(
                color.r,
                color.g,
                color.b,
                Mathf.Clamp01(alpha));
            if (fieldRenderer != null)
            {
                _properties ??= new MaterialPropertyBlock();
                _properties.Clear();
                _properties.SetColor(FieldColorId, _fieldColor);
                _properties.SetFloat(FieldPulseId, 0.9f);
                fieldRenderer.SetPropertyBlock(_properties);
            }
            gameObject.SetActive(visible && alpha > 0f);
        }
    }
}
