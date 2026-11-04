"""Checks a rendered stage track: length, peak/RMS, clipping and how strongly
onsets line up with the Stage's gate arrival times.

    python Tools/Music/analyze_track.py Artifacts/Music/Stage01.wav \
        Artifacts/GateTimeline/Stage01.csv
"""

import array
import csv
import math
import sys
import wave

WINDOW = 0.01


def main(wav_path, csv_path):
    with wave.open(wav_path, 'rb') as handle:
        rate = handle.getframerate()
        frames = handle.getnframes()
        pcm = array.array('h', handle.readframes(frames))
    mono = [(pcm[2 * i] + pcm[2 * i + 1]) / 65536.0 for i in range(frames)]
    peak = max(abs(x) for x in mono)
    rms = math.sqrt(sum(x * x for x in mono) / frames)
    clipped = sum(1 for x in pcm if abs(x) >= 32760)

    hop = int(WINDOW * rate)
    energy = []
    for start in range(0, frames - hop, hop):
        chunk = mono[start:start + hop]
        energy.append(math.sqrt(sum(x * x for x in chunk) / hop))
    flux = [0.0] + [max(0.0, energy[i] - energy[i - 1])
                    for i in range(1, len(energy))]
    mean_flux = sum(flux) / len(flux)

    with open(csv_path, newline='') as handle:
        gates = [float(row['arrival_seconds'])
                 for row in csv.DictReader(handle)]

    def strongest_near(t, radius=0.03):
        center = int(t / WINDOW)
        lo, hi = max(0, center - int(radius / WINDOW)), center + \
            int(radius / WINDOW) + 1
        return max(flux[lo:hi]) if lo < len(flux) else 0.0

    on_gate = [strongest_near(t) for t in gates]
    off_gate = [strongest_near((a + b) / 2 + 0.07)
                for a, b in zip(gates, gates[1:])]
    hits = sum(1 for v in on_gate if v > 2.0 * mean_flux)

    sections = []
    for name, lo, hi in (('intro', 0.0, gates[0]),
                         ('first third', gates[0], gates[len(gates) // 3]),
                         ('last third', gates[2 * len(gates) // 3],
                          gates[-1])):
        a, b = int(lo * rate), int(hi * rate)
        seg = mono[a:b]
        sections.append('%s rms %.3f' % (
            name, math.sqrt(sum(x * x for x in seg) / max(1, len(seg)))))

    print('length %.2fs, peak %.3f, rms %.3f, clipped samples %d' % (
        frames / rate, peak, rms, clipped))
    print('gate onsets above 2x mean flux: %d / %d' % (hits, len(gates)))
    print('mean onset strength on gate %.4f vs mid-bar %.4f' % (
        sum(on_gate) / len(on_gate), sum(off_gate) / len(off_gate)))
    print(', '.join(sections))


if __name__ == '__main__':
    main(sys.argv[1], sys.argv[2])
