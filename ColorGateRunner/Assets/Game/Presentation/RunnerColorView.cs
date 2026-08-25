using UnityEngine;

namespace ColorGateRunner.Presentation
{
    public sealed class RunnerColorView : MonoBehaviour
    {
        private static readonly int BaseColorId =
            Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorId =
            Shader.PropertyToID("_EmissionColor");

        [SerializeField] private Renderer emissiveAccent;
        [SerializeField] private Renderer glassShell;

        private MaterialPropertyBlock _glassProperties;
        private Material _appliedAccentMaterial;

        internal Renderer EmissiveAccent => emissiveAccent;
        internal Renderer GlassShell => glassShell;
        internal bool HasRequiredReferences =>
            emissiveAccent != null && glassShell != null &&
            glassShell.sharedMaterial != null;

        internal void Configure(Renderer accent, Renderer shell)
        {
            emissiveAccent = accent;
            glassShell = shell;
            _appliedAccentMaterial = null;
        }

        internal void ApplyMaterial(Material material)
        {
            if (material == null || !HasRequiredReferences ||
                _appliedAccentMaterial == material)
            {
                return;
            }

            _appliedAccentMaterial = material;
            emissiveAccent.sharedMaterial = material;
            _glassProperties ??= new MaterialPropertyBlock();
            glassShell.GetPropertyBlock(_glassProperties);
            Color color = material.HasProperty(BaseColorId)
                ? material.GetColor(BaseColorId)
                : material.color;
            color.a = 1f;
            _glassProperties.SetColor(BaseColorId, color);
            _glassProperties.SetColor(ColorId, color);
            _glassProperties.SetColor(EmissionColorId, Color.black);
            glassShell.SetPropertyBlock(_glassProperties);
        }
    }
}
