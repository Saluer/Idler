"""
generate_hero.py — Blender Python script to create a Bandit Hero character.

Run in Blender 3.x+:
  - Scripting tab → Open → Run Script
  - Or: blender --background --python generate_hero.py

Style: Mid-poly, Madness Combat / Megabonk inspired.
Character: Bandit cowboy — black glasses, mask, cowboy hat, duster coat.
"""

import bpy
import bmesh
import math
import os

# ---------------------------------------------------------------------------
# 1. Utility functions
# ---------------------------------------------------------------------------

def clear_scene():
    """Remove all mesh objects and orphan data from the scene."""
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    # Purge orphan data
    for block in bpy.data.meshes:
        if block.users == 0:
            bpy.data.meshes.remove(block)
    for block in bpy.data.materials:
        if block.users == 0:
            bpy.data.materials.remove(block)


def get_or_create_material(name, color):
    """Return an existing material or create a new one with the given RGBA color."""
    mat = bpy.data.materials.get(name)
    if mat is None:
        mat = bpy.data.materials.new(name=name)
        mat.use_nodes = True
        bsdf = mat.node_tree.nodes.get("Principled BSDF")
        if bsdf:
            bsdf.inputs["Base Color"].default_value = color
            bsdf.inputs["Roughness"].default_value = 0.7
    return mat


def assign_material(obj, mat):
    """Assign a material to an object, replacing any existing slots."""
    obj.data.materials.clear()
    obj.data.materials.append(mat)


def add_uv_sphere(name, radius=1.0, segments=24, rings=16, location=(0, 0, 0)):
    """Add a UV sphere and return it."""
    bpy.ops.mesh.primitive_uv_sphere_add(
        radius=radius, segments=segments, ring_count=rings, location=location
    )
    obj = bpy.context.active_object
    obj.name = name
    bpy.ops.object.shade_smooth()
    return obj


def add_cube(name, size=1.0, location=(0, 0, 0)):
    """Add a cube and return it."""
    bpy.ops.mesh.primitive_cube_add(size=size, location=location)
    obj = bpy.context.active_object
    obj.name = name
    return obj


def add_cylinder(name, radius=1.0, depth=1.0, vertices=24, location=(0, 0, 0)):
    """Add a cylinder and return it."""
    bpy.ops.mesh.primitive_cylinder_add(
        radius=radius, depth=depth, vertices=vertices, location=location
    )
    obj = bpy.context.active_object
    obj.name = name
    bpy.ops.object.shade_smooth()
    return obj


def add_cone(name, radius1=1.0, radius2=0.0, depth=1.0, vertices=24, location=(0, 0, 0)):
    """Add a cone and return it."""
    bpy.ops.mesh.primitive_cone_add(
        radius1=radius1, radius2=radius2, depth=depth, vertices=vertices, location=location
    )
    obj = bpy.context.active_object
    obj.name = name
    return obj


def join_objects(objects):
    """Join a list of objects into a single mesh. Returns the joined object."""
    if not objects:
        return None
    bpy.ops.object.select_all(action='DESELECT')
    for obj in objects:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = objects[0]
    bpy.ops.object.join()
    return bpy.context.active_object


def apply_transforms(obj):
    """Apply location, rotation, and scale transforms."""
    bpy.ops.object.select_all(action='DESELECT')
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)


# ---------------------------------------------------------------------------
# 2. Materials — dark bandit palette
# ---------------------------------------------------------------------------

def create_materials():
    """Create all character materials and return them as a dict."""
    return {
        "skin":       get_or_create_material("Skin",       (0.85, 0.72, 0.58, 1.0)),
        "black":      get_or_create_material("Black",      (0.02, 0.02, 0.02, 1.0)),
        "dark_grey":  get_or_create_material("DarkGrey",   (0.15, 0.15, 0.15, 1.0)),
        "dark_brown": get_or_create_material("DarkBrown",  (0.18, 0.10, 0.05, 1.0)),
        "coat":       get_or_create_material("Coat",       (0.05, 0.05, 0.07, 1.0)),
        "pants":      get_or_create_material("Pants",      (0.08, 0.08, 0.10, 1.0)),
        "belt":       get_or_create_material("Belt",       (0.12, 0.08, 0.04, 1.0)),
        "bandana":    get_or_create_material("Bandana",    (0.12, 0.12, 0.12, 1.0)),
    }


# ---------------------------------------------------------------------------
# 3. Head — oval / pill shape (Madness Combat style)
# ---------------------------------------------------------------------------

