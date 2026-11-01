using System.Numerics;
using System.Text.Json.Nodes;
using Forgeborne.Core;

internal static class ContinuousSurfaceTests
{
    public static void Run(Action<bool,string> check)
    {
        var chunk=new VoxelChunk{SurfaceProfile=SurfaceProfile.ContinuousTerrainV1};
        var empty=chunk.Geometry;
        check(empty.TriangleCount==0 && !empty.Raycast(new Vector3(0,5,5),Vector3.UnitX,20,out _),
            "continuous empty geometry has no phantom surfaces");
        chunk.TrySet(5,5,5,BlockType.EmberOre);
        var geometry=chunk.Geometry;var mesh=geometry.BuildMesh();
        // Nonplanar zero-node ties use the profile's 1/256 air bias. At the
        // isolated deposit extremum this moves the contour inward by 1/258 m.
        float isolatedDistance=3+1f/258;
        int rebuilt=geometry.Regions.Where((region,index)=>!object.ReferenceEquals(region,empty.Regions[index])).Count();
        check(rebuilt==1,"continuous isolated edit replaces one region and reuses all other immutable regions");
        check(geometry.TriangleCount>12 && mesh.Indices.Count==geometry.TriangleCount*3,
            "continuous resource is a contoured mesh rather than six cube faces");
        check(geometry.Raycast(new Vector3(2,5.5f,5.5f),Vector3.UnitX,10,out var hit)
            && Math.Abs(hit.Distance-isolatedDistance)<0.0001f && hit.X==5 && hit.Y==5 && hit.Z==5
            && hit.Type==BlockType.EmberOre && hit.Normal.X<0,
            "continuous centre ray hits the visible mineral surface and canonical material");
        check(!geometry.Raycast(new Vector3(2,5.95f,5.95f),Vector3.UnitX,10,out _),
            "continuous corner ray passes through space formerly blocked by an invisible cube");
        check(!geometry.OverlapsBody(new Vector3(6.2f,5,6.2f))
            && geometry.OverlapsBody(new Vector3(5.5f,5.2f,5.5f)),
            "continuous body collision follows the actual contour and catches contained penetration");
        check(geometry.Contains(new Vector3(5.5f)) && !geometry.Contains(new Vector3(5.95f)),
            "continuous closed-surface interior query distinguishes mineral centre and cut-off corner");
        var sweep=geometry.SweepBody(new Vector3(2,4.5f,5.5f),new Vector3(10,0,0));
        check(sweep.Hit && Math.Abs(sweep.Fraction-(isolatedDistance-0.3f)/10)<0.0001f,
            "continuous swept body stops at the profile's exact contoured extremum and cannot tunnel through a deposit");
        var away=geometry.SweepBody(new Vector3(4.7f,4.5f,5.5f),new Vector3(-1,0,0));
        check(!away.Hit,"continuous body can move away from an existing contact");
        var edgeCounts=new Dictionary<(Vector3,Vector3),int>();
        bool valid=true;
        Vector3 V((float,float,float) p)=>new(p.Item1,p.Item2,p.Item3);
        int Compare(Vector3 a,Vector3 b)
        {int x=a.X.CompareTo(b.X);if(x!=0)return x;int y=a.Y.CompareTo(b.Y);return y!=0 ? y : a.Z.CompareTo(b.Z);}
        for(int i=0;i<mesh.Indices.Count;i+=3)
        {
            var a=V(mesh.Vertices[mesh.Indices[i]]);var b=V(mesh.Vertices[mesh.Indices[i+1]]);var c=V(mesh.Vertices[mesh.Indices[i+2]]);
            valid&=Vector3.Cross(b-a,c-a).LengthSquared()>1e-12f;
            foreach(var edge in new[]{(a,b),(b,c),(c,a)})
            {
                var key=Compare(edge.Item1,edge.Item2)<0 ? edge : (edge.Item2,edge.Item1);
                edgeCounts.TryGetValue(key,out int count);edgeCounts[key]=count+1;
            }
        }
        check(valid && edgeCounts.Values.All(count=>count==2),
            "continuous isolated mineral mesh is nondegenerate and closed along every edge");
        var seamChunk=new VoxelChunk{SurfaceProfile=SurfaceProfile.ContinuousTerrainV1};
        var seamBefore=seamChunk.Geometry;seamChunk.TrySet(3,3,3,BlockType.Stone);var seam=seamChunk.Geometry;
        rebuilt=seam.Regions.Where((region,index)=>!object.ReferenceEquals(region,seamBefore.Regions[index])).Count();
        check(rebuilt==8,"continuous edit at a three-axis region boundary refreshes all affected normals and triangles");
        var seamMesh=seam.BuildMesh();edgeCounts.Clear();
        for(int i=0;i<seamMesh.Indices.Count;i+=3)
        {
            var a=V(seamMesh.Vertices[seamMesh.Indices[i]]);var b=V(seamMesh.Vertices[seamMesh.Indices[i+1]]);var c=V(seamMesh.Vertices[seamMesh.Indices[i+2]]);
            foreach(var edge in new[]{(a,b),(b,c),(c,a)})
            {
                var key=Compare(edge.Item1,edge.Item2)<0 ? edge : (edge.Item2,edge.Item1);
                edgeCounts.TryGetValue(key,out int count);edgeCounts[key]=count+1;
            }
        }
        check(edgeCounts.Values.All(count=>count==2),"continuous contour remains closed across all intersected region seams");
        check(mesh.Vertices.Count==mesh.Normals.Count && mesh.Vertices.Count==mesh.BlockTypes.Count
            && mesh.Normals.All(n=>Math.Abs(V(n).Length()-1)<0.001f),
            "continuous render buffers carry finite unit normals and material IDs");
        for(int pattern=1;pattern<256;pattern++)
        {
            var topology=new VoxelChunk{SurfaceProfile=SurfaceProfile.ContinuousTerrainV1};
            for(int bit=0;bit<8;bit++)if((pattern&(1<<bit))!=0)
                topology.TrySet(9+(bit&1),9+((bit>>1)&1),9+((bit>>2)&1),BlockType.Stone);
            var topologyMesh=topology.Geometry.BuildMesh();edgeCounts.Clear();
            var links=new Dictionary<Vector3,Dictionary<Vector3,List<Vector3>>>();
            void Link(Vector3 vertex,Vector3 a,Vector3 b)
            {
                if(!links.TryGetValue(vertex,out var graph)){graph=new Dictionary<Vector3,List<Vector3>>();links.Add(vertex,graph);}
                if(!graph.TryGetValue(a,out var from)){from=new List<Vector3>();graph.Add(a,from);}from.Add(b);
                if(!graph.TryGetValue(b,out var to)){to=new List<Vector3>();graph.Add(b,to);}to.Add(a);
            }
            for(int i=0;i<topologyMesh.Indices.Count;i+=3)
            {
                var a=V(topologyMesh.Vertices[topologyMesh.Indices[i]]);var b=V(topologyMesh.Vertices[topologyMesh.Indices[i+1]]);var c=V(topologyMesh.Vertices[topologyMesh.Indices[i+2]]);
                Link(a,b,c);Link(b,c,a);Link(c,a,b);
                foreach(var edge in new[]{(a,b),(b,c),(c,a)})
                {
                    var key=Compare(edge.Item1,edge.Item2)<0 ? edge : (edge.Item2,edge.Item1);
                    edgeCounts.TryGetValue(key,out int count);edgeCounts[key]=count+1;
                }
            }
            bool manifold=edgeCounts.Count>0 && edgeCounts.Values.All(count=>count==2);
            foreach(var graph in links.Values)
            {
                var visited=new HashSet<Vector3>();var pending=new Stack<Vector3>();pending.Push(graph.Keys.First());
                while(pending.Count>0){var vertex=pending.Pop();if(!visited.Add(vertex))continue;foreach(var neighbour in graph[vertex])pending.Push(neighbour);}
                manifold&=visited.Count==graph.Count && graph.Values.All(neighbours=>neighbours.Count==2);
            }
            check(manifold,"continuous compound occupancy pattern "+pattern+" has closed edges and one connected vertex fan");
        }

        // Independent plane/edge ray oracle over delivered triangles, rather
        // than the BVH's determinant/barycentric implementation.
        float Reference(Vector3 origin,Vector3 direction,out bool inside)
        {
            float nearest=float.PositiveInfinity;inside=false;
            for(int i=0;i<mesh.Indices.Count;i+=3)
            {
                var a=V(mesh.Vertices[mesh.Indices[i]]);var b=V(mesh.Vertices[mesh.Indices[i+1]]);var c=V(mesh.Vertices[mesh.Indices[i+2]]);
                var normal=Vector3.Cross(b-a,c-a);float denominator=Vector3.Dot(direction,normal);
                if(Math.Abs(denominator)<1e-8f)continue;
                float t=Vector3.Dot(a-origin,normal)/denominator;if(t<0 || t>20)continue;
                var p=origin+direction*t;
                if(Vector3.Dot(normal,Vector3.Cross(b-a,p-a)) < -1e-6f
                    || Vector3.Dot(normal,Vector3.Cross(c-b,p-b)) < -1e-6f
                    || Vector3.Dot(normal,Vector3.Cross(a-c,p-c)) < -1e-6f)continue;
                if(t<nearest){nearest=t;inside=denominator>0;}
            }
            return nearest;
        }
        bool agrees=true;var random=new Random(771);
        for(int i=0;i<300;i++)
        {
            var origin=new Vector3(2,4.8f+(float)random.NextDouble()*1.4f,4.8f+(float)random.NextDouble()*1.4f);
            var direction=Vector3.Normalize(new Vector3(1,(float)random.NextDouble()*0.08f-0.04f,(float)random.NextDouble()*0.08f-0.04f));
            float expected=Reference(origin,direction,out _);
            bool actual=geometry.Raycast(origin,direction,20,out hit);
            agrees&=float.IsPositiveInfinity(expected) ? !actual : actual && Math.Abs(expected-hit.Distance)<0.0002f;
        }
        check(agrees,"continuous BVH rays agree with an independent plane/edge oracle for 300 aimed paths");
        agrees=true;
        for(int i=0;i<500;i++)
        {
            var point=new Vector3(4.8f+(float)random.NextDouble()*1.4f,4.8f+(float)random.NextDouble()*1.4f,4.8f+(float)random.NextDouble()*1.4f);
            Reference(point,Vector3.Normalize(new Vector3(1,0.3197f,0.3533f)),out bool inside);
            agrees&=geometry.Contains(point)==inside;
        }
        check(agrees,"continuous immutable field containment agrees with independent delivered-mesh crossings for 500 points");
        for(int i=0;i<10;i++){geometry.Raycast(new Vector3(2,5.5f,5.5f),Vector3.UnitX,10,out _);geometry.SweepBody(new Vector3(2,4.5f,5.5f),new Vector3(10,0,0));}
        long allocated=GC.GetAllocatedBytesForCurrentThread();
        for(int i=0;i<500;i++){geometry.Raycast(new Vector3(2,5.5f,5.5f),Vector3.UnitX,10,out _);geometry.SweepBody(new Vector3(2,4.5f,5.5f),new Vector3(10,0,0));}
        check(GC.GetAllocatedBytesForCurrentThread()==allocated,"continuous warm ray/sweep queries allocate zero managed bytes");
        check(object.ReferenceEquals(geometry,chunk.Geometry),"continuous unchanged chunk reuses its one immutable geometry snapshot");
        long revision=chunk.Revision;chunk.TrySet(5,5,5,BlockType.EmberOre);
        check(chunk.Revision==revision && object.ReferenceEquals(geometry,chunk.Geometry),
            "continuous no-op material write does not rebuild geometry");
        chunk.TrySet(5,5,5,BlockType.Air);
        check(!object.ReferenceEquals(geometry,chunk.Geometry) && chunk.Geometry.TriangleCount==0
            && geometry.Raycast(new Vector3(2,5.5f,5.5f),Vector3.UnitX,10,out _),
            "continuous destruction replaces the cache without mutating an earlier immutable snapshot");
        mesh.Vertices.Clear();
        check(geometry.Raycast(new Vector3(2,5.5f,5.5f),Vector3.UnitX,10,out _),
            "continuous exported render buffers cannot mutate authoritative geometry");

        var simulation=new Simulation(42);simulation.AdvanceFrame(0,0,0,false);
        string legacy=simulation.SaveToJson();
        check(!legacy.Contains("SurfaceProfile"),"continuous legacy saves retain their exact missing-profile schema");
        simulation.World.Chunk.SurfaceProfile=SurfaceProfile.ContinuousTerrainV1;
        string continuous=simulation.SaveToJson();var fps=simulation.Fps;var look=simulation.Look;
        check(simulation.LoadFromJson(continuous) && simulation.SaveToJson()==continuous
            && simulation.World.Chunk.SurfaceProfile==SurfaceProfile.ContinuousTerrainV1
            && object.ReferenceEquals(fps,simulation.Fps) && object.ReferenceEquals(look,simulation.Look),
            "continuous profile survives exact in-place save recovery with existing motion/look owners");
        var bad=JsonNode.Parse(continuous)!;bad["SurfaceProfile"]=99;
        var priorWorld=simulation.World;
        check(!simulation.LoadFromJson(bad.ToJsonString()) && object.ReferenceEquals(priorWorld,simulation.World)
            && simulation.SaveToJson()==continuous,"continuous unknown profile fails before any world swap or state change");
        check(simulation.LoadFromJson(legacy) && simulation.World.Chunk.SurfaceProfile==SurfaceProfile.LegacyBlocks
            && simulation.SaveToJson()==legacy,"continuous loading an old save explicitly restores its original binary rules");

        var action=new Simulation(42);action.World.Enemies.Clear();
        action.World.Chunk.FromBytes(new byte[VoxelChunk.Volume]);
        action.World.Chunk.TrySet(5,5,5,BlockType.EmberOre);
        action.World.Chunk.SurfaceProfile=SurfaceProfile.ContinuousTerrainV1;
        action.Fps.Teleport(2,3.9f,5.5f);action.AdvanceFrame(0,0,0,false);action.ApplyLook(90,0);
        var aimed=action.InspectAim(false);int rounds=action.Weapon.Mag;
        check(aimed.Kind==AimTargetKind.Solid && aimed.X==5 && Math.Abs(aimed.Distance!.Value-isolatedDistance)<0.0001f
            && action.FireAimed(out var shot) && shot.Target==AimTargetKind.Solid
            && Math.Abs(shot.HitDistance!.Value-isolatedDistance)<0.0001f && action.Weapon.Mag==rounds-1,
            "continuous production inspect/fire returns the actual triangle distance for impact feedback");
        int stock=action.World.Player.Inventory.Get(BlockType.EmberOre);
        check(action.GatherAhead()==GatherResult.Success && action.World.Player.Inventory.Get(BlockType.EmberOre)==stock+1
            && action.World.Chunk.Geometry.TriangleCount==0,
            "continuous production gathering edits the material owner and removes its visible/physical surface");
        action.World.Chunk.TrySet(5,5,5,BlockType.Stone);action.World.Player.Inventory.Add(BlockType.Stone,2);
        var build=action.InspectBuildAhead(BlockType.Wall);
        check(build.HasCell && build.X==4 && build.Y==5 && build.Z==5 && build.Result==BuildResult.Success
            && action.BuildAhead(BlockType.Wall)==BuildResult.Success
            && action.World.Chunk.Geometry.Raycast(new Vector3(2,5.5f,5.5f),Vector3.UnitX,10,out hit)
            && hit.X==4 && hit.Type==BlockType.Wall,
            "continuous construction uses a face-adjacent supported cell and updates the same physical/visual geometry");
        action.World.Chunk.TrySet(4,5,5,BlockType.Air);
        action.Fps.Teleport(2,4.35f,5.95f);action.AdvanceFrame(0,0,0,false);
        action.World.Enemies.Add(new Enemy(8,5,5));
        check(action.InspectAim().Kind==AimTargetKind.Enemy,
            "continuous shooting can reach an actor through the deposit's genuinely empty corner");
        action.Fps.Teleport(2,3.9f,5.5f);action.AdvanceFrame(0,0,0,false);
        check(action.InspectAim().Kind==AimTargetKind.Solid,
            "continuous deposit centre occludes an actor behind the actual surface");

        var ground=new VoxelChunk{SurfaceProfile=SurfaceProfile.ContinuousTerrainV1};
        for(int x=0;x<32;x++)for(int z=0;z<32;z++)for(int y=0;y<4;y++)ground.TrySet(x,y,z,BlockType.Stone);
        var mover=new FpsController(16.5f,4,16.5f);
        for(int i=0;i<120;i++)mover.UpdateWorld(1f/120,1,0,false,ground);
        check(Math.Abs(mover.X-22.5f)<0.001f && Math.Abs(mover.Y-4)<0.0001f && mover.IsGrounded,
            "continuous controller walks six metres over the visible floor without contact snagging or sinking");
        float peak=mover.Y;
        for(int i=0;i<180;i++){mover.UpdateWorld(1f/120,0,0,i==0,ground);peak=Math.Max(peak,mover.Y);}
        check(peak>5.4f && peak<5.7f && Math.Abs(mover.Y-4)<0.0001f && mover.IsGrounded && mover.VelocityY==0,
            "continuous jump and gravity land on the exact rendered floor");
        mover.Teleport(16.5f,20,16.5f);mover.UpdateWorld(1,0,0,false,ground);
        check(Math.Abs(mover.Y-4)<0.0001f && mover.IsGrounded,
            "continuous controller sweeps a large vertical step instead of tunnelling through the floor");
        for(int x=0;x<32;x++)for(int z=0;z<32;z++)ground.TrySet(x,7,z,BlockType.Stone);
        mover.Teleport(16.5f,4,16.5f);mover.UpdateWorld(1f/120,0,0,false,ground);peak=mover.Y;
        for(int i=0;i<180;i++){mover.UpdateWorld(1f/120,0,0,i==0,ground);peak=Math.Max(peak,mover.Y);}
        check(peak<=5.2001f && peak>5 && Math.Abs(mover.Y-4)<0.0001f && mover.IsGrounded,
            "continuous suit height collides with the rendered ceiling and recovers to the floor");
    }
}
