"""Deterministic hard-rock stage music synthesizer for Color Gate Runner.

Each Campaign Stage track is fitted to that Stage's exported gate timeline
(Artifacts/GateTimeline/StageNN.csv): every gate arrival lands on a bar
downbeat, so the tempo accelerates with the Stage. Every Stage has its own
theme (key, scale, progression, riffs, groove, tone and hook melody) on one
shared arrangement engine. The first Stage of each chapter keeps the original
chapter theme of Iteration 72 and renders identically. One Stage in the middle
of each chapter is a calmer 90s city-pop track (electric piano, clean cutting
guitar, fingered bass, half-time groove) for pacing.

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
    'd': (2, 'single'),
    'u': (12, 'single'),
}

MINOR_PENT = [0, 3, 5, 7, 10]
NATURAL_MINOR = [0, 2, 3, 5, 7, 8, 10]
HARMONIC_MINOR = [0, 2, 3, 5, 7, 8, 11]
PHRYGIAN_DOMINANT = [0, 1, 4, 5, 7, 8, 10]
DORIAN = [0, 2, 3, 5, 7, 9, 10]
PHRYGIAN = [0, 1, 3, 5, 7, 8, 10]
MAJOR = [0, 2, 4, 5, 7, 9, 11]
MAJOR_PENT = [0, 2, 4, 7, 9]

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

# Stages that keep their Iteration 72 chapter theme unchanged.
ORIGINAL_STAGES = {1: 'basics', 6: 'items', 9: 'camouflage', 12: 'fog',
                   15: 'ice', 18: 'echo', 21: 'hidden', 24: 'flicker'}


def stage_theme(name, key, seed, scale, progression, riff, **options):
    return dict(name=name, key=key, seed=seed, lead_scale=scale,
                progression=dict(intro=[0], **progression), riff=riff,
                **options)


# Every other Stage: its own theme. Options: groove (GROOVES name), tone
# (TONES name), hook (melody seed), bass ('octave'), lead_drive, delay.
STAGE_THEMES = {
    2: stage_theme(
        'street_sprint', 45, 212, MINOR_PENT,
        dict(A=[0, 5, 0, 7], B=[3, 10, 5, 7], C=[5, 7, 3, 10],
             D=[0, 5, 7, 7]),
        dict(A='m.m.m.m.o---m.m.', B='o-m.o-m.o-m.o-m.',
             C='o-------o---o---', D='m.m.m.m.m.m.mvmx'),
        groove='punk', tone='crunch', bass='octave', hook=2),
    3: stage_theme(
        'gallop_run', 38, 313, NATURAL_MINOR,
        dict(A=[0, 0, 10, 10], B=[8, 8, 10, 0], C=[3, 10, 8, 7],
             D=[0, 10, 8, 7]),
        dict(A='m.mmm.mmm.mmm.mm', B='o---m.mmo---m.mm',
             C='m.mmm.mmo---o---', D='m.mmm.mmm.mmmvmx'),
        groove='gallop', tone='heavy', hook=3),

    5: stage_theme(
        'basics_finale', 40, 515, MINOR_PENT,
        dict(A=[0, 3, 5, 3], B=[0, 10, 8, 7], C=[8, 10, 0, 3],
             D=[0, 3, 5, 7]),
        dict(A='mmmmm.mmmmm.mnmf', B='o---mmmmo---mmmm',
             C='o---o---o-o-o---', D='mmmmmmmmmnmfmvo-'),
        groove='blast', tone='fuzz', hook=5),

    8: stage_theme(
        'heavy_cargo', 37, 818, HARMONIC_MINOR,
        dict(A=[0, 0, 8, 7], B=[0, 1, 8, 7], C=[5, 7, 8, 7],
             D=[0, 8, 1, 7]),
        dict(A='o-----m.m.o-----', B='m.m.o---m.mbo---',
             C='o---------------', D='m.mbm.mbm.mnmvo-'),
        groove='halftime', tone='heavy', hook=8),
    10: stage_theme(
        'chameleon', 46, 1010, PHRYGIAN,
        dict(A=[0, 1, 0, 10], B=[0, 8, 10, 1], C=[8, 10, 1, 0],
             D=[0, 1, 3, 1]),
        dict(A='m.mbm.m.m.mbo---', B='mbm.mbm.o---m.mb',
             C='o---o---o---o---', D='mbmbmbmbo-mbmbo-'),
        groove='tribal', tone='heavy', hook=10),

    13: stage_theme(
        'doom_fog', 38, 1313, NATURAL_MINOR,
        dict(A=[0, 0, 3, 5], B=[8, 8, 7, 7], C=[3, 5, 8, 10],
             D=[0, 3, 8, 7]),
        dict(A='o-------o-----m.', B='o---o---m.m.o---',
             C='o---------------', D='m.m.m.m.o---o---'),
        groove='halftime', tone='heavy', hook=13, lead_drive=3.0,
        delay=0.41),

    16: stage_theme(
        'frost_punk', 47, 1616, DORIAN,
        dict(A=[0, 10, 0, 5], B=[3, 10, 5, 0], C=[5, 10, 3, 7],
             D=[0, 5, 10, 7]),
        dict(A='m.m.m.m.mvmxo---', B='o---m.m.o---mvmx',
             C='o---o---o---o-o-', D='mvmvmxmxmvmvo---'),
        groove='punk', tone='fuzz', hook=16),

    19: stage_theme(
        'echo_chamber', 40, 1919, NATURAL_MINOR,
        dict(A=[0, 3, 10, 8], B=[0, 8, 10, 3], C=[8, 3, 10, 7],
             D=[0, 3, 8, 7]),
        dict(A='o--m.mo--m.mm.m.', B='m.mo--m.mo--o---',
             C='o-------o---o---', D='o--mo--mo--mo---'),
        groove='rock', tone='crunch', hook=19, delay=0.38),

    22: stage_theme(
        'shadow_step', 37, 2222, PHRYGIAN,
        dict(A=[0, 1, 0, 0], B=[0, 1, 3, 1], C=[8, 10, 8, 1],
             D=[0, 1, 10, 1]),
        dict(A='m...m.mbm...m.mb', B='o---mbmbo---m...',
             C='o-------....o---', D='mbmbm...mbmbo---'),
        groove='tribal', tone='fuzz', hook=22),

    25: stage_theme(
        'strobe', 41, 2525, PHRYGIAN_DOMINANT,
        dict(A=[0, 1, 4, 1], B=[0, 4, 5, 1], C=[8, 7, 5, 4],
             D=[0, 1, 8, 7]),
        dict(A='mbmmmbmmmbmmo---', B='o---mbmbo---mbmb',
             C='o---o---o-o-o-o-', D='mmmmmbmbmmmmmnmv'),
        groove='blast', tone='fuzz', bass='octave', hook=25),
    26: stage_theme(
        'final_gate', 40, 2626, HARMONIC_MINOR,
        dict(A=[0, 8, 10, 0], B=[0, 5, 7, 8], C=[8, 10, 7, 7],
             D=[0, 8, 10, 7]),
        dict(A='m.mmm.mmo-m.mmo-', B='mmmmo---mmmmo---',
             C='o---o---o---o-o-', D='mmmmmmmmmnmvmxo-'),
        groove='blast', tone='heavy', hook=26),
}

CHORDS = dict(maj7=(0, 4, 7, 11), m7=(0, 3, 7, 10), dom7=(0, 4, 7, 10),
              maj9=(0, 4, 7, 11, 14), m9=(0, 3, 7, 10, 14),
              dom9=(0, 4, 10, 14), sus=(0, 5, 7, 10))


def citypop_theme(name, key, seed, progression, comp, cut, bass, **options):
    """key is the tonic bass note; progression bars are (offset, chord)."""
    return dict(name=name, key=key, seed=seed, style='citypop',
                lead_scale=MAJOR, progression=progression, comp=comp,
                cut=cut, bass_line=bass, **options)


# City-pop breathers, one in the middle of each chapter. Patterns: comp and
# cut 'c'/'x' hits, '-' sustain; bass 'r' root, 'o' octave, 'f' fifth,
# 's' chord seventh.
CITYPOP_THEMES = {
    4: citypop_theme(
        'sunset_drive', 41, 404,
        dict(A=[(5, 'maj7'), (4, 'dom7'), (9, 'm7'), (7, 'dom9')],
             B=[(2, 'm9'), (7, 'dom9'), (0, 'maj9'), (9, 'm7')],
             C=[(5, 'maj7'), (7, 'sus'), (4, 'dom7'), (9, 'm9')],
             D=[(5, 'maj9'), (4, 'dom7'), (9, 'm7'), (0, 'maj9')]),
        comp=dict(A='c-----c---c-----', B='c---c-----c---c-',
                  C='c-------c---c---', D='c-----c-c---c---'),
        cut=dict(A='................', B='..x...x...x...x.',
                 C='..x...x...x..x.x', D='..x...x...x...x.'),
        bass=dict(A='r-----r-o-r-f---', B='r---o-r---f-o-r-',
                  C='r-----f---r-o---', D='r-o-r---f-o-r-s-'),
        hook=404),
    7: citypop_theme(
        'neon_boulevard', 43, 707,
        dict(A=[(0, 'maj9'), (11, 'm7'), (9, 'm9'), (2, 'dom9')],
             B=[(5, 'maj7'), (7, 'dom7'), (4, 'm7'), (9, 'm7')],
             C=[(2, 'm9'), (7, 'dom9'), (0, 'maj9'), (0, 'maj7')],
             D=[(5, 'maj9'), (7, 'dom9'), (4, 'm7'), (9, 'm9')]),
        comp=dict(A='c---------c-----', B='c-----c---c---c-',
                  C='c---c---c---c---', D='c-----c---c-c---'),
        cut=dict(A='......x.......x.', B='..x.x...x.x...x.',
                 C='..x...x...x...x.', D='x.x.x.x.x.x.x.x.'),
        bass=dict(A='r-------o-r-----', B='r---r-o---f---o-',
                  C='r-o-r-o-f---r---', D='r-o-f-o-r-o-s-o-'),
        hook=707),
    11: citypop_theme(
        'mirage', 45, 1101,
        dict(A=[(0, 'maj7'), (9, 'm7'), (5, 'maj7'), (7, 'dom7')],
             B=[(2, 'm7'), (4, 'm7'), (5, 'maj9'), (7, 'sus')],
             C=[(9, 'm9'), (2, 'dom9'), (7, 'm7'), (0, 'dom9')],
             D=[(5, 'maj7'), (7, 'dom9'), (9, 'm9'), (0, 'maj9')]),
        comp=dict(A='c-------c-------', B='c-----c-----c---',
                  C='c---c-----c---c-', D='c-----c---c---c-'),
        cut=dict(A='....x.......x...', B='..x...x...x...x.',
                 C='x.x...x.x.x...x.', D='..x.x.x...x.x.x.'),
        bass=dict(A='r-------f---o---', B='r-----r-o---f---',
                  C='r-o---r-o-f-r---', D='r-o-r-f-o-r-s-r-'),
        hook=1101, delay=0.34),
    14: citypop_theme(
        'midnight_mist', 38, 1402,
        dict(A=[(5, 'maj9'), (7, 'dom9'), (4, 'm7'), (9, 'm7')],
             B=[(2, 'm9'), (7, 'dom7'), (0, 'maj7'), (0, 'maj9')],
             C=[(5, 'maj7'), (4, 'dom7'), (9, 'm9'), (9, 'm7')],
             D=[(2, 'm9'), (7, 'dom9'), (4, 'm7'), (9, 'm9')]),
        comp=dict(A='c---------------', B='c-------c-----c-',
                  C='c-----c---c-----', D='c---c---c---c-c-'),
        cut=dict(A='................', B='....x.......x...',
                 C='..x...x...x...x.', D='..x...x...x.x.x.'),
        bass=dict(A='r-------r-------', B='r-----o-r-------',
                  C='r---f---o---r---', D='r-o-r---f-o-r-o-'),
        hook=1402, delay=0.4, lead_drive=1.6),
    17: citypop_theme(
        'crystal_coast', 44, 1703,
        dict(A=[(0, 'maj9'), (4, 'm7'), (5, 'maj7'), (7, 'dom9')],
             B=[(9, 'm7'), (4, 'm7'), (5, 'maj9'), (2, 'dom9')],
             C=[(5, 'maj7'), (7, 'dom7'), (0, 'maj9'), (9, 'm9')],
             D=[(5, 'maj9'), (7, 'sus'), (4, 'dom7'), (9, 'm9')]),
        comp=dict(A='c-----c---c-----', B='c---c---c-----c-',
                  C='c-------c-c-----', D='c-----c-c---c---'),
        cut=dict(A='..x.......x.....', B='..x...x...x...x.',
                 C='x...x.x...x.x.x.', D='..x.x.x...x.x.x.'),
        bass=dict(A='r-----r---o-----', B='r---o---f---r-o-',
                  C='r-o-r---o-r-f---', D='r-o-r-o-f-o-s-o-'),
        hook=1703),
    20: citypop_theme(
        'tokyo_echo', 39, 2004,
        dict(A=[(9, 'm9'), (2, 'dom9'), (7, 'maj7'), (0, 'maj7')],
             B=[(5, 'maj7'), (4, 'dom7'), (9, 'm7'), (9, 'm9')],
             C=[(2, 'm7'), (7, 'dom9'), (4, 'm7'), (9, 'm7')],
             D=[(5, 'maj9'), (7, 'dom9'), (0, 'maj9'), (9, 'm9')]),
        comp=dict(A='c-------c---c---', B='c-----c-----c---',
                  C='c---c---c---c---', D='c-----c---c-c-c-'),
        cut=dict(A='......x.......x.', B='..x...x...x...x.',
                 C='..x.x...x.x...x.', D='x.x.x.x.x.x.x.x.'),
        bass=dict(A='r-----o---r-----', B='r-o---r-o---f---',
                  C='r---o---r-f-o---', D='r-o-r-o-f-o-r-s-'),
        hook=2004, delay=0.38),
    23: citypop_theme(
        'night_window', 42, 2305,
        dict(A=[(5, 'maj7'), (4, 'm7'), (2, 'm9'), (7, 'dom9')],
             B=[(0, 'maj9'), (9, 'm7'), (5, 'maj7'), (7, 'sus')],
             C=[(9, 'm9'), (4, 'dom7'), (5, 'maj7'), (7, 'dom9')],
             D=[(5, 'maj9'), (4, 'dom7'), (9, 'm9'), (0, 'maj9')]),
        comp=dict(A='c-------c-------', B='c-----c---c-----',
                  C='c---c-----c-c---', D='c-----c-c---c-c-'),
        cut=dict(A='....x.......x...', B='..x...x...x...x.',
                 C='..x.x.x...x.x.x.', D='x.x.x.x.x.x.x.x.'),
        bass=dict(A='r-------o-------', B='r-----r-f---o---',
                  C='r-o---f-r-o-----', D='r-o-r-f-o-r-s-o-'),
        hook=2305, lead_drive=1.6),
}

# Half-time city-pop groove: one gate bar spans two beats.
CITYPOP_DRUMS = dict(
    intro=dict(kick='k...............', snare='................',
               hat='........h.......'),
    A=dict(kick='k.....k.........', snare='........s.......',
           hat='h.h.h.h.h.h.h.h.'),
    B=dict(kick='k.....k...k.....', snare='........s.......',
           hat='h.h.h.r.h.h.h.r.'),
    C=dict(kick='k.......k.......', snare='........s.....s.',
           hat='h.h.h.h.h.h.h.h.'),
    D=dict(kick='k.....k...k.k...', snare='........s.......',
           hat='h.h.h.r.h.h.h.r.'),
    outro=dict(kick='k.......k.......', snare='........s.......',
               hat='h...h...h...h...'),
)

# Guitar tone: (distortion gain scale, filter cutoff scale).
TONES = dict(classic=(1.0, 1.0), crunch=(0.6, 1.2), heavy=(1.5, 0.85),
             fuzz=(2.2, 0.95))

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

# Alternative grooves for the gate sections; intro and outro come from DRUMS.
# The optional tom lane plays floor-tom hits.
GROOVES = dict(
    rock=DRUMS,
    punk=dict(
        A=dict(kick='k...k...k...k...', snare='..s...s...s...s.',
               hat='h.h.h.h.h.h.h.h.'),
        B=dict(kick='k.k.k.k.k.k.k.k.', snare='..s...s...s...s.',
               hat='h.h.h.h.h.h.h.h.'),
        C=dict(kick='k.k.k.k.k.k.k.k.', snare='.s.s.s.s.s.s.s.s',
               hat='r...r...r...r...'),
        D=dict(kick='kkkkkkkkkkkkkkkk', snare='..s...s...s...s.',
               hat='h.h.h.h.h.h.h.h.')),
    gallop=dict(
        A=dict(kick='k.kkk.kkk.kkk.kk', snare='....s.......s...',
               hat='h...h...h...h...'),
        B=dict(kick='k.kkk.kkk.kkk.kk', snare='....s.......s...',
               hat='h.h.h.h.h.h.h.h.'),
        C=dict(kick='k.kkk.kkk.kkk.kk', snare='....s.......s...',
               hat='r...r...r...r...'),
        D=dict(kick='kkkkkkkkkkkkkkkk', snare='....s...s...s.s.',
               hat='h.h.h.h.h.h.h.h.')),
    halftime=dict(
        A=dict(kick='k.......k.k.....', snare='........s.......',
               hat='h.h.h.h.h.h.h.h.'),
        B=dict(kick='k.....k.k.k.....', snare='........s.......',
               hat='h.h.h.h.h.h.h.h.'),
        C=dict(kick='k.......k.......', snare='........s.......',
               hat='r...r...r...r...'),
        D=dict(kick='kkkkkkkkkkkkkkkk', snare='....s.......s...',
               hat='h.h.h.h.h.h.h.h.')),
    blast=dict(
        A=dict(kick='k.k.k.k.k.k.k.k.', snare='....s.......s...',
               hat='h.h.h.h.h.h.h.h.'),
        B=dict(kick='kkkk..kkkkkk..kk', snare='....s.......s...',
               hat='h.h.h.h.h.h.h.h.'),
        C=dict(kick='kkkkkkkkkkkkkkkk', snare='..s...s...s...s.',
               hat='r...r...r...r...'),
        D=dict(kick='kkkkkkkkkkkkkkkk', snare='.s.s.s.s.s.s.s.s',
               hat='h...h...h...h...')),
    tribal=dict(
        A=dict(kick='k.....k...k.....', snare='....s.......s...',
               hat='h...h...h...h...', tom='..t...t..t..t.t.'),
        B=dict(kick='k..k..k.k..k..k.', snare='....s.......s...',
               hat='h.h.h.h.h.h.h.h.', tom='........t.t.t.t.'),
        C=dict(kick='k...k...k...k...', snare='....s.......s...',
               hat='r...r...r...r...', tom='t..t..t.t..t..t.'),
        D=dict(kick='kkkkkkkkkkkkkkkk', snare='....s.......s.s.',
               hat='h.h.h.h.h.h.h.h.', tom='............tttt')),
)

HOOK_CELLS = [
    [(0, 4), (4, 4), (8, 8)],
    [(0, 2), (2, 2), (4, 4), (8, 4), (12, 4)],
    [(0, 6), (6, 2), (8, 8)],
    [(0, 3), (3, 3), (6, 2), (8, 8)],
    [(0, 4), (4, 2), (6, 2), (8, 4), (12, 4)],
    [(0, 2), (2, 4), (6, 2), (8, 2), (10, 6)],
]


def theme_for(stage):
    if stage in ORIGINAL_STAGES:
        return THEMES[ORIGINAL_STAGES[stage]]
    if stage in CITYPOP_THEMES:
        return CITYPOP_THEMES[stage]
    return STAGE_THEMES[stage]


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

def render_guitar(bus, start, end, root, style, vel, detune,
                  tone=(1.0, 1.0)):
    if style == 'open':
        notes, cutoff, gain, tau = (0, 7, 12), 3200.0, 9.0, 1.6
    elif style == 'mute':
        notes, cutoff, gain, tau = (0, 7), 750.0, 5.5, 0.06
    else:
        notes, cutoff, gain, tau = (0, 12), 1400.0, 6.5, 0.09
    gain *= tone[0]
    cutoff *= tone[1]
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


def render_bass(bus, start, end, note, vel, muted, cutoff=320.0, drive=2.2):
    f = midi_hz(note) / SR
    a = one_pole(cutoff)
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
        buf[i0 + k] += math.tanh(drive * lp) * amp * vel


def render_lead(bus, start, end, note, vel, drive=10.0):
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
        buf[i0 + k] += math.tanh(drive * lp) * amp * vel


def render_epiano(bus, start, end, notes, vel):
    """Two-operator FM electric piano: a bell-like tine that mellows."""
    i0 = int(start * SR)
    length = int(max(0.05, end - start) * SR)
    release = int(0.08 * SR)
    incs = [TWO_PI * midi_hz(note) / SR for note in notes]
    phases = [0.0] * len(notes)
    count = len(notes)
    norm = vel / count ** 0.5
    decay = math.exp(-1.0 / (1.6 * SR))
    mod_decay = math.exp(-1.0 / (0.25 * SR))
    attack = int(0.004 * SR)
    buf = bus.buf
    env = 1.0
    index = 1.6
    for k in range(min(length + release, bus.n - i0)):
        s = 0.0
        for j in range(count):
            ph = phases[j] + incs[j]
            phases[j] = ph
            s += math.sin(ph + index * math.sin(ph))
        env *= decay
        index = 0.35 + (index - 0.35) * mod_decay
        amp = env if k >= attack else env * k / attack
        if k >= length:
            amp *= 1.0 - (k - length) / release
        buf[i0 + k] += s * amp * norm
    return bus


def render_pluck(bus, noise, start, notes, vel):
    """Karplus-Strong muted clean-guitar chop on the chord's top notes."""
    i0 = int(start * SR)
    n = min(int(0.16 * SR), bus.n - i0)
    table = noise.table
    buf = bus.buf
    for note in notes:
        period = max(2, int(SR / midi_hz(note)))
        offset = noise.take()
        line = [table[(offset + k) % SR] for k in range(period)]
        previous = 0.0
        for k in range(n):
            x = line[k % period]
            y = 0.5 * (x + previous) * 0.985
            previous = x
            line[k % period] = y
            buf[i0 + k] += y * vel * (1.0 - k / n)


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


