"""SPIKE 3D (boleh dibuang): lembar referensi 4 sisi Tulang Penjaga, versi 2.

Versi 1 (bola dan elips) tidak terbaca sebagai tengkorak. Versi ini memakai ciri kerangka stylized ala
low-poly (dipelajari dari gaya paket CC0 seperti KayKit Skeletons, tanpa menyalin aset): rongga mata besar
dan miring, tulang pipi, lubang hidung segitiga, deretan gigi, rahang bawah terpisah, rusuk tembus
pandang, dan tulang "tulang anjing" dengan bonggol di ujung.
Satuan: 1 unit = 1 tile, tinggi ~1. Output: argv[1]/tulang_ref_sheet.png
"""
import os, sys
from PIL import Image, ImageDraw

OUT = sys.argv[1]
os.makedirs(OUT, exist_ok=True)

BONE, BONE_D, INK = (222, 214, 192), (165, 156, 136), (26, 20, 32)
EYE, EYE_H = (240, 60, 66), (255, 190, 180)
RUST, RUST_D, IRON, IRON_H = (138, 72, 40), (92, 48, 28), (72, 68, 78), (120, 116, 128)
BG, GRID = (40, 40, 46), (56, 56, 64)
S, W, H = 380, 420, 470  # piksel per unit, ukuran panel


class Panel:
    def __init__(self, title, top=False):
        self.img = Image.new('RGB', (W, H), BG)
        self.d = ImageDraw.Draw(self.img)
        self.cx, self.base = W / 2, (H / 2 if top else H - 40)
        for i in range(-5, 6):
            x = self.cx + i * S / 10
            self.d.line([(x, 30), (x, H - 10)], fill=GRID)
        for i in range(-2, 12):
            y = self.base - i * S / 10
            if 30 < y < H - 5: self.d.line([(10, y), (W - 10, y)], fill=GRID)
        self.d.text((12, 8), title, fill=(235, 230, 220))

    def p(self, x, z): return (self.cx + x * S, self.base - z * S)
    def pts(self, ps): return [self.p(x, z) for x, z in ps]

    def poly(self, ps, fill, outline=INK, w=2):
        self.d.polygon(self.pts(ps), fill=fill, outline=outline, width=w)

    def ell(self, c, rx, rz, fill, outline=INK, w=2):
        x, y = self.p(*c)
        self.d.ellipse([x - rx * S, y - rz * S, x + rx * S, y + rz * S], fill=fill, outline=outline, width=w)

    def line(self, ps, fill=INK, w=2): self.d.line(self.pts(ps), fill=fill, width=w, joint='curve')

    def bone(self, a, b, r, col=BONE):
        """Tulang 'tulang anjing': batang kurus dengan bonggol di kedua ujung."""
        self.line([a, b], INK, int(2 * r * S) + 4)
        for c in (a, b): self.ell(c, r * 1.45, r * 1.45, col)
        self.line([a, b], col, int(2 * r * S))

    def rib(self, ps, col=BONE):
        self.line(ps, INK, int(0.022 * S) + 4)
        self.line(ps, col, int(0.022 * S))

    def eye(self, c):
        self.ell(c, 0.024, 0.024, EYE, outline=(255, 120, 110), w=1)
        self.ell((c[0] - 0.008, c[1] + 0.008), 0.007, 0.007, EYE_H, outline=EYE_H, w=1)


M = lambda ps: [(-x, z) for x, z in ps]  # cermin kiri-kanan


