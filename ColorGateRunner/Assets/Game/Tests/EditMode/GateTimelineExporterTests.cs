using System.Collections.Generic;
using ColorGateRunner.Core;
using ColorGateRunner.Editor;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class GateTimelineExporterTests
    {
        [Test]
        public void EveryStage_ExportsOneIncreasingArrivalPerGate()
        {
            for (int number = 1; number <= StageCatalog.Count; number++)
            {
                StageDefinition stage = StageCatalog.GetByDisplayNumber(number);
                IReadOnlyList<GateTimelineExporter.GateTiming> timeline =
                    GateTimelineExporter.Build(stage);
                Assert.That(timeline.Count, Is.EqualTo(stage.TargetGateCount),
                    $"Stage {number}");
                float previous = 0f;
                for (int gate = 0; gate < timeline.Count; gate++)
                {
                    Assert.That(timeline[gate].GateIndex, Is.EqualTo(gate));
                    Assert.That(timeline[gate].ArrivalSeconds,
                        Is.GreaterThan(previous), $"Stage {number} gate {gate}");
                    Assert.That(timeline[gate].Outcome,
                        Is.Not.EqualTo(GateOutcome.Mismatched));
                    previous = timeline[gate].ArrivalSeconds;
                }
            }
        }

        [Test]
        public void Export_IsDeterministic()
        {
            StageDefinition stage = StageCatalog.GetByDisplayNumber(12);
            string first = GateTimelineExporter.ToCsv(GateTimelineExporter.Build(stage));
            string second = GateTimelineExporter.ToCsv(GateTimelineExporter.Build(stage));
            Assert.That(second, Is.EqualTo(first));
            Assert.That(first, Does.StartWith("gate,arrival_seconds,"));
        }

        [Test]
        public void Midi_HasHeaderTrackAndOneNotePerGate()
        {
            StageDefinition stage = StageCatalog.GetByDisplayNumber(3);
            IReadOnlyList<GateTimelineExporter.GateTiming> timeline =
                GateTimelineExporter.Build(stage);
            byte[] midi = GateTimelineExporter.ToMidi(timeline);
            Assert.That(System.Text.Encoding.ASCII.GetString(midi, 0, 4),
                Is.EqualTo("MThd"));
            Assert.That(System.Text.Encoding.ASCII.GetString(midi, 14, 4),
                Is.EqualTo("MTrk"));
            int noteOns = 0;
            for (int index = 22; index < midi.Length - 2; index++)
            {
                bool pitch = midi[index + 1] == 60 || midi[index + 1] == 64 ||
                    midi[index + 1] == 67;
                if (midi[index] == 0x90 && pitch && midi[index + 2] == 100)
                {
                    noteOns++;
                }
            }
            Assert.That(noteOns, Is.EqualTo(timeline.Count));
        }

        [Test]
        public void Midi_CloseGatesDoNotShiftLaterNotes()
        {
            var timeline = new[]
            {
                Gate(0, 1f),
                Gate(1, 1.05f),
                Gate(2, 2f)
            };
            byte[] midi = GateTimelineExporter.ToMidi(timeline);
            var noteOnTicks = new List<long>();
            int index = 22;
            long clock = 0;
            while (index < midi.Length)
            {
                long delta = 0;
                byte next;
                do
                {
                    next = midi[index++];
                    delta = (delta << 7) | (next & 0x7Fu);
                }
                while ((next & 0x80) != 0);
                clock += delta;
                byte status = midi[index];
                if (status == 0xFF)
                {
                    int length = midi[index + 2];
                    index += 3 + length;
                    continue;
                }
                if (status == 0x90)
                {
                    noteOnTicks.Add(clock);
                }
                index += 3;
            }
            Assert.That(noteOnTicks.Count, Is.EqualTo(3));
            Assert.That(noteOnTicks[0], Is.EqualTo(960));
            Assert.That(noteOnTicks[1], Is.EqualTo(960 + 120));
            Assert.That(noteOnTicks[2], Is.EqualTo(1920));
        }

        private static GateTimelineExporter.GateTiming Gate(int index, float seconds) =>
            new GateTimelineExporter.GateTiming(
                index,
                seconds,
                0.5f,
                RunnerColor.Red,
                "None",
                GateOutcome.Matched);
    }
}
