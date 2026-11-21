"""Runecrypt 3D: rig, klip Idle/Move, dan ekspor FBX Pyromancer.

Jalankan di Blender lewat MCP: exec(open(r'D:/Claude/PGIM/Runecrypt3D/ArtSource/scripts/m3d_pyromancer_rig.py').read())
Membangun ulang model (m3d_pyromancer_blender.py) lalu rig-nya. Spec: Docs/specs/2026-11-21-3d-animasi-jalan-design.md.
Tanpa tulang kaki: empat rantai jubah digerakkan fase "paha bayangan"; bobot jubah dihitung dari tinggi dan azimut.
Hem tetap di lantai: tulang _2 membatalkan bob pinggul, jadi jubah memendek-memanjang, bukan terangkat.
"""
import math
import bpy
from mathutils import Euler, Vector, kdtree

HERE = r'D:/Claude/PGIM/Runecrypt3D/ArtSource/scripts/'
exec(open(HERE + 'm3d_pyromancer_blender.py').read())
exec(open(HERE + 'm3d_rig.py').read())

OUT = r'D:/Claude/PGIM/Runecrypt3D/Assets/Runecrypt3D/Models/Pyromancer3D.fbx'
MOVE_FRAMES = 32         # 60 fps -> 0,5333 detik per siklus pada v0 = 5 tile/detik
IDLE_FRAMES = 180
LAG = 2 / MOVE_FRAMES    # ujung jubah tertinggal 2 frame
D = math.radians

SH = {s: Vector((s * 0.12, 0.0, 0.55)) for s in (1, -1)}      # bahu dan pergelangan = ujung Sleeve di script model
WR = {s: Vector((s * 0.255, -0.06, 0.31)) for s in (1, -1)}
SIDE = {1: '.L', -1: '.R'}
CHAINS = {'robe_front.L': 45.0, 'robe_back.L': 135.0, 'robe_back.R': -135.0, 'robe_front.R': -45.0}   # 0 = depan, + = kiri


def chain_dir(az):
    a = math.radians(az)
    return Vector((math.sin(a), -math.cos(a), 0.0))


BONES = [('root', None, (0, 0, 0), (0, 0, 0.08), False),
         ('hips', 'root', (0, 0, 0.40), (0, 0, 0.45), False),
         ('spine', 'hips', (0, 0, 0.45), (0, 0, 0.52), True),
         ('chest', 'spine', (0, 0, 0.52), (0, 0, 0.60), True),
         ('neck', 'chest', (0, 0, 0.60), (0, 0, 0.64), True),
         ('head', 'neck', (0, 0, 0.64), (0, 0, 0.95), True)]
for s, sd in SIDE.items():
    mid = (SH[s] + WR[s]) / 2
    BONES += [(f'upperarm{sd}', 'chest', SH[s][:], mid[:], False),
              (f'forearm{sd}', f'upperarm{sd}', mid[:], WR[s][:], True),
              (f'hand{sd}', f'forearm{sd}', WR[s][:], (s * 0.262, -0.075, 0.25), True)]
for name, az in CHAINS.items():
    d = chain_dir(az)
    a, b, c = d * 0.13 + Vector((0, 0, 0.50)), d * 0.20 + Vector((0, 0, 0.28)), d * 0.26 + Vector((0, 0, 0.06))
    BONES += [(name + '_1', 'hips', a[:], b[:], False), (name + '_2', name + '_1', b[:], c[:], False)]


# ---------- bobot ----------
def lerp_w(z, z0, z1):
    return min(1.0, max(0.0, (z0 - z) / (z0 - z1)))


def robe_weights(co):
    """Tinggi: hips -> _1 -> _2. Azimut: dibagi antara dua rantai bertetangga."""
    t1 = lerp_w(co.z, 0.45, 0.28)            # hips -> _1
    t2 = lerp_w(co.z, 0.28, 0.15)            # _1 -> _2
    w_hips, w1, w2 = 1 - t1, t1 * (1 - t2), t2
    az = math.degrees(math.atan2(co.x, -co.y))
    out = {'hips': w_hips} if w_hips > 0 else {}
    for name, caz in CHAINS.items():
        d = abs((az - caz + 180) % 360 - 180)
        share = max(0.0, 1 - d / 90.0)
        if share > 0:
            out[name + '_1'] = out.get(name + '_1', 0) + w1 * share
            out[name + '_2'] = out.get(name + '_2', 0) + w2 * share
    total = sum(out.values())
    return {k: v / total for k, v in out.items() if v > 1e-6}


dg = bpy.context.evaluated_depsgraph_get()
robe_obj = bpy.data.objects['Robe']
robe_eval = robe_obj.evaluated_get(dg).to_mesh()
robe_pts = [robe_obj.matrix_world @ v.co for v in robe_eval.vertices]
robe_obj.evaluated_get(dg).to_mesh_clear()
kd = kdtree.KDTree(len(robe_pts))
for i, p in enumerate(robe_pts):
    kd.insert(p, i)