def make_hook(seed, scale, root):
    """Returns two bars of a theme's hook melody: [[(step, length, midi)]]."""
    rng = random.Random(seed * 7919)
    degrees = []
    for octave in (0, 12, 24):
        degrees += [root + octave + d for d in scale]
    size = len(scale)
    index = size + rng.randrange(1, size)
    bars = []
    for bar in range(2):
        notes = []
        cells = rng.choice(HOOK_CELLS)
        for position, (step, length) in enumerate(cells):
            if bar == 1 and position == len(cells) - 1:
                index = size
            elif position > 0 or bar > 0:
                # A step past the hook's two-octave range turns back.
                move = rng.choice((-2, -1, -1, 1, 1, 2, 3))
                if not 2 <= index + move <= 2 * size + 1:
                    move = -move
                index += move
            notes.append((step, length, degrees[index]))
        bars.append(notes)
    return bars


def write_track(out_path, mix_l, mix_r, peak, seconds):
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


def voice_chord(root, quality):
    """Electric-piano voicing of a chord, folded into the middle register.
    Ninth chords drop the root, which the bass already plays."""
    intervals = CHORDS[quality]
    if len(intervals) > 4:
        intervals = intervals[1:]
    notes = []
    for interval in intervals:
        note = root + interval
        while note < 57:
            note += 12
        while note > 76:
            note -= 12
        notes.append(note)
    return sorted(set(notes))


