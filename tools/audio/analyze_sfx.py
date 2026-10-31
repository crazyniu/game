#!/usr/bin/env python3
"""Validate delivered PCM and export measured waveform/spectral summaries (stdlib)."""
import cmath
import hashlib
import json
import math
from pathlib import Path
import struct
import wave

ROOT = Path(__file__).resolve().parents[2]
OUTPUT = Path(__file__).resolve().parent
NAMES = ("select", "move", "collect", "win", "error", "undo", "click")


def fft(values):
    n = len(values)
    if n == 1:
        return values
    even, odd = fft(values[::2]), fft(values[1::2])
    rotated = [cmath.exp(-2j * math.pi * k / n) * odd[k] for k in range(n // 2)]
    return [even[k] + rotated[k] for k in range(n // 2)] + [even[k] - rotated[k] for k in range(n // 2)]


def spectrum(samples, rate):
    size = 4096
    power = [0.0] * (size // 2 + 1)
    for start in range(0, len(samples), size // 2):
        window = [samples[start + i] if start + i < len(samples) else 0 for i in range(size)]
        bins = fft([value * (0.5 - 0.5 * math.cos(2 * math.pi * i / (size - 1)))
                    for i, value in enumerate(window)])
        for i in range(len(power)):
            power[i] += abs(bins[i]) ** 2
    frequencies = [i * rate / size for i in range(len(power))]
    total = sum(power)
    local_peaks = [i for i in range(1, len(power) - 1) if power[i] > power[i - 1] and power[i] > power[i + 1]]
    strongest = sorted(local_peaks, key=lambda i: power[i], reverse=True)[:4]
    return power, frequencies, {
        "strongest_peaks_hz": [round(frequencies[i], 1) for i in strongest],
        "power_centroid_hz": round(sum(f * p for f, p in zip(frequencies, power)) / total, 1),
        "energy_above_5khz_percent": round(100 * sum(p for f, p in zip(frequencies, power) if f > 5000) / total, 5),
    }


def main():
    summaries = []
    svg = ['<svg xmlns="http://www.w3.org/2000/svg" width="1200" height="1420" viewBox="0 0 1200 1420">',
           '<rect width="1200" height="1420" fill="#f8fafc"/>',
           '<g font-family="sans-serif" fill="#1e293b"><text x="28" y="30" font-size="20">T6 original SFX: PCM waveform / average power spectrum (0-6 kHz, -80 to 0 dB)</text>']
    for row, name in enumerate(NAMES):
        path = ROOT / "PerchSort/Assets/Audio/Resources/Audio" / (name + ".wav")
        with wave.open(str(path), "rb") as clip:
            channels, width, rate, count = clip.getnchannels(), clip.getsampwidth(), clip.getframerate(), clip.getnframes()
            pcm = struct.unpack(f"<{count * channels}h", clip.readframes(count))
        samples = [value / 32768 for value in pcm]
        peak = max(abs(value) for value in samples)
        rms = math.sqrt(sum(value * value for value in samples) / count)
        edge_count = round(rate * 0.001)
        power, frequencies, spectral = spectrum(samples, rate)
        entry = {
            "name": name, "duration_s": round(count / rate, 6), "sample_rate": rate,
            "bits": width * 8, "channels": channels,
            "peak_dbfs": round(20 * math.log10(peak), 4), "rms_dbfs": round(20 * math.log10(rms), 2),
            "first_sample": pcm[0], "last_sample": pcm[-1],
            "first_1ms_peak": round(max(abs(v) for v in samples[:edge_count]), 6),
            "last_1ms_peak": round(max(abs(v) for v in samples[-edge_count:]), 6),
            "clipped_samples": sum(abs(value) >= 32767 for value in pcm),
            "dc_offset": round(sum(samples) / count, 8),
            "max_adjacent_step": round(max(abs(a - b) for a, b in zip(samples, samples[1:])), 6),
            "sha256": hashlib.sha256(path.read_bytes()).hexdigest(), **spectral,
        }
        assert channels == 1 and width == 2 and rate == 44100, entry
        assert 0 < count / rate <= 1.2 and abs(entry["peak_dbfs"] + 3) < 0.02, entry
        assert pcm[0] == pcm[-1] == 0 and entry["clipped_samples"] == 0, entry
        assert max(entry["first_1ms_peak"], entry["last_1ms_peak"]) < 0.002, entry
        if name == "collect":
            assert 0.8 <= count / rate <= 1.0, entry
        summaries.append(entry)
        y = 70 + row * 190
        svg.append(f'<text x="28" y="{y}" font-size="16">{name} | {count / rate:.3f} s | peak {entry["peak_dbfs"]:.2f} dBFS | RMS {entry["rms_dbfs"]:.2f} dBFS</text>')
        svg.append(f'<path d="M 28 {y + 75} H 558 M 620 {y + 135} H 1150" stroke="#cbd5e1"/>')
        # Min/max buckets preserve transients in the thumbnail.
        for i in range(530):
            chunk = samples[i * count // 530:(i + 1) * count // 530]
            svg.append(f'<path d="M {28+i} {y+75-max(chunk)*65:.2f} V {y+75-min(chunk)*65:.2f}" stroke="#0f766e"/>')
        top = max(power)
        points = []
        for f, p in zip(frequencies, power):
            if f > 6000:
                break
            db = max(-80, 10 * math.log10(max(p / top, 1e-12)))
            points.append(f'{620+530*f/6000:.2f},{y+15-db*1.5:.2f}')
        svg.append(f'<polyline points="{" ".join(points)}" fill="none" stroke="#7c3aed" stroke-width="1.5"/>')
        svg.append(f'<text x="28" y="{y+155}" font-size="12">0 to {count/rate:.3f}s; vertical range ±1 PCM</text>')
        for frequency in (0, 2000, 4000, 6000):
            svg.append(f'<text x="{620+530*frequency/6000:.2f}" y="{y+155}" font-size="12" text-anchor="middle">{frequency/1000:g} kHz</text>')
    svg.append('</g></svg>')
    (OUTPUT / "analysis.json").write_text(json.dumps(summaries, indent=2) + "\n")
    (OUTPUT / "waveforms.svg").write_text("\n".join(svg) + "\n")
    print(json.dumps(summaries, indent=2))
    print("PASS: all seven WAV format, duration, peak, boundary and clipping checks")


if __name__ == "__main__":
    main()
