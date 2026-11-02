"""Original bounded 3D fracture cells for Forgeborne's Blender authoring.

Plane intersections use Python double precision. The runtime receives only the
validated closed meshes; this tool never runs in a player or reads save data.
"""
from itertools import combinations
import math

# Core rejects cross-product squared length below 1e-14. Final export repeats
# that exact 3D check after coordinate quantization; do not substitute a larger
# projected-area margin for the actual retained runtime contract.
MIN_CROSS=1e-7


def dot(a,b):
    return math.fsum(x*y for x,y in zip(a,b))


def cross(a,b):
    return (a[1]*b[2]-a[2]*b[1],a[2]*b[0]-a[0]*b[2],a[0]*b[1]-a[1]*b[0])


def intersection(a,b,c):
    na,nb,nc=a[:3],b[:3],c[:3];bc=cross(nb,nc);det=dot(na,bc)
    if abs(det)<1e-9:return None
    ca,ab=cross(nc,na),cross(na,nb)
    return tuple((a[3]*bc[i]+b[3]*ca[i]+c[3]*ab[i])/det for i in range(3))


def clipped_cells(bounds,seed):
    x0,x1,z0,z1=bounds
    if not (0<x1-x0<=8 and 0<z1-z0<=8 and (x1-x0)*(z1-z0)<=32):
        raise ValueError("Fracture authoring capacity exceeded")
    centres={(x,z):(x+.5+.12*math.sin(seed+x*17+z*11),
                     4.35+.16*math.sin(seed+x*13+z*7),
                     z+.5+.12*math.cos(seed+x*7-z*13))
             for z in range(z0,z1) for x in range(x0,x1)}
    bounds_planes=[(-1,0,0,-x0),(1,0,0,x1),(0,-1,0,-4),(0,1,0,5),
                   (0,0,-1,-z0),(0,0,1,z1)]
    cells={};all_planes=list(bounds_planes)
    for owner,a in centres.items():
        planes=list(bounds_planes)
        for other,b in centres.items():
            if owner==other:continue
            normal=tuple(b[i]-a[i] for i in range(3));length=math.sqrt(dot(normal,normal))
            distance=(dot(b,b)-dot(a,a))/2
            plane=tuple(n/length for n in normal)+(distance/length,)
            planes.append(plane)
            if owner<other:all_planes.append(plane)
        vertices=[]
        for triple in combinations(planes,3):
            point=intersection(*triple)
            if point is None or any(dot(p[:3],point)>p[3]+1e-7 for p in planes):continue
            if not any(math.dist(point,v)<3e-7 for v in vertices):vertices.append(point)
        faces=[]
        for plane in planes:
            indices=[i for i,p in enumerate(vertices) if abs(dot(plane[:3],p)-plane[3])<2e-6]
            if len(indices)<3:continue
            centre=tuple(math.fsum(vertices[i][axis] for i in indices)/len(indices) for axis in range(3))
            n=plane[:3];u=cross(n,(1,0,0) if abs(n[0])<.8 else (0,1,0));length=math.sqrt(dot(u,u))
            u=tuple(x/length for x in u);v=cross(n,u)
            def angle(index):
                p=tuple(vertices[index][i]-centre[i] for i in range(3))
                return math.atan2(dot(p,v),dot(p,u))
            faces.append(tuple(sorted(indices,key=angle)))
        if len(vertices)<4 or len(faces)<4:raise RuntimeError("Fracture cell is not a closed solid")
        cells[owner]=(vertices,faces,planes)
    return cells,all_planes


def canonical_point(point,planes):
    near=[p for p in planes if abs(dot(p[:3],point)-p[3])<.0005]
    if not near:return point
    for triple in combinations(near,3):
        candidate=intersection(*triple)
        if candidate is not None and math.dist(candidate,point)<.002:return candidate
    a=near[0];error=dot(a[:3],point)-a[3]
    result=tuple(point[i]-a[i]*error for i in range(3))
    for b in near[1:]:
        cosine=dot(a[:3],b[:3]);det=1-cosine*cosine
        if det<.0025:continue
        other=dot(b[:3],point)-b[3]
        ca,cb=(error-cosine*other)/det,(other-cosine*error)/det
        candidate=tuple(point[i]-a[i]*ca-b[i]*cb for i in range(3))
        if math.dist(candidate,point)<.002:return candidate
    return result


