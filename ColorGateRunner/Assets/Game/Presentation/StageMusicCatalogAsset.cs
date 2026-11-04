using System;
using ColorGateRunner.Core;
using UnityEngine;

namespace ColorGateRunner.Presentation
{
    // One rendered track per Campaign Stage with the nominal gate map it was
    // composed to. Built by StageMusicCatalogBuilder; never edited by hand.
    public sealed class StageMusicCatalogAsset : ScriptableObject
    {
        [Serializable]
        internal sealed class Entry
        {
            [SerializeField] private int stageNumber;
            [SerializeField] private AudioClip clip;
            [SerializeField] private float[] gateDistances;
            [SerializeField] private float[] gateSeconds;

            internal Entry(
                int stageNumber,
                AudioClip clip,
                float[] gateDistances,
                float[] gateSeconds)
            {
                this.stageNumber = stageNumber;
                this.clip = clip;
                this.gateDistances = gateDistances;
                this.gateSeconds = gateSeconds;
            }

            internal int StageNumber => stageNumber;
            internal AudioClip Clip => clip;
            internal float[] GateDistances => gateDistances;
            internal float[] GateSeconds => gateSeconds;
        }

        [SerializeField] private Entry[] entries = Array.Empty<Entry>();

        internal int Count => entries.Length;

        internal void Configure(Entry[] value)
        {
            entries = value ?? Array.Empty<Entry>();
        }

        internal bool TryGet(
            int stageNumber,
            out AudioClip clip,
            out StageMusicTimeline timeline)
        {
            for (int index = 0; index < entries.Length; index++)
            {
                Entry entry = entries[index];
                if (entry != null && entry.StageNumber == stageNumber &&
                    entry.Clip != null)
                {
                    clip = entry.Clip;
                    timeline = new StageMusicTimeline(
                        entry.GateDistances,
                        entry.GateSeconds);
                    return true;
                }
            }
            clip = null;
            timeline = null;
            return false;
        }

        internal Entry GetEntry(int stageNumber)
        {
            for (int index = 0; index < entries.Length; index++)
            {
                if (entries[index] != null &&
                    entries[index].StageNumber == stageNumber)
                {
                    return entries[index];
                }
            }
            return null;
        }
    }
}
