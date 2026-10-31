"""Original Perch Sort score and procedural Foley; no samples or soundfonts."""
from pathlib import Path
import wave

import numpy as np
from scipy.signal import butter, sosfilt

from analyze import SR, OUT, loudness

RNG = np.random.default_rng(130031)
TAU = 2 * np.pi


def hz(midi):
    return 440 * 2 ** ((midi - 69) / 12)


def fade(x, attack=.006, release=.025):
    x = x.copy()
    a, r = round(attack * SR), round(release * SR)
    ramp_a = np.sin(np.linspace(0, np.pi / 2, a)) ** 2
    ramp_r = np.sin(np.linspace(np.pi / 2, 0, r)) ** 2
    if x.ndim == 2:
        ramp_a, ramp_r = ramp_a[:, None], ramp_r[:, None]
    x[:a] *= ramp_a
    x[-r:] *= ramp_r
    x[0] = x[-1] = 0
    return x


def tone(midi, seconds, instrument):
    t = np.arange(round(seconds * SR)) / SR
    f = hz(midi)
    if instrument == "wood":
        # Soft wooden fundamental with short, non-harmonic bar modes.
        x = (np.sin(TAU*f*t)*np.exp(-4.3*t)
             + .22*np.sin(TAU*f*2.76*t)*np.exp(-12*t)
             + .08*np.sin(TAU*f*5.4*t)*np.exp(-22*t))
        return fade(x, .004, .06)
    if instrument == "harp":
        x = sum((1 / k**1.8) * np.sin(TAU*f*k*t) * np.exp(-(2.1+.7*k)*t)
                for k in range(1, 7))
        return fade(x, .008, .12)
    if instrument == "bell":
        x = (np.sin(TAU*f*t)*np.exp(-4*t)
             + .24*np.sin(TAU*f*2.01*t)*np.exp(-6*t)
             + .09*np.sin(TAU*f*3.98*t)*np.exp(-9*t))
        return fade(x, .004, .07)
    # Rounded flute with quiet breath and delayed vibrato.
    phase = TAU*f*t + .026*np.sin(TAU*4.6*t)*(1-np.exp(-4*t))
    breath = sosfilt(butter(2, [1100, 4200], btype="bandpass", fs=SR, output="sos"),
                     RNG.normal(size=len(t)))
    x = np.sin(phase) + .13*np.sin(2*phase) + .03*np.sin(3*phase) + .018*breath
    x *= .85 + .15*np.sin(np.pi*t/seconds)
    return fade(x, .075, min(.20, seconds*.35))


def place(buffer, note, start, gain=1, pan=0, wrap=False):
    stereo = note[:, None] * np.array([np.cos((pan+1)*np.pi/4),
                                      np.sin((pan+1)*np.pi/4)]) * gain
    offset = round(start*SR)
    if wrap:
        idx = (np.arange(len(note)) + offset) % len(buffer)
        buffer[idx] += stereo
    else:
        count = min(len(note), len(buffer)-offset)
        buffer[offset:offset+count] += stereo[:count]


def room(buffer, loop):
    wet = buffer.copy()
    for delay, gain in ((.053, .11), (.097, .075), (.173, .045)):
        shift = round(delay*SR)
        if loop:
            wet += np.roll(buffer[:, ::-1], shift, axis=0)*gain
        else:
            wet[shift:] += buffer[:-shift, ::-1]*gain
    return wet


def save(name, samples, loop=False):
    if samples.ndim == 1:
        samples = samples[:, None]
    # Mild, deliberate tape-like rounding of the synthesized voices.
    samples = np.tanh(samples*.65)/.65
    samples = fade(samples, .003 if loop else .004, .003 if loop else .018)
    samples *= 10 ** ((-16-loudness(samples))/20)
    assert np.max(np.abs(samples)) < .89, (name, np.max(np.abs(samples)))
    pcm = np.rint(samples*32768).astype("<i2")
    with wave.open(str(OUT / (name+".wav")), "wb") as wav:
        wav.setnchannels(samples.shape[1])
        wav.setsampwidth(2)
        wav.setframerate(SR)
        wav.writeframes(pcm.tobytes())
    print(f"Rendered {name}: {len(samples)/SR:.3f}s", flush=True)