def skull_front(P, back=False):
    P.ell((0, 0.80), 0.205, 0.175, BONE)                                   # tempurung
    face = [(-0.19, 0.775), (-0.04, 0.745), (0, 0.75), (0.04, 0.745), (0.19, 0.775),
            (0.185, 0.705), (0.15, 0.655), (0.11, 0.618), (-0.11, 0.618), (-0.15, 0.655), (-0.185, 0.705)]
    if back:
        # rahang bawah tampak dari belakang di bawah tempurung
        P.poly([(-0.115, 0.62), (0.115, 0.62), (0.108, 0.545), (0.062, 0.512), (-0.062, 0.512), (-0.108, 0.545)], BONE_D)
        P.poly([(-0.19, 0.775), (0.19, 0.775), (0.165, 0.655), (0.09, 0.6), (-0.09, 0.6), (-0.165, 0.655)], BONE)
        P.line([(-0.12, 0.92), (-0.04, 0.88), (0.02, 0.92), (0.10, 0.88)], BONE_D, 2)   # sutura
        return
    P.poly(face, BONE)                                                      # tepi atas = alis tulang
    for s in (1, -1):
        P.poly([(s * 0.185, 0.745), (s * 0.20, 0.715), (s * 0.18, 0.69)], BONE_D, outline=BONE_D, w=1)  # pelipis
        P.poly([(s * 0.17, 0.68), (s * 0.12, 0.655), (s * 0.15, 0.645)], BONE_D, outline=BONE_D, w=1)   # bawah tulang pipi
        sock = [(0.165, 0.770), (0.035, 0.742), (0.042, 0.690), (0.080, 0.668), (0.150, 0.682)]
        P.poly(sock if s > 0 else M(sock), INK)                             # rongga mata miring (marah)
        P.eye((s * 0.095, 0.712))
    P.poly([(0, 0.684), (-0.03, 0.636), (0, 0.646), (0.03, 0.636)], INK)   # lubang hidung
    P.poly([(-0.105, 0.628), (0.105, 0.628), (0.10, 0.598), (-0.10, 0.598)], BONE)    # gigi atas
    for x in (-0.067, -0.034, 0, 0.034, 0.067): P.line([(x, 0.626), (x, 0.600)], INK, 2)
    P.poly([(-0.095, 0.598), (0.095, 0.598), (0.09, 0.586), (-0.09, 0.586)], INK)     # mulut sedikit terbuka
    jaw = [(-0.115, 0.588), (0.115, 0.588), (0.108, 0.545), (0.062, 0.512), (-0.062, 0.512), (-0.108, 0.545)]
    P.poly(jaw, BONE)                                                       # rahang bawah
    for x in (-0.05, -0.017, 0.017, 0.05): P.line([(x, 0.586), (x, 0.565)], INK, 2)
    P.line([(-0.075, 0.565), (0.075, 0.565)], INK, 2)


def body_front(P, back=False):
    for s in (1, -1):
        P.bone((s * 0.06, 0.265), (s * 0.072, 0.15), 0.017)                # paha
        P.bone((s * 0.072, 0.15), (s * 0.072, 0.045), 0.015, BONE_D)        # tulang kering
        P.poly([(s * 0.035, 0), (s * 0.11, 0), (s * 0.105, 0.035), (s * 0.04, 0.04)], BONE)  # kaki
    for i in range(6): P.ell((0, 0.315 + i * 0.034), 0.022, 0.014, BONE_D)  # tulang belakang
    P.poly([(-0.115, 0.31), (-0.03, 0.29), (0, 0.302), (0.03, 0.29), (0.115, 0.31), (0.095, 0.258),
            (0.035, 0.248), (0, 0.268), (-0.035, 0.248), (-0.095, 0.258)], BONE)   # panggul kupu-kupu
    for i, z in enumerate((0.465, 0.425, 0.385)):                          # rusuk tembus pandang
        wdt = 0.13 - i * 0.012
        for s in (1, -1): P.rib([(s * 0.018, z), (s * wdt * 0.7, z + 0.012), (s * wdt, z - 0.02), (s * wdt * 0.8, z - 0.05)])
    if back:
        for s in (1, -1): P.poly([(s * 0.03, 0.48), (s * 0.12, 0.47), (s * 0.06, 0.39)], BONE)  # belikat
        for i in range(6): P.ell((0, 0.315 + i * 0.034), 0.022, 0.014, BONE_D)
    else:
        P.poly([(-0.018, 0.48), (0.018, 0.48), (0.014, 0.37), (-0.014, 0.37)], BONE)  # tulang dada
    for i in range(2): P.ell((0, 0.505 + i * 0.03), 0.026, 0.014, BONE_D)   # leher


