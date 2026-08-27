using UnityEngine;

namespace ColorGateRunner.Presentation
{
    public sealed class RunnerSteeringView : MonoBehaviour
    {
        internal const float MaximumYawDegrees = 10f;
        internal const float MaximumLeanDegrees = 6f;
        private const float SteeringHalfLife = 0.12f;
        private const float MinimumLookAheadDistance = 6f;
        private const float MaximumLookAheadDistance = 12f;
        private const float LookAheadSeconds = 0.18f;

        [SerializeField] private Transform artworkRoot;

        internal bool HasRequiredReference => artworkRoot != null;
        internal float CurrentYawDegrees =>
            NormalizeSignedAngle(artworkRoot.localEulerAngles.y);
        internal float CurrentLeanDegrees =>
            NormalizeSignedAngle(artworkRoot.localEulerAngles.z);

        internal void Configure(Transform visualRoot)
        {
            artworkRoot = visualRoot;
            ResetSteering();
        }

        internal void Tick(
            CampaignSplinePathView path,
            float distance,
            float speed,
            float deltaSeconds)
        {
            if (path == null || !path.VisualsActive || artworkRoot == null)
            {
                return;
            }

            path.EvaluatePose(
                distance,
                0f,
                out _,
                out Quaternion currentRotation);
            float lookAheadDistance = Mathf.Clamp(
                Mathf.Max(0f, speed) * LookAheadSeconds,
                MinimumLookAheadDistance,
                MaximumLookAheadDistance);
            path.EvaluatePose(
                distance + lookAheadDistance,
                0f,
                out _,
                out Quaternion futureRotation);

            Quaternion relative =
                Quaternion.Inverse(currentRotation) * futureRotation;
            float routeYaw = NormalizeSignedAngle(relative.eulerAngles.y);
            float targetYaw = Mathf.Clamp(
                routeYaw,
                -MaximumYawDegrees,
                MaximumYawDegrees);
            float targetLean = Mathf.Clamp(
                -targetYaw * 0.6f,
                -MaximumLeanDegrees,
                MaximumLeanDegrees);
            Quaternion target = Quaternion.Euler(0f, targetYaw, targetLean);
            float blend = 1f - Mathf.Exp(
                -Mathf.Log(2f) * Mathf.Max(0f, deltaSeconds) /
                SteeringHalfLife);
            artworkRoot.localRotation = Quaternion.Slerp(
                artworkRoot.localRotation,
                target,
                blend);
        }

        internal void ResetSteering()
        {
            if (artworkRoot != null)
            {
                artworkRoot.localRotation = Quaternion.identity;
            }
        }

        private static float NormalizeSignedAngle(float angle)
        {
            return angle > 180f ? angle - 360f : angle;
        }
    }
}
