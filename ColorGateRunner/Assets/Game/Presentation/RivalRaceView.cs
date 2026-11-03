using System;
using ColorGateRunner.Core;
using UnityEngine;

namespace ColorGateRunner.Presentation
{
    // Draws the two Ghost rivals on their own curved lanes beside the
    // Campaign road (see RivalTrackLayout). They are ambience only: no rank
    // is shown because the player's clean-run time is fixed per stage.
    // The rivals are presentation only: RivalRaceSimulation owns their
    // progress and nothing here touches the player's StageSession.
    public sealed class RivalRaceView : MonoBehaviour
    {
        internal const int GatesPerLane = 3;
        internal const int NeonPartsPerGate = 3;
        private const float GateFlashSeconds = 0.35f;
        private const float StumbleWobbleDegrees = 14f;
        private const float RunnerHeight = 1f;
        private const float HeadingLookAhead = 1.5f;

        [SerializeField] private MeshFilter[] laneMeshFilters;
        [SerializeField] private Transform[] rivalRunners;
        [SerializeField] private Transform[] laneGates;
        [SerializeField] private Renderer[] laneGateRenderers;
        [SerializeField] private Material gateIdleMaterial;
        [SerializeField] private Material gatePassMaterial;
        [SerializeField] private Material gateFailMaterial;

        private readonly Mesh[] _laneMeshes = new Mesh[RivalRaceSimulation.RivalCount];
        private readonly int[] _gateIndices =
            new int[RivalRaceSimulation.RivalCount * GatesPerLane];
        private readonly float[] _gateFlash =
            new float[RivalRaceSimulation.RivalCount * GatesPerLane];
        private readonly int[] _lastResolved = new int[RivalRaceSimulation.RivalCount];
        private RivalRaceSimulation _race;
        private RivalTrackLayout _layout;
        private CampaignSplinePathView _path;

        internal bool RaceActive => _race != null;
        internal RivalRaceSimulation Race => _race;
        internal RivalTrackLayout TrackLayout => _layout;
        internal Transform GetRunner(int rival) => rivalRunners[rival];
        internal Mesh GetLaneMesh(int rival) => _laneMeshes[rival];

