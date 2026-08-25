"""Build the Theme 01 gameplay art source, exports, maps, and lobby VFX alpha.

Run with Blender 5.1+:
  blender --background --factory-startup --python build_theme01_gameplay_assets.py -- <Unity project Assets path>
"""

import math
import os
import sys

import bpy
from mathutils import Vector


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


def material(name, base, emission=None, strength=0.0, metallic=0.65, roughness=0.24):
    value = bpy.data.materials.new(name)
    value.diffuse_color = (*base, 1.0)
    value.use_nodes = True
    shader = value.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value = (*base, 1.0)
    shader.inputs["Metallic"].default_value = metallic
    shader.inputs["Roughness"].default_value = roughness
    if emission is not None:
        shader.inputs["Emission Color"].default_value = (*emission, 1.0)
        shader.inputs["Emission Strength"].default_value = strength
    return value


DARK = material("DarkAlloy", (0.012, 0.025, 0.06), metallic=0.9, roughness=0.18)
GLASS = material("RunnerGlass", (0.035, 0.24, 0.62), metallic=0.15, roughness=0.08)
BLUE = material("NeonBlue", (0.015, 0.18, 0.45), (0.0, 0.65, 1.0), 8.0)
CYAN = material("NeonCyan", (0.01, 0.24, 0.35), (0.0, 1.0, 1.0), 10.0)
WHITE = material("NeonWhite", (0.4, 0.55, 0.7), (0.75, 0.95, 1.0), 6.0)
BLACK = material("Road", (0.006, 0.012, 0.028), metallic=0.72, roughness=0.3)


def link_to(obj, collection):
    for old in list(obj.users_collection):
        old.objects.unlink(obj)
    collection.objects.link(obj)


def make_collection(name):
    value = bpy.data.collections.new(name)
    bpy.context.scene.collection.children.link(value)
    return value


def empty(name, collection):
    obj = bpy.data.objects.new(name, None)
    collection.objects.link(obj)
    return obj


def bevel(obj, amount=0.08, segments=2):
    modifier = obj.modifiers.new("ProductionBevel", "BEVEL")
    modifier.width = amount
    modifier.segments = segments
    modifier.limit_method = "ANGLE"
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.modifier_apply(modifier=modifier.name)


def cube(name, location, scale, mat, collection, parent=None, bevel_size=0.06):
    bpy.ops.mesh.primitive_cube_add(location=location)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if bevel_size > 0:
        bevel(obj, bevel_size)
    obj.data.materials.append(mat)
    link_to(obj, collection)
    obj.parent = parent
    return obj


def uv_sphere(name, location, scale, mat, collection, parent=None, segments=32, rings=16):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=rings, location=location)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.data.materials.append(mat)
    link_to(obj, collection)
    obj.parent = parent
    return obj


def torus(name, location, rotation, major, minor, mat, collection, parent=None):
    bpy.ops.mesh.primitive_torus_add(
        align="WORLD", major_radius=major, minor_radius=minor,
        major_segments=32, minor_segments=8, location=location, rotation=rotation)
    obj = bpy.context.object
    obj.name = name
    obj.data.materials.append(mat)
    link_to(obj, collection)
    obj.parent = parent
    return obj


def cylinder(name, location, rotation, radius, depth, mat, collection, parent=None):
    bpy.ops.mesh.primitive_cylinder_add(vertices=24, radius=radius, depth=depth,
                                       location=location, rotation=rotation)
    obj = bpy.context.object
    obj.name = name
    bevel(obj, min(radius * 0.15, 0.06))
    obj.data.materials.append(mat)
    link_to(obj, collection)
    obj.parent = parent
    return obj


