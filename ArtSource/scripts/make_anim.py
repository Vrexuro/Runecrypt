"""4-frame animation test: floating mage + walking skeleton + floating wisp. Output dir = argv[1]."""
import sys, os
from PIL import Image

OUT = sys.argv[1]
os.makedirs(OUT, exist_ok=True)

PAL = {
    '.': None,
    'k': (26, 20, 32), 'p': (52, 40, 82), 'P': (84, 66, 128), 's': (14, 10, 20),
    'y': (255, 204, 102), 'o': (255, 138, 61), 'O': (255, 226, 150),
    'w': (217, 209, 189), 'W': (160, 151, 132), 'r': (224, 71, 76),
    'b': (134, 214, 255), 'B': (220, 245, 255), 'd': (40, 70, 110),
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
SKEL_TOP = [
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
]
SKEL_LEGS = [  # rows 14-15 per frame: both down, right lifted, both down, left lifted
    [".....kw..wk.....", ".....kk..kk....."],
    [".....kw..wk.....", ".....kk........."],
    [".....kw..wk.....", ".....kk..kk....."],
    [".....kw..wk.....", ".........kk....."],
]
WISP = [  # frost wisp; tail rows swap per frame
    "................",
    "......kkkk......",
    ".....kbbbbk.....",
    "....kbBBBBbk....",
    "...kbBBBBBBbk...",
    "...kbBkBBkBbk...",
    "...kbBkBBkBbk...",
    "...kbBBBBBBbk...",
    "...kbbBBBBbbk...",
    "....kbbbbbbk....",
]
WISP_TAILS = [
    ["....kdbbbbdk....", ".....kdbbdk.....", "......kddk......", ".......kk......."],
    ["....kdbbbbdk....", "....kdbbdk......", ".....kddk.......", "......kk........"],
    ["....kdbbbbdk....", ".....kdbbdk.....", "......kddk......", ".......kk......."],
    ["....kdbbbbdk....", "......kdbbdk....", ".......kddk.....", "........kk......"],
]

def check(grid):
    assert all(len(r) == 16 for r in grid), [r for r in grid if len(r) != 16]

def draw(grid, dy=0, swap=None):
    check(grid)
    im = Image.new('RGBA', (16, 16), (0, 0, 0, 0))
    for y, row in enumerate(grid):
        for x, c in enumerate(row):
            if swap: c = swap.get(c, c)
            if PAL[c] and 0 <= y + dy < 16:
                im.putpixel((x, y + dy), PAL[c] + (255,))
    return im

frames = {'mage': [], 'skeleton': [], 'wisp': []}
for f in range(4):
    bob = [0, -1, -1, 0][f]
    hem = ".kkkkkkkkkkkkkk." if f % 2 == 0 else "..kkkkkkkkkkkk.."
    m = draw(MAGE[:15] + [hem], dy=bob)
    flick = {'O': 'o', 'o': 'O'} if f in (1, 2) else None
    for (x, y, c) in [(14, 9, 'O'), (13, 9, 'o'), (15, 9, 'o'), (14, 8, 'o'), (14, 10, 'o')]:
        c = flick.get(c, c) if flick else c
        m.putpixel((x, y + bob), PAL[c] + (255,))
    frames['mage'].append(m)

    s = draw(SKEL_TOP + SKEL_LEGS[f], dy=0)
    if f in (1, 3):  # body bobs up while a foot is lifted; feet stay planted
        top = draw(SKEL_TOP + ["." * 16, "." * 16], dy=-1)
        legs = draw(["." * 16] * 14 + SKEL_LEGS[f])
        s = Image.alpha_composite(legs, top)
    frames['skeleton'].append(s)

    frames['wisp'].append(draw(["." * 16] * 2 + WISP + WISP_TAILS[f], dy=[0, -1, 0, 1][f]))

BG = (11, 13, 18, 255)
SCALE = 10
gif_frames = []
for f in range(4):
    canvas = Image.new('RGBA', (16 * 3 + 8, 18), BG)
    for i, n in enumerate(('mage', 'skeleton', 'wisp')):
        canvas.alpha_composite(frames[n][f], (i * 20 + 1, 1))
    gif_frames.append(canvas.resize((canvas.width * SCALE, canvas.height * SCALE), Image.NEAREST).convert('RGB'))
gif_frames[0].save(os.path.join(OUT, 'anim_preview.gif'), save_all=True, append_images=gif_frames[1:], duration=160, loop=0)

# All frames in a grid so they can be inspected as a still image.
strip = Image.new('RGBA', (4 * 18, 3 * 18), BG)
for i, n in enumerate(('mage', 'skeleton', 'wisp')):
    for f in range(4):
        strip.alpha_composite(frames[n][f], (f * 18 + 1, i * 18 + 1))
        frames[n][f].save(os.path.join(OUT, f'{n}_{f}.png'))
strip.resize((strip.width * 8, strip.height * 8), Image.NEAREST).save(os.path.join(OUT, 'anim_frames.png'))
print('ok')
