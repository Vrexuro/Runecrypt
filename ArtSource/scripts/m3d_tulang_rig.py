"""Runecrypt 3D: rig, klip Idle/Move, dan ekspor FBX Tulang Penjaga.

Jalankan di Blender lewat MCP: exec(open(r'D:/Claude/PGIM/Runecrypt3D/ArtSource/scripts/m3d_tulang_rig.py').read())
Membangun ulang model (spike3d_tulang_blender.py) lalu rig-nya. Spec: Docs/specs/2026-11-21-3d-animasi-jalan-design.md.
Gaya: lari kecil bergemeretak dengan kaki menapak (kaki pendek, pinggul 0,265), tubuh atas kaku seperti prajurit.
"""
import math
import bpy
from mathutils import Euler, Vector

HERE = r'D:/Claude/PGIM/Runecrypt3D/ArtSource/scripts/'
exec(open(HERE + 'spike3d_tulang_blender.py').read())
exec(open(HERE + 'm3d_rig.py').read())

OUT = r'D:/Claude/PGIM/Runecrypt3D/Assets/Spike3D/Models/TulangPenjaga3D.fbx'
V0 = 2.0                 # tile/detik, sama dengan Locomotion3D.designSpeed di prefab
MOVE_FRAMES = 16         # 60 fps -> 0,2667 detik per siklus (2 langkah)
IDLE_FRAMES = 150
STEP = V0 * MOVE_FRAMES / FPS / 2      # jarak tapak per langkah = 0,2667
DROP = 0.06              # pinggul turun saat lari supaya kaki (hampir lurus saat rest) mampu menjangkau langkah
ANKLE_Z = 0.045
D = math.radians

# ---------- tulang ----------
BONES = [('root', None, (0, 0, 0), (0, 0, 0.08), False),
         ('hips', 'root', (0, 0, 0.265), (0, 0, 0.32), False),
         ('spine', 'hips', (0, 0.05, 0.32), (0, 0.06, 0.40), False),
         ('chest', 'spine', (0, 0.06, 0.40), (0, 0.03, 0.50), True),
         ('neck', 'chest', (0, 0.02, 0.50), (0, 0.02, 0.55), False),
         ('head', 'neck', (0, 0.02, 0.55), (0, 0.0, 0.95), True)]
for s, sd in ((1, '.L'), (-1, '.R')):
    BONES += [(f'upperarm{sd}', 'chest', (s * 0.15, 0, 0.475), (s * 0.19, -0.02, 0.375), False),
              (f'forearm{sd}', f'upperarm{sd}', (s * 0.19, -0.02, 0.375), (s * 0.21, -0.07, 0.31), True),
              (f'hand{sd}', f'forearm{sd}', (s * 0.21, -0.07, 0.31), (s * 0.21, -0.11, 0.312), True),
              (f'thigh{sd}', 'hips', (s * 0.06, 0, 0.265), (s * 0.072, -0.01, 0.15), False),
              (f'shin{sd}', f'thigh{sd}', (s * 0.072, -0.01, 0.15), (s * 0.072, 0, ANKLE_Z), True),
              (f'foot{sd}', f'shin{sd}', (s * 0.072, 0, ANKLE_Z), (s * 0.072, -0.08, 0.02), True)]

# ---------- bobot kaku: satu bagian = satu tulang ----------
FIXED = {'Skull': 'head', 'SocketDark': 'head', 'Eye': 'head', 'EarHole': 'head', 'NoseDark': 'head', 'Mouth': 'head',
         'ToothUp': 'head', 'ToothLow': 'head', 'Jaw': 'head', 'JawRamus': 'head', 'Chin': 'head', 'Neck': 'neck',
         'Sternum': 'chest', 'Rib': 'chest', 'Scapula': 'chest', 'Pelvis': 'hips',
         'Blade': 'hand.R', 'Guard': 'hand.R', 'Grip': 'hand.R',
         'ShieldRim': 'forearm.L', 'ShieldBoard': 'forearm.L', 'ShieldPlank': 'forearm.L', 'ShieldBoss': 'forearm.L',
         'Rivet': 'forearm.L'}
SIDED = {'Shoulder': 'upperarm', 'UpperArm': 'upperarm', 'Forearm': 'forearm', 'Fist': 'hand',
         'Femur': 'thigh', 'Shin': 'shin', 'Foot': 'foot'}


def weigh(name, co):
    t = name.split('_')
    if t[0] in FIXED:
        return {FIXED[t[0]]: 1.0}
    if t[0] == 'Vertebra':
        return {'spine' if int(t[1]) < 3 else 'chest': 1.0}
    if t[0] in SIDED:
        return {SIDED[t[0]] + {'1': '.L', '-1': '.R'}[t[1]]: 1.0}
    return {}


mesh = bake_mesh('TulangPenjaga', 'TulangPenjaga_Export', weigh)
arm = make_armature('Tulang_Rig', BONES)
bind(mesh, arm)


