"""Sprite juice M1.5 dan M2: digit angka damage, serpihan tulang, tumpukan abu, bara, cincin uap, kepulan abu,
lalu (M2) orb relic, cincin Kobaran, ikon Buta/Stun, dan penanda homing.
Pemakaian: python make_juice_sprites.py <folder_output>   (misalnya Assets/Runecrypt/Art/Juice)"""
import math
import os
import random
import sys
from PIL import Image

OUT = sys.argv[1]
os.makedirs(OUT, exist_ok=True)

WHITE = (255, 255, 255)
BONE, BONE_SHADOW = (217, 209, 189), (150, 140, 120)  # bone = warna uap/tulang di make_proto_fx
ASH_DARK, ASH, ASH_LIGHT = (52, 48, 56), (78, 73, 80), (104, 98, 104)
EMBER, EMBER_HOT = (255, 138, 61), (255, 204, 102)    # jingga dan kuning bola api
STEAM = (217, 209, 189)

# Digit 3x5, baris atas ke bawah. Putih: warna diberi SpriteRenderer.color di Unity.
DIGITS = {
    0: ["###", "#.#", "#.#", "#.#", "###"],
    1: [".#.", "##.", ".#.", ".#.", "###"],
    2: ["###", "..#", "###", "#..", "###"],
    3: ["###", "..#", ".##", "..#", "###"],
    4: ["#.#", "#.#", "###", "..#", "..#"],
    5: ["###", "#..", "###", "..#", "###"],
    6: ["###", "#..", "###", "#.#", "###"],
    7: ["###", "..#", ".#.", ".#.", ".#."],
    8: ["###", "#.#", "###", "#.#", "###"],
    9: ["###", "#.#", "###", "..#", "###"],
}


def save(im, name):
    im.save(os.path.join(OUT, name + ".png"))
    return name


made = []
for d, rows in DIGITS.items():
    im = Image.new("RGBA", (3, 5), (0, 0, 0, 0))
    for y, row in enumerate(rows):
        for x, ch in enumerate(row):
            if ch == "#":
                im.putpixel((x, y), WHITE + (255,))
    made.append(save(im, f"digit_{d}"))

# Serpihan tulang 4x4: tiga bentuk (batang miring, ruas, pecahan).
SHARDS = [
    ["#...", ".#..", "..#.", "...s"],
    [".##.", "#ss#", ".##.", "...."],
    ["##..", "#s#.", ".s..", "...."],
]
for i, rows in enumerate(SHARDS):
    im = Image.new("RGBA", (4, 4), (0, 0, 0, 0))
    for y, row in enumerate(rows):
        for x, ch in enumerate(row):
            if ch == "#":
                im.putpixel((x, y), BONE + (255,))
            elif ch == "s":
                im.putpixel((x, y), BONE_SHADOW + (255,))
    made.append(save(im, f"bone_shard_{i}"))

# Tumpukan abu 16x16: gundukan rendah di bagian bawah tile, dengan serpihan tulang kecil.
rng = random.Random(16)
pile = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
for y in range(16):
    for x in range(16):
        e = ((x - 7.5) / 6.5) ** 2 + ((y - 11.5) / 3.2) ** 2
        if e < 1.0:
            shade = ASH_LIGHT if (e < 0.35 and y < 11) else (ASH if e < 0.75 else ASH_DARK)
            if rng.random() < 0.12:
                shade = ASH_DARK
            pile.putpixel((x, y), shade + (255,))
for (x, y) in [(4, 11), (5, 11), (11, 12), (9, 10)]:
    pile.putpixel((x, y), BONE_SHADOW + (255,))
made.append(save(pile, "ash_pile"))

# Bara: hanya beberapa piksel di dalam gundukan; dirender unlit dan berkedip di Unity.
embers = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
for (x, y), c in zip([(6, 10), (8, 11), (10, 11), (7, 12), (9, 12)], [EMBER, EMBER_HOT, EMBER, EMBER, EMBER_HOT]):
    assert pile.getpixel((x, y))[3] == 255, "bara harus di dalam gundukan"
    embers.putpixel((x, y), c + (255,))
made.append(save(embers, "ash_embers"))

