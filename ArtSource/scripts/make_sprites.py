"""Sample 16x16 pixel-art sprites + a lit preview scene for Runecrypt 2D. Output dir = argv[1]."""
import sys, os, random, math
from PIL import Image

OUT = sys.argv[1]
os.makedirs(OUT, exist_ok=True)

PAL = {
    '.': None,
    'k': (26, 20, 32), 'p': (52, 40, 82), 'P': (84, 66, 128), 's': (14, 10, 20),
    'y': (255, 204, 102), 'o': (255, 138, 61), 'O': (255, 226, 150),
    'w': (217, 209, 189), 'W': (160, 151, 132), 'r': (224, 71, 76),
}

MAGE = [
    "................",
    "......kkkk......",
    ".....kPPPPk.....",
    "....kPPpppPk....",
    "...kPPkkkkpPk...",
    "...kPkssssskPk..",
    "...kPksysyskPk..",
    "...kPkssssskPk..",
    "..kpPPkkkkkPPpk.",
    "..kpPPPpppPPPpk.",
    ".kppPPpPPpPPppk.",
    ".kpPPPpPPpPPPpk.",
    ".kpPPpPPPPpPPpk.",
    "kppPPpPPPPpPPppk",
    "kppppppppppppppk",
    ".kkkkkkkkkkkkkk.",
]
SKEL = [
    "................",
    ".....kkkkkk.....",
    "....kwwwwwwk....",
    "...kwwwwwwwwk...",
    "...kwkrwwrkwk...",
    "...kwkkwwkkwk...",
    "...kwwwkkwwwk...",
    "....kwkwkwkwk...",
    ".....kkkkkk.....",
    "......kWk.......",
    "....kwwwwwwk....",
    "...kwkWkkWkwk...",
    "...kk.kwwk.kk...",
    "......kWWk......",
    ".....kw..wk.....",
    ".....kk..kk.....",
]
for g in (MAGE, SKEL):
    assert len(g) == 16 and all(len(r) == 16 for r in g), "sprite rows must be 16x16"

def sprite(grid, extra=()):
    im = Image.new('RGBA', (16, 16), (0, 0, 0, 0))
    for y, row in enumerate(grid):
        for x, c in enumerate(row):
            if PAL[c]: im.putpixel((x, y), PAL[c] + (255,))
    for (x, y, c) in extra:
        im.putpixel((x, y), PAL[c] + (255,))
    return im

ORB = [(14, 9, 'O'), (13, 9, 'o'), (15, 9, 'o'), (14, 8, 'o'), (14, 10, 'o')]
mage, skel = sprite(MAGE, ORB), sprite(SKEL)

rng = random.Random(3)
def floor_tile():
    im = Image.new('RGBA', (16, 16))
    for y in range(16):
        for x in range(16):
            v = 44 + rng.randint(-5, 5)
            if x in (0, 8) or y in (0, 8): v = 28  # stone slab seams
            im.putpixel((x, y), (v, v + 2, v + 10, 255))
    for _ in range(2):  # small crack
        x, y = rng.randint(2, 13), rng.randint(2, 13)
        for _ in range(4):
            im.putpixel((x, y), (24, 24, 32, 255)); x += rng.choice((-1, 0, 1)); y += 1
            if not (0 <= x < 16 and 0 <= y < 16): break
    return im

def wall_tile():
    im = Image.new('RGBA', (16, 16))
    for y in range(16):
        off = 0 if (y // 4) % 2 == 0 else 4
        for x in range(16):
            mortar = y % 4 == 3 or (x + off) % 8 == 7
            v = 22 if mortar else 62 + rng.randint(-6, 6) - (y % 4) * 4
            im.putpixel((x, y), (v, v, v + 12, 255))
    return im

for name, im in (('mage', mage), ('skeleton', skel), ('floor', floor_tile()), ('wall', wall_tile())):
    im.save(os.path.join(OUT, name + '.png'))

# Lit scene: 12x8 tiles, light around the mage in pixel-art bands.
TW, TH = 12, 8
W, H = TW * 16, TH * 16
scene = Image.new('RGBA', (W, H))
for ty in range(TH):
    for tx in range(TW):
        scene.paste(wall_tile() if ty == 0 else floor_tile(), (tx * 16, ty * 16))
mx, my = 88, 64
scene.alpha_composite(mage, (mx, my))
foes = [(40, 40), (150, 52), (120, 100), (20, 96)]
for fx, fy in foes:
    scene.alpha_composite(skel, (fx, fy))

lx, ly, R = mx + 14, my + 9, 46
glow = (255, 138, 61)
px = scene.load()
for y in range(H):
    for x in range(W):
        d = math.hypot(x - lx, y - ly) / R
        band = min(4, int(d * 4))  # 0..4 → stepped light, typical of pixel art
        light = [1.0, 0.8, 0.5, 0.25, 0.06][band]
        r, g, b, a = px[x, y]
        warm = 0.25 * light
        px[x, y] = (int(r * light + glow[0] * warm), int(g * light + glow[1] * warm * 0.6), int(b * light + glow[2] * warm * 0.3), 255)
# Enemy eyes glow through darkness.
for fx, fy in foes:
    for ex in (6, 9):
        px[fx + ex, fy + 4] = PAL['r'] + (255,)

scene.resize((W * 5, H * 5), Image.NEAREST).save(os.path.join(OUT, 'scene_preview.png'))

# Sprite sheet preview, 10x.
sheet = Image.new('RGBA', (16 * 4 + 12, 16), (11, 13, 18, 255))
for i, n in enumerate(('mage', 'skeleton', 'floor', 'wall')):
    sheet.alpha_composite(Image.open(os.path.join(OUT, n + '.png')), (i * 19, 0))
sheet.resize((sheet.width * 10, sheet.height * 10), Image.NEAREST).save(os.path.join(OUT, 'sprites_preview.png'))
print('ok', OUT)
