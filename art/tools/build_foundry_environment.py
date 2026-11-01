"""Original playable Cinder Exchange geometry, Blender 4.5.15 LTS.

No imported geometry/textures. The .blend and Core's embedded mesh payload are
exported from the same closed components. Game space is X right/Y up/Z forward.
Run Blender --background --python art/tools/build_foundry_environment.py.
"""
from pathlib import Path
import bpy
import json
import math
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[2]
DEST = ROOT / "src/Forgeborne.Core/Content/CinderExchangeV1.json"
PARTS = []
KIND, OWNER, TYPE = "Static", None, 6
PALETTE = {
    "Graphite": ((.075, .095, .105), .72, .45),
    "Steel": ((.31, .39, .42), .88, .35),
    "Copper": ((.43, .19, .075), .77, .4),
    "Ceramic": ((.57, .63, .61), .12, .48),
    "Rubber": ((.018, .025, .027), 0, .77),
    "Cyan": ((.025, .62, .85), .25, .22),
    "Amber": ((1, .19, .018), .2, .25),
    "Basalt": ((.12, .145, .16), .03, .82),
    "Paint": ((.10, .22, .24), .55, .48),
    "Warm": ((1, .43, .15), .1, .3),
    "Glass": ((.12, .20, .24), .55, .18),
    "Timber": ((.32, .16, .065), 0, .86),
}


def point(p):
    return Vector((p[0], -p[2], p[1]))


def game(p):
    return [round(p.x, 6), round(p.z, 6), round(-p.y, 6)]


def finish(obj, name, role, bevel=0):
    obj.name = name
    obj.data.materials.append(bpy.data.materials[role])
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if bevel:
        modifier = obj.modifiers.new("Machined edge radius", "BEVEL")
        modifier.width = bevel
        modifier.segments = 2
        bpy.ops.object.modifier_apply(modifier=modifier.name)
    obj["SurfaceKind"] = KIND
    obj["LedgerType"] = TYPE
    if OWNER is not None:
        obj["LedgerCell"] = OWNER
    PARTS.append(obj)
    return obj


def box(name, centre, size, role="Graphite", bevel=.025):
    bpy.ops.mesh.primitive_cube_add(size=1, location=point(centre))
    obj = bpy.context.object
    obj.dimensions = (size[0], size[2], size[1])
    return finish(obj, name, role, min(bevel, min(size)/4))


def cylinder(name, centre, radius, length, role="Steel", axis=(0, 1, 0), sides=24):
    bpy.ops.mesh.primitive_cylinder_add(vertices=sides, radius=radius, depth=length, location=point(centre))
    obj = bpy.context.object
    obj.rotation_mode = "QUATERNION"
    obj.rotation_quaternion = Vector((0, 0, 1)).rotation_difference(point(axis).normalized())
    for face in obj.data.polygons:
        face.use_smooth = len(face.vertices) == 4
    return finish(obj, name, role, min(.018, radius/8, length/8))


def ring(name, centre, radius, tube, role="Copper", axis=(0, 0, 1)):
    bpy.ops.mesh.primitive_torus_add(major_segments=64, minor_segments=12,
                                   major_radius=radius, minor_radius=tube, location=point(centre))
    obj=bpy.context.object
    obj.rotation_mode="QUATERNION"
    obj.rotation_quaternion=Vector((0, 0, 1)).rotation_difference(point(axis).normalized())
    for face in obj.data.polygons:
        face.use_smooth=True
    return finish(obj,name,role)


def vessel(name, centre, radius, length, role="Paint"):
    vertices=[]
    sections=((-0.5,.14),(-.47,.53),(-.40,.85),(-.32,1),(.32,1),(.40,.85),(.47,.53),(.5,.14))
    for height, scale in sections:
        for i in range(40):
            angle=2*math.pi*i/40
            vertices.append(tuple(point((centre[0]+math.cos(angle)*radius*scale,
                                         centre[1]+height*length,centre[2]+math.sin(angle)*radius*scale))))
    faces=[tuple(reversed(range(40))),tuple(range(280,320))]
    for row in range(7):
        for i in range(40):
            a=row*40+i;b=row*40+(i+1)%40
            faces.append((a,b,b+40,a+40))
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(vertices,[],[tuple(reversed(f)) for f in faces]);mesh.update()
    obj=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(obj)
    for face in mesh.polygons:
        face.use_smooth=len(face.vertices)==4
    return finish(obj,name,role)


