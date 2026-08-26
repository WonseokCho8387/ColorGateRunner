using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace ColorGateRunner.Presentation
{
    [RequireComponent(typeof(SplineContainer))]
    public sealed class CampaignSplinePathView : MonoBehaviour
    {
        internal const float TrackWidth = 7f;
        internal const float TrackRearExtension = 20f;
        private const float KnotSpacing = 52f;
        private const float RouteEndBuffer = 80f;
        private const float MeshSampleSpacing = 3f;
        private const float EdgeWidth = 0.16f;

        [SerializeField] private SplineContainer splineContainer;
        [SerializeField] private GameObject visualRoot;
        [SerializeField] private MeshFilter trackMeshFilter;

        private Mesh _runtimeMesh;
        private float _pathLength;

        internal float PathLength => _pathLength;
        internal float HorizontalAmplitude { get; private set; }
        internal float VerticalAmplitude { get; private set; }
        internal bool VisualsActive =>
            visualRoot != null && visualRoot.activeSelf;
        internal Mesh TrackMesh => trackMeshFilter == null
            ? null
            : trackMeshFilter.sharedMesh;
        internal bool HasRequiredReferences =>
            splineContainer != null &&
            visualRoot != null &&
            trackMeshFilter != null;

        internal void Configure(
            SplineContainer container,
            GameObject visuals,
            MeshFilter meshFilter)
        {
            splineContainer = container;
            visualRoot = visuals;
            trackMeshFilter = meshFilter;
            SetVisualsActive(false);
        }

        internal void BuildRoute(
            int stageNumber,
            float requiredDistance)
        {
            if (!HasRequiredReferences)
            {
                throw new InvalidOperationException(
                    "Campaign Spline path references are incomplete.");
            }
            if (stageNumber < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(stageNumber));
            }
            if (requiredDistance <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(requiredDistance));
            }

            ResolveRouteAmplitude(
                stageNumber,
                out float horizontalAmplitude,
                out float verticalAmplitude);
            HorizontalAmplitude = horizontalAmplitude;
            VerticalAmplitude = verticalAmplitude;

            float forwardLength = requiredDistance + RouteEndBuffer;
            int segmentCount = Mathf.Max(
                3,
                Mathf.CeilToInt(forwardLength / KnotSpacing));
            float3[] knots = new float3[segmentCount + 1];
            knots[0] = float3.zero;
            for (int index = 1; index <= segmentCount; index++)
            {
                float z = Mathf.Min(index * KnotSpacing, forwardLength);
                float x = 0f;
                float y = 0f;
                if (index > 1)
                {
                    float routeIndex = index - 1f;
                    float horizontalPhase =
                        (routeIndex * 1.02f) + (stageNumber * 0.17f);
                    x = Mathf.Sin(horizontalPhase) * horizontalAmplitude;
                    if (verticalAmplitude > 0f)
                    {
                        float verticalPhase = routeIndex * 0.78f;
                        y = verticalAmplitude *
                            (0.5f - (0.5f * Mathf.Cos(verticalPhase)));
                    }
                }
                knots[index] = new float3(x, y, z);
            }
            knots[segmentCount].z = forwardLength;

            splineContainer.Spline = new Spline(
                knots,
                TangentMode.AutoSmooth,
                false);
            _pathLength = splineContainer.CalculateLength();
            if (_pathLength < requiredDistance)
            {
                throw new InvalidOperationException(
                    "Generated Campaign Spline is shorter than the stage.");
            }

            RebuildTrackMesh();
            SetVisualsActive(true);
        }

        internal void EvaluatePose(
            float distance,
            float heightOffset,
            out Vector3 position,
            out Quaternion rotation)
        {
            if (_pathLength <= 0f)
            {
                throw new InvalidOperationException(
                    "Campaign Spline route has not been built.");
            }

            float clampedDistance = Mathf.Clamp(distance, 0f, _pathLength);
            float interpolation = SplineUtility.GetNormalizedInterpolation(
                splineContainer.Spline,
                clampedDistance,
                PathIndexUnit.Distance);
            Vector3 localPosition =
                splineContainer.EvaluatePosition(interpolation);
            Vector3 localTangent =
                splineContainer.EvaluateTangent(interpolation);
            Vector3 worldTangent = splineContainer.transform
                .TransformDirection(localTangent)
                .normalized;
            if (worldTangent.sqrMagnitude < 0.0001f)
            {
                worldTangent = Vector3.forward;
            }

            rotation = Quaternion.LookRotation(worldTangent, Vector3.up);
            Vector3 worldPosition = splineContainer.transform
                .TransformPoint(localPosition);
            position = worldPosition +
                (rotation * Vector3.up * heightOffset);
        }

        internal void ResetRoute()
        {
            _pathLength = 0f;
            HorizontalAmplitude = 0f;
            VerticalAmplitude = 0f;
            SetVisualsActive(false);
        }

        internal void SetVisualsActive(bool active)
        {
            if (visualRoot != null)
            {
                visualRoot.SetActive(active);
            }
        }

        private void OnDestroy()
        {
            if (_runtimeMesh == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(_runtimeMesh);
            }
            else
            {
                DestroyImmediate(_runtimeMesh);
            }
            _runtimeMesh = null;
        }

        private void RebuildTrackMesh()
        {
            if (_runtimeMesh == null)
            {
                _runtimeMesh = new Mesh
                {
                    name = "CampaignSplineTrackRuntime"
                };
                _runtimeMesh.MarkDynamic();
            }
            else
            {
                _runtimeMesh.Clear();
            }

            int pathRowCount = Mathf.Max(
                2,
                Mathf.CeilToInt(_pathLength / MeshSampleSpacing) + 1);
            int rowCount = pathRowCount + 1;
            int segmentCount = rowCount - 1;
            Vector3[] vertices = new Vector3[rowCount * 4];
            Vector2[] uvs = new Vector2[vertices.Length];
            int[] roadTriangles = new int[segmentCount * 6];
            int[] edgeTriangles = new int[segmentCount * 12];
            float halfWidth = TrackWidth * 0.5f;

            EvaluatePose(0f, 0f, out Vector3 start, out Quaternion startPose);
            Vector3 startForward = startPose * Vector3.forward;
            for (int row = 0; row < rowCount; row++)
            {
                Vector3 worldPosition;
                Quaternion worldRotation;
                float textureDistance;
                if (row == 0)
                {
                    worldPosition = start -
                        (startForward * TrackRearExtension);
                    worldRotation = startPose;
                    textureDistance = 0f;
                }
                else
                {
                    float distance = _pathLength *
                        ((row - 1f) / (pathRowCount - 1f));
                    EvaluatePose(
                        distance,
                        0f,
                        out worldPosition,
                        out worldRotation);
                    textureDistance = distance + TrackRearExtension;
                }

                Vector3 position = transform.InverseTransformPoint(
                    worldPosition);
                Vector3 right = transform.InverseTransformDirection(
                    worldRotation * Vector3.right).normalized;
                int vertex = row * 4;
                vertices[vertex] = position - (right * halfWidth);
                vertices[vertex + 1] =
                    position - (right * (halfWidth - EdgeWidth));
                vertices[vertex + 2] =
                    position + (right * (halfWidth - EdgeWidth));
                vertices[vertex + 3] = position + (right * halfWidth);
                float v = textureDistance / 8f;
                uvs[vertex] = new Vector2(0f, v);
                uvs[vertex + 1] = new Vector2(0.02f, v);
                uvs[vertex + 2] = new Vector2(0.98f, v);
                uvs[vertex + 3] = new Vector2(1f, v);
            }

            for (int row = 0; row < segmentCount; row++)
            {
                int current = row * 4;
                int next = current + 4;
                int road = row * 6;
                roadTriangles[road] = current + 1;
                roadTriangles[road + 1] = next + 1;
                roadTriangles[road + 2] = current + 2;
                roadTriangles[road + 3] = current + 2;
                roadTriangles[road + 4] = next + 1;
                roadTriangles[road + 5] = next + 2;

                int edge = row * 12;
                edgeTriangles[edge] = current;
                edgeTriangles[edge + 1] = next;
                edgeTriangles[edge + 2] = current + 1;
                edgeTriangles[edge + 3] = current + 1;
                edgeTriangles[edge + 4] = next;
                edgeTriangles[edge + 5] = next + 1;
                edgeTriangles[edge + 6] = current + 2;
                edgeTriangles[edge + 7] = next + 2;
                edgeTriangles[edge + 8] = current + 3;
                edgeTriangles[edge + 9] = current + 3;
                edgeTriangles[edge + 10] = next + 2;
                edgeTriangles[edge + 11] = next + 3;
            }

            _runtimeMesh.vertices = vertices;
            _runtimeMesh.uv = uvs;
            _runtimeMesh.subMeshCount = 2;
            _runtimeMesh.SetTriangles(roadTriangles, 0);
            _runtimeMesh.SetTriangles(edgeTriangles, 1);
            _runtimeMesh.RecalculateNormals();
            _runtimeMesh.RecalculateBounds();
            trackMeshFilter.sharedMesh = _runtimeMesh;
        }

        private static void ResolveRouteAmplitude(
            int stageNumber,
            out float horizontal,
            out float vertical)
        {
            if (stageNumber <= 7)
            {
                horizontal = Mathf.Lerp(
                    0f,
                    3f,
                    (stageNumber - 1f) / 6f);
                vertical = 0f;
                return;
            }
            if (stageNumber <= 13)
            {
                horizontal = Mathf.Lerp(
                    6f,
                    10f,
                    (stageNumber - 8f) / 5f);
                vertical = Mathf.Lerp(
                    0f,
                    1.5f,
                    (stageNumber - 8f) / 5f);
                return;
            }
            if (stageNumber <= 17)
            {
                horizontal = Mathf.Lerp(
                    10f,
                    14f,
                    (stageNumber - 14f) / 3f);
                vertical = Mathf.Lerp(
                    2.5f,
                    5.5f,
                    (stageNumber - 14f) / 3f);
                return;
            }

            horizontal = Mathf.Lerp(
                14f,
                18f,
                Mathf.Clamp01((stageNumber - 18f) / 2f));
            vertical = Mathf.Lerp(
                6f,
                9f,
                Mathf.Clamp01((stageNumber - 18f) / 2f));
        }
    }
}
