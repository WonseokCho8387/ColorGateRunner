using System;

namespace ColorGateRunner.Core
{
    public readonly struct RivalProfile
    {
        public RivalProfile(
            float paceFactor,
            float passChance,
            float stumbleSeconds,
            float stumbleSpeedFactor)
        {
            PaceFactor = paceFactor;
            PassChance = passChance;
            StumbleSeconds = stumbleSeconds;
            StumbleSpeedFactor = stumbleSpeedFactor;
        }

        public float PaceFactor { get; }
        public float PassChance { get; }
        public float StumbleSeconds { get; }
        public float StumbleSpeedFactor { get; }
    }

    // Presentation-only Ghost rivals. They follow the stage's deterministic
    // gate spacing and speed profile on their own scalar distance and never
    // read or change the player's StageSession, judgment, rewards or saves.
    public sealed class RivalRaceSimulation
    {
        public const int RivalCount = 2;
        public const int FirstRivalStage = 2;
        private const uint SeedSalt = 0x9E3779B9u;

        private static readonly RivalProfile[] BaseProfiles =
        {
            // Quick but error-prone, then steady and careful.
            new RivalProfile(1.04f, 0.82f, 0.9f, 0.45f),
            new RivalProfile(0.97f, 0.94f, 0.9f, 0.45f)
        };

        private readonly StageDefinition _stage;
        private readonly float[] _gateDistances;
        private readonly bool[,] _passes;
        private readonly RivalProfile[] _profiles = new RivalProfile[RivalCount];
        private readonly float[] _distances = new float[RivalCount];
        private readonly int[] _resolvedGates = new int[RivalCount];
        private readonly float[] _stumbleRemaining = new float[RivalCount];
        private readonly float[] _finishSeconds = new float[RivalCount];
        private float _elapsedSeconds;

        public RivalRaceSimulation(
            StageDefinition stage,
            float initialGateLeadDistance,
            float goalOffsetAfterFinalGate)
        {
            _stage = stage ?? throw new ArgumentNullException(nameof(stage));
            if (initialGateLeadDistance < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(initialGateLeadDistance));
            }
            if (goalOffsetAfterFinalGate <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(goalOffsetAfterFinalGate));
            }

            // Same raw spacing walk as StageGoalPlanner, on a private
            // sequence instance so the player's sequence is never advanced.
            var sequence = new DeterministicStageGateSequence(stage);
            _gateDistances = new float[stage.TargetGateCount];
            float distance = initialGateLeadDistance;
            for (int gate = 0; gate < _gateDistances.Length; gate++)
            {
                distance += sequence.GetPlan(gate).Spacing;
                _gateDistances[gate] = distance;
            }
            GoalDistance = distance + goalOffsetAfterFinalGate;

