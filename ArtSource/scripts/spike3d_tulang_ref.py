"""SPIKE 3D (boleh dibuang): lembar referensi 4 sisi Tulang Penjaga + data bagian tubuh untuk Blender.

Satu daftar bentuk 3D (elipsoid dan kotak) dipakai dua kali: digambar ortografis di sini, lalu dibangun di
Blender dari parts JSON yang sama, supaya gambar referensi dan model tidak berbeda.
Sumbu: x kanan (kiri karakter), y ke belakang, z ke atas. Tinggi total sekitar 1 unit (1 tile), kaki di z=0.
Output: argv[1]/tulang_ref_sheet.png dan argv[1]/tulang_parts.json
"""
import json, os, sys
from PIL import Image, ImageDraw

OUT = sys.argv[1]
os.makedirs(OUT, exist_ok=True)

BONE, BONE_D, INK = (217, 209, 189), (160, 151, 132), (26, 20, 32)
EYE, RUST, RUST_D, IRON = (224, 71, 76), (140, 74, 40), (96, 52, 30), (70, 66, 74)

def E(name, c, r, col, emit=False): return dict(name=name, kind='ellipsoid', c=c, r=r, col=col, emit=emit)
def B(name, c, r, col): return dict(name=name, kind='box', c=c, r=r, col=col, emit=False)

PARTS = [
    E('skull', [0, 0, 0.78], [0.20, 0.18, 0.19], BONE),
    E('jaw', [0, -0.03, 0.60], [0.13, 0.12, 0.06], BONE),
    E('nose', [0, -0.178, 0.72], [0.02, 0.012, 0.025], INK),
    E('spine', [0, 0.03, 0.48], [0.03, 0.03, 0.10], BONE_D),
    E('ribcage', [0, 0, 0.43], [0.13, 0.09, 0.09], BONE),
    B('rib_gap_1', [0, -0.08, 0.455], [0.10, 0.012, 0.008], INK),
    B('rib_gap_2', [0, -0.08, 0.415], [0.09, 0.012, 0.008], INK),
    E('pelvis', [0, 0, 0.29], [0.10, 0.07, 0.04], BONE),
    B('shield_rim', [0.21, -0.115, 0.33], [0.14, 0.012, 0.14], IRON),
    B('shield', [0.21, -0.13, 0.33], [0.12, 0.012, 0.12], RUST),
    E('shield_boss', [0.21, -0.15, 0.33], [0.035, 0.02, 0.035], IRON),
    B('sword_blade', [-0.21, -0.10, 0.17], [0.018, 0.008, 0.15], RUST_D),
    B('sword_guard', [-0.21, -0.10, 0.33], [0.055, 0.012, 0.012], IRON),
    B('sword_grip', [-0.21, -0.10, 0.36], [0.012, 0.012, 0.03], INK),
]
for side, sx in (('L', 1), ('R', -1)):
    PARTS += [
        E('socket_' + side, [sx * 0.075, -0.15, 0.80], [0.05, 0.04, 0.045], INK),
        E('eye_' + side, [sx * 0.075, -0.175, 0.80], [0.022, 0.012, 0.022], EYE, emit=True),
        E('upperarm_' + side, [sx * 0.165, 0, 0.42], [0.022, 0.022, 0.08], BONE),
        E('forearm_' + side, [sx * 0.20, -0.05, 0.33], [0.02, 0.05, 0.02], BONE_D),
        E('femur_' + side, [sx * 0.06, 0, 0.20], [0.025, 0.025, 0.09], BONE),
        E('shin_' + side, [sx * 0.065, 0, 0.08], [0.022, 0.022, 0.08], BONE_D),
        E('foot_' + side, [sx * 0.065, -0.03, 0.015], [0.035, 0.06, 0.015], BONE),
    ]

