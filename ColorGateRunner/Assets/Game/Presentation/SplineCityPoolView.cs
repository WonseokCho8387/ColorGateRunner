using System;
using UnityEngine;

namespace ColorGateRunner.Presentation
{
    public sealed class SplineCityPoolView : MonoBehaviour
    {
        internal const float MinimumLateralOffset = 7.5f;
        private const float LateralJitter = 4f;
        private const float InitialDistance = 8f;
        private const float PlacementSpacing = 12f;
        private const float DistanceJitter = 2f;
        private const float RecycleBehindDistance = 24f;
        private const float GoalVisualBuffer = 24f;
        // Towers are 1.6 units wide and 2.5 deep (Theme01 NeonCityBackdrop).
        internal const float TowerHalfWidth = 0.8f;
        private const float TowerHalfDepth = 1.25f;
        internal const float LaneClearance = 1f;
        private const float BetweenChance = 0.45f;

        [SerializeField] private Transform[] buildingBodies;
        [SerializeField] private Transform[] buildingGlows;

        private Vector3[] _originalBodyPositions;
        private Quaternion[] _originalBodyRotations;
        private Vector3[] _originalGlowPositions;
        private Quaternion[] _originalGlowRotations;
        private Vector3[] _glowOffsets;
        private int[] _sequenceIndices;
        private int[] _sideOrdinals;
        private int[] _sideCounts;
        private CampaignSplinePathView _path;
        private float[] _lateralDistances;
        private uint _stageSeed;
        private RivalTrackLayout _rivalTracks;
        private float _goalDistance;
        private bool _campaignActive;

