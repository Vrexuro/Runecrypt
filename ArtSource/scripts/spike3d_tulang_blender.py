"""SPIKE 3D (boleh dibuang): bangun Tulang Penjaga low-poly di Blender dari lembar referensi v2.

Jalankan di dalam Blender (lewat MCP: exec(open(path).read())). Semua objek masuk koleksi "TulangPenjaga",
yang dihapus dan dibuat ulang setiap kali script dijalankan.
Sumbu sama dengan lembar referensi: x kanan (kiri karakter), -y depan, z atas; 1 unit = 1 tile, tinggi ~1.
"""
import math
import bpy
from mathutils import Vector

COL = 'TulangPenjaga'

# ---------- koleksi dan material ----------
old = bpy.data.collections.get(COL)
if old:
    for o in list(old.objects): bpy.data.objects.remove(o, do_unlink=True)
    bpy.data.collections.remove(old)
col = bpy.data.collections.new(COL)
bpy.context.scene.collection.children.link(col)
for name in ('Cube',):
    o = bpy.data.objects.get(name)
    if o: bpy.data.objects.remove(o, do_unlink=True)


def mat(name, rgb, rough=0.8, metal=0.0, emit=0.0):
    m = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.use_nodes = True
    b = m.node_tree.nodes.get('Principled BSDF')
    b.inputs['Base Color'].default_value = (*[c / 255 for c in rgb], 1)
    b.inputs['Roughness'].default_value = rough
    b.inputs['Metallic'].default_value = metal
    if emit:
        b.inputs['Emission Color'].default_value = (*[c / 255 for c in rgb], 1)
        b.inputs['Emission Strength'].default_value = emit
    m.diffuse_color = (*[c / 255 for c in rgb], 1)
    return m


M_BONE = mat('Tulang_Bone', (222, 214, 192))
M_BONE_D = mat('Tulang_BoneDark', (165, 156, 136))
M_INK = mat('Tulang_Ink', (20, 15, 24), rough=1.0)
M_EYE = mat('Tulang_Eye', (255, 20, 10), emit=1.6)  # lebih kuat dari ini warnanya pecah jadi putih
M_RUST = mat('Tulang_Rust', (138, 72, 40), rough=0.9, metal=0.2)
M_RUST_D = mat('Tulang_RustDark', (92, 48, 28), rough=0.85, metal=0.4)
M_IRON = mat('Tulang_Iron', (72, 68, 78), rough=0.5, metal=0.8)
M_WOOD = mat('Tulang_Wood', (96, 58, 34), rough=0.9)


def link(o, m, name):
    o.name = name
    for c in o.users_collection: c.objects.unlink(o)
    col.objects.link(o)
    o.data.materials.clear()
    o.data.materials.append(m)
    for p in o.data.polygons: p.use_smooth = False
    return o


def sphere(name, c, r, m, seg=10, rings=7):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=seg, ring_count=rings, location=c)
    o = bpy.context.active_object
    o.scale = r
    return link(o, m, name)


def box(name, c, r, m, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cube_add(location=c, rotation=rot)
    o = bpy.context.active_object
    o.scale = r
    return link(o, m, name)


def cyl(name, a, b, r, m, verts=8):
    a, b = Vector(a), Vector(b)
    d = b - a
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=r, depth=d.length, location=(a + b) / 2)
    o = bpy.context.active_object
    o.rotation_mode = 'QUATERNION'
    o.rotation_quaternion = d.to_track_quat('Z', 'Y')
    return link(o, m, name)


def prism(name, pts_xz, y0, y1, m, bevel=0.0):
    """Profil 2D di bidang x-z diekstrusi dari y0 ke y1 (untuk rahang, panggul, bilah pedang)."""
    n = len(pts_xz)
    verts = [(x, y0, z) for x, z in pts_xz] + [(x, y1, z) for x, z in pts_xz]
    faces = [list(range(n))[::-1], list(range(n, 2 * n))] + [[i, (i + 1) % n, n + (i + 1) % n, n + i] for i in range(n)]
    me = bpy.data.meshes.new(name)
    me.from_pydata(verts, [], faces)
    me.update()
    o = bpy.data.objects.new(name, me)
    col.objects.link(o)
    o.data.materials.append(m)
    if bevel:
        bv = o.modifiers.new('bevel', 'BEVEL'); bv.width = bevel; bv.segments = 1
    return o