def rest_angle(name):
    """Sudut rest tulang kaki di bidang y-z: 0 = lurus ke bawah, positif = ujung ke +Y."""
    b = arm.data.bones[name]
    v = b.tail_local - b.head_local
    return math.atan2(v.y, -v.z)


L1 = (arm.data.bones['thigh.L'].tail_local - arm.data.bones['thigh.L'].head_local).yz.length
L2 = (arm.data.bones['shin.L'].tail_local - arm.data.bones['shin.L'].head_local).yz.length
REST = {n: rest_angle(n + '.L') for n in ('thigh', 'shin', 'foot')}


def ankle_target(p):
    """Pergelangan relatif pinggul (y) dan tinggi dunia (z). Fase 0-0,5 menapak, 0,5-1 mengayun ke depan."""
    if p < 0.5:
        return -STEP / 2 + STEP * (p / 0.5), ANKLE_Z
    w = (p - 0.5) / 0.5
    return STEP / 2 - STEP * (0.5 - 0.5 * math.cos(math.pi * w)), ANKLE_Z + 0.04 * math.sin(math.pi * w)


def pulse(p, at, width):
    """Denyut periodik (0..1) berpusat di fase `at`."""
    d = abs((p - at + 0.5) % 1.0 - 0.5)
    return max(0.0, 1.0 - d / width)


def move_pose(p):
    bob = -0.005 * math.cos(4 * math.pi * (p - 1 / 16))          # terendah tepat setelah kontak
    hip_z = 0.265 - DROP + bob
    pose = {'hips': (None, Vector((0, 0, -DROP + bob)))}
    for sd, ph in (('.L', p), ('.R', (p + 0.5) % 1.0)):
        y, z = ankle_target(ph)
        thigh_abs, knee = two_bone_ik((0.0, hip_z), (y, z), L1, L2)
        d_thigh = thigh_abs - REST['thigh']
        d_shin = (thigh_abs + knee - REST['shin']) - d_thigh
        pose['thigh' + sd] = (Euler((d_thigh, 0, 0)), None)
        pose['shin' + sd] = (Euler((d_shin, 0, 0)), None)
        pose['foot' + sd] = (Euler((-(d_thigh + d_shin), 0, 0)), None)   # telapak tetap rata
    s = math.sin(2 * math.pi * p)
    pose['spine'] = (Euler((D(4), D(3) * s, D(4) * s)), None)          # condong sedikit, roll dan yaw ke kaki tumpu
    pose['chest'] = (Euler((0, 0, -D(7) * s)), None)                    # bahu melawan pinggul
    lag = math.sin(2 * math.pi * (p - 2 / 16))
    pose['upperarm.R'] = (Euler((D(8) * lag, 0, 0)), None)              # pedang bergetar, ujungnya tertinggal
    pose['hand.R'] = (Euler((D(6) * math.sin(2 * math.pi * (p - 4 / 16)), 0, 0)), None)
    pose['upperarm.L'] = (Euler((D(-10), 0, 0)), None)                  # perisai diam di depan
    pose['head'] = (Euler((D(6) * pulse(p, 2 / 16, 0.12), 0, -D(3) * s)), None)   # tengkorak telat di langkah kiri
    return pose


def idle_pose(p):
    a = 2 * math.pi * p
    return {
        'spine': (Euler((0, D(1.5) * math.sin(a), 0)), None),
        'head': (Euler((D(2) * math.sin(2 * a), 0, D(5) * math.sin(a + 1))), None),
        'upperarm.R': (Euler((D(3) * math.sin(a + 2), 0, 0)), None),
        'forearm.L': (Euler((D(1.5) * math.sin(2 * a), 0, 0)), None),
    }


key_clip(arm, 'Idle', IDLE_FRAMES, idle_pose)
key_clip(arm, 'Move', MOVE_FRAMES, move_pose)

# ---------- self-check ----------
for v in mesh.data.vertices:
    assert len(v.groups) == 1 and abs(v.groups[0].weight - 1) < 1e-6, 'bobot tidak kaku di vertex %d' % v.index
ankles = []
for f in range(MOVE_FRAMES + 1):            # baca hasil track NLA Move (yang diekspor), bukan pose manual
    with solo_track(arm, 'Move'):
        bpy.context.scene.frame_set(f)
        ankles.append(arm.pose.bones['foot.L'].head.copy())
for f in range(MOVE_FRAMES // 2):            # fase menapak kaki kiri: bergeser mundur tepat v0/fps, tidak naik-turun
    dy, dz = ankles[f + 1].y - ankles[f].y, ankles[f + 1].z - ankles[f].z
    assert abs(dy - V0 / FPS) < 0.004 and abs(ankles[f].z - ANKLE_Z) < 0.005, (f, round(dy, 4), round(ankles[f].z, 4))
assert (ankles[0] - ankles[-1]).length < 1e-5
bpy.context.scene.frame_set(0)
export_fbx(arm, mesh, OUT)
print('TULANG RIG ok', len(mesh.data.vertices), 'verts', len(arm.data.bones), 'bones')
