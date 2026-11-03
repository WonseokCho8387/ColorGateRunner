using System;
using ColorGateRunner.Core;
using UnityEngine;

namespace ColorGateRunner.Presentation
{
    // Gives each rival lane its own seeded sway and climb around the player's
    // route, so the side runners read as separate roads rather than copies.
    // Offsets are measured across the player's Spline; rival 0 runs left.
    internal sealed class RivalTrackLayout
    {
        internal const float StartLaneDistance = 9f;
        internal const float MinimumLaneDistance = 8.5f;
        internal const float MaximumLaneDistance = 16.5f;
        internal const float MaximumHeight = 4.5f;
        internal const float LaneHalfWidth = CampaignSplinePathView.TrackWidth * 0.5f;
        private const float CenterLaneDistance = 12.5f;
        private const float PrimarySway = 2.8f;
        private const float SecondarySway = 1.2f;
        private const float BlendDistance = 40f;
        private const float TwoPi = Mathf.PI * 2f;

        private readonly float[] _primaryWavelength = new float[RivalRaceSimulation.RivalCount];
        private readonly float[] _primaryPhase = new float[RivalRaceSimulation.RivalCount];
        private readonly float[] _secondaryWavelength = new float[RivalRaceSimulation.RivalCount];
        private readonly float[] _secondaryPhase = new float[RivalRaceSimulation.RivalCount];
        private readonly float[] _climbWavelength = new float[RivalRaceSimulation.RivalCount];
        private readonly float[] _climbPhase = new float[RivalRaceSimulation.RivalCount];

        internal RivalTrackLayout(uint stageSeed)
        {
            for (int rival = 0; rival < RivalRaceSimulation.RivalCount; rival++)
            {
                _primaryWavelength[rival] = Mathf.Lerp(110f, 170f, Hash01(stageSeed, rival, 3u));
                _primaryPhase[rival] = TwoPi * Hash01(stageSeed, rival, 5u);
                _secondaryWavelength[rival] = Mathf.Lerp(47f, 67f, Hash01(stageSeed, rival, 7u));
                _secondaryPhase[rival] = TwoPi * Hash01(stageSeed, rival, 11u);
                _climbWavelength[rival] = Mathf.Lerp(90f, 150f, Hash01(stageSeed, rival, 13u));
                _climbPhase[rival] = TwoPi * Hash01(stageSeed, rival, 17u);
            }
        }

        internal static float GetSide(int rival) => rival == 0 ? -1f : 1f;

        internal static int GetRivalOnSide(float side) => side < 0f ? 0 : 1;

        // Unsigned distance from the player's route centre to the lane centre.
        internal float GetLaneDistance(int rival, float distance)
        {
            ValidateRival(rival);
            float sway = CenterLaneDistance +
                (PrimarySway * Mathf.Sin((TwoPi * distance / _primaryWavelength[rival]) +
                    _primaryPhase[rival])) +
                (SecondarySway * Mathf.Sin((TwoPi * distance / _secondaryWavelength[rival]) +
                    _secondaryPhase[rival]));
            return Mathf.Lerp(StartLaneDistance, sway, Blend(distance));
        }

        internal float GetLateralOffset(int rival, float distance) =>
            GetSide(rival) * GetLaneDistance(rival, distance);

        internal float GetHeight(int rival, float distance)
        {
            ValidateRival(rival);
            float climb = 0.5f - (0.5f * Mathf.Cos(
                (TwoPi * distance / _climbWavelength[rival]) + _climbPhase[rival]));
            return MaximumHeight * climb * Blend(distance);
        }

        // Narrowest and widest lane distance across a stretch of route, used
        // to keep city towers off the lane for their whole footprint.
        internal void GetLaneDistanceRange(
            int rival,
            float fromDistance,
            float toDistance,
            out float minimum,
            out float maximum)
        {
            minimum = float.MaxValue;
            maximum = float.MinValue;
            const int Samples = 4;
            for (int sample = 0; sample <= Samples; sample++)
            {
                float lane = GetLaneDistance(
                    rival,
                    Mathf.Lerp(fromDistance, toDistance, sample / (float)Samples));
                minimum = Mathf.Min(minimum, lane);
                maximum = Mathf.Max(maximum, lane);
            }
        }

        private static float Blend(float distance)
        {
            return Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(distance / BlendDistance));
        }

        private static void ValidateRival(int rival)
        {
            if (rival < 0 || rival >= RivalRaceSimulation.RivalCount)
            {
                throw new ArgumentOutOfRangeException(nameof(rival));
            }
        }

        private static float Hash01(uint seed, int rival, uint salt)
        {
            uint value = seed ^ (salt * 0x27D4EB2Fu);
            value ^= (uint)(rival + 1) * 0x9E3779B9u;
            value ^= value >> 16;
            value *= 0x7FEB352Du;
            value ^= value >> 15;
            value *= 0x846CA68Bu;
            value ^= value >> 16;
            return (value & 0x00FFFFFFu) / 16777215f;
        }
    }
}
