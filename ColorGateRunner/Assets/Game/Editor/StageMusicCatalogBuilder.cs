using System;
using System.Collections.Generic;
using ColorGateRunner.Core;
using ColorGateRunner.Presentation;
using UnityEditor;
using UnityEngine;

namespace ColorGateRunner.Editor
{
    // Pairs each rendered Stage track (Tools/Music/synth_stage_music.py) with
    // the nominal gate map it was composed to, taken from the same
    // GateTimelineExporter run, and keeps the music import settings.
    public static class StageMusicCatalogBuilder
    {
        public const string MusicFolder = "Assets/Game/Audio/Music";
        public const string AssetPath = "Assets/Game/Audio/StageMusicCatalog.asset";
        internal const float VorbisQuality = 0.5f;

        [MenuItem("Tools/Color Gate Runner/Build Stage Music Catalog")]
        public static StageMusicCatalogAsset EnsureAndConfigure()
        {
            StageCatalogAssetBuilder.EnsureAndConfigure();
            var entries = new List<StageMusicCatalogAsset.Entry>(
                StageCatalog.Count);
            for (int number = 1; number <= StageCatalog.Count; number++)
            {
                string clipPath = GetClipPath(number);
                ConfigureImporter(clipPath);
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(
                    clipPath);
                if (clip == null)
                {
                    throw new InvalidOperationException(
                        $"Stage music is missing: {clipPath}");
                }

                IReadOnlyList<GateTimelineExporter.GateTiming> timeline =
                    GateTimelineExporter.Build(
                        StageCatalog.GetByDisplayNumber(number));
                var distances = new float[timeline.Count];
                var seconds = new float[timeline.Count];
                for (int gate = 0; gate < timeline.Count; gate++)
                {
                    distances[gate] = timeline[gate].PathDistance;
                    seconds[gate] = timeline[gate].ArrivalSeconds;
                }
                entries.Add(new StageMusicCatalogAsset.Entry(
                    number,
                    clip,
                    distances,
                    seconds));
            }

            StageMusicCatalogAsset asset =
                AssetDatabase.LoadAssetAtPath<StageMusicCatalogAsset>(
                    AssetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<StageMusicCatalogAsset>();
                AssetDatabase.CreateAsset(asset, AssetPath);
            }
            asset.Configure(entries.ToArray());
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            return asset;
        }

        internal static string GetClipPath(int stageNumber)
        {
            return $"{MusicFolder}/Stage{stageNumber:00}.wav";
        }

        // Streams compressed so a Stage never holds 26 decoded tracks.
        private static void ConfigureImporter(string clipPath)
        {
            if (!(AssetImporter.GetAtPath(clipPath) is AudioImporter importer))
            {
                return;
            }

            AudioImporterSampleSettings settings =
                importer.defaultSampleSettings;
            bool changed =
                settings.loadType != AudioClipLoadType.Streaming ||
                settings.compressionFormat != AudioCompressionFormat.Vorbis ||
                !Mathf.Approximately(settings.quality, VorbisQuality) ||
                settings.preloadAudioData ||
                !importer.loadInBackground ||
                importer.forceToMono;
            if (!changed)
            {
                return;
            }
            settings.loadType = AudioClipLoadType.Streaming;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = VorbisQuality;
            settings.preloadAudioData = false;
            importer.defaultSampleSettings = settings;
            importer.loadInBackground = true;
            importer.forceToMono = false;
            importer.SaveAndReimport();
        }
    }
}