def citypop_bass_note(root, quality, symbol):
    if symbol == 'o':
        return root + 12
    if symbol == 'f':
        return root + 7
    if symbol == 's':
        # The chord's seventh; dom9 lists its seventh before the ninth.
        sevenths = [i for i in CHORDS[quality] if i in (10, 11)]
        return root + (sevenths[0] if sevenths else 7)
    return root


def render_citypop(stage, theme, csv_path, out_path):
    rng = random.Random(theme['seed'] * 1000 + stage)
    bars, lead, gates = load_bars(csv_path)
    total_bars = len(bars) - 1
    seconds = bars[-1] + 3.5
    keys, cut, bass = Bus(seconds), Bus(seconds), Bus(seconds)
    lead_bus, drums = Bus(seconds), Bus(seconds)
    noise = Noise(theme['seed'])
    key = theme['key']
    lead_drive = theme.get('lead_drive', 2.2)
    hook = make_hook(theme['hook'], MAJOR, key + 12)
    lead_note = key + 36

    for bar in range(total_bars):
        section = section_of(bar, lead, gates)
        phrase_bar = (bar - lead) % 4 if bar >= lead else bar
        if section == 'intro':
            offset, quality = theme['progression']['A'][0]
        elif section == 'outro':
            offset, quality = 0, 'maj9'
        else:
            offset, quality = theme['progression'][section][phrase_bar % 4]
        root = key + offset
        voicing = voice_chord(root + 12, quality)
        bar_end = bars[bar + 1]
        gate_bar = lead <= bar < lead + gates
        swell = (0.55 + 0.45 * (bar + 1) / max(1, lead)
                 if section == 'intro' else 1.0)
        part = 'A' if section in ('intro', 'outro') else section

        # Electric piano comping; every gate downbeat is a chord hit.
        comp = ('c---------------' if section in ('intro', 'outro')
                else theme['comp'][part])
        for step, length, _, _ in riff_events(comp.replace('c', 'o')):
            start = step_time(bars, bar, step)
            end = step_time(bars, bar, step + length) \
                if step + length < STEPS else bar_end
            accent = 1.15 if step == 0 and gate_bar else 0.85
            render_epiano(keys, start, end, voicing, 0.3 * accent * swell)

        # Clean cutting guitar on the upper chord tones.
        if section in ('B', 'C', 'D'):
            top = [note + 12 for note in voicing[-3:]]
            for step in range(STEPS):
                if theme['cut'][part][step] == 'x':
                    render_pluck(cut, noise, step_time(bars, bar, step), top,
                                 0.22 if step % 4 == 2 else 0.17)

        # Fingered bass.
        line = ('r---------------' if section in ('intro', 'outro')
                else theme['bass_line'][part])
        events = []
        step = 0
        while step < STEPS:
            symbol = line[step]
            if symbol in 'rofs':
                length = 1
                while step + length < STEPS and line[step + length] == '-':
                    length += 1
                events.append((step, length, symbol))
                step += length
            else:
                step += 1
        for step, length, symbol in events:
            start = step_time(bars, bar, step)
            end = step_time(bars, bar, step + length) \
                if step + length < STEPS else bar_end
            render_bass(bass, start, end,
                        citypop_bass_note(root - 12, quality, symbol),
                        0.6 * swell, True, cutoff=900.0, drive=1.4)

        # Lead: hook in A's answer bars, B and D; a free solo over C.
        hook_bar = None
        if bar >= lead and section != 'outro':
            if section == 'A' and phrase_bar >= 2:
                hook_bar, shift, vel = phrase_bar - 2, 0, 0.24
            elif section == 'B':
                hook_bar, shift, vel = phrase_bar % 2, 0, 0.26
            elif section == 'D':
                hook_bar, shift, vel = phrase_bar % 2, \
                    12 if phrase_bar >= 2 else 0, 0.24
        if hook_bar is not None:
            for step, length, note in hook[hook_bar]:
                start = step_time(bars, bar, step)
                end = step_time(bars, bar, step + length) \
                    if step + length < STEPS else bar_end
                render_lead(lead_bus, start, end, note + shift, vel,
                            lead_drive)
                lead_note = note
        elif section == 'C':
            for step, length, note in lead_phrase(
                    rng, MAJOR_PENT, key + 24, section, lead_note):
                start = step_time(bars, bar, step)
                end = step_time(bars, bar, min(STEPS, step + length)) \
                    if step + length < STEPS else bar_end
                render_lead(lead_bus, start, end, note, 0.26, lead_drive)
                lead_note = note

        # Drums.
        groove = CITYPOP_DRUMS[section]
        for step in range(STEPS):
            t = step_time(bars, bar, step)
            if groove['kick'][step] == 'k':
                render_drum(drums, noise, t, 'kick',
                            0.85 if step == 0 else 0.6)
            if groove['snare'][step] == 's':
                render_drum(drums, noise, t, 'snare', 0.6)
            hat = groove['hat'][step]
            if hat == 'h':
                render_drum(drums, noise, t, 'hat',
                            0.2 if step % 4 == 0 else 0.13)
            elif hat == 'r':
                render_drum(drums, noise, t, 'ride', 0.13)
        previous = section_of(bar - 1, lead, gates) if bar > 0 else None
        if section != 'intro' and section != previous:
            render_drum(drums, noise, bars[bar], 'crash', 0.28)
        elif gate_bar:
            render_drum(drums, noise, bars[bar], 'hat', 0.35)
        next_section = section_of(bar + 1, lead, gates)
        if (bar == lead - 1 or next_section != section) and \
                section != 'outro':
            for step in (10, 12, 14):
                render_drum(drums, noise, step_time(bars, bar, step), 'tom',
                            0.3 + 0.05 * (step - 10))

    # Final chord rings out.
    final = bars[-1]
    render_epiano(keys, final, final + 3.0, voice_chord(key + 12, 'maj9'),
                  0.34)
    render_bass(bass, final, final + 2.6, key - 12, 0.6, False, cutoff=900.0,
                drive=1.4)
    render_drum(drums, noise, final, 'crash', 0.32)
    render_drum(drums, noise, final, 'kick', 0.85)

    keys.lowpass(6500.0)
    cut.highpass(400.0)
    bass.lowpass(1800.0)
    lead_bus.lowpass(5000.0)

    delay = int(theme.get('delay', 0.3) * SR)
    for bus, feedback in ((lead_bus, 0.32), (keys, 0.16)):
        buf = bus.buf
        for i in range(delay, bus.n):
            buf[i] += buf[i - delay] * feedback

    mix_l = array.array('f', bytes(4 * keys.n))
    mix_r = array.array('f', bytes(4 * keys.n))
    kb, cb, bb, db, lb = keys.buf, cut.buf, bass.buf, drums.buf, lead_bus.buf
    side = int(0.011 * SR)
    peak = 0.0
    for i in range(keys.n):
        center = bb[i] * 0.5 + db[i] * 0.55
        wide = kb[i - side] if i >= side else 0.0
        l = kb[i] * 0.30 + wide * 0.06 + cb[i] * 0.10 + center + lb[i] * 0.17
        r = kb[i] * 0.22 + wide * 0.14 + cb[i] * 0.30 + center + lb[i] * 0.21
        mix_l[i] = l
        mix_r[i] = r
        peak = max(peak, abs(l), abs(r))

    write_track(out_path, mix_l, mix_r, peak, seconds)
    return dict(stage=stage, chapter=stage_chapter(stage),
                theme=theme['name'], seconds=seconds, bars=total_bars,
                gates=gates, lead_in_bars=lead, first_gate=bars[lead],
                last_gate=bars[lead + gates - 1])