def build_runner():
    collection = make_collection("CyberOrbRunner")
    root = empty("CyberOrbRunner", collection)
    cube("Chassis", (0, 0.68, 0), (0.86, 0.24, 1.16), DARK, collection, root, 0.16)
    uv_sphere("HullShell", (0, 0.96, 0), (0.94, 0.84, 1.04), GLASS, collection, root)
    cube("ColorShell", (0, 1.10, -0.93), (0.64, 0.34, 0.11),
         BLUE, collection, root, 0.18)
    uv_sphere("FrontVisor", (0, 1.08, 0.79), (0.60, 0.38, 0.22), DARK, collection, root)
    cube("VisorGlow", (0, 1.08, 1.01), (0.45, 0.08, 0.025),
         CYAN, collection, root, 0.04)
    for side in (-1, 1):
        torus("SideRing_L" if side < 0 else "SideRing_R",
              (side * 0.96, 0.72, -0.08), (0, math.pi / 2, 0), 0.48, 0.12,
              DARK, collection, root)
        torus("SideGlow_L" if side < 0 else "SideGlow_R",
              (side * 0.975, 0.72, -0.08), (0, math.pi / 2, 0), 0.35, 0.05,
              CYAN, collection, root)
        fin = cube("SideFin_L" if side < 0 else "SideFin_R",
                   (side * 1.02, 0.82, -0.58), (0.16, 0.13, 0.52),
                   DARK, collection, root, 0.07)
        fin.rotation_euler.y = math.radians(side * 10)

    cube("RearBumper", (0, 0.58, -0.91), (0.88, 0.16, 0.18),
         DARK, collection, root, 0.08)
    for side in (-1, 1):
        cylinder("RearThruster_L" if side < 0 else "RearThruster_R",
                 (side * 0.38, 0.72, -1.03), (0, 0, 0), 0.25, 0.38,
                 DARK, collection, root)
        torus("RearThrusterGlow_L" if side < 0 else "RearThrusterGlow_R",
              (side * 0.38, 0.72, -1.25), (0, 0, 0), 0.18, 0.05,
              CYAN, collection, root)

    for side in (-1, 1):
        chevron = cube(
            "RearChevronGlow_L" if side < 0 else "RearChevronGlow_R",
            (side * 0.17, 1.20, -1.065), (0.22, 0.045, 0.025),
            WHITE, collection, root, 0.025)
        chevron.rotation_euler.z = math.radians(side * 32)
    cube("RearLightBar", (0, 0.98, -1.07), (0.48, 0.035, 0.025),
         CYAN, collection, root, 0.02)
    cube("RunnerArrow", (0, 1.69, -0.10), (0.22, 0.035, 0.30), WHITE,
         collection, root, 0.03).rotation_euler.y = math.radians(45)
    return root


def build_gate():
    collection = make_collection("NeonGate")
    root = empty("NeonGate", collection)
    left = cube("Left", (-2.55, 1.55, 0), (0.42, 1.55, 0.35), DARK, collection, root, 0.14)
    right = cube("Right", (2.55, 1.55, 0), (0.42, 1.55, 0.35), DARK, collection, root, 0.14)
    top = cube("Top", (0, 3.1, 0), (2.95, 0.42, 0.35), DARK, collection, root, 0.14)
    for name, x in (("LeftNeon", -2.55), ("RightNeon", 2.55)):
        cube(name, (x, 1.55, -0.37), (0.11, 1.35, 0.035), BLUE, collection, root, 0.025)
    cube("TopNeon", (0, 3.1, -0.37), (2.72, 0.11, 0.035), BLUE, collection, root, 0.025)
    for i, x in enumerate((-2.55, -1.25, 0, 1.25, 2.55)):
        cube(f"Fracture_{i:02}", (x, 3.1, 0.38), (0.22, 0.22, 0.12),
             DARK if i % 2 == 0 else BLUE, collection, root, 0.04)
    return root


def build_track():
    collection = make_collection("NeonTrackSegment")
    root = empty("NeonTrackSegment", collection)
    cube("Road", (0, -0.08, 0), (3.5, 0.10, 20.0), BLACK, collection, root, 0.02)
    for side in (-1, 1):
        cube("Rail_L" if side < 0 else "Rail_R", (side * 3.34, 0.18, 0),
             (0.08, 0.08, 20.0), CYAN, collection, root, 0.03)
        cube("Barrier_L" if side < 0 else "Barrier_R", (side * 3.48, 0.42, 0),
             (0.08, 0.35, 20.0), DARK, collection, root, 0.04)
    for z in range(-18, 20, 4):
        cube(f"LanePulse_{z + 18:02}", (0, 0.035, z), (0.035, 0.018, 0.8),
             BLUE, collection, root, 0.01)
    return root


def build_goal():
    collection = make_collection("NeonGoalPortal")
    root = empty("NeonGoalPortal", collection)
    torus("GoalRing", (0, 2.15, 0), (math.pi / 2, 0, 0), 2.35, 0.18,
          WHITE, collection, root)
    torus("GoalGlow", (0, 2.15, -0.02), (math.pi / 2, 0, 0), 2.05, 0.06,
          CYAN, collection, root)
    cube("GoalBase", (0, 0.12, 0), (2.8, 0.12, 0.55), DARK, collection, root, 0.08)
    return root


