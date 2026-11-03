"""Original pressure-carbine and anatomical glove authoring, Blender 4.5.15.

Game-space metres: X right, Y up, Z muzzle-forward. No external geometry,
anatomy templates, textures or animation data. Continuous glove volumes and
their original rest-pose skeleton are generated together.
"""
import math
import sys
from pathlib import Path
import bpy
import bmesh
from mathutils import Vector
from mathutils.kdtree import KDTree

A=None


def tube(name,points,radii,role="GloveFabric",parent="SkinBuild",sides=12,steps=4,folds=False,section_power=1.0):
    points=[Vector(p) for p in points]
    radii=[Vector((r,r)) if isinstance(r,(float,int)) else Vector(r) for r in radii]
    samples=[]
    for i in range(len(points)-1):
        p0=points[max(0,i-1)];p1=points[i];p2=points[i+1];p3=points[min(len(points)-1,i+2)]
        for step in range(steps):
            t=step/steps
            p=.5*((2*p1)+(-p0+p2)*t+(2*p0-5*p1+4*p2-p3)*t*t+(-p0+3*p1-3*p2+p3)*t*t*t)
            r=radii[i]*(1-t)+radii[i+1]*t
            samples.append((p,r))
    samples.append((points[-1],radii[-1]))
    vertices=[];faces=[];previous=None
    for i,(p,r) in enumerate(samples):
        tangent=(samples[min(i+1,len(samples)-1)][0]-samples[max(0,i-1)][0]).normalized()
        u=tangent.cross(Vector((0,1,0)))
        if u.length<.01:u=tangent.cross(Vector((1,0,0)))
        u.normalize()
        if previous is not None and u.dot(previous)<0:u=-u
        v=tangent.cross(u).normalized();previous=u
        for side in range(sides):
            angle=side*2*math.pi/sides
            # Cloth bunches asymmetrically along the bent arm; its cross-section
            # is oval rather than the old constant circular pipe.
            length=i/max(1,len(samples)-1)
            fold=.0024*math.sin(length*math.pi*22+angle*.8)*math.sin(length*math.pi)**2 if folds else 0
            cosine,sine=math.cos(angle),math.sin(angle)
            if section_power!=1.0:
                cosine=math.copysign(abs(cosine)**section_power,cosine)
                sine=math.copysign(abs(sine)**section_power,sine)
            vertices.append(tuple(A.point(p+(r.x+fold)*u*cosine+(r.y+fold)*v*sine)))
        if i:
            for side in range(sides):
                j=(side+1)%sides
                faces.append(((i-1)*sides+side,(i-1)*sides+j,i*sides+j,i*sides+side))
    faces.extend((tuple(reversed(range(sides))),tuple(range((len(samples)-1)*sides,len(vertices)))))
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(vertices,[],[tuple(reversed(f)) for f in faces]);mesh.update()
    obj=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(obj)
    for polygon in mesh.polygons:polygon.use_smooth=True
    return A.finish_part(obj,name,role,parent,0)


def join(objects,name):
    bpy.ops.object.select_all(action="DESELECT")
    for obj in objects:obj.select_set(True)
    bpy.context.view_layer.objects.active=objects[0]
    if len(objects)>1:bpy.ops.object.join()
    obj=bpy.context.object;obj.name=name
    bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
    return obj


def annulus(name,centre,outer,inner,depth,role="Steel",parent="Frame",sides=40):
    x,y,z=centre;vertices=[];faces=[]
    for dz,r in ((-depth/2,outer),(depth/2,outer),(-depth/2,inner),(depth/2,inner)):
        vertices.extend(tuple(A.point((x+r*math.cos(i*2*math.pi/sides),y+r*math.sin(i*2*math.pi/sides),z+dz))) for i in range(sides))
    for i in range(sides):
        j=(i+1)%sides
        faces.extend(((i,j,sides+j,sides+i),(2*sides+j,2*sides+i,3*sides+i,3*sides+j),
                      (j,i,2*sides+i,2*sides+j),(sides+i,sides+j,3*sides+j,3*sides+i)))
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(vertices,[],[tuple(reversed(f)) for f in faces]);mesh.update()
    obj=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(obj)
    for poly in mesh.polygons:poly.use_smooth=True
    return A.finish_part(obj,name,role,parent,.0008)