def render_stage(stage, csv_path, out_path):
    chapter = stage_chapter(stage)
    theme = theme_for(stage)
    if theme.get('style') == 'citypop':
        return render_citypop(stage, theme, csv_path, out_path)
    groove_set = GROOVES[theme.get('groove', 'rock')]
    tone = TONES[theme.get('tone', 'classic')]
    lead_drive = theme.get('lead_drive', 10.0)
    hook = (make_hook(theme['hook'], theme['lead_scale'], theme['key'] + 12)
            if 'hook' in theme else None)
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
        octave_bass = theme.get('bass') == 'octave'
        for event, (step, length, offset, style) in enumerate(
                riff_events(pattern)):
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
                          1.0, tone)
            render_guitar(guitar_r, start + 0.007, end, root + offset, style,
                          vel, 1.0035, tone)
            bass_note = root - 12 + offset
            if octave_bass and style != 'open' and event % 2 == 1:
                bass_note += 12
            render_bass(bass, start, end, bass_note, 0.55 * swell,
                        style != 'open')

        # Hook melody: answers the riff in A, carries B and opens each
        # final-pressure phrase an octave up.
        hook_bar = None
        if hook is not None and bar >= lead:
            if section == 'A' and phrase_bar >= 2:
                hook_bar, hook_shift, hook_vel = phrase_bar - 2, 0, 0.24
            elif section == 'B':
                hook_bar, hook_shift, hook_vel = phrase_bar % 2, 0, 0.27
            elif section == 'D' and phrase_bar < 2:
                hook_bar, hook_shift, hook_vel = phrase_bar, 12, 0.24
        if hook_bar is not None:
            for step, length, note in hook[hook_bar]:
                start = step_time(bars, bar, step)
                end = step_time(bars, bar, step + length) \
                    if step + length < STEPS else bar_end
                render_lead(lead_bus, start, end, note + hook_shift,
                            hook_vel, lead_drive)
                lead_note = note
        # Lead guitar over the climb and the final pressure.
        elif section in ('C', 'D'):
            for step, length, note in lead_phrase(
                    rng, theme['lead_scale'], key + 24, section, lead_note):
                start = step_time(bars, bar, step)
                end = step_time(bars, bar, min(STEPS, step + length)) \
                    if step + length < STEPS else bar_end
                render_lead(lead_bus, start, end, note,
                            0.32 if section == 'C' else 0.27, lead_drive)
                lead_note = note

        # Drums.
        groove = (DRUMS[section] if section in ('intro', 'outro')
                  else groove_set[section])
        toms = groove.get('tom')
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
            if toms is not None and toms[step] == 't':
                render_drum(drums, noise, t, 'tom', 0.55)
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
            if theme.get('groove') == 'blast':
                render_drum(drums, noise, bars[bar], 'crash', 0.3)
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
    render_guitar(guitar_l, final, final + 2.6, key, 'open', 0.55, 1.0, tone)
    render_guitar(guitar_r, final + 0.007, final + 2.6, key, 'open', 0.55,
                  1.0035, tone)
    render_bass(bass, final, final + 2.6, key - 12, 0.55, False)
    render_drum(drums, noise, final, 'crash', 0.5)
    render_drum(drums, noise, final, 'kick', 0.9)

    for bus in (guitar_l, guitar_r):
        bus.highpass(90.0)
        bus.lowpass(5200.0, poles=2)
    lead_bus.lowpass(5500.0)
    bass.lowpass(900.0)

    # Lead delay (slap echo) for space.
    delay = int(theme.get('delay', 0.27) * SR)
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

    write_track(out_path, mix_l, mix_r, peak, seconds)
    return dict(stage=stage, chapter=chapter,
                theme=theme.get('name', ORIGINAL_STAGES.get(stage)),
                seconds=seconds,
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
