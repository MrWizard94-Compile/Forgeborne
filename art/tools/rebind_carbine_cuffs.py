"""Rebind the existing original sleeve welts without regenerating geometry.

Run in pinned Blender with the existing PulseCarbine.blend loaded. Rubber is
exclusive to the two disconnected cuff welts in each joined glove mesh.
The normal generator now applies this binding directly.
"""
import json
from pathlib import Path
import sys
import bpy

sys.path.insert(0, str(Path(__file__).resolve().parent))
import build_visual_assets as authoring

assert bpy.app.version[:3] == (4, 5, 15)
manifest = json.loads((authoring.SOURCE / 'PulseCarbine.json').read_text())
reports = []
for side in ('Left', 'Right'):
    hand = bpy.data.objects[side + 'HandSkinned']
    mesh = hand.data
    rubber = {i for i, material in enumerate(mesh.materials) if material.name == 'Rubber'}
    assert len(rubber) == 1
    cuff = {index for face in mesh.polygons if face.material_index in rubber for index in face.vertices}
    assert cuff and all(face.material_index in rubber for face in mesh.polygons if any(i in cuff for i in face.vertices))
    forearm = hand.vertex_groups[side + 'Forearm']
    changed = 0
    for index in sorted(cuff):
        vertex = mesh.vertices[index]
        if len(vertex.groups) == 1 and vertex.groups[0].group == forearm.index and vertex.groups[0].weight == 1:
            continue
        assert len(vertex.groups) == 1 and hand.vertex_groups[vertex.groups[0].group].name == side + 'Palm'
        assert vertex.groups[0].weight == 1
        for group in hand.vertex_groups:
            group.remove([index])
        forearm.add([index], 1, 'REPLACE')
        changed += 1
    reports.append(dict(side=side, cuffVertices=len(cuff), reboundFromPalmToForearm=changed,
                        geometryAndMaterialIndicesEdited=False))

# Export records the declared palette. Blender may purge its unused roles;
# existing mesh material slots are preserved and need no recreated materials.
authoring.MATERIALS.update({name: bpy.data.materials.get(name) for name in manifest['materials']})
authoring.export('PulseCarbine')
print('FORGEBORNE_CUFF_BINDING', json.dumps(reports), flush=True)
