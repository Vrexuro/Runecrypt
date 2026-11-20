"""Runecrypt 3D: bangun Pyromancer (versi gelap) di Blender dari ArtSource/m3d/pyromancer_ref_sheet.png.

Jalankan di dalam Blender (lewat MCP: exec(open(path).read())). Semua objek masuk koleksi "Pyromancer",
yang dihapus dan dibuat ulang setiap kali script dijalankan. Koleksi Tulang disembunyikan dari render.
Sumbu: x kanan (kiri karakter), -y depan, z atas; 1 unit = 1 tile, tinggi ~1.
Permukaan kain halus sejak awal (subdivision + smooth shading), tepi robek dan bara tetap tajam.
"""
import math
import bpy
from mathutils import Vector

COL = 'Pyromancer'
old = bpy.data.collections.get(COL)
if old:
    for o in list(old.objects): bpy.data.objects.remove(o, do_unlink=True)
    bpy.data.collections.remove(old)
col = bpy.data.collections.new(COL)
bpy.context.scene.collection.children.link(col)
for name in ('TulangPenjaga',):
    c = bpy.data.collections.get(name)
    if c: c.hide_render = True; c.hide_viewport = True
exp = bpy.data.objects.get('TulangPenjaga_Export')
if exp: exp.hide_viewport = True


def mat(name, rgb, rough=0.9, metal=0.0, emit=0.0):
    m = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.use_nodes = True
    b = m.node_tree.nodes.get('Principled BSDF')
    b.inputs['Base Color'].default_value = (*[c / 255 for c in rgb], 1)
    b.inputs['Roughness'].default_value = rough
    b.inputs['Metallic'].default_value = metal
    b.inputs['Emission Strength'].default_value = emit
    if emit: b.inputs['Emission Color'].default_value = (*[c / 255 for c in rgb], 1)
    m.diffuse_color = (*[c / 255 for c in rgb], 1)
    return m


M_ROBE = mat('Pyro_Robe', (40, 31, 56), rough=0.95)
M_ROBE_D = mat('Pyro_RobeDark', (22, 17, 31), rough=0.95)
M_VOID = mat('Pyro_Void', (3, 2, 5), rough=1.0)
M_EYE = mat('Pyro_Eye', (255, 100, 20), emit=1.4)
M_EMBER = mat('Pyro_Ember', (255, 90, 20), emit=2.2)
M_BURN = mat('Pyro_Burn', (255, 70, 15), emit=1.5)
M_HAND = mat('Pyro_Hand', (44, 36, 38), rough=0.8)
M_LEATHER = mat('Pyro_Leather', (58, 38, 28), rough=0.8)
M_BONE = mat('Pyro_Bone', (196, 186, 164), rough=0.8)


def link(o, m, name, smooth=False):
    o.name = name
    for c in o.users_collection: c.objects.unlink(o)
    col.objects.link(o)
    o.data.materials.clear()
    o.data.materials.append(m)
    for p in o.data.polygons: p.use_smooth = smooth
    return o


def subsurf(o, levels=2):
    md = o.modifiers.new('sub', 'SUBSURF'); md.levels = md.render_levels = levels
    return o


def sphere(name, c, r, m, seg=16, rings=10, smooth=True):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=seg, ring_count=rings, location=c)
    o = bpy.context.active_object
    o.scale = r
    return link(o, m, name, smooth)


def cone(name, a, b, r_a, r_b, m, verts=16, open_ends=False):
    """Kerucut terpancung dari titik a (radius r_a) ke b (radius r_b)."""
    a, b = Vector(a), Vector(b)
    d = b - a
    bpy.ops.mesh.primitive_cone_add(vertices=verts, radius1=r_a, radius2=r_b, depth=d.length, location=(a + b) / 2,
                                    end_fill_type='NOTHING' if open_ends else 'NGON')
    o = bpy.context.active_object
    o.rotation_mode = 'QUATERNION'
    o.rotation_quaternion = d.to_track_quat('Z', 'Y')
    return link(o, m, name, smooth=True)


def boolean(target, cutter, op='DIFFERENCE'):
    md = target.modifiers.new(cutter.name, 'BOOLEAN')
    md.operation = op
    md.object = cutter
    cutter.hide_render = True
    cutter.hide_viewport = True


# ---------- jubah lonceng dengan pinggir robek ----------
bpy.ops.mesh.primitive_cone_add(vertices=40, radius1=0.27, radius2=0.125, depth=0.53, location=(0, 0, 0.325),
                                end_fill_type='NOTHING')
robe = link(bpy.context.active_object, M_ROBE, 'Robe', smooth=True)
jag = (0.0, 0.035, 0.012, 0.045, 0.02, 0.05, 0.015, 0.04, 0.008, 0.03)
tips = []
bottom = [v for v in robe.data.vertices if v.co.z < 0]
for i, v in enumerate(bottom):                       # cincin bawah: robekan bergerigi
    d = jag[i % len(jag)] * 0.9 if i % 2 else 0.0    # koordinat lokal sudah dalam tile
    v.co.z -= d
    if d > 0.03: tips.append((v.co.x * 0.97, v.co.y * 0.97, v.co.z + 0.325 + 0.006))