def arms_front(P, sword_x, shield_x, back=False):
    for s in (1, -1):
        P.bone((s * 0.15, 0.475), (s * 0.19, 0.375), 0.016)                # lengan atas
        P.bone((s * 0.19, 0.375), (s * 0.21, 0.31), 0.014, BONE_D)          # lengan bawah
        P.ell((s * 0.15, 0.48), 0.032, 0.03, BONE)                          # bahu
    P.poly([(sword_x - 0.035, 0.335), (sword_x + 0.035, 0.335), (sword_x + 0.03, 0.29), (sword_x - 0.03, 0.29)], BONE)  # kepalan
    # pedang pendek berkarat, ujung ke bawah, mata bergerigi
    blade = [(sword_x - 0.02, 0.28), (sword_x + 0.02, 0.28), (sword_x + 0.018, 0.11), (sword_x + 0.008, 0.10),
             (sword_x + 0.016, 0.08), (sword_x, 0.035), (sword_x - 0.017, 0.085), (sword_x - 0.02, 0.15)]
    P.poly(blade, RUST_D)
    P.poly([(sword_x - 0.06, 0.29), (sword_x + 0.06, 0.29), (sword_x + 0.06, 0.275), (sword_x - 0.06, 0.275)], IRON)
    # perisai bundar berkarat: bingkai besi, papan, tonjolan, paku keling, retak
    c = (shield_x, 0.33)
    if back:
        P.ell(c, 0.145, 0.145, IRON); P.ell(c, 0.125, 0.125, RUST_D)
        for dx in (-0.06, 0, 0.06): P.line([(c[0] + dx, 0.215), (c[0] + dx, 0.445)], INK, 2)
        P.poly([(c[0] - 0.1, 0.345), (c[0] + 0.1, 0.345), (c[0] + 0.1, 0.315), (c[0] - 0.1, 0.315)], IRON)  # pegangan
        return
    P.ell(c, 0.145, 0.145, IRON); P.ell(c, 0.125, 0.125, RUST)
    for dx in (-0.06, 0, 0.06): P.line([(c[0] + dx, 0.215), (c[0] + dx, 0.445)], RUST_D, 2)
    P.ell(c, 0.038, 0.038, IRON); P.ell((c[0] - 0.01, c[1] + 0.01), 0.012, 0.012, IRON_H, outline=IRON_H, w=1)
    for a, b in ((0, 0.135), (0.135, 0), (0, -0.135), (-0.135, 0)): P.ell((c[0] + a, c[1] + b), 0.009, 0.009, IRON_H)
    P.line([(c[0] + 0.05, 0.43), (c[0] + 0.07, 0.39), (c[0] + 0.055, 0.36)], INK, 2)   # retak


def view_front():
    P = Panel('DEPAN')
    body_front(P); arms_front(P, sword_x=-0.215, shield_x=0.22); skull_front(P)
    return P.img


def view_back():
    P = Panel('BELAKANG')
    arms_front(P, sword_x=0.215, shield_x=-0.22, back=True)
    body_front(P, back=True); skull_front(P, back=True)
    return P.img


def view_side():
    P = Panel('SAMPING KANAN (menghadap kanan)')
    # perisai di sisi jauh (lengan kiri): tampak tipis di depan tubuh
    P.poly([(0.115, 0.19), (0.145, 0.19), (0.145, 0.47), (0.115, 0.47)], IRON)
    P.bone((0.0, 0.265), (0.03, 0.15), 0.017); P.bone((0.03, 0.15), (0.0, 0.045), 0.015, BONE_D)
    P.poly([(-0.04, 0), (0.09, 0), (0.08, 0.035), (-0.02, 0.045)], BONE)    # kaki
    for i in range(6): P.ell((-0.035 + 0.01 * (i % 2), 0.315 + i * 0.034), 0.016, 0.014, BONE_D)
    P.poly([(-0.07, 0.31), (0.05, 0.30), (0.07, 0.26), (-0.02, 0.245), (-0.06, 0.265)], BONE)   # panggul
    for i, z in enumerate((0.465, 0.425, 0.385)):
        P.rib([(-0.035, z), (0.03, z + 0.008), (0.085, z - 0.03), (0.08, z - 0.05)])
    for i in range(2): P.ell((-0.025, 0.505 + i * 0.03), 0.02, 0.014, BONE_D)
    # tengkorak dari samping: tempurung bulat di belakang, wajah datar di depan
    P.ell((-0.015, 0.80), 0.185, 0.175, BONE)
    P.poly([(0.05, 0.90), (0.15, 0.835), (0.185, 0.78), (0.16, 0.755), (0.175, 0.70), (0.19, 0.665),
            (0.165, 0.63), (0.17, 0.60), (0.04, 0.60), (-0.03, 0.66), (-0.02, 0.80)], BONE)
    P.poly([(0.12, 0.775), (0.175, 0.772), (0.165, 0.69), (0.125, 0.695)], INK)         # rongga mata
    P.eye((0.152, 0.728))
    P.poly([(0.10, 0.688), (0.005, 0.70), (0.0, 0.685), (0.10, 0.672)], BONE_D, outline=INK, w=1)  # lengkung pipi
    P.ell((-0.015, 0.70), 0.018, 0.022, INK)                                 # lubang telinga
    P.poly([(0.185, 0.665), (0.165, 0.645), (0.18, 0.64)], INK)              # hidung
    for x in (0.12, 0.145): P.line([(x, 0.626), (x, 0.600)], INK, 2)
    P.poly([(0.168, 0.598), (0.03, 0.598), (0.03, 0.588), (0.165, 0.588)], INK)          # mulut
    P.poly([(0.165, 0.588), (0.155, 0.535), (0.10, 0.508), (0.01, 0.515), (-0.035, 0.55),
            (-0.03, 0.665), (-0.005, 0.665), (0.02, 0.588)], BONE)                         # rahang + ramus
    for x in (0.10, 0.13): P.line([(x, 0.586), (x, 0.566)], INK, 2)
    # lengan kanan (sisi dekat) dan pedang
    P.ell((-0.01, 0.48), 0.03, 0.03, BONE)
    P.bone((-0.01, 0.475), (-0.02, 0.375), 0.016); P.bone((-0.02, 0.375), (0.05, 0.315), 0.014, BONE_D)
    P.poly([(0.045, 0.28), (0.075, 0.28), (0.073, 0.10), (0.06, 0.035), (0.047, 0.10)], RUST_D)
    P.poly([(0.03, 0.29), (0.09, 0.29), (0.09, 0.275), (0.03, 0.275)], IRON)
    P.poly([(0.035, 0.335), (0.08, 0.335), (0.078, 0.29), (0.037, 0.29)], BONE)
    return P.img


