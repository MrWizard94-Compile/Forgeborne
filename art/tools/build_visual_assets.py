"""Original Forgeborne hard-surface candidates, authored with Blender 4.5.15.

No third-party geometry, textures or templates. Source .blend and exported FBX
are generated together. Game-space convention: X right, Y up, Z forward.
Candidates require inspection in the actual Unity consumer; no quality approval.
"""
from pathlib import Path
import bpy
import json
import math
import sys
from mathutils import Vector, Matrix, Euler

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "art" / "source"
EXPORT = ROOT / "ForgeborneUnity" / "Assets" / "Resources" / "Art" / "Models"
ROT = Matrix(((-1,0,0,0),(0,0,-1,0),(0,1,0,0),(0,0,0,1)))
MATERIALS = {}
GROUPS = {}


def point(p):
    return Vector((-p[0], -p[2], p[1]))


def game(p):
    return (-p.x, p.z, -p.y)


def material(name, color, metal, roughness, emission=0):
    value = bpy.data.materials.new(name)
    value.diffuse_color = (*color, 1)
    value.use_nodes = True
    shader = value.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value = (*color, 1)
    shader.inputs["Metallic"].default_value = metal
    shader.inputs["Roughness"].default_value = roughness
    if emission:
        shader.inputs["Emission Color"].default_value = (*color, 1)
        shader.inputs["Emission Strength"].default_value = emission
    MATERIALS[name] = value


def begin(name):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    MATERIALS.clear()
    GROUPS.clear()
    bpy.context.scene.unit_settings.system = "METRIC"
    material("Graphite", (0.055, 0.075, 0.085), 0.78, 0.38)
    material("Steel", (0.31, 0.39, 0.42), 0.88, 0.27)
    material("Copper", (0.43, 0.19, 0.075), 0.77, 0.34)
    material("Ceramic", (0.57, 0.63, 0.61), 0.12, 0.48)
    material("Rubber", (0.018, 0.025, 0.027), 0.0, 0.77)
    material("Cyan", (0.025, 0.62, 0.85), 0.25, 0.22, 3)
    material("Amber", (1.0, 0.19, 0.018), 0.2, 0.25, 3)
    material("Basalt", (0.12, 0.145, 0.16), 0.03, 0.82)
    material("Paint", (0.10, 0.22, 0.24), 0.55, 0.48)
    material("Warm", (1.0, 0.43, 0.15), 0.1, 0.3, 2)
    material("Glass", (0.12, 0.20, 0.24), 0.55, 0.18, 0.08)
    root = bpy.data.objects.new(name, None)
    bpy.context.collection.objects.link(root)
    GROUPS["Root"] = root
    for label,position in (("ArtRight",(0.25,0,0)),("ArtUp",(0,0.25,0)),("ArtForward",(0,0,0.25))):
        marker=bpy.data.objects.new(label,None)
        bpy.context.collection.objects.link(marker)
        marker.parent=root
        marker.location=point(position)


def group(name):
    if name not in GROUPS:
        value = bpy.data.objects.new(name, None)
        bpy.context.collection.objects.link(value)
        value.parent = GROUPS["Root"]
        GROUPS[name] = value
    return GROUPS[name]


def finish_part(obj, name, role, parent, bevel=0.003, rotation=(0, 0, 0)):
    obj.name = name
    obj.data.materials.append(MATERIALS[role])
    if rotation != (0, 0, 0):
        obj.rotation_mode = "QUATERNION"
        obj.rotation_quaternion = (ROT.to_3x3() @ Euler(tuple(math.radians(a) for a in rotation), "XYZ").to_matrix() @ ROT.to_3x3().inverted()).to_quaternion()
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if bevel:
        modifier = obj.modifiers.new("Authored edge radius", "BEVEL")
        modifier.width = bevel
        modifier.segments = 3
        modifier.harden_normals = True
        bpy.ops.object.modifier_apply(modifier=modifier.name)
        normals = obj.modifiers.new("Panel normals", "WEIGHTED_NORMAL")
        normals.keep_sharp = True
        bpy.ops.object.modifier_apply(modifier=normals.name)
    obj.parent = group(parent)
    return obj


def box(name, center, size, role="Graphite", parent="Frame", bevel=0.004, rotation=(0, 0, 0)):
    bpy.ops.mesh.primitive_cube_add(size=1, location=point(center))
    obj = bpy.context.object
    obj.dimensions = (size[0], size[2], size[1])
    return finish_part(obj, name, role, parent, bevel, rotation)


