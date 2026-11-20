"""SPIKE 3D (boleh dibuang): gabungkan render_*.png menjadi render_sheet.png untuk dibandingkan dengan sprite 2D."""
import os
from PIL import Image, ImageDraw

D = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'spike3d')
names = [('front', 'Depan'), ('side', 'Samping'), ('back', 'Belakang'), ('top', 'Atas'), ('game', 'Sudut kamera game')]
W = 512
out = Image.new('RGB', (W * 5, W + 300), (30, 30, 36))
d = ImageDraw.Draw(out)
for i, (n, t) in enumerate(names):
    im = Image.open(os.path.join(D, f'render_{n}.png')).convert('RGBA')
    out.paste(im, (i * W, 30), im)
    d.text((i * W + 10, 8), t, fill=(235, 230, 220))
g = Image.open(os.path.join(D, 'render_game.png')).convert('RGB')
out.paste(g.resize((96, 96), Image.LANCZOS).resize((288, 288), Image.NEAREST), (20, W))
d.text((320, W + 20), 'Kiri: sudut game diperkecil ke ~96 px (ukuran di layar 1080p), diperbesar 3x supaya terlihat.', fill=(235, 230, 220))
sp = Image.open(os.path.join(D, '..', '..', 'Assets/Runecrypt/Art/Prototype/skeleton_0.png')).convert('RGBA')
sp = sp.resize((192, 192), Image.NEAREST)
out.paste(sp, (W * 4, W + 60), sp)
d.text((W * 4, W + 40), 'Sprite 2D sekarang', fill=(235, 230, 220))
out.save(os.path.join(D, 'render_sheet.png'))
print('ok')
