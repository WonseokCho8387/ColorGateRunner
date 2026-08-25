using ColorGateRunner.Presentation;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class SplineTrackLabEditModeTests
    {
        [Test]
        public void EvaluatePose_UsesDistanceAndPreservesHorizontalUp()
        {
            GameObject root = new GameObject("SplineTrackLabTest");
            GameObject visuals = new GameObject("Visuals");
            visuals.transform.SetParent(root.transform, false);
            GameObject surface = new GameObject("Surface");
            surface.transform.SetParent(visuals.transform, false);
            MeshFilter filter = surface.AddComponent<MeshFilter>();
            filter.sharedMesh = new Mesh();
            SplineContainer container = root.AddComponent<SplineContainer>();
            container.Spline = new Spline(
                new[]
                {
                    new float3(0f, 0f, 0f),
                    new float3(8f, 0f, 40f),
                    new float3(-8f, 0f, 80f),
                    new float3(0f, 0f, 120f)
                },
                TangentMode.AutoSmooth,
                false);
            SplineTrackLabView view =
                root.AddComponent<SplineTrackLabView>();
            view.Configure(container, visuals, filter, 7f);

            try
            {
                Assert.That(view.HasRequiredReferences, Is.True);
                Assert.That(view.PathLength, Is.GreaterThan(120f));
                view.EvaluatePose(
                    view.PathLength * 0.3f,
                    1f,
                    out Vector3 first,
                    out Quaternion firstRotation);
                view.EvaluatePose(
                    view.PathLength * 0.7f,
                    1f,
                    out Vector3 second,
                    out Quaternion secondRotation);
                Assert.That(first.y, Is.EqualTo(1f).Within(0.001f));
                Assert.That(second.y, Is.EqualTo(1f).Within(0.001f));
                Assert.That(Mathf.Abs(first.x - second.x), Is.GreaterThan(2f));
                Assert.That(
                    Vector3.Dot(firstRotation * Vector3.up, Vector3.up),
                    Is.GreaterThan(0.999f));
                Assert.That(
                    Vector3.Dot(secondRotation * Vector3.up, Vector3.up),
                    Is.GreaterThan(0.999f));
            }
            finally
            {
                Object.DestroyImmediate(filter.sharedMesh);
                Object.DestroyImmediate(root);
            }
        }
    }
}
