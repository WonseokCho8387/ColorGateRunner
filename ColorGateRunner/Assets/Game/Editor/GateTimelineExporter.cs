using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using ColorGateRunner.Core;
using ColorGateRunner.Presentation;
using UnityEditor;
using UnityEngine;

namespace ColorGateRunner.Editor
{
    // Exports each Campaign Stage's nominal gate arrival times so music can
    // be composed to the authored gate rhythm. Time 0 is GO. The run is a
    // clean no-item StageSession advanced in fixed steps; Stage-provided
    // Shield/Booster grants still apply because they are part of the stage.
    public static class GateTimelineExporter
    {
        internal const float StepSeconds = 1f / 240f;
        private const int TicksPerQuarter = 480;
        private const int MicrosecondsPerQuarter = 500000; // 120 BPM
        private const int NoteTicks = 120;
        private const float MaximumRunSeconds = 600f;

        internal readonly struct GateTiming
        {
            public GateTiming(
                int gateIndex,
                float arrivalSeconds,
                float intervalSeconds,
                RunnerColor color,
                string modifier,
                GateOutcome outcome)
            {
                GateIndex = gateIndex;
                ArrivalSeconds = arrivalSeconds;
                IntervalSeconds = intervalSeconds;
                Color = color;
                Modifier = modifier;
                Outcome = outcome;
            }

            public int GateIndex { get; }
            public float ArrivalSeconds { get; }
            public float IntervalSeconds { get; }
            public RunnerColor Color { get; }
            public string Modifier { get; }
            public GateOutcome Outcome { get; }
        }

        [MenuItem("Tools/Color Gate Runner/Export Gate Timeline For Music")]
        public static void ExportAll()
        {
            StageCatalogProvider.EnsureConfigured();
            string folder = Path.GetFullPath(Path.Combine(
                Application.dataPath,
                "../../Artifacts/GateTimeline"));
            Directory.CreateDirectory(folder);
            for (int number = 1; number <= StageCatalog.Count; number++)
            {
                StageDefinition stage = StageCatalog.GetByDisplayNumber(number);
                IReadOnlyList<GateTiming> timeline = Build(stage);
                string name = $"Stage{number:00}";
                File.WriteAllText(
                    Path.Combine(folder, name + ".csv"),
                    ToCsv(timeline));
                File.WriteAllBytes(
                    Path.Combine(folder, name + ".mid"),
                    ToMidi(timeline));
            }
            Debug.Log($"Gate timeline exported to {folder}");
        }

        internal static IReadOnlyList<GateTiming> Build(StageDefinition stage)
        {
            if (stage == null)
            {
                throw new ArgumentNullException(nameof(stage));
            }
            var session = new StageSession(stage);
            session.BeginCountdown();
            session.CompleteCountdown();
            var timeline = new List<GateTiming>(stage.TargetGateCount);
            float distance = 0f;
            float gateDistance = StageSceneController.CampaignInitialGateLeadDistance;
            int plannedGate = -1;
            float previousArrival = 0f;
            GatePlan plan = default;
            while (session.GatesPassed < stage.TargetGateCount)
            {
                if (session.FlowState != StageFlowState.Playing &&
                    session.FlowState != StageFlowState.ShieldRecovery)
                {
                    throw new InvalidOperationException(
                        $"Stage {stage.DisplayNumber} left play at gate " +
                        $"{session.GatesPassed} ({session.FlowState}).");
                }
                if (plannedGate != session.GatesPassed)
                {
                    plannedGate = session.GatesPassed;
                    plan = session.AdjustForSafeTransition(
                        session.GetGatePlan(plannedGate));
                    gateDistance += plan.Spacing;
                }
                if (session.ElapsedPlayingSeconds > MaximumRunSeconds)
                {
                    throw new InvalidOperationException(
                        $"Stage {stage.DisplayNumber} did not finish.");
                }

                float speed = Math.Max(0.01f, session.GetSpeedForPlan(plan));
                float remaining = gateDistance - distance;
                float step = Math.Min(StepSeconds, remaining / speed);
                if (plan.Modifier.IsFlicker)
                {
                    session.UpdateFlickerGate(plan, remaining, step);
                }
                session.Advance(step, speed * step);
                distance += speed * step;
                if (distance < gateDistance - 0.0001f)
                {
                    continue;
                }

                float arrival = session.ElapsedPlayingSeconds;
                RunnerColor color = plan.Modifier.IsFlicker
                    ? session.UpdateFlickerGate(plan, 0f, 0f).CurrentColor
                    : plan.GetJudgmentColor(arrival);
                for (int tap = 0; tap < 3 && session.CurrentColor != color; tap++)
                {
                    session.TryToggleColor();
                }
                GateOutcome outcome = session.ResolveGate(plan, arrival);
                if (outcome == GateOutcome.Mismatched)
                {
                    throw new InvalidOperationException(
                        $"Stage {stage.DisplayNumber} gate {plannedGate} " +
                        "could not be matched.");
                }
                timeline.Add(new GateTiming(
                    plannedGate,
                    arrival,
                    arrival - previousArrival,
                    color,
                    DescribeModifier(plan.Modifier),
                    outcome));
                previousArrival = arrival;
                distance = gateDistance;
            }
            return timeline;
        }

