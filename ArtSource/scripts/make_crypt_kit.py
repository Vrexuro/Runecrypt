"""Kit makam M2.5 (Docs/specs/2026-11-24-isi-makam-design.md): variasi lantai, overlay dinding, dekorasi lepas,
Anglo, Guci Abu, Bara Pemulih, nisan, sarkofagus, pilar, dan debu. Semua 16 PPU, palet GDD §7.1.
Pemakaian: python make_crypt_kit.py <folder_output> <file_pratinjau>
           (misalnya Assets/Runecrypt/Art/Crypt ArtSource/prototype/crypt_preview.png)

Aturan keterbacaan (spec §5.5): benda yang bisa dipakai (Anglo, Guci) punya rim light 1 piksel di sisi atas dan
bentuk lebih gemuk; dekorasi lebih gelap, rata, tanpa rim light. Tidak ada merah terang: merah hanya untuk mata musuh.
"""
import math
import os
import random
import sys
from PIL import Image, ImageDraw

OUT, PREVIEW = sys.argv[1], sys.argv[2]
os.makedirs(OUT, exist_ok=True)
os.makedirs(os.path.dirname(PREVIEW) or '.', exist_ok=True)

K = (26, 20, 32)                                  # garis luar
STONE = [(40, 36, 48), (62, 58, 72), (84, 80, 96), (110, 106, 122)]
RIM = (168, 162, 178)                             # rim light benda yang bisa dipakai
CLAY = [(70, 44, 36), (96, 62, 48), (122, 82, 62)]
CLAY_RIM = (176, 132, 100)
BONE_DIM = [(96, 90, 80), (132, 124, 108)]        # tulang dekorasi, lebih gelap dari tulang musuh
IRON = [(52, 48, 58), (78, 74, 86)]
MOSS = [(40, 56, 40), (56, 76, 50)]
FIRE = [(150, 70, 30), (255, 138, 61), (255, 204, 102), (255, 240, 200)]
EMBER = [(190, 90, 40), (255, 138, 61), (255, 226, 150)]
BANNER = [(40, 30, 62), (60, 46, 94)]
WEB = (120, 118, 132)


def new(w=16, h=16):
    return Image.new('RGBA', (w, h), (0, 0, 0, 0))


def put(im, x, y, c, a=255):
    if 0 <= x < im.width and 0 <= y < im.height:
        im.putpixel((x, y), c + (a,))


def from_rows(rows, pal):
    im = new(len(rows[0]), len(rows))
    for y, row in enumerate(rows):
        for x, ch in enumerate(row):
            if ch in pal:
                put(im, x, y, pal[ch])
    return im


# ---------- lantai ----------
def floor_base(rng):
    im = new()
    for y in range(16):
        for x in range(16):
            v = 44 + rng.randint(-5, 5)
            if x in (0, 8) or y in (0, 8):
                v = 28  # sambungan ubin, sama dengan floor.png M1
            put(im, x, y, (v, v + 2, v + 10))
    return im


def crack(im, rng, n, length):
    for _ in range(n):
        x, y = rng.randint(2, 13), rng.randint(1, 6)
        for _ in range(length):
            put(im, x, y, (24, 24, 32))
            x += rng.choice((-1, 0, 1))
            y += 1


