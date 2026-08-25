using UnityEngine;

namespace ColorGateRunner.Presentation
{
    public sealed class ProtectionFieldView : MonoBehaviour
    {
        private static readonly int FieldColorId =
            Shader.PropertyToID("_FieldColor");
        private static readonly int FieldPulseId =
            Shader.PropertyToID("_FieldPulse");

        [SerializeField] private Renderer[] fieldRenderers;
        [SerializeField] private ParticleSystem collapseParticles;
        [SerializeField] private Color fieldColor =
            new Color(0f, 0.72f, 1f, 1f);
        [SerializeField] private float rotationSpeed = 9f;
        private MaterialPropertyBlock _properties;

        internal Color FieldColor => fieldColor;
        internal int RendererCount =>
            fieldRenderers == null ? 0 : fieldRenderers.Length;
        internal ParticleSystem CollapseParticles => collapseParticles;
        internal bool HasRequiredReferences()
        {
            if (fieldRenderers == null || fieldRenderers.Length == 0 ||
                collapseParticles == null)
            {
                return false;
            }
            for (int index = 0; index < fieldRenderers.Length; index++)
            {
                if (fieldRenderers[index] == null)
                {
                    return false;
                }
            }
            return true;
        }

        internal void Configure(
            Renderer[] renderers,
            ParticleSystem collapseEffect,
            Color initialColor)
        {
            fieldRenderers = renderers;
            collapseParticles = collapseEffect;
            SetColor(initialColor);
        }

        internal void SetColor(Color color)
        {
            fieldColor = new Color(color.r, color.g, color.b, 1f);
            ApplyProperties(1f);
        }

        internal void PlayCollapse()
        {
            ParticleSystem.MainModule main = collapseParticles.main;
            main.startColor = fieldColor;
            collapseParticles.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear);
            collapseParticles.Play(true);
        }

        internal void ResetCollapse()
        {
            collapseParticles.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void OnEnable()
        {
            ApplyProperties(1f);
        }

        private void Update()
        {
            transform.Rotate(
                0f,
                rotationSpeed * Time.unscaledDeltaTime,
                rotationSpeed * 0.35f * Time.unscaledDeltaTime,
                Space.Self);
            float pulse = 0.88f +
                (Mathf.Sin(Time.unscaledTime * 4.2f) * 0.12f);
            ApplyProperties(pulse);
        }

        private void ApplyProperties(float pulse)
        {
            if (!HasRequiredReferences())
            {
                return;
            }
            _properties ??= new MaterialPropertyBlock();
            _properties.SetColor(FieldColorId, fieldColor);
            _properties.SetFloat(FieldPulseId, pulse);
            for (int index = 0; index < fieldRenderers.Length; index++)
            {
                fieldRenderers[index].SetPropertyBlock(_properties);
            }
        }
    }
}