        internal static string ToCsv(IReadOnlyList<GateTiming> timeline)
        {
            var csv = new StringBuilder();
            csv.AppendLine("gate,arrival_seconds,interval_seconds,local_bpm,color,modifier,outcome");
            foreach (GateTiming gate in timeline)
            {
                float bpm = gate.IntervalSeconds > 0f ? 60f / gate.IntervalSeconds : 0f;
                csv.Append(gate.GateIndex.ToString(CultureInfo.InvariantCulture)).Append(',')
                    .Append(gate.ArrivalSeconds.ToString("0.000", CultureInfo.InvariantCulture)).Append(',')
                    .Append(gate.IntervalSeconds.ToString("0.000", CultureInfo.InvariantCulture)).Append(',')
                    .Append(bpm.ToString("0.0", CultureInfo.InvariantCulture)).Append(',')
                    .Append(gate.Color).Append(',')
                    .Append(gate.Modifier).Append(',')
                    .Append(gate.Outcome).AppendLine();
            }
            return csv.ToString();
        }

        // Format 0 MIDI at a fixed 120 BPM: one note per gate, pitch by color,
        // so a DAW shows the gate rhythm at its real clock time.
        internal static byte[] ToMidi(IReadOnlyList<GateTiming> timeline)
        {
            var track = new List<byte>();
            WriteVariableLength(track, 0);
            track.AddRange(new byte[] { 0xFF, 0x51, 0x03 });
            track.Add((byte)((MicrosecondsPerQuarter >> 16) & 0xFF));
            track.Add((byte)((MicrosecondsPerQuarter >> 8) & 0xFF));
            track.Add((byte)(MicrosecondsPerQuarter & 0xFF));
            long lastTick = 0;
            double ticksPerSecond = TicksPerQuarter * 1000000.0 / MicrosecondsPerQuarter;
            foreach (GateTiming gate in timeline)
            {
                long tick = (long)Math.Round(gate.ArrivalSeconds * ticksPerSecond);
                // Track the real event clock so a gate closer than one note
                // length is delayed alone instead of shifting later notes.
                long onTick = Math.Max(tick, lastTick);
                byte pitch = GetPitch(gate.Color);
                WriteVariableLength(track, (int)(onTick - lastTick));
                track.AddRange(new byte[] { 0x90, pitch, 100 });
                WriteVariableLength(track, NoteTicks);
                track.AddRange(new byte[] { 0x80, pitch, 0 });
                lastTick = onTick + NoteTicks;
            }
            WriteVariableLength(track, 0);
            track.AddRange(new byte[] { 0xFF, 0x2F, 0x00 });

            var file = new List<byte>();
            file.AddRange(Encoding.ASCII.GetBytes("MThd"));
            file.AddRange(new byte[] { 0, 0, 0, 6, 0, 0, 0, 1 });
            file.Add((byte)(TicksPerQuarter >> 8));
            file.Add((byte)(TicksPerQuarter & 0xFF));
            file.AddRange(Encoding.ASCII.GetBytes("MTrk"));
            int length = track.Count;
            file.Add((byte)(length >> 24));
            file.Add((byte)(length >> 16));
            file.Add((byte)(length >> 8));
            file.Add((byte)length);
            file.AddRange(track);
            return file.ToArray();
        }

        private static byte GetPitch(RunnerColor color)
        {
            switch (color)
            {
                case RunnerColor.Red:
                    return 60;
                case RunnerColor.Blue:
                    return 64;
                default:
                    return 67;
            }
        }

        private static string DescribeModifier(GateModifier modifier)
        {
            if (modifier.IsNone) return "None";
            var names = new List<string>();
            if (modifier.IsFlicker) names.Add("Flicker");
            if (modifier.IsIce) names.Add("Ice");
            if (modifier.IsEchoProvider) names.Add("EchoProvider");
            if (modifier.IsHidden) names.Add("Hidden");
            if (modifier.IsCamouflage) names.Add("Camouflage");
            if (modifier.IsFog) names.Add("Fog");
            return string.Join("+", names);
        }

        private static void WriteVariableLength(List<byte> bytes, int value)
        {
            int buffer = value & 0x7F;
            while ((value >>= 7) > 0)
            {
                buffer <<= 8;
                buffer |= (value & 0x7F) | 0x80;
            }
            while (true)
            {
                bytes.Add((byte)buffer);
                if ((buffer & 0x80) == 0)
                {
                    break;
                }
                buffer >>= 8;
            }
        }
    }
}