def create_head(mats):
    """Create the head: a vertically stretched sphere (oval/pill)."""
    head = add_uv_sphere("Head", radius=0.32, segments=24, rings=16, location=(0, 0, 1.65))
    head.scale = (1.0, 0.85, 1.25)
    apply_transforms(head)
    assign_material(head, mats["skin"])
    return head


# ---------------------------------------------------------------------------
# 4. Sunglasses — thick wraparound shades
# ---------------------------------------------------------------------------

def create_sunglasses(mats):
    """Create wraparound sunglasses: two lenses + bridge."""
    parts = []

    # Left lens
    lens_l = add_cube("Lens_L", size=0.16, location=(-0.12, -0.26, 1.70))
    lens_l.scale = (1.3, 0.3, 0.7)
    apply_transforms(lens_l)
    assign_material(lens_l, mats["black"])
    parts.append(lens_l)

    # Right lens
    lens_r = add_cube("Lens_R", size=0.16, location=(0.12, -0.26, 1.70))
    lens_r.scale = (1.3, 0.3, 0.7)
    apply_transforms(lens_r)
    assign_material(lens_r, mats["black"])
    parts.append(lens_r)

    # Bridge
    bridge = add_cube("Bridge", size=0.04, location=(0, -0.27, 1.70))
    bridge.scale = (1.5, 0.4, 0.5)
    apply_transforms(bridge)
    assign_material(bridge, mats["black"])
    parts.append(bridge)

    # Side arms (temple pieces) that wrap around
    for side, x in [("L", -0.22), ("R", 0.22)]:
        arm = add_cube(f"GlassArm_{side}", size=0.03, location=(x, -0.12, 1.70))
        arm.scale = (0.4, 5.0, 0.5)
        apply_transforms(arm)
        assign_material(arm, mats["black"])
        parts.append(arm)

    glasses = join_objects(parts)
    glasses.name = "Sunglasses"
    return glasses


# ---------------------------------------------------------------------------
# 5. Mask / Bandana — covering lower face
# ---------------------------------------------------------------------------

def create_mask(mats):
    """Create a triangular bandit bandana covering the lower face."""
    # Main mask — scaled sphere cut to lower half feel
    mask = add_cube("Mask", size=0.30, location=(0, -0.20, 1.55))
    mask.scale = (1.2, 0.5, 0.8)
    apply_transforms(mask)

    # Taper the bottom using edit mode to make it triangular
    bpy.ops.object.select_all(action='DESELECT')
    mask.select_set(True)
    bpy.context.view_layer.objects.active = mask
    bpy.ops.object.mode_set(mode='EDIT')
    bm = bmesh.from_edit_mesh(mask.data)
    bm.verts.ensure_lookup_table()

    # Scale bottom vertices inward for triangular shape
    for v in bm.verts:
        if v.co.z < -0.05:
            v.co.x *= 0.3
            v.co.y *= 0.6
            v.co.z -= 0.02

    bmesh.update_edit_mesh(mask.data)
    bpy.ops.object.mode_set(mode='OBJECT')

    assign_material(mask, mats["bandana"])
    bpy.ops.object.shade_smooth()
    return mask


# ---------------------------------------------------------------------------
# 6. Hat — cowboy hat (wide brim + dome crown)
# ---------------------------------------------------------------------------

def create_hat(mats):
    """Create a cowboy hat: wide brim cylinder + dome crown."""
    parts = []

    # Brim
    brim = add_cylinder("HatBrim", radius=0.48, depth=0.04, vertices=32, location=(0, 0, 1.92))
    brim.scale = (1.0, 0.85, 1.0)
    apply_transforms(brim)
    assign_material(brim, mats["dark_brown"])
    parts.append(brim)

    # Crown (dome)
    crown = add_uv_sphere("HatCrown", radius=0.24, segments=24, rings=12, location=(0, 0, 2.02))
    crown.scale = (1.0, 0.85, 0.7)
    apply_transforms(crown)

    # Flatten the bottom of the crown (remove lower hemisphere feel)
    bpy.ops.object.select_all(action='DESELECT')
    crown.select_set(True)
    bpy.context.view_layer.objects.active = crown
    bpy.ops.object.mode_set(mode='EDIT')
    bm = bmesh.from_edit_mesh(crown.data)
    bm.verts.ensure_lookup_table()
    for v in bm.verts:
        if v.co.z < -0.02:
            v.co.z = -0.02
    bmesh.update_edit_mesh(crown.data)
    bpy.ops.object.mode_set(mode='OBJECT')

    assign_material(crown, mats["dark_brown"])
    parts.append(crown)

    # Hat band
    band = add_cylinder("HatBand", radius=0.25, depth=0.03, vertices=32, location=(0, 0, 1.94))
    band.scale = (1.0, 0.85, 1.0)
    apply_transforms(band)
    assign_material(band, mats["black"])
    parts.append(band)

    hat = join_objects(parts)
    hat.name = "CowboyHat"
    return hat


