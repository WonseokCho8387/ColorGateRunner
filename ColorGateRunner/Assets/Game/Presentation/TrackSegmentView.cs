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
    }
}
