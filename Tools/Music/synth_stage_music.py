"""Deterministic hard-rock stage music synthesizer for Color Gate Runner.

Each Campaign Stage track is fitted to that Stage's exported gate timeline
(Artifacts/GateTimeline/StageNN.csv): every gate arrival lands on a bar
downbeat, so the tempo accelerates with the Stage. Eight chapter themes share
one arrangement engine; a Stage selects its chapter theme and tempo map.

Standard library only. The same inputs always render the same samples.

    python Tools/Music/synth_stage_music.py --stage 1 --out Artifacts/Music
"""

import argparse
import array
import csv
import math
import os
import random
import wave

SR = 44100
TWO_PI = 2.0 * math.pi

# Steps per bar; one bar spans one gate interval.
STEPS = 16

# Riff symbols: semitone offset from the bar's chord root and articulation.
RIFF_NOTES = {
    'm': (0, 'mute'),
    'o': (0, 'open'),
    'b': (1, 'single'),
    'n': (3, 'single'),
    'f': (5, 'single'),
    'v': (7, 'single'),
    'x': (10, 'single'),
}

MINOR_PENT = [0, 3, 5, 7, 10]
NATURAL_MINOR = [0, 2, 3, 5, 7, 8, 10]
HARMONIC_MINOR = [0, 2, 3, 5, 7, 8, 11]
PHRYGIAN_DOMINANT = [0, 1, 4, 5, 7, 8, 10]
DORIAN = [0, 2, 3, 5, 7, 9, 10]

# Chapter themes. key is the power-chord root MIDI note (E2 = 40).
THEMES = {
    'basics': dict(
        key=40, seed=101, lead_scale=MINOR_PENT,
        progression=dict(intro=[0], A=[0, 0, 8, 10], B=[0, 5, 8, 10],
                         C=[8, 10, 0, 0], D=[0, 10, 8, 10]),
        riff=dict(A='m.mmm.mmm.mmo---', B='o---m.mmo---m.mm',
                  C='o-------o---o---', D='mmmmmmmmo---mmmm'),
    ),
    'items': dict(
        key=45, seed=202, lead_scale=NATURAL_MINOR,
        progression=dict(intro=[0], A=[0, 3, 5, 3], B=[0, 8, 3, 10],
                         C=[5, 3, 8, 10], D=[0, 3, 5, 7]),
        riff=dict(A='m.mm.mm.m.mmo-m.', B='o---mmm.o---mmm.',
                  C='o-------o-------', D='mmm.mmm.mmmmo---'),
    ),
    'camouflage': dict(
        key=39, seed=303, lead_scale=HARMONIC_MINOR,
        progression=dict(intro=[0], A=[0, 0, 1, 0], B=[0, 6, 5, 1],
                         C=[8, 7, 6, 7], D=[0, 1, 0, 6]),
        riff=dict(A='m.m.mbm.m.m.mnm.', B='m.mbm.mfo---m.mb',
                  C='o-----o-----o---', D='mbmbmbmbmnmnmvo-'),
    ),
    'fog': dict(
        key=37, seed=404, lead_scale=NATURAL_MINOR,
        progression=dict(intro=[0], A=[0, 8, 3, 10], B=[0, 8, 5, 7],
                         C=[8, 10, 3, 7], D=[0, 8, 10, 7]),
        riff=dict(A='o-------m.m.m.m.', B='o---m.m.o---m.mm',
                  C='o---------------', D='mmmmo---mmmmo---'),
    ),
    'ice': dict(
        key=42, seed=505, lead_scale=DORIAN,
        progression=dict(intro=[0], A=[0, 10, 8, 10], B=[0, 3, 10, 5],
                         C=[8, 10, 0, 3], D=[0, 10, 8, 7]),
        riff=dict(A='m.mvm.mxm.mvo---', B='o-m.o-m.o-m.mmmm',
                  C='o-------o---o-o-', D='mvmxmvmxmvmxo---'),
    ),
    'echo': dict(
        key=47, seed=606, lead_scale=NATURAL_MINOR,
        progression=dict(intro=[0], A=[0, 0, 8, 7], B=[0, 3, 8, 10],
                         C=[3, 10, 8, 7], D=[0, 3, 5, 7]),
        riff=dict(A='m.mm.mm.m.mm.mm.', B='o--m.mo--m.mo---',
                  C='o-------o-------', D='mmmmmmmmmmmmo-o-'),
    ),
    'hidden': dict(
        key=43, seed=707, lead_scale=HARMONIC_MINOR,
        progression=dict(intro=[0], A=[0, 1, 0, 8], B=[0, 8, 7, 1],
                         C=[5, 7, 8, 7], D=[0, 1, 8, 7]),
        riff=dict(A='o-..m.m.o-..m.mb', B='m.m.m...o-..o-..',
                  C='o-------o---....', D='mmmm....mmmmo---'),
    ),
    'flicker': dict(
        key=40, seed=808, lead_scale=PHRYGIAN_DOMINANT,
        progression=dict(intro=[0], A=[0, 1, 0, 1], B=[0, 1, 4, 1],
                         C=[8, 7, 5, 4], D=[0, 1, 0, 4]),
        riff=dict(A='mbmbm.mbmbm.mno-', B='mmbmmbmmbmmbo---',
                  C='o-----o-----o-o-', D='mbmbmbmbmbmbmbo-'),
    ),
}

