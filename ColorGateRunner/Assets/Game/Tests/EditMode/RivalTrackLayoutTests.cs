using ColorGateRunner.Core;
using ColorGateRunner.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class RivalTrackLayoutTests
    {
        [Test]
        public void Layout_IsDeterministicPerSeed()
        {
            var first = new RivalTrackLayout(1234u);
            var second = new RivalTrackLayout(1234u);
            for (float distance = 0f; distance < 600f; distance += 7f)
            {
                for (int rival = 0; rival < RivalRaceSimulation.RivalCount; rival++)
                {
                    Assert.That(second.GetLateralOffset(rival, distance),
                        Is.EqualTo(first.GetLateralOffset(rival, distance)));
                    Assert.That(second.GetHeight(rival, distance),
                        Is.EqualTo(first.GetHeight(rival, distance)));
                }
            }
        }

        [Test]
        public void Lanes_StartBesideThePlayerOnOppositeSides()
        {
            var layout = new RivalTrackLayout(77u);
            Assert.That(layout.GetLateralOffset(0, 0f),
                Is.EqualTo(-RivalTrackLayout.StartLaneDistance).Within(0.0001f));
            Assert.That(layout.GetLateralOffset(1, 0f),
                Is.EqualTo(RivalTrackLayout.StartLaneDistance).Within(0.0001f));
            Assert.That(layout.GetHeight(0, 0f), Is.Zero.Within(0.0001f));
            Assert.That(layout.GetHeight(1, 0f), Is.Zero.Within(0.0001f));
        }

        [Test]
        public void Lanes_StayInsideTheirBandAndNeverTouchThePlayerRoad()
        {
            float roadClear = CampaignSplinePathView.TrackWidth;
            foreach (uint seed in new[] { 1u, 42u, 9001u, 0xDEADBEEFu })
            {
                var layout = new RivalTrackLayout(seed);
                for (float distance = 0f; distance < 1200f; distance += 1.5f)
                {
                    for (int rival = 0; rival < RivalRaceSimulation.RivalCount; rival++)
                    {
                        float lane = layout.GetLaneDistance(rival, distance);
                        Assert.That(lane, Is.InRange(
                            RivalTrackLayout.MinimumLaneDistance - 0.0001f,
                            RivalTrackLayout.MaximumLaneDistance + 0.0001f));
                        Assert.That(lane, Is.GreaterThan(roadClear));
                        Assert.That(layout.GetHeight(rival, distance), Is.InRange(
                            0f,
                            RivalTrackLayout.MaximumHeight + 0.0001f));
                    }
                }
            }
        }

        [Test]
        public void Rivals_SwayAndClimbDifferentlyFromEachOther()
        {
            var layout = new RivalTrackLayout(2026u);
            float lateralGap = 0f;
            float heightGap = 0f;
            float minimumLane = float.MaxValue;
            float maximumLane = float.MinValue;
            float maximumHeight = 0f;
            for (float distance = 40f; distance < 640f; distance += 2f)
            {
                float left = layout.GetLaneDistance(0, distance);
                float right = layout.GetLaneDistance(1, distance);
                lateralGap = Mathf.Max(lateralGap, Mathf.Abs(left - right));
                heightGap = Mathf.Max(heightGap,
                    Mathf.Abs(layout.GetHeight(0, distance) -
                        layout.GetHeight(1, distance)));
                minimumLane = Mathf.Min(minimumLane, left);
                maximumLane = Mathf.Max(maximumLane, left);
                maximumHeight = Mathf.Max(maximumHeight, layout.GetHeight(0, distance));
            }
            Assert.That(lateralGap, Is.GreaterThan(2f));
            Assert.That(heightGap, Is.GreaterThan(1f));
            Assert.That(maximumLane - minimumLane, Is.GreaterThan(4f));
            Assert.That(maximumHeight, Is.GreaterThan(3f));
        }
    }
}