def rounded_volume(name,center,size,role="Rubber",parent="Frame",rotation=(0,0,0)):
    """Continuous curved pressure fabric/gauntlet volume, not a beveled box."""
    bpy.ops.mesh.primitive_uv_sphere_add(segments=24, ring_count=12, radius=1, location=point(center))
    obj=bpy.context.object
    obj.scale=(size[0]/2,size[2]/2,size[1]/2)
    for face in obj.data.polygons:
        face.use_smooth=True
    return finish_part(obj,name,role,parent,0,rotation)


def cylinder(name, center, radius, depth, role="Steel", parent="Frame", axis=(0, 0, 1), sides=32, bevel=0.0015):
    bpy.ops.mesh.primitive_cylinder_add(vertices=sides, radius=radius, depth=depth, location=point(center))
    obj = bpy.context.object
    obj.rotation_mode = "QUATERNION"
    obj.rotation_quaternion = Vector((0, 0, 1)).rotation_difference(point(axis).normalized())
    for polygon in obj.data.polygons:
        polygon.use_smooth = len(polygon.vertices) == 4
    return finish_part(obj, name, role, parent, bevel)


def ring(name, center, radius, tube, role="Copper", parent="Frame", axis=(0, 0, 1)):
    bpy.ops.mesh.primitive_torus_add(major_segments=40, minor_segments=8, major_radius=radius, minor_radius=tube, location=point(center))
    obj = bpy.context.object
    obj.rotation_mode = "QUATERNION"
    obj.rotation_quaternion = Vector((0, 0, 1)).rotation_difference(point(axis).normalized())
    for polygon in obj.data.polygons:
        polygon.use_smooth = True
    return finish_part(obj, name, role, parent, 0)


def profile(name, sections, outline, role="Graphite", parent="Frame", bevel=0.003):
    vertices = [tuple(point((x * width, y * height, z))) for z, width, height in sections for x, y in outline]
    count = len(outline)
    faces = [tuple(reversed(range(count))), tuple(range((len(sections)-1)*count, len(vertices)))]
    for row in range(len(sections)-1):
        for i in range(count):
            j = (i+1) % count
            faces.append((row*count+i, row*count+j, (row+1)*count+j, (row+1)*count+i))
    # Unity's game-space basis and Blender's authoring basis differ in handedness.
    faces=[tuple(reversed(face)) for face in faces]
    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    return finish_part(obj, name, role, parent, bevel)


def bolt(center, axis=(1, 0, 0), parent="Frame", radius=0.0045):
    cylinder("Captive hex fastener", center, radius, 0.0035, "Steel", parent, axis, sides=6, bevel=0.0005)


def glove(name, center, rotation):
    x, y, z = center
    rounded_volume(name+" palm", center, (0.080,0.060,0.10), "Rubber", name, rotation)
    rounded_volume(name+" knuckle shell", (x,y+0.026,z), (0.073,0.023,0.080), "Graphite", name, rotation)
    for i in range(4):
        dx = (i-1.5)*0.017
        rounded_volume(name+" finger", (x+dx,y-0.02,z+0.047), (0.017,0.040,0.037), "Rubber", name, rotation)
        rounded_volume(name+" fingertip", (x+dx,y-0.044,z+0.032), (0.016,0.032,0.038), "Rubber", name, rotation)
    rounded_volume(name+" thumb", (x+0.043,y-0.015,z-0.006), (0.028,0.043,0.055), "Rubber", name, rotation)