            _passes = new bool[RivalCount, _gateDistances.Length];
            for (int rival = 0; rival < RivalCount; rival++)
            {
                uint state = stage.Seed ^ (SeedSalt * (uint)(rival + 1));
                if (state == 0u)
                {
                    state = SeedSalt;
                }
                state = DeterministicGateSequence.AdvanceXorshift32(state);
                float paceJitter = ((state % 1000u) / 1000f - 0.5f) * 0.04f;
                RivalProfile baseline = BaseProfiles[rival];
                _profiles[rival] = new RivalProfile(
                    baseline.PaceFactor + paceJitter,
                    baseline.PassChance,
                    baseline.StumbleSeconds,
                    baseline.StumbleSpeedFactor);
                for (int gate = 0; gate < _gateDistances.Length; gate++)
                {
                    state = DeterministicGateSequence.AdvanceXorshift32(state);
                    _passes[rival, gate] =
                        (state % 10000u) / 10000f < baseline.PassChance;
                }
            }
            Reset();
        }

        public float GoalDistance { get; }
        public int GateCount => _gateDistances.Length;
        public float ElapsedSeconds => _elapsedSeconds;

        public static bool IsEnabledFor(StageDefinition stage)
        {
            return stage != null && stage.DisplayNumber >= FirstRivalStage;
        }

        public void Reset()
        {
            _elapsedSeconds = 0f;
            for (int rival = 0; rival < RivalCount; rival++)
            {
                _distances[rival] = 0f;
                _resolvedGates[rival] = 0;
                _stumbleRemaining[rival] = 0f;
                _finishSeconds[rival] = -1f;
            }
        }

        public void Advance(float deltaSeconds)
        {
            if (deltaSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            }

            _elapsedSeconds += deltaSeconds;
            for (int rival = 0; rival < RivalCount; rival++)
            {
                if (_finishSeconds[rival] >= 0f)
                {
                    continue;
                }

                RivalProfile profile = _profiles[rival];
                // Matches StageSession.UpdateSpeed's non-booster base speed.
                float progress = _gateDistances.Length == 0
                    ? 1f
                    : (float)_resolvedGates[rival] / _gateDistances.Length;
                float baseSpeed = _stage.SpeedProfile.IsLinear
                    ? _stage.StartingSpeed +
                        ((_stage.MaximumSpeed - _stage.StartingSpeed) * progress)
                    : _stage.GetBaseSpeed(progress);
                float speed = baseSpeed * profile.PaceFactor;
                if (_stumbleRemaining[rival] > 0f)
                {
                    speed *= profile.StumbleSpeedFactor;
                    _stumbleRemaining[rival] = Math.Max(
                        0f,
                        _stumbleRemaining[rival] - deltaSeconds);
                }
                _distances[rival] += speed * deltaSeconds;

                while (_resolvedGates[rival] < _gateDistances.Length &&
                    _distances[rival] >= _gateDistances[_resolvedGates[rival]])
                {
                    if (!_passes[rival, _resolvedGates[rival]])
                    {
                        _stumbleRemaining[rival] = profile.StumbleSeconds;
                    }
                    _resolvedGates[rival]++;
                }

                if (_distances[rival] >= GoalDistance)
                {
                    _distances[rival] = GoalDistance;
                    _finishSeconds[rival] = _elapsedSeconds;
                }
            }
        }

        public float GetDistance(int rival) => _distances[Check(rival)];
        public int GetResolvedGateCount(int rival) => _resolvedGates[Check(rival)];
        public bool IsStumbling(int rival) => _stumbleRemaining[Check(rival)] > 0f;
        public bool HasFinished(int rival) => _finishSeconds[Check(rival)] >= 0f;
        public float GetFinishSeconds(int rival) => _finishSeconds[Check(rival)];
        public RivalProfile GetProfile(int rival) => _profiles[Check(rival)];

        public float GetGateDistance(int gate)
        {
            if (gate < 0 || gate >= _gateDistances.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(gate));
            }
            return _gateDistances[gate];
        }

        public bool PassesGate(int rival, int gate)
        {
            if (gate < 0 || gate >= _gateDistances.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(gate));
            }
            return _passes[Check(rival), gate];
        }

        // 1-based place among the player and every rival. A finished racer
        // ranks by finish time; otherwise by distance. Ties favor the player.
        public int GetPlayerPlace(float playerDistance, float playerFinishSeconds)
        {
            bool playerFinished = playerFinishSeconds >= 0f;
            int place = 1;
            for (int rival = 0; rival < RivalCount; rival++)
            {
                bool rivalFinished = _finishSeconds[rival] >= 0f;
                bool ahead;
                if (rivalFinished && playerFinished)
                {
                    ahead = _finishSeconds[rival] < playerFinishSeconds;
                }
                else if (rivalFinished != playerFinished)
                {
                    ahead = rivalFinished;
                }
                else
                {
                    ahead = _distances[rival] > playerDistance;
                }
                if (ahead)
                {
                    place++;
                }
            }
            return place;
        }

        private static int Check(int rival)
        {
            if (rival < 0 || rival >= RivalCount)
            {
                throw new ArgumentOutOfRangeException(nameof(rival));
            }
            return rival;
        }
    }
}
