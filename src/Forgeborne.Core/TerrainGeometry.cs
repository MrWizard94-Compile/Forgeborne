using System.Numerics;

namespace Forgeborne.Core;

public readonly record struct SurfaceRayHit(float Distance, Vector3 Position, Vector3 Normal,
    int X, int Y, int Z, BlockType Type);
public readonly record struct SurfaceSweep(bool Hit, float Fraction, Vector3 Normal);
/// <summary>Immutable render region. Physical queries belong to its complete owning TerrainGeometry.</summary>
public sealed class TerrainRegion
{
    public int Id { get; }
    internal TerrainGeometry Geometry { get; }
    internal TerrainRegion(int id,TerrainGeometry geometry){Id=id;Geometry=geometry;}
    public int TriangleCount=>Geometry.TriangleCount;
    public bool HasPowerSensitiveMaterials=>Geometry.HasPowerSensitiveMaterials;
    public MeshData BuildMesh(Func<int,int,int,CellPowerState>? powerState=null)=>Geometry.BuildMesh(powerState);
    public (float,float,float,float)[] BuildColors(Func<int,int,int,CellPowerState> powerState)=>Geometry.BuildColors(powerState);
}

internal readonly record struct TerrainFace(Vector3 A, Vector3 B, Vector3 C,
    Vector3 NA, Vector3 NB, Vector3 NC, int X, int Y, int Z, BlockType Type)
{
    public Vector3 Min => Vector3.Min(A,Vector3.Min(B,C));
    public Vector3 Max => Vector3.Max(A,Vector3.Max(B,C));
    public Vector3 Normal => Vector3.Normalize(Vector3.Cross(B-A,C-A));
    public Vector3 Centre => (A+B+C)/3;
}

/// <summary>
/// Immutable triangle surface and BVH. Rendering receives copies of these exact
/// vertices; contact, swept body motion and aim query the same triangle snapshot.
/// No independently smoothed collider, mutable view state or Simulation exists here.
/// </summary>
public sealed class TerrainGeometry
{
    private readonly TerrainFace[] _faces;
    private readonly int[] _order;
    private readonly Node[] _nodes;
    private readonly TerrainRegion[]? _parts;
    private readonly TerrainGeometry[]? _activeParts;
    private readonly int _triangleCount;
    private readonly Vector3 _min,_max;
    private readonly TerrainFieldSamples? _field;
    public IReadOnlyList<TerrainRegion> Regions { get; }
    public bool HasPowerSensitiveMaterials { get; }
    private const float Epsilon=0.000001f;
    private readonly struct Node
    {
        public readonly Vector3 Min,Max;
        public readonly int Start,Count,Left,Right;
        public Node(Vector3 min,Vector3 max,int start,int count,int left,int right)
        {Min=min;Max=max;Start=start;Count=count;Left=left;Right=right;}
    }
    private sealed class FaceComparer : IComparer<int>
    {
        private readonly Vector3[] _centres;
        private readonly int _axis;
        public FaceComparer(Vector3[] centres,int axis){_centres=centres;_axis=axis;}
        public int Compare(int a,int b)
        {
            int comparison=Component(_centres[a],_axis).CompareTo(Component(_centres[b],_axis));
            return comparison!=0 ? comparison : a.CompareTo(b);
        }
    }
    internal TerrainGeometry(List<TerrainFace> faces,TerrainFieldSamples? field=null)
    {
        _field=field;
        _faces=faces.ToArray();_order=Enumerable.Range(0,_faces.Length).ToArray();
        _triangleCount=_faces.Length;Regions=Array.Empty<TerrainRegion>();
        HasPowerSensitiveMaterials=_faces.Any(f=>f.Type==BlockType.Cable || f.Type==BlockType.KineticMill
            || f.Type==BlockType.Workstation || f.Type==BlockType.ShockPylon);
        if(_faces.Length==0){_nodes=Array.Empty<Node>();return;}
        var nodes=new List<Node>();
        var minimums=new Vector3[_faces.Length];var maximums=new Vector3[_faces.Length];var centres=new Vector3[_faces.Length];
        for(int i=0;i<_faces.Length;i++){minimums[i]=_faces[i].Min;maximums[i]=_faces[i].Max;centres[i]=_faces[i].Centre;}
        var comparers=new[]{new FaceComparer(centres,0),new FaceComparer(centres,1),new FaceComparer(centres,2)};
        int Build(int start,int count)
        {
            var min=new Vector3(float.PositiveInfinity);var max=new Vector3(float.NegativeInfinity);
            for(int i=start;i<start+count;i++){int at=_order[i];min=Vector3.Min(min,minimums[at]);max=Vector3.Max(max,maximums[at]);}
            int index=nodes.Count;nodes.Add(default);
            if(count<=8){nodes[index]=new Node(min,max,start,count,-1,-1);return index;}
            var size=max-min;int axis=size.X>=size.Y && size.X>=size.Z ? 0 : size.Y>=size.Z ? 1 : 2;
            Array.Sort(_order,start,count,comparers[axis]);
            int half=count/2,left=Build(start,half),right=Build(start+half,count-half);
            nodes[index]=new Node(min,max,0,0,left,right);return index;
        }
        Build(0,_faces.Length);_nodes=nodes.ToArray();_min=_nodes[0].Min;_max=_nodes[0].Max;
    }
    internal TerrainGeometry(TerrainRegion[] parts)
    {
        _parts=(TerrainRegion[])parts.Clone();Regions=Array.AsReadOnly(_parts);
        _activeParts=_parts.Where(p=>p.Geometry.TriangleCount>0).Select(p=>p.Geometry).ToArray();
        _faces=Array.Empty<TerrainFace>();_order=Array.Empty<int>();_nodes=Array.Empty<Node>();
        _min=new Vector3(float.PositiveInfinity);_max=new Vector3(float.NegativeInfinity);
        foreach(var part in _activeParts)
        {_triangleCount+=part.TriangleCount;_min=Vector3.Min(_min,part._min);_max=Vector3.Max(_max,part._max);
            HasPowerSensitiveMaterials|=part.HasPowerSensitiveMaterials;}
    }
    public int TriangleCount => _triangleCount;

