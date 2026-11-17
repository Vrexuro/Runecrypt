"""Sprite efek prototipe M1: bola api, Genangan Beku, kepulan Uap, dan lapisan mata kerangka.
Usage: python make_proto_fx.py <dir prototype>  (membaca skeleton_0..3.png dari dir yang sama)."""
import sys, os, math
from PIL import Image

OUT = sys.argv[1]
y, o, O = (255, 204, 102), (255, 138, 61), (255, 226, 150)
w = (217, 209, 189)
r = (224, 71, 76)
ICE_LIGHT, ICE_DARK = (159, 211, 230), (94, 143, 176)

def save(im, name):
    im.save(os.path.join(OUT, name + '.png'))

# Bola api: inti terang 2x2, cincin kuning, tepi jingga (diameter ~6 px).
fire = Image.new('RGBA', (16, 16), (0, 0, 0, 0))
for py in range(16):
    for px in range(16):
        d = math.hypot(px - 7.5, py - 7.5)
        if d < 1.2: fire.putpixel((px, py), O + (255,))
        elif d < 2.2: fire.putpixel((px, py), y + (255,))
        elif d < 3.1: fire.putpixel((px, py), o + (255,))
save(fire, 'fireball')

# Genangan Beku: elips rata dengan tepi gelap dan kilau terang.
puddle = Image.new('RGBA', (16, 16), (0, 0, 0, 0))
for py in range(16):
    for px in range(16):
        e = ((px - 7.5) / 7.0) ** 2 + ((py - 9.5) / 4.0) ** 2
        if e < 0.55: puddle.putpixel((px, py), ICE_LIGHT + (255,))
        elif e < 1.0: puddle.putpixel((px, py), ICE_DARK + (255,))
for (px, py) in [(5, 8), (6, 8), (10, 10)]:
    puddle.putpixel((px, py), (255, 255, 255, 255))
save(puddle, 'frost_puddle')

# Kepulan Uap: lingkaran lembut, alfa menurun ke tepi (dipakai partikel).
steam = Image.new('RGBA', (16, 16), (0, 0, 0, 0))
for py in range(16):
    for px in range(16):
        d = math.hypot(px - 7.5, py - 7.5) / 7.5
        if d < 1.0:
            a = int(255 * (1 - d) ** 0.8)
            steam.putpixel((px, py), w + (a,))
save(steam, 'steam_puff')

# Mata kerangka: hanya piksel merah dari tiap frame, posisi sama (material unlit di Unity).
for f in range(4):
    src = Image.open(os.path.join(OUT, f'skeleton_{f}.png')).convert('RGBA')
    eyes = Image.new('RGBA', src.size, (0, 0, 0, 0))
    n = 0
    for py in range(src.height):
        for px in range(src.width):
            if src.getpixel((px, py))[:3] == r:
                eyes.putpixel((px, py), r + (255,))
                n += 1
    assert n > 0, f'skeleton_{f}.png tidak punya piksel mata merah'
    save(eyes, f'skeleton_eyes_{f}')
print('ok')
