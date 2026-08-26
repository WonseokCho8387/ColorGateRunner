"""Build Theme 01 fog-bank geometry and road/ice surface maps.

Run with Blender 5.1+:
  blender --background --factory-startup --python \
    build_theme01_mechanic_polish_assets.py -- <Unity project Assets path>
"""

import math
import os
import sys

import bpy


def unity_assets_path():
    marker = sys.argv.index("--") if "--" in sys.argv else -1
    if marker < 0 or marker + 1 >= len(sys.argv):
        raise RuntimeError("Expected Unity Assets path after --")
    return os.path.abspath(sys.argv[marker + 1])


ASSETS = unity_assets_path()
ROOT = os.path.join(ASSETS, "Game", "Art", "Gameplay", "Theme01")
SOURCE = os.path.join(ROOT, "Source")
MODELS = os.path.join(ROOT, "Models")
TEXTURES = os.path.join(ROOT, "Textures")
for folder in (SOURCE, MODELS, TEXTURES):
    os.makedirs(folder, exist_ok=True)


def reset_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for collection in list(bpy.data.collections):
        if collection.name != "Collection":
            bpy.data.collections.remove(collection)


def make_collection(name):
    value = bpy.data.collections.new(name)
    bpy.context.scene.collection.children.link(value)
    return value


def link_to(obj, collection):
    for old in list(obj.users_collection):
        old.objects.unlink(obj)
    collection.objects.link(obj)


def build_fog_bank():
    collection = make_collection("FogBankSection")
    root = bpy.data.objects.new("FogBankSection", None)
    collection.objects.link(root)
    material = bpy.data.materials.new("FogBankPreview")
    material.diffuse_color = (0.16, 0.22, 0.30, 0.42)
    material.use_nodes = True
    shader = material.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value = (0.16, 0.22, 0.30, 1.0)
    shader.inputs["Roughness"].default_value = 1.0
    shader.inputs["Alpha"].default_value = 0.42

    layouts = (
        (-4.6, -0.4, -0.8, 3.2, 2.8, 2.8),
        (-2.8, 1.2, 0.8, 3.8, 3.2, 2.4),
        (-0.8, -0.8, -1.0, 4.1, 2.9, 3.0),
        (1.7, 1.0, 0.5, 4.0, 3.4, 2.8),
        (4.4, -0.3, -0.6, 3.1, 2.7, 2.5),
        (-4.1, 2.7, 1.4, 2.6, 2.1, 2.1),
        (-1.4, 3.4, -0.2, 3.1, 2.2, 2.4),
        (2.1, 3.3, 1.2, 3.0, 2.4, 2.2),
        (4.4, 2.5, 0.2, 2.4, 2.0, 2.0),
        (0.2, 5.0, 0.4, 3.6, 1.9, 2.1),
    )
    volumes = []
    for index, (x, y, z, sx, sy, sz) in enumerate(layouts):
        bpy.ops.mesh.primitive_ico_sphere_add(
            subdivisions=2,
            radius=1.0,
            location=(x, y, z))
        volume = bpy.context.object
        volume.name = f"FogBillow_{index:02}"
        volume.scale = (sx, sy, sz)
        bpy.ops.object.transform_apply(
            location=False,
            rotation=False,
            scale=True)
        volume.data.materials.append(material)
        for polygon in volume.data.polygons:
            polygon.use_smooth = True
        link_to(volume, collection)
        volumes.append(volume)

    bpy.ops.object.select_all(action="DESELECT")
    for volume in volumes:
        volume.select_set(True)
    bpy.context.view_layer.objects.active = volumes[0]
    bpy.ops.object.join()
    fog = bpy.context.object
    fog.name = "FogVolume"
    fog.parent = root
    return root


def save_image(name, generator, size=256, alpha=False):
    image = bpy.data.images.new(name, width=size, height=size, alpha=alpha)
    pixels = [0.0] * (size * size * 4)
    for y in range(size):
        for x in range(size):
            r, g, b, a = generator(x / (size - 1), y / (size - 1))
            offset = (y * size + x) * 4
            pixels[offset:offset + 4] = (r, g, b, a)
    image.pixels.foreach_set(pixels)
    image.filepath_raw = os.path.join(TEXTURES, name + ".png")
    image.file_format = "PNG"
    image.save()
    bpy.data.images.remove(image)