    public (float,float,float,float)[] BuildColors(Func<int,int,int,CellPowerState> powerState)
    {
        if(powerState==null)throw new ArgumentNullException(nameof(powerState));
        var colours=new (float,float,float,float)[TriangleCount*3];int at=0;
        void Append(TerrainGeometry part)
        {
            if(part._activeParts!=null){foreach(var child in part._activeParts)Append(child);return;}
            foreach(var face in part._faces)
            {
                var colour=PowerTint.ColorFor(powerState(face.X,face.Y,face.Z));
                colours[at++]=colour;colours[at++]=colour;colours[at++]=colour;
            }
        }
        Append(this);return colours;
    }

    public MeshData BuildMesh(Func<int,int,int,CellPowerState>? powerState=null)
    {
        var mesh=new MeshData();
        AppendMesh(mesh,powerState);
        return mesh;
    }
    private void AppendMesh(MeshData mesh,Func<int,int,int,CellPowerState>? powerState)
    {
        if(_activeParts!=null){foreach(var part in _activeParts)part.AppendMesh(mesh,powerState);return;}
        foreach(var face in _faces)
        {
            var colour=powerState==null ? (1f,1f,1f,1f) : PowerTint.ColorFor(powerState(face.X,face.Y,face.Z));
            void Vertex(Vector3 position,Vector3 normal)
            {
                mesh.Indices.Add(mesh.Vertices.Count);
                mesh.Vertices.Add((position.X,position.Y,position.Z));
                mesh.Normals.Add((normal.X,normal.Y,normal.Z));
                mesh.Uvs.Add((position.X,position.Z));mesh.Colors.Add(colour);mesh.BlockTypes.Add(face.Type);
            }
            Vertex(face.A,face.NA);Vertex(face.B,face.NB);Vertex(face.C,face.NC);
        }
    }

