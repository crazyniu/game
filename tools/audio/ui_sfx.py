#!/usr/bin/env python3
"""Soft, musical UI and reward effects (original synthesis, numpy only).

uv run --no-project --with numpy python tools/audio/ui_sfx.py
Writes PerchSort/audio/{ui_tap,ui_back,undo,sapling_grow,reward,egg_hatch}.wav (44.1 kHz mono 16-bit).
- ui_tap: a soft felted wood tap (every button press, restart, home).
- ui_back: the same tap a fourth lower and softer (close, back).
- undo: a short rewind swish into two falling marimba notes.
- sapling_grow: a rising marimba arpeggio with a leafy rustle and a small sparkle (sapling booster).
- reward: two coin chimes and a sparkle tail (check-in, purchases, gifts, rewards).
- egg_hatch: three small shell ticks, then two cute rising peeps.
"""
from pathlib import Path
import wave
import numpy as np

RATE = 44100
OUT = Path(__file__).resolve().parents[2] / "PerchSort/audio"
rng = np.random.default_rng(7)


def t_axis(d):
    return np.arange(int(d * RATE)) / RATE


def fade(x, a=0.004, r=0.02):
    n = len(x); ai = max(1, int(a * RATE)); ri = max(1, int(r * RATE))
    env = np.ones(n)
    env[:ai] = np.sin(np.linspace(0, np.pi / 2, ai)) ** 2
    env[-ri:] *= np.cos(np.linspace(0, np.pi / 2, ri)) ** 2
    return x * env


def lowpass(x, hz):
    a = 1 - np.exp(-2 * np.pi * hz / RATE); y = np.empty_like(x); s = 0.0
    for i, v in enumerate(x):
        s += a * (v - s); y[i] = s
    return y


def highpass(x, hz):
    return x - lowpass(x, hz)


def tap(freq, d=0.12, decay=0.028):
    """A soft wood-block knock muffled by felt: a warm fundamental, one inharmonic overtone that dies at once,
    a short low thump and no click or pitch glide."""
    t = t_axis(d)
    x = (np.sin(2 * np.pi * freq * t) * np.exp(-t / decay)
         + 0.22 * np.sin(2 * np.pi * freq * 2.71 * t) * np.exp(-t / (decay * 0.3))
         + 0.35 * np.sin(2 * np.pi * freq * 0.5 * t) * np.exp(-t / (decay * 0.6)))
    return fade(lowpass(x, 2600), 0.003, 0.03)


def marimba(freq, d=0.45, decay=0.16, gain=1.0):
    t = t_axis(d)
    x = (np.sin(2 * np.pi * freq * t) * np.exp(-t / decay)
         + 0.35 * np.sin(2 * np.pi * freq * 3.93 * t) * np.exp(-t / (decay * 0.18))
         + 0.10 * np.sin(2 * np.pi * freq * 9.2 * t) * np.exp(-t / (decay * 0.07)))
    return fade(x * gain, 0.002, 0.03)


def coin(freq, d=0.6, decay=0.22):
    t = t_axis(d)
    parts = [(1.0, 1.0), (2.76, 0.5), (5.40, 0.25), (8.93, 0.12)]
    x = sum(g * np.sin(2 * np.pi * freq * r * t) * np.exp(-t / (decay / (1 + 0.6 * r))) for r, g in parts)
    x += 0.15 * highpass(rng.uniform(-1, 1, len(t)), 4000) * np.exp(-t / 0.004)
    return fade(x, 0.001, 0.05)


def sparkle(d=0.5, n=6, base=2400, seed=3):
    r = np.random.default_rng(seed); out = np.zeros(int(d * RATE))
    for k in range(n):
        f = base * 2 ** (r.integers(0, 12) / 12)
        s = marimba(f, 0.18, 0.05, 0.25)
        at = int((0.04 + k * (d - 0.2) / n) * RATE)
        out[at:at + len(s)] += s[:len(out) - at]
    return out


def place(total, items):
    out = np.zeros(int(total * RATE))
    for at, x in items:
        i = int(at * RATE); out[i:i + len(x)] += x[:len(out) - i]
    return out


def norm(x, peak_db):
    return fade(x / np.max(np.abs(x)) * 10 ** (peak_db / 20), 0.002, 0.03)


def write(name, x):
    pcm = (np.clip(x, -1, 1) * 32767).astype("<i2")
    with wave.open(str(OUT / f"{name}.wav"), "wb") as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(RATE); w.writeframes(pcm.tobytes())
    print(name, round(len(x) / RATE, 3), "s")


C5, D5, E5, G5, A5, C6, E6, G6 = 523.25, 587.33, 659.26, 783.99, 880.0, 1046.5, 1318.5, 1568.0

# The former bubble pops drew their click noise from rng; the same draws keep undo and reward byte-identical.
rng.uniform(-1, 1, int(0.11 * RATE)); rng.uniform(-1, 1, int(0.12 * RATE))
write("ui_tap", norm(tap(740), -7))
write("ui_back", norm(tap(555, 0.13, 0.032), -10))
swish = lowpass(highpass(rng.uniform(-1, 1, int(0.12 * RATE)), 600), 2500)
swish *= np.sin(np.linspace(0, np.pi, len(swish))) ** 2
write("undo", norm(place(0.42, [(0.0, 0.35 * swish), (0.08, marimba(G5, 0.3, 0.09)), (0.17, marimba(D5, 0.25, 0.1, 0.9))]), -7))
rustle = lowpass(highpass(rng.uniform(-1, 1, int(0.35 * RATE)), 1500), 5000)
rustle *= np.exp(-t_axis(0.35) / 0.12) * (0.6 + 0.4 * np.sin(2 * np.pi * 18 * t_axis(0.35)))
write("sapling_grow", norm(place(0.85, [(0.0, 0.18 * rustle), (0.0, marimba(C5, 0.4)), (0.08, marimba(E5, 0.4)),
                                         (0.16, marimba(G5, 0.45)), (0.25, marimba(C6, 0.55, 0.22)),
                                         (0.3, 0.6 * sparkle(0.5, 4, 2600, 5))]), -6))
write("reward", norm(place(1.0, [(0.0, coin(2093, 0.6)), (0.12, coin(2637, 0.7)), (0.05, 0.5 * marimba(C6, 0.5, 0.2)),
                                   (0.17, 0.5 * marimba(E6, 0.5, 0.2)), (0.3, 0.7 * sparkle(0.65, 6, 2800, 9))]), -5))


def tick(d=0.03, hz=3200):
    t = t_axis(d)
    x = highpass(lowpass(rng.uniform(-1, 1, len(t)), hz * 1.6), hz * 0.6) * np.exp(-t / 0.006)
    return fade(x + 0.5 * np.sin(2 * np.pi * hz * t) * np.exp(-t / 0.005), 0.0005, 0.005)


def peep(f0, f1, d=0.13):
    t = t_axis(d)
    u = t / d
    f = f0 + (f1 - f0) * np.sin(u * np.pi / 2) + 40 * np.sin(2 * np.pi * 26 * t)
    ph = 2 * np.pi * np.cumsum(f) / RATE
    return fade((np.sin(ph) + 0.12 * np.sin(2 * ph)) * np.sin(u * np.pi) ** 0.7, 0.003, 0.01)


write("egg_hatch", norm(place(0.55, [(0.0, 0.5 * tick(hz=2600)), (0.07, 0.7 * tick(hz=3000)), (0.13, tick(hz=3400)),
                                      (0.2, 0.8 * peep(1500, 2300)), (0.36, 0.9 * peep(1700, 2600, 0.15))]), -8))
