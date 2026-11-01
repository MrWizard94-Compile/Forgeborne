using System.Numerics;
using System.Text.Json.Nodes;
using Forgeborne.Core;

internal static class AuthoredSurfaceTests
{
    public static void Run(Action<bool,string> check)
    {
        var s=new Simulation(42);
        AuthoredFoundry.Populate(s.World);s.Fps.Teleport(16.5f,4,7.5f);s.AdvanceFrame(0,0,0,false);
        var chunk=s.World.Chunk;var geometry=chunk.Geometry;
        var stock=chunk.ToBytes();var required=new[]{BlockType.Workstation,BlockType.KineticMill,BlockType.Dynamo,BlockType.Wall};
        var costs=required.Select(type=>{BuildCost.TryGet(type,out var value);return value;}).ToArray();
        check(stock.Count(b=>b==(byte)BlockType.Stone)>=costs.Sum(c=>c.Stone)
            && stock.Count(b=>b==(byte)BlockType.EmberOre)>=costs.Sum(c=>c.EmberOre)
            && stock.Count(b=>b==(byte)BlockType.Wood)>=costs.Sum(c=>c.Wood),
            "authored encounter contains enough real gatherable material for a workstation, mill, dynamo and defensive plate without debug grants");
        check(AuthoredFoundry.SourcePartCount>150 && AuthoredFoundry.ContentSha256.Length==64,
            "authored original content loads with a validated closed topology and exact digest");
        check(geometry.Regions.All(r=>r.MaterialRole!=null && r.Label!=null)
            && geometry.TriangleCount>20_000,"authored regions expose their original material and component identity");
        check(geometry.Contains(new Vector3(16,3.8f,16)) && !geometry.Contains(new Vector3(16,6,16)),
            "authored continuous floor contains its solid foundation and leaves the human-scale hall empty");
        check(geometry.Contains(new Vector3(5.2f,4.5f,15.2f)) && geometry.Contains(new Vector3(5.2f,6.7f,15.2f)),
            "authored compound containment uses union semantics in overlapping vessel and collar shells");
        check(!geometry.Contains(new Vector3(5.8f,5.6f,15.8f)),
            "authored cylinder bounding-box corner remains real empty space");
        check(geometry.Raycast(new Vector3(16,8,16),-Vector3.UnitY,10,out var floor)
            && Math.Abs(floor.Distance-4)<.0001f && floor.Y==3 && floor.X==16 && floor.Z==16,
            "authored floor ray uses actual point semantics instead of an arbitrary component anchor cell");
        check(geometry.Raycast(new Vector3(4,5.6f,15.2f),Vector3.UnitX,4,out var vessel)
            && Math.Abs(vessel.Distance-.55f)<.0001f && vessel.Normal.X<-.99f,
            "authored circular vessel ray matches an independent radius and distance oracle");
        var sweep=geometry.SweepBody(new Vector3(4,4.7f,15.2f),new Vector3(4,0,0));
        check(sweep.Hit && Math.Abs(sweep.Fraction-(.55f-.3f)/4)<.0001f,
            "authored fast swept suit movement stops at the rendered pressure vessel");
        check(!geometry.OverlapsBody(new Vector3(16.5f,4,7.5f)),"authored entry pose has full suit clearance");
        var enemy=new Enemy(3,4,15);enemy.TickTowardPosition(8,4,15,chunk);
        check(enemy.X==3,"authored enemy planning refuses a step through the rendered vessel instead of consulting only empty ledger cells");
        string beforePreview=s.SaveToJson();
        check(s.World.InspectBuild(2,5,17,BlockType.Wall)==BuildResult.Occupied && s.SaveToJson()==beforePreview,
            "authored read-only construction refuses an empty ledger cell occupied by a visible service pipe");
        var body=new FpsController(16.5f,4,7.5f);
        for(int i=0;i<240;i++)body.UpdateWorld(1f/120,0,-1,false,chunk);
        check(body.IsGrounded && Math.Abs(body.Y-4)<.001f && body.Z>=3.139f && body.Z<3.15f,
            "authored entry bulkhead prevents world escape under sustained production movement");

        // Ray/gather go through the real Simulation adapter boundary. The
        // resource is below eye height, so aim at its visible fractured crown.
        s.Fps.Teleport(11.5f,4,9.2f);s.ApplyLook(0,-31);s.AdvanceFrame(0,0,0,false);
        var aim=s.InspectAim();
        check(aim.Kind==AimTargetKind.Solid && aim.X==11 && aim.Y==4 && aim.Z==11 && aim.Distance.HasValue,
            "authored gather aim selects the visible mineral owner rather than a static architecture anchor");
        int ore=s.World.Player.Inventory.Get(BlockType.EmberOre);var beforeRegions=geometry.Regions.ToArray();
        check(s.GatherAhead()==GatherResult.Success && chunk.Get(11,4,11)==BlockType.Air
            && s.World.Player.Inventory.Get(BlockType.EmberOre)==ore+1,
            "authored production gather removes one canonical resource and credits the existing inventory");
        var removed=chunk.Geometry;
        check(removed.TriangleCount<geometry.TriangleCount
            && removed.Regions.All(r=>beforeRegions.Any(old=>ReferenceEquals(r,old))),
            "authored gather removes exactly its bound geometry while reusing every unaffected immutable component");
        s.World.Player.Inventory.Add(BlockType.Stone,300);s.World.Player.Inventory.Add(BlockType.EmberOre,300);s.World.Player.Inventory.Add(BlockType.Wood,300);
        int placed=0;
        foreach(var type in new[]{BlockType.Wall,BlockType.Workstation,BlockType.KineticMill,BlockType.Dynamo,BlockType.Transformer,
            BlockType.Cable,BlockType.Alternator,BlockType.ShockPylon,BlockType.Conduit})
        {
            int x=6+placed*2;
            var before=chunk.Geometry;
            var result=s.World.Build(x,4,6,type);
            check(result==BuildResult.Success && chunk.Geometry.TriangleCount>before.TriangleCount,
                "authored construction creates real original shared mesh components for "+type+" ("+result+", triangles "+before.TriangleCount+" -> "+chunk.Geometry.TriangleCount+")");
            var added=chunk.Geometry.Regions.Where(r=>!before.Regions.Contains(r)).ToArray();
            var preview=AuthoredFoundry.ConstructionTemplate(type);
            bool same=added.Length==preview.Count;
            for(int j=0;j<added.Length && same;j++)
            {
                var actual=added[j].BuildMesh();var expected=preview[j].BuildMesh();
                same&=actual.Indices.SequenceEqual(expected.Indices) && actual.Vertices.Count==expected.Vertices.Count;
                for(int k=0;k<actual.Vertices.Count && same;k++)
                {
                    var a=actual.Vertices[k];var p=expected.Vertices[k];
                    same&=Math.Abs(a.Item1-p.Item1-x)<.00001f && Math.Abs(a.Item2-p.Item2-4)<.00001f && Math.Abs(a.Item3-p.Item3-6)<.00001f;
                }
            }
            check(same,"authored construction preview uses the exact placed mesh buffers for "+type);
            placed++;
        }
        string saved=s.SaveToJson();var owner=s;var fps=s.Fps;var look=s.Look;
        check(s.LoadFromJson(saved) && s.SaveToJson()==saved && ReferenceEquals(s,owner) && ReferenceEquals(s.Fps,fps) && ReferenceEquals(s.Look,look)
            && s.World.Chunk.SurfaceProfile==SurfaceProfile.AuthoredFoundryV1
            && s.World.Chunk.Get(11,4,11)==BlockType.Air,
            "authored exact save recovery preserves removal, all machine meshes and canonical owner identities");
        var cold=new Simulation(7);
        check(cold.LoadFromJson(saved) && cold.SaveToJson()==saved
            && cold.World.Chunk.Geometry.TriangleCount==s.World.Chunk.Geometry.TriangleCount,
            "authored cold simulation recovery resolves exact embedded geometry and edited construction state");
        foreach(string key in new[]{"SurfaceContentId","SurfaceContentSha256"})
        {
            var bad=JsonNode.Parse(saved)!.AsObject();bad[key]="unknown";
            check(!s.LoadFromJson(bad.ToJsonString()) && s.SaveToJson()==saved,
                "authored mismatched "+key+" is rejected before any live mutation");
            bad.Remove(key);
            check(!s.LoadFromJson(bad.ToJsonString()) && s.SaveToJson()==saved,
                "authored missing "+key+" is rejected before any live mutation");
        }
        var corrupt=JsonNode.Parse(saved)!.AsObject();var bytes=Convert.FromBase64String(corrupt["ChunkBase64"]!.GetValue<string>());
        bytes[VoxelChunk.Index(16,3,16)]=0;corrupt["ChunkBase64"]=Convert.ToBase64String(bytes);
        check(!s.LoadFromJson(corrupt.ToJsonString()) && s.SaveToJson()==saved,
            "authored invisible foundation edits are rejected instead of disagreeing with the immutable visible floor");
        bytes=Convert.FromBase64String(JsonNode.Parse(saved)!["ChunkBase64"]!.GetValue<string>());
        bytes[VoxelChunk.Index(16,12,16)]=(byte)BlockType.EmberOre;corrupt["ChunkBase64"]=Convert.ToBase64String(bytes);
        check(!s.LoadFromJson(corrupt.ToJsonString()) && s.SaveToJson()==saved,
            "authored unsupported resource bindings cannot enter a save as invisible geometry");
        using var contentStream=typeof(AuthoredFoundry).Assembly.GetManifestResourceStream("Forgeborne.Core.Content.CinderExchangeV1.json")!;
        using var reader=new StreamReader(contentStream);string contentJson=reader.ReadToEnd();
        check(!contentJson.Contains('\r'),"authored embedded geometry uses canonical LF bytes so a Windows checkout cannot change save identity");
        void Reject(Action<JsonObject> alter,string name)
        {
            var value=JsonNode.Parse(contentJson)!.AsObject();alter(value);bool refused=false;
            try{AuthoredFoundry.ValidatePayload(value.ToJsonString());}
            catch(Exception e) when(e is InvalidOperationException or System.Text.Json.JsonException){refused=true;}
            check(refused,"authored content boundary rejects "+name);
        }
        Reject(v=>v["Schema"]=99,"unknown geometry schema");
        Reject(v=>v["Parts"]![0]!["Material"]="unknown","an unavailable material role");
        Reject(v=>v["Parts"]![0]!["Vertices"]![0]![0]=129,"out-of-contract coordinates");
        Reject(v=>v["Parts"]![0]!["Indices"]![0]=999999,"an invalid triangle vertex reference");
        Reject(v=>v["Parts"]![0]!["Normals"]![0]=new JsonArray(0,0,0),"a zero shading normal");
        Reject(v=>
        {
            var part=v["Parts"]![0]!;var indices=part["Indices"]!.AsArray();var normals=part["Normals"]!.AsArray();
            for(int i=0;i<3;i++){indices.RemoveAt(indices.Count-1);normals.RemoveAt(normals.Count-1);}
        },"an open shell");
    }
}
