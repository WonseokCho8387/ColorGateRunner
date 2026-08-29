using UnityEngine;

namespace ColorGateRunner.Presentation
{
    public sealed class FlickerGateEmblemView : MonoBehaviour
    {
        private static readonly int ProgressId =
            Shader.PropertyToID("_DissolveProgress");
        private static readonly int IncomingId =
            Shader.PropertyToID("_Incoming");
        private static readonly int EdgeColorId =
            Shader.PropertyToID("_EdgeColor");

        [SerializeField] private SpriteRenderer currentEmblem;
        [SerializeField] private SpriteRenderer nextEmblem;
        private MaterialPropertyBlock _currentProperties;
        private MaterialPropertyBlock _nextProperties;

        internal bool IsTransitioning { get; private set; }
        internal float Progress { get; private set; }
        internal bool NextEmblemVisible =>
            nextEmblem != null && nextEmblem.gameObject.activeSelf;
        internal Sprite CurrentSprite =>
            currentEmblem == null ? null : currentEmblem.sprite;
        internal Sprite NextSprite =>
            nextEmblem == null ? null : nextEmblem.sprite;
        internal bool HasRequiredReferences =>
            HasDissolveMaterial(currentEmblem) &&
            HasDissolveMaterial(nextEmblem);

        internal void Configure(
            SpriteRenderer current,
            SpriteRenderer next,
            Material dissolveMaterial)
        {
            currentEmblem = current;
            nextEmblem = next;
            currentEmblem.sharedMaterial = dissolveMaterial;
            nextEmblem.sharedMaterial = dissolveMaterial;
            SetStatic(currentEmblem.sprite, 1f);
        }

        internal void SetStatic(Sprite sprite, float alpha)
        {
            IsTransitioning = false;
            Progress = 0f;
            currentEmblem.sprite = sprite;
            SetRendererAlpha(currentEmblem, alpha);
            currentEmblem.gameObject.SetActive(alpha > 0f);
            ApplyProperties(
                currentEmblem,
                ref _currentProperties,
                0f,
                false,
                Color.clear);
            nextEmblem.gameObject.SetActive(false);
            nextEmblem.SetPropertyBlock(null);
        }

        internal void SetTransition(
            Sprite currentSprite,
            Sprite nextSprite,
            Color edgeColor,
            float progress)
        {
            IsTransitioning = true;
            Progress = Mathf.Clamp01(progress);
            currentEmblem.sprite = currentSprite;
            nextEmblem.sprite = nextSprite;
            SetRendererAlpha(currentEmblem, 1f);
            SetRendererAlpha(nextEmblem, 1f);
            currentEmblem.gameObject.SetActive(true);
            nextEmblem.gameObject.SetActive(true);
            ApplyProperties(
                currentEmblem,
                ref _currentProperties,
                Progress,
                false,
                edgeColor);
            ApplyProperties(
                nextEmblem,
                ref _nextProperties,
                Progress,
                true,
                edgeColor);
        }

        private static bool HasDissolveMaterial(SpriteRenderer renderer)
        {
            return renderer != null &&
                renderer.sharedMaterial != null &&
                renderer.sharedMaterial.shader != null &&
                renderer.sharedMaterial.shader.name ==
                    "ColorGateRunner/FlickerEmblemDissolve";
        }

        private static void SetRendererAlpha(
            SpriteRenderer renderer,
            float alpha)
        {
            Color tint = Color.white;
            tint.a = Mathf.Clamp01(alpha);
            renderer.color = tint;
        }

        private static void ApplyProperties(
            SpriteRenderer renderer,
            ref MaterialPropertyBlock properties,
            float progress,
            bool incoming,
            Color edgeColor)
        {
            properties ??= new MaterialPropertyBlock();
            properties.Clear();
            properties.SetFloat(ProgressId, Mathf.Clamp01(progress));
            properties.SetFloat(IncomingId, incoming ? 1f : 0f);
            properties.SetColor(EdgeColorId, edgeColor);
            renderer.SetPropertyBlock(properties);
        }
    }
}