DRUMS = dict(
    intro=dict(kick='k...............', snare='................',
               hat='h...h...h...h...'),
    A=dict(kick='k.....k.k.......', snare='....s.......s...',
           hat='h.h.h.h.h.h.h.h.'),
    B=dict(kick='k.k...k.k.k...k.', snare='....s.......s...',
           hat='h.h.h.h.h.h.h.h.'),
    C=dict(kick='kkkkkkkkkkkkkkkk', snare='....s.......s...',
           hat='r...r...r...r...'),
    D=dict(kick='kkkkkkkkkkkkkkkk', snare='....s.......s.s.',
           hat='h.h.h.h.h.h.h.h.'),
    outro=dict(kick='k...k...k...k...', snare='....s.......s...',
               hat='................'),
)


def stage_chapter(stage):
    if stage <= 5:
        return 'basics'
    if stage <= 8:
        return 'items'
    return ['camouflage', 'fog', 'ice', 'echo', 'hidden', 'flicker'][
        (stage - 9) // 3]


def midi_hz(note):
    return 440.0 * 2.0 ** ((note - 69) / 12.0)


def one_pole(cutoff):
    return 1.0 - math.exp(-TWO_PI * cutoff / SR)


class Bus:
    def __init__(self, seconds):
        self.n = int(seconds * SR) + SR
        self.buf = array.array('f', bytes(4 * self.n))

    def lowpass(self, cutoff, poles=1):
        a = one_pole(cutoff)
        buf = self.buf
        for _ in range(poles):
            y = 0.0
            for i in range(self.n):
                y += a * (buf[i] - y)
                buf[i] = y

    def highpass(self, cutoff):
        a = one_pole(cutoff)
        buf = self.buf
        y = 0.0
        for i in range(self.n):
            x = buf[i]
            y += a * (x - y)
            buf[i] = x - y


def load_bars(csv_path):
    with open(csv_path, newline='') as handle:
        rows = list(csv.DictReader(handle))
    arrivals = [float(row['arrival_seconds']) for row in rows]
    first = arrivals[1] - arrivals[0]
    lead = max(1, round(arrivals[0] / first))
    bars = [arrivals[0] * i / lead for i in range(lead)] + arrivals
    last = arrivals[-1] - arrivals[-2]
    bars += [arrivals[-1] + last, arrivals[-1] + 2.0 * last]
    return bars, lead, len(arrivals)


def section_of(bar, lead, gates):
    if bar < lead:
        return 'intro'
    gate = bar - lead
    if gate >= gates:
        return 'outro'
    fraction = gate / gates
    if fraction < 0.30:
        return 'A'
    if fraction < 0.55:
        return 'B'
    if fraction < 0.80:
        return 'C'
    return 'D'


def step_time(bars, bar, step):
    start = bars[bar]
    return start + (bars[bar + 1] - start) * step / STEPS


# ---------------------------------------------------------------- voices

def render_guitar(bus, start, end, root, style, vel, detune):
    if style == 'open':
        notes, cutoff, gain, tau = (0, 7, 12), 3200.0, 9.0, 1.6
    elif style == 'mute':
        notes, cutoff, gain, tau = (0, 7), 750.0, 5.5, 0.06
    else:
        notes, cutoff, gain, tau = (0, 12), 1400.0, 6.5, 0.09
    freqs = [midi_hz(root + offset) * detune for offset in notes]
    incs = [f / SR for f in freqs]
    phases = [0.13 * k for k in range(len(freqs))]
    a = one_pole(cutoff)
    i0 = int(start * SR)
    length = int(max(0.02, end - start) * SR)
    release = int(0.018 * SR)
    attack = int(0.003 * SR)
    decay = math.exp(-1.0 / (tau * SR))
    norm = 1.0 / len(freqs)
    buf = bus.buf
    lp = 0.0
    env = 1.0
    total = min(length + release, bus.n - i0)
    count = len(freqs)
    for k in range(total):
        s = 0.0
        for j in range(count):
            ph = phases[j] + incs[j]
            if ph >= 1.0:
                ph -= 1.0
            phases[j] = ph
            s += ph
        s = (2.0 * s * norm) - 1.0
        lp += a * (s - lp)
        if k < attack:
            amp = k / attack
        else:
            env *= decay
            amp = env
        if k >= length:
            amp *= 1.0 - (k - length) / release
        buf[i0 + k] += math.tanh(gain * lp) * amp * vel


def render_bass(bus, start, end, note, vel, muted):
    f = midi_hz(note) / SR
    a = one_pole(320.0)
    i0 = int(start * SR)
    length = int(max(0.02, end - start) * SR)
    release = int(0.02 * SR)
    decay = math.exp(-1.0 / ((0.12 if muted else 0.5) * SR))
    buf = bus.buf
    ph = 0.0
    lp = 0.0
    env = 1.0
    for k in range(min(length + release, bus.n - i0)):
        ph += f
        if ph >= 1.0:
            ph -= 1.0
        lp += a * ((2.0 * ph - 1.0) - lp)
        env *= decay
        amp = env if k < length else env * (1.0 - (k - length) / release)
        buf[i0 + k] += math.tanh(2.2 * lp) * amp * vel


def render_lead(bus, start, end, note, vel):
    base = midi_hz(note)
    a = one_pole(2600.0)
    i0 = int(start * SR)
    length = int(max(0.03, end - start) * SR)
    release = int(0.03 * SR)
    attack = int(0.004 * SR)
    glide = int(0.025 * SR)
    decay = math.exp(-1.0 / (2.5 * SR))
    buf = bus.buf
    ph = 0.0
    lp = 0.0
    env = 1.0
    for k in range(min(length + release, bus.n - i0)):
        t = k / SR
        bend = 0.97 + 0.03 * (k / glide) if k < glide else 1.0
        vib = 1.0 + 0.006 * min(1.0, t / 0.25) * math.sin(TWO_PI * 5.6 * t)
        ph += base * bend * vib / SR
        if ph >= 1.0:
            ph -= 1.0
        lp += a * ((2.0 * ph - 1.0) - lp)
        if k < attack:
            amp = k / attack
        else:
            env *= decay
            amp = env
        if k >= length:
            amp *= 1.0 - (k - length) / release
        buf[i0 + k] += math.tanh(10.0 * lp) * amp * vel


class Noise:
    def __init__(self, seed):
        rng = random.Random(seed)
        self.table = array.array('f', (rng.uniform(-1.0, 1.0)
                                       for _ in range(SR)))
        self.cursor = 0

    def take(self):
        self.cursor = (self.cursor + 7919) % SR
        return self.cursor


def render_drum(bus, noise, start, kind, vel):
    i0 = int(start * SR)
    table = noise.table
    offset = noise.take()
    buf = bus.buf
    if kind == 'kick':
        n = int(0.32 * SR)
        ph = 0.0
        for k in range(min(n, bus.n - i0)):
            t = k / SR
            ph += (48.0 + 110.0 * math.exp(-t * 35.0)) / SR
            s = math.sin(TWO_PI * ph) * math.exp(-t * 9.0)
            if k < 120:
                s += table[(offset + k) % SR] * 0.5 * (1.0 - k / 120.0)
            buf[i0 + k] += math.tanh(1.6 * s) * vel
    elif kind == 'snare':
        n = int(0.28 * SR)
        hp = 0.0
        a = one_pole(1200.0)
        for k in range(min(n, bus.n - i0)):
            t = k / SR
            x = table[(offset + k) % SR]
            hp += a * (x - hp)
            body = math.sin(TWO_PI * 188.0 * t) * math.exp(-t * 22.0)
            buf[i0 + k] += ((x - hp) * math.exp(-t * 13.0) * 0.9 +
                            body * 0.6) * vel
    elif kind in ('hat', 'ride', 'crash'):
        tau, cutoff, n = {'hat': (0.035, 7000.0, 0.09),
                          'ride': (0.25, 5000.0, 0.6),
                          'crash': (0.9, 3800.0, 2.2)}[kind]
        n = int(n * SR)
        a = one_pole(cutoff)
        lp = 0.0
        decay = math.exp(-1.0 / (tau * SR))
        env = 1.0
        for k in range(min(n, bus.n - i0)):
            x = table[(offset + k) % SR]
            lp += a * (x - lp)
            env *= decay
            buf[i0 + k] += (x - lp) * env * vel
    elif kind == 'tom':
        n = int(0.3 * SR)
        ph = 0.0
        for k in range(min(n, bus.n - i0)):
            t = k / SR
            ph += (95.0 + 60.0 * math.exp(-t * 20.0)) / SR
            buf[i0 + k] += math.sin(TWO_PI * ph) * math.exp(-t * 10.0) * vel


# ------------------------------------------------------------ arranging

def riff_events(pattern):
    """Yields (step, length_in_steps, offset, style) for one bar."""
    events = []
    step = 0
    while step < STEPS:
        symbol = pattern[step]
        if symbol in RIFF_NOTES:
            length = 1
            while (step + length < STEPS and
                   pattern[step + length] == '-'):
                length += 1
            offset, style = RIFF_NOTES[symbol]
            events.append((step, length, offset, style))
            step += length
        else:
            step += 1
    return events


def lead_phrase(rng, scale, root, section, previous):
    """Returns [(step, length, midi)] for one bar of lead guitar."""
    degrees = []
    for octave in (0, 12, 24):
        degrees += [root + octave + d for d in scale]
    if section == 'D':
        chord = [0, 3, 7, 12, 15, 19, 24]
        up = rng.random() < 0.5
        tones = [root + c for c in (chord if up else chord[::-1])]
        return [(s, 1, tones[s % len(tones)]) for s in range(STEPS)]
    rhythms = [
        [(0, 4), (4, 2), (6, 2), (8, 6), (14, 2)],
        [(0, 2), (2, 2), (4, 4), (8, 2), (10, 2), (12, 4)],
        [(0, 6), (6, 2), (8, 4), (12, 4)],
    ]
    notes = []
    index = min(range(len(degrees)), key=lambda i: abs(degrees[i] - previous))
    for step, length in rng.choice(rhythms):
        index = max(0, min(len(degrees) - 1, index + rng.choice(
            (-2, -1, -1, 1, 1, 2))))
        notes.append((step, length, degrees[index]))
    return notes


def render_stage(stage, csv_path, out_path):
    chapter = stage_chapter(stage)
    theme = THEMES[chapter]
    rng = random.Random(theme['seed'] * 1000 + stage)
    bars, lead, gates = load_bars(csv_path)
    total_bars = len(bars) - 1
    seconds = bars[-1] + 3.5
    guitar_l, guitar_r = Bus(seconds), Bus(seconds)
    bass, lead_bus, drums = Bus(seconds), Bus(seconds), Bus(seconds)
    noise = Noise(theme['seed'])
    key = theme['key']
    lead_note = key + 24 + 12

    for bar in range(total_bars):
        section = section_of(bar, lead, gates)
        progression = theme['progression'].get(
            section, theme['progression']['D'] if section == 'outro'
            else [0])
        phrase_bar = (bar - lead) % 4 if bar >= lead else bar
        root = key + progression[phrase_bar % len(progression)]
        if section == 'outro':
            root = key
        bar_end = bars[bar + 1]

        # Rhythm guitars, double-tracked.
        if section == 'intro':
            pattern = 'o---------------'
        elif section == 'outro':
            pattern = ('o---o---o---o---' if bar < total_bars - 1
                       else 'o---------------')
        else:
            pattern = theme['riff'][section]
        swell = (0.55 + 0.45 * (bar + 1) / max(1, lead)
                 if section == 'intro' else 1.0)
        gate_bar = lead <= bar < lead + gates
        for step, length, offset, style in riff_events(pattern):
            start = step_time(bars, bar, step)
            end = step_time(bars, bar, step + length) if step + length < \
                STEPS else bar_end
            if step == 0 and gate_bar:
                accent = 1.2
            elif step % 4 == 0:
                accent = 1.0
            else:
                accent = 0.8
            vel = 0.5 * accent * swell
            render_guitar(guitar_l, start, end, root + offset, style, vel,
                          1.0)
            render_guitar(guitar_r, start + 0.007, end, root + offset, style,
                          vel, 1.0035)
            render_bass(bass, start, end, root - 12 + offset, 0.55 * swell,
                        style != 'open')

        # Lead guitar over the climb and the final pressure.
        if section in ('C', 'D'):
            for step, length, note in lead_phrase(
                    rng, theme['lead_scale'], key + 24, section, lead_note):
                start = step_time(bars, bar, step)
                end = step_time(bars, bar, min(STEPS, step + length)) \
                    if step + length < STEPS else bar_end
                render_lead(lead_bus, start, end, note,
                            0.32 if section == 'C' else 0.27)
                lead_note = note

        # Drums.
        groove = DRUMS[section]
        for step in range(STEPS):
            t = step_time(bars, bar, step)
            if groove['kick'][step] == 'k':
                render_drum(drums, noise, t, 'kick',
                            0.9 if step % 4 == 0 else 0.7)
            if groove['snare'][step] == 's':
                render_drum(drums, noise, t, 'snare', 0.75)
            hat = groove['hat'][step]
            if hat == 'h':
                render_drum(drums, noise, t, 'hat',
                            0.32 if step % 4 == 0 else 0.22)
            elif hat == 'r':
                render_drum(drums, noise, t, 'ride', 0.2)
        # Gate downbeat accent: a crash on every section change and on every
        # final-pressure gate.
        previous = section_of(bar - 1, lead, gates) if bar > 0 else None
        if section != 'intro' and (section != previous or section == 'D'):
            render_drum(drums, noise, bars[bar], 'crash', 0.42)
        elif gate_bar:
            # Every gate downbeat gets a doubled kick and a short splash so
            # dense riffs never hide the arrival.
            render_drum(drums, noise, bars[bar], 'kick', 0.5)
            render_drum(drums, noise, bars[bar], 'hat', 0.55)
        # Fill into each new phrase and into GO.
        next_section = section_of(bar + 1, lead, gates)
        if (bar == lead - 1 or (bar >= lead and phrase_bar == 3) or
                next_section != section) and section != 'outro':
            for step in range(12, STEPS):
                kind = 'tom' if section == 'intro' else 'snare'
                render_drum(drums, noise, step_time(bars, bar, step), kind,
                            0.35 + 0.08 * (step - 12))

    # Final hit rings out.
    final = bars[-1]
    render_guitar(guitar_l, final, final + 2.6, key, 'open', 0.55, 1.0)
    render_guitar(guitar_r, final + 0.007, final + 2.6, key, 'open', 0.55,
                  1.0035)
    render_bass(bass, final, final + 2.6, key - 12, 0.55, False)
    render_drum(drums, noise, final, 'crash', 0.5)
    render_drum(drums, noise, final, 'kick', 0.9)

    for bus in (guitar_l, guitar_r):
        bus.highpass(90.0)
        bus.lowpass(5200.0, poles=2)
    lead_bus.lowpass(5500.0)
    bass.lowpass(900.0)

    # Lead delay (slap echo) for space.
    delay = int(0.27 * SR)
    lb = lead_bus.buf
    for i in range(delay, lead_bus.n):
        lb[i] += lb[i - delay] * 0.28

    mix_l = array.array('f', bytes(4 * guitar_l.n))
    mix_r = array.array('f', bytes(4 * guitar_l.n))
    gl, gr, bb, db = guitar_l.buf, guitar_r.buf, bass.buf, drums.buf
    peak = 0.0
    for i in range(guitar_l.n):
        center = bb[i] * 0.42 + db[i] * 0.55
        l = gl[i] * 0.30 + gr[i] * 0.06 + center + lb[i] * 0.12
        r = gr[i] * 0.30 + gl[i] * 0.06 + center + lb[i] * 0.20
        mix_l[i] = l
        mix_r[i] = r
        peak = max(peak, abs(l), abs(r))

    drive = 1.25 / peak if peak > 0.0 else 1.0
    ceiling = 0.92 / math.tanh(1.25)
    end = int(seconds * SR)
    pcm = array.array('h', bytes(4 * end))
    for i in range(end):
        pcm[2 * i] = int(32767 * ceiling * math.tanh(mix_l[i] * drive))
        pcm[2 * i + 1] = int(32767 * ceiling * math.tanh(mix_r[i] * drive))

    os.makedirs(os.path.dirname(out_path) or '.', exist_ok=True)
    with wave.open(out_path, 'wb') as handle:
        handle.setnchannels(2)
        handle.setsampwidth(2)
        handle.setframerate(SR)
        handle.writeframes(pcm.tobytes())
    return dict(stage=stage, chapter=chapter, seconds=seconds,
                bars=total_bars, gates=gates, lead_in_bars=lead,
                first_gate=bars[lead], last_gate=bars[lead + gates - 1])


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--stage', type=int, action='append', required=True)
    parser.add_argument('--timeline', default='Artifacts/GateTimeline')
    parser.add_argument('--out', default='Artifacts/Music')
    args = parser.parse_args()
    for stage in args.stage:
        csv_path = os.path.join(args.timeline, 'Stage%02d.csv' % stage)
        out_path = os.path.join(args.out, 'Stage%02d.wav' % stage)
        print(render_stage(stage, csv_path, out_path))


if __name__ == '__main__':
    main()