kd.balance()

FIXED = {'Hood': 'head', 'HoodRim': 'head', 'FaceVoid': 'head', 'Eye': 'head', 'Shoulders': 'chest',
         'Belt': 'hips', 'SkullCharm': 'hips', 'CharmEye': 'hips', 'Ember': 'hand.R', 'Flame': 'hand.R'}


def weigh(name, co):
    t = name.split('_')
    if t[0] in FIXED:
        return {FIXED[t[0]]: 1.0}
    if t[0] == 'Robe':
        return robe_weights(co)
    if t[0] in ('Burn', 'FrontFold'):              # ikut jubah di titik terdekat
        return robe_weights(kd.find(co)[0])
    if t[0] in ('SleeveInside', 'Hand'):
        return {'hand' + SIDE[int(t[1])]: 1.0}
    if t[0] == 'Sleeve':
        s = int(t[1])
        axis = WR[s] - SH[s]
        k = min(1.0, max(0.0, ((co - SH[s]).dot(axis) / axis.length_squared - 0.4) / 0.2))
        return {k2: v for k2, v in (('upperarm' + SIDE[s], 1 - k), ('forearm' + SIDE[s], k)) if v > 0}
    return {}


mesh = bake_mesh('Pyromancer', 'Pyromancer_Export', weigh)
arm = make_armature('Pyro_Rig', BONES)
bind(mesh, arm)


# ---------- klip ----------
def pulse(p, at, width):
    d = abs((p - at + 0.5) % 1.0 - 0.5)
    return max(0.0, 1.0 - d / width)


def move_pose(p):
    bob = -0.0325 * math.cos(4 * math.pi * (p - 0.03))            # 6,5 cm, terendah tepat setelah kontak
    stride = {'.L': math.sin(2 * math.pi * p), '.R': -math.sin(2 * math.pi * p)}           # >0 = paha bayangan maju
    late = {'.L': math.sin(2 * math.pi * (p - LAG)), '.R': -math.sin(2 * math.pi * (p - LAG))}
    sway = math.sin(2 * math.pi * p)
    pose = {'hips': (Euler((0, D(3) * sway, 0)), Vector((0, 0, bob)))}
    for name in CHAINS:
        sd = name[-2:]
        front = name.startswith('robe_front')
        pose[name + '_1'] = (Euler((-(D(18) if front else D(8)) * stride[sd], 0, 0)), None)   # tonjolan lutut
        pose[name + '_2'] = (Euler((-D(8) * late[sd], D(1.5) * math.sin(2 * math.pi * (p - LAG)), 0)),
                             Vector((0, 0, -bob)))                  # hem tetap di lantai
    pose['upperarm.L'] = (Euler((D(20) * stride['.L'], 0, 0)), None)                 # berayun melawan kaki kiri
    pose['forearm.L'] = (Euler((D(8) * late['.L'], 0, 0)), None)
    pose['head'] = (Euler((D(3) * (pulse(p, 0.06, 0.1) + pulse(p, 0.56, 0.1)), 0, 0)), None)
    return pose


def idle_pose(p):
    a = 2 * math.pi * p
    late = a - 2 * math.pi * 5 / IDLE_FRAMES
    pose = {'hips': (Euler((0, D(1.5) * math.sin(a), 0)), None),
            'chest': (None, Vector((0, 0, 0.006 * math.sin(a)))),
            'upperarm.L': (Euler((D(2.5) * math.sin(a + 1), 0, 0)), None),
            'hand.R': (None, Vector((0, 0, 0.005 * math.sin(2 * a + 1))))}
    for name in CHAINS:
        pose[name + '_2'] = (Euler((0, D(1.5) * math.sin(late), 0)), None)
    return pose


key_clip(arm, 'Idle', IDLE_FRAMES, idle_pose)
key_clip(arm, 'Move', MOVE_FRAMES, move_pose)

# ---------- self-check ----------
for v in mesh.data.vertices:
    assert v.groups and abs(sum(g.weight for g in v.groups) - 1) < 1e-4, 'bobot vertex %d' % v.index
lows = []
for f in range(MOVE_FRAMES + 1):                   # hem tidak melayang dan tidak tenggelam di frame mana pun
    with solo_track(arm, 'Move'):
        bpy.context.scene.frame_set(f)
        ev = mesh.evaluated_get(bpy.context.evaluated_depsgraph_get())
        m = ev.to_mesh()
        lows.append(min((mesh.matrix_world @ v.co).z for v in m.vertices))
        ev.to_mesh_clear()
assert all(-0.01 <= z <= 0.05 for z in lows), [round(z, 3) for z in lows]
assert abs(lows[0] - lows[-1]) < 1e-5
bpy.context.scene.frame_set(0)
export_fbx(arm, mesh, OUT)
print('PYRO RIG ok', len(mesh.data.vertices), 'verts', len(arm.data.bones), 'bones, hem z', round(min(lows), 3), round(max(lows), 3))