        internal int SlotCount =>
            buildingBodies == null ? 0 : buildingBodies.Length;
        internal bool CampaignActive => _campaignActive;
        internal bool HasRequiredReferences
        {
            get
            {
                if (buildingBodies == null || buildingGlows == null ||
                    buildingBodies.Length != buildingGlows.Length ||
                    buildingBodies.Length < 4)
                {
                    return false;
                }
                for (int index = 0; index < buildingBodies.Length; index++)
                {
                    if (buildingBodies[index] == null ||
                        buildingGlows[index] == null)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        private void Awake()
        {
            if (HasRequiredReferences)
            {
                CaptureOriginalPoses();
            }
        }

        internal void Configure(
            Transform[] bodies,
            Transform[] glows)
        {
            buildingBodies = bodies ??
                throw new ArgumentNullException(nameof(bodies));
            buildingGlows = glows ??
                throw new ArgumentNullException(nameof(glows));
            if (!HasRequiredReferences)
            {
                throw new InvalidOperationException(
                    "Spline city requires paired body and glow transforms.");
            }
            CaptureOriginalPoses();
        }

        internal void Build(
            CampaignSplinePathView path,
            uint stageSeed,
            float goalDistance)
        {
            Build(path, stageSeed, goalDistance, null);
        }

        // On rival stages each tower stands either in the gap between the
        // player road and a rival lane, or outside that lane, never on it.
        internal void Build(
            CampaignSplinePathView path,
            uint stageSeed,
            float goalDistance,
            RivalTrackLayout rivalTracks)
        {
            if (!HasRequiredReferences)
            {
                throw new InvalidOperationException(
                    "Spline city pool references are incomplete.");
            }
            _path = path ?? throw new ArgumentNullException(nameof(path));
            if (!_path.HasRequiredReferences || _path.PathLength <= 0f)
            {
                throw new InvalidOperationException(
                    "Spline city requires a built Campaign path.");
            }
            _stageSeed = stageSeed;
            _goalDistance = Mathf.Max(0f, goalDistance);
            _rivalTracks = rivalTracks;
            _campaignActive = true;

            int leftCount = 0;
            int rightCount = 0;
            for (int index = 0; index < buildingBodies.Length; index++)
            {
                bool right = _originalBodyPositions[index].x >= 0f;
                _sideOrdinals[index] = right ? rightCount++ : leftCount++;
            }
            for (int index = 0; index < buildingBodies.Length; index++)
            {
                bool right = _originalBodyPositions[index].x >= 0f;
                _sideCounts[index] = right ? rightCount : leftCount;
                _sequenceIndices[index] = _sideOrdinals[index];
                PlaceSlot(index);
            }
        }

        internal void Tick(float playerDistance)
        {
            if (!_campaignActive)
            {
                return;
            }
            for (int index = 0; index < buildingBodies.Length; index++)
            {
                int guard = 0;
                while (GetPlacementDistance(index) <
                    playerDistance - RecycleBehindDistance)
                {
                    _sequenceIndices[index] += _sideCounts[index];
                    guard++;
                    if (guard > 4)
                    {
                        break;
                    }
                }
                PlaceSlot(index);
            }
        }

        internal void ResetPool()
        {
            _campaignActive = false;
            _path = null;
            _rivalTracks = null;
            if (_originalBodyPositions == null)
            {
                return;
            }
            for (int index = 0; index < buildingBodies.Length; index++)
            {
                Transform body = buildingBodies[index];
                Transform glow = buildingGlows[index];
                body.gameObject.SetActive(true);
                glow.gameObject.SetActive(true);
                body.localPosition = _originalBodyPositions[index];
                body.localRotation = _originalBodyRotations[index];
                glow.localPosition = _originalGlowPositions[index];
                glow.localRotation = _originalGlowRotations[index];
            }
        }

        internal float GetSlotLateralDistance(int index)
        {
            return _lateralDistances[index];
        }

        internal float GetSlotPathDistance(int index)
        {
            return GetPlacementDistance(index);
        }

        internal float GetSlotSide(int index)
        {
            return _originalBodyPositions[index].x >= 0f ? 1f : -1f;
        }

        internal Vector3 GetSlotPosition(int index)
        {
            return buildingBodies[index].position;
        }

        private void CaptureOriginalPoses()
        {
            int count = buildingBodies.Length;
            _originalBodyPositions = new Vector3[count];
            _originalBodyRotations = new Quaternion[count];
            _originalGlowPositions = new Vector3[count];
            _originalGlowRotations = new Quaternion[count];
            _glowOffsets = new Vector3[count];
            _sequenceIndices = new int[count];
            _sideOrdinals = new int[count];
            _sideCounts = new int[count];
            _lateralDistances = new float[count];
            for (int index = 0; index < count; index++)
            {
                Transform body = buildingBodies[index];
                Transform glow = buildingGlows[index];
                _originalBodyPositions[index] = body.localPosition;
                _originalBodyRotations[index] = body.localRotation;
                _originalGlowPositions[index] = glow.localPosition;
                _originalGlowRotations[index] = glow.localRotation;
                _glowOffsets[index] =
                    glow.localPosition - body.localPosition;
            }
        }

        private void PlaceSlot(int index)
        {
            float distance = GetPlacementDistance(index);
            Transform body = buildingBodies[index];
            Transform glow = buildingGlows[index];
            float side = GetSlotSide(index);
            float lateral = GetLateralOffset(index, distance, side);
            _lateralDistances[index] = lateral;
            if (distance > _goalDistance + GoalVisualBuffer)
            {
                body.gameObject.SetActive(false);
                glow.gameObject.SetActive(false);
                return;
            }

            body.gameObject.SetActive(true);
            glow.gameObject.SetActive(true);
            _path.EvaluatePose(
                distance,
                0f,
                out Vector3 pathPosition,
                out Quaternion pathRotation);
            Vector3 pathRight = pathRotation * Vector3.right;
            Vector3 flatForward = Vector3.ProjectOnPlane(
                pathRotation * Vector3.forward,
                Vector3.up).normalized;
            if (flatForward.sqrMagnitude < 0.0001f)
            {
                flatForward = Vector3.forward;
            }
            Quaternion yaw = Quaternion.LookRotation(
                flatForward,
                Vector3.up);
            Vector3 bodyPosition = pathPosition +
                (pathRight * side * lateral) +
                (Vector3.up * _originalBodyPositions[index].y);
            body.SetPositionAndRotation(
                bodyPosition,
                yaw * _originalBodyRotations[index]);
            glow.SetPositionAndRotation(
                bodyPosition + (yaw * _glowOffsets[index]),
                yaw * _originalGlowRotations[index]);
        }

        private float GetPlacementDistance(int index)
        {
            int sequence = _sequenceIndices[index];
            return InitialDistance +
                (sequence * PlacementSpacing) +
                ((Hash01(_stageSeed, sequence, index, 11u) - 0.5f) *
                 DistanceJitter * 2f);
        }

        private float GetLateralOffset(int index, float distance, float side)
        {
            int sequence = _sequenceIndices[index];
            float jitter = Hash01(_stageSeed, sequence, index, 29u);
            if (_rivalTracks == null)
            {
                return MinimumLateralOffset + (jitter * LateralJitter);
            }

            _rivalTracks.GetLaneDistanceRange(
                RivalTrackLayout.GetRivalOnSide(side),
                distance - TowerHalfDepth,
                distance + TowerHalfDepth,
                out float nearestLane,
                out float farthestLane);
            float laneMargin = RivalTrackLayout.LaneHalfWidth +
                LaneClearance + TowerHalfWidth;
            float gapOuter = nearestLane - laneMargin;
            if (gapOuter >= MinimumLateralOffset &&
                Hash01(_stageSeed, sequence, index, 41u) < BetweenChance)
            {
                return Mathf.Lerp(MinimumLateralOffset, gapOuter, jitter);
            }
            return farthestLane + laneMargin + (jitter * LateralJitter);
        }

        private static float Hash01(
            uint seed,
            int sequence,
            int slot,
            uint salt)
        {
            uint value = seed ^ salt;
            value ^= (uint)(sequence + 1) * 0x9E3779B9u;
            value ^= (uint)(slot + 1) * 0x85EBCA6Bu;
            value ^= value >> 16;
            value *= 0x7FEB352Du;
            value ^= value >> 15;
            value *= 0x846CA68Bu;
            value ^= value >> 16;
            return (value & 0x00FFFFFFu) / 16777215f;
        }
    }
}