def floor_variant(kind, seed):
    rng = random.Random(seed)
    im = floor_base(rng)
    if kind == 'plain':
        crack(im, rng, 1, 3)
    elif kind == 'cracked':
        crack(im, rng, 3, 7)
    elif kind == 'moss':
        for _ in range(3):
            cx, cy = rng.randint(2, 13), rng.randint(2, 13)
            for _ in range(6):
                put(im, cx + rng.randint(-2, 2), cy + rng.randint(-1, 1), rng.choice(MOSS))
    elif kind == 'bones':
        for x0, y0 in ((3, 4), (9, 11)):  # dua tulang kecil miring
            for i in range(5):
                put(im, x0 + i, y0 + i // 2, BONE_DIM[1 if i in (1, 2, 3) else 0])
            put(im, x0, y0 - 1, BONE_DIM[0])
            put(im, x0 + 4, y0 + 3, BONE_DIM[0])
    elif kind == 'rune':
        for t in range(0, 360, 12):  # lingkaran rune pudar
            r = math.radians(t)
            put(im, round(8 + 5 * math.cos(r)), round(8 + 5 * math.sin(r)), (62, 56, 88))
        for dx, dy in ((0, -3), (0, -2), (0, 2), (0, 3), (-3, 0), (3, 0)):
            put(im, 8 + dx, 8 + dy, (70, 62, 98))
    return im


# ---------- overlay dinding (digambar di atas tile dinding baris atas) ----------
def wall_niche():
    im = new()
    for y in range(3, 15):  # ceruk melengkung, sangat gelap
        for x in range(3, 13):
            arch = y >= 6 or math.hypot(x - 7.5, y - 6) <= 4.6
            if arch:
                put(im, x, y, (14, 10, 20))
    for x in range(3, 13):
        put(im, x, 14, STONE[1])  # ambang bawah
    skull = ["..wwww..", ".wwwwww.", ".wkwwkw.", ".wwwwww.", "..wkkw..", "..w..w.."]
    for y, row in enumerate(skull):
        for x, ch in enumerate(row):
            if ch == 'w':
                put(im, 4 + x, 7 + y, BONE_DIM[1] if y < 4 else BONE_DIM[0])
            elif ch == 'k':
                put(im, 4 + x, 7 + y, (14, 10, 20))
    return im


def wall_torch_dead():
    im = new()
    for y in range(8, 12):  # penyangga besi
        put(im, 7, y, IRON[1]); put(im, 8, y, IRON[0])
    for x in range(5, 11):
        put(im, x, 8, IRON[1])
    for y in range(3, 8):  # obor hangus, tanpa api
        put(im, 7, y, (46, 34, 30)); put(im, 8, y, (34, 26, 24))
    put(im, 7, 2, (24, 20, 22)); put(im, 8, 2, (24, 20, 22))
    return im


def wall_cracked():
    rng = random.Random(7)
    im = new()
    for x in range(8, 15):  # satu bata hilang
        for y in range(4, 7):
            put(im, x, y, (14, 10, 20))
    x, y = 3, 0
    while y < 16:
        put(im, x, y, (16, 12, 22))
        x += rng.choice((-1, 0, 1)); y += 1
    return im


# ---------- dekorasi lepas (gelap, tanpa rim) ----------
def web(mirror):
    im = new()
    for i in range(0, 13):
        put(im, i, 0, WEB, 150); put(im, 0, i, WEB, 150)
        put(im, i, i, WEB, 120)
    for r in (4, 8, 12):
        for t in range(0, 91, 6):
            a = math.radians(t)
            put(im, round(r * math.cos(a)), round(r * math.sin(a)), WEB, 110)
    return im.transpose(Image.FLIP_LEFT_RIGHT) if mirror else im


def chain():
    im = new()
    for y in range(0, 15):
        link = y % 4
        if link in (0, 2):
            put(im, 7, y, IRON[1]); put(im, 8, y, IRON[1])
        else:
            put(im, 6 if link == 1 else 9, y, IRON[0]); put(im, 7, y, IRON[0]); put(im, 8, y, IRON[0])
    put(im, 6, 15, IRON[1]); put(im, 9, 15, IRON[1])  # kait
    return im


def bones_pile():
    im = new()
    for x0, y0, d in ((2, 10, 1), (7, 12, -1), (9, 8, 1)):
        for i in range(6):
            put(im, x0 + i, y0 + (i // 3) * d, BONE_DIM[1 if 0 < i < 5 else 0])
    for x, y in ((5, 6), (6, 6), (5, 7), (6, 7), (4, 7)):  # tengkorak kecil pecah
        put(im, x, y, BONE_DIM[1])
    put(im, 5, 7, (20, 16, 24))
    return im


def banner():
    rng = random.Random(3)
    im = new()
    for x in range(3, 13):
        put(im, x, 0, IRON[1])  # tiang gantungan
    for x in range(4, 12):
        bottom = 13 + rng.randint(-3, 1)
        for y in range(1, bottom):
            put(im, x, y, BANNER[1] if x in (5, 6, 9, 10) else BANNER[0])
    for x, y in ((7, 5), (8, 5), (7, 6), (8, 6), (8, 4), (7, 7)):  # lambang api pudar
        put(im, x, y, (120, 70, 44))
    return im


def candle():
    im = new()
    for y in range(9, 14):
        put(im, 7, y, (132, 124, 108)); put(im, 8, y, (110, 104, 92))
    put(im, 7, 8, (24, 20, 22))  # sumbu padam
    for x in range(5, 11):
        put(im, x, 14, (110, 104, 92))  # lelehan lilin
    put(im, 6, 13, (110, 104, 92))
    return im


# ---------- Anglo ----------
def brazier_body(lit):
    rows = [
        "................",
        "................",
        "................",
        "................",
        "................",
        "................",
        "..RRRRRRRRRRRR..",
        ".KcccccccccccccK",
        ".K3222222222223K",
        "..K32222222223K.",
        "...KK3222223KK..",
        ".....K3221K.....",
        ".....K3221K.....",
        "....K332221K....",
        "...K33222211K...",
        "...KKKKKKKKKK...",
    ]
    pal = {'K': K, 'R': RIM, '1': STONE[0], '2': STONE[1], '3': STONE[2],
           'c': (60, 40, 34) if lit else (34, 30, 34)}
    im = from_rows([r[:16].ljust(16, '.') for r in rows], pal)
    if not lit:  # abu dan arang padam
        for x in (4, 7, 9, 12):
            put(im, x, 7, (70, 68, 72))
    return im


def brazier_on(frame):
    im = brazier_body(True)
    rng = random.Random(100 + frame)
    for x in range(4, 12):  # lidah api, tingginya berubah tiap frame
        h = 3 + int(2.5 * (1 + math.sin(x * 1.3 + frame * 1.7))) + rng.randint(0, 1)
        for i in range(h):
            y = 6 - i
            c = FIRE[3] if (i == 0 and 6 <= x <= 9) else FIRE[2] if i < h * 0.4 else FIRE[1] if i < h * 0.8 else FIRE[0]
            put(im, x, y, c)
    return im


# ---------- Guci Abu ----------
def urn(kind):
    if kind == 'a':
        rows = [
            "................",
            "................",
            ".....RRRRRR.....",
            ".....K3333K.....",
            "......K22K......",
            ".....K3222K.....",
            "....K332221K....",
            "...K33322221K...",
            "...K33222221K...",
            "...K32222211K...",
            "...K32222211K...",
            "....K322211K....",
            ".....K22211K....",
            "......K111K.....",
            "......KKKKK.....",
            "................",
        ]
    else:
        rows = [
            "................",
            "......RRRR......",
            "......K33K......",
            ".......K2K......",
            "...KK.K322K.KK..",
            "..K3.K33221K.1K.",
            "..K3K3322221K1K.",
            "...KK3322221KK..",
            "....K3222221K...",
            "....K3222211K...",
            "....K3222211K...",
            ".....K32211K....",
            ".....K32211K....",
            "......K211K.....",
            "......KKKKK.....",
            "................",
        ]
    pal = {'K': K, 'R': CLAY_RIM, '1': CLAY[0], '2': CLAY[1], '3': CLAY[2]}
    return from_rows(rows, pal)


def urn_broken():
    im = new()
    for x in range(4, 12):  # tumpukan abu
        for y in range(12, 15):
            if abs(x - 7.5) < 4 - (14 - y):
                put(im, x, y, (78, 73, 80) if (x + y) % 3 else (104, 98, 104))
    for x, y, c in ((3, 13, 1), (4, 12, 2), (11, 13, 1), (12, 14, 0), (9, 11, 2), (6, 14, 0)):
        put(im, x, y, CLAY[c])  # pecahan tanah liat
    return im


# ---------- Bara Pemulih (unlit, berpendar) ----------
def ember_pickup(frame):
    im = new()
    r_out = 3.6 if frame == 0 else 3.0
    for y in range(16):
        for x in range(16):
            d = math.hypot(x - 7.5, y - 8.5)
            if d < 1.3:
                put(im, x, y, EMBER[2])
            elif d < 2.4:
                put(im, x, y, EMBER[1])
            elif d < r_out:
                put(im, x, y, EMBER[0], 150)
    return im


# ---------- tempat berlindung ----------
def tombstone():
    rows = [
        "................",
        "................",
        ".....KKKKKK.....",
        "....K334333K....",
        "...K33333322K...",
        "...K33K33322K...",
        "...K3KKK3222K...",
        "...K33K33222K...",
        "...K33K32222K...",
        "...K33332221K...",
        "...K33322221K...",
        "...K33222211K...",
        "...K32222211K...",
        "..KmmK222211KK..",
        "..KKKKKKKKKKKK..",
        "................",
    ]
    pal = {'K': K, '1': STONE[0], '2': STONE[1], '3': STONE[2], '4': STONE[3], 'm': MOSS[1]}
    return from_rows(rows, pal)


def sarcophagus():
    im = new(32, 16)
    for y in range(2, 15):
        for x in range(1, 31):
            edge = x in (1, 30) or y in (2, 14)
            c = K if edge else STONE[2] if y == 3 else STONE[1] if y < 12 else STONE[0]
            put(im, x, y, c)
    for x in range(4, 28):
        put(im, x, 6, STONE[0]); put(im, x, 10, STONE[0])  # alur tutup
    for x, y in ((15, 7), (16, 7), (15, 8), (16, 8), (16, 9), (15, 9), (14, 8), (17, 8)):  # lambang api pudar
        put(im, x, y, (110, 66, 42))
    put(im, 5, 13, MOSS[1]); put(im, 6, 13, MOSS[0]); put(im, 26, 13, MOSS[1])
    return im


def pillar():
    im = new(32, 32)
    for y in range(32):
        for x in range(32):
            if y < 5 or y > 27:  # kepala dan alas pilar lebih lebar
                band = y in (0, 31) or x in (0, 31)
                c = K if band else STONE[3] if y in (1, 28) else STONE[2] if y < 5 else STONE[1]
                put(im, x, y, c)
            elif 3 <= x <= 28:
                edge = x in (3, 28)
                flute = (x - 4) % 5 == 0
                c = K if edge else STONE[0] if flute else STONE[2] if x < 12 else STONE[1]
                put(im, x, y, c)
    for y in range(9, 22):  # retak
        put(im, 18 + (y % 3 == 0), y, (24, 22, 30))
    return im


def dust():
    im = new(2, 2)
    for p in ((0, 0), (1, 0), (0, 1), (1, 1)):
        put(im, p[0], p[1], (220, 214, 200), 200 if p == (0, 0) else 120)
    return im


SPRITES = {
    'floor_plain': floor_variant('plain', 11), 'floor_cracked': floor_variant('cracked', 12),
    'floor_moss': floor_variant('moss', 13), 'floor_bones': floor_variant('bones', 14),
    'floor_rune': floor_variant('rune', 15),
    'wall_niche': wall_niche(), 'wall_torch_dead': wall_torch_dead(), 'wall_cracked': wall_cracked(),
    'decor_web_l': web(False), 'decor_web_r': web(True), 'decor_chain': chain(), 'decor_bones': bones_pile(),
    'decor_banner': banner(), 'decor_candle': candle(),
    'brazier_off': brazier_body(False),
    **{f'brazier_on_{i}': brazier_on(i) for i in range(4)},
    'urn_a': urn('a'), 'urn_b': urn('b'), 'urn_broken': urn_broken(),
    **{f'ember_pickup_{i}': ember_pickup(i) for i in range(2)},
    'tombstone': tombstone(), 'sarcophagus': sarcophagus(), 'pillar': pillar(), 'dust': dust(),
}
for name, im in SPRITES.items():
    im.save(os.path.join(OUT, name + '.png'))
assert len(SPRITES) == 28, len(SPRITES)


# ---------- pratinjau: lembar semua sprite + contoh ruangan bercahaya ----------
def wall_tile(rng):
    im = new()
    for y in range(16):
        off = 0 if (y // 4) % 2 == 0 else 4
        for x in range(16):
            mortar = y % 4 == 3 or (x + off) % 8 == 7
            v = 22 if mortar else 62 + rng.randint(-6, 6) - (y % 4) * 4
            put(im, x, y, (v, v, v + 12))
    return im


S = 4
names = list(SPRITES)
cols = 10
cell = 32 * S + 8
sheet_h = ((len(names) + cols - 1) // cols) * cell
TW, TH = 14, 9
room = Image.new('RGBA', (TW * 16, TH * 16), (0, 0, 0, 255))
rng = random.Random(1)
floors = ['floor_plain'] * 7 + ['floor_cracked', 'floor_moss', 'floor_bones', 'floor_rune']
for ty in range(TH):
    for tx in range(TW):
        border = ty == 0 or ty == TH - 1 or tx == 0 or tx == TW - 1
        room.alpha_composite(wall_tile(rng) if border else SPRITES[rng.choice(floors)], (tx * 16, ty * 16))
for tx, n in ((3, 'wall_niche'), (6, 'wall_torch_dead'), (9, 'wall_niche'), (11, 'wall_cracked')):
    room.alpha_composite(SPRITES[n], (tx * 16, 0))
for (x, y), n in {(16, 16): 'decor_web_l', ((TW - 2) * 16, 16): 'decor_web_r', (72, 16): 'decor_banner',
                  (40, 96): 'decor_bones', (168, 40): 'decor_candle', (120, 16): 'decor_chain'}.items():
    room.alpha_composite(SPRITES[n], (x, y))
for (x, y), n in {(24, 104): 'brazier_on_1', (184, 24): 'brazier_off', (176, 104): 'urn_a', (192, 104): 'urn_b',
                  (160, 112): 'urn_broken', (64, 48): 'tombstone', (112, 96): 'ember_pickup_0'}.items():
    room.alpha_composite(SPRITES[n], (x, y))
room.alpha_composite(SPRITES['pillar'], (128, 48))
room.alpha_composite(SPRITES['sarcophagus'], (40, 72))
# Cahaya: Anglo menyala di kiri bawah dan pemain di tengah; sisanya sangat gelap.
px = room.load()
lights = [((32, 110), 40, (255, 150, 70)), ((112, 72), 48, (255, 160, 90))]
for y in range(room.height):
    for x in range(room.width):
        r, g, b, a = px[x, y]
        lr = lg = lb = 0.08
        for (lx, ly), rad, col in lights:
            k = max(0.0, 1 - math.hypot(x - lx, y - ly) / rad)
            k = round(k * 4) / 4  # pita cahaya pixel art
            lr += k * col[0] / 255; lg += k * col[1] / 255; lb += k * col[2] / 255
        px[x, y] = (min(255, int(r * lr)), min(255, int(g * lg)), min(255, int(b * lb)), a)
for (x, y), n in {(24, 104): 'brazier_on_1', (112, 96): 'ember_pickup_0'}.items():
    room.alpha_composite(SPRITES[n], (x, y))  # api dan bara unlit tetap terang

room_big = room.resize((room.width * 3, room.height * 3), Image.NEAREST)
W = max(cols * cell, room_big.width) + 16
sheet = Image.new('RGBA', (W, sheet_h + room_big.height + 40), (20, 16, 26, 255))
d = ImageDraw.Draw(sheet)
for i, n in enumerate(names):
    im = SPRITES[n]
    x, y = (i % cols) * cell + 8, (i // cols) * cell + 8
    sheet.alpha_composite(im.resize((im.width * S, im.height * S), Image.NEAREST), (x, y))
    d.text((x, y + 32 * S - 10), n, fill=(200, 194, 180, 255))
sheet.alpha_composite(room_big, (8, sheet_h + 24))
d.text((8, sheet_h + 8), "Contoh ruangan (cahaya Anglo kiri bawah + pemain di tengah), skala 3x", fill=(200, 194, 180, 255))
sheet.save(PREVIEW)
print(f"{len(SPRITES)} sprite -> {OUT}; pratinjau -> {PREVIEW}")
