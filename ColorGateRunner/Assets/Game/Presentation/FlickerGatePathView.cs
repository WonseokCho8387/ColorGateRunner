using UnityEngine;

namespace ColorGateRunner.Presentation
{
    public sealed class FlickerGatePathView : MonoBehaviour
    {
        private static readonly int TraceColorId =
            Shader.PropertyToID("_TraceColor");
        private const float SideLength = 2.75f;
        private const float TopLength = 4.64f;
        private const float TotalLength = (SideLength * 2f) + TopLength;
        private static readonly Vector3[] PathPoints =
        {
            new Vector3(-2.32f, 0.18f, -0.42f),
            new Vector3(-2.32f, 2.93f, -0.42f),
            new Vector3(2.32f, 2.93f, -0.42f),
            new Vector3(2.32f, 0.18f, -0.42f)
        };

        [SerializeField] private LineRenderer pathRenderer;
        private MaterialPropertyBlock _properties;

        internal bool IsVisible => gameObject.activeSelf;
        internal float Progress { get; private set; }
        internal Color TraceColor { get; private set; } = Color.clear;
        internal int RenderedPointCount =>
            pathRenderer == null ? 0 : pathRenderer.positionCount;
        internal Vector3 RenderedEndPoint =>
            pathRenderer == null || pathRenderer.positionCount == 0
                ? Vector3.zero
                : pathRenderer.GetPosition(pathRenderer.positionCount - 1);
        internal bool HasRequiredReferences =>
            pathRenderer != null &&
            pathRenderer.sharedMaterial != null &&
            pathRenderer.sharedMaterial.shader != null &&
            pathRenderer.sharedMaterial.shader.name ==
                "ColorGateRunner/FlickerGateTrace";

        internal void Configure(LineRenderer renderer)
        {
            pathRenderer = renderer;
            SetPresentation(false, Color.clear, 0f);
        }

        internal void SetPresentation(
            bool visible,
            Color color,
            float progress)
        {
            Progress = Mathf.Clamp01(progress);
            TraceColor = color;
            UpdatePathGeometry(Progress);
            if (pathRenderer != null)
            {
                _properties ??= new MaterialPropertyBlock();
                _properties.Clear();
                Color emission = color;
                emission.a = 0.92f;
                _properties.SetColor(TraceColorId, emission);
                pathRenderer.SetPropertyBlock(_properties);
            }
            gameObject.SetActive(visible && Progress > 0f);
        }

        private void UpdatePathGeometry(float progress)
        {
            if (pathRenderer == null)
            {
                return;
            }

            float remaining = TotalLength * progress;

            pathRenderer.positionCount = 2;
            pathRenderer.SetPosition(0, PathPoints[0]);
            if (remaining <= SideLength)
            {
                pathRenderer.SetPosition(
                    1,
                    Vector3.Lerp(
                        PathPoints[0],
                        PathPoints[1],
                        remaining / SideLength));
                return;
            }

            remaining -= SideLength;
            pathRenderer.positionCount = 3;
            pathRenderer.SetPosition(1, PathPoints[1]);
            if (remaining <= TopLength)
            {
                pathRenderer.SetPosition(
                    2,
                    Vector3.Lerp(
                        PathPoints[1],
                        PathPoints[2],
                        remaining / TopLength));
                return;
            }

            remaining -= TopLength;
            pathRenderer.positionCount = 4;
            pathRenderer.SetPosition(2, PathPoints[2]);
            pathRenderer.SetPosition(
                3,
                Vector3.Lerp(
                    PathPoints[2],
                    PathPoints[3],
                    Mathf.Clamp01(remaining / SideLength)));
        }
    }
}