        internal bool HasRequiredReferences
        {
            get
            {
                int lanes = RivalRaceSimulation.RivalCount;
                if (laneMeshFilters == null || laneMeshFilters.Length != lanes ||
                    rivalRunners == null || rivalRunners.Length != lanes ||
                    laneGates == null || laneGates.Length != lanes * GatesPerLane ||
                    laneGateRenderers == null ||
                    laneGateRenderers.Length != laneGates.Length * NeonPartsPerGate ||
                    gateIdleMaterial == null || gatePassMaterial == null ||
                    gateFailMaterial == null)
                {
                    return false;
                }
                for (int index = 0; index < lanes; index++)
                {
                    if (laneMeshFilters[index] == null || rivalRunners[index] == null)
                    {
                        return false;
                    }
                }
                for (int index = 0; index < laneGates.Length; index++)
                {
                    if (laneGates[index] == null)
                    {
                        return false;
                    }
                }
                for (int index = 0; index < laneGateRenderers.Length; index++)
                {
                    if (laneGateRenderers[index] == null)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        internal void Configure(
            MeshFilter[] lanes,
            Transform[] runners,
            Transform[] gates,
            Renderer[] gateRenderers,
            Material idle,
            Material pass,
            Material fail)
        {
            laneMeshFilters = lanes;
            rivalRunners = runners;
            laneGates = gates;
            laneGateRenderers = gateRenderers;
            gateIdleMaterial = idle;
            gatePassMaterial = pass;
            gateFailMaterial = fail;
            Hide();
        }

        internal void Begin(
            StageDefinition stage,
            CampaignSplinePathView path,
            float initialGateLeadDistance,
            float goalOffsetAfterFinalGate)
        {
            if (!HasRequiredReferences)
            {
                throw new InvalidOperationException(
                    "Rival race references are incomplete.");
            }
            if (!RivalRaceSimulation.IsEnabledFor(stage))
            {
                Hide();
                return;
            }

            _path = path ?? throw new ArgumentNullException(nameof(path));
            _race = new RivalRaceSimulation(
                stage,
                initialGateLeadDistance,
                goalOffsetAfterFinalGate);
            _layout = new RivalTrackLayout(stage.Seed);
            gameObject.SetActive(true);
            for (int rival = 0; rival < RivalRaceSimulation.RivalCount; rival++)
            {
                if (_laneMeshes[rival] == null)
                {
                    _laneMeshes[rival] = new Mesh { name = $"RivalLane_{rival}" };
                }
                int lane = rival;
                _path.BuildLaneMesh(
                    _laneMeshes[lane],
                    distance => _layout.GetLateralOffset(lane, distance),
                    distance => _layout.GetHeight(lane, distance));
                laneMeshFilters[rival].sharedMesh = _laneMeshes[rival];
                laneMeshFilters[rival].gameObject.SetActive(true);
                rivalRunners[rival].gameObject.SetActive(true);
                _lastResolved[rival] = 0;
                for (int slot = 0; slot < GatesPerLane; slot++)
                {
                    AssignGate(rival, slot, slot);
                }
            }
            PoseRunners();
        }

        internal void Tick(float deltaSeconds)
        {
            if (_race == null)
            {
                return;
            }

            _race.Advance(deltaSeconds);
            for (int rival = 0; rival < RivalRaceSimulation.RivalCount; rival++)
            {
                int resolved = _race.GetResolvedGateCount(rival);
                while (_lastResolved[rival] < resolved)
                {
                    OnGateResolved(rival, _lastResolved[rival]);
                    _lastResolved[rival]++;
                }
            }
            TickGateFlashes(deltaSeconds);
            PoseRunners();
        }

        internal void Hide()
        {
            _race = null;
            _layout = null;
            _path = null;
            gameObject.SetActive(false);
        }

        private void OnGateResolved(int rival, int gate)
        {
            int slot = FindSlot(rival, gate);
            if (slot >= 0)
            {
                int index = (rival * GatesPerLane) + slot;
                SetGateMaterial(
                    index,
                    _race.PassesGate(rival, gate) ? gatePassMaterial : gateFailMaterial);
                _gateFlash[index] = GateFlashSeconds;
            }
        }

        private void TickGateFlashes(float deltaSeconds)
        {
            for (int rival = 0; rival < RivalRaceSimulation.RivalCount; rival++)
            {
                for (int slot = 0; slot < GatesPerLane; slot++)
                {
                    int index = (rival * GatesPerLane) + slot;
                    if (_gateFlash[index] <= 0f)
                    {
                        continue;
                    }
                    _gateFlash[index] -= deltaSeconds;
                    if (_gateFlash[index] <= 0f)
                    {
                        // Recycle the resolved gate to the next unplanned one.
                        int next = _gateIndices[index] + GatesPerLane;
                        AssignGate(rival, slot, next);
                    }
                }
            }
        }

        private void AssignGate(int rival, int slot, int gate)
        {
            int index = (rival * GatesPerLane) + slot;
            _gateIndices[index] = gate;
            _gateFlash[index] = 0f;
            Transform view = laneGates[index];
            if (gate >= _race.GateCount)
            {
                view.gameObject.SetActive(false);
                return;
            }
            EvaluateLanePose(
                rival,
                _race.GetGateDistance(gate),
                0f,
                out Vector3 position,
                out Quaternion rotation);
            view.SetPositionAndRotation(position, rotation);
            SetGateMaterial(index, gateIdleMaterial);
            view.gameObject.SetActive(true);
        }

        private void SetGateMaterial(int gateIndex, Material material)
        {
            int first = gateIndex * NeonPartsPerGate;
            for (int part = 0; part < NeonPartsPerGate; part++)
            {
                laneGateRenderers[first + part].sharedMaterial = material;
            }
        }

        private int FindSlot(int rival, int gate)
        {
            for (int slot = 0; slot < GatesPerLane; slot++)
            {
                if (_gateIndices[(rival * GatesPerLane) + slot] == gate)
                {
                    return slot;
                }
            }
            return -1;
        }

        private void PoseRunners()
        {
            for (int rival = 0; rival < RivalRaceSimulation.RivalCount; rival++)
            {
                EvaluateLanePose(
                    rival,
                    _race.GetDistance(rival),
                    RunnerHeight,
                    out Vector3 position,
                    out Quaternion rotation);
                if (_race.IsStumbling(rival))
                {
                    float wobble = Mathf.Sin(_race.ElapsedSeconds * 40f) *
                        StumbleWobbleDegrees;
                    rotation *= Quaternion.Euler(0f, 0f, wobble);
                }
                rivalRunners[rival].SetPositionAndRotation(position, rotation);
            }
        }

        // World pose on a rival lane, facing along the lane's own sway.
        internal void EvaluateLanePose(
            int rival,
            float distance,
            float heightOffset,
            out Vector3 position,
            out Quaternion rotation)
        {
            position = GetLanePoint(rival, distance, heightOffset);
            Vector3 ahead = GetLanePoint(
                rival,
                distance + HeadingLookAhead,
                heightOffset);
            Vector3 forward = ahead - position;
            if (forward.sqrMagnitude < 0.0001f)
            {
                _path.EvaluatePose(distance, 0f, out _, out rotation);
                return;
            }
            rotation = Quaternion.LookRotation(forward, Vector3.up);
        }

        private Vector3 GetLanePoint(int rival, float distance, float heightOffset)
        {
            _path.EvaluatePose(
                distance,
                heightOffset + _layout.GetHeight(rival, distance),
                out Vector3 center,
                out Quaternion pathRotation);
            return center + (pathRotation * Vector3.right *
                _layout.GetLateralOffset(rival, distance));
        }

        private void OnDestroy()
        {
            for (int rival = 0; rival < _laneMeshes.Length; rival++)
            {
                if (_laneMeshes[rival] == null)
                {
                    continue;
                }
                if (Application.isPlaying)
                {
                    Destroy(_laneMeshes[rival]);
                }
                else
                {
                    DestroyImmediate(_laneMeshes[rival]);
                }
                _laneMeshes[rival] = null;
            }
        }
    }
}