def view_top():
    P = Panel('ATAS (depan = atas gambar)', top=True)
    # sumbu panel: x = kiri-kanan, z = maju (ke atas gambar)
    for s in (1, -1): P.ell((s * 0.07, 0.04), 0.035, 0.06, BONE)              # ujung kaki
    for s in (1, -1): P.ell((s * 0.16, -0.01), 0.04, 0.045, BONE)             # bahu
    P.poly([(-0.235, 0.105), (-0.195, 0.105), (-0.195, 0.07), (-0.235, 0.07)], BONE)   # kepalan kanan
    P.poly([(-0.275, 0.09), (-0.155, 0.09), (-0.155, 0.08), (-0.275, 0.08)], IRON)      # pelindung pedang
    P.poly([(0.08, 0.15), (0.36, 0.15), (0.36, 0.115), (0.08, 0.115)], IRON)            # perisai dari atas
    P.poly([(0.095, 0.14), (0.345, 0.14), (0.345, 0.125), (0.095, 0.125)], RUST)
    P.ell((0, 0.0), 0.205, 0.185, BONE)                                       # tempurung
    P.line([(-0.12, -0.08), (-0.03, -0.02), (0.03, -0.07), (0.11, -0.01)], BONE_D, 2)   # sutura
    P.line([(-0.17, 0.12), (0, 0.15), (0.17, 0.12)], BONE_D, 3)               # alis tulang di depan
    return P.img


def sheet():
    panels = [view_front(), view_side(), view_back(), view_top()]
    out = Image.new('RGB', (W * 4 + 260, H + 60), (24, 24, 28))
    for i, p in enumerate(panels): out.paste(p, (i * W, 40))
    d = ImageDraw.Draw(out)
    d.text((10, 12), 'Tulang Penjaga - lembar referensi 3D v2 (spike). Grid 0,1 unit; tinggi ~1 unit = 1 tile.', fill=(235, 230, 220))
    here = os.path.dirname(os.path.abspath(__file__))
    sprite = Image.open(os.path.join(here, '..', '..', 'Assets/Runecrypt/Art/Prototype/skeleton_0.png')).convert('RGBA')
    big = sprite.resize((224, 224), Image.NEAREST)
    out.paste(big, (W * 4 + 18, 80), big)
    d.text((W * 4 + 18, 60), 'Sprite 2D sekarang', fill=(235, 230, 220))
    return out


img = sheet()
assert img.size == (W * 4 + 260, H + 60)
img.save(os.path.join(OUT, 'tulang_ref_sheet.png'))
print('ok')