def arched_panel(name, centre, width, straight, role="Paint"):
    outline=[(-width/2,0),(width/2,0),(width/2,straight)]
    outline += [(math.cos(i*math.pi/32)*width/2,straight+math.sin(i*math.pi/32)*width*.34) for i in range(1,33)]
    count=len(outline)
    vertices=[tuple(point((centre[0]+x,centre[1]+y,centre[2]+depth))) for depth in (-.10,.10) for x,y in outline]
    faces=[tuple(reversed(range(count))),tuple(range(count,2*count))]
    for i in range(count):faces.append((i,(i+1)%count,(i+1)%count+count,i+count))
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(vertices,[],faces);mesh.update()
    obj=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(obj)
    return finish(obj,name,role,.025)


def arch(name, z, role="Paint", rx=14.25, ry=7.4, width=.5, thickness=.24):
    # Closed elliptical structural rib, including both end caps. Its aperture
    # remains real empty space; there is no filled collider behind the arch.
    vertices = []
    for i in range(41):
        angle = math.pi*i/40
        for radial, depth in ((-1, -1), (1, -1), (1, 1), (-1, 1)):
            vertices.append(tuple(point((16+(rx+radial*thickness/2)*math.cos(angle),
                                         4+(ry+radial*thickness/2)*math.sin(angle),
                                         z+depth*width/2))))
    faces = [(3, 2, 1, 0), tuple(range(160, 164))]
    for i in range(40):
        for j in range(4):
            a, b = 4*i+j, 4*i+(j+1)%4
            faces.append((a, b, b+4, a+4))
    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata(vertices, [], [tuple(reversed(face)) for face in faces])
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    return finish(obj, name, role)


def rock(name, centre, size, role, seed):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=3, radius=1, location=point(centre))
    obj = bpy.context.object
    for i, vertex in enumerate(obj.data.vertices):
        # Faceted fracture planes, asymmetric deterministic silhouette. This is
        # a loose mineral fragment, not a displaced one-metre material cube.
        p=vertex.co
        factor=.91+.045*math.sin(p.x*5.7+seed)*math.cos(p.y*4.9-seed)+.035*math.sin(p.z*8.1+seed*2.7)
        vertex.co *= factor
        vertex.co.z=min(vertex.co.z,.74+.18*vertex.co.x)
    for face in obj.data.polygons:
        face.use_smooth=False
    obj.scale = (size[0]/2, size[2]/2, size[1]/2)
    return finish(obj, name, role)


