"""Runecrypt 3D: lembar referensi 4 sisi Pyromancer, versi gelap (v3).

Dari sprite mage 2D (make_sprites.py MAGE), digelapkan setelah masukan user ("kurang dark").
Ciri yang dipelajari dari penyihir/wraith berkerudung gaya dark fantasy (hanya ciri, tanpa aset):
palet hampir hitam, wajah hilang dalam kerudung dan hanya mata yang menyala tajam, jubah compang-camping
dengan ujung robek yang terbakar, bahu sedikit membungkuk, tangan kurus hangus, bara api dengan asap tipis,
jimat tengkorak kecil di sabuk.
Satuan: 1 unit = 1 tile, tinggi ~1. Output: argv[1]/pyromancer_ref_sheet.png
"""
import os, sys
from PIL import Image, ImageDraw

OUT = sys.argv[1]
os.makedirs(OUT, exist_ok=True)

ROBE, ROBE_D, ROBE_RIM, INK = (40, 31, 56), (22, 17, 31), (74, 56, 98), (10, 8, 14)
VOID, EYE, EYE_CORE = (4, 3, 6), (255, 140, 40), (255, 236, 170)
EMBER, EMBER_L, BURN = (255, 120, 40), (255, 226, 150), (255, 96, 30)
HAND, LEATHER, BONE, SMOKE = (44, 36, 38), (58, 38, 28), (196, 186, 164), (110, 104, 116)
BG, GRID = (30, 30, 36), (46, 46, 54)
S, W, H = 380, 420, 470


def chaikin(ps, n=4):
    """Haluskan poligon tertutup dengan memotong setiap sudut (algoritma Chaikin)."""
    for _ in range(n):
        out = []
        for i in range(len(ps)):
            (x0, z0), (x1, z1) = ps[i], ps[(i + 1) % len(ps)]
            out += [(0.75 * x0 + 0.25 * x1, 0.75 * z0 + 0.25 * z1), (0.25 * x0 + 0.75 * x1, 0.25 * z0 + 0.75 * z1)]
        ps = out
    return ps