# Cincin uap 32x32 setebal 1 piksel.
ring = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
for y in range(32):
    for x in range(32):
        if abs(math.hypot(x - 15.5, y - 15.5) - 14.5) < 0.55:
            ring.putpixel((x, y), STEAM + (220,))
made.append(save(ring, "steam_ring"))

# Kepulan abu 6x6 untuk partikel.
puff = Image.new("RGBA", (6, 6), (0, 0, 0, 0))
for y in range(6):
    for x in range(6):
        d = math.hypot(x - 2.5, y - 2.5) / 2.8
        if d < 1.0:
            puff.putpixel((x, y), ASH + (int(230 * (1 - d) ** 0.7),))
made.append(save(puff, "ash_puff"))

# --- M2 ---
TOMB_DARK, TOMB_MID = (26, 20, 32), (84, 66, 128)   # palet makam GDD §7.1
GREY, GREY_LIGHT = (120, 116, 130), (170, 166, 178)
STUN_YELLOW = (255, 226, 150)

# Orb relic 16x16: bola rune gelap bertepi ungu dengan inti bara, supaya terbaca sebagai "benda berharga".
orb = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
for y in range(16):
    for x in range(16):
        d = math.hypot(x - 7.5, y - 7.5)
        if d < 6.0:
            c = TOMB_MID if d > 4.8 else TOMB_DARK
            if d < 2.2:
                c = EMBER_HOT if d < 1.2 else EMBER
            orb.putpixel((x, y), c + (255,))
for (x, y) in [(7, 3), (8, 3), (3, 7), (12, 8), (7, 12)]:   # empat titik rune di cangkang
    orb.putpixel((x, y), EMBER + (255,))
made.append(save(orb, "relic_orb"))

# Cincin Kobaran 48x48 (3 unit) setebal 2 piksel: luar jingga, dalam kuning.
blaze = Image.new("RGBA", (48, 48), (0, 0, 0, 0))
for y in range(48):
    for x in range(48):
        r = math.hypot(x - 23.5, y - 23.5)
        if 21.5 <= r < 22.5:
            blaze.putpixel((x, y), EMBER_HOT + (255,))
        elif 22.5 <= r < 23.5:
            blaze.putpixel((x, y), EMBER + (230,))
made.append(save(blaze, "blaze_ring"))

# Ikon status 5x5 di atas kepala musuh. Buta: pusaran abu-abu. Stun: bintik kuning.
ICONS = {
    "icon_blind": ([".###.", "#...#", "#.#.#", "#..#.", ".##.."], GREY, GREY_LIGHT),
    "icon_stun": (["#...#", ".#.#.", "..#..", ".#.#.", "#...#"], STUN_YELLOW, EMBER_HOT),
}
for name, (rows, c1, c2) in ICONS.items():
    im = Image.new("RGBA", (5, 5), (0, 0, 0, 0))
    for y, row in enumerate(rows):
        for x, ch in enumerate(row):
            if ch == "#":
                im.putpixel((x, y), (c2 if (x + y) % 2 == 0 else c1) + (255,))
    made.append(save(im, name))

# Penanda homing 3x3: bara kecil berbentuk plus (GDD §5.5 aturan 2).
marker = Image.new("RGBA", (3, 3), (0, 0, 0, 0))
for (x, y), c in [((1, 0), EMBER), ((0, 1), EMBER), ((1, 1), EMBER_HOT), ((2, 1), EMBER), ((1, 2), EMBER)]:
    marker.putpixel((x, y), c + (255,))
made.append(save(marker, "homing_marker"))

# Self-check: jumlah dan ukuran setiap file.
SIZES = {"digit": (3, 5), "bone": (4, 4), "ash_pile": (16, 16), "ash_embers": (16, 16), "steam_ring": (32, 32), "ash_puff": (6, 6),
         "relic_orb": (16, 16), "blaze_ring": (48, 48), "icon_": (5, 5), "homing_marker": (3, 3)}
assert sum(n.startswith("digit_") for n in made) == 10
for n in made:
    key = next(k for k in SIZES if n.startswith(k))
    assert Image.open(os.path.join(OUT, n + ".png")).size == SIZES[key], n
print("\n".join(made))