def tube(name, pts, r, m):
    cu = bpy.data.curves.new(name, 'CURVE')
    cu.dimensions = '3D'
    cu.bevel_depth = r
    cu.bevel_resolution = 1
    sp = cu.splines.new('POLY')
    sp.points.add(len(pts) - 1)
    for p, (x, y, z) in zip(sp.points, pts): p.co = (x, y, z, 1)
    o = bpy.data.objects.new(name, cu)
    col.objects.link(o)
    o.data.materials.append(m)
    return o


def bone(name, a, b, r, m=M_BONE):
    """Tulang 'tulang anjing': batang + bonggol di kedua ujung."""
    cyl(name, a, b, r, m)
    for i, p in enumerate((a, b)): sphere(f'{name}_knob{i}', p, (r * 1.5,) * 3, m, seg=8, rings=5)


def boolean(target, cutter, op='DIFFERENCE'):
    md = target.modifiers.new(cutter.name, 'BOOLEAN')
    md.operation = op
    md.object = cutter
    cutter.hide_render = True
    cutter.display_type = 'WIRE'
    cutter.hide_viewport = True


# ---------- tengkorak ----------
cranium = sphere('Cranium', (0, 0.0, 0.80), (0.205, 0.19, 0.175), M_BONE, seg=12, rings=8)
# pelat wajah: profil depan diekstrusi ke belakang, jadi wajah datar dengan tulang pipi
face = prism('Face', [(-0.19, 0.775), (-0.04, 0.745), (0, 0.75), (0.04, 0.745), (0.19, 0.775), (0.185, 0.705),
                      (0.15, 0.655), (0.11, 0.618), (-0.11, 0.618), (-0.15, 0.655), (-0.185, 0.705)],
             -0.175, -0.06, M_BONE, bevel=0.008)  # tidak menembus ke belakang tempurung
for s in (1, -1):
    # rongga mata miring: kotak pemotong diputar supaya alis turun ke tengah (kesan marah)
    cut = box(f'SocketCut_{s}', (s * 0.095, -0.17, 0.722), (0.068, 0.07, 0.045), M_INK, rot=(0, s * math.radians(-12), 0))
    boolean(face, cut)
    sphere(f'SocketDark_{s}', (s * 0.095, -0.125, 0.72), (0.06, 0.03, 0.045), M_INK)
    sphere(f'Eye_{s}', (s * 0.095, -0.15, 0.715), (0.026, 0.014, 0.026), M_EYE, seg=8, rings=6)
    sphere(f'Cheek_{s}', (s * 0.165, -0.11, 0.67), (0.035, 0.05, 0.022), M_BONE, seg=8, rings=5)
    sphere(f'EarHole_{s}', (s * 0.2, 0.0, 0.70), (0.006, 0.02, 0.022), M_INK, seg=8, rings=5)
nose = prism('NoseCut', [(0, 0.684), (-0.03, 0.636), (0, 0.646), (0.03, 0.636)], -0.2, -0.12, M_INK)
boolean(face, nose)
prism('NoseDark', [(0, 0.68), (-0.026, 0.638), (0, 0.646), (0.026, 0.638)], -0.14, -0.12, M_INK)
sphere('SkullBase', (0, 0.03, 0.655), (0.15, 0.14, 0.06), M_BONE)  # menutup celah bawah tempurung dari belakang
# gigi atas, mulut, rahang bawah, gigi bawah
for i, x in enumerate((-0.083, -0.05, -0.017, 0.017, 0.05, 0.083)):
    box(f'ToothUp_{i}', (x, -0.165, 0.612), (0.014, 0.012, 0.014), M_BONE)
    if 0 < i < 5: box(f'ToothLow_{i}', (x, -0.155, 0.576), (0.013, 0.011, 0.011), M_BONE)
box('Mouth', (0, -0.14, 0.592), (0.095, 0.025, 0.006), M_INK)
prism('Jaw', [(-0.115, 0.588), (0.115, 0.588), (0.108, 0.545), (0.062, 0.512), (-0.062, 0.512), (-0.108, 0.545)],
      -0.16, 0.02, M_BONE, bevel=0.006)
for s in (1, -1):  # ramus rahang naik ke arah telinga
    box(f'JawRamus_{s}', (s * 0.11, 0.0, 0.6), (0.012, 0.035, 0.05), M_BONE)

