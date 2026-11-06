"""Deterministic UI and gameplay sound effects for Color Gate Runner.

Short synthesized cues pitched to E minor so they sit with the Stage music.
Standard library only; the same script always writes the same samples.

    python Tools/Music/synth_ui_sfx.py --out ColorGateRunner/Assets/Game/Audio/Sfx
"""

import argparse
import array
import math
import os
import random
import wave

SR = 44100
TWO_PI = 2.0 * math.pi


def hz(midi):
    return 440.0 * 2.0 ** ((midi - 69) / 12.0)


def buffer(seconds):
    return array.array('f', bytes(4 * int(seconds * SR)))


def tone(buf, start, length, freq, vel, wave_kind='sine', attack=0.004,
         decay=0.15, glide=0.0, vibrato=0.0):
    """Adds one enveloped oscillator note; glide is the start ratio."""
    i0 = int(start * SR)
    n = min(int(length * SR), len(buf) - i0)
    ph = 0.0
    a = int(attack * SR) or 1
    for k in range(n):
        t = k / SR
        f = freq * (1.0 + glide * math.exp(-t * 40.0))
        if vibrato:
            f *= 1.0 + vibrato * math.sin(TWO_PI * 6.0 * t)
        ph += f / SR
        ph -= math.floor(ph)
        if wave_kind == 'sine':
            s = math.sin(TWO_PI * ph)
        elif wave_kind == 'tri':
            s = 4.0 * abs(ph - 0.5) - 1.0
        elif wave_kind == 'square':
            s = 0.6 if ph < 0.5 else -0.6
        else:
            s = 2.0 * ph - 1.0
        env = (k / a) if k < a else math.exp(-(t - attack) / decay)
        fade = min(1.0, (n - k) / (0.004 * SR))
        buf[i0 + k] += s * env * fade * vel


def noise(buf, start, length, vel, seed, cutoff=6000.0, decay=0.05,
          highpass=True):
    rng = random.Random(seed)
    i0 = int(start * SR)
    n = min(int(length * SR), len(buf) - i0)
    a = 1.0 - math.exp(-TWO_PI * cutoff / SR)
    lp = 0.0
    for k in range(n):
        x = rng.uniform(-1.0, 1.0)
        lp += a * (x - lp)
        s = x - lp if highpass else lp
        fade = min(1.0, (n - k) / (0.004 * SR))
        buf[i0 + k] += s * math.exp(-(k / SR) / decay) * fade * vel


def bell(buf, start, midi, vel, decay=0.35):
    # Inharmonic partials give a small glassy chime.
    for ratio, gain in ((1.0, 1.0), (2.76, 0.35), (5.4, 0.15)):
        tone(buf, start, decay * 4.0, hz(midi) * ratio, vel * gain,
             decay=decay / ratio ** 0.5)


# ------------------------------------------------------------------- cues

def ui_tap():
    b = buffer(0.09)
    tone(b, 0.0, 0.09, hz(88), 0.5, 'tri', decay=0.025)
    noise(b, 0.0, 0.02, 0.15, 1, decay=0.006)
    return b


def ui_confirm():
    b = buffer(0.32)
    tone(b, 0.0, 0.12, hz(76), 0.42, 'tri', decay=0.05)
    tone(b, 0.06, 0.26, hz(83), 0.48, 'tri', decay=0.09)
    bell(b, 0.06, 95, 0.12, decay=0.08)
    return b


def ui_back():
    b = buffer(0.2)
    tone(b, 0.0, 0.2, hz(81), 0.45, 'tri', decay=0.05, glide=-0.18)
    return b


def ui_tab():
    b = buffer(0.16)
    tone(b, 0.0, 0.16, hz(79), 0.38, 'sine', decay=0.05, glide=0.12)
    noise(b, 0.0, 0.06, 0.12, 2, cutoff=4000.0, decay=0.02)
    return b


def ui_toggle_on():
    b = buffer(0.16)
    tone(b, 0.0, 0.07, hz(76), 0.4, 'square', decay=0.02)
    tone(b, 0.05, 0.11, hz(83), 0.4, 'square', decay=0.03)
    return b


def ui_toggle_off():
    b = buffer(0.16)
    tone(b, 0.0, 0.07, hz(83), 0.36, 'square', decay=0.02)
    tone(b, 0.05, 0.11, hz(76), 0.36, 'square', decay=0.03)
    return b


def ui_danger():
    b = buffer(0.3)
    tone(b, 0.0, 0.3, hz(64), 0.4, 'saw', decay=0.09)
    tone(b, 0.0, 0.3, hz(65), 0.3, 'saw', decay=0.09)
    return b


def ui_purchase():
    b = buffer(0.55)
    for i, note in enumerate((88, 95, 100)):
        bell(b, i * 0.055, note, 0.32, decay=0.12)
    noise(b, 0.0, 0.4, 0.06, 3, cutoff=9000.0, decay=0.12)
    return b


def ui_denied():
    b = buffer(0.24)
    tone(b, 0.0, 0.1, hz(58), 0.45, 'square', decay=0.04)
    tone(b, 0.11, 0.12, hz(58), 0.45, 'square', decay=0.04)
    return b


def countdown_tick():
    b = buffer(0.2)
    tone(b, 0.0, 0.2, hz(81), 0.5, 'square', decay=0.05)
    return b