def grain(u, v):
    return (
        math.sin(u * 173.0 + v * 71.0) * 0.45 +
        math.sin(u * 419.0 - v * 257.0) * 0.30 +
        math.sin((u + v) * 911.0) * 0.25)


def build_surface_maps():
    def road_base(u, v):
        noise = grain(u, v)
        aggregate = 0.055 + noise * 0.018
        seam = min(abs((u * 7.0) % 1.0 - 0.5),
                   abs((v * 11.0) % 1.0 - 0.5))
        seam_dark = 0.018 if seam < 0.012 else 0.0
        value = max(0.015, aggregate - seam_dark)
        return (value * 0.78, value * 0.9, value * 1.08, 1.0)

    def road_normal(u, v):
        nx = math.sin(u * 251.0 + v * 83.0) * 0.12
        ny = math.cos(v * 233.0 - u * 57.0) * 0.12
        return (0.5 + nx, 0.5 + ny, 0.98, 1.0)

    def road_surface(u, v):
        rough_variation = 0.18 + (grain(u, v) + 1.0) * 0.025
        return (0.08, rough_variation, 0.0, 0.22)

    def ice_crack(u, v):
        first = abs(math.sin(u * 16.0 + math.sin(v * 8.0) * 1.7))
        second = abs(math.sin(v * 21.0 - math.sin(u * 11.0) * 1.4))
        return min(first, second)

    def ice_base(u, v):
        crack = ice_crack(u, v)
        frost = (grain(u, v) + 1.0) * 0.035
        line = max(0.0, (0.08 - crack) / 0.08)
        return (0.12 + frost + line * 0.28,
                0.34 + frost + line * 0.36,
                0.46 + frost + line * 0.44,
                1.0)

    def ice_normal(u, v):
        crack = ice_crack(u, v)
        strength = max(0.0, (0.12 - crack) / 0.12)
        return (0.5 + math.sin(v * 37.0) * strength * 0.16,
                0.5 + math.cos(u * 31.0) * strength * 0.16,
                0.98,
                1.0)

    def ice_surface(u, v):
        return (0.04, 0.10, 0.0, 0.92)

    def fog_noise(u, v):
        noise = 0.5 + grain(u, v) * 0.22
        broad = 0.5 + math.sin(u * 9.0 + v * 6.0) * 0.16
        alpha = max(0.12, min(0.72, noise * broad))
        value = 0.72 + noise * 0.18
        return (value * 0.72, value * 0.82, value, alpha)

    save_image("Road_BaseColor", road_base)
    save_image("Road_Normal", road_normal)
    save_image("Road_MetallicSmoothness", road_surface)
    save_image("Ice_BaseColor", ice_base)
    save_image("Ice_Normal", ice_normal)
    save_image("Ice_MetallicSmoothness", ice_surface)
    save_image("Fog_Noise", fog_noise, alpha=True)


def export_root(root, filename):
    bpy.ops.object.select_all(action="DESELECT")
    root.select_set(True)
    for child in root.children_recursive:
        child.select_set(True)
    bpy.context.view_layer.objects.active = root
    bpy.ops.export_scene.fbx(
        filepath=os.path.join(MODELS, filename),
        use_selection=True,
        use_mesh_modifiers=True,
        apply_scale_options="FBX_SCALE_UNITS",
        axis_forward="-Z",
        axis_up="Y",
        add_leaf_bones=False,
        mesh_smooth_type="FACE")


reset_scene()
fog_root = build_fog_bank()
build_surface_maps()
blend_path = os.path.join(SOURCE, "Theme01_Mechanics.blend")
bpy.ops.wm.save_as_mainfile(filepath=blend_path)
export_root(fog_root, "FogBankSection.fbx")
backup_path = blend_path + "1"
if os.path.exists(backup_path):
    os.remove(backup_path)
print("THEME01_MECHANIC_POLISH_BUILD_COMPLETE", ROOT)