def environment():
    # Flush steel runners replace foundation strips. They are not overlapping
    # raised decals: shared edges are exactly coplanar, without z-fighting or
    # a hidden lip that would block a machine's service envelope.
    strips = ((0, 9.1575, "Basalt"), (9.1575, 9.2425, "Steel"),
              (9.2425, 22.7575, "Basalt"), (22.7575, 22.8425, "Steel"),
              (22.8425, 32, "Basalt"))
    for lo, hi, role in strips:
        box("Continuous cast foundation" if role == "Basalt" else "Flush service runner",
            ((lo+hi)/2, 3.65, 16), (hi-lo, .7, 32), role, 0)
    # The damaged exchange hall has a service route down each side and a broad
    # central work floor. Floor inlays are long structural joints, never cells.
    for z in (2.6, 30.6):
        box("End bulkhead", (16, 6.65, z), (31.8, 5.3, .48), "Paint", .06)
        box("Bulkhead lower impact rail", (16, 4.7, z-.3), (30.5, .3, .22), "Graphite")
    for x in (1.4, 30.6):
        box("Retaining wall", (x, 6.6, 16), (.55, 5.2, 28), "Basalt", .06)
        for z in (6, 13.5, 21, 28):
            box("Pier shoe", (x, 4.55, z), (1.1, 1.1, 1.3), "Graphite", .08)
            cylinder("Pier pressure column", (x, 6.6, z), .30, 4.6, "Paint")
            cylinder("Pier capital", (x, 8.85, z), .46, .20, "Copper")
    for z in (6, 13.5, 21, 28):
        arch("Exchange elliptical load rib", z)
        arch("Rib inset copper spine", z-.29, "Copper", 14.27, 7.42, .075, .055)
        for x in (5.8, 26.2):
            # Fixtures are actual closed geometry with useful silhouette.
            box("Hanging luminaire hood", (x, 9.35, z), (1.35, .22, .5), "Graphite")
            box("Hanging warm diffuser", (x, 9.22, z), (1.15, .045, .36), "Warm", .01)
    # Service bays are 3.5 m high: doors, pressure vessels and workshop benches
    # establish human scale underneath the larger exchange ribs.
    for x, sign in ((3.2, 1), (28.8, -1)):
        for z in (10, 18, 26):
            box("Bay lower panel", (x, 5.0, z), (.24, 1.85, 4.5), "Paint")
            box("Bay upper lintel", (x, 7.35, z), (.46, .48, 4.8), "Graphite")
            for dz in (-2.2, 2.2):
                box("Bay jamb", (x, 5.9, z+dz), (.42, 3.3, .22), "Steel")
            box("Bay cyan wayfinding lamp", (x+sign*.28, 7.02, z), (.055, .08, 1.7), "Cyan", .01)
    # Left workshop: failed separator, pipes with unions, maintenance workbench.
    for z, radius in ((15.2, .65), (17, .5)):
        vessel("Separator pressure vessel", (5.2, 5.6, z), radius, 2.7, "Paint")
        for y in (4.5, 6.7):
            cylinder("Vessel reinforcing collar", (5.2, y, z), radius+.055, .14, "Steel")
        cylinder("Vessel exposed feed", (5.2, 7.3, z), .13, 1.1, "Copper")
    for y in (5.3, 6.0):
        cylinder("Service manifold", (2.15, y, 17.5), .10, 17, "Copper", (0, 0, 1))
        for z in (11, 17, 23):
            cylinder("Pipe flange", (2.15, y, z), .16, .08, "Steel", (0, 0, 1))
    box("Maintenance bench", (6.5, 5.0, 10.6), (3.6, .18, 1.15), "Steel")
    for x in (5, 8):
        box("Bench leg", (x, 4.48, 10.6), (.12, .88, .8), "Graphite")
    # Right bay: low cable spool cover and a damaged exchanger, leaving routes
    # on both sides. No invisible cell corner fills these curved forms.
    for z in (15.0, 18.5):
        cylinder("Horizontal cable drum", (25.2, 4.65, z), .59, 1.7, "Graphite", (1, 0, 0))
        for x in (24.35, 26.05):
            cylinder("Drum cheek", (x, 4.65, z), .70, .10, "Copper", (1, 0, 0))
    box("Exchanger low body", (21.5, 4.62, 22.5), (3.7, 1.24, 1.3), "Paint", .16)
    for x in (20.2, 20.8, 21.4, 22, 22.6):
        box("Exchanger cooling vane", (x, 5.34, 22.5), (.10, .22, 1.0), "Steel", .02)
    # Core approach is a deliberate shallow podium, not an abrupt metre step.
    cylinder("Relay approach dais", (28.5, 4.12, 28.3), 1.55, .24, "Graphite", sides=40)
    # Collapsed roof panel supplies cover and a local explanation for exposed
    # mineral. It is modeled as a tilted solid, not a floating decoration.
    panel = box("Collapsed roof plate", (13.0, 4.60, 19.0), (3.8, .18, 1.9), "Paint", .05)
    panel.rotation_euler.x = math.radians(25)
    for x in (12.1, 13.8):
        cylinder("Broken support strut", (x, 4.55, 19.6), .10, 1.1, "Steel")
    # Rear elevation: original arched pressure doors, a large exchanger rotor,
    # cast housings and functional service detail replace the blank box facade.
    for x in (6.6,25.4):
        arched_panel("Arched pressure-bay door",(x,4.02,30.24),6.6,2.8,"Graphite")
        arched_panel("Door raised inner casting",(x,4.12,30.09),5.9,2.5,"Paint")
        for offset in (-2.2,-1.1,0,1.1,2.2):
            cylinder("Door vertical reinforcement",(x+offset,5.7,29.94),.065,2.9,"Steel")
        for side in (-1,1):
            cylinder("Door hydraulic column",(x+side*3.0,5.5,29.9),.13,2.7,"Copper")
            for y in (4.4,6.6):
                cylinder("Hydraulic gland",(x+side*3.0,y,29.9),.20,.20,"Steel")
        cylinder("Door pressure lock",(x,5.6,29.85),.28,.10,"Copper",(0,0,1),32)
        ring("Lock handwheel",(x,5.6,29.72),.34,.04,"Steel")
        for offset in (-.22,.22):
            cylinder("Lock wheel spokes",(x+offset,5.6,29.72),.025,.44,"Steel",(1,0,0),12)
        for offset in (-2.6,2.6):
            for y in (4.6,5.4,6.2):
                cylinder("Captive door fastener",(x+offset,y,29.94),.045,.04,"Copper",(0,0,1),8)
    cylinder("Exchange turbine recess",(16,7.2,30.22),2.45,.30,"Graphite",(0,0,1),64)
    ring("Turbine bronze outer housing",(16,7.2,29.98),2.18,.16)
    ring("Turbine inset ceramic throat",(16,7.2,29.89),1.91,.09,"Ceramic")
    cylinder("Turbine armature",(16,7.2,29.92),.69,.45,"Paint",(0,0,1),48)
    ring("Armature electrical seam",(16,7.2,29.66),.55,.055,"Cyan")
    for i in range(18):
        angle=i*math.pi/9
        x,y=16+math.cos(angle)*1.36,7.2+math.sin(angle)*1.36
        blade=box("Curved exchanger guide vane",(x,y,30.0),(.12,1.0,.32),"Steel",.05)
        blade.rotation_euler.y=angle+math.pi/2
    for x in (12.7,19.3):
        vessel("Turbine surge accumulator",(x,6.0,29.7),.35,3.7,"Copper")
        cylinder("Accumulator upper feed",(x,8.2,29.7),.11,.8,"Steel")
    for x in (9.8,22.2):
        box("Cast machinery buttress",(x,6.1,30.0),(.55,4.2,.85),"Paint",.18)
        for y in (4.6,5.5,6.4,7.3):
            cylinder("Buttress captive cap",(x,y,29.53),.075,.06,"Copper",(0,0,1),8)


