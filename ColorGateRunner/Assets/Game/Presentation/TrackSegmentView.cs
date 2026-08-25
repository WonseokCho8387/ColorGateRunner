using UnityEngine;

namespace ColorGateRunner.Presentation
{
    public sealed class TrackSegmentView : MonoBehaviour
    {
        [SerializeField] private Transform startAnchor;
        [SerializeField] private Transform endAnchor;
        [SerializeField] private bool curveExperiment;
        [SerializeField] private Renderer[] surfaceRenderers;

        internal Vector3 StartAnchorPosition => startAnchor.position;
        internal Vector3 EndAnchorPosition => endAnchor.position;
        internal bool IsCurveExperiment => curveExperiment;
        internal bool HasRequiredReferences =>
            startAnchor != null && endAnchor != null;
        internal Material SurfaceMaterial => GetSurfaceRenderer() == null
            ? null
            : GetSurfaceRenderer().sharedMaterial;

        internal void PlaceAt(Vector3 worldStart, Quaternion worldRotation)
        {
            transform.SetPositionAndRotation(worldStart, worldRotation);
            transform.position += worldStart - startAnchor.position;
        }

        internal void Configure(
            Transform start,
            Transform end,
            bool isCurveExperiment,
            Renderer[] surfaces = null)
        {
            startAnchor = start;
            endAnchor = end;
            curveExperiment = isCurveExperiment;
            surfaceRenderers = surfaces;
        }

        internal void SetSurfaceMaterial(Material material)
        {
            if (surfaceRenderers != null && surfaceRenderers.Length > 0)
            {
                for (int index = 0; index < surfaceRenderers.Length; index++)
                {
                    if (surfaceRenderers[index] != null)
                    {
                        surfaceRenderers[index].sharedMaterial = material;
                    }
                }
                return;
            }
            Renderer surface = GetComponent<Renderer>();
            if (surface != null)
            {
                surface.sharedMaterial = material;
            }
        }

        private Renderer GetSurfaceRenderer()
        {
            if (surfaceRenderers != null && surfaceRenderers.Length > 0)
            {
                return surfaceRenderers[0];
            }
            return GetComponent<Renderer>();
        }
    }
}
