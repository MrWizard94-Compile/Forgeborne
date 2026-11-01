using System.Numerics;

namespace Forgeborne.Core;

/// <summary>
/// Continuous material-field contouring. The initial profile reconstructs a
/// continuous field from canonical material samples; it does not draw cube faces.
/// A consistent six-tetrahedron split gives one closed, shared triangle surface.
/// This is geometry infrastructure, not the final authored terrain/art direction.
/// </summary>
internal static class TerrainMesher
{
    private const float Step=0.5f;
    private const float Origin=-0.5f;
    private const int Cells=(VoxelChunk.Size+1)*2;
    private const int RegionCells=8;
    private const int RegionsPerAxis=(Cells+RegionCells-1)/RegionCells;
    internal const int RegionCount=RegionsPerAxis*RegionsPerAxis*RegionsPerAxis;
    private static readonly int[,] Tetrahedra={{0,1,3,7},{0,1,5,7},{0,2,3,7},{0,2,6,7},{0,4,5,7},{0,4,6,7}};

    internal static void MarkAffected(bool[] dirty,int x,int y,int z)
    {
        int Low(int c)=>Math.Max(0,(int)Math.Floor((c-0.53f-Origin)/(RegionCells*Step)));
        int High(int c)=>Math.Min(RegionsPerAxis-1,(int)Math.Floor((c+1.53f-Origin)/(RegionCells*Step)));
        for(int ry=Low(y);ry<=High(y);ry++)for(int rz=Low(z);rz<=High(z);rz++)for(int rx=Low(x);rx<=High(x);rx++)
            dirty[(ry*RegionsPerAxis+rz)*RegionsPerAxis+rx]=true;
    }
    internal static TerrainGeometry Build(VoxelChunk chunk,TerrainGeometry? previous,bool[] dirty)
    {
        var regions=new TerrainRegion[RegionCount];
        for(int y=0;y<RegionsPerAxis;y++)for(int z=0;z<RegionsPerAxis;z++)for(int x=0;x<RegionsPerAxis;x++)
        {
            int id=(y*RegionsPerAxis+z)*RegionsPerAxis+x;
            regions[id]=previous!=null && !dirty[id] ? previous.Regions[id] : new TerrainRegion(id,BuildRegion(chunk,x,y,z));
        }
        return new TerrainGeometry(regions);
    }
    private static TerrainGeometry BuildRegion(VoxelChunk chunk,int rx,int ry,int rz)
    {
        int startX=rx*RegionCells,startY=ry*RegionCells,startZ=rz*RegionCells;
        int endX=Math.Min(startX+RegionCells,Cells),endY=Math.Min(startY+RegionCells,Cells),endZ=Math.Min(startZ+RegionCells,Cells);
        int nodeCountX=endX-startX+1,nodeCountY=endY-startY+1,nodeCountZ=endZ-startZ+1;
        var samples=new float[nodeCountX*nodeCountY*nodeCountZ];
        int Index(int x,int y,int z)=>((y-startY)*nodeCountZ+z-startZ)*nodeCountX+x-startX;
        for(int y=startY;y<=endY;y++)for(int z=startZ;z<=endZ;z++)for(int x=startX;x<=endX;x++)
            samples[Index(x,y,z)]=Density(chunk,new Vector3(Origin+x*Step,Origin+y*Step,Origin+z*Step));
        var faces=new List<TerrainFace>();
        Span<Vector3> corners=stackalloc Vector3[8];Span<float> densities=stackalloc float[8];
        Span<Vector3> points=stackalloc Vector3[4];Span<float> values=stackalloc float[4];
        for(int y=startY;y<endY;y++)
        for(int z=startZ;z<endZ;z++)
        for(int x=startX;x<endX;x++)
        {
            int inside=0;
            for(int c=0;c<8;c++)
            {
                int nx=x+(c&1),ny=y+((c>>1)&1),nz=z+((c>>2)&1);
                corners[c]=new Vector3(Origin+nx*Step,Origin+ny*Step,Origin+nz*Step);
                densities[c]=samples[Index(nx,ny,nz)];if(densities[c]>=0)inside++;
            }
            if(inside==0 || inside==8)continue;
            for(int t=0;t<6;t++)
            {
                for(int c=0;c<4;c++){int at=Tetrahedra[t,c];points[c]=corners[at];values[c]=densities[at];}
                Polygonise(chunk,faces,points,values);
            }
        }
        return new TerrainGeometry(faces,new TerrainFieldSamples(samples,nodeCountX,nodeCountY,nodeCountZ,
            new Vector3(Origin+startX*Step,Origin+startY*Step,Origin+startZ*Step),Step));
    }
    internal static int RegionAt(Vector3 position)
    {
        var p=(position-new Vector3(Origin))/(RegionCells*Step);
        if(p.X<0||p.Y<0||p.Z<0||p.X>=RegionsPerAxis||p.Y>=RegionsPerAxis||p.Z>=RegionsPerAxis)return -1;
        return ((int)p.Y*RegionsPerAxis+(int)p.Z)*RegionsPerAxis+(int)p.X;
    }
    private static float Density(VoxelChunk chunk,Vector3 position)
    {
        float value=RawDensity(chunk,position);
        if(value!=0)return value;
        // Preserve genuine local axis planes, not merely an axis-looking gradient
        // at a saddle. All other zero nodes resolve consistently toward air.
        return IsLocalAxisPlane(chunk,position) ? 0 : -1f/256;
    }
    private static bool IsLocalAxisPlane(VoxelChunk chunk,Vector3 position)
    {
        Span<int> low=stackalloc int[3];Span<int> high=stackalloc int[3];Span<float> p=stackalloc float[3];
        p[0]=position.X;p[1]=position.Y;p[2]=position.Z;
        for(int i=0;i<3;i++){low[i]=(int)Math.Floor(p[i]-0.75f);high[i]=(int)Math.Floor(p[i]-0.25f)+1;}
        for(int axis=0;axis<3;axis++)
        {
            if(p[axis]!=(float)Math.Floor(p[axis]))continue;
            int a=(axis+1)%3,b=(axis+2)%3;bool valid=true,firstSide=false;
            for(int side=0;side<2 && valid;side++)
            {
                bool first=true,reference=false;int along=(int)p[axis]-1+side;
                for(int u=low[a];u<=high[a] && valid;u++)for(int v=low[b];v<=high[b];v++)
                {
                    int x=axis==0 ? along : a==0 ? u : v;
                    int y=axis==1 ? along : a==1 ? u : v;
                    int z=axis==2 ? along : a==2 ? u : v;
                    bool solid=BlockInfo.IsSolid(chunk.Get(x,y,z));
                    if(first){first=false;reference=solid;}else if(reference!=solid){valid=false;break;}
                }
                if(side==0)firstSide=reference;else if(reference==firstSide)valid=false;
            }
            if(valid)return true;
        }
        return false;
    }
    private static float RawDensity(VoxelChunk chunk,Vector3 position)
    {
        var p=position-new Vector3(0.5f);
        int x=(int)Math.Floor(p.X),y=(int)Math.Floor(p.Y),z=(int)Math.Floor(p.Z);
        float fx=p.X-x,fy=p.Y-y,fz=p.Z-z,result=0;
        for(int dx=0;dx<2;dx++)
        for(int dy=0;dy<2;dy++)
        for(int dz=0;dz<2;dz++)
            if(BlockInfo.IsSolid(chunk.Get(x+dx,y+dy,z+dz)))
                result+=(dx==0 ? 1-fx : fx)*(dy==0 ? 1-fy : fy)*(dz==0 ? 1-fz : fz);
        return result-0.5f;
    }
    private static Vector3 Normal(VoxelChunk chunk,Vector3 p,Vector3 fallback)
    {
        const float e=0.02f;
        var gradient=new Vector3(RawDensity(chunk,p-new Vector3(e,0,0))-RawDensity(chunk,p+new Vector3(e,0,0)),
            RawDensity(chunk,p-new Vector3(0,e,0))-RawDensity(chunk,p+new Vector3(0,e,0)),
            RawDensity(chunk,p-new Vector3(0,0,e))-RawDensity(chunk,p+new Vector3(0,0,e)));
        return gradient.LengthSquared()>1e-10f ? Vector3.Normalize(gradient) : fallback;
    }
    private static (int X,int Y,int Z,BlockType Type) Owner(VoxelChunk chunk,Vector3 position)
    {
        var p=position-new Vector3(0.5f);
        int x=(int)Math.Floor(p.X),y=(int)Math.Floor(p.Y),z=(int)Math.Floor(p.Z);
        float fx=p.X-x,fy=p.Y-y,fz=p.Z-z,best=-1;
        var owner=(X:0,Y:0,Z:0,Type:BlockType.Air);
        for(int dx=0;dx<2;dx++)
        for(int dy=0;dy<2;dy++)
        for(int dz=0;dz<2;dz++)
        {
            var type=chunk.Get(x+dx,y+dy,z+dz);
            float weight=(dx==0 ? 1-fx : fx)*(dy==0 ? 1-fy : fy)*(dz==0 ? 1-fz : fz);
            if(BlockInfo.IsSolid(type) && weight>best){best=weight;owner=(x+dx,y+dy,z+dz,type);}
        }
        if(best<0)throw new InvalidOperationException("A terrain triangle has no canonical material contributor.");
        return owner;
    }
    private static Vector3 Crossing(Vector3 a,Vector3 b,float av,float bv)
    {
        if(av==0)return a;if(bv==0)return b;
        // Every tetrahedron sharing an edge must produce bit-identical vertices.
        // Reversed lerp arithmetic otherwise leaves microscopic cracks in the
        // delivered mesh even when the contour is mathematically continuous.
        if(a.X>b.X || (a.X==b.X && (a.Y>b.Y || (a.Y==b.Y && a.Z>b.Z))))
        {var point=a;a=b;b=point;float value=av;av=bv;bv=value;}
        double fraction=(double)av/(av-bv);
        return new Vector3((float)(a.X+((double)b.X-a.X)*fraction),
            (float)(a.Y+((double)b.Y-a.Y)*fraction),(float)(a.Z+((double)b.Z-a.Z)*fraction));
    }
    private static void Polygonise(VoxelChunk chunk,List<TerrainFace> faces,ReadOnlySpan<Vector3> p,ReadOnlySpan<float> values)
    {
        Span<int> inside=stackalloc int[4];Span<int> outside=stackalloc int[4];int ni=0,no=0;
        var inner=Vector3.Zero;var outer=Vector3.Zero;
        for(int i=0;i<4;i++)
            if(values[i]>=0){inside[ni++]=i;inner+=p[i];}else{outside[no++]=i;outer+=p[i];}
        if(ni==0 || ni==4)return;
        var outward=outer/no-inner/ni;
        if(ni==1 || ni==3)
        {
            int lone=ni==1 ? inside[0] : outside[0];
            var others=ni==1 ? outside : inside;
            var a=Crossing(p[lone],p[others[0]],values[lone],values[others[0]]);
            var b=Crossing(p[lone],p[others[1]],values[lone],values[others[1]]);
            var c=Crossing(p[lone],p[others[2]],values[lone],values[others[2]]);
            Emit(chunk,faces,a,b,c,outward);return;
        }
        int i0=inside[0],i1=inside[1],o0=outside[0],o1=outside[1];
        var aa=Crossing(p[i0],p[o0],values[i0],values[o0]);
        var bb=Crossing(p[i0],p[o1],values[i0],values[o1]);
        var cc=Crossing(p[i1],p[o0],values[i1],values[o0]);
        var dd=Crossing(p[i1],p[o1],values[i1],values[o1]);
        Emit(chunk,faces,aa,bb,cc,outward);Emit(chunk,faces,bb,dd,cc,outward);
    }
    private static void Emit(VoxelChunk chunk,List<TerrainFace> faces,Vector3 a,Vector3 b,Vector3 c,Vector3 outward)
    {
        var cross=Vector3.Cross(b-a,c-a);
        if(cross.LengthSquared()<1e-12f)return;
        if(Vector3.Dot(cross,outward)<0){var swap=b;b=c;c=swap;cross=-cross;}
        var normal=Vector3.Normalize(cross);var owner=Owner(chunk,(a+b+c)/3);
        faces.Add(new TerrainFace(a,b,c,Normal(chunk,a,normal),Normal(chunk,b,normal),Normal(chunk,c,normal),
            owner.X,owner.Y,owner.Z,owner.Type));
    }
}