def side_panel(name,side,outline,x,thickness,role="Graphite"):
    vertices=[tuple(A.point((side*(x+dx),y,z))) for dx in (0,thickness) for y,z in outline]
    count=len(outline);faces=[tuple(reversed(range(count))),tuple(range(count,count*2))]
    for i in range(count):j=(i+1)%count;faces.append((i,j,count+j,count+i))
    if side==1:faces=[tuple(reversed(f)) for f in faces]
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(vertices,[],faces);mesh.update()
    obj=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(obj)
    # Orient the closed extrusion independently of outline/side winding.
    bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(mesh);bm.free()
    return A.finish_part(obj,name,role,"Frame",min(.0025,thickness*.40))


def socket_bolt(centre,side=1):
    x,y,z=centre
    A.cylinder("Recessed fastener seat",(x-side*.001,y,z),.0056,.0018,"Rubber",axis=(side,0,0),sides=24,bevel=.0003)
    A.cylinder("Captive machined hex head",centre,.0041,.0022,"Steel",axis=(side,0,0),sides=6,bevel=.0005)
    A.box("Fastener tool recess",(x+side*.0013,y,z),(.0005,.0014,.0048),"Graphite",bevel=.0002)


def grip_shell(name,sections,role,parent="Frame"):
    """Closed tapered shell along Y; sections are Y, width, depth, centre-Z."""
    outline=[(-.5,-.30),(-.30,-.5),(.30,-.5),(.5,-.30),(.5,.30),(.30,.5),(-.30,.5),(-.5,.30)]
    vertices=[tuple(A.point((x*width,y,z+v*depth))) for y,width,depth,z in sections for x,v in outline]
    count=len(outline);faces=[tuple(reversed(range(count))),tuple(range((len(sections)-1)*count,len(vertices)))]
    for row in range(len(sections)-1):
        for i in range(count):j=(i+1)%count;faces.append((row*count+i,row*count+j,(row+1)*count+j,(row+1)*count+i))
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(vertices,[],faces);mesh.update()
    bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(mesh);bm.free()
    obj=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(obj)
    return A.finish_part(obj,name,role,parent,.003)


def glove_palm(side,wrist,direction):
    """Original wrist/metacarpal fan, with a thinner back and staggered knuckles."""
    if side=="Right":
        sections=[((.057,-.154,-.180),(.018,.030)),
                  ((.050,-.137,-.160),(.016,.032)),((.048,-.118,-.140),(.0155,.028)),
                  ((.047,-.104,-.120),(.013,.0175)),((.045,-.103,-.114),(.007,.009))]
    else:
        sections=[((-.068,-.061,.280),(.020,.024)),
                  ((-.053,-.049,.302),(.016,.021)),((-.044,-.042,.330),(.015,.021)),
                  ((-.043,-.045,.351),(.0155,.0205)),((-.043,-.050,.372),(.013,.0175)),
                  ((-.043,-.051,.378),(.007,.009))]
    sections=[(wrist-direction*.014,(.028,.023)),(wrist,(.025,.021))]+sections
    return tube(side+" tailored metacarpal glove",[p for p,_ in sections],[r for _,r in sections],
                sides=24,steps=6)


def glove_digit(side,digit,path):
    """Shaped phalanges and a rounded pad; the original joint rest rig is retained."""
    a,b,c,tip=[Vector(p) for p in path]
    if digit=="Thumb":tip=c.lerp(tip,.75)
    tangent=(tip-c).normalized()
    radius=.0092 if digit=="Thumb" else .0076 if digit=="Little" else .0087
    centres=[a,a.lerp(b,.72),b,b.lerp(c,.40),b.lerp(c,.78),c,c.lerp(tip,.66),
             tip,tip+tangent*.003,tip+tangent*.005]
    profile=[1.05,.93,1.07,.87,.85,.95,.78,.65,.35,.04]
    # Oval pads distinguish the dorsal knuckle width from the softer finger pad.
    radii=[(radius*r,radius*r*.86) for r in profile]
    return tube(side+" "+digit+" shaped glove",centres,radii,steps=3,sides=20,section_power=.82)


def dorsal_outline(side):
    return [(-.156,-.180),(-.143,-.164),(-.126,-.145),(-.108,-.126),(-.100,-.131),
        (-.115,-.153),(-.134,-.176)] if side=="Right" else [(-.054,.281),(-.059,.301),(-.059,.326),
        (-.057,.351),(-.048,.365),(-.035,.349),(-.027,.324),(-.034,.299),(-.045,.281)]


