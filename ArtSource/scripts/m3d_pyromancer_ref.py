"""Runecrypt 3D: lembar referensi 4 sisi Pyromancer, dari sprite mage 2D (make_sprites.py MAGE).

Penyihir chibi berjubah ungu: kerudung besar berujung ke belakang, wajah gelap dengan dua mata kuning menyala,
jubah melebar seperti lonceng, selempang jingga, lengan baju lebar, bara api melayang di tangan kanan.
Satuan: 1 unit = 1 tile, tinggi ~1. Output: argv[1]/pyromancer_ref_sheet.png
"""
import os, sys
from PIL import Image, ImageDraw

OUT = sys.argv[1]
os.makedirs(OUT, exist_ok=True)

ROBE, ROBE_D, ROBE_L, INK = (84, 66, 128), (52, 40, 82), (112, 92, 160), (26, 20, 32)
FACE, EYE, EMBER, EMBER_L, SASH, TRIM = (14, 10, 20), (255, 204, 102), (255, 138, 61), (255, 226, 150), (196, 92, 40), (230, 160, 70)
BG, GRID = (40, 40, 46), (56, 56, 64)
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
        self.d.text((12, 8), title, fill=(235, 230, 220))

    def p(self, x, z): return (self.cx + x * S, self.base - z * S)
    def poly(self, ps, fill, outline=INK, w=2, soft=True):
        if soft and len(ps) > 3: ps = chaikin(ps)
        self.d.polygon([self.p(*q) for q in ps], fill=fill, outline=outline, width=w)
    def ell(self, c, rx, rz, fill, outline=INK, w=2):
        x, y = self.p(*c)
        self.d.ellipse([x - rx * S, y - rz * S, x + rx * S, y + rz * S], fill=fill, outline=outline, width=w)
    def line(self, ps, fill=INK, w=2): self.d.line([self.p(*q) for q in ps], fill=fill, width=w, joint='curve')

    def glow(self, c, r, col, core):
        for k, a in ((2.2, 40), (1.6, 80)):
            x, y = self.p(*c)
            ov = Image.new('RGBA', self.img.size, (0, 0, 0, 0))
            ImageDraw.Draw(ov).ellipse([x - r * k * S, y - r * k * S, x + r * k * S, y + r * k * S], fill=col + (a,))
            self.img.paste(Image.alpha_composite(self.img.convert('RGBA'), ov).convert('RGB'))
            self.d = ImageDraw.Draw(self.img)
        self.ell(c, r, r, col, outline=core, w=1)
        self.ell((c[0] - r * 0.3, c[1] + r * 0.3), r * 0.4, r * 0.4, core, outline=core, w=1)


def robe_front(P, back=False):
    P.poly([(-0.13, 0.57), (0.0, 0.58), (0.13, 0.57), (0.17, 0.45), (0.2, 0.3), (0.25, 0.12), (0.28, 0.03), (0.2, -0.005),
            (0.0, 0.01), (-0.2, -0.005), (-0.28, 0.03), (-0.25, 0.12), (-0.2, 0.3), (-0.17, 0.45)], ROBE)
    for x in (-0.13, 0.13): P.line([(x * 0.6, 0.5), (x * 1.5, 0.04)], ROBE_D, 3)     # lipatan kain
    P.poly([(-0.27, 0.03), (0.27, 0.03), (0.26, 0.06), (-0.26, 0.06)], TRIM, outline=INK, w=1, soft=False)  # pinggiran rune
    if not back:
        P.poly([(-0.03, 0.42), (0.03, 0.42), (0.045, 0.2), (0.06, 0.03), (-0.06, 0.03), (-0.045, 0.2)], ROBE_D, soft=False)  # belahan depan
    P.poly([(-0.16, 0.44), (0.16, 0.44), (0.17, 0.4), (-0.17, 0.4)], SASH, soft=False)              # selempang
    tail_x = -0.09 if not back else 0.09
    P.poly([(tail_x, 0.41), (tail_x + 0.04, 0.41), (tail_x + 0.05, 0.27), (tail_x + 0.01, 0.29)], SASH, soft=False)


def sleeves_front(P, ember_x, back=False):
    for s in (1, -1):
        P.poly([(s * 0.12, 0.55), (s * 0.2, 0.5), (s * 0.29, 0.34), (s * 0.22, 0.3), (s * 0.15, 0.42)], ROBE_L if s == 1 else ROBE)
        P.ell((s * 0.255, 0.32), 0.04, 0.025, FACE)                                     # lubang lengan baju
    if not back: P.glow((ember_x, 0.27), 0.045, EMBER, EMBER_L)                         # bara api di tangan kanan


