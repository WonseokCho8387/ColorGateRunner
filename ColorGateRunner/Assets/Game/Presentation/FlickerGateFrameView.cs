using UnityEngine;

namespace ColorGateRunner.Presentation
{
    public sealed class FlickerGateFrameView : MonoBehaviour
    {
        private const float SideLength = 2.75f;
        private const float TopLength = 4.64f;
        private const float TotalLength = (SideLength * 2f) + TopLength;

        private static readonly int CurrentColorId =
            Shader.PropertyToID("_CurrentColor");
        private static readonly int NextColorId =
            Shader.PropertyToID("_NextColor");
        private static readonly int CurrentEmissionId =
            Shader.PropertyToID("_CurrentEmission");
        private static readonly int NextEmissionId =
            Shader.PropertyToID("_NextEmission");
        private static readonly int RevealProgressId =
            Shader.PropertyToID("_RevealProgress");
        private static readonly int AxisId =
            Shader.PropertyToID("_Axis");
        private static readonly int AxisMinId =
            Shader.PropertyToID("_AxisMin");
        private static readonly int AxisRangeId =
            Shader.PropertyToID("_AxisRange");
        private static readonly int AxisDirectionId =
            Shader.PropertyToID("_AxisDirection");

        [SerializeField] private Renderer leftRenderer;
        [SerializeField] private Renderer topRenderer;
        [SerializeField] private Renderer rightRenderer;
        [SerializeField] private Material transitionMaterial;

        private MaterialPropertyBlock _leftProperties;
        private MaterialPropertyBlock _topProperties;
        private MaterialPropertyBlock _rightProperties;

        internal bool IsTransitioning { get; private set; }
        internal float Progress { get; private set; }
        internal float LeftProgress { get; private set; }
        internal float TopProgress { get; private set; }
        internal float RightProgress { get; private set; }
        internal Material TransitionMaterial => transitionMaterial;
        internal bool HasRequiredReferences =>
            HasMesh(leftRenderer) &&
            HasMesh(topRenderer) &&
            HasMesh(rightRenderer) &&
            transitionMaterial != null &&
            transitionMaterial.shader != null &&
            transitionMaterial.shader.name ==
                "ColorGateRunner/FlickerGateFrameDissolve";

        internal void Configure(
            Renderer left,
            Renderer top,
            Renderer right,
            Material dissolveMaterial)
        {
            leftRenderer = left;
            topRenderer = top;
            rightRenderer = right;
            transitionMaterial = dissolveMaterial;
            ResetPresentation();
        }

        internal void SetTransition(
            Material currentMaterial,
            Material nextMaterial,
            float progress)
        {
            Progress = Mathf.Clamp01(progress);
            IsTransitioning = true;

            float distance = Progress * TotalLength;
            LeftProgress = Mathf.Clamp01(distance / SideLength);
            TopProgress = Mathf.Clamp01(
                (distance - SideLength) / TopLength);
            RightProgress = Mathf.Clamp01(
                (distance - SideLength - TopLength) / SideLength);

            Color currentColor = GetMaterialColor(currentMaterial);
            Color nextColor = GetMaterialColor(nextMaterial);
            Color currentEmission = GetEmissionColor(
                currentMaterial,
                currentColor);
            Color nextEmission = GetEmissionColor(
                nextMaterial,
                nextColor);

            ApplyPart(
                leftRenderer,
                ref _leftProperties,
                currentColor,
                nextColor,
                currentEmission,
                nextEmission,
                LeftProgress,
                1f,
                1f);
            ApplyPart(
                topRenderer,
                ref _topProperties,
                currentColor,
                nextColor,
                currentEmission,
                nextEmission,
                TopProgress,
                0f,
                1f);
            ApplyPart(
                rightRenderer,
                ref _rightProperties,
                currentColor,
                nextColor,
                currentEmission,
                nextEmission,
                RightProgress,
                1f,
                -1f);
        }

        internal void ResetPresentation()
        {
            IsTransitioning = false;
            Progress = 0f;
            LeftProgress = 0f;
            TopProgress = 0f;
            RightProgress = 0f;
            leftRenderer?.SetPropertyBlock(null);
            topRenderer?.SetPropertyBlock(null);
            rightRenderer?.SetPropertyBlock(null);
        }

        private void ApplyPart(
            Renderer renderer,
            ref MaterialPropertyBlock properties,
            Color currentColor,
            Color nextColor,
            Color currentEmission,
            Color nextEmission,
            float progress,
            float axis,
            float direction)
        {
            Mesh mesh = renderer.GetComponent<MeshFilter>().sharedMesh;
            Bounds bounds = mesh.bounds;
            float axisMin = axis < 0.5f ? bounds.min.x : bounds.min.y;
            float axisRange = axis < 0.5f ? bounds.size.x : bounds.size.y;

            properties ??= new MaterialPropertyBlock();
            properties.Clear();
            properties.SetColor(CurrentColorId, currentColor);
            properties.SetColor(NextColorId, nextColor);
            properties.SetColor(CurrentEmissionId, currentEmission);
            properties.SetColor(NextEmissionId, nextEmission);
            properties.SetFloat(RevealProgressId, progress);
            properties.SetFloat(AxisId, axis);
            properties.SetFloat(AxisMinId, axisMin);
            properties.SetFloat(AxisRangeId, Mathf.Max(0.0001f, axisRange));
            properties.SetFloat(AxisDirectionId, direction);
            renderer.sharedMaterial = transitionMaterial;
            renderer.SetPropertyBlock(properties);
        }

        private static bool HasMesh(Renderer renderer)
        {
            if (renderer == null)
            {
                return false;
            }
            MeshFilter filter = renderer.GetComponent<MeshFilter>();
            return filter != null && filter.sharedMesh != null;
        }

        private static Color GetMaterialColor(Material material)
        {
            if (material != null && material.HasProperty("_BaseColor"))
            {
                return material.GetColor("_BaseColor");
            }
            if (material != null && material.HasProperty("_Color"))
            {
                return material.GetColor("_Color");
            }
            return Color.white;
        }

        private static Color GetEmissionColor(
            Material material,
            Color fallback)
        {
            if (material != null && material.HasProperty("_EmissionColor"))
            {
                return material.GetColor("_EmissionColor");
            }
            return fallback * 4.5f;
        }
    }
}