def rifle():
    begin("PulseCarbine")
    outline = [(math.cos(i*math.pi/12)*.5,math.sin(i*math.pi/12)*.5) for i in range(24)]
    profile("Forged pressure receiver", [(-0.24,0.115,0.135),(-0.17,0.15,0.15),(0.16,0.15,0.15),(0.27,0.10,0.105)], outline)
    profile("Shoulder stock", [(-0.47,0.09,0.11),(-0.40,0.105,0.13),(-0.24,0.07,0.085)], outline, "Rubber")
    box("Stock spine", (0,0.047,-0.30), (0.045,0.035,0.22), "Steel", bevel=0.006)
    cylinder("Barrel core", (0,0.016,0.41), 0.024,0.34)
    cylinder("Bored muzzle housing", (0,0.016,0.61), 0.038,0.09, "Graphite", sides=12)
    cylinder("Muzzle aperture", (0,0.016,0.658), 0.022,0.002, "Rubber", bevel=0)
    ring("Muzzle lip", (0,0.016,0.661), 0.027,0.004, "Steel")
    for z in (0.29,0.35,0.41,0.47,0.53):
        ring("Induction collar", (0,0.016,z),0.032,0.005)
    for side in (-1,1):
        box("Vented barrel shield", (side*0.041,-0.002,0.39),(0.014,0.064,0.25),"Graphite",bevel=0.005)
        for z in (0.30,0.34,0.38,0.42,0.46):
            box("Ceramic heat bridge",(side*0.049,0.004,z),(0.008,0.041,0.012),"Ceramic",bevel=0.002)
        box("Replaceable side armour",(side*0.079,0.005,-0.004),(0.013,0.080,0.24),"Graphite",bevel=0.006)
        box("Copper service panel",(side*0.087,0.005,-0.015),(0.005,0.047,0.15),"Copper",bevel=0.003)
        for z in (-0.09,0.065):
            for y in (-0.023,0.032):
                bolt((side*0.092,y,z),axis=(side,0,0))
    box("Grip",(0,-0.113,-0.13),(0.063,0.15,0.072),"Rubber",bevel=0.012,rotation=(16,0,0))
    for y in (-0.08,-0.105,-0.13,-0.155):
        box("Grip rib",(0,y,-0.169),(0.062,0.009,0.012),"Graphite",bevel=0.003)
    box("Trigger guard base",(0,-0.125,-0.032),(0.042,0.017,0.11),"Steel",bevel=0.005)
    box("Trigger guard front",(0,-0.094,0.018),(0.042,0.074,0.015),"Steel",bevel=0.005)
    box("Capacitor magazine",(0,-0.11,0.105),(0.083,0.16,0.12),"Graphite","Magazine",0.010,(-7,0,0))
    for side in (-1,1):
        for z in (0.077,0.105,0.133):
            box("Magazine charge gauge",(side*0.044,-0.10,z),(0.005,0.071,0.011),"Cyan","Magazine",0.002)
    box("Magazine shoe",(0,-0.19,0.114),(0.098,0.023,0.14),"Copper","Magazine",0.005)
    box("Sight rail",(0,0.084,-0.002),(0.054,0.025,0.28),"Steel",bevel=0.004)
    for z in (-0.12,-0.08,-0.04,0,0.04,0.08):
        box("Machined rail notch",(0,0.101,z),(0.059,0.009,0.014),"Graphite",bevel=0.0015)
    cylinder("Readout pressure housing",(0,0.117,0.003),0.034,0.115,"Graphite",sides=32)
    box("Readout glass",(0,0.139,-0.025),(0.050,0.003,0.057),"Cyan",bevel=0.002)
    ring("Reflex sight rim",(0,0.137,0.13),0.028,0.005,"Steel")
    box("Sight support",(0,0.108,0.13),(0.022,0.045,0.018),"Graphite",bevel=0.004)
    cylinder("Power selector",(0.093,0.026,-0.128),0.013,0.012,"Copper",axis=(1,0,0),sides=12)
    box("Charging handle",(-0.09,0.047,-0.08),(0.037,0.023,0.055),"Steel","ChargingHandle",0.005)
    glove("RightHand",(0.012,-0.108,-0.117),(10,0,0))
    glove("LeftHand",(-0.025,-0.069,0.285),(0,0,-12))
    rounded_volume("Right arm under-suit",(0.115,-0.235,-0.30),(0.095,0.11,0.35),"Rubber","RightHand",(-22,-24,0))
    rounded_volume("Right curved forearm shell",(0.12,-0.20,-0.30),(0.084,0.055,0.25),"Graphite","RightHand",(-22,-24,0))
    rounded_volume("Left arm under-suit",(-0.17,-0.18,0.09),(0.095,0.11,0.34),"Rubber","LeftHand",(-24,29,0))
    rounded_volume("Left curved forearm shell",(-0.17,-0.147,0.09),(0.084,0.055,0.24),"Graphite","LeftHand",(-24,29,0))
    socket=bpy.data.objects.new("MuzzleSocket",None)
    bpy.context.collection.objects.link(socket)
    socket.parent=GROUPS["Root"]
    socket.location=point((0,0.016,0.665))
    export("PulseCarbine")


