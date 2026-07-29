using UnityEngine;

namespace ColorGateRunner.Presentation
{
    public sealed class TrackSegmentView : MonoBehaviour
    {
        [SerializeField] private Transform startAnchor;
        [SerializeField] private Transform endAnchor;
        [SerializeField] private bool curveExperiment;

        internal Vector3 StartAnchorPosition => startAnchor.position;
        internal Vector3 EndAnchorPosition => endAnchor.position;
        internal bool IsCurveExperiment => curveExperiment;
        internal bool HasRequiredReferences =>
            startAnchor != null && endAnchor != null;
        internal Material SurfaceMaterial =>
            GetComponent<Renderer>() == null
                ? null
                : GetComponent<Renderer>().sharedMaterial;

        internal void PlaceAt(Vector3 worldStart, Quaternion worldRotation)
        {
            transform.SetPositionAndRotation(worldStart, worldRotation);
        }

        internal void Configure(
            Transform start,
            Transform end,
            bool isCurveExperiment)
        {
            startAnchor = start;
            endAnchor = end;
            curveExperiment = isCurveExperiment;
        }

        internal void SetSurfaceMaterial(Material material)
        {
            Renderer surface = GetComponent<Renderer>();
            if (surface != null)
            {
                surface.sharedMaterial = material;
            }
        }
    }
}