# Pandangan ortografis: (nama, sumbu layar-x, tanda, sumbu layar-y, tanda, sumbu kedalaman, tanda; kecil = dekat)
VIEWS = [
    ('DEPAN', 0, 1, 2, 1, 1, 1),
    ('SAMPING KANAN', 1, -1, 2, 1, 0, -1),
    ('BELAKANG', 0, -1, 2, 1, 1, -1),
    ('ATAS', 0, 1, 1, 1, 2, -1),
]
S, W, H = 380, 420, 460  # piksel per unit, ukuran panel

def draw_view(v):
    name, ax, sx, ay, sy, ad, sd = v
    img = Image.new('RGB', (W, H), (40, 40, 46))
    d = ImageDraw.Draw(img)
    cx, base = W // 2, H - 40 if ay == 2 else H // 2 + int(0.0 * S)
    for i in range(0, 11):  # grid 0,1 unit
        y = base - i * S // 10 if ay == 2 else None
        if y is not None:
            d.line([(10, y), (W - 10, y)], fill=(58, 58, 66))
    for i in range(-5, 6):
        d.line([(cx + i * S // 10, 30), (cx + i * S // 10, H - 10)], fill=(58, 58, 66))
    # Detail wajah dan dada menempel di permukaan depan: tidak terlihat dari belakang dan dari atas.
    decal = lambda p: p['name'].startswith(('socket', 'eye', 'nose', 'rib_gap'))
    parts = [p for p in PARTS if not (decal(p) and name in ('BELAKANG', 'ATAS'))]
    order = sorted(parts, key=lambda p: -sd * p['c'][ad])  # yang jauh digambar dulu
    for p in order:
        px = cx + sx * p['c'][ax] * S
        py = (base - sy * p['c'][ay] * S) if ay == 2 else (H // 2 + sy * p['c'][ay] * S)
        rx, ry = p['r'][ax] * S, p['r'][ay] * S
        box = [px - rx, py - ry, px + rx, py + ry]
        if p['kind'] == 'ellipsoid': d.ellipse(box, fill=p['col'], outline=INK, width=2)
        else: d.rectangle(box, fill=p['col'], outline=INK, width=2)
        if p['emit']: d.ellipse([px - rx - 4, py - ry - 4, px + rx + 4, py + ry + 4], outline=(255, 120, 110), width=1)
    d.text((12, 8), name, fill=(235, 230, 220))
    return img

def sheet():
    panels = [draw_view(v) for v in VIEWS]
    out = Image.new('RGB', (W * 4 + 260, H + 60), (24, 24, 28))
    for i, p in enumerate(panels): out.paste(p, (i * W, 40))
    d = ImageDraw.Draw(out)
    d.text((10, 12), 'Tulang Penjaga - lembar referensi 3D (spike). Grid 0,1 unit; tinggi ~1 unit = 1 tile.', fill=(235, 230, 220))
    sprite = Image.open(os.path.join(os.path.dirname(__file__), '..', '..', 'Assets/Runecrypt/Art/Prototype/skeleton_0.png')).convert('RGBA')
    big = sprite.resize((224, 224), Image.NEAREST)
    out.paste(big, (W * 4 + 18, 80), big)
    d.text((W * 4 + 18, 60), 'Sprite 2D sekarang', fill=(235, 230, 220))
    return out

# self-check: semua bagian di atas lantai, tinggi total ~1 unit
top = max(p['c'][2] + p['r'][2] for p in PARTS); bottom = min(p['c'][2] - p['r'][2] for p in PARTS)
assert abs(bottom) < 1e-6 and 0.9 < top < 1.05, (bottom, top)
assert len({p['name'] for p in PARTS}) == len(PARTS), 'nama bagian harus unik'

sheet().save(os.path.join(OUT, 'tulang_ref_sheet.png'))
with open(os.path.join(OUT, 'tulang_parts.json'), 'w') as f: json.dump(PARTS, f, indent=1)
print('ok', len(PARTS), 'bagian, tinggi', round(top, 3))