# ---------------------------------------------------------------------------
# 7. Torso — duster coat, broad shoulders, slim waist
# ---------------------------------------------------------------------------

def create_torso(mats):
    """Create the torso: a trapezoid-shaped duster coat."""
    parts = []

    # Main torso block
    torso = add_cube("Torso", size=0.5, location=(0, 0, 1.15))
    torso.scale = (1.3, 0.7, 1.4)
    apply_transforms(torso)

    # Taper waist in edit mode
    bpy.ops.object.select_all(action='DESELECT')
    torso.select_set(True)
    bpy.context.view_layer.objects.active = torso
    bpy.ops.object.mode_set(mode='EDIT')
    bm = bmesh.from_edit_mesh(torso.data)
    bm.verts.ensure_lookup_table()
    for v in bm.verts:
        # Taper bottom for waist
        if v.co.z < -0.1:
            v.co.x *= 0.75
        # Broaden shoulders at top
        if v.co.z > 0.1:
            v.co.x *= 1.1
    bmesh.update_edit_mesh(torso.data)
    bpy.ops.object.mode_set(mode='OBJECT')

    assign_material(torso, mats["coat"])
    parts.append(torso)

    # Collar / neck area
    neck = add_cylinder("Neck", radius=0.10, depth=0.12, vertices=16, location=(0, 0, 1.50))
    assign_material(neck, mats["skin"])
    parts.append(neck)

    # Belt
    belt = add_cube("Belt", size=0.1, location=(0, 0, 0.82))
    belt.scale = (3.2, 2.5, 0.5)
    apply_transforms(belt)
    assign_material(belt, mats["belt"])
    parts.append(belt)

    # Belt buckle
    buckle = add_cube("Buckle", size=0.05, location=(0, -0.19, 0.82))
    buckle.scale = (1.2, 0.5, 1.4)
    apply_transforms(buckle)
    assign_material(buckle, mats["dark_brown"])
    parts.append(buckle)

    # Coat tails (lower part of duster)
    coat_lower = add_cube("CoatTails", size=0.45, location=(0, 0, 0.60))
    coat_lower.scale = (1.15, 0.6, 0.8)
    apply_transforms(coat_lower)

    bpy.ops.object.select_all(action='DESELECT')
    coat_lower.select_set(True)
    bpy.context.view_layer.objects.active = coat_lower
    bpy.ops.object.mode_set(mode='EDIT')
    bm = bmesh.from_edit_mesh(coat_lower.data)
    bm.verts.ensure_lookup_table()
    for v in bm.verts:
        if v.co.z < -0.1:
            v.co.x *= 1.2
    bmesh.update_edit_mesh(coat_lower.data)
    bpy.ops.object.mode_set(mode='OBJECT')

    assign_material(coat_lower, mats["coat"])
    parts.append(coat_lower)

    body = join_objects(parts)
    body.name = "Torso"
    return body


# ---------------------------------------------------------------------------
# 8. Arms — cylinders with elbow, mitten hands
# ---------------------------------------------------------------------------

def create_arm(mats, side="L"):
    """Create one arm with upper arm, forearm, and mitten hand."""
    sign = -1 if side == "L" else 1
    x_shoulder = sign * 0.42
    parts = []

    # Upper arm
    upper = add_cylinder(f"UpperArm_{side}", radius=0.07, depth=0.35,
                         vertices=12, location=(x_shoulder, 0, 1.22))
    upper.rotation_euler = (0, 0, sign * 0.1)
    apply_transforms(upper)
    assign_material(upper, mats["coat"])
    parts.append(upper)

    # Forearm
    forearm_x = x_shoulder + sign * 0.05
    forearm = add_cylinder(f"Forearm_{side}", radius=0.06, depth=0.30,
                           vertices=12, location=(forearm_x, 0, 0.90))
    forearm.rotation_euler = (0, 0, sign * 0.05)
    apply_transforms(forearm)
    assign_material(forearm, mats["skin"])
    parts.append(forearm)

    # Mitten hand
    hand_x = forearm_x + sign * 0.02
    hand = add_uv_sphere(f"Hand_{side}", radius=0.065, segments=12, rings=8,
                         location=(hand_x, 0, 0.72))
    hand.scale = (0.9, 0.7, 1.2)
    apply_transforms(hand)
    assign_material(hand, mats["skin"])
    parts.append(hand)

    arm = join_objects(parts)
    arm.name = f"Arm_{side}"
    return arm