def glove_webbing(side,paths):
    """Original soft interdigital saddles; leave the distal fingers separate."""
    parts=[]
    digits=("Index","Middle","Ring","Little")
    for first,second in zip(digits,digits[1:]):
        a,b=([Vector(p) for p in paths[name]] for name in (first,second))
        start=a[0].lerp(a[1],.24);end=b[0].lerp(b[1],.24)
        saddle=(a[0]+b[0])*.5+((a[1]-a[0])+(b[1]-b[0]))*.04
        parts.append(tube(side+first+second+" continuous glove web",[start,saddle,end],
                          [(.0075,.008),(.0085,.009),(.0075,.008)],steps=4,sides=16))
    return parts


def construct_glove_surface(side,skin,paths,wrist,direction):
    """Sculpt flexion folds and cut a sewn dorsal panel into the closed body."""
    chains={name:[Vector(p) for p in points] for name,points in paths.items()}
    for vertex in skin.data.vertices:
        p=Vector(A.game(vertex.co));nearest=None
        for name,points in chains.items():
            for segment,(head,tail) in enumerate(zip(points,points[1:])):
                delta=tail-head;t=max(0,min(1,(p-head).dot(delta)/delta.length_squared))
                candidate=((p-head-delta*t).length,name,segment,t)
                if nearest is None or candidate[0]<nearest[0]:nearest=candidate
        distance,digit,segment,t=nearest
        if distance>.016 or (segment==0 and t<.30):continue
        points=chains[digit];displacement=Vector((0,0,0))
        for joint in (1,2):
            incoming=(points[joint]-points[joint-1]).normalized()
            outgoing=(points[joint+1]-points[joint]).normalized()
            tangent=(incoming+outgoing).normalized();inner=(outgoing-incoming).normalized()
            delta=p-points[joint];axial=delta.dot(tangent);radial=delta-tangent*axial
            if abs(axial)>.010 or not .003<radial.length<.015:continue
            facing=max(0,radial.normalized().dot(inner))**2
            crease=.00075*math.exp(-((axial+.0025)/.0022)**2)
            crease+=.00040*math.exp(-((axial-.0020)/.0018)**2)
            displacement-=radial.normalized()*crease*facing
        vertex.co=A.point(p+displacement)
    skin.data.update()

    outline=dorsal_outline(side)
    low=Vector((min(p[0] for p in outline)-.004,min(p[1] for p in outline)-.004))
    high=Vector((max(p[0] for p in outline)+.004,max(p[1] for p in outline)+.004))
    outward=A.point((1 if side=="Right" else -1,0,0))
    bm=bmesh.new();bm.from_mesh(skin.data);bm.normal_update()
    for start,end in zip(outline,outline[1:]+outline[:1]):
        bm.verts.index_update();bm.edges.index_update();bm.faces.index_update()
        selected=[]
        for face in bm.faces:
            p=Vector(A.game(face.calc_center_median()))
            if face.normal.dot(outward)>.08 and low.x<=p.y<=high.x and low.y<=p.z<=high.y:selected.append(face)
        edges={edge for face in selected for edge in face.edges}
        vertices={vertex for edge in edges for vertex in edge.verts}
        dy,dz=end[0]-start[0],end[1]-start[1]
        bmesh.ops.bisect_plane(bm,geom=sorted(vertices,key=lambda v:v.index)+sorted(edges,key=lambda e:e.index)+selected,dist=.000001,
            plane_co=A.point((0,*start)),plane_no=A.point((0,-dz,dy)).normalized())
        bm.normal_update()
    bmesh.ops.dissolve_degenerate(bm,dist=.000001,edges=list(bm.edges))
    # A cut can leave three collinear corners in a quad. Choose the diagonal
    # explicitly instead of letting a later fixed quad split create a sliver.
    bmesh.ops.triangulate(bm,faces=list(bm.faces),quad_method='BEAUTY',ngon_method='EAR_CLIP')
    bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(skin.data);bm.free()
    skin.data.update()
    sleeve_slot=len(skin.data.materials);skin.data.materials.append(A.MATERIALS["SleeveFabric"])
    def in_panel(y,z):
        inside=False
        for (ay,az),(by,bz) in zip(outline,outline[1:]+outline[:1]):
            if (az>z)!=(bz>z) and y<(by-ay)*(z-az)/(bz-az)+ay:inside=not inside
        return inside
    candidates={face.index for face in skin.data.polygons if face.normal.dot(outward)>.08
                and in_panel(Vector(A.game(face.center)).y,Vector(A.game(face.center)).z)}
    # A sewn back is one piece of cloth. Grow it from the actual exposed palm,
    # so occluded finger surfaces with the same Y/Z projection stay leather.
    seed_y=sum(p[0] for p in outline)/len(outline);seed_z=sum(p[1] for p in outline)/len(outline)
    origin=A.point((.18 if side=="Right" else -.18,seed_y,seed_z))
    hit,_,_,seed=skin.ray_cast(origin,-outward,distance=.4)
    if not hit or seed not in candidates:raise RuntimeError(side+" dorsal panel seed missed its cloth surface")
    edge_faces={}
    for face in skin.data.polygons:
        indices=list(face.vertices)
        for a,b in zip(indices,indices[1:]+indices[:1]):edge_faces.setdefault(tuple(sorted((a,b))),[]).append(face.index)
    adjacency={index:set() for index in candidates}
    for faces in edge_faces.values():
        selected=set(faces)&candidates
        for index in selected:adjacency[index].update(selected-{index})
    panel=set();pending=[seed]
    while pending:
        index=pending.pop()
        if index in panel:continue
        panel.add(index);pending.extend(adjacency[index]-panel)
    for face in skin.data.polygons:
        p=Vector(A.game(face.center))
        if (p-wrist).dot(direction)<-.038 or face.index in panel:
            face.material_index=sleeve_slot
        face.use_smooth=True
    skin.data.update()


