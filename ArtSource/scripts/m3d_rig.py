"""Runecrypt 3D: helper rig dan animasi untuk script karakter (m3d_tulang_rig.py, m3d_pyromancer_rig.py).

Jalankan di dalam Blender lewat exec(open(path).read()). Satuan Blender = tile, karakter menghadap -Y, z atas.
Pose ditulis dalam sumbu ARMATURE (bukan sumbu lokal tulang yang bergantung roll): rotasi tulang adalah delta
terhadap pose rest, diputar di kepala tulang, dan berantai seperti FK biasa. key_clip mengubahnya ke matrix_basis.
Rotasi +X memutar ujung tulang yang mengarah ke bawah ke arah +Y (belakang).
"""
import math
import bpy
from mathutils import Matrix, Quaternion, Vector

FPS = 60


def _remove(name):
    o = bpy.data.objects.get(name)
    if o:
        data = o.data
        bpy.data.objects.remove(o, do_unlink=True)
        if data is not None and data.users == 0:
            (bpy.data.meshes if isinstance(data, bpy.types.Mesh) else bpy.data.armatures).remove(data)


def _rig_collection():
    c = bpy.data.collections.get('Rigs')
    if c is None:
        c = bpy.data.collections.new('Rigs')
        bpy.context.scene.collection.children.link(c)
    return c


def bake_mesh(col_name, out_name, weigh):
    """Salin semua mesh/kurva yang dirender di koleksi (modifier diterapkan), beri bobot per vertex, gabung jadi satu.

    weigh(obj_name, co_dunia) -> {nama_tulang: bobot}. Dict kosong = objek tidak dikenal = gagal.
    """
    _remove(out_name)
    col = bpy.data.collections[col_name]
    dg = bpy.context.evaluated_depsgraph_get()
    parts = []
    for obj in col.all_objects:
        if obj.type not in ('MESH', 'CURVE') or obj.hide_render:
            continue
        me = bpy.data.meshes.new_from_object(obj.evaluated_get(dg), preserve_all_data_layers=True, depsgraph=dg)
        me.transform(obj.matrix_world)
        part = bpy.data.objects.new(out_name, me)
        _rig_collection().objects.link(part)
        groups = {}
        for v in me.vertices:
            w = weigh(obj.name, v.co)
            assert w, 'objek tanpa aturan bobot: ' + obj.name
            for bone, value in w.items():
                if value <= 0:
                    continue
                if bone not in groups:
                    groups[bone] = part.vertex_groups.new(name=bone)
                groups[bone].add([v.index], value, 'REPLACE')
        parts.append(part)
    assert parts, 'koleksi kosong: ' + col_name
    with bpy.context.temp_override(active_object=parts[0], selected_editable_objects=parts):
        bpy.ops.object.join()
    out = parts[0]
    out.name = out_name
    out.data.name = out_name
    return out


def make_armature(name, bones):
    """bones: [(nama, induk|None, head, tail, connect)]. Semua deform, rotasi quaternion."""
    _remove(name)
    arm = bpy.data.armatures.new(name)
    obj = bpy.data.objects.new(name, arm)
    _rig_collection().objects.link(obj)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.mode_set(mode='EDIT')
    for n, parent, head, tail, connect in bones:
        eb = arm.edit_bones.new(n)
        eb.head, eb.tail, eb.roll = Vector(head), Vector(tail), 0.0
        if parent:
            eb.parent = arm.edit_bones[parent]
            eb.use_connect = connect
    bpy.ops.object.mode_set(mode='OBJECT')
    for pb in obj.pose.bones:
        pb.rotation_mode = 'QUATERNION'
    return obj


def bind(mesh, arm):
    md = mesh.modifiers.new('Armature', 'ARMATURE')
    md.object = arm
    mw = mesh.matrix_world.copy()
    mesh.parent = arm
    mesh.matrix_world = mw


