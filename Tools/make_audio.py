"""Makes every sound and both music loops for Riverlight (Lab 08), so there's no third-party audio.

Run: python3 Tools/make_audio.py Assets/Audio
Needs numpy. Writes 16-bit mono WAV files at 44.1 kHz.
"""
import sys, os, wave
import numpy as np

RATE = 44100
rng = np.random.default_rng(23051)


def t(seconds):
    return np.arange(int(seconds * RATE)) / RATE


def hz(midi):
    return 440.0 * 2 ** ((midi - 69) / 12)


def note(freq, seconds, attack=0.004, decay=6.0, harmonics=(1.0, 0.35, 0.12)):
    x = t(seconds)
    tone = sum(a * np.sin(2 * np.pi * freq * (k + 1) * x) for k, a in enumerate(harmonics))
    return tone * np.minimum(x / attack, 1.0) * np.exp(-decay * x)


def place(buf, sound, at):
    i = int(at * RATE)
    end = min(len(buf), i + len(sound))
    buf[i:end] += sound[: end - i]


def seq(notes, gap, seconds, decay=8.0, harmonics=(1.0, 0.3, 0.1)):
    buf = np.zeros(int(seconds * RATE))
    for i, m in enumerate(notes):
        place(buf, note(hz(m), seconds - i * gap, decay=decay, harmonics=harmonics), i * gap)
    return buf


def noise(seconds, smooth=8):
    n = rng.standard_normal(int(seconds * RATE))
    return np.convolve(n, np.ones(smooth) / smooth, mode="same")


def save(path, x, peak_db):
    x = x / (np.max(np.abs(x)) + 1e-9) * 10 ** (peak_db / 20)
    data = (np.clip(x, -1, 1) * 32767).astype(np.int16)
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(RATE)
        w.writeframes(data.tobytes())
    print(f"{os.path.basename(path)}: {len(x) / RATE:.2f} s, peak {peak_db} dBFS")


def loop(bars, bar, build):
    """Render a loop with a second of spare room, then fold the tail back onto the start so it loops cleanly."""
    n = int(bars * bar * RATE)
    buf = np.zeros(n + RATE)
    build(buf)
    out = buf[:n].copy()
    out[: RATE] += buf[n:]
    return out


def calm():
    """16 s at 60 bpm: Am F C G pads with a slow, soft arpeggio. Menus and results."""
    bar = 4.0
    chords = [[57, 60, 64], [53, 57, 60], [48, 52, 55], [55, 59, 62]]

    def build(buf):
        for c, chord in enumerate(chords):
            start = c * bar
            x = t(bar + 1.0)
            env = np.minimum(x / 0.8, 1.0) * np.clip((bar + 1.0 - x) / 1.0, 0, 1)
            pad = sum(np.sin(2 * np.pi * hz(m) * x) + 0.5 * np.sin(2 * np.pi * hz(m) * 1.004 * x) for m in chord)
            place(buf, 0.16 * pad * env, start)
            for k in range(4):
                place(buf, 0.18 * note(hz(chord[k % 3] + 12), 1.2, decay=3.5), start + k * 1.0)
    return loop(4, bar, build)


def river():
    """16 s at 120 bpm: the same chords, quicker, with a bass line, eighth-note arpeggios and a soft hat."""
    bar = 2.0
    chords = [[57, 60, 64], [53, 57, 60], [48, 52, 55], [55, 59, 62]] * 2

    def build(buf):
        for c, chord in enumerate(chords):
            start = c * bar
            x = t(bar + 0.4)
            env = np.minimum(x / 0.2, 1.0) * np.clip((bar + 0.4 - x) / 0.4, 0, 1)
            pad = sum(np.sin(2 * np.pi * hz(m) * x) for m in chord)
            place(buf, 0.10 * pad * env, start)
            for k in range(4):                                      # bass on every beat
                place(buf, 0.45 * note(hz(chord[0] - 24), 0.45, attack=0.01, decay=5, harmonics=(1.0, 0.5, 0.2)), start + k * 0.5)
            for k in range(8):                                      # eighth-note arpeggio
                m = chord[k % 3] + 12 * (1 + (k // 3) % 2)
                place(buf, 0.20 * note(hz(m), 0.3, decay=10), start + k * 0.25)
                hat = noise(0.05, 2) * np.exp(-90 * t(0.05))       # a soft hi-hat on the off-beats
                if k % 2 == 1:
                    place(buf, 0.10 * hat, start + k * 0.25)
    return loop(8, bar, build)


def sweep(f0, f1, seconds, decay=8):
    x = t(seconds)
    f = np.linspace(f0, f1, len(x))
    return np.sin(2 * np.pi * np.cumsum(f) / RATE) * np.minimum(x / 0.005, 1) * np.exp(-decay * x)


def main(out):
    os.makedirs(out, exist_ok=True)
    tracks = {
        # music: quieter than the effects
        "MusicCalm": (calm(), -10), "MusicRiver": (river(), -10),
        # UI
        "UiHover": (note(2400, 0.04, decay=120, harmonics=(1.0,)), -18),
        "UiPress": (note(1300, 0.07, decay=60, harmonics=(1.0, 0.4)), -8),
        "UiBack": (seq([76, 69], 0.06, 0.25, decay=18), -9),
        "UiOpen": (sweep(500, 1400, 0.18, decay=14) + 0.4 * noise(0.18, 20) * np.exp(-20 * t(0.18)), -10),
        "UiClose": (sweep(1300, 450, 0.16, decay=16) + 0.3 * noise(0.16, 20) * np.exp(-25 * t(0.16)), -10),
        "UiPause": (seq([72, 67], 0.09, 0.4, decay=8), -7),
        "UiResume": (seq([67, 72], 0.09, 0.4, decay=8), -7),
        "UiSuccess": (seq([72, 76, 79, 84], 0.07, 0.7, decay=6), -6),
        "UiError": (sweep(220, 180, 0.12, decay=10) * np.sign(np.sin(2 * np.pi * 30 * t(0.12))) * 0.6
                    + np.concatenate([np.zeros(int(0.14 * RATE)), sweep(200, 170, 0.12, decay=10)])[: int(0.12 * RATE)], -9),
        # gameplay
        "GameStart": (sweep(300, 900, 0.5, decay=4) * 0.6 + 0.5 * noise(0.5, 30) * np.exp(-6 * t(0.5)), -8),
        "Spark": (seq([88, 95], 0.05, 0.4, decay=10), -6),
        "Hit": ((0.9 * noise(0.35, 12) + 1.2 * np.sin(2 * np.pi * (150 - 200 * t(0.35)) * t(0.35))) * np.exp(-12 * t(0.35)), -3),
        "Die": (seq([67, 63, 60, 55], 0.18, 1.4, decay=3, harmonics=(1.0, 0.5, 0.25, 0.12)), -4),
        "Win": (seq([72, 76, 79, 84, 88], 0.11, 1.4, decay=3.5), -4),
    }
    for name, (x, peak) in tracks.items():
        save(os.path.join(out, name + ".wav"), x, peak)


main(sys.argv[1] if len(sys.argv) > 1 else "Assets/Audio")