def fitted_dorsal_stitches(side,skin):
    """Project original seam dashes onto the same cut panel outline."""
    right=side=="Right";yz=dorsal_outline(side)
    inverse=skin.matrix_world.inverted()
    direction=(inverse.to_3x3()@A.point((-1 if right else 1,0,0))).normalized()
    def project(y,z):
        origin=inverse@A.point((.18 if right else -.18,y,z))
        hit,point,normal,_=skin.ray_cast(origin,direction,distance=.4)
        if not hit:raise RuntimeError(side+" authored dorsal seam missed the original glove")
        return Vector(A.game(skin.matrix_world@(point+normal*.0004)))
    parts=[]
    for start,end in zip(yz,yz[1:]+yz[:1]):
        start,end=Vector(start),Vector(end)
        count=max(1,math.ceil((end-start).length/.0045))
        for i in range(count):
            a=start.lerp(end,(i+.18)/count);b=start.lerp(end,(i+.68)/count)
            parts.append(tube(side+" fitted saddle stitch",[project(*a),project(*b)],
                              [.0004,.0004],"GloveStitch",steps=1,sides=6))
    return parts


def build_hand(side):
    right=side=="Right"
    if right:
        elbow=Vector((.21,-.32,-.365));wrist=Vector((.065,-.174,-.202));palm=Vector((.048,-.128,-.149))
        paths={"Index":[(.047,-.101,-.130),(.039,-.084,-.095),(.022,-.084,-.051),(.002,-.093,-.037)],
               "Middle":[(.052,-.116,-.148),(.046,-.112,-.108),(.005,-.112,-.097),(-.027,-.119,-.124)],
               "Ring":[(.052,-.139,-.152),(.043,-.136,-.117),(.001,-.135,-.108),(-.026,-.142,-.136)],
               "Little":[(.048,-.159,-.163),(.037,-.156,-.126),(.002,-.154,-.115),(-.030,-.160,-.143)],
               "Thumb":[(.050,-.096,-.164),(.028,-.074,-.180),(-.014,-.075,-.158),(-.028,-.096,-.136)]}
    else:
        wrist=Vector((-.078,-.074,.255));palm=Vector((-.044,-.042,.327))
        # Author the support elbow for the actual camera-local weapon offset.
        # The old x=-.195 elbow was almost cancelled by ViewOffset.x=.19 and
        # made the sleeve enter on the camera centre line. Keep its original
        # forearm length while bringing the elbow left and slightly forward.
        forearm_length=(wrist-Vector((-.195,-.235,.045))).length
        elbow=wrist-Vector((.24,.145,.075)).normalized()*forearm_length
        paths={}
        for name,z,length in zip(("Index","Middle","Ring","Little"),(.365,.340,.315,.290),(.96,1.04,.98,.84)):
            paths[name]=[(-.044,-.052,z),(-.044+.021*length,-.080,z),
                         (-.044+.060*length,-.071,z),(-.044+.086*length,-.044,z-.004)]
        paths["Thumb"]=[(-.045,-.012,.285),(-.047,.018,.305),(-.022,.041,.332),(.003,.045,.350)]
    length=(wrist-elbow).length
    if not .24<length<.32:raise RuntimeError(side+" forearm proportions are outside the authored adult suit range")
    bones={side+"Forearm":(elbow,wrist,None),side+"Palm":(wrist,palm+Vector((0,0,.025)),side+"Forearm")}
    direction=(wrist-elbow).normalized()
    soft=[glove_palm(side,wrist,direction)]
    continuation=elbow+Vector((.035 if right else -.045,-.22,-.095))
    soft.append(tube(side+" tailored continuous sleeve",[continuation,elbow,elbow*.65+wrist*.35,elbow*.25+wrist*.75,wrist],
                     [(.063,.050),(.057,.046),(.052,.041),(.039,.031),(.029,.024)],
                     steps=8,sides=24,folds=True))
    # The palm's thumb pad and little-finger heel are distinct volumes, rather
    # than one overinflated ellipsoid hiding all metacarpal structure.
    thenar=(.043,-.118,-.164) if right else (-.038,-.015,.305)
    heel=(.044,-.157,-.170) if right else (-.045,-.043,.291)
    thumb_pad_size=(.024,.044,.028) if right else (.027,.032,.038)
    soft.append(A.rounded_volume(side+" opposed thumb pad",thenar,thumb_pad_size,"GloveFabric","SkinBuild"))
    soft.append(A.rounded_volume(side+" little finger palm heel",heel,(.025,.027,.041),"GloveFabric","SkinBuild"))
    for digit,path in paths.items():
        soft.append(glove_digit(side,digit,path))
        for i in range(3):bones[side+digit+str(i)]=(Vector(path[i]),Vector(path[i+1]),side+"Palm" if i==0 else side+digit+str(i-1))
    soft.extend(glove_webbing(side,paths))
    skin=join(soft,side+" continuous glove and sleeve")
    remesh=skin.modifiers.new("Continuous original glove volume","REMESH");remesh.mode="VOXEL";remesh.voxel_size=.0012
    remesh.use_smooth_shade=True;remesh.use_remove_disconnected=False
    bpy.ops.object.modifier_apply(modifier=remesh.name)
    smooth=skin.modifiers.new("Pressure fabric relaxation","SMOOTH");smooth.factor=.45;smooth.iterations=2
    bpy.ops.object.modifier_apply(modifier=smooth.name)
    triangles=sum(len(p.vertices)-2 for p in skin.data.polygons)
    if triangles>27000:
        decimate=skin.modifiers.new("Bounded view topology","DECIMATE");decimate.ratio=27000/triangles
        bpy.ops.object.modifier_apply(modifier=decimate.name)
    construct_glove_surface(side,skin,paths,wrist,direction)
    # Validate a single connected closed organic surface before adding rigid trim.
    bm=bmesh.new();bm.from_mesh(skin.data);bm.verts.ensure_lookup_table()
    if any(not e.is_manifold for e in bm.edges):raise RuntimeError(side+" glove surface is open or nonmanifold")
    pending=[bm.verts[0]];seen=set()
    while pending:
        vertex=pending.pop()
        if vertex in seen:continue
        seen.add(vertex);pending.extend(e.other_vert(vertex) for e in vertex.link_edges)
    if len(seen)!=len(bm.verts):raise RuntimeError(side+" glove has disconnected anatomy")
    bm.free()
    rig_data=bpy.data.armatures.new(side+" original glove skeleton")
    rig=bpy.data.objects.new(side+"HandRig",rig_data);bpy.context.collection.objects.link(rig);rig.parent=A.GROUPS["Root"]
    bpy.ops.object.select_all(action="DESELECT");rig.select_set(True);bpy.context.view_layer.objects.active=rig
    bpy.ops.object.mode_set(mode="EDIT")
    for name,(head,tail,parent) in bones.items():
        bone=rig_data.edit_bones.new(name);bone.head=A.point(head);bone.tail=A.point(tail)
        if parent:bone.parent=rig_data.edit_bones[parent]
    bpy.ops.object.mode_set(mode="OBJECT")
    groups={name:skin.vertex_groups.new(name=name) for name in bones}
    for vertex in skin.data.vertices:
        p=Vector(A.game(skin.matrix_world @ vertex.co));distances=[]
        for name,(head,tail,_) in bones.items():
            delta=tail-head;t=max(0,min(1,(p-head).dot(delta)/delta.length_squared))
            distances.append(((p-head-delta*t).length,name))
        distances.sort()
        # Sleeve vertices follow the forearm, not nearby curled fingers.
        # At the wrist, blend across a short cuff into the palm. Finger chains
        # bind locally instead of averaging unrelated adjacent digits.
        along=(p-wrist).dot(direction)
        if along<.008:
            palm_weight=max(0,min(1,(along+.025)/.033))
            if palm_weight<1:groups[side+"Forearm"].add([vertex.index],1-palm_weight,"REPLACE")
            if palm_weight>0:groups[side+"Palm"].add([vertex.index],palm_weight,"REPLACE")
            continue
        closest=distances[0][1]
        digit=next((name for name in paths if closest.startswith(side+name)),None)
        nearest=[entry for entry in distances if entry[1]==side+"Palm" or (digit and entry[1].startswith(side+digit))][:4]
        weights=[1/(.004+distance)**4 for distance,_ in nearest];total=sum(weights)
        for (_,name),weight in zip(nearest,weights):groups[name].add([vertex.index],weight/total,"REPLACE")
    trim=[]
    for offset in (-.032,-.022):
        centre=wrist+direction*offset
        obj=A.ring(side+" stitched cuff welt",centre,.033,.0012,"Rubber","SkinBuild",axis=tuple(direction))
        trim.append((obj,side+"Forearm"))
    trim.extend((obj,None) for obj in fitted_dorsal_stitches(side,skin))
    # Knuckles are part of the continuous phalange surface. Separate rounded
    # beads at the MCP joints looked detached from the glove during flexion.
    for t in (.18,.47,.73):
        point=elbow.lerp(wrist,t)
        a=point+Vector((.042 if right else -.042,.004,0))
        b=point+Vector((.023 if right else -.023,.035,0))
        c=point+Vector((0,.040,.002))
        trim.append((tube(side+" articulated sleeve dart",[a,b,c],[.0011]*3,"SleeveFabric",steps=4,sides=6),side+"Forearm"))
    tree=KDTree(len(skin.data.vertices))
    for vertex in skin.data.vertices:tree.insert(vertex.co,vertex.index)
    tree.balance()
    for obj,bone in trim:
        if bone is not None:
            obj.vertex_groups.new(name=bone).add(list(range(len(obj.data.vertices))),1,"REPLACE")
        else:
            # Stitch dashes inherit their nearest actual skin vertex's normalized
            # four-weight binding, including the wrist transition and knuckles.
            transform=skin.matrix_world.inverted()@obj.matrix_world
            for vertex in obj.data.vertices:
                _,index,_=tree.find(transform@vertex.co)
                for weight in skin.data.vertices[index].groups:
                    name=skin.vertex_groups[weight.group].name
                    group=obj.vertex_groups.get(name) or obj.vertex_groups.new(name=name)
                    group.add([vertex.index],weight.weight,"REPLACE")
    combined=join([skin]+[obj for obj,_ in trim],side+"HandSkinned")
    combined.parent=rig
    modifier=combined.modifiers.new("Original four-weight glove deformation","ARMATURE");modifier.object=rig
    for vertex in combined.data.vertices:
        weights=[g.weight for g in vertex.groups]
        if not 1<=len(weights)<=4 or abs(sum(weights)-1)>.00001:raise RuntimeError("Invalid original glove skin weights")
    # Blender's default exporter otherwise names all armatures Armature.
    rig_data.display_type="STICK"
    contact=bpy.data.objects.new(side+"PalmContact",None);bpy.context.collection.objects.link(contact)
    contact.parent=A.GROUPS["Root"];contact.location=A.point(palm)
    return {"side":side,"bones":len(bones),"skinVertices":len(combined.data.vertices),"continuousGlove":True,"maxInfluences":4,"forearmLengthMetres":length}