def countdown_go():
    b = buffer(0.6)
    for note in (76, 83, 88):
        tone(b, 0.0, 0.6, hz(note), 0.28, 'saw', decay=0.18)
    noise(b, 0.0, 0.3, 0.12, 4, cutoff=7000.0, decay=0.08)
    return b


def gate_perfect():
    b = buffer(0.5)
    bell(b, 0.0, 88, 0.42, decay=0.14)
    bell(b, 0.045, 95, 0.36, decay=0.16)
    noise(b, 0.0, 0.25, 0.08, 5, cutoff=9500.0, decay=0.06)
    return b


def gate_great():
    b = buffer(0.36)
    bell(b, 0.0, 88, 0.38, decay=0.11)
    return b


def gate_good():
    b = buffer(0.14)
    tone(b, 0.0, 0.14, hz(76), 0.35, 'tri', decay=0.03)
    return b


def fail():
    b = buffer(0.8)
    tone(b, 0.0, 0.8, hz(52), 0.4, 'saw', decay=0.3, glide=0.6)
    tone(b, 0.0, 0.8, hz(52.3), 0.3, 'saw', decay=0.3, glide=0.6)
    noise(b, 0.0, 0.5, 0.25, 6, cutoff=1200.0, decay=0.15, highpass=False)
    return b


def clear():
    b = buffer(1.4)
    for i, note in enumerate((76, 79, 83, 88, 91, 95)):
        bell(b, i * 0.07, note, 0.3, decay=0.25)
    for note in (64, 71, 76):
        tone(b, 0.42, 0.95, hz(note), 0.16, 'saw', attack=0.02, decay=0.4)
    return b


def crown():
    b = buffer(1.0)
    for i, note in enumerate((95, 100, 103, 107)):
        bell(b, i * 0.05, note, 0.26, decay=0.22)
    noise(b, 0.0, 0.8, 0.05, 7, cutoff=10000.0, decay=0.25)
    return b


def booster_wind():
    """Seamless 3 s loop of rushing air: band-limited noise with slow gusts
    and a faint high whistle. The tail is crossfaded into the head so the
    loop has no seam."""
    seconds, fade = 3.0, 0.4
    n, m = int(seconds * SR), int(fade * SR)
    rng = random.Random(2401)
    raw = array.array('f', bytes(4 * (n + m)))
    low = band = whistle = 0.0
    a_low = 1.0 - math.exp(-TWO_PI * 900.0 / SR)
    a_band = 1.0 - math.exp(-TWO_PI * 2600.0 / SR)
    a_whistle = 1.0 - math.exp(-TWO_PI * 5200.0 / SR)
    for k in range(n + m):
        t = k / SR
        x = rng.uniform(-1.0, 1.0)
        low += a_low * (x - low)
        band += a_band * (x - band)
        whistle += a_whistle * (x - whistle)
        # Gusts repeat every loop (periods divide 3 s).
        gust = 0.7 + 0.3 * math.sin(TWO_PI * t / 1.5) *             math.sin(TWO_PI * t / 3.0 + 0.6)
        raw[k] = (low * 1.4 + (band - low) * 0.9 +
                  (whistle - band) * 0.25 * (0.6 + 0.4 * math.sin(
                      TWO_PI * t / 0.75))) * gust
    buf = buffer(seconds)
    for k in range(n):
        buf[k] = raw[k]
    for k in range(m):
        w = k / m
        buf[k] = raw[k] * w + raw[n + k] * (1.0 - w)
    return buf


CUES = {
    'UiTap': ui_tap,
    'UiConfirm': ui_confirm,
    'UiBack': ui_back,
    'UiTab': ui_tab,
    'UiToggleOn': ui_toggle_on,
    'UiToggleOff': ui_toggle_off,
    'UiDanger': ui_danger,
    'UiPurchase': ui_purchase,
    'UiDenied': ui_denied,
    'CountdownTick': countdown_tick,
    'CountdownGo': countdown_go,
    'GatePerfect': gate_perfect,
    'GateGreat': gate_great,
    'GateGood': gate_good,
    'Fail': fail,
    'Clear': clear,
    'Crown': crown,
    # Looped by StageMusicPlayer during a Booster dash, not a SoundCue.
    'BoosterWind': booster_wind,
}


# Relative loudness: UI cues stay quiet, gate cues sit just above the music.
PEAKS = {'UiTap': 0.45, 'UiTab': 0.5, 'UiToggleOn': 0.45, 'UiToggleOff': 0.45,
         'UiBack': 0.5, 'GateGood': 0.5, 'GateGreat': 0.6, 'GatePerfect': 0.7,
         'BoosterWind': 0.6}


def write(path, buf, peak_target=0.8):
    peak = max(abs(x) for x in buf) or 1.0
    gain = peak_target / peak
    pcm = array.array('h', (int(32767 * max(-1.0, min(1.0, x * gain)))
                            for x in buf))
    with wave.open(path, 'wb') as handle:
        handle.setnchannels(1)
        handle.setsampwidth(2)
        handle.setframerate(SR)
        handle.writeframes(pcm.tobytes())


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--out', required=True)
    args = parser.parse_args()
    os.makedirs(args.out, exist_ok=True)
    for name, build in CUES.items():
        write(os.path.join(args.out, name + '.wav'), build(),
              PEAKS.get(name, 0.8))
        print(name)


if __name__ == '__main__':
    main()