# ---------- badan ----------
for i in range(2): cyl(f'Neck_{i}', (0, 0.02, 0.505 + i * 0.03 - 0.012), (0, 0.02, 0.505 + i * 0.03 + 0.012), 0.024, M_BONE_D)
for i in range(6): cyl(f'Vertebra_{i}', (0, 0.06, 0.305 + i * 0.034), (0, 0.06, 0.325 + i * 0.034), 0.02, M_BONE_D)
box('Sternum', (0, -0.085, 0.425), (0.016, 0.01, 0.055), M_BONE)
for i, z in enumerate((0.465, 0.425, 0.385)):
    w = 0.13 - i * 0.012
    for s in (1, -1):
        tube(f'Rib_{i}_{s}', [(s * 0.016, -0.085, z), (s * w * 0.75, -0.07, z + 0.008), (s * w, -0.005, z - 0.02),
                               (s * w * 0.8, 0.05, z - 0.035), (s * 0.03, 0.065, z - 0.03)], 0.011, M_BONE)
prism('Pelvis', [(-0.115, 0.31), (-0.03, 0.29), (0, 0.302), (0.03, 0.29), (0.115, 0.31), (0.095, 0.258),
                 (0.035, 0.248), (0, 0.268), (-0.035, 0.248), (-0.095, 0.258)], -0.04, 0.05, M_BONE, bevel=0.006)
for s in (1, -1):
    prism(f'Scapula_{s}', [(s * 0.03, 0.48), (s * 0.12, 0.47), (s * 0.06, 0.39)], 0.07, 0.085, M_BONE)
    bone(f'Femur_{s}', (s * 0.06, 0.0, 0.265), (s * 0.072, -0.01, 0.15), 0.017)
    bone(f'Shin_{s}', (s * 0.072, -0.01, 0.15), (s * 0.072, 0.0, 0.045), 0.015, M_BONE_D)
    box(f'Foot_{s}', (s * 0.072, -0.035, 0.02), (0.035, 0.06, 0.02), M_BONE)
    sphere(f'Shoulder_{s}', (s * 0.15, 0.0, 0.48), (0.032, 0.032, 0.03), M_BONE, seg=8, rings=6)
    bone(f'UpperArm_{s}', (s * 0.15, 0.0, 0.475), (s * 0.19, -0.02, 0.375), 0.016)
    bone(f'Forearm_{s}', (s * 0.19, -0.02, 0.375), (s * 0.21, -0.07, 0.31), 0.014, M_BONE_D)
    box(f'Fist_{s}', (s * 0.21, -0.09, 0.312), (0.03, 0.025, 0.024), M_BONE)

# ---------- pedang (tangan kanan karakter = x negatif) ----------
sx = -0.21
prism('Blade', [(sx - 0.02, 0.28), (sx + 0.02, 0.28), (sx + 0.018, 0.11), (sx + 0.008, 0.10), (sx + 0.016, 0.08),
                (sx, 0.035), (sx - 0.017, 0.085), (sx - 0.02, 0.15)], -0.098, -0.082, M_RUST_D)
box('Guard', (sx, -0.09, 0.283), (0.06, 0.014, 0.008), M_IRON)
cyl('Grip', (sx, -0.09, 0.29), (sx, -0.09, 0.345), 0.01, M_INK)

# ---------- perisai bundar (lengan kiri karakter = x positif) ----------
c = (0.22, -0.13, 0.33)
def disc(name, y, r, depth, m, verts=14):
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=r, depth=depth, location=(c[0], y, c[2]), rotation=(math.pi / 2, 0, 0))
    return link(bpy.context.active_object, m, name)
disc('ShieldRim', -0.12, 0.145, 0.02, M_IRON)
disc('ShieldBoard', -0.128, 0.125, 0.02, M_RUST)
for i, dx in enumerate((-0.06, 0.0, 0.06)): box(f'ShieldPlank_{i}', (c[0] + dx, -0.139, c[2]), (0.003, 0.002, 0.11), M_WOOD)
sphere('ShieldBoss', (c[0], -0.145, c[2]), (0.038, 0.025, 0.038), M_IRON, seg=10, rings=6)
for i, (a, b) in enumerate(((0, 0.135), (0.135, 0), (0, -0.135), (-0.135, 0))):
    sphere(f'Rivet_{i}', (c[0] + a, -0.132, c[2] + b), (0.01,) * 3, M_IRON, seg=6, rings=4)

# self-check: tinggi model ~1 unit, kaki di lantai (matrix_world baru benar setelah view layer diperbarui)
bpy.context.view_layer.update()
zs =[(o.matrix_world @ Vector(v)).z for o in col.objects if o.type == 'MESH' and not o.hide_render
      for v in o.bound_box]
print('TULANG ok objek', len(col.objects), 'zmin', round(min(zs), 3), 'zmax', round(max(zs), 3))
assert -0.01 < min(zs) < 0.02 and 0.9 < max(zs) < 1.02
