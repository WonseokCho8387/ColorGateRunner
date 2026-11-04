"""Deterministic calm lobby loop for Color Gate Runner.

A night-city synth pad in E minor at 84 BPM with a soft plucked arpeggio,
sub bass and sparse glass bells. The tail of the last bar is folded onto the
start, so the file loops without a seam. Standard library only.

    python Tools/Music/synth_lobby_music.py --out Artifacts/Music/Lobby.wav
"""

import argparse
import array
import math
import os
import random
import wave

SR = 44100
TWO_PI = 2.0 * math.pi
BPM = 84.0
BEAT = 60.0 / BPM
BAR = 4.0 * BEAT
BARS = 32
TAIL = 4.0

# Two bars per chord; MIDI notes of each voicing.
CHORDS = [
    (40, (52, 55, 59, 62, 66)),   # Em9
    (36, (52, 55, 59, 64, 67)),   # Cmaj7
    (43, (50, 55, 59, 62, 64)),   # G6
    (38, (50, 54, 57, 62, 64)),   # Dsus2/F#
]
PENTATONIC = [76, 79, 81, 83, 86, 88]


def hz(midi):
    return 440.0 * 2.0 ** ((midi - 69) / 12.0)


class Stereo:
    def __init__(self, seconds):
        self.n = int(seconds * SR)
        self.l = array.array('f', bytes(4 * self.n))
        self.r = array.array('f', bytes(4 * self.n))


def pad_note(bus, start, length, midi, vel):
    """Two detuned saws per side through a slow one-pole lowpass."""
    i0 = int(start * SR)
    n = min(int(length * SR), bus.n - i0)
    attack, release = 1.6 * SR, 1.8 * SR
    a = 1.0 - math.exp(-TWO_PI * 900.0 / SR)
    for side, buf, detunes in ((0, bus.l, (0.996, 1.003)),
                              (1, bus.r, (0.998, 1.005))):
        incs = [hz(midi) * d / SR for d in detunes]
        phases = [0.21 * side, 0.57]
        lp = 0.0
        for k in range(n):
            s = 0.0
            for j in (0, 1):
                ph = phases[j] + incs[j]
                if ph >= 1.0:
                    ph -= 1.0
                phases[j] = ph
                s += ph
            lp += a * ((s - 1.0) - lp)
            env = min(1.0, k / attack, (n - k) / release)
            buf[i0 + k] += lp * env * vel


def pluck(bus, start, midi, vel, pan):
    i0 = int(start * SR)
    n = min(int(0.9 * SR), bus.n - i0)
    inc = hz(midi) / SR
    ph = 0.0
    a = 1.0 - math.exp(-TWO_PI * 2200.0 / SR)
    lp = 0.0
    gl, gr = vel * (1.0 - pan), vel * (1.0 + pan)
    for k in range(n):
        ph += inc
        ph -= math.floor(ph)
        lp += a * ((4.0 * abs(ph - 0.5) - 1.0) - lp)
        env = math.exp(-(k / SR) / 0.28) * min(1.0, k / 60.0)
        s = lp * env
        bus.l[i0 + k] += s * gl
        bus.r[i0 + k] += s * gr


def bass(bus, start, length, midi, vel):
    i0 = int(start * SR)
    n = min(int(length * SR), bus.n - i0)
    inc = hz(midi) / SR
    ph = 0.0
    for k in range(n):
        ph += inc
        ph -= math.floor(ph)
        env = min(1.0, k / (0.08 * SR), (n - k) / (0.3 * SR))
        s = math.sin(TWO_PI * ph) * env * vel
        bus.l[i0 + k] += s
        bus.r[i0 + k] += s


