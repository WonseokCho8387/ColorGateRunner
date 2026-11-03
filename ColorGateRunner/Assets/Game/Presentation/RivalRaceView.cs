using System;
using ColorGateRunner.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    // Draws the two Ghost rivals on their own lanes beside the Campaign road.
    // The rivals are presentation only: RivalRaceSimulation owns their
    // progress and nothing here touches the player's StageSession.
    public sealed class RivalRaceView : MonoBehaviour
    {
        internal const float LaneOffset = 9f;
        internal const float CityClearance = 16f;
        internal const int GatesPerLane = 3;
        internal const int NeonPartsPerGate = 3;
        private const float GateFlashSeconds = 0.35f;
        private const float StumbleWobbleDegrees = 14f;
        private const float RunnerHeight = 1f;

        [SerializeField] private MeshFilter[] laneMeshFilters;
        [SerializeField] private Transform[] rivalRunners;
        [SerializeField] private Transform[] laneGates;
        [SerializeField] private Renderer[] laneGateRenderers;
        [SerializeField] private Material gateIdleMaterial;
        [SerializeField] private Material gatePassMaterial;
        [SerializeField] private Material gateFailMaterial;
        [SerializeField] private Text rankText;

        private readonly Mesh[] _laneMeshes = new Mesh[RivalRaceSimulation.RivalCount];
        private readonly int[] _gateIndices =
            new int[RivalRaceSimulation.RivalCount * GatesPerLane];
        private readonly float[] _gateFlash =
            new float[RivalRaceSimulation.RivalCount * GatesPerLane];
        private readonly int[] _lastResolved = new int[RivalRaceSimulation.RivalCount];
        private RivalRaceSimulation _race;
        private CampaignSplinePathView _path;
        private int _lastPlace = -1;

        internal bool RaceActive => _race != null;
        internal RivalRaceSimulation Race => _race;
        internal Text RankText => rankText;
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
                    gateFailMaterial == null || rankText == null)
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
            Material fail,
            Text rank)
        {
            laneMeshFilters = lanes;
            rivalRunners = runners;
            laneGates = gates;
            laneGateRenderers = gateRenderers;
            gateIdleMaterial = idle;
            gatePassMaterial = pass;
            gateFailMaterial = fail;
            rankText = rank;
            Hide();
        }

        internal static float GetLaneOffset(int rival) =>
            rival == 0 ? -LaneOffset : LaneOffset;

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
            gameObject.SetActive(true);
            for (int rival = 0; rival < RivalRaceSimulation.RivalCount; rival++)
            {
                if (_laneMeshes[rival] == null)
                {
                    _laneMeshes[rival] = new Mesh { name = $"RivalLane_{rival}" };
                }
                _path.BuildLaneMesh(_laneMeshes[rival], GetLaneOffset(rival));
                laneMeshFilters[rival].sharedMesh = _laneMeshes[rival];
                laneMeshFilters[rival].gameObject.SetActive(true);
                rivalRunners[rival].gameObject.SetActive(true);
                _lastResolved[rival] = 0;
                for (int slot = 0; slot < GatesPerLane; slot++)
                {
                    AssignGate(rival, slot, slot);
                }
            }
            _lastPlace = -1;
            PoseRunners();
            rankText.gameObject.SetActive(true);
            UpdateRank(0f, -1f);
        }

        internal void Tick(float deltaSeconds, float playerDistance)
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
            UpdateRank(playerDistance, -1f);
        }

        internal int GetPlayerPlace(float playerDistance, float playerFinishSeconds)
        {
            return _race == null
                ? 0
                : _race.GetPlayerPlace(playerDistance, playerFinishSeconds);
        }

        internal int CompleteForPlayer(float playerDistance)
        {
            if (_race == null)
            {
                return 0;
            }
            int place = _race.GetPlayerPlace(playerDistance, _race.ElapsedSeconds);
            UpdateRank(playerDistance, _race.ElapsedSeconds);
            return place;
        }

        internal void Hide()
        {
            _race = null;
            _path = null;
            _lastPlace = -1;
            if (rankText != null)
            {
                rankText.gameObject.SetActive(false);
            }
            gameObject.SetActive(false);
        }

        internal static string FormatPlace(int place)
        {
            switch (place)
            {
                case 1:
                    return "1ST";
                case 2:
                    return "2ND";
                case 3:
                    return "3RD";
                default:
                    return place + "TH";
            }
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
            _path.EvaluatePose(
                _race.GetGateDistance(gate),
                0f,
                out Vector3 position,
                out Quaternion rotation);
            view.SetPositionAndRotation(
                position + (rotation * Vector3.right * GetLaneOffset(rival)),
                rotation);
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
                _path.EvaluatePose(
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
                rivalRunners[rival].SetPositionAndRotation(
                    position + (rotation * Vector3.right * GetLaneOffset(rival)),
                    rotation);
            }
        }

        private void UpdateRank(float playerDistance, float playerFinishSeconds)
        {
            int place = _race.GetPlayerPlace(playerDistance, playerFinishSeconds);
            if (place == _lastPlace)
            {
                return;
            }
            _lastPlace = place;
            rankText.text = FormatPlace(place) + " / " +
                (RivalRaceSimulation.RivalCount + 1);
            rankText.color = place == 1
                ? NeonUiPalette.Magenta
                : NeonUiPalette.Signal;
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