def warden():
    begin("EmberWarden")
    shape=[(-0.50,-0.20),(-0.30,-0.5),(0.30,-0.5),(0.50,-0.2),(0.50,0.20),(0.30,0.50),(-0.30,0.50),(-0.50,0.20)]
    profile("Warden pressure hull",[(-0.27,0.40,0.25),(-0.16,0.56,0.32),(0.19,0.52,0.31),(0.29,0.34,0.22)],shape)
    box("Dorsal armour",(0,0.18,-0.04),(0.34,0.048,0.31),"Steel",bevel=0.014)
    cylinder("Sensor mount",(0,0.017,0.285),0.093,0.04,"Copper",sides=12)
    cylinder("Sensor lens",(0,0.017,0.310),0.064,0.008,"Amber",sides=24)
    ring("Sensor bezel",(0,0.017,0.315),0.069,0.009,"Steel")
    for side in (-1,1):
        box("Lateral arm",(side*0.29,-0.04,-0.04),(0.16,0.07,0.095),"Steel",bevel=0.008)
        cylinder("Thruster bell",(side*0.34,-0.13,-0.04),0.071,0.18,"Graphite",axis=(0,1,0),sides=20,bevel=0.006)
        ring("Thruster rim",(side*0.34,-0.225,-0.04),0.059,0.008,"Copper",axis=(0,1,0))
        cylinder("Thruster core",(side*0.34,-0.229,-0.04),0.041,0.006,"Amber",axis=(0,1,0),bevel=0)
        box("Side service hatch",(side*0.275,0.02,-0.05),(0.018,0.12,0.17),"Copper",bevel=0.007)
        for z in (-0.11,0.01):
            bolt((side*0.29,0.05,z),axis=(side,0,0),radius=0.009)
    box("Aft stabiliser",(0,0.19,-0.23),(0.035,0.19,0.12),"Graphite",bevel=0.007,rotation=(-15,0,0))
    for x in (-0.11,0,0.11):
        box("Rear cooling vent",(x,0,-0.276),(0.060,0.071,0.014),"Rubber",bevel=0.004)
    export("EmberWarden",radius_limit=0.49)


def relay():
    begin("AetherRelay")
    cylinder("Relay plinth",(0,-0.37,0),0.39,0.12,"Graphite",axis=(0,1,0),sides=12,bevel=0.008)
    cylinder("Induction chamber",(0,0,0),0.14,0.65,"Cyan",axis=(0,1,0),sides=8,bevel=0.005)
    for y in (-0.24,0,0.24):
        ring("Copper field cage",(0,y,0),0.26,0.025,"Copper",axis=(0,1,0))
    for i in range(4):
        a=math.pi/4+i*math.pi/2
        x,z=math.cos(a)*0.36,math.sin(a)*0.36
        box("Armoured pillar",(x,0,z),(0.065,0.75,0.065),"Steel",bevel=0.012)
        box("Pillar indicator",(x,0.1,z+0.035),(0.023,0.24,0.008),"Cyan",bevel=0.003)
    cylinder("Relay crown",(0,0.37,0),0.39,0.09,"Graphite",axis=(0,1,0),sides=12,bevel=0.008)
    ring("Crown seam",(0,0.414,0),0.31,0.012,"Copper",axis=(0,1,0))
    export("AetherRelay",radius_limit=0.49)