robe.data.update()
sol = robe.modifiers.new('tebal', 'SOLIDIFY'); sol.thickness = 0.01
subsurf(robe, 1)                                     # 1 level: kain halus, gerigi tetap terbaca
# ujung robekan yang membara: posisi diambil dari mesh SETELAH subdivision, karena subdivision menarik ujung ke atas
bpy.context.view_layer.update()
ev = robe.evaluated_get(bpy.context.evaluated_depsgraph_get()).to_mesh()
low = [robe.matrix_world @ v.co for v in ev.vertices]
for i, t in enumerate(tips):
    near = [p for p in low if (p.x - t[0]) ** 2 + (p.y - t[1]) ** 2 < 0.03 ** 2]
    p = min(near, key=lambda q: q.z)
    sphere(f'Burn_{i}', (p.x, p.y, p.z + 0.004), (0.008,) * 3, M_BURN, seg=8, rings=5)
robe.evaluated_get(bpy.context.evaluated_depsgraph_get()).to_mesh_clear()
# belahan depan gelap
sphere('FrontFold', (0, -0.2, 0.2), (0.022, 0.02, 0.15), M_ROBE_D)   # lipatan depan, bukan lubang
# sabuk kulit dan jimat tengkorak
bpy.ops.mesh.primitive_cylinder_add(vertices=32, radius=0.162, depth=0.035, location=(0, 0, 0.425))
link(bpy.context.active_object, M_LEATHER, 'Belt', smooth=True)
sphere('SkullCharm', (0.075, -0.17, 0.35), (0.022, 0.016, 0.02), M_BONE)
for s in (1, -1): sphere(f'CharmEye_{s}', (0.075 + s * 0.008, -0.185, 0.352), (0.005, 0.004, 0.005), M_VOID, seg=8, rings=5)

# ---------- kerudung: bola yang ditarik jadi berujung ke belakang, bukaan wajah dipotong ----------
bpy.ops.mesh.primitive_uv_sphere_add(segments=28, ring_count=18, radius=1, location=(0, 0, 0))
hood = bpy.context.active_object
for v in hood.data.vertices:
    x, y, z = v.co.x * 0.21, v.co.y * 0.2, v.co.z * 0.2
    if z > 0:                                         # puncak meruncing ke belakang-atas
        t = z / 0.2
        y += 0.07 * t * t
        z += 0.06 * t * t * t
        x *= 1 - 0.25 * t * t
    v.co = (x, y - 0.02, z + 0.76)                    # sedikit maju: bahu membungkuk
hood.data.update()
link(hood, M_ROBE, 'Hood', smooth=True)
subsurf(hood, 1)
opening = sphere('HoodOpening', (0, -0.2, 0.715), (0.14, 0.12, 0.15), M_VOID)
boolean(hood, opening)
sphere('FaceVoid', (0, -0.08, 0.715), (0.15, 0.12, 0.15), M_VOID)   # wajah hilang dalam gelap
for s in (1, -1):                                     # mata menyala tajam, miring ke dalam
    e = sphere(f'Eye_{s}', (s * 0.05, -0.185, 0.71), (0.026, 0.008, 0.009), M_EYE, seg=12, rings=6)
    e.rotation_euler = (0, s * math.radians(-18), 0)
# tepi kerudung: cincin tipis di bibir bukaan
bpy.ops.mesh.primitive_torus_add(major_radius=0.14, minor_radius=0.012, location=(0, -0.175, 0.715), rotation=(math.pi / 2, 0, 0))
rim = link(bpy.context.active_object, M_ROBE, 'HoodRim', smooth=True)
rim.scale = (1.0, 1.08, 1.0)
subsurf(sphere('Shoulders', (0, 0.0, 0.575), (0.165, 0.155, 0.06), M_ROBE, seg=24, rings=12), 1)   # bahu menyambung kerudung ke jubah

# ---------- lengan baju lebar, tangan hangus, bara api ----------
for s in (1, -1):
    sh, wr = (s * 0.12, 0.0, 0.55), (s * 0.255, -0.06, 0.31)
    sl = cone(f'Sleeve_{s}', wr, sh, 0.06, 0.04, M_ROBE, verts=16, open_ends=True)
    so = sl.modifiers.new('tebal', 'SOLIDIFY'); so.thickness = 0.008
    subsurf(sl, 1)
    sphere(f'SleeveInside_{s}', (s * 0.255, -0.065, 0.305), (0.045, 0.045, 0.02), M_VOID)
    hand = sphere(f'Hand_{s}', (s * 0.262, -0.075, 0.265), (0.022, 0.02, 0.03), M_HAND)
ember_c = (-0.27, -0.09, 0.2)                          # tangan kanan karakter = x negatif
sphere('Ember', ember_c, (0.022, 0.022, 0.024), M_EMBER)
cone('Flame', (ember_c[0], ember_c[1], ember_c[2] + 0.01), (ember_c[0], ember_c[1], ember_c[2] + 0.075), 0.02, 0.002, M_EMBER, verts=10)

# self-check: tinggi ~1 unit, kaki (ujung robek) dekat lantai
bpy.context.view_layer.update()
zs = [(o.matrix_world @ Vector(v)).z for o in col.objects if o.type == 'MESH' and not o.hide_render for v in o.bound_box]
print('PYRO ok objek', len(col.objects), 'zmin', round(min(zs), 3), 'zmax', round(max(zs), 3))
assert -0.02 < min(zs) < 0.04 and 0.92 < max(zs) < 1.06, (min(zs), max(zs))
