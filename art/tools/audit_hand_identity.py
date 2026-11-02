"""Read-only semantic fingerprints of original glove meshes and rest rigs."""
import bpy,json,hashlib,argparse,sys

parser=argparse.ArgumentParser(description=__doc__)
parser.add_argument('--detail-output')
args=parser.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])

bpy.context.view_layer.update()
rows=[]
details={}
for side in ('Left','Right'):
    mesh=bpy.data.objects[side+'HandSkinned']
    rig=bpy.data.objects[side+'HandRig']
    groups={g.index:g.name for g in mesh.vertex_groups}
    data={
        'positions':[list(mesh.matrix_world@v.co) for v in mesh.data.vertices],
        'faces':[list(p.vertices) for p in mesh.data.polygons],
        'materialRoles':[mesh.data.materials[p.material_index].name for p in mesh.data.polygons],
        'weights':[sorted((groups[g.group],g.weight) for g in v.groups) for v in mesh.data.vertices],
        'bones':[{'name':b.name,'parent':b.parent.name if b.parent else None,
                  'head':list(b.head_local),'tail':list(b.tail_local),
                  'matrix':[list(row) for row in b.matrix_local]} for b in rig.data.bones]
    }
    # Blender may reorder trim polygons. Preserve winding and indexed vertex
    # identity while normalizing face-list order and each polygon's start corner.
    oriented=[]
    for face,role in zip(data['faces'],data['materialRoles']):
        cycle=min(tuple(face[i:]+face[:i]) for i in range(len(face)))
        oriented.append((cycle,role))
    semantic={key:value for key,value in data.items() if key not in ('faces','materialRoles')}
    semantic['orientedTopologyAndRoles']=sorted(oriented)
    canonical=json.dumps(semantic,sort_keys=True,separators=(',',':'),allow_nan=False).encode()
    details[side]=data
    rows.append({'side':side,'vertices':len(mesh.data.vertices),'bones':len(rig.data.bones),
                 'attributeSha256':{key:hashlib.sha256(json.dumps(value,sort_keys=True,separators=(',',':'),allow_nan=False).encode()).hexdigest() for key,value in data.items()},
                 'orientedGeometryMaterialBindingAndRestSha256':hashlib.sha256(canonical).hexdigest()})
if args.detail_output:
 from pathlib import Path
 Path(args.detail_output).write_text(json.dumps(details,separators=(',',':'),allow_nan=False),encoding='utf-8')
print('FORGEBORNE_HAND_IDENTITY '+json.dumps({'blender':bpy.app.version_string,'hands':rows,
    'scope':'Source rest geometry, topology, material roles, weights and rig; not motion, visible pixels or director acceptance.'}))