def suit():
    """Pressure suit at the canonical 1.8 m body scale, designed for the orbital view."""
    begin("ForgeSuit")
    box("Pressure torso",(0,1.22,0),(0.40,0.43,0.27),"Rubber",bevel=0.07)
    box("Sternal armour",(0,1.28,0.13),(0.34,0.28,0.075),"Paint",bevel=0.045)
    box("Collar plate",(0,1.46,0.025),(0.35,0.06,0.26),"Steel",bevel=0.025)
    box("Abdominal armour",(0,1.02,0.09),(0.29,0.14,0.12),"Graphite",bevel=0.035)
    for y in (1.01,1.055):
        box("Flexible waist rib",(0,y,0.155),(0.24,0.016,0.02),"Steel",bevel=0.004)
    box("Hip pressure housing",(0,0.88,0),(0.35,0.20,0.26),"Paint",bevel=0.045)
    box("Belt buckle",(0,0.93,0.145),(0.11,0.045,0.02),"Copper",bevel=0.006)
    box("Chest power seam",(0,1.30,0.171),(0.025,0.15,0.008),"Cyan",bevel=0.004)
    for x in (-0.115,0.115):
        cylinder("Front pressure coupling",(x,1.19,0.175),0.028,0.022,"Copper",sides=16,bevel=0.003)
    box("Back power pack",(0,1.28,-0.16),(0.29,0.29,0.10),"Graphite",bevel=0.035)
    for x in (-0.09,0.09):
        cylinder("Back induction cell",(x,1.29,-0.205),0.034,0.25,"Copper",axis=(0,1,0),sides=16,bevel=0.004)
        ring("Pack collar",(x,1.38,-0.205),0.035,0.006,"Steel",axis=(0,1,0))
    box("Pack beacon",(0,1.43,-0.21),(0.06,0.035,0.016),"Cyan",bevel=0.005)
    cylinder("Helmet seal",(0,1.51,0),0.105,0.055,"Rubber",axis=(0,1,0),sides=24,bevel=0.006)
    box("Helmet shell",(0,1.65,0),(0.23,0.28,0.23),"Paint",bevel=0.065)
    box("Visor frame",(0,1.68,0.103),(0.22,0.12,0.05),"Steel",bevel=0.025)
    box("Smoked visor",(0,1.683,0.130),(0.185,0.087,0.018),"Glass",bevel=0.022)
    box("Visor brow signal",(0,1.73,0.131),(0.155,0.012,0.009),"Cyan",bevel=0.003)
    box("Respirator",(0,1.568,0.11),(0.125,0.065,0.065),"Graphite",bevel=0.018)
    for x in (-0.044,0,0.044):
        box("Breather slot",(x,1.565,0.146),(0.012,0.033,0.006),"Steel",bevel=0.002)
    for side in (-1,1):
        x=side*0.105
        leg="LeftLeg" if side<0 else "RightLeg"
        cylinder("Thigh pressure sleeve",(x,0.66,0),0.081,0.32,"Rubber",leg,axis=(0,1,0),sides=20,bevel=0.012)
        box("Thigh plate",(x,0.69,0.065),(0.145,0.28,0.07),"Paint",leg,0.025)
        box("Knee guard",(x,0.47,0.088),(0.15,0.13,0.085),"Steel",leg,0.028)
        cylinder("Shin sleeve",(x,0.28,0),0.069,0.30,"Rubber",leg,axis=(0,1,0),sides=20,bevel=0.012)
        box("Shin plate",(x,0.29,0.065),(0.12,0.25,0.055),"Paint",leg,0.025)
        box("Boot",(x,0.075,0.04),(0.155,0.15,0.27),"Graphite",leg,0.025)
        box("Boot sole",(x,0.018,0.04),(0.16,0.036,0.28),"Rubber",leg,0.008)
        box("Toe cap",(x,0.082,0.16),(0.145,0.08,0.035),"Steel",leg,0.018)
        arm="LeftArm" if side<0 else "RightArm"
        x=side*0.247
        box("Shoulder guard",(x,1.37,0),(0.10,0.17,0.22),"Paint",arm,0.035)
        cylinder("Upper arm sleeve",(x,1.21,0.015),0.044,0.21,"Rubber",arm,axis=(0,1,0),sides=16,bevel=0.008)
        box("Elbow cap",(x,1.075,0.025),(0.09,0.10,0.105),"Steel",arm,0.022)
        box("Forearm gauntlet",(x,0.97,0.065),(0.095,0.18,0.12),"Graphite",arm,0.023)
        box("Glove",(x,0.835,0.07),(0.08,0.10,0.10),"Rubber",arm,0.025)
        box("Wrist power indicator",(x,1.015,0.127),(0.04,0.045,0.008),"Cyan",arm,0.004)
    export("ForgeSuit",radius_limit=0.43)