# ---------------------------------------------------------------------------
# 9. Legs — cylinders with knee, cowboy boots
# ---------------------------------------------------------------------------

def create_leg(mats, side="L"):
    """Create one leg with thigh, shin, and cowboy boot."""
    sign = -1 if side == "L" else 1
    x_hip = sign * 0.15
    parts = []

    # Thigh
    thigh = add_cylinder(f"Thigh_{side}", radius=0.09, depth=0.35,
                         vertices=14, location=(x_hip, 0, 0.45))
    assign_material(thigh, mats["pants"])
    parts.append(thigh)

    # Shin
    shin = add_cylinder(f"Shin_{side}", radius=0.075, depth=0.32,
                        vertices=14, location=(x_hip, 0, 0.12))
    assign_material(shin, mats["pants"])
    parts.append(shin)

    # Boot shaft
    boot_shaft = add_cylinder(f"BootShaft_{side}", radius=0.085, depth=0.15,
                              vertices=14, location=(x_hip, 0, -0.08))
    assign_material(boot_shaft, mats["dark_brown"])
    parts.append(boot_shaft)

    # Boot foot
    boot_foot = add_cube(f"BootFoot_{side}", size=0.14, location=(x_hip, -0.04, -0.18))
    boot_foot.scale = (0.65, 1.4, 0.5)
    apply_transforms(boot_foot)
    assign_material(boot_foot, mats["dark_brown"])
    parts.append(boot_foot)

    # Boot heel
    heel = add_cube(f"BootHeel_{side}", size=0.04, location=(x_hip, 0.06, -0.20))
    heel.scale = (0.8, 0.8, 1.2)
    apply_transforms(heel)
    assign_material(heel, mats["dark_brown"])
    parts.append(heel)

    leg = join_objects(parts)
    leg.name = f"Leg_{side}"
    return leg


# ---------------------------------------------------------------------------
# 10. Assembly — parent all to empty, center at origin
# ---------------------------------------------------------------------------

def assemble_character():
    """Build the full character and parent all parts to an empty."""
    mats = create_materials()

    # Create all body parts
    head = create_head(mats)
    glasses = create_sunglasses(mats)
    mask = create_mask(mats)
    hat = create_hat(mats)
    torso = create_torso(mats)
    arm_l = create_arm(mats, side="L")
    arm_r = create_arm(mats, side="R")
    leg_l = create_leg(mats, side="L")
    leg_r = create_leg(mats, side="R")

    all_parts = [head, glasses, mask, hat, torso, arm_l, arm_r, leg_l, leg_r]

    # Create parent empty
    bpy.ops.object.empty_add(type='PLAIN_AXES', location=(0, 0, 0))
    root = bpy.context.active_object
    root.name = "BanditHero"

    # Parent all parts
    for part in all_parts:
        part.parent = root

    # Select all for easy export
    bpy.ops.object.select_all(action='DESELECT')
    root.select_set(True)
    for part in all_parts:
        part.select_set(True)
    bpy.context.view_layer.objects.active = root

    return root, all_parts


# ---------------------------------------------------------------------------
# 11. Export — optional FBX export
# ---------------------------------------------------------------------------

def export_fbx(filepath):
    """Export the scene to FBX."""
    # Ensure directory exists
    dirpath = os.path.dirname(filepath)
    if dirpath and not os.path.exists(dirpath):
        os.makedirs(dirpath, exist_ok=True)

    bpy.ops.export_scene.fbx(
        filepath=filepath,
        use_selection=True,
        apply_scale_options='FBX_SCALE_ALL',
        bake_space_transform=True,
        object_types={'MESH', 'EMPTY'},
        mesh_smooth_type='FACE',
        add_leaf_bones=False,
    )
    print(f"Exported FBX to: {filepath}")


# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

def main():
    clear_scene()
    root, parts = assemble_character()

    print("=" * 50)
    print("Bandit Hero character generated successfully!")
    print(f"  Parts: {len(parts)}")
    print(f"  Root: {root.name}")
    print("=" * 50)

    # Optional: auto-export FBX
    # Uncomment and adjust path as needed:
    # export_fbx(os.path.join(os.path.dirname(bpy.data.filepath), "Assets/_Game/Models/Hero.fbx"))

    # If running from project root, export relative to script location
    script_dir = os.path.dirname(os.path.realpath(__file__)) if "__file__" in dir() else ""
    if script_dir:
        export_path = os.path.join(script_dir, "Assets", "_Game", "Models", "Hero.fbx")
        export_fbx(export_path)
    else:
        print("Run with --python flag to auto-export, or export manually via File > Export > FBX")


if __name__ == "__main__":
    main()