def build_refined_carbine(authoring):
    global A
    A=authoring;A.begin("PulseCarbine")
    A.material("GloveFabric",(.16,.105,.061),0,.61)
    A.material("SleeveFabric",(.12,.155,.16),0,.78)
    A.material("GloveStitch",(.28,.21,.13),0,.85)
    A.material("OpticGlass",(.76,.90,.94),0,.08)
    outline=[(-.5,-.28),(-.36,-.5),(.34,-.5),(.5,-.29),(.5,.24),(.29,.5),(-.30,.5),(-.5,.24)]
    A.profile("Forged contoured pressure receiver",[(-.255,.065,.075),(-.19,.090,.105),(.125,.090,.105),(.215,.080,.090),(.265,.050,.060)],outline,bevel=.004)
    A.profile("Dropped shoulder recoil stock",[(-.475,.075,.093),(-.441,.10,.123),(-.295,.075,.095),(-.25,.075,.101)],
              [(x,y-1.0) for x,y in outline],"Rubber",bevel=.006)
    for y in (-.040,-.020,0,.020,.040):
        A.box("Stock pad compression groove",(0,y-.093,-.477),(.066,.002,.0025),"Graphite",bevel=.0005)
    tube("Curved stock retaining spine",[(0,-.020,-.210),(0,-.060,-.280),(0,-.085,-.350)],
         [.013,.014,.012],"Steel","Frame",sides=12,steps=5)
    panel=[(-.041,-.185),(-.048,-.135),(-.044,.115),(-.019,.177),(.028,.135),(.045,-.118),(.030,-.184)]
    for side in (-1,1):
        side_panel("Sculpted removable receiver cheek",side,panel,.046,.004,"Paint")
        side_panel("Inset machined service insert",side,[(-.020,-.137),(-.025,.072),(.018,.093),(.029,-.124)],.051,.002,"Graphite")
        for y,z in ((-.027,-.154),(.027,-.145),(-.022,.111),(.018,.116)):
            socket_bolt((side*.056,y,z),side)
        for z in (-.095,-.072,-.049,-.026):
            A.box("Receiver recessed cooling louvre",(side*.054,.009,z),(.0014,.025,.010),"Rubber",bevel=.0004)
            A.box("Louvre lower machined lip",(side*.055,-.003,z),(.0018,.002,.010),"Steel",bevel=.0005)
        A.box("Service latch seat",(side*.057,-.025,.036),(.004,.013,.027),"Steel",bevel=.0015)
        A.cylinder("Service latch pivot",(side*.061,-.023,.029),.003,.003,"Copper",axis=(side,0,0),sides=12,bevel=.0004)
    A.box("Seated magazine socket",(0,-.055,.101),(.078,.027,.121),"Graphite",bevel=.004)
    A.box("Pistol grip receiver bridge",(0,-.045,-.128),(.057,.020,.075),"Graphite",bevel=.003)
    A.cylinder("Barrel inner pressure tube",(0,.016,.433),.0195,.36,"Steel",sides=40)
    annulus("Actually bored muzzle crown",(0,.016,.626),.031,.0195,.070,"Graphite",sides=40)
    annulus("Machined open muzzle lip",(0,.016,.661),.029,.020,.006,"Steel",sides=40)
    # Open structural ribs expose the pressure tube and connect the heat guards.
    for z in (.285,.365,.445,.526):
        annulus("Induction heat sink support",(0,.016,z),.033,.025,.012,"Copper",sides=32)
    for side in (-1,1):
        for y in (-.009,.041):
            tube("Connected barrel shroud spine",[(side*.027,y,.27),(side*.035,y,.32),(side*.030,y,.55)],
                 [.004,.004,.003],"Graphite","Frame",sides=8,steps=3)
        for z in (.31,.35,.39,.43,.47,.51):
            A.box("Ceramic vent separator",(side*.035,.016,z),(.006,.044,.007),"Ceramic",bevel=.0015)
        tube("Continuous copper transfer line",[(side*.046,-.018,.11),(side*.044,-.035,.20),(side*.032,-.023,.265),(side*.028,-.003,.29)],
             [.0034]*4,"Copper","Frame",sides=10,steps=6)
    # The support-hand grip has rounded, tapered side profiles and real relief.
    A.profile("Lower grip pressure chassis",[(.23,.059,.053),(.27,.072,.063),(.38,.069,.062),(.42,.049,.039)],
              [(x,y-.65) for x,y in outline],"Rubber",bevel=.004)
    for z in (.255,.28,.305,.33,.355,.38):
        A.box("Foregrip compression relief",(0,-.066,z),(.045,.003,.003),"Graphite",bevel=.0007)
    grip_shell("Sculpted seated pistol grip",[(-.055,.052,.060,-.123),(-.078,.057,.067,-.129),
               (-.130,.055,.061,-.143),(-.190,.050,.053,-.158)],"Rubber")
    # A swept open guard leaves a visible trigger and room for the index finger.
    tube("Open continuous trigger guard",[(-.021,-.067,-.143),(-.022,-.134,-.108),(-.020,-.133,-.019),(-.018,-.076,.012)],
         [.0035]*4,"Steel","Frame",sides=10,steps=6)
    tube("Curved mechanical trigger",[(0,-.063,-.033),(0,-.085,-.040),(0,-.103,-.026)],
         [.003,.003,.0025],"Copper","Frame",sides=8,steps=4)
    grip_shell("Seated tapered capacitor magazine",[(-.065,.066,.098,.095),(-.085,.072,.112,.100),
               (-.190,.070,.114,.110),(-.215,.061,.100,.117)],"Graphite","Magazine")
    A.box("Magazine copper shoe",(0,-.216,.117),(.078,.013,.110),"Copper","Magazine",.004)
    for side in (-1,1):
        for z in (.079,.102,.125):
            A.box("Recessed magazine status aperture",(side*.038,-.105,z),(.002,.037,.005),"Cyan","Magazine",.0005)
    A.box("Integrated optic dovetail",(0,.053,-.025),(.042,.014,.22),"Steel",bevel=.003)
    for z in (-.121,-.092,-.063,-.034,-.005,.024,.053):
        A.box("Dovetail transverse slot",(0,.061,z),(.044,.003,.007),"Graphite",bevel=.0007)
    # A genuinely open hood, rather than a solid cylinder standing in for glass.
    tube("Open reflex protective hood",[(-.024,.063,.091),(-.029,.111,.091),(-.016,.128,.091),(.017,.128,.091),(.029,.111,.091),(.024,.063,.091)],
         [.0035]*6,"Graphite","Frame",sides=10,steps=4)
    A.box("Recessed reflex combiner",(0,.103,.092),(.041,.030,.0015),"OpticGlass",bevel=.0003)
    A.box("Reflex central emission mark",(0,.103,.090),(.0018,.0018,.0010),"Cyan",bevel=.0003)
    A.box("Charging slide",(-.057,.038,-.095),(.028,.013,.069),"Steel","ChargingHandle",.003)
    A.box("Charging grip inset",(-.072,.041,-.095),(.010,.014,.030),"Rubber","ChargingHandle",.002)
    A.cylinder("Selector bezel",(.058,.017,-.174),.010,.005,"Copper",axis=(1,0,0),sides=24,bevel=.0008)
    A.box("Selector lever",(.062,.020,-.162),(.005,.008,.026),"Graphite",bevel=.0015)
    gloves=[build_hand("Right"),build_hand("Left")]
    socket=bpy.data.objects.new("MuzzleSocket",None);bpy.context.collection.objects.link(socket)
    socket.parent=A.GROUPS["Root"];socket.location=A.point((0,.016,.665))
    # Named exact assembly pivots are exportable independently of the hand rig.
    for name,pivot in (("Magazine",(0,-.11,.105)),("ChargingHandle",(-.057,.038,-.095))):
        bpy.context.view_layer.update()
        group=A.GROUPS[name];matrices=[(child,child.matrix_world.copy()) for child in group.children]
        group.location=A.point(pivot)
        bpy.context.view_layer.update()
        for child,matrix in matrices:child.matrix_world=matrix
        bpy.context.view_layer.update()
        if any(abs(child.matrix_world[i][j]-matrix[i][j])>1e-6 for child,matrix in matrices for i in range(4) for j in range(4)):
            raise RuntimeError("Moving assembly pivot shifted original world geometry: "+name)
    triangles=sum(len(p.vertices)-2 for obj in bpy.context.scene.objects if obj.type=="MESH" for p in obj.data.polygons)
    if triangles>100000:raise RuntimeError("Carbine view exceeded its 100k triangle budget: "+str(triangles))
    bpy.context.view_layer.update()
    for obj in bpy.context.scene.objects:
        if obj.type!="MESH":continue
        obj.data.calc_loop_triangles()
        for triangle in obj.data.loop_triangles:
            points=[obj.matrix_world@obj.data.vertices[i].co for i in triangle.vertices]
            if (points[1]-points[0]).cross(points[2]-points[0]).length/2<1e-12:
                raise RuntimeError("Degenerate original carbine triangle before export: "+obj.name)
    A.export("PulseCarbine")
    print("FORGEBORNE_GLOVES",gloves,"TOTAL_TRIANGLES",triangles,flush=True)


if __name__=="__main__":
    sys.path.insert(0,str(Path(__file__).resolve().parent))
    import build_visual_assets as authoring
    assert bpy.app.version[:3]==(4,5,15)
    build_refined_carbine(authoring)
