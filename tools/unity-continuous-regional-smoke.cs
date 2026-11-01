// Paused Main: scoped regional edit latency, reuse and live power-tint consumer proof.
var g=UnityEngine.Object.FindFirstObjectByType<GameManager>();
var flow=UnityEngine.Object.FindFirstObjectByType<PlayerFlow>();
if(!Application.isPlaying || g==null || g.Simulation==null || flow==null || !flow.IsMenuOpen || Time.timeScale!=0)
    throw new System.InvalidOperationException("Started paused Main required.");
var s=g.Simulation;string before=g.SaveToJson();
try
{
    var chunk=s.World.Chunk;chunk.FromBytes(new byte[Forgeborne.Core.VoxelChunk.Volume]);s.World.Enemies.Clear();
    for(int x=0;x<32;x++)for(int z=0;z<32;z++)for(int y=0;y<4;y++)chunk.TrySet(x,y,z,Forgeborne.Core.BlockType.Stone);
    chunk.SurfaceProfile=Forgeborne.Core.SurfaceProfile.ContinuousTerrainV1;
    g.MeshRenderer.RefreshMesh();
    var times=new System.Collections.Generic.List<double>();var coreTimes=new System.Collections.Generic.List<double>();
    var uploadTimes=new System.Collections.Generic.List<double>();int largestRefresh=0;
    var positions=new[]{new Vector3Int(5,5,5),new Vector3Int(3,3,3),new Vector3Int(4,4,4),new Vector3Int(15,3,15)};
    foreach(var p in positions)
    for(int pass=0;pass<4;pass++)
    {
        var original=chunk.Get(p.x,p.y,p.z);
        chunk.TrySet(p.x,p.y,p.z,original==Forgeborne.Core.BlockType.Air ? Forgeborne.Core.BlockType.EmberOre : Forgeborne.Core.BlockType.Air);
        var timer=System.Diagnostics.Stopwatch.StartNew();var updated=chunk.Geometry;
        double coreTime=timer.Elapsed.TotalMilliseconds;g.MeshRenderer.RefreshMesh();timer.Stop();
        coreTimes.Add(coreTime);uploadTimes.Add(timer.Elapsed.TotalMilliseconds-coreTime);
        times.Add(timer.Elapsed.TotalMilliseconds);largestRefresh=System.Math.Max(largestRefresh,g.MeshRenderer.LastRegionsRebuilt);
        chunk.TrySet(p.x,p.y,p.z,original);g.MeshRenderer.RefreshMesh();
    }
    chunk.TrySet(5,4,5,Forgeborne.Core.BlockType.KineticMill);
    for(int x=6;x<=20;x++)chunk.TrySet(x,4,5,Forgeborne.Core.BlockType.Cable);
    g.MeshRenderer.RefreshMesh();var previous=chunk.Geometry;
    Forgeborne.Core.TerrainRegion target=null;int vertex=-1;
    foreach(var region in previous.Regions)
    {
        if(!region.HasPowerSensitiveMaterials)continue;
        var data=region.BuildMesh();vertex=data.BlockTypes.FindIndex(type=>type==Forgeborne.Core.BlockType.KineticMill);
        if(vertex>=0){target=region;break;}
    }
    if(target==null)throw new System.InvalidOperationException("Tint fixture has no visible mill surface.");
    var view=g.MeshRenderer.transform.Find("Surface region "+target.Id);
    var mesh=view.GetComponent<MeshFilter>().sharedMesh;var unpowered=mesh.colors[vertex];
    if(s.World.PowerState(5,4,5)!=Forgeborne.Core.CellPowerState.None)throw new System.InvalidOperationException("Tint fixture starts powered.");
    chunk.TrySet(21,4,5,Forgeborne.Core.BlockType.Dynamo);
    var current=chunk.Geometry;
    if(!object.ReferenceEquals(target,current.Regions[target.Id]) || s.World.PowerState(5,4,5)!=Forgeborne.Core.CellPowerState.Powered)
        throw new System.InvalidOperationException("Remote source did not preserve geometry while powering the mill.");
    g.MeshRenderer.RefreshMesh();var powered=mesh.colors[vertex];
    var tint=Forgeborne.Core.PowerTint.ColorFor(Forgeborne.Core.CellPowerState.Powered);
    var expected=new Color(unpowered.r*tint.Item1,unpowered.g*tint.Item2,unpowered.b*tint.Item3,unpowered.a*tint.Item4);
    if((new Vector4(powered.r-expected.r,powered.g-expected.g,powered.b-expected.b,powered.a-expected.a)).sqrMagnitude>0.00000001f)
        throw new System.InvalidOperationException("Unchanged region retained stale power colours.");
    double maximum=0,total=0;foreach(double value in times){maximum=System.Math.Max(maximum,value);total+=value;}
    return new{timedEdits=times.Count,meanEditMilliseconds=total/times.Count,maxEditMilliseconds=maximum,
        largestRegionsRebuilt=largestRefresh,individualEditMilliseconds=times.ToArray(),
        geometryMilliseconds=coreTimes.ToArray(),unityUploadAndColliderMilliseconds=uploadTimes.ToArray(),
        unchangedRegionPowerTintUpdated=true,sameSimulation=true,
        scope="Scoped paused Editor CPU/collider edit samples; not native frame-rate or final workload acceptance"};
}
finally
{
    if(!g.TryLoadFromJson(before) || !object.ReferenceEquals(s,g.Simulation) || g.SaveToJson()!=before)
        throw new System.InvalidOperationException("Regional probe did not restore exact state in the original Simulation.");
}