def foundry():
    """Original off-yard architectural context; local origin is yard centre at ground height.

    Facades stand outside the 32 m playable footprint. They are visual context,
    not alternate colliders. The playable yard must retain canonical boundaries.
    """
    begin("RelayFoundry")
    for side in (-1,1):
        x=side*19.5
        height=23 if side<0 else 17
        box("Foundry foundation",(x,-1,0),(7,4,43),"Basalt",bevel=0.12)
        box("Industrial hall",(x,height/2,0),(7,height,40),"Graphite",bevel=0.14)
        box("Upper cornice",(x,height-0.4,0),(7.6,0.8,40.6),"Steel",bevel=0.10)
        for z in (-16,-8,0,8,16):
            box("Facade pier",(side*16.2,height/2,z),(0.4,height,0.75),"Paint",bevel=0.06)
            box("Pier foot",(side*16.1,0.6,z),(0.7,1.2,1.15),"Basalt",bevel=0.09)
        for z in (-12,-4,4,12):
            box("Recessed service bay",(side*16.05,3.2,z),(0.09,5.0,5.1),"Rubber",bevel=0.03)
            for dz in (-2.4,2.4):
                box("Service bay jamb",(side*15.95,3.4,z+dz),(0.12,5.6,0.20),"Copper",bevel=0.025)
            box("Service bay lintel",(side*15.95,6.1,z),(0.12,0.23,5.0),"Copper",bevel=0.035)
            box("Inspection window",(side*15.99,8.9,z),(0.06,1.4,4.5),"Glass",bevel=0.04)
            box("Window rain hood",(side*15.77,9.66,z),(0.60,0.15,4.85),"Graphite",bevel=0.025)
            box("Inspection sill",(side*15.8,8.15,z),(0.5,0.16,4.85),"Steel",bevel=0.025)
            for dz in (-1.5,0,1.5):
                box("Window mullion",(side*15.91,8.9,z+dz),(0.15,1.5,0.11),"Steel",bevel=0.015)
            for y in (1.5,2.2,2.9,3.6,4.3):
                box("Door armour seam",(side*15.92,y,z),(0.10,0.06,4.3),"Steel",bevel=0.012)
        for z in (-12,4,12):
            # Lamps are above the walk/build interaction band. A named empty
            # supplies the practical light position to the Unity scene author.
            box("Lamp bracket",(side*15.8,3.7,z),(0.48,0.16,0.22),"Steel",bevel=0.03)
            box("Warm work lamp",(side*15.6,3.55,z),(0.26,0.19,0.36),"Warm",bevel=0.04)
            marker=bpy.data.objects.new("Lamp_"+str(len([o for o in bpy.data.objects if o.name.startswith('Lamp_')])),None)
            bpy.context.collection.objects.link(marker)
            marker.parent=GROUPS["Root"]
            marker.location=point((side*15.25,3.4,z))
        for y in (11.5,12.2):
            cylinder("Longitudinal utility pipe",(side*15.9,y,0),0.18,41,"Copper",sides=24,bevel=0.02)
            for z in (-16,-8,0,8,16):
                ring("Pipe expansion collar",(side*15.9,y,z),0.21,0.025,"Steel")
        box("Roof service crown",(x,height+0.8,-5),(4.8,2.1,18),"Paint",bevel=0.1)
        for z in (-12,-5,3,10):
            cylinder("Exhaust stack",(x,height+4,z),0.65,7,"Graphite",axis=(0,1,0),sides=24,bevel=0.08)
            ring("Stack mouth",(x,height+7.55,z),0.67,0.065,"Steel",axis=(0,1,0))
    # Rear exchange is asymmetric: the relay side carries the field aperture;
    # the opposite side carries transformer housings and a tall service spire.
    box("Rear exchange foundation",(0,-0.8,20),(46,3.6,8),"Basalt",bevel=0.15)
    box("Rear exchange facade",(0,8,21),(45,16,7),"Paint",bevel=0.18)
    for x in (-18,-12,-6,0,6,12,18):
        box("Rear facade rib",(x,7.8,17.35),(0.55,16,0.45),"Steel",bevel=0.06)
    for x in (-15,-9,-3,3,9,15):
        for y in (2,6,10,14):
            box("Exchange armour panel",(x,y,17.43),(5.3,3.7,0.12),"Graphite",bevel=0.06)
            box("Panel service seam",(x,y-1.65,17.32),(4.8,0.055,0.08),"Steel",bevel=0.008)
    cylinder("Field aperture backing",(11,8.5,17.4),4.4,0.35,"Graphite",sides=48,bevel=0.07)
    for radius,tube,role in ((4.25,0.18,"Copper"),(3.7,0.13,"Steel"),(3.35,0.065,"Cyan")):
        ring("Exchange field ring",(11,8.5,17.12),radius,tube,role)
    for i in range(12):
        angle=i*math.tau/12
        x,y=11+math.cos(angle)*3.98,8.5+math.sin(angle)*3.98
        box("Field locking segment",(x,y,17.03),(0.42,0.42,0.24),"Steel",bevel=0.045)
    for x in (-13,-7):
        box("Transformer housing",(x,4.1,16.8),(4.2,6.4,1.1),"Graphite",bevel=0.16)
        for y in (1.8,2.8,3.8,4.8,5.8):
            box("Cooling fin bank",(x,y,16.12),(4.5,0.30,0.50),"Copper",bevel=0.04)
    box("Service spire",(-16,16,23),(5,32,7),"Graphite",bevel=0.2)
    for y in (8,16,24):
        box("Spire band",(-16,y,23),(5.5,0.65,7.5),"Steel",bevel=0.08)
    box("Warning beacon",(-16,32.6,23),(0.55,0.9,0.55),"Amber",bevel=0.10)
    # Layered distant massing establishes a city/plant beyond the local yard.
    for x,z,w,h in ((-36,42,11,39),(-22,58,10,54),(5,52,13,43),(27,48,10,34),(39,64,14,61)):
        box("Distant production hall",(x,h/2,z),(w,h,12),"Basalt",bevel=0.25)
        box("Distant crown",(x,h,z),(w+1,1.4,13),"Graphite",bevel=0.15)
        for y in range(8,int(h)-3,6):
            box("Distant narrow glazing",(x,y,z-6.06),(w*0.68,0.50,0.12),"Glass",bevel=0.05)
    export("RelayFoundry")


