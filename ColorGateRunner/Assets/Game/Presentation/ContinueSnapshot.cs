using ColorGateRunner.Core;
using UnityEngine;

namespace ColorGateRunner.Presentation
{
    internal readonly struct ActiveGateSnapshot
    {
        internal ActiveGateSnapshot(
            int poolIdentity,
            bool active,
            bool resolved,
            int planIndex,
            GatePlan plan,
            Vector3 position,
            Quaternion rotation,
            Vector3[] partPositions,
            Quaternion[] partRotations)
        {
            PoolIdentity = poolIdentity;
            Active = active;
            Resolved = resolved;
            PlanIndex = planIndex;
            Plan = plan;
            Position = position;
            Rotation = rotation;
            PartPositions = partPositions;
            PartRotations = partRotations;
        }

        internal int PoolIdentity { get; }
        internal bool Active { get; }
        internal bool Resolved { get; }
        internal int PlanIndex { get; }
        internal GatePlan Plan { get; }
        internal Vector3 Position { get; }
        internal Quaternion Rotation { get; }
        internal Vector3[] PartPositions { get; }
        internal Quaternion[] PartRotations { get; }
    }

    internal sealed class ContinueSnapshot
    {
        internal ContinueSnapshot(
            string stageId,
            float elapsedPlayingSeconds,
            float normalizedProgress,
            float traveledDistance,
            float normalSpeed,
            RunnerColor playerColor,
            int sequenceCursor,
            int failedGateIndex,
            bool finalSection,
            bool goalActive,
            bool continueUsed,
            bool shieldSelected,
            bool shieldConsumed,
            bool boosterSelected,
            bool boosterCompleted,
            Vector3 playerPosition,
            Vector3 cameraPosition,
            Quaternion cameraRotation,
            ActiveGateSnapshot[] activeGates)
        {
            StageId = stageId;
            ElapsedPlayingSeconds = elapsedPlayingSeconds;
            NormalizedProgress = normalizedProgress;
            TraveledDistance = traveledDistance;
            NormalSpeed = normalSpeed;
            PlayerColor = playerColor;
            SequenceCursor = sequenceCursor;
            FailedGateIndex = failedGateIndex;
            FinalSection = finalSection;
            GoalActive = goalActive;
            ContinueUsed = continueUsed;
            ShieldSelected = shieldSelected;
            ShieldConsumed = shieldConsumed;
            BoosterSelected = boosterSelected;
            BoosterCompleted = boosterCompleted;
            PlayerPosition = playerPosition;
            CameraPosition = cameraPosition;
            CameraRotation = cameraRotation;
            ActiveGates = activeGates;
        }

        internal string StageId { get; }
        internal float ElapsedPlayingSeconds { get; }
        internal float NormalizedProgress { get; }
        internal float TraveledDistance { get; }
        internal float NormalSpeed { get; }
        internal RunnerColor PlayerColor { get; }
        internal int SequenceCursor { get; }
        internal int FailedGateIndex { get; }
        internal bool FinalSection { get; }
        internal bool GoalActive { get; }
        internal bool ContinueUsed { get; }
        internal bool ShieldSelected { get; }
        internal bool ShieldConsumed { get; }
        internal bool BoosterSelected { get; }
        internal bool BoosterCompleted { get; }
        internal Vector3 PlayerPosition { get; }
        internal Vector3 CameraPosition { get; }
        internal Quaternion CameraRotation { get; }
        internal ActiveGateSnapshot[] ActiveGates { get; }
    }
}
