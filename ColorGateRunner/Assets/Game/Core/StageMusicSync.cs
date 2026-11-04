using System;

namespace ColorGateRunner.Core
{
    // Maps runner path distance to the Stage track's clock. A track is
    // composed so each gate arrival of the nominal run lands on a bar
    // downbeat; between gates the map is linear in distance, so the same
    // distance always asks for the same musical moment whatever the speed.
    public sealed class StageMusicTimeline
    {
        private readonly float[] _distances;
        private readonly float[] _seconds;

        public StageMusicTimeline(float[] gateDistances, float[] gateSeconds)
        {
            if (gateDistances == null)
            {
                throw new ArgumentNullException(nameof(gateDistances));
            }
            if (gateSeconds == null)
            {
                throw new ArgumentNullException(nameof(gateSeconds));
            }
            if (gateDistances.Length == 0 ||
                gateDistances.Length != gateSeconds.Length)
            {
                throw new ArgumentException(
                    "Every gate needs one distance and one arrival time.");
            }

            // Point 0 is GO: distance 0 at track time 0.
            _distances = new float[gateDistances.Length + 1];
            _seconds = new float[gateSeconds.Length + 1];
            for (int index = 0; index < gateDistances.Length; index++)
            {
                if (gateDistances[index] <= _distances[index] ||
                    gateSeconds[index] <= _seconds[index])
                {
                    throw new ArgumentException(
                        "Gate distances and arrival times must increase.");
                }
                _distances[index + 1] = gateDistances[index];
                _seconds[index + 1] = gateSeconds[index];
            }
        }

        public int GateCount => _distances.Length - 1;
        public float FinalGateDistance => _distances[_distances.Length - 1];
        public float FinalGateSeconds => _seconds[_seconds.Length - 1];

        public float TimeAtDistance(float distance)
        {
            int segment = FindSegment(distance);
            float start = _distances[segment];
            return _seconds[segment] +
                (Math.Max(0f, distance) - start) *
                SecondsPerDistance(segment);
        }

        // Track seconds per metre at this distance; times the runner speed
        // it is the playback rate that keeps the track on the runner.
        public float SecondsPerDistanceAt(float distance)
        {
            return SecondsPerDistance(FindSegment(distance));
        }

        private float SecondsPerDistance(int segment)
        {
            return (_seconds[segment + 1] - _seconds[segment]) /
                (_distances[segment + 1] - _distances[segment]);
        }

        // Past the final gate the last segment's slope carries on.
        private int FindSegment(float distance)
        {
            int last = _distances.Length - 2;
            for (int segment = 0; segment < last; segment++)
            {
                if (distance < _distances[segment + 1])
                {
                    return segment;
                }
            }
            return last;
        }
    }

    // Playback rate that follows the runner. The feed-forward term plays at
    // the runner's own rate; the correction term closes any gap. The rate
    // is capped, so a Booster run plays fast and falls behind, then the
    // track keeps running fast after the Booster until it has caught up.
    public static class StageMusicSync
    {
        public const float MaximumPitch = 2f;
        public const float MinimumPitch = 0.7f;
        public const float CorrectionPerSecond = 1.5f;
        // A gap this large is not chased: the track jumps instead.
        public const float AheadResyncSeconds = 1f;
        public const float BehindResyncSeconds = 6f;

        public static float GetPitch(
            float targetSeconds,
            float playbackSeconds,
            float targetRate)
        {
            float pitch = targetRate +
                (targetSeconds - playbackSeconds) * CorrectionPerSecond;
            return Math.Max(MinimumPitch, Math.Min(MaximumPitch, pitch));
        }

        public static bool NeedsResync(
            float targetSeconds,
            float playbackSeconds)
        {
            float gap = targetSeconds - playbackSeconds;
            return gap < -AheadResyncSeconds || gap > BehindResyncSeconds;
        }
    }
}