def hood_front(P, back=False):
    P.poly([(-0.2, 0.56), (-0.23, 0.68), (-0.22, 0.8), (-0.16, 0.9), (-0.07, 0.96), (0.0, 0.985), (0.07, 0.96),
            (0.16, 0.9), (0.22, 0.8), (0.23, 0.68), (0.2, 0.56), (0.12, 0.52), (0.0, 0.51), (-0.12, 0.52)], ROBE)
    if back:
        P.line([(0, 0.95), (0, 0.6)], ROBE_D, 3)                                         # jahitan belakang
        return
    P.ell((0, 0.705), 0.13, 0.115, FACE)                                                  # wajah gelap
    for s in (1, -1): P.glow((s * 0.045, 0.72), 0.018, EYE, (255, 245, 210))
    P.line([(-0.18, 0.63), (-0.17, 0.74), (-0.13, 0.83), (-0.05, 0.88)], ROBE_L, 3)                    # tepi kerudung terang


def view_front():
    P = Panel('DEPAN')
    robe_front(P); sleeves_front(P, ember_x=-0.27); hood_front(P)
    return P.img


def view_back():
    P = Panel('BELAKANG')
    robe_front(P, back=True); sleeves_front(P, ember_x=0.27, back=True); hood_front(P, back=True)
    P.glow((0.27, 0.27), 0.045, EMBER, EMBER_L)                                          # bara terlihat dari belakang
    return P.img


def view_side():
    P = Panel('SAMPING KANAN (menghadap kanan)')
    P.poly([(-0.08, 0.57), (0.08, 0.57), (0.12, 0.42), (0.17, 0.22), (0.23, 0.04), (0.17, -0.005), (-0.19, -0.005),
            (-0.25, 0.04), (-0.18, 0.22), (-0.12, 0.42)], ROBE)
    P.poly([(-0.24, 0.04), (0.22, 0.04), (0.21, 0.07), (-0.23, 0.07)], TRIM, outline=INK, w=1, soft=False)
    P.poly([(-0.09, 0.44), (0.09, 0.44), (0.1, 0.4), (-0.1, 0.4)], SASH, soft=False)
    # kerudung dari samping: ujung tertarik ke belakang, wajah gelap di depan
    P.poly([(0.13, 0.57), (0.17, 0.66), (0.17, 0.76), (0.13, 0.86), (0.04, 0.94), (-0.08, 0.975), (-0.17, 0.95),
            (-0.21, 0.86), (-0.2, 0.74), (-0.16, 0.62), (-0.06, 0.53), (0.05, 0.53)], ROBE)
    P.poly([(0.16, 0.74), (0.11, 0.84), (0.07, 0.78), (0.08, 0.64), (0.14, 0.6)], FACE)
    P.glow((0.12, 0.72), 0.016, EYE, (255, 245, 210))
    # lengan kanan (dekat) ke depan, bara di ujung
    P.poly([(-0.04, 0.54), (0.06, 0.5), (0.2, 0.36), (0.15, 0.3), (0.02, 0.4)], ROBE_L)
    P.ell((0.185, 0.33), 0.03, 0.03, FACE)
    P.glow((0.24, 0.27), 0.045, EMBER, EMBER_L)
    return P.img


def view_top():
    P = Panel('ATAS (depan = atas gambar)', top=True)
    P.ell((0, -0.02), 0.27, 0.22, ROBE_D)                                                # tepi jubah
    for s in (1, -1): P.ell((s * 0.2, 0.04), 0.07, 0.09, ROBE_L if s == 1 else ROBE)    # lengan baju
    P.glow((-0.24, 0.14), 0.045, EMBER, EMBER_L)
    P.ell((0, 0.0), 0.2, 0.19, ROBE)                                                     # kerudung
    P.poly([(0, -0.17), (-0.05, -0.05), (0.05, -0.05)], ROBE_D, outline=ROBE_D, w=1)    # ujung kerudung ke belakang
    P.line([(-0.13, 0.13), (0, 0.18), (0.13, 0.13)], ROBE_L, 3)                          # tepi depan kerudung
    return P.img


def sheet():
    panels = [view_front(), view_side(), view_back(), view_top()]
    out = Image.new('RGB', (W * 4 + 260, H + 60), (24, 24, 28))
    for i, p in enumerate(panels): out.paste(p, (i * W, 40))
    d = ImageDraw.Draw(out)
    d.text((10, 12), 'Pyromancer - lembar referensi 3D. Grid 0,1 unit; tinggi ~1 unit = 1 tile.', fill=(235, 230, 220))
    here = os.path.dirname(os.path.abspath(__file__))
    sprite = Image.open(os.path.join(here, '..', '..', 'Assets/Runecrypt/Art/Prototype/mage_0.png')).convert('RGBA')
    big = sprite.resize((224, 224), Image.NEAREST)
    out.paste(big, (W * 4 + 18, 80), big)
    d.text((W * 4 + 18, 60), 'Sprite 2D sekarang', fill=(235, 230, 220))
    return out


img = sheet()
assert img.size == (W * 4 + 260, H + 60)
img.save(os.path.join(OUT, 'pyromancer_ref_sheet.png'))
print('ok')
