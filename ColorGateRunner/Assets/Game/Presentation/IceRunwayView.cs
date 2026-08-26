using System;
using ColorGateRunner.Core;
using UnityEngine;

namespace ColorGateRunner.Presentation
{
    public sealed class IceRunwayView : MonoBehaviour
    {
        internal const int RequiredPanelCount = 50;
        internal const float PanelWidth = 6.8f;
        internal const float PanelThickness = 0.05f;
        internal const float PanelCenterY = 0.125f;
        private const float SeamOverlap = 0.08f;
        private const float MeshSampleSpacing = 1.25f;
        private const float TextureRepeatDistance = 5f;

        [SerializeField] private Renderer[] panels;
        [SerializeField] private MeshFilter[] panelMeshFilters;
        private Mesh[] _runtimeMeshes;

        internal int PanelCount => panels == null ? 0 : panels.Length;

        internal int ActivePanelCount
        {
            get
            {
                int count = 0;
                if (panels == null)
                {
                    return count;
                }
                for (int index = 0; index < panels.Length; index++)
                {
                    if (panels[index] != null &&
                        panels[index].gameObject.activeSelf)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        internal void Configure(
            Renderer[] runwayPanels,
            MeshFilter[] meshFilters)
        {
            panels = runwayPanels;
            panelMeshFilters = meshFilters;
            ResetRunway();
        }

        internal bool HasRequiredReferences()
        {
            if (panels == null || panels.Length != RequiredPanelCount ||
                panelMeshFilters == null ||
                panelMeshFilters.Length != RequiredPanelCount)
            {
                return false;
            }
            for (int index = 0; index < panels.Length; index++)
            {
                if (panels[index] == null || panelMeshFilters[index] == null)
                {
                    return false;
                }
            }
            return true;
        }

        internal void Build(
            StageSession session,
            float playerZ,
            float initialGateLeadDistance)
        {
            ValidateBuild(session);
            EnsureRuntimeMeshes();

            float previousGateZ = playerZ + initialGateLeadDistance;
            for (int index = 0; index < panels.Length; index++)
            {
                if (index >= session.Stage.TargetGateCount)
                {
                    panels[index].gameObject.SetActive(false);
                    continue;
                }

                GatePlan plan = session.GetGatePlan(index);
                float gateZ = previousGateZ + plan.Spacing;
                panels[index].gameObject.SetActive(plan.Modifier.IsIce);
                if (plan.Modifier.IsIce)
                {
                    BuildStraightRibbon(
                        index,
                        previousGateZ,
                        gateZ + SeamOverlap);
                }
                previousGateZ = gateZ;
            }
        }

        internal void Build(
            StageSession session,
            CampaignSplinePathView path,
            float initialGateLeadDistance)
        {
            ValidateBuild(session);
            if (path == null || path.PathLength <= 0f)
            {
                throw new ArgumentException(
                    "Ice runway requires a built Campaign Spline.",
                    nameof(path));
            }
            EnsureRuntimeMeshes();

            float previousGateDistance = initialGateLeadDistance;
            for (int index = 0; index < panels.Length; index++)
            {
                if (index >= session.Stage.TargetGateCount)
                {
                    panels[index].gameObject.SetActive(false);
                    continue;
                }

                GatePlan plan = session.GetGatePlan(index);
                float gateDistance = previousGateDistance + plan.Spacing;
                panels[index].gameObject.SetActive(plan.Modifier.IsIce);
                if (plan.Modifier.IsIce)
                {
                    BuildSplineRibbon(
                        index,
                        path,
                        previousGateDistance,
                        gateDistance + SeamOverlap);
                }
                previousGateDistance = gateDistance;
            }
        }

        internal void ResetRunway()
        {
            if (panels == null)
            {
                return;
            }
            for (int index = 0; index < panels.Length; index++)
            {
                if (panels[index] != null)
                {
                    panels[index].gameObject.SetActive(false);
                }
            }
        }

        internal Renderer GetPanel(int index)
        {
            return panels[index];
        }

        internal Mesh GetPanelMesh(int index)
        {
            return panelMeshFilters[index].sharedMesh;
        }

        private void OnDestroy()
        {
            if (_runtimeMeshes == null)
            {
                return;
            }
            for (int index = 0; index < _runtimeMeshes.Length; index++)
            {
                if (_runtimeMeshes[index] == null)
                {
                    continue;
                }
                if (Application.isPlaying)
                {
                    Destroy(_runtimeMeshes[index]);
                }
                else
                {
                    DestroyImmediate(_runtimeMeshes[index]);
                }
            }
            _runtimeMeshes = null;
        }

        private void ValidateBuild(StageSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }
            if (!HasRequiredReferences())
            {
                throw new InvalidOperationException(
                    "Ice runway requires exactly 50 configured mesh panels.");
            }
            if (session.Stage.TargetGateCount > panels.Length)
            {
                throw new InvalidOperationException(
                    "The authored stage exceeds the fixed Ice runway pool.");
            }
        }

        private void EnsureRuntimeMeshes()
        {
            if (_runtimeMeshes != null &&
                _runtimeMeshes.Length == RequiredPanelCount)
            {
                return;
            }
            _runtimeMeshes = new Mesh[RequiredPanelCount];
            for (int index = 0; index < _runtimeMeshes.Length; index++)
            {
                Mesh mesh = new Mesh
                {
                    name = $"IceRunwayRibbon_{index:00}"
                };
                mesh.MarkDynamic();
                _runtimeMeshes[index] = mesh;
                panelMeshFilters[index].sharedMesh = mesh;
            }
        }

        private void BuildStraightRibbon(
            int panelIndex,
            float startZ,
            float endZ)
        {
            float length = Mathf.Max(0.01f, endZ - startZ);
            int rowCount = ResolveRowCount(length);
            Vector3[] vertices = new Vector3[rowCount * 2];
            Vector2[] uvs = new Vector2[vertices.Length];
            int[] triangles = new int[(rowCount - 1) * 6];
            float halfWidth = PanelWidth * 0.5f;
            for (int row = 0; row < rowCount; row++)
            {
                float normalized = row / (float)(rowCount - 1);
                float z = Mathf.Lerp(startZ, endZ, normalized);
                int vertex = row * 2;
                vertices[vertex] = transform.InverseTransformPoint(
                    new Vector3(-halfWidth, PanelCenterY, z));
                vertices[vertex + 1] = transform.InverseTransformPoint(
                    new Vector3(halfWidth, PanelCenterY, z));
                float v = length * normalized / TextureRepeatDistance;
                uvs[vertex] = new Vector2(0f, v);
                uvs[vertex + 1] = new Vector2(1f, v);
            }
            PopulateTriangles(triangles, rowCount);
            ApplyMesh(panelIndex, vertices, uvs, triangles);
        }

        private void BuildSplineRibbon(
            int panelIndex,
            CampaignSplinePathView path,
            float startDistance,
            float endDistance)
        {
            float length = Mathf.Max(0.01f, endDistance - startDistance);
            int rowCount = ResolveRowCount(length);
            Vector3[] vertices = new Vector3[rowCount * 2];
            Vector2[] uvs = new Vector2[vertices.Length];
            int[] triangles = new int[(rowCount - 1) * 6];
            float halfWidth = PanelWidth * 0.5f;
            for (int row = 0; row < rowCount; row++)
            {
                float normalized = row / (float)(rowCount - 1);
                float distance = Mathf.Lerp(
                    startDistance,
                    endDistance,
                    normalized);
                path.EvaluatePose(
                    distance,
                    PanelCenterY,
                    out Vector3 position,
                    out Quaternion rotation);
                Vector3 right = rotation * Vector3.right;
                int vertex = row * 2;
                vertices[vertex] = transform.InverseTransformPoint(
                    position - (right * halfWidth));
                vertices[vertex + 1] = transform.InverseTransformPoint(
                    position + (right * halfWidth));
                float v = length * normalized / TextureRepeatDistance;
                uvs[vertex] = new Vector2(0f, v);
                uvs[vertex + 1] = new Vector2(1f, v);
            }
            PopulateTriangles(triangles, rowCount);
            ApplyMesh(panelIndex, vertices, uvs, triangles);
        }

        private static int ResolveRowCount(float length)
        {
            return Mathf.Max(
                2,
                Mathf.CeilToInt(length / MeshSampleSpacing) + 1);
        }

        private static void PopulateTriangles(
            int[] triangles,
            int rowCount)
        {
            for (int row = 0; row < rowCount - 1; row++)
            {
                int current = row * 2;
                int next = current + 2;
                int triangle = row * 6;
                triangles[triangle] = current;
                triangles[triangle + 1] = next;
                triangles[triangle + 2] = current + 1;
                triangles[triangle + 3] = current + 1;
                triangles[triangle + 4] = next;
                triangles[triangle + 5] = next + 1;
            }
        }

        private void ApplyMesh(
            int panelIndex,
            Vector3[] vertices,
            Vector2[] uvs,
            int[] triangles)
        {
            Transform panel = panels[panelIndex].transform;
            panel.localPosition = Vector3.zero;
            panel.localRotation = Quaternion.identity;
            panel.localScale = Vector3.one;
            Mesh mesh = _runtimeMeshes[panelIndex];
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            panelMeshFilters[panelIndex].sharedMesh = mesh;
        }
    }
}