def apply_pose(arm, pose):
    """pose: {tulang: (rotasi Euler/Quaternion sumbu armature, offset lokasi Vector sumbu armature)}."""
    for pb in arm.pose.bones:
        rot, loc = pose.get(pb.name, (None, None))
        rr = pb.bone.matrix_local.to_3x3()
        q = (rot.to_quaternion() if rot is not None else Quaternion())
        basis_rot = (rr.inverted() @ q.to_matrix() @ rr).to_quaternion()
        basis_loc = rr.inverted() @ (loc if loc is not None else Vector())
        pb.rotation_quaternion = basis_rot
        pb.location = basis_loc


def key_clip(arm, name, frames, pose_fn):
    """Key semua tulang di frame 0..frames dari pose_fn(phase) -> pose (lihat apply_pose), lalu jadikan NLA strip."""
    old = bpy.data.actions.get(name + '_' + arm.name)
    if old:
        bpy.data.actions.remove(old)
    ad = arm.animation_data_create()
    for t in [t for t in ad.nla_tracks if t.name == name]:
        ad.nla_tracks.remove(t)
    act = bpy.data.actions.new(name + '_' + arm.name)
    ad.action = act
    prev = {}
    for f in range(frames + 1):
        apply_pose(arm, pose_fn(f / frames))
        for pb in arm.pose.bones:
            q = pb.rotation_quaternion.copy()
            if pb.name in prev:
                q.make_compatible(prev[pb.name])  # cegah lompatan tanda quaternion antar frame
            pb.rotation_quaternion = prev[pb.name] = q
            pb.keyframe_insert('rotation_quaternion', frame=f, group=pb.name)
            pb.keyframe_insert('location', frame=f, group=pb.name)
    act.use_frame_range = True
    act.frame_start, act.frame_end = 0, frames
    act.use_cyclic = True
    ad.action = None
    track = ad.nla_tracks.new()
    track.name = name
    track.strips.new(name, 0, act)
    apply_pose(arm, {})
    return act


def two_bone_ik(hip, target, l1, l2):
    """IK analitis di bidang y-z. hip, target: (y, z). Hasil: (sudut paha absolut, tekuk lutut) dalam radian.

    Sudut 0 = lurus ke bawah, positif = ke +Y (belakang). Lutut menekuk ke depan (-Y), jadi tekuk lutut positif.
    """
    dy, dz = target[0] - hip[0], target[1] - hip[1]
    dist = math.hypot(dy, dz)
    assert dist <= 0.98 * (l1 + l2), f'target di luar jangkauan kaki: {dist:.3f} > {0.98 * (l1 + l2):.3f}'
    a = math.atan2(dy, -dz)
    alpha = math.acos(max(-1.0, min(1.0, (l1 * l1 + dist * dist - l2 * l2) / (2 * l1 * dist))))
    beta = math.acos(max(-1.0, min(1.0, (l1 * l1 + l2 * l2 - dist * dist) / (2 * l1 * l2))))
    return a - alpha, math.pi - beta


def bone_world(arm, name, tail=False):
    pb = arm.pose.bones[name]
    return arm.matrix_world @ (pb.tail if tail else pb.head)


def export_fbx(arm, mesh, path):
    bpy.ops.object.select_all(action='DESELECT')
    arm.select_set(True)
    mesh.select_set(True)
    bpy.context.view_layer.objects.active = arm
    bpy.context.scene.render.fps = FPS
    bpy.ops.export_scene.fbx(
        filepath=path, use_selection=True, object_types={'ARMATURE', 'MESH'},
        add_leaf_bones=False, use_armature_deform_only=True, armature_nodetype='NULL',
        bake_anim=True, bake_anim_use_nla_strips=True, bake_anim_use_all_actions=False,
        bake_anim_force_startend_keying=True, bake_anim_simplify_factor=0.0, bake_anim_use_all_bones=True,
        # ekspor uji 2026-11-21: forward 'Z' + bakeAxisConversion Unity mati -> depan (-Y Blender) jadi +Z, skala 1
        use_mesh_modifiers=True, apply_scale_options='FBX_SCALE_ALL', axis_forward='Z', axis_up='Y')


bpy.context.scene.render.fps = FPS
