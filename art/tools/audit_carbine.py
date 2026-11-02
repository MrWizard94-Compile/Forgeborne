import bpy,json,math
from pathlib import Path
rows=[]
for obj in bpy.context.scene.objects:
 if obj.type!='MESH':continue
 mesh=obj.data;mesh.calc_loop_triangles();zero=0;smallest=1e30
 for triangle in mesh.loop_triangles:
  points=[obj.matrix_world@mesh.vertices[i].co for i in triangle.vertices]
  area=(points[1]-points[0]).cross(points[2]-points[0]).length/2
  if area<1e-12:zero+=1
  smallest=min(smallest,area)
 rows.append({'mesh':obj.name,'vertices':len(mesh.vertices),'triangles':len(mesh.loop_triangles),'degenerate':zero,'smallestArea':smallest})
report={'blender':bpy.app.version_string,'meshes':rows,'triangles':sum(r['triangles'] for r in rows),'sourceOnlyNotRuntimeAcceptance':True}
(Path(__file__).resolve().parents[2]/'docs/evidence/2026-11-02/carbine-0127/blender-source-audit.json').write_text(json.dumps(report,indent=2)+'\n')
assert report['triangles']<100000 and all(row['degenerate']==0 for row in rows),report
print(json.dumps(report))