def export(name,radius_limit=None):
    # Merge by moving assembly, retaining material slots and named sockets.
    for key,parent in list(GROUPS.items()):
        if key=="Root": continue
        meshes=[o for o in parent.children if o.type=="MESH"]
        if not meshes: continue
        bpy.ops.object.select_all(action="DESELECT")
        for obj in meshes: obj.select_set(True)
        bpy.context.view_layer.objects.active=meshes[0]
        if len(meshes)>1:
            bpy.ops.object.join()
        bpy.context.object.name=key+"Mesh"
    vertices=[game(obj.matrix_world @ v.co) for obj in bpy.context.scene.objects if obj.type=="MESH" for v in obj.data.vertices]
    low=[min(p[i] for p in vertices) for i in range(3)]
    high=[max(p[i] for p in vertices) for i in range(3)]
    radius=max(math.hypot(p[0],p[2]) for p in vertices)
    if radius_limit is not None: assert radius<=radius_limit,(name,radius)
    SOURCE.mkdir(parents=True,exist_ok=True)
    EXPORT.mkdir(parents=True,exist_ok=True)
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/(name+".blend")))
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.export_scene.fbx(filepath=str(EXPORT/(name+".fbx")),use_selection=True,
        object_types={"MESH","EMPTY"},axis_forward="-Z",axis_up="Y",apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_ALL",bake_space_transform=False,
        use_mesh_modifiers=True,add_leaf_bones=False,bake_anim=False)
    manifest={"name":name,"authoring":"Original Forgeborne geometry; no imported assets",
        "tool":bpy.app.version_string,"source":"art/tools/build_visual_assets.py",
        "gameBoundsMin":low,"gameBoundsMax":high,"horizontalRadius":radius,
        "vertices":len(vertices),"triangles":sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in bpy.context.scene.objects if o.type=="MESH"),
        "materials":list(MATERIALS),"status":"Authored candidate; Unity import/render and director review required"}
    (SOURCE/(name+".json")).write_text(json.dumps(manifest,indent=2)+"\n",encoding="utf-8")
    print("FORGEBORNE_ASSET",json.dumps(manifest))


assert bpy.app.version[:3]==(4,5,15),bpy.app.version
builders={"PulseCarbine":rifle,"EmberWarden":warden,"AetherRelay":relay,"ForgeSuit":suit,"RelayFoundry":foundry}
selected=sys.argv[sys.argv.index("--")+1:] if "--" in sys.argv else list(builders)
for asset in selected:
    if asset not in builders:raise ValueError("Unknown original asset "+asset)
    builders[asset]()