def music(name, bpm, bars):
    beat = 60/bpm
    x = np.zeros((round(bars*4*beat*SR), 2))
    game = name == "bgm_game"
    # C, Am, F, G, Em, Am, Dm, G: all diatonic; cadence returns G -> C.
    chords = [(48, 52, 55), (45, 48, 52), (41, 45, 48), (43, 47, 50),
              (40, 43, 47), (45, 48, 52), (41, 45, 50), (43, 47, 50)]
    melody = [
        [(0, 76, 1), (1.5, 79, .6), (2.5, 76, 1)],
        [(0, 72, 1.4), (2, 76, .8), (3, 74, .7)],
        [(.5, 72, 1), (2, 69, 1.3)],
        [(0, 71, 1), (1.5, 74, 1), (3, 79, .7)],
        [(0, 76, 1.2), (2, 74, .8), (3, 71, .7)],
        [(.5, 72, 1), (2, 76, 1.4)],
        [(0, 74, 1), (1.5, 77, .8), (3, 76, .6)],
        [(0, 74, 1.3), (2, 71, 1.2)],
    ]
    for bar in range(bars):
        c = bar % 8
        # Final four bars use I-IV-V-V, returning naturally to the opening I.
        if game and bar >= bars-4:
            c = [0, 2, 3, 7][bar-(bars-4)]
        chord = chords[c]
        base = bar*4*beat
        arp = [0, 1, 2, 1, 0, 2, 1, 2] if game else [0, 2, 1, 2]
        spacing = .5 if game else 1
        for n, degree in enumerate(arp):
            place(x, tone(chord[degree]+12, 1.45, "harp"),
                  base+n*spacing*beat, .16 if game else .21,
                  -.32 if n % 2 == 0 else .32, True)
        place(x, tone(chord[0], 2.5*beat, "flute"), base, .115, -.1, True)
        for onset, pitch, length in melody[c]:
            # Second phrase answers in wood, third combines sparse wood/flute.
            voice = "wood" if game or (bar//8) % 2 else "flute"
            volume = .235 if voice == "wood" else .13
            place(x, tone(pitch, max(.4, length*beat), voice),
                  base+onset*beat, volume, .16, True)
        if game and bar % 4 == 2:
            place(x, tone(chord[2]+24, .65, "bell"), base+3.5*beat, .045, -.3, True)
    save(name, room(x, True), True)


def chirps():
    for i, (duration, start, end) in enumerate([
        (.21, 1600, 2500), (.25, 2050, 1450), (.29, 1350, 2200),
        (.32, 2450, 3100), (.36, 1750, 2600)], 1):
        t = np.arange(round(duration*SR))/SR
        u = t/duration
        freq = start+(end-start)*np.sin(np.pi*u*.8) + 70*np.sin(TAU*18*t)
        phase = TAU*np.cumsum(freq)/SR
        envelope = np.sin(np.pi*u)**1.6 * (.7+.3*np.cos(TAU*(5+i)*t))
        x = envelope*(np.sin(phase)+.11*np.sin(2*phase))
        save(f"chirp_{i:02}", fade(x, .008, .035))


def effects():
    t = np.arange(round(.3*SR))/SR
    noise = sosfilt(butter(2, [250, 3200], btype="bandpass", fs=SR, output="sos"),
                     RNG.normal(size=len(t)))
    pulses = sum(np.exp(-((t-center)/.023)**2) for center in (.04, .105, .17, .235))
    flutter = .60*np.tanh(noise*1.5)*pulses + .10*np.sin(TAU*145*t)*pulses
    save("flutter", fade(flutter, .01, .035))

    t = np.arange(round(.5*SR))/SR
    phase = TAU*np.cumsum(740-360*(t/.5)**.8)/SR
    save("sleep_z", fade((np.sin(phase)+.09*np.sin(2*phase))*np.sin(np.pi*t/.5), .04, .13))

    x = np.zeros((round(.8*SR), 2))
    for j, pitch in enumerate((72, 76, 79)):
        place(x, tone(pitch, .48, "harp"), j*.15, .7, (j-1)*.2)
    place(x, tone(91, .34, "bell"), .44, .28, .15)
    save("nest_home", room(x, False))

    t = np.arange(round(.6*SR))/SR
    crack = np.zeros_like(t)
    for start, level in ((0, 1), (.075, .7), (.16, .5), (.27, .33), (.39, .20)):
        elapsed = np.maximum(0, t-start)
        envelope = (t >= start)*(1-np.exp(-elapsed/.003))*np.exp(-elapsed/.07)
        grit = sosfilt(butter(2, [450, 6500], btype="bandpass", fs=SR, output="sos"),
                       RNG.normal(size=len(t)))
        wood = sum(np.sin(TAU*f*elapsed)*np.exp(-elapsed*14) for f in (182, 317, 531))*.12
        crack += level*envelope*(.40*np.tanh(grit*1.8)+wood)
    save("crate_open", fade(crack, .004, .07))

    for i, pitch in enumerate((84, 88, 91), 1):
        save(f"star_{i}", tone(pitch, .35, "bell"))

    x = np.zeros((2*SR, 2))
    for start, pitch, length in ((0, 72, .4), (.23, 76, .4), (.46, 79, .4),
                                 (.74, 84, 1.15)):
        place(x, tone(pitch, length, "flute"), start, .34, .08)
        place(x, tone(pitch, length, "wood"), start, .55, -.2)
    for pitch in (60, 64, 67):
        place(x, tone(pitch, 1.15, "harp"), .74, .18, -.12)
    place(x, tone(96, .9, "bell"), 1.03, .15, .3)
    save("win_fanfare", room(x, False))

    x = np.zeros((round(.45*SR), 2))
    place(x, tone(79, .32, "wood"), 0, .65, -.1)
    place(x, tone(84, .29, "bell"), .15, .4, .1)
    save("hint", x)
    save("ui_tap", tone(76, .10, "wood"))
    t = np.arange(round(.18*SR))/SR
    phase = TAU*np.cumsum(660-220*t/.18)/SR
    save("ui_back", fade(np.sin(phase)*np.exp(-9*t), .008, .065))


if __name__ == "__main__":
    OUT.mkdir(exist_ok=True)
    music("bgm_menu", 64, 16)
    music("bgm_game", 96, 36)
    chirps()
    effects()
