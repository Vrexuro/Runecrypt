"""SFX M1.5 dari empat paket CC0: potong, campur, dan normalkan ke WAV 44,1 kHz mono 16-bit.

Hasil: WAV dengan puncak -6 dBFS (headroom untuk bunyi serempak).
Pemakaian: python make_sfx_cc0.py <folder_cc0_raw> <folder_output>
   misalnya: python ArtSource/scripts/make_sfx_cc0.py ArtSource/cc0_raw Assets/Runecrypt/Audio
Butuh: numpy dan ffmpeg di PATH (alat ArtSource, bukan dependensi game).

Sumber (semua CC0, unduh lalu ekstrak masing-masing ke <folder_cc0_raw>/<nama zip tanpa .zip>/):
  - Fire Staff Collection, LEGIT Audio
      https://opengameart.org/content/fire-staff-sound-effects   (legit_audio_fire_staff_-_free_0.zip)
  - Deep Bone Crack/Break SFX, Zane Little Music
      https://opengameart.org/content/deep-bone-crackbreak-sfx   (deep_breaks.zip)
  - Bones Rattle, congusbongus
      https://opengameart.org/content/bones-rattle               (bones_rattle.zip)
  - Steam Release Sounds, bart (atribusi diminta walau CC0)
      https://opengameart.org/content/steam-release-sounds       (steam_hisses.zip)
  - M2.5: 75 CC0 breaking / falling / hit sfx, rubberduck
      https://opengameart.org/content/75-cc0-breaking-falling-hit-sfx   (sfx_breaking_and_falling.zip)
  - M2.5: Loopable Dungeon Ambience, JaggedStone
      https://opengameart.org/content/loopable-dungeon-ambience  (dungeon_ambient_1.ogg, taruh di dungeon_ambient/)
Kredit dicatat di Docs/GDD.md §14.
"""
import subprocess
import sys
import wave
from pathlib import Path

import numpy as np

SR = 44100
RAW, OUT = Path(sys.argv[1]), Path(sys.argv[2])
OUT.mkdir(parents=True, exist_ok=True)


def load(name):
    path = next(RAW.rglob(name))
    raw = subprocess.run(["ffmpeg", "-v", "error", "-i", str(path), "-ac", "1", "-ar", str(SR), "-f", "f32le", "-"],
                         capture_output=True, check=True).stdout
    return np.frombuffer(raw, dtype=np.float32).astype(np.float64)


def cut(x, start=0.0, dur=None, fade=0.03):
    """Potong, buang hening di depan (di bawah 1% puncak), dan beri fade-out pendek."""
    x = x[int(start * SR):]
    on = np.nonzero(np.abs(x) > np.max(np.abs(x)) * 0.01)[0]
    x = x[on[0]:] if len(on) else x
    x = x[:int(dur * SR)].copy() if dur else x.copy()
    f = min(int(fade * SR), len(x))
    x[-f:] *= np.linspace(1, 0, f)
    return x


def lowpass(x, fc):
    a = 1 - np.exp(-2 * np.pi * fc / SR)
    y, acc = np.empty_like(x), 0.0
    for i, v in enumerate(x):
        acc += a * (v - acc)
        y[i] = acc
    return y


def norm(x, peak=0.89):  # per lapisan; hasil akhir lihat HEADROOM
    m = np.max(np.abs(x))
    return x * (peak / m) if m > 0 else x


def mix(dst, src, at, gain=1.0):
    s = int(at * SR)
    if s + len(src) > len(dst):
        dst = np.concatenate([dst, np.zeros(s + len(src) - len(dst))])
    dst[s:s + len(src)] += src * gain
    return dst


# Puncak akhir -6 dBFS: kena dan mati sering berbunyi di frame yang sama, puncak awalnya berimpit (review M1.5).
HEADROOM = 0.5


def write(name, x):
    x = norm(x, HEADROOM)
    assert np.all(np.isfinite(x)) and np.max(np.abs(x)) <= HEADROOM + 1e-6, name
    with wave.open(str(OUT / f"{name}.wav"), "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes((x * 32767).astype("<i2").tobytes())
    print(f"{name}.wav  {len(x) / SR:.2f} dtk")


# Tembak: tiga klip Fire Staff pendek dengan attack cepat, dipotong 0,55 dtk.
for n in (6, 7, 9):
    write(f"sfx_shot_{n}", cut(load(f"LEGIT_FIR_Fire_Staff {n}-Audio.wav"), dur=0.55))

# Kena: klik tulang ditambah ekor api (Fire Staff 11, diredam di atas 6 kHz).
click = norm(cut(load("2.ogg")))
sizzle = norm(lowpass(cut(load("LEGIT_FIR_Fire_Staff 11-Audio.wav"), start=0.05, dur=0.3), 6000))
write("sfx_hit", mix(click.copy(), sizzle, 0.01, 0.45))

# Tulang Penjaga mati: retak besar, lalu klik tulang yang makin jarang dan pelan seperti memantul.
rattle = [norm(cut(load(f"{i}.ogg"))) for i in range(10)]
rng = np.random.default_rng(7)
death = norm(cut(load("Deep Break 6.wav")))
t, gap, amp = 0.08, 0.05, 0.6
for _ in range(9):
    death = mix(death, rattle[rng.integers(10)], t, amp)
    t += gap * rng.uniform(0.8, 1.3)
    gap *= 1.2
    amp *= 0.82
write("sfx_death_tulang", death)

# Uap: dua desis disambung tumpang tindih 60%, ujung atas diredam supaya tidak menusuk.
h1, h2 = cut(load("steam hisses - Marker #1.wav")), cut(load("steam hisses - Marker #3.wav"))
write("sfx_steam", lowpass(mix(norm(h1), norm(h2), len(h1) / SR * 0.6, 0.8), 9000))

# ---------- M2.5 Isi Makam ----------
# Anglo menyala: Fire Staff 1, "wus" yang membesar 0,3 dtk lalu reda; ujung atas diredam.
write("sfx_brazier_ignite", lowpass(cut(load("LEGIT_FIR_Fire_Staff 1-Audio.wav"), dur=1.1, fade=0.25), 8000))

# Guci pecah: retakan batu sebagai badan tanah liat, ditambah sedikit denting kaca untuk pecahan.
body = norm(lowpass(cut(load("bfh1_rock_breaking_01.ogg")), 7000))
shards = norm(cut(load("bfh1_glass_breaking_01.ogg")))
write("sfx_urn_break", mix(body, shards, 0.02, 0.25))

# Bara Pemulih diambil: letupan api pendek yang lembut.
write("sfx_ember_pickup", lowpass(cut(load("LEGIT_FIR_Fire_Staff 8-Audio.wav"), dur=0.35, fade=0.1), 4000))

# Ambien makam: 40 dtk pertama, ujungnya di-crossfade 2 dtk ke awal supaya loop tanpa sambungan.
amb = cut(load("dungeon_ambient_1.ogg"), dur=42.0, fade=0.001)
xf = int(2.0 * SR)
head, tail = amb[:xf].copy(), amb[-xf:]
ramp = np.linspace(0, 1, xf)
amb = amb[:-xf].copy()
amb[:xf] = head * ramp + tail * (1 - ramp)
write("amb_crypt_loop", amb)
