using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Forgeborne.Core;

/// <summary>
/// Original closed components embedded in the Core DLL. Saves name the exact
/// content; no save-supplied geometry, path, URL or executable is ever loaded.
/// Components bind to the existing material ledger, never a second world.
/// </summary>
public static class AuthoredFoundry
{
    public const string ContentId="forgeborne.cinder-exchange.v1";
    public const string ContentIdV2="forgeborne.cinder-exchange.v2";
    private static readonly Lazy<Content> BuiltIn=new(()=>Load("CinderExchangeV1.json",ContentId));
    private static readonly Lazy<Content> BuiltInV2=new(()=>Load("CinderExchangeV2.json",ContentIdV2));
    public static string ContentSha256=>BuiltIn.Value.Digest;
    public static int SourcePartCount=>BuiltIn.Value.Parts.Length;
    public static bool IsAuthored(SurfaceProfile profile)=>profile==SurfaceProfile.AuthoredFoundryV1 || profile==SurfaceProfile.AuthoredFoundryV2;
    public static string GetContentId(SurfaceProfile profile)=>profile switch
    {
        SurfaceProfile.AuthoredFoundryV1=>ContentId,
        SurfaceProfile.AuthoredFoundryV2=>ContentIdV2,
        _=>throw new ArgumentOutOfRangeException(nameof(profile))
    };
    private static Content Resolve(SurfaceProfile profile)=>profile switch
    {
        SurfaceProfile.AuthoredFoundryV1=>BuiltIn.Value,
        SurfaceProfile.AuthoredFoundryV2=>BuiltInV2.Value,
        _=>throw new ArgumentOutOfRangeException(nameof(profile))
    };
    public static string GetContentSha256(SurfaceProfile profile)=>Resolve(profile).Digest;
    public static int GetSourcePartCount(SurfaceProfile profile)=>Resolve(profile).Parts.Length;
    private static readonly HashSet<string> Materials=new(StringComparer.Ordinal)
        {"Graphite","Steel","Copper","Ceramic","Rubber","Cyan","Amber","Basalt","Paint","Warm","Glass","Timber","CastFloor","EmberMineral"};