    public bool Raycast(Vector3 origin,Vector3 direction,float reach,out SurfaceRayHit hit)
    {
        Validate(origin);Validate(direction);
        if(!Finite(reach) || reach<0) throw new ArgumentOutOfRangeException(nameof(reach));
        float length=direction.Length();
        hit=default;
        if(length<Epsilon || reach==0 || TriangleCount==0) return false;
        direction/=length;
        return RaycastUnit(origin,direction,reach,out hit);
    }
    private bool RaycastUnit(Vector3 origin,Vector3 direction,float reach,out SurfaceRayHit hit)
    {
        hit=default;
        if(_activeParts!=null)
        {
            bool foundPart=false;float nearestPart=reach;
            foreach(var part in _activeParts)
                if(RayBounds(origin,direction,part._min,part._max,nearestPart) && part.RaycastUnit(origin,direction,nearestPart,out var candidate))
                {hit=candidate;nearestPart=hit.Distance;foundPart=true;}
            return foundPart;
        }
        if(_nodes.Length==0) return false;
        Span<int> stack=stackalloc int[64];int top=0;stack[top++]=0;
        int nearest=-1;float distance=reach;
        while(top>0)
        {
            var node=_nodes[stack[--top]];
            if(!RayBounds(origin,direction,node.Min,node.Max,distance)) continue;
            if(node.Count==0){stack[top++]=node.Left;stack[top++]=node.Right;continue;}
            for(int i=node.Start;i<node.Start+node.Count;i++)
            {
                int index=_order[i];var face=_faces[index];
                if(RayTriangle(origin,direction,face,out float value) && value<=distance)
                {distance=value;nearest=index;}
            }
        }
        if(nearest<0)return false;
        var found=_faces[nearest];
        hit=new SurfaceRayHit(distance,origin+direction*distance,found.Normal,found.X,found.Y,found.Z,found.Type);
        return true;
    }
    public bool Contains(Vector3 point)
    {
        Validate(point);
        if(_parts!=null)
        {
            int region=TerrainMesher.RegionAt(point);
            return region>=0 && _parts[region].Geometry.Contains(point);
        }
        if(_field!=null)return _field.Contains(point);
        // For a closed outward-oriented surface, the first crossing from an
        // interior point faces along the ray. This avoids parity double counts
        // at a triangulation edge and allocates no per-query hit collection.
        var direction=Vector3.Normalize(new Vector3(0.87913f,0.31971f,0.35337f));
        return RaycastUnit(point,direction,128,out var hit) && hit.Distance>Epsilon
            && Vector3.Dot(hit.Normal,direction)>0;
    }
    public bool OverlapsBody(Vector3 feet,float halfWidth=FpsController.HalfWidth,float height=FpsController.Height)
    {
        ValidateBody(feet,halfWidth,height);
        var centre=feet+new Vector3(0,height/2,0);var half=new Vector3(halfWidth,height/2,halfWidth);
        if(Contains(centre))return true;
        if(_activeParts!=null)
        {
            foreach(var part in _activeParts)
                if(BoundsOverlap(centre-half,centre+half,part._min,part._max) && part.OverlapsSurface(centre,half))return true;
            return false;
        }
        return OverlapsSurface(centre,half);
    }
    private bool OverlapsSurface(Vector3 centre,Vector3 half)
    {
        if(_nodes.Length==0)return false;
        Span<int> stack=stackalloc int[64];int top=0;stack[top++]=0;
        while(top>0)
        {
            var node=_nodes[stack[--top]];
            if(!BoundsOverlap(centre-half,centre+half,node.Min,node.Max))continue;
            if(node.Count==0){stack[top++]=node.Left;stack[top++]=node.Right;continue;}
            for(int i=node.Start;i<node.Start+node.Count;i++)
                if(TriangleSweep(_faces[_order[i]],centre,half,Vector3.Zero,out _,true))return true;
        }
        return false;
    }
    public SurfaceSweep SweepBody(Vector3 feet,Vector3 displacement,
        float halfWidth=FpsController.HalfWidth,float height=FpsController.Height)
    {
        ValidateBody(feet,halfWidth,height);Validate(displacement);
        if(TriangleCount==0 || displacement.LengthSquared()<1e-16f) return new(false,1,Vector3.Zero);
        var centre=feet+new Vector3(0,height/2,0);var half=new Vector3(halfWidth,height/2,halfWidth);
        if(Contains(centre))return new(true,0,-Vector3.Normalize(displacement));
        var min=Vector3.Min(centre,centre+displacement)-half;
        var max=Vector3.Max(centre,centre+displacement)+half;
        if(_activeParts!=null)
        {
            var nearest=new SurfaceSweep(false,1,Vector3.Zero);
            foreach(var part in _activeParts)
            {
                if(!BoundsOverlap(min,max,part._min,part._max))continue;
                var candidate=part.SweepTriangles(centre,half,displacement,min,max);
                if(candidate.Hit && candidate.Fraction<=nearest.Fraction)nearest=candidate;
            }
            return nearest;
        }
        return SweepTriangles(centre,half,displacement,min,max);
    }
    private SurfaceSweep SweepTriangles(Vector3 centre,Vector3 half,Vector3 displacement,Vector3 min,Vector3 max)
    {
        if(_nodes.Length==0)return new(false,1,Vector3.Zero);
        Span<int> stack=stackalloc int[64];int top=0;stack[top++]=0;
        float first=1;bool found=false;var normal=Vector3.Zero;
        while(top>0)
        {
            var node=_nodes[stack[--top]];
            if(!BoundsOverlap(min,max,node.Min,node.Max))continue;
            if(node.Count==0){stack[top++]=node.Left;stack[top++]=node.Right;continue;}
            for(int i=node.Start;i<node.Start+node.Count;i++)
            {
                var face=_faces[_order[i]];
                if(TriangleSweep(face,centre,half,displacement,out float time,false) && time<=first)
                {first=time;normal=face.Normal;found=true;}
            }
        }
        return new(found,first,normal);
    }
    private static bool TriangleSweep(TerrainFace face,Vector3 centre,Vector3 half,Vector3 velocity,out float time,bool overlapOnly)
    {
        float entry=float.NegativeInfinity,exit=float.PositiveInfinity;
        bool Axis(Vector3 axis)
        {
            if(axis.LengthSquared()<1e-14f)return true;
            float a=Vector3.Dot(face.A,axis),b=Vector3.Dot(face.B,axis),c=Vector3.Dot(face.C,axis);
            float low=Math.Min(a,Math.Min(b,c)),high=Math.Max(a,Math.Max(b,c));
            float radius=Vector3.Dot(half,Vector3.Abs(axis)),position=Vector3.Dot(centre,axis);
            float speed=Vector3.Dot(velocity,axis);
            // Parallel contact is not penetration: a body may slide on a floor.
            if(Math.Abs(speed)<1e-9f) return high>position-radius+Epsilon && low<position+radius-Epsilon;
            float start=(low-radius-position)/speed,end=(high+radius-position)/speed;
            if(start>end){float swap=start;start=end;end=swap;}
            entry=Math.Max(entry,start);exit=Math.Min(exit,end);
            return entry<=exit+Epsilon;
        }
        var e0=face.B-face.A;var e1=face.C-face.B;var e2=face.A-face.C;
        time=0;
        if(!Axis(Vector3.UnitX)||!Axis(Vector3.UnitY)||!Axis(Vector3.UnitZ)||!Axis(Vector3.Cross(e0,e1)))return false;
        if(!Axis(Vector3.Cross(e0,Vector3.UnitX))||!Axis(Vector3.Cross(e0,Vector3.UnitY))||!Axis(Vector3.Cross(e0,Vector3.UnitZ))
            ||!Axis(Vector3.Cross(e1,Vector3.UnitX))||!Axis(Vector3.Cross(e1,Vector3.UnitY))||!Axis(Vector3.Cross(e1,Vector3.UnitZ))
            ||!Axis(Vector3.Cross(e2,Vector3.UnitX))||!Axis(Vector3.Cross(e2,Vector3.UnitY))||!Axis(Vector3.Cross(e2,Vector3.UnitZ)))return false;
        if(overlapOnly)return true;
        if(exit<=Epsilon || entry>1+Epsilon || exit<Math.Max(0,entry)-Epsilon)return false;
        time=Math.Max(0,Math.Min(1,entry));return true;
    }
    private static bool RayTriangle(Vector3 origin,Vector3 direction,TerrainFace face,out float distance)
    {
        distance=0;var edge0=face.B-face.A;var edge1=face.C-face.A;
        var perpendicular=Vector3.Cross(direction,edge1);float determinant=Vector3.Dot(edge0,perpendicular);
        if(Math.Abs(determinant)<1e-9f)return false;
        var offset=origin-face.A;float u=Vector3.Dot(offset,perpendicular)/determinant;
        if(u < -Epsilon || u>1+Epsilon)return false;
        var q=Vector3.Cross(offset,edge0);float v=Vector3.Dot(direction,q)/determinant;
        if(v < -Epsilon || u+v>1+Epsilon)return false;
        distance=Vector3.Dot(edge1,q)/determinant;
        return distance>=0;
    }
    private static bool RayBounds(Vector3 origin,Vector3 direction,Vector3 min,Vector3 max,float reach)
    {
        float start=0,end=reach;
        for(int axis=0;axis<3;axis++)
        {
            float value=Component(origin,axis),delta=Component(direction,axis);
            if(Math.Abs(delta)<1e-9f){if(value<Component(min,axis)||value>Component(max,axis))return false;continue;}
            float a=(Component(min,axis)-value)/delta,b=(Component(max,axis)-value)/delta;
            if(a>b){float swap=a;a=b;b=swap;}
            start=Math.Max(start,a);end=Math.Min(end,b);if(start>end+Epsilon)return false;
        }
        return true;
    }
    private static bool BoundsOverlap(Vector3 amin,Vector3 amax,Vector3 bmin,Vector3 bmax) =>
        amin.X<=bmax.X && amax.X>=bmin.X && amin.Y<=bmax.Y && amax.Y>=bmin.Y && amin.Z<=bmax.Z && amax.Z>=bmin.Z;
    private static float Component(Vector3 value,int axis)=>axis==0 ? value.X : axis==1 ? value.Y : value.Z;
    private static bool Finite(float value)=>!float.IsNaN(value)&&!float.IsInfinity(value);
    private static void Validate(Vector3 value)
    {
        if(!Finite(value.X)||!Finite(value.Y)||!Finite(value.Z))throw new ArgumentException("Geometry coordinates must be finite.");
    }
    private static void ValidateBody(Vector3 feet,float halfWidth,float height)
    {
        Validate(feet);
        if(!Finite(halfWidth)||!Finite(height)||halfWidth<=0||height<=0)throw new ArgumentOutOfRangeException(nameof(halfWidth));
    }
}