def deposits():
    global KIND, OWNER, TYPE
    KIND = "Deposit"
    # Each original visible fragment binds to one canonical resource. Gathering
    # removes that exact component and updates the existing inventory ledger.
    seams=[
        ((11, 4, 11), 2), ((12, 4, 11), 2), ((11, 4, 12), 2),
        ((12, 4, 12), 1), ((10, 4, 12), 1), ((13, 4, 12), 1),
        ((20, 4, 13), 1), ((21, 4, 13), 2), ((20, 4, 14), 1),
        ((21, 4, 14), 1), ((19, 4, 14), 1), ((22, 4, 14), 2),
    ]
    # Finite first-encounter stock must actually fund the gather/build loop:
    # workstation + mill + dynamo + defensive plate, without debug inventory.
    seams += [((x,4,z),1) for x in range(7,11) for z in range(20,23)]
    seams += [((x,4,z),1) for x in range(17,20) for z in (18,19)]
    seams += [((x,4,z),2) for x in range(20,23) for z in (10,11)]
    for i, (cell, material) in enumerate(seams):
        OWNER, TYPE = cell, material
        x, y, z = cell
        rock("Fractured ember seam" if material == 2 else "Fractured host rock",
             (x+.5, y+.32, z+.5), (.92, .61, .88), "Basalt", i)
        if material == 2:
            rock("Exposed ember crystal", (x+.48, y+.48, z+.53), (.26, .27, .23), "Amber", i+41)
    for i, cell in enumerate(((7, 4, 8), (8, 4, 8), (7, 4, 9), (8, 4, 9), (9, 4, 9), (6, 4, 9))):
        OWNER, TYPE = cell, 3
        x, y, z = cell
        for offset in (-.20, 0, .20):
            obj = box("Recovered timber packing slat", (x+.5, y+.10, z+.5+offset), (.87, .16, .14), "Timber", .012)
            obj.rotation_euler.z = math.radians((i-2)*4)
    KIND, OWNER, TYPE = "Static", None, 6