def bell(bus, start, midi, vel, pan):
    i0 = int(start * SR)
    n = min(int(3.0 * SR), bus.n - i0)
    partials = [(hz(midi) * ratio / SR, gain, 1.1 / ratio ** 0.5)
                for ratio, gain in ((1.0, 1.0), (2.76, 0.3), (5.4, 0.1))]
    gl, gr = vel * (1.0 - pan), vel * (1.0 + pan)
    for k in range(n):
        t = k / SR
        s = 0.0
        for inc, gain, decay in partials:
            s += math.sin(TWO_PI * inc * k) * gain * math.exp(-t / decay)
        s *= min(1.0, k / 40.0)
        bus.l[i0 + k] += s * gl
        bus.r[i0 + k] += s * gr


def ping_pong(src, dst, delay_seconds, feedback, mix):
    d = int(delay_seconds * SR)
    l, r = src.l, src.r
    tl = array.array('f', l)
    tr = array.array('f', r)
    for i in range(d, src.n):
        tl[i] += tr[i - d] * feedback
        tr[i] += tl[i - d] * feedback
    for i in range(src.n):
        dst.l[i] += l[i] + (tl[i] - l[i]) * mix
        dst.r[i] += r[i] + (tr[i] - r[i]) * mix


def render(out_path):
    loop_seconds = BARS * BAR
    total = loop_seconds + TAIL
    pads, arps, low, bells = (Stereo(total) for _ in range(4))
    rng = random.Random(8484)

    for bar in range(BARS):
        root, voicing = CHORDS[(bar // 2) % len(CHORDS)]
        start = bar * BAR
        section = bar // 8
        if bar % 2 == 0:
            for note in voicing:
                pad_note(pads, start, 2.0 * BAR + 1.0, note, 0.05)
        # Arpeggio: eighth notes up and down the voicing, thinner in A.
        order = list(voicing) + list(voicing[-2:0:-1])
        for step in range(8):
            if section == 0 and step % 2 == 1:
                continue
            note = order[(bar * 8 + step) % len(order)] + 12
            pan = -0.35 if step % 2 == 0 else 0.35
            pluck(arps, start + step * BEAT / 2.0, note,
                  0.11 if step % 4 == 0 else 0.08, pan)
        if section in (1, 2):
            bass(low, start, BAR * 0.95, root - 12 if root > 38 else root,
                 0.22)
        if section == 2 and bar % 2 == 0:
            for beat in (0, 2.5):
                bell(bells, start + beat * BEAT, rng.choice(PENTATONIC),
                     0.06, rng.uniform(-0.5, 0.5))

    mix = Stereo(total)
    ping_pong(arps, mix, BEAT * 0.75, 0.38, 0.45)
    ping_pong(bells, mix, BEAT * 1.5, 0.45, 0.5)
    for src in (pads, low):
        for i in range(src.n):
            mix.l[i] += src.l[i]
            mix.r[i] += src.r[i]

    # Fold the ringing tail onto the start so the loop is seamless.
    loop_n = int(loop_seconds * SR)
    for i in range(loop_n, mix.n):
        mix.l[i - loop_n] += mix.l[i]
        mix.r[i - loop_n] += mix.r[i]

    peak = max(max(abs(x) for x in mix.l[:loop_n]),
               max(abs(x) for x in mix.r[:loop_n]))
    gain = 0.7 / peak
    pcm = array.array('h', bytes(4 * loop_n))
    for i in range(loop_n):
        pcm[2 * i] = int(32767 * mix.l[i] * gain)
        pcm[2 * i + 1] = int(32767 * mix.r[i] * gain)
    os.makedirs(os.path.dirname(out_path) or '.', exist_ok=True)
    with wave.open(out_path, 'wb') as handle:
        handle.setnchannels(2)
        handle.setsampwidth(2)
        handle.setframerate(SR)
        handle.writeframes(pcm.tobytes())
    return loop_seconds


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--out', default='Artifacts/Music/Lobby.wav')
    args = parser.parse_args()
    print('loop seconds %.3f' % render(args.out))


if __name__ == '__main__':
    main()
