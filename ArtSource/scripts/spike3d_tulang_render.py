"""SPIKE 3D (boleh dibuang): render Tulang Penjaga dari 4 sisi + sudut kamera game.

Jalankan di Blender setelah spike3d_tulang_blender.py. Output PNG ke ArtSource/spike3d/render_*.png.
Panggung (lantai, lampu, kamera) ada di koleksi "SpikeStage" dan dibuat ulang setiap kali dijalankan.
"""
import math
import os
import bpy

OUT = r'D:\Claude\PGIM\Runecrypt\ArtSource\spike3d'
scene = bpy.context.scene

old = bpy.data.collections.get('SpikeStage')
if old:
    for o in list(old.objects): bpy.data.objects.remove(o, do_unlink=True)
    bpy.data.collections.remove(old)
stage = bpy.data.collections.new('SpikeStage')
scene.collection.children.link(stage)
for o in [o for o in scene.objects if o.type in ('LIGHT', 'CAMERA') and o.name in ('Light', 'Camera')]:
    o.hide_render = True

for eng in ('BLENDER_EEVEE', 'BLENDER_EEVEE_NEXT'):
    try:
        scene.render.engine = eng
        break
    except TypeError:
        continue
scene.render.resolution_x = scene.render.resolution_y = 512
scene.render.film_transparent = True
scene.view_settings.view_transform = 'Standard'
world = scene.world or bpy.data.worlds.new('World')
scene.world = world
world.use_nodes = True
world.node_tree.nodes['Background'].inputs['Color'].default_value = (0.02, 0.02, 0.025, 1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value = 1.0
try:
    scene.eevee.use_bloom = True
except AttributeError:
    pass


def add(obj):
    stage.objects.link(obj)
    return obj


def light(name, kind, loc, energy, color, size=0.3):
    ld = bpy.data.lights.new(name, kind)
    ld.energy = energy
    ld.color = color
    if kind in ('POINT', 'SPOT', 'AREA'): ld.shadow_soft_size = size
    o = add(bpy.data.objects.new(name, ld))
    o.location = loc
    return o


def camera(name, loc, rot_deg, scale):
    cd = bpy.data.cameras.new(name)
    cd.type = 'ORTHO'
    cd.ortho_scale = scale
    o = add(bpy.data.objects.new(name, cd))
    o.location = loc
    o.rotation_euler = [math.radians(a) for a in rot_deg]
    return o


def render(cam, path):
    scene.camera = cam
    scene.render.filepath = path
    bpy.ops.render.render(write_still=True)


# --- 4 sisi: cahaya studio netral supaya bentuk terbaca ---
key = light('Key', 'SUN', (0, 0, 0), 3.0, (1, 0.97, 0.92))
key.rotation_euler = (math.radians(50), 0, math.radians(-35))
fill = light('Fill', 'SUN', (0, 0, 0), 1.0, (0.8, 0.85, 1.0))
fill.rotation_euler = (math.radians(60), 0, math.radians(150))
views = {
    'front': ((0, -3, 0.5), (90, 0, 0)),
    'side': ((3, 0, 0.5), (90, 0, 90)),
    'back': ((0, 3, 0.5), (90, 0, 180)),
    'top': ((0, 0, 3), (0, 0, 0)),
}
for name, (loc, rot) in views.items():
    render(camera('Cam_' + name, loc, rot, 1.15), os.path.join(OUT, f'render_{name}.png'))

# --- sudut kamera game: lantai gelap, tanpa cahaya studio, hanya cahaya hangat pemain + sedikit cahaya ruang ---
key.hide_render = fill.hide_render = True
scene.render.film_transparent = False
bpy.ops.mesh.primitive_plane_add(size=6, location=(0, 0, 0))
floor = bpy.context.active_object
for c in floor.users_collection: c.objects.unlink(floor)
add(floor)
fm = bpy.data.materials.get('Spike_Floor') or bpy.data.materials.new('Spike_Floor')
fm.use_nodes = True
fm.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value = (0.09, 0.08, 0.1, 1)
floor.data.materials.append(fm)
light('PlayerLight', 'POINT', (-0.7, -0.9, 0.8), 90, (1.0, 0.55, 0.25), size=0.2)
light('Ambient', 'SUN', (0, 0, 0), 0.35, (0.6, 0.6, 0.9))
cam = camera('Cam_game', (0, -2.2, 2.6), (40, 0, 0), 1.3)
render(cam, os.path.join(OUT, 'render_game.png'))
print('RENDER ok')
