using System;
using System.Collections.Generic;
using ColorGateRunner.Core;
using ColorGateRunner.Editor;
using ColorGateRunner.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class StageMusicTests
    {
        // Gates every 10 m, one second apart: a 10 m/s composed run.
        private static StageMusicTimeline Uniform(int gates)
        {
            var distances = new float[gates];
            var seconds = new float[gates];
            for (int index = 0; index < gates; index++)
            {
                distances[index] = 10f * (index + 1);
                seconds[index] = index + 1f;
            }
            return new StageMusicTimeline(distances, seconds);
        }

        [Test]
        public void Timeline_MapsGoAndGatesAndIsLinearBetween()
        {
            var timeline = new StageMusicTimeline(
                new[] { 48f, 60f, 70f },
                new[] { 4f, 5f, 5.5f });

            Assert.That(timeline.TimeAtDistance(0f), Is.Zero);
            Assert.That(timeline.TimeAtDistance(24f), Is.EqualTo(2f).Within(1e-5f));
            Assert.That(timeline.TimeAtDistance(48f), Is.EqualTo(4f).Within(1e-5f));
            Assert.That(timeline.TimeAtDistance(54f), Is.EqualTo(4.5f).Within(1e-5f));
            Assert.That(timeline.TimeAtDistance(70f), Is.EqualTo(5.5f).Within(1e-5f));
            Assert.That(timeline.TimeAtDistance(80f), Is.EqualTo(6f).Within(1e-5f));
            Assert.That(timeline.SecondsPerDistanceAt(65f),
                Is.EqualTo(0.05f).Within(1e-6f));
            Assert.That(timeline.FinalGateDistance, Is.EqualTo(70f));
            Assert.That(timeline.FinalGateSeconds, Is.EqualTo(5.5f));
        }

        [Test]
        public void Timeline_RejectsMismatchedOrNonIncreasingGates()
        {
            Assert.Throws<ArgumentException>(() =>
                new StageMusicTimeline(new[] { 10f }, new[] { 1f, 2f }));
            Assert.Throws<ArgumentException>(() =>
                new StageMusicTimeline(new[] { 10f, 10f }, new[] { 1f, 2f }));
            Assert.Throws<ArgumentException>(() =>
                new StageMusicTimeline(new[] { 10f, 20f }, new[] { 2f, 1f }));
        }

        [Test]
        public void Sync_PlaysAtRunnerRateAndCapsBoosterFastForward()
        {
            Assert.That(StageMusicSync.GetPitch(3f, 3f, 1f), Is.EqualTo(1f));
            Assert.That(StageMusicSync.GetPitch(3f, 3f, 1.6f),
                Is.EqualTo(1.6f).Within(1e-6f));
            Assert.That(StageMusicSync.GetPitch(3f, 3f, 2.6f),
                Is.EqualTo(StageMusicSync.MaximumPitch));
            Assert.That(StageMusicSync.GetPitch(3f, 2.5f, 1f),
                Is.GreaterThan(1f));
            Assert.That(StageMusicSync.GetPitch(3f, 3.5f, 1f),
                Is.LessThan(1f));
            Assert.That(StageMusicSync.GetPitch(3f, 3f, 0f),
                Is.EqualTo(StageMusicSync.MinimumPitch));
            Assert.That(StageMusicSync.NeedsResync(3f, 3f), Is.False);
            Assert.That(StageMusicSync.NeedsResync(3f, 4.5f), Is.True);
            Assert.That(StageMusicSync.NeedsResync(8f, 3f), Is.False);
            Assert.That(StageMusicSync.NeedsResync(10f, 3f), Is.True);
        }

        [Test]
        public void Sync_BoosterRunFallsBehindThenCatchesUpOnTheBeat()
        {
            StageMusicTimeline timeline = Uniform(60);
            float distance = 0f;
            float playback = 0f;
            float maximumLag = 0f;
            const float step = 1f / 60f;
            for (int frame = 0; frame < 60 * 40; frame++)
            {
                // Booster at 25 m/s for the first 100 m, then the composed
                // 10 m/s run.
                float speed = distance < 100f ? 25f : 10f;
                float target = timeline.TimeAtDistance(distance);
                float pitch = StageMusicSync.GetPitch(
                    target,
                    playback,
                    timeline.SecondsPerDistanceAt(distance) * speed);
                Assert.That(pitch, Is.LessThanOrEqualTo(
                    StageMusicSync.MaximumPitch));
                Assert.That(StageMusicSync.NeedsResync(target, playback),
                    Is.False, $"frame {frame}");
                maximumLag = Math.Max(maximumLag, target - playback);
                playback += pitch * step;
                distance += speed * step;
            }

            Assert.That(maximumLag, Is.GreaterThan(1f),
                "A 2.5x Booster must outrun the 2x fast-forward.");
            float finalGap = timeline.TimeAtDistance(distance) - playback;
            Assert.That(Math.Abs(finalGap), Is.LessThan(0.02f));
        }

        [Test]
        public void Catalog_HasOneStreamingTrackPerStageOnTheExportedGateMap()
        {
            StageMusicCatalogAsset catalog =
                AssetDatabase.LoadAssetAtPath<StageMusicCatalogAsset>(
                    StageMusicCatalogBuilder.AssetPath);
            Assert.That(catalog, Is.Not.Null);
            Assert.That(catalog.Count, Is.EqualTo(StageCatalog.Count));
            for (int number = 1; number <= StageCatalog.Count; number++)
            {
                StageMusicCatalogAsset.Entry entry = catalog.GetEntry(number);
                Assert.That(entry, Is.Not.Null, $"Stage {number}");
                Assert.That(entry.Clip, Is.Not.Null, $"Stage {number}");
                Assert.That(AssetDatabase.GetAssetPath(entry.Clip),
                    Is.EqualTo(StageMusicCatalogBuilder.GetClipPath(number)));

                IReadOnlyList<GateTimelineExporter.GateTiming> exported =
                    GateTimelineExporter.Build(
                        StageCatalog.GetByDisplayNumber(number));
                Assert.That(entry.GateSeconds.Length,
                    Is.EqualTo(exported.Count), $"Stage {number}");
                for (int gate = 0; gate < exported.Count; gate++)
                {
                    Assert.That(entry.GateDistances[gate],
                        Is.EqualTo(exported[gate].PathDistance),
                        $"Stage {number} gate {gate}");
                    Assert.That(entry.GateSeconds[gate],
                        Is.EqualTo(exported[gate].ArrivalSeconds),
                        $"Stage {number} gate {gate}");
                }
                // Two outro bars and the final chord follow the last gate.
                Assert.That(entry.Clip.length, Is.GreaterThan(
                    exported[exported.Count - 1].ArrivalSeconds + 2f),
                    $"Stage {number}");

                var importer = (AudioImporter)AssetImporter.GetAtPath(
                    StageMusicCatalogBuilder.GetClipPath(number));
                AudioImporterSampleSettings settings =
                    importer.defaultSampleSettings;
                Assert.That(settings.loadType,
                    Is.EqualTo(AudioClipLoadType.Streaming));
                Assert.That(settings.compressionFormat,
                    Is.EqualTo(AudioCompressionFormat.Vorbis));
                Assert.That(settings.preloadAudioData, Is.False);
            }
        }
    }
}
