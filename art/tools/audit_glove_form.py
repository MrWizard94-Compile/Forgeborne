"""Read-only source geometry, closed bodies and fitted stitch binding audit."""
import argparse
import hashlib
import json
from pathlib import Path
import sys
import bpy
import bmesh
from mathutils.bvhtree import BVHTree
from mathutils.kdtree import KDTree

parser=argparse.ArgumentParser(description=__doc__)
parser.add_argument('--output',required=True)
parser.add_argument('--require-fitted-stitches',action='store_true')
parser.add_argument('--require-dorsal-panels',action='store_true')
args=parser.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
assert bpy.app.version[:3]==(4,5,15)
bpy.context.view_layer.update()

def digest(value):
    return hashlib.sha256(json.dumps(value,sort_keys=True,separators=(',',':'),allow_nan=False).encode()).hexdigest()

weapon=[]
for name in ('FrameMesh','MagazineMesh','ChargingHandleMesh'):
    obj=bpy.data.objects[name];faces=[]
    for face in obj.data.polygons:
        points=[tuple(round(v*10_000_000) for v in obj.matrix_world@obj.data.vertices[i].co) for i in face.vertices]
        cycle=min(tuple(points[i:]+points[:i]) for i in range(len(points)))
        faces.append((cycle,obj.data.materials[face.material_index].name))
    weapon.append(dict(mesh=name,triangles=sum(len(p.vertices)-2 for p in obj.data.polygons),
                       orientedSurfaceAndRoleSha256=digest(sorted(faces)),positionQuantumMetres=1e-7))

