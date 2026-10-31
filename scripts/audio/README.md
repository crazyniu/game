# T13a original audio build

Run from the repository root using Python 3.14 (the verified build used 3.14.6):

```sh
python3 -m venv /tmp/perchsort-audio-venv
/tmp/perchsort-audio-venv/bin/pip install -r scripts/audio/requirements.txt
/tmp/perchsort-audio-venv/bin/python scripts/audio/generate.py
/tmp/perchsort-audio-venv/bin/python scripts/audio/analyze.py
```

`generate.py` deterministically writes the 18 WAV files in `audio_src/`, using
seed 130031. `analyze.py` reads the delivered 16-bit PCM, asserts the required
format, durations, levels and loop boundaries, and writes `MANIFEST.md` plus
`analysis.json`. It does not modify the game project. No ffmpeg, sample pack,
soundfont, external recording, network service or generative-audio API is used.

The original C-major score uses diatonic chords, a two-phrase melody, equal-power
stereo positioning and synthetic room taps. Menu is 64 BPM × 16 bars = 60 seconds;
game is 96 BPM × 36 bars = 90 seconds. Harp uses decaying harmonic partials,
wooden bars use damped non-harmonic modes, and flute uses a rounded fundamental,
subtle vibrato and filtered procedural breath. Bird calls are frequency-modulated
oscillators; wings and wood debris use seeded filtered noise. The three star
chimes ascend C6–E6–G6 and each lasts 350ms, matching UI_SPEC's star interval.

BGM notes and room tails wrap around the period. A 3ms raised-cosine fade on
each endpoint makes the PCM join reach digital zero with near-zero slope. This
is a short seam treatment, not a long fade to silence. One-shots have individual
attack/release shaping plus final endpoint fades. Loudness normalization occurs
after synthesis and seam treatment, before PCM quantization.

For effects shorter than 400ms, the meter receives a copy padded to 400ms with
silence because BS.1770/EBU R128 needs a 400ms block. Padding never enters the
WAV. This convention and per-file padding lengths are recorded in the manifest.
True peak is estimated by 4× polyphase oversampling, not a certified hardware
meter. The verifier requires about -16 LUFS (±0.5), true peak ≤ -1 dBTP,
no clipped samples, zero endpoints, and small DC offset. BGM additionally checks
head/tail sample difference, endpoint slope, real zero crossings and non-silent
20ms windows on both sides of the join.

The task's numerical acceptance is automated. Final subjective approval on
headphones/speakers and in-game mixing remains with the coordinator; this asset
task neither changes the runtime audio mixer nor claims a listening review.