def templates():
    global KIND, OWNER, TYPE
    KIND, OWNER = "Template", None
    for TYPE in (4, 5, 7, 8, 9, 10, 11, 12, 13):
        if TYPE == 4:
            box("Defensive plate", (.5, .46, .5), (.90, .92, .22), "Paint", .06)
            for x in (.13, .87):
                box("Wall foot", (x, .07, .5), (.16, .14, .70), "Graphite")
            box("Wall inspection rail", (.5, .69, .37), (.62, .06, .04), "Steel", .008)
        elif TYPE == 10:
            cylinder("Armoured cable", (.5, .10, .5), .065, .90, "Copper", (0, 0, 1), 12)
            for z in (.10, .90):
                cylinder("Cable coupling", (.5, .10, z), .09, .12, "Graphite", (0, 0, 1), 12)
        elif TYPE == 5:
            for x in (.16, .84):
                box("Console pedestal", (x, .34, .5), (.15, .68, .68), "Graphite")
            box("Console worktop", (.5, .73, .5), (.92, .14, .78), "Paint")
            box("Regen control surface", (.5, .82, .60), (.65, .04, .30), "Cyan", .015)
            cylinder("Console power socket", (.5, .35, .13), .10, .06, "Copper", (0, 0, 1), 16)
        else:
            box("Machine vibration plinth", (.5, .075, .5), (.90, .15, .84), "Graphite", .04)
            if TYPE == 7:
                cylinder("Kinetic flywheel", (.5, .53, .5), .37, .22, "Copper", (0, 0, 1), 32)
                cylinder("Flywheel hub", (.5, .53, .5), .13, .40, "Steel", (0, 0, 1), 20)
                box("Mill bearing frame", (.5, .33, .5), (.62, .37, .58), "Paint", .035)
            elif TYPE in (8, 11):
                cylinder("Generator stator", (.5, .49, .5), .31, .66, "Paint", (1, 0, 0), 24)
                for x in (.20, .80):
                    cylinder("Stator end cap", (x, .49, .5), .34, .10, "Steel", (1, 0, 0), 24)
                cylinder("Generator terminal", (.5, .86, .5), .10, .17, "Amber" if TYPE == 11 else "Cyan", sides=16)
            elif TYPE == 9:
                box("Transformer core", (.5, .43, .5), (.37, .50, .57), "Steel", .035)
                for x in (.24, .76):
                    cylinder("Transformer coil", (x, .48, .5), .17, .61, "Copper", sides=20)
            else:
                cylinder("Field tower", (.5, .51, .5), .16, .74, "Steel", sides=20)
                for y in (.39, .60, .81):
                    cylinder("Field induction disc", (.5, y, .5), .31 if TYPE == 12 else .25, .075, "Copper", sides=24)
                cylinder("Field emitter", (.5, .93, .5), .12, .10, "Amber" if TYPE == 12 else "Cyan", sides=20)
    KIND, OWNER, TYPE = "Static", None, 6


def export():
    data = {"Schema": 1, "Id": "forgeborne.cinder-exchange.v1", "Parts": []}
    graph = bpy.context.evaluated_depsgraph_get()
    for obj in PARTS:
        evaluated = obj.evaluated_get(graph)
        mesh = evaluated.to_mesh()
        mesh.calc_loop_triangles()
        vertices = [game(obj.matrix_world @ vertex.co) for vertex in mesh.vertices]
        triangles, normals = [], []
        # inverse transpose is required for the collapsed rotated plate, and
        # makes the export contract independent of unapplied object rotation.
        normal_matrix = obj.matrix_world.to_3x3().inverted().transposed()
        for triangle in mesh.loop_triangles:
            for vertex, loop in zip(triangle.vertices, triangle.loops):
                triangles.append(vertex)
                normals.append(game((normal_matrix @ mesh.corner_normals[loop].vector).normalized()))
        data["Parts"].append({"Name": obj.name, "Kind": obj["SurfaceKind"],
            "Owner": list(obj["LedgerCell"]) if "LedgerCell" in obj else None,
            "Type": obj["LedgerType"], "Material": obj.data.materials[0].name,
            "Vertices": vertices, "Indices": triangles, "Normals": normals})
        evaluated.to_mesh_clear()
    DEST.parent.mkdir(parents=True, exist_ok=True)
    DEST.write_bytes((json.dumps(data, separators=(",", ":"), allow_nan=False)+"\n").encode("utf-8"))
    # Templates are source assets, not extra floating scene objects in the hall.
    for obj in PARTS:
        obj.hide_render = obj["SurfaceKind"] == "Template"
        obj.hide_set(obj.hide_render)
    path = ROOT/"art/source/CinderExchange.blend"
    bpy.ops.wm.save_as_mainfile(filepath=str(path))
    print(json.dumps({"geometry": str(DEST), "source": str(path), "parts": len(PARTS),
                      "triangles": sum(len(p["Indices"])//3 for p in data["Parts"])}))


bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.context.scene.unit_settings.system = "METRIC"
for name, (colour, metal, roughness) in PALETTE.items():
    material = bpy.data.materials.new(name)
    material.diffuse_color = (*colour, 1)
    material.use_nodes = True
    shader = material.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value = (*colour, 1)
    shader.inputs["Metallic"].default_value = metal
    shader.inputs["Roughness"].default_value = roughness
environment()
deposits()
templates()
export()