hands=[]
for side in ('Left','Right'):
    obj=bpy.data.objects[side+'HandSkinned'];mesh=obj.data
    adjacency=[[] for _ in mesh.vertices]
    for edge in mesh.edges:
        a,b=edge.vertices;adjacency[a].append(b);adjacency[b].append(a)
    unseen=set(range(len(mesh.vertices)));components=[]
    while unseen:
        stack=[next(iter(unseen))];component=set()
        while stack:
            index=stack.pop()
            if index not in unseen:continue
            unseen.remove(index);component.add(index);stack.extend(adjacency[index])
        components.append(component)
    primary=max(components,key=len)
    bm=bmesh.new();bm.from_mesh(mesh)
    open_edges=sum(not edge.is_manifold for edge in bm.edges);bm.free()
    assert open_edges==0 and primary
    primary_roles={mesh.materials[face.material_index].name for face in mesh.polygons if face.vertices[0] in primary}
    assert primary_roles=={'GloveFabric','SleeveFabric'}
    polygons=[list(face.vertices) for face in mesh.polygons if face.vertices[0] in primary]
    bvh=BVHTree.FromPolygons([vertex.co for vertex in mesh.vertices],polygons)
    tree=KDTree(len(primary))
    for index in sorted(primary):tree.insert(mesh.vertices[index].co,index)
    tree.balance()
    def weights(index):
        return {obj.vertex_groups[w.group].name:w.weight for w in mesh.vertices[index].groups if w.weight>0}
    primary_joints={name for index in primary for name in weights(index)}
    assert len(primary_joints)==17 and all(name.startswith(side) for name in primary_joints)
    maximum_influences=0;maximum_sum_error=0
    for vertex in mesh.vertices:
        values=weights(vertex.index)
        assert values and all(name.startswith(side) for name in values)
        maximum_influences=max(maximum_influences,len(values));maximum_sum_error=max(maximum_sum_error,abs(sum(values.values())-1))
    assert maximum_influences<=4 and maximum_sum_error<.00001
    stitches={index for face in mesh.polygons if mesh.materials[face.material_index].name=='GloveStitch' for index in face.vertices}
    maximum_gap=0;maximum_weight_error=0
    for index in sorted(stitches):
        point=mesh.vertices[index].co
        hit=bvh.find_nearest(point);assert hit[0] is not None
        maximum_gap=max(maximum_gap,hit[3])
        _,nearest,_=tree.find(point);a,b=weights(index),weights(nearest)
        maximum_weight_error=max(maximum_weight_error,sum(abs(a.get(name,0)-b.get(name,0)) for name in set(a)|set(b)))
    if args.require_fitted_stitches:
        assert stitches and maximum_gap<.0015 and maximum_weight_error<.000002,(side,maximum_gap,maximum_weight_error)
    rig=bpy.data.objects[side+'HandRig']
    rig_to_skin=obj.matrix_world.inverted()@rig.matrix_world
    forearm=rig.data.bones[side+'Forearm']
    wrist=rig_to_skin@forearm.tail_local
    direction=(wrist-rig_to_skin@forearm.head_local).normalized()
    primary_faces=[face for face in mesh.polygons if face.vertices[0] in primary]
    panel_faces={face.index for face in primary_faces
        if mesh.materials[face.material_index].name=='SleeveFabric' and (face.center-wrist).dot(direction)>.008}
    edge_faces={}
    for face in primary_faces:
        indices=list(face.vertices)
        for a,b in zip(indices,indices[1:]+indices[:1]):edge_faces.setdefault(tuple(sorted((a,b))),[]).append(face.index)
    panel_adjacency={index:set() for index in panel_faces};border=[]
    for edge,faces in edge_faces.items():
        selected=set(faces)&panel_faces
        for index in selected:panel_adjacency[index].update(selected-{index})
        if selected and len(selected)==1 and len(faces)==2:
            roles={mesh.materials[mesh.polygons[index].material_index].name for index in faces}
            if roles=={'GloveFabric','SleeveFabric'}:border.append(edge)
    unseen_faces=set(panel_faces);panel_components=0
    while unseen_faces:
        panel_components+=1;todo=[next(iter(unseen_faces))]
        while todo:
            index=todo.pop()
            if index not in unseen_faces:continue
            unseen_faces.remove(index);todo.extend(panel_adjacency[index])
    maximum_border_gap=None
    if border:
        segments=[(mesh.vertices[a].co,mesh.vertices[b].co) for a,b in border]
        maximum_border_gap=0
        for index in stitches:
            point=mesh.vertices[index].co;nearest=float('inf')
            for a,b in segments:
                delta=b-a;assert delta.length_squared>0
                t=max(0,min(1,(point-a).dot(delta)/delta.length_squared))
                nearest=min(nearest,(point-a-delta*t).length)
            maximum_border_gap=max(maximum_border_gap,nearest)
    if args.require_dorsal_panels:
        assert panel_faces and panel_components==1 and border,(side,len(panel_faces),panel_components)
        assert maximum_border_gap is not None and maximum_border_gap<.0015,(side,maximum_border_gap)
    hands.append(dict(side=side,vertices=len(mesh.vertices),triangles=sum(len(p.vertices)-2 for p in mesh.polygons),
        rigObjectWorldMatrix=[list(row) for row in rig.matrix_world],
        sourceComponentsIncludingTrim=len(components),closedPrimaryBodyVertices=len(primary),nonmanifoldEdges=open_edges,
        primaryBodyMaterialRoles=sorted(primary_roles),primaryBodyActiveJoints=len(primary_joints),
        maximumInfluences=maximum_influences,maximumWeightSumError=maximum_sum_error,stitchVertices=len(stitches),
        maximumStitchDistanceFromBodyMetres=maximum_gap,maximumStitchNearestSkinWeightL1Error=maximum_weight_error,
        dorsalFabricFaces=len(panel_faces),dorsalFabricComponents=panel_components,
        dorsalFabricAreaSquareMetres=sum(mesh.polygons[index].area for index in panel_faces),
        dorsalPanelLeatherBoundaryEdges=len(border),maximumStitchDistanceFromPanelBoundaryMetres=maximum_border_gap))

landmarks={name:list(bpy.data.objects['PulseCarbine'].matrix_world.inverted()@bpy.data.objects[name].matrix_world.translation)
    for name in ('MuzzleSocket','ArtRight','ArtUp','ArtForward','LeftPalmContact','RightPalmContact')}
report=dict(blender=bpy.app.version_string,weaponAssemblies=weapon,hands=hands,modelSpaceBlenderLandmarks=landmarks,
    fittedStitchContractRequired=args.require_fitted_stitches,
    dorsalPanelContractRequired=args.require_dorsal_panels,
    scope='Source topology/binding and quantized oriented weapon surfaces. No claim of natural anatomy, actual import, rendered skin, native controls or director acceptance.')
Path(args.output).write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print(json.dumps(report))
