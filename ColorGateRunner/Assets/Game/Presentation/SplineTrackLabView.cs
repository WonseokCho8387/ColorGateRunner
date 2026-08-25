using UnityEngine;
using UnityEngine.Splines;

namespace ColorGateRunner.Presentation
{
    [RequireComponent(typeof(SplineContainer))]
    public sealed class SplineTrackLabView : MonoBehaviour
    {
        [SerializeField] private SplineContainer splineContainer;
        [SerializeField] private GameObject visualRoot;
        [SerializeField] private MeshFilter trackMeshFilter;
        [SerializeField] private float trackWidth = 7f;

        private float _pathLength;

        internal float PathLength
        {
            get
            {
                if (_pathLength <= 0f && splineContainer != null &&
                    splineContainer.Splines.Count > 0)
                {
                    _pathLength = splineContainer.CalculateLength();
                }
                return _pathLength;
            }
        }
        internal float TrackWidth => trackWidth;
        internal bool VisualsActive => visualRoot != null && visualRoot.activeSelf;
        internal Mesh TrackMesh => trackMeshFilter == null
            ? null
            : trackMeshFilter.sharedMesh;
        internal bool HasRequiredReferences =>
            splineContainer != null &&
            splineContainer.Splines.Count == 1 &&
            visualRoot != null &&
            trackMeshFilter != null &&
            trackMeshFilter.sharedMesh != null &&
            trackWidth > 0f &&
            PathLength > 100f;

        internal void Configure(
            SplineContainer container,
            GameObject visuals,
            MeshFilter meshFilter,
            float width)
        {
            splineContainer = container;
            visualRoot = visuals;
            trackMeshFilter = meshFilter;
            trackWidth = width;
            _pathLength = container == null || container.Splines.Count == 0
                ? 0f
                : container.CalculateLength();
        }

        internal void SetVisualsActive(bool active)
        {
            visualRoot.SetActive(active);
        }

        internal void EvaluatePose(
            float distance,
            float heightOffset,
            out Vector3 position,
            out Quaternion rotation)
        {
            float length = PathLength;
            float clampedDistance = Mathf.Clamp(distance, 0f, length);
            float interpolation = SplineUtility.GetNormalizedInterpolation(
                splineContainer.Spline,
                clampedDistance,
                PathIndexUnit.Distance);
            Vector3 localPosition = splineContainer.EvaluatePosition(interpolation);
            Vector3 localTangent = splineContainer.EvaluateTangent(interpolation);
            Vector3 worldTangent = splineContainer.transform.TransformDirection(
                localTangent).normalized;
            if (worldTangent.sqrMagnitude < 0.0001f)
            {
                worldTangent = Vector3.forward;
            }
            rotation = Quaternion.LookRotation(worldTangent, Vector3.up);
            position = splineContainer.transform.TransformPoint(localPosition) +
                (Vector3.up * heightOffset);
        }
    }
}
