#!/usr/bin/env python3
"""Original, deterministic synthesis. Python standard library; no sampled audio."""
import math
from pathlib import Path
import random
import struct
import wave

RATE = 44100
OUT = Path(__file__).resolve().parents[2] / "PerchSort/Assets/Audio/Resources/Audio"
TAU = 2 * math.pi


def envelope(t, duration, decay, attack=0.008, release=0.035):
    fade_in = math.sin(min(t / attack, 1) * math.pi / 2) ** 2
    fade_out = math.sin(min((duration - t) / release, 1) * math.pi / 2) ** 2
    return fade_in * fade_out * math.exp(-t / decay)


def tone(duration, frequency, decay, kind="wood", end_frequency=None):
    partials = {
        "wood": [(1, 1), (2, 0.16), (3.01, 0.055)],
        "bell": [(1, 1), (2, 0.20), (3, 0.065), (4.02, 0.025)],
        "horn": [(1, 1), (2, 0.23), (3, 0.11), (4, 0.035)],
    }[kind]
    count = round(duration * RATE)
    result = []
    for i in range(count):
        t = i / RATE
        slope = 0 if end_frequency is None else (end_frequency - frequency) / duration
        phase = TAU * (frequency * t + 0.5 * slope * t * t)
        sample = sum(gain * math.sin(ratio * phase) * math.exp(-t * (ratio - 1) * 2)
                     for ratio, gain in partials)
        result.append(sample * envelope(t, duration, decay))
    return result


def mix(duration, notes):
    result = [0.0] * round(duration * RATE)
    for offset, gain, samples in notes:
        start = round(offset * RATE)
        for i, value in enumerate(samples[:len(result) - start]):
            result[start + i] += gain * value
    return result


def puff():
    duration = 0.18
    samples = tone(duration, 220, 0.042, end_frequency=150)
    rng = random.Random(601)
    smooth = 0.0
    # One-pole lowpass at 850 Hz: a little air, with no sharp white-noise click.
    alpha = 1 - math.exp(-TAU * 850 / RATE)
    for i in range(len(samples)):
        smooth += alpha * (rng.uniform(-1, 1) - smooth)
        samples[i] += 0.20 * smooth * envelope(i / RATE, duration, 0.030)
    return samples


def sounds():
    return {
        "select": tone(0.24, 659.255, 0.085, end_frequency=783.991),
        "move": puff(),
        "collect": mix(0.94, [
            (0.00, 0.85, tone(0.42, 523.251, 0.14, "bell")),
            (0.13, 0.90, tone(0.42, 659.255, 0.14, "bell")),
            (0.26, 0.95, tone(0.45, 783.991, 0.16, "bell")),
            (0.40, 1.00, tone(0.54, 1046.502, 0.18, "bell")),
        ]),
        "win": mix(1.20, [
            (0.00, 0.85, tone(0.23, 523.251, 0.13, "horn")),
            (0.20, 0.90, tone(0.23, 659.255, 0.13, "horn")),
            (0.40, 0.95, tone(0.24, 783.991, 0.14, "horn")),
            (0.62, 0.85, tone(0.58, 1046.502, 0.25, "horn")),
            (0.62, 0.30, tone(0.58, 523.251, 0.23, "wood")),
            (0.62, 0.18, tone(0.58, 659.255, 0.23, "wood")),
        ]),
        "error": tone(0.22, 196.0, 0.065, end_frequency=164.814),
        "undo": tone(0.28, 783.991, 0.10, end_frequency=523.251),
        "click": tone(0.085, 880.0, 0.020),
    }


def write_wav(path, samples):
    # Global 8 ms / 20 ms raised-cosine boundary fades, in addition to note envelopes.
    for i in range(len(samples)):
        fade_in = math.sin(min(i / (RATE * 0.008), 1) * math.pi / 2) ** 2
        fade_out = math.sin(min((len(samples) - 1 - i) / (RATE * 0.020), 1) * math.pi / 2) ** 2
        samples[i] *= fade_in * fade_out
    gain = (10 ** (-3 / 20)) / max(abs(value) for value in samples)
    pcm = [round(value * gain * 32767) for value in samples]
    with wave.open(str(path), "wb") as output:
        output.setnchannels(1)
        output.setsampwidth(2)
        output.setframerate(RATE)
        output.writeframes(struct.pack(f"<{len(pcm)}h", *pcm))


if __name__ == "__main__":
    OUT.mkdir(parents=True, exist_ok=True)
    for name, samples in sounds().items():
        write_wav(OUT / f"{name}.wav", samples)
        print(f"{name}: {len(samples) / RATE:.3f}s -> {OUT / (name + '.wav')}")
