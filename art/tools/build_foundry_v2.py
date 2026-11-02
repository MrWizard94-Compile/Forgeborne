"""Original Cinder Exchange v2 geometry; v1 source is retained unchanged, Blender 4.5.15 LTS.

No imported geometry/textures. The .blend and Core's embedded mesh payload are
exported from the same closed components. Game space is X right/Y up/Z forward.
Run Blender --background --python-exit-code 1 --python art/tools/build_foundry_v2.py.
"""
from pathlib import Path
import bpy
import bmesh
import json
import math
import sys
sys.path.insert(0,str(Path(__file__).resolve().parent))
from fracture_geometry import clipped_cells, canonical_point, triangulate
from mathutils import Vector
from mathutils.bvhtree import BVHTree
from mathutils.geometry import barycentric_transform

ROOT = Path(__file__).resolve().parents[2]
DEST = ROOT / "src/Forgeborne.Core/Content/CinderExchangeV2.json"
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
    "CastFloor": ((.12,.145,.16),.03,.66),
    "EmberMineral": ((.22,.065,.018),.58,.62),
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


def arch(name, z, role="Paint", rx=14.25, ry=4.8, width=.5, thickness=.24, start=0, end=math.pi):
    # Closed elliptical structural rib, including both end caps. Its aperture
    # remains real empty space; there is no filled collider behind the arch.
    vertices = []
    for i in range(41):
        angle = start+(end-start)*i/40
        for radial, depth in ((-1, -1), (1, -1), (1, 1), (-1, 1)):
            vertices.append(tuple(point((16+(rx+radial*thickness/2)*math.cos(angle),
                                         8.6+(ry+radial*thickness/2)*math.sin(angle),
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


def fractured_bank(label, bounds, ore_cells, seed):
    """One original formation, cut into closed ledger-owned gather sections.

    The cut planes are internal until gathering exposes a fresh fracture. Source
    normals are interpolated onto cut fragments so owner boundaries do not turn
    into a visible grid. No enlarged render proxy disagrees with physical queries.
    """
    global KIND, OWNER, TYPE
    x0,x1,z0,z1=bounds
    cells,all_planes=clipped_cells(bounds,seed)
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=6,radius=1,
        location=point(((x0+x1)/2,3.96,(z0+z1)/2)))
    source=bpy.context.object;source.name=label+" original uncut formation"
    planes=[(Vector((math.cos(a),math.sin(a),.30)).normalized(),limit)
            for a,limit in ((seed*.71,.81),(seed*.71+2.3,.87),(seed*.71+4.5,.79))]
    planes += [(Vector((.31,-.23,1)).normalized(),.85),
               (Vector((math.cos(seed*.4),math.sin(seed*.4),.65)).normalized(),.76)]
    for vertex in source.data.vertices:
        p=vertex.co.copy()
        radius=1+.14*math.sin(p.x*4.6+seed)*math.cos(p.y*5.1-seed*.3)+.05*math.sin(p.z*9+p.x*2.4+seed)
        radius-=.035*abs(math.sin((p.z+.3*p.x)*13+seed))
        for normal,limit in planes:
            projection=normal.dot(p)
            if projection>0:radius=min(radius,limit/projection)
        radius=min(radius,.99/max(abs(p.x),abs(p.y),.000001))
        vertex.co=p*radius
    source.scale=((x1-x0)/2-.025,(z1-z0)/2-.025,.76+.07*math.sin(seed*.37))
    bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
    # Keep the sculpted source intact; exact clipping and bounded post-cut
    # cleanup handle proximity to fracture planes without denting planar beds.
    for vertex in source.data.vertices:
        if abs(vertex.co.z-4)<.0005:vertex.co.z=4
    for face in source.data.polygons:face.use_smooth=True
    sharp=bmesh.new();sharp.from_mesh(source.data)
    for edge in sharp.edges:
        if len(edge.link_faces)==2 and edge.calc_face_angle()>math.radians(25):edge.smooth=False
    sharp.to_mesh(source.data);sharp.free()
    mesh=source.data;mesh.update();mesh.calc_loop_triangles()
    mesh.materials.append(bpy.data.materials["Basalt"])
    cap_material=bpy.data.materials.get("__Forgeborne authoring fracture cap")
    if cap_material is None:cap_material=bpy.data.materials.new("__Forgeborne authoring fracture cap")
    vertices=[v.co.copy() for v in mesh.vertices]
    triangles=[tuple(t.vertices) for t in mesh.loop_triangles]
    print(json.dumps({"formation":label,"sourceTriangles":len(triangles)}),flush=True)
    normals=[[mesh.corner_normals[loop].vector.copy() for loop in t.loops] for t in mesh.loop_triangles]
    tree=BVHTree.FromPolygons(vertices,triangles,all_triangles=True)
    for z in range(z0,z1):
        for x in range(x0,x1):
            OWNER=(x,4,z);TYPE=2 if (x,z) in ore_cells else 1;KIND="Deposit"
            prism_vertices,prism_faces,cut_planes=cells[(x,z)]
            obj=bpy.data.objects.new(label+" gather section",mesh.copy());bpy.context.collection.objects.link(obj)
            prism=bpy.data.meshes.new("Natural 3D fracture cell")
            prism.from_pydata([tuple(point(v)) for v in prism_vertices],[],prism_faces);prism.update()
            prism.materials.append(cap_material)
            cutter=bpy.data.objects.new("Natural 3D fracture cell",prism);bpy.context.collection.objects.link(cutter)
            for selected in bpy.context.selected_objects:selected.select_set(False)
            bpy.context.view_layer.objects.active=obj;obj.select_set(True);cutter.select_set(False)
            modifier=obj.modifiers.new("Exact owner intersection","BOOLEAN")
            modifier.operation="INTERSECT";modifier.solver="EXACT";modifier.object=cutter;modifier.material_mode="TRANSFER"
            bpy.ops.object.modifier_apply(modifier=modifier.name)
            bpy.data.objects.remove(cutter,do_unlink=True)
            bpy.data.meshes.remove(prism)
            for vertex in obj.data.vertices:
                if abs(vertex.co.z-4)<.00001:vertex.co.z=4
            # Exact intersections retain collinear cap edges from source
            # fracture planes. Dissolve those edges before triangulation;
            # never export slivers or relax the runtime topology validator.
            clean=bmesh.new();clean.from_mesh(obj.data)
            bmesh.ops.remove_doubles(clean,verts=list(clean.verts),dist=.0005)
            bmesh.ops.dissolve_degenerate(clean,edges=list(clean.edges),dist=.0005)
            bmesh.ops.dissolve_limit(clean,angle_limit=.0001,use_dissolve_boundaries=True,
                verts=list(clean.verts),edges=list(clean.edges),delimit={"MATERIAL"})
            clean.to_mesh(obj.data);clean.free()
            # Blender's generic ngon tessellator can make zero-area triangles
            # from a cap's collinear boundary points. A verified interior fan
            # retains every shared boundary edge and avoids those slivers. Only
            # use it when the centre lies strictly inside every oriented edge.
            obj.data.update();closed_vertices=[tuple(v.co) for v in obj.data.vertices];closed_faces=[];closed_materials=[]
            for polygon in obj.data.polygons:
                indices=list(polygon.vertices);points=[Vector(closed_vertices[i]) for i in indices]
                centre=Vector(tuple(math.fsum(p[axis] for p in points)/len(points) for axis in range(3)))
                inside=all((p-centre).cross(points[(i+1)%len(points)]-centre).dot(polygon.normal)>.0000002
                           for i,p in enumerate(points))
                if len(points)>3 and inside:
                    middle=len(closed_vertices);closed_vertices.append(tuple(centre))
                    closed_faces.extend((indices[i],indices[(i+1)%len(indices)],middle) for i in range(len(indices)))
                    closed_materials.extend([polygon.material_index]*len(indices))
                elif len(points)>3:
                    triangles_out=triangulate(closed_vertices,indices,tuple(polygon.normal));closed_faces.extend(triangles_out)
                    closed_materials.extend([polygon.material_index]*len(triangles_out))
                else:closed_faces.append(indices);closed_materials.append(polygon.material_index)
            previous=obj.data;slots=list(previous.materials);obj.data=bpy.data.meshes.new(label+" closed owner solid")
            obj.data.from_pydata(closed_vertices,[],closed_faces);obj.data.update()
            for material in slots:obj.data.materials.append(material)
            for polygon,material in zip(obj.data.polygons,closed_materials):polygon.material_index=material
            bpy.data.meshes.remove(previous)
            if len(obj.data.polygons)<4:raise RuntimeError("Formation misses declared resource owner "+str(OWNER))
            obj.data.update();custom=[None]*len(obj.data.loops)
            for polygon in obj.data.polygons:
                points=[obj.data.vertices[v].co for v in polygon.vertices]
                cut=obj.data.materials[polygon.material_index]==cap_material
                polygon.use_smooth=not cut
                for loop in polygon.loop_indices:
                    if cut:normal=polygon.normal.copy()
                    else:
                        position=obj.data.vertices[obj.data.loops[loop].vertex_index].co
                        candidates=tree.find_nearest_range(position,.00075)
                        if not candidates:raise RuntimeError("Cut surface lost its source formation: "+label+str(OWNER))
                        nearest,face_normal,index,distance=max(candidates,key=lambda hit:hit[1].dot(polygon.normal))
                        if face_normal.dot(polygon.normal)<.1:
                            raise RuntimeError("Cut surface winding no longer agrees with its original formation")
                        triangle=triangles[index]
                        normal=barycentric_transform(nearest,*[vertices[v] for v in triangle],*normals[index]).normalized()
                    custom[loop]=normal
            obj.data.normals_split_custom_set(custom)
            obj.data.materials.clear()
            for polygon in obj.data.polygons:polygon.material_index=0
            finish(obj,label+" gather section","Basalt")
            if TYPE==2:ember_vein(label,tree,x,z,seed,cut_planes)
    source.hide_render=True;source.hide_set(True)


def ember_vein(label,tree,x,z,seed,cut_planes):
    # Mineral matrix and a thin hot inclusion follow the real fractured host.
    # Branches are closed solids; all pieces keep the same canonical owner.
    for branch in range(2):
        points=[]
        for i in range(13 if branch==0 else 8):
            t=.12+i*.76/12 if branch==0 else i/7
            wx=x+t if branch==0 else x+.5+.30*t
            wz=z+.23+.55*t+.035*math.sin(t*13+seed) if branch==0 else z+.5-.23*t+.015*math.sin(t*9)
            hit,_,_,_=tree.ray_cast(point((wx,5.2,wz)),Vector((0,0,-1)),2)
            if hit is not None and hit.z>=4.014 and all(nx*wx+ny*hit.z+nz*wz-d<-.035 for nx,ny,nz,d in cut_planes):
                hit.z+=.006;points.append(hit)
        if len(points)<2:
            if branch==0:raise RuntimeError("Ore owner lacks a visible surface vein")
            continue
        for role,scale,lift,depth in (("EmberMineral",1,0,.008),("Amber",.16,.009,.004)):
            vertices=[]
            for i,p in enumerate(points):
                tangent=(points[min(i+1,len(points)-1)]-points[max(0,i-1)]).normalized()
                side=tangent.cross(Vector((0,0,1))).normalized()
                radius=(.009+.006*math.sin(i*2.7+seed)**2)*scale
                for sx,sy in ((-1,-1),(1,-1),(1,1),(-1,1)):
                    vertices.append(tuple(p+side*radius*sx+Vector((0,0,lift+depth*sy))))
            faces=[(3,2,1,0),tuple(range((len(points)-1)*4,len(points)*4))]
            for i in range(len(points)-1):
                for j in range(4):
                    a=i*4+j;b=i*4+(j+1)%4;faces.append((a,b,b+4,a+4))
            mesh=bpy.data.meshes.new(label+" mineral ribbon");mesh.from_pydata(vertices,[],[tuple(reversed(f)) for f in faces]);mesh.update()
            obj=bpy.data.objects.new(label+" exposed mineral vein",mesh);bpy.context.collection.objects.link(obj)
            finish(obj,label+" exposed mineral vein",role)


def environment():
    # Flush steel runners replace foundation strips. They are not overlapping
    # raised decals: shared edges are exactly coplanar, without z-fighting or
    # a hidden lip that would block a machine's service envelope.
    strips = ((0, 9.1575, "CastFloor"), (9.1575, 9.2425, "Steel"),
              (9.2425, 22.7575, "CastFloor"), (22.7575, 22.8425, "Steel"),
              (22.8425, 32, "CastFloor"))
    for lo, hi, role in strips:
        box("Continuous cast foundation" if role == "CastFloor" else "Flush service runner",
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
        arch("Rib inset copper spine", z-.29, "Copper", 14.27, 4.82, .075, .055)
        for x in (5.8, 26.2):
            # Fixtures are actual closed geometry with useful silhouette.
            box("Hanging luminaire hood", (x, 9.35, z), (1.35, .22, .5), "Graphite")
            box("Hanging warm diffuser", (x, 9.22, z), (1.15, .045, .36), "Warm", .01)
            for dx in (-.45,.45):
                cylinder("Luminaire suspension stay",(x+dx,10.64,z),.022,2.5,"Steel",sides=8)
    # Closed curved roof shells leave a real longitudinal daylight slot and a
    # damaged transverse opening above the fallen plate. No skybox substitute.
    slot=math.acos(2/14.25)
    for lo,hi in ((2.9,12.9),(14.0,18.0),(20.1,30.5)):
        for a,b in ((0,slot),(math.pi-slot,math.pi)):
            arch("Vaulted exchange roof shell",(lo+hi)/2,"Graphite",14.25,4.8,hi-lo,.14,a,b)
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
    # Five whole mineral formations, with continuous source normals across
    # ledger cuts. Stock funds the real machine/defence loop without grants.
    banks=(
        ("Separator fracture",(10,14,11,13),{(11,11),(12,11),(11,12)},17),
        ("Eastern pressure fracture",(19,23,13,15),{(21,13),(22,14)},31),
        ("Collapsed host formation",(7,11,20,23),set(),47),
        ("Central host formation",(17,20,18,20),set(),59),
        ("Exposed ember formation",(20,23,10,12),{(x,z) for x in range(20,23) for z in range(10,12)},71),
    )
    for label,bounds,ore,seed in banks:fractured_bank(label,bounds,ore,seed)
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
            # A real bowed ballistic plate, rather than a rectangular block.
            vertices=[];faces=[]
            for i in range(25):
                x=.07+.86*i/24;z=.45+.14*((x-.5)/.43)**2
                for y,depth in ((.14,-.06),(.94,-.06),(.94,.06),(.14,.06)):
                    vertices.append(tuple(point((x,y,z+depth))))
            faces=[(3,2,1,0),tuple(range(96,100))]
            for i in range(24):
                for j in range(4):
                    a=i*4+j;b=i*4+(j+1)%4;faces.append((a,b,b+4,a+4))
            mesh=bpy.data.meshes.new("Bowed defensive plate");mesh.from_pydata(vertices,[],faces);mesh.update()
            obj=bpy.data.objects.new("Bowed defensive plate",mesh);bpy.context.collection.objects.link(obj)
            for i,face in enumerate(mesh.polygons):face.use_smooth=i>=2 and (i-2)%4 in (0,2)
            finish(obj,"Bowed defensive plate","Paint")
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
    data = {"Schema": 1, "Id": "forgeborne.cinder-exchange.v2", "Parts": []}
    graph = bpy.context.evaluated_depsgraph_get()
    for obj in PARTS:
        evaluated = obj.evaluated_get(graph)
        mesh = evaluated.to_mesh()
        mesh.calc_loop_triangles()
        vertices = [game(obj.matrix_world @ vertex.co) for vertex in mesh.vertices]
        if obj["SurfaceKind"]!="Static":
            owner=list(obj["LedgerCell"]) if "LedgerCell" in obj else (0,0,0)
            spread=.45 if obj["SurfaceKind"]=="Deposit" else 0
            if any(not (-spread if axis!=1 else 0)<=v[axis]-owner[axis]<=(1+spread if axis!=1 else 1)
                   for v in vertices for axis in range(3)):
                raise RuntimeError("Exported geometry leaves its owner: "+obj.name)
        triangles, normals = [], []
        # inverse transpose is required for the collapsed rotated plate, and
        # makes the export contract independent of unapplied object rotation.
        normal_matrix = obj.matrix_world.to_3x3().inverted().transposed()
        for triangle in mesh.loop_triangles:
            a,b,c=[Vector(vertices[i]) for i in triangle.vertices]
            if (b-a).cross(c-a).length_squared<1e-14:
                raise RuntimeError("Degenerate exported triangle in "+obj.name+" polygon vertices "+str(len(mesh.polygons[triangle.polygon_index].vertices))+" "+str([tuple(a),tuple(b),tuple(c)]))
            outward=(b-a).cross(c-a).normalized()
            for vertex, loop in zip(triangle.vertices, triangle.loops):
                triangles.append(vertex)
                normal=game((normal_matrix @ mesh.corner_normals[loop].vector).normalized())
                if Vector(normal).dot(outward)<.01:raise RuntimeError("Exported shading normal points into its triangle in "+obj.name)
                normals.append(normal)
        data["Parts"].append({"Name": obj.name, "Kind": obj["SurfaceKind"],
            "Owner": list(obj["LedgerCell"]) if "LedgerCell" in obj else None,
            "Type": obj["LedgerType"], "Material": obj.data.materials[0].name,
            "Vertices": vertices, "Indices": triangles, "Normals": normals})
        evaluated.to_mesh_clear()
    DEST.parent.mkdir(parents=True, exist_ok=True)
    encoded=(json.dumps(data, separators=(",", ":"), allow_nan=False)+"\n").encode("utf-8")
    if len(data["Parts"])>1024 or sum(len(p["Indices"])//3 for p in data["Parts"])>150000 or len(encoded)>24000000:
        raise RuntimeError("Authored v2 content exceeds the retained runtime capacity contract")
    DEST.write_bytes(encoded)
    # Templates are source assets, not extra floating scene objects in the hall.
    for obj in PARTS:
        obj.hide_render = obj["SurfaceKind"] == "Template"
        obj.hide_set(obj.hide_render)
    path = ROOT/"art/source/CinderExchangeV2.blend"
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