class Panel:
    def __init__(self, title, top=False):
        self.img = Image.new('RGB', (W, H), BG)
        self.d = ImageDraw.Draw(self.img)
        self.cx, self.base = W / 2, (H / 2 if top else H - 40)
        for i in range(-5, 6):
            self.d.line([(self.cx + i * S / 10, 30), (self.cx + i * S / 10, H - 10)], fill=GRID)
        for i in range(-2, 12):
            y = self.base - i * S / 10
            if 30 < y < H - 5: self.d.line([(10, y), (W - 10, y)], fill=GRID)
        self.d.text((12, 8), title, fill=(220, 214, 205))

    def p(self, x, z): return (self.cx + x * S, self.base - z * S)

    def poly(self, ps, fill, outline=INK, w=2, soft=True):
        if soft and len(ps) > 3: ps = chaikin(ps)
        self.d.polygon([self.p(*q) for q in ps], fill=fill, outline=outline, width=w)

    def ell(self, c, rx, rz, fill, outline=INK, w=2):
        x, y = self.p(*c)
        self.d.ellipse([x - rx * S, y - rz * S, x + rx * S, y + rz * S], fill=fill, outline=outline, width=w)

    def line(self, ps, fill=INK, w=2): self.d.line([self.p(*q) for q in ps], fill=fill, width=w, joint='curve')

    def halo(self, c, r, col, alpha=60):
        x, y = self.p(*c)
        ov = Image.new('RGBA', self.img.size, (0, 0, 0, 0))
        od = ImageDraw.Draw(ov)
        for k, a in ((2.6, alpha // 3), (1.8, alpha // 2), (1.2, alpha)):
            od.ellipse([x - r * k * S, y - r * k * S, x + r * k * S, y + r * k * S], fill=col + (a,))
        self.img = Image.alpha_composite(self.img.convert('RGBA'), ov).convert('RGB')
        self.d = ImageDraw.Draw(self.img)

    def eye(self, c, s):
        """Mata menyala tajam dan miring ke dalam (bukan lingkaran imut)."""
        self.halo(c, 0.02, EYE, 70)
        x, z = c
        self.poly([(x - 0.026 * s, z + 0.008), (x + 0.02 * s, z - 0.004), (x + 0.012 * s, z - 0.012), (x - 0.022 * s, z - 0.004)],
                  EYE, outline=EYE, w=1, soft=False)
        self.line([(x - 0.012 * s, z), (x + 0.008 * s, z - 0.005)], EYE_CORE, 2)

    def ember(self, c):
        self.halo(c, 0.05, EMBER, 80)
        for i, (dx, dz) in enumerate(((0.0, 0.07), (0.018, 0.11), (-0.01, 0.15))):   # asap tipis naik
            self.ell((c[0] + dx, c[1] + dz), 0.014 + i * 0.006, 0.01 + i * 0.004, SMOKE, outline=SMOKE, w=1)
        self.poly([(c[0], c[1] + 0.06), (c[0] + 0.035, c[1] + 0.01), (c[0] + 0.02, c[1] - 0.03),
                   (c[0] - 0.02, c[1] - 0.03), (c[0] - 0.035, c[1] + 0.01)], EMBER, outline=BURN, w=1)  # nyala api
        self.ell((c[0], c[1] - 0.005), 0.016, 0.018, EMBER_L, outline=EMBER_L, w=1)


def tattered_hem(P, half, z, back=False):
    """Pinggir jubah robek bergerigi, ujung-ujungnya terbakar menyala."""
    depths = (0.0, 0.035, 0.012, 0.045, 0.02, 0.05, 0.015, 0.04, 0.008, 0.03)
    if back: depths = depths[::-1]
    step = 2 * half / 9
    jag = []
    for i in range(10):
        x = -half + i * step
        jag += [(x, z - depths[i]), (x + step / 2, z + 0.01)]
    jag.append((half, z))
    # warna sama dengan jubah dan tanpa garis atas, supaya robekan menyatu dengan kain (bukan pita kotak)
    P.poly([(-half, z + 0.02)] + jag + [(half, z + 0.02)], ROBE, outline=None, soft=False)
    P.line(jag, INK, 2)
    for i in range(10):
        if depths[i] > 0.025:
            P.ell((-half + i * step, z - depths[i] + 0.008), 0.008, 0.006, BURN, outline=BURN, w=1)


def robe_front(P, back=False):
    P.poly([(-0.12, 0.58), (0.0, 0.59), (0.12, 0.58), (0.16, 0.45), (0.19, 0.3), (0.24, 0.14), (0.27, 0.06),
            (-0.27, 0.06), (-0.24, 0.14), (-0.19, 0.3), (-0.16, 0.45)], ROBE)
    for x in (-0.13, 0.13): P.line([(x * 0.5, 0.52), (x * 1.6, 0.08)], ROBE_D, 3)       # lipatan kain
    P.line([(-0.16, 0.45), (-0.21, 0.25), (-0.25, 0.1)], ROBE_RIM, 2)                    # cahaya tepi dari bara
    if not back:
        P.poly([(-0.025, 0.41), (0.025, 0.41), (0.045, 0.2), (0.055, 0.07), (-0.055, 0.07), (-0.045, 0.2)], VOID, soft=False)
    tattered_hem(P, 0.27, 0.06, back)
    P.poly([(-0.15, 0.44), (0.15, 0.44), (0.16, 0.405), (-0.16, 0.405)], LEATHER, soft=False)   # sabuk kulit
    if not back:                                                                         # jimat tengkorak kecil
        P.line([(0.07, 0.405), (0.075, 0.36)], INK, 2)
        P.ell((0.075, 0.345), 0.022, 0.02, BONE)
        for s in (1, -1): P.ell((0.075 + s * 0.008, 0.348), 0.005, 0.005, VOID, outline=VOID, w=1)


def sleeves_front(P, ember_x, back=False):
    for s in (1, -1):
        hx = s * 0.255
        P.poly([(s * 0.11, 0.57), (s * 0.19, 0.53), (s * 0.28, 0.36), (s * 0.27, 0.31), (s * 0.2, 0.3), (s * 0.14, 0.43)], ROBE)
        P.poly([(s * 0.2, 0.3), (s * 0.27, 0.31), (s * 0.25, 0.27), (s * 0.21, 0.27)], ROBE_D, soft=False)  # ujung sobek
        P.poly([(hx - 0.02, 0.29), (hx + 0.02, 0.29), (hx + 0.025, 0.25), (hx + 0.008, 0.22),
                (hx - 0.008, 0.225), (hx - 0.025, 0.25)], HAND, soft=False)             # tangan kurus hangus
    if not back: P.ember((ember_x, 0.2))


def hood_front(P, back=False):
    # bahu sedikit membungkuk: kerudung duduk rendah
    P.poly([(-0.19, 0.55), (-0.22, 0.67), (-0.21, 0.79), (-0.15, 0.9), (-0.06, 0.97), (0.0, 1.0), (0.06, 0.97),
            (0.15, 0.9), (0.21, 0.79), (0.22, 0.67), (0.19, 0.55), (0.1, 0.52), (0.0, 0.51), (-0.1, 0.52)], ROBE)
    if back:
        P.line([(0, 0.98), (0, 0.58)], ROBE_D, 3)
        P.poly([(-0.1, 0.56), (0.1, 0.56), (0.12, 0.5), (0.04, 0.47), (0.0, 0.5), (-0.05, 0.46), (-0.12, 0.5)], ROBE_D, soft=False)
        return
    P.poly([(-0.15, 0.6), (-0.16, 0.72), (-0.11, 0.83), (0.0, 0.87), (0.11, 0.83), (0.16, 0.72), (0.15, 0.6),
            (0.06, 0.56), (-0.06, 0.56)], VOID)                                          # wajah hilang dalam gelap
    P.line([(-0.17, 0.6), (-0.175, 0.72), (-0.12, 0.85), (0.0, 0.9)], ROBE_RIM, 3)      # tepi kerudung
    for s in (1, -1): P.eye((s * 0.05, 0.71), s)


def view_front():
    P = Panel('DEPAN')
    robe_front(P); sleeves_front(P, ember_x=-0.27); hood_front(P)
    return P.img


def view_back():
    P = Panel('BELAKANG')
    robe_front(P, back=True); sleeves_front(P, ember_x=0.27, back=True); hood_front(P, back=True)
    P.ember((0.27, 0.2))
    return P.img


def view_side():
    P = Panel('SAMPING KANAN (menghadap kanan)')
    P.poly([(-0.07, 0.58), (0.09, 0.58), (0.13, 0.42), (0.18, 0.22), (0.23, 0.08), (-0.25, 0.08), (-0.19, 0.22), (-0.12, 0.42)], ROBE)
    tattered_hem(P, 0.24, 0.08)
    P.poly([(-0.09, 0.44), (0.1, 0.44), (0.11, 0.405), (-0.1, 0.405)], LEATHER, soft=False)
    # kerudung dari samping: maju ke depan (membungkuk), ujung jatuh ke belakang
    P.poly([(0.16, 0.56), (0.2, 0.66), (0.2, 0.77), (0.15, 0.87), (0.05, 0.95), (-0.08, 0.99), (-0.18, 0.96),
            (-0.22, 0.86), (-0.2, 0.72), (-0.15, 0.6), (-0.05, 0.52), (0.07, 0.52)], ROBE)
    P.poly([(0.2, 0.66), (0.2, 0.77), (0.15, 0.85), (0.1, 0.78), (0.1, 0.64), (0.16, 0.58)], VOID)
    P.line([(0.2, 0.66), (0.2, 0.77), (0.15, 0.86)], ROBE_RIM, 3)
    P.eye((0.165, 0.71), 1)
    P.poly([(-0.03, 0.55), (0.07, 0.51), (0.22, 0.35), (0.21, 0.3), (0.15, 0.3), (0.03, 0.41)], ROBE)
    P.poly([(0.17, 0.3), (0.21, 0.3), (0.22, 0.25), (0.2, 0.22), (0.17, 0.25)], HAND, soft=False)
    P.ember((0.25, 0.2))
    return P.img


def view_top():
    P = Panel('ATAS (depan = atas gambar)', top=True)
    P.ell((0, -0.02), 0.27, 0.22, ROBE_D)
    for s in (1, -1): P.ell((s * 0.2, 0.05), 0.07, 0.09, ROBE)
    P.ell((0, 0.02), 0.2, 0.19, ROBE)
    P.poly([(0, -0.17), (-0.05, -0.05), (0.05, -0.05)], ROBE_D, outline=ROBE_D, w=1, soft=False)
    P.line([(-0.14, 0.14), (0, 0.2), (0.14, 0.14)], ROBE_RIM, 3)
    P.ember((-0.25, 0.16))
    return P.img


def sheet():
    panels = [view_front(), view_side(), view_back(), view_top()]
    out = Image.new('RGB', (W * 4 + 260, H + 60), (18, 18, 22))
    for i, p in enumerate(panels): out.paste(p, (i * W, 40))
    d = ImageDraw.Draw(out)
    d.text((10, 12), 'Pyromancer v3 (gelap) - lembar referensi 3D. Grid 0,1 unit; tinggi ~1 unit = 1 tile.', fill=(220, 214, 205))
    here = os.path.dirname(os.path.abspath(__file__))
    sprite = Image.open(os.path.join(here, '..', '..', 'Assets/Runecrypt/Art/Prototype/mage_0.png')).convert('RGBA')
    big = sprite.resize((224, 224), Image.NEAREST)
    out.paste(big, (W * 4 + 18, 80), big)
    d.text((W * 4 + 18, 60), 'Sprite 2D sekarang', fill=(220, 214, 205))
    return out


img = sheet()
assert img.size == (W * 4 + 260, H + 60)
img.save(os.path.join(OUT, 'pyromancer_ref_sheet.png'))
print('ok')