    internal sealed class Payload
    {
        public int Schema {get;set;}
        public string? Id {get;set;}
        public PartData[]? Parts {get;set;}
    }
    internal sealed class PartData
    {
        public string? Name {get;set;}
        public string? Kind {get;set;}
        public int[]? Owner {get;set;}
        public int Type {get;set;}
        public string? Material {get;set;}
        public float[][]? Vertices {get;set;}
        public int[]? Indices {get;set;}
        public float[][]? Normals {get;set;}
    }
    private sealed class Part
    {
        public readonly string Name,Kind,Material;
        public readonly BlockType Type;
        public readonly int X,Y,Z;
        public readonly TerrainFace[] Faces;
        public readonly TerrainRegion? FixedRegion;
        public Part(PartData data,int id,TerrainFace[] faces)
        {
            Name=data.Name!;Kind=data.Kind!;Material=data.Material!;Type=(BlockType)data.Type;Faces=faces;
            X=data.Owner?[0] ?? 0;Y=data.Owner?[1] ?? 0;Z=data.Owner?[2] ?? 0;
            if(Kind!="Template")FixedRegion=Create(id,0,0,0);
        }
        public TerrainRegion Create(int id,int x,int y,int z)
        {
            var offset=new Vector3(x,y,z);
            var faces=Faces.Select(f=>f with{A=f.A+offset,B=f.B+offset,C=f.C+offset,
                X=Kind=="Template" ? x : X,Y=Kind=="Template" ? y : Y,Z=Kind=="Template" ? z : Z}).ToList();
            return new TerrainRegion(id,new TerrainGeometry(faces),Name,Material);
        }
    }
    private sealed class Content
    {
        public readonly string Digest;
        public readonly Part[] Parts;
        public readonly Dictionary<BlockType,Part[]> Templates;
        public readonly Dictionary<BlockType,IReadOnlyList<TerrainRegion>> Previews;
        public Content(string digest,Part[] parts)
        {
            Digest=digest;Parts=parts;
            Templates=parts.Where(p=>p.Kind=="Template").GroupBy(p=>p.Type).ToDictionary(g=>g.Key,g=>g.ToArray());
            Previews=Templates.ToDictionary(p=>p.Key,p=>(IReadOnlyList<TerrainRegion>)Array.AsReadOnly(
                p.Value.Select((part,index)=>part.Create(index,0,0,0)).ToArray()));
        }
    }
    private static Content Load(string resource,string expectedId)
    {
        using var stream=typeof(AuthoredFoundry).Assembly.GetManifestResourceStream("Forgeborne.Core.Content."+resource)
            ?? throw new InvalidOperationException("Embedded original foundry geometry is missing.");
        using var reader=new StreamReader(stream,Encoding.UTF8,true);
        string json=reader.ReadToEnd();
        return Parse(json,expectedId);
    }
    internal static void ValidatePayload(string json,SurfaceProfile profile=SurfaceProfile.AuthoredFoundryV1)=>Parse(json,GetContentId(profile));
    private static Content Parse(string json,string expectedId)
    {
        if(json==null || json.Length>24_000_000)throw new InvalidOperationException("Foundry content exceeds its size contract.");
        var data=JsonSerializer.Deserialize<Payload>(json);
        if(data?.Schema!=1 || data.Id!=expectedId || data.Parts==null || data.Parts.Length==0 || data.Parts.Length>1024)
            throw new InvalidOperationException("Unsupported foundry content schema or identity.");
        var parts=new List<Part>();var names=new HashSet<string>(StringComparer.Ordinal);int triangleCount=0;
        foreach(var item in data.Parts)
        {
            if(item==null || string.IsNullOrWhiteSpace(item.Name) || item.Name!.Length>120 || !names.Add(item.Name)
                || item.Material==null || !Materials.Contains(item.Material)
                || (expectedId==ContentId && (item.Material=="CastFloor" || item.Material=="EmberMineral"))
                || (item.Kind!="Static" && item.Kind!="Deposit" && item.Kind!="Template")
                || item.Vertices==null || item.Indices==null || item.Normals==null
                || item.Vertices.Length<4 || item.Vertices.Length>8192 || item.Indices.Length<12
                || item.Indices.Length%3!=0 || item.Normals.Length!=item.Indices.Length)
                throw new InvalidOperationException("Invalid authored component shape, material or name.");
            var type=(BlockType)item.Type;
            if(item.Type<0 || item.Type>byte.MaxValue
                || (item.Kind=="Static" && (type!=BlockType.Bedrock || item.Owner!=null))
                || (item.Kind=="Deposit" && (!BlockInfo.IsGatherable(type) || item.Owner?.Length!=3
                    || !VoxelChunk.InBounds(item.Owner[0],item.Owner[1],item.Owner[2])))
                || (item.Kind=="Template" && (!BuildCost.TryGet(type,out _) || item.Owner!=null)))
                throw new InvalidOperationException("Invalid authored component ledger binding.");
            triangleCount+=item.Indices.Length/3;
            if(triangleCount>150_000)throw new InvalidOperationException("Foundry triangle budget exceeded.");
            Vector3 Vector(float[] v)
            {
                if(v==null || v.Length!=3 || v.Any(f=>float.IsNaN(f) || float.IsInfinity(f) || Math.Abs(f)>128))
                    throw new InvalidOperationException("Authored component contains invalid coordinates.");
                return new Vector3(v[0],v[1],v[2]);
            }
            var vertices=item.Vertices.Select(Vector).ToArray();
            var normals=item.Normals.Select(Vector).ToArray();
            var weld=new Dictionary<Vector3,int>();var welded=new int[vertices.Length];
            for(int i=0;i<vertices.Length;i++)
            {
                if(!weld.TryGetValue(vertices[i],out int at)){at=weld.Count;weld.Add(vertices[i],at);}welded[i]=at;
                if(item.Kind!="Static")
                {
                    var local=vertices[i]-(item.Kind=="Deposit" ? new Vector3(item.Owner![0],item.Owner[1],item.Owner[2]) : Vector3.Zero);
                    float spread=item.Kind=="Deposit" && expectedId==ContentIdV2 ? .45f : 0;
                    if(local.X < -spread || local.X>1+spread || local.Y<0 || local.Y>1 || local.Z < -spread || local.Z>1+spread)
                        throw new InvalidOperationException("Bound geometry must stay inside its declared ledger cell: "+item.Name);
                }
            }
            var edges=new Dictionary<(int,int),List<(int Face,int Sign)>>();var faces=new List<TerrainFace>();double volume=0;
            for(int i=0;i<item.Indices.Length;i+=3)
            {
                int a=item.Indices[i],b=item.Indices[i+1],c=item.Indices[i+2];
                if(a<0 || a>=vertices.Length || b<0 || b>=vertices.Length || c<0 || c>=vertices.Length)
                    throw new InvalidOperationException("Authored triangle references an invalid vertex.");
                var cross=Vector3.Cross(vertices[b]-vertices[a],vertices[c]-vertices[a]);
                if(cross.LengthSquared()<1e-14f)throw new InvalidOperationException("Degenerate authored triangle in "+item.Name);
                var normal=Vector3.Normalize(cross);
                for(int n=i;n<i+3;n++)
                    if(Math.Abs(normals[n].Length()-1)>0.002f || Vector3.Dot(normal,normals[n])<0.01f)
                        throw new InvalidOperationException("Authored shading normals disagree with outward triangles in "+item.Name);
                int face=faces.Count;
                foreach(var (from,to) in new[]{(welded[a],welded[b]),(welded[b],welded[c]),(welded[c],welded[a])})
                {
                    var edge=from<to ? (from,to) : (to,from);
                    if(!edges.TryGetValue(edge,out var uses)){uses=new List<(int,int)>();edges.Add(edge,uses);}
                    uses.Add((face,from<to ? 1 : -1));
                }
                // Translate to a local origin before the signed-volume sum to
                // avoid cancellation on thin pieces far from the world origin.
                var origin=vertices[0];
                volume+=Vector3.Dot(vertices[a]-origin,Vector3.Cross(vertices[b]-origin,vertices[c]-origin))/6.0;
                faces.Add(new TerrainFace(vertices[a],vertices[b],vertices[c],normals[i],normals[i+1],normals[i+2],
                    item.Owner?[0] ?? 0,item.Owner?[1] ?? 0,item.Owner?[2] ?? 0,type,item.Kind=="Static"));
            }
            if(volume<=1e-9 || edges.Values.Any(uses=>uses.Count!=2 || uses[0].Sign+uses[1].Sign!=0))
                throw new InvalidOperationException("Authored component must be closed and outward-oriented: "+item.Name);
            var neighbours=new List<int>[faces.Count];for(int i=0;i<neighbours.Length;i++)neighbours[i]=new List<int>();
            foreach(var uses in edges.Values){neighbours[uses[0].Face].Add(uses[1].Face);neighbours[uses[1].Face].Add(uses[0].Face);}
            var visited=new HashSet<int>();var pending=new Stack<int>();pending.Push(0);
            while(pending.Count>0){int f=pending.Pop();if(visited.Add(f))foreach(int n in neighbours[f])pending.Push(n);}
            if(visited.Count!=faces.Count)throw new InvalidOperationException("Each authored component must be one connected closed shell.");
            parts.Add(new Part(item,parts.Count,faces.ToArray()));
        }
        foreach(var group in parts.Where(p=>p.Kind=="Template").GroupBy(p=>p.Type))
            if(group.Count()>64)throw new InvalidOperationException("Constructed component budget exceeded.");
        foreach(BlockType type in Enum.GetValues(typeof(BlockType)))
            if(BuildCost.TryGet(type,out _) && !parts.Any(p=>p.Kind=="Template" && p.Type==type))
                throw new InvalidOperationException("Authored construction template is missing for "+type);
        using var sha=SHA256.Create();
        string digest=BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(json))).Replace("-","").ToLowerInvariant();
        return new Content(digest,parts.ToArray());
    }

    internal static TerrainGeometry Build(VoxelChunk chunk,TerrainGeometry? previous)
    {
        ValidateLedger(chunk);
        var content=Resolve(chunk.SurfaceProfile);var regions=new List<TerrainRegion>();
        foreach(var part in content.Parts)
            if(part.Kind=="Static" || (part.Kind=="Deposit" && chunk.Get(part.X,part.Y,part.Z)==part.Type))regions.Add(part.FixedRegion!);
        var prior=previous?.Regions.ToDictionary(p=>p.Id);
        for(int y=0;y<VoxelChunk.Size;y++)for(int z=0;z<VoxelChunk.Size;z++)for(int x=0;x<VoxelChunk.Size;x++)
        {
            var type=chunk.Get(x,y,z);
            if(!content.Templates.TryGetValue(type,out var templates))continue;
            for(int i=0;i<templates.Length;i++)
            {
                int id=100_000+VoxelChunk.Index(x,y,z)*1024+(int)type*64+i;
                if(prior!=null && prior.TryGetValue(id,out var old))regions.Add(old);
                else regions.Add(templates[i].Create(id,x,y,z));
            }
        }
        return new TerrainGeometry(regions.ToArray(),false);
    }

    internal static void ValidateLedger(VoxelChunk chunk)
    {
        var content=Resolve(chunk.SurfaceProfile);
        var deposits=content.Parts.Where(p=>p.Kind=="Deposit").Select(p=>(p.X,p.Y,p.Z,p.Type)).ToHashSet();
        for(int y=0;y<VoxelChunk.Size;y++)for(int z=0;z<VoxelChunk.Size;z++)for(int x=0;x<VoxelChunk.Size;x++)
        {
            var type=chunk.Get(x,y,z);
            bool valid=y<=3 ? type==BlockType.Bedrock : type==BlockType.Air || content.Templates.ContainsKey(type)
                || deposits.Contains((x,y,z,type));
            if(!valid)throw new InvalidOperationException("Saved material ledger has no matching authored surface at "+x+","+y+","+z+".");
        }
    }

    /// <summary>The exact original construction meshes, in local placement space; no colliders or world mutation.</summary>
    public static IReadOnlyList<TerrainRegion> ConstructionTemplate(BlockType type)=>ConstructionTemplate(type,SurfaceProfile.AuthoredFoundryV1);
    public static IReadOnlyList<TerrainRegion> ConstructionTemplate(BlockType type,SurfaceProfile profile)
    {
        if(!Resolve(profile).Previews.TryGetValue(type,out var regions))throw new ArgumentOutOfRangeException(nameof(type));
        return regions;
    }

    /// <summary>Populate the existing zone. The caller owns the one Simulation and its pose.</summary>
    public static void Populate(WorldZone world)=>Populate(world,SurfaceProfile.AuthoredFoundryV1);
    public static void Populate(WorldZone world,SurfaceProfile profile)
    {
        if(world==null)throw new ArgumentNullException(nameof(world));
        var content=Resolve(profile);var bytes=new byte[VoxelChunk.Volume];
        for(int y=0;y<=3;y++)for(int z=0;z<VoxelChunk.Size;z++)for(int x=0;x<VoxelChunk.Size;x++)
            bytes[VoxelChunk.Index(x,y,z)]=(byte)BlockType.Bedrock;
        foreach(var part in content.Parts)
            if(part.Kind=="Deposit")bytes[VoxelChunk.Index(part.X,part.Y,part.Z)]=(byte)part.Type;
        world.Chunk.FromBytes(bytes);world.Chunk.SurfaceProfile=profile;
        world.Player.X=16;world.Player.Y=4;world.Player.Z=7;
        world.Enemies.Clear();world.Squads.Clear();
    }
}