def build_city():
    collection = make_collection("NeonCityBackdrop")
    root = empty("NeonCityBackdrop", collection)
    for side in (-1, 1):
        for index in range(12):
            x = side * (5.5 + (index % 3) * 2.1)
            z = 8 + index * 8
            height = 3.0 + ((index * 7) % 6)
            building = cube(f"City_{'L' if side < 0 else 'R'}_{index:02}",
                            (x, height * 0.5 - 0.1, z), (0.8, height * 0.5, 1.25),
                            DARK, collection, root, 0.08)
            cube(f"CityGlow_{'L' if side < 0 else 'R'}_{index:02}",
                 (x - side * 0.81, height * 0.55, z - 0.15), (0.025, height * 0.28, 0.55),
                 BLUE if index % 2 == 0 else CYAN, collection, root, 0.01)
    return root


def save_image(name, generator, size=512, alpha=False):
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


def build_maps(prefix, accent):
    def base(u, v):
        panel = 0.045 if (int(u * 16) + int(v * 16)) % 2 else 0.075
        line = 1.0 if min((u * 8) % 1, (v * 8) % 1) < 0.025 else 0.0
        return (panel + accent[0] * line * 0.28,
                panel + accent[1] * line * 0.28,
                panel * 1.3 + accent[2] * line * 0.32, 1.0)
    def emission(u, v):
        line = 1.0 if min((u * 8) % 1, (v * 8) % 1) < 0.03 else 0.0
        return (accent[0] * line, accent[1] * line, accent[2] * line, 1.0)
    def normal(u, v):
        return (0.5, 0.5, 1.0, 1.0)
    def metal(u, v):
        return (0.82, 0.24, 0.0, 0.78)
    def uv(u, v):
        line = 1.0 if min((u * 8) % 1, (v * 8) % 1) < 0.012 else 0.0
        return (line, line, line, line)
    save_image(prefix + "_BaseColor", base)
    save_image(prefix + "_Emission", emission)
    save_image(prefix + "_Normal", normal)
    save_image(prefix + "_MetallicSmoothness", metal)
    save_image(prefix + "_UVLayout", uv, alpha=True)


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


def convert_lobby_ambient_alpha():
    path = os.path.join(ASSETS, "Game", "Art", "Lobby", "Theme01", "ColorCourtyard_Ambient.png")
    if not os.path.exists(path):
        return
    image = bpy.data.images.load(path, check_existing=False)
    width, height = image.size[:]
    source = list(image.pixels)
    converted = source[:]
    transparent = 0
    for offset in range(0, len(source), 4):
        r, g, b = source[offset], source[offset + 1], source[offset + 2]
        saturation = max(r, g, b) - min(r, g, b)
        alpha = max(0.0, min(1.0, (saturation - 0.018) * 5.2))
        if alpha < 0.01:
            alpha = 0.0
            transparent += 1
        converted[offset] = r
        converted[offset + 1] = g
        converted[offset + 2] = b
        converted[offset + 3] = alpha
    output = bpy.data.images.new("ColorCourtyard_Ambient_RGBA", width=width, height=height, alpha=True)
    output.alpha_mode = "STRAIGHT"
    output.pixels.foreach_set(converted)
    temporary = path + ".rgba.png"
    output.filepath_raw = temporary
    output.file_format = "PNG"
    output.save()
    os.replace(temporary, path)
    print("AMBIENT_TRANSPARENT_RATIO", transparent / (len(source) / 4))
    bpy.data.images.remove(output)
    bpy.data.images.remove(image)


reset_scene()
runner = build_runner()
gate = build_gate()
track = build_track()
goal = build_goal()
city = build_city()
build_maps("Runner", (0.0, 0.75, 1.0))
build_maps("Gate", (0.0, 0.65, 1.0))
build_maps("Environment", (0.0, 0.8, 1.0))
blend_path = os.path.join(SOURCE, "Theme01_Gameplay.blend")
bpy.ops.wm.save_as_mainfile(filepath=blend_path)
export_root(runner, "CyberOrbRunner.fbx")
export_root(gate, "NeonGate.fbx")
export_root(track, "NeonTrackSegment.fbx")
export_root(goal, "NeonGoalPortal.fbx")
export_root(city, "NeonCityBackdrop.fbx")
convert_lobby_ambient_alpha()
backup_path = blend_path + "1"
if os.path.exists(backup_path):
    os.remove(backup_path)
print("THEME01_BUILD_COMPLETE", ROOT)