def triangulate(vertices,face,normal):
    """Triangulate a simple planar cap while retaining collinear boundary nodes.

    An ear may not skip another boundary point, even one lying on its diagonal.
    This keeps the adjacent shell's exact edges paired without zero-area ears.
    """
    u=cross(normal,(1,0,0) if abs(normal[0])<.8 else (0,1,0));length=math.sqrt(dot(u,u))
    u=tuple(x/length for x in u);v=cross(normal,u);origin=vertices[face[0]]
    points={i:(dot(tuple(vertices[i][a]-origin[a] for a in range(3)),u),
               dot(tuple(vertices[i][a]-origin[a] for a in range(3)),v)) for i in face}
    def area(a,b,c):
        a,b,c=points[a],points[b],points[c]
        return (b[0]-a[0])*(c[1]-a[1])-(b[1]-a[1])*(c[0]-a[0])
    remaining=list(face);result=[]
    while len(remaining)>3:
        candidates=[]
        for index,b in enumerate(remaining):
            a,c=remaining[index-1],remaining[(index+1)%len(remaining)];size=area(a,b,c)
            if size<=MIN_CROSS:continue
            if any(area(a,b,p)>=-1e-10 and area(b,c,p)>=-1e-10 and area(c,a,p)>=-1e-10
                   for p in remaining if p not in (a,b,c)):continue
            after=[p for p in remaining if p!=b]
            if len(after)==3 and area(*after)<=MIN_CROSS:continue
            perimeter=sum(math.dist(points[p],points[q])**2 for p,q in ((a,b),(b,c),(c,a)))
            candidates.append((size/perimeter,index,(a,b,c)))
        if not candidates:return constrained_cap(points,face)
        _,index,triangle=max(candidates);result.append(triangle);remaining.pop(index)
    if area(*remaining)<=MIN_CROSS:raise RuntimeError("Degenerate final fracture cap ear")
    return result+[tuple(remaining)]


def constrained_cap(points,face):
    """Native CDT fallback for Boolean caps with touching contour bridges.

    Keep the author's exact 3D vertices and verify oriented boundary coverage.
    No Steiner point, omitted sliver or unpaired shell edge is accepted.
    """
    from mathutils import Vector
    from mathutils.geometry import delaunay_2d_cdt
    unique=list(dict.fromkeys(face));lookup={v:i for i,v in enumerate(unique)}
    edges=sorted({tuple(sorted((lookup[a],lookup[b]))) for a,b in zip(face,face[1:]+face[:1]) if a!=b})
    result=delaunay_2d_cdt([Vector(points[i]) for i in unique],edges,[],0,1e-10,True)
    mapping=[]
    for origins in result[3]:
        if not origins:raise RuntimeError("Fracture cap requires an unexpected new intersection vertex")
        originals=[unique[i] for i in origins]
        if any(math.dist(points[originals[0]],points[i])>1e-9 for i in originals):
            raise RuntimeError("Fracture cap triangulation merged distinct boundary vertices")
        mapping.append(originals[0])
    def inside(p):
        found=False
        for a,b in zip(face,face[1:]+face[:1]):
            a,b=points[a],points[b]
            if (a[1]>p[1])!=(b[1]>p[1]) and p[0]<(b[0]-a[0])*(p[1]-a[1])/(b[1]-a[1])+a[0]:found=not found
        return found
    triangles=[]
    for polygon in result[2]:
        if len(polygon)!=3:raise RuntimeError("Native CDT did not return triangles")
        triangle=tuple(mapping[i] for i in polygon)
        centre=tuple(math.fsum(points[i][axis] for i in triangle)/3 for axis in range(2))
        if not inside(centre):continue
        a,b,c=[points[i] for i in triangle]
        area=(b[0]-a[0])*(c[1]-a[1])-(b[1]-a[1])*(c[0]-a[0])
        if abs(area)<=MIN_CROSS:raise RuntimeError("Native CDT retained a sub-contract cap sliver: "+str(area))
        if area<0:triangle=triangle[0],triangle[2],triangle[1]
        triangles.append(triangle)
    def boundary(polygons):
        edges={}
        for polygon in polygons:
            for a,b in zip(polygon,polygon[1:]+polygon[:1]):
                edge=(min(a,b),max(a,b));edges[edge]=edges.get(edge,0)+(1 if a<b else -1)
        return {edge:count for edge,count in edges.items() if count}
    if not triangles or boundary(triangles)!=boundary([face]):
        raise RuntimeError("Native cap triangulation changed the closed shell boundary")
    return triangles
