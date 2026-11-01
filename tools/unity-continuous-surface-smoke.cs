// Paused canonical fixture in the existing Main Simulation. Not native input/art/fun acceptance.
var g=UnityEngine.Object.FindFirstObjectByType<GameManager>();
var flow=UnityEngine.Object.FindFirstObjectByType<PlayerFlow>();
if(!Application.isPlaying || g==null || g.Simulation==null || flow==null || !flow.IsMenuOpen || Time.timeScale!=0)
    throw new System.InvalidOperationException("Started paused Main required.");
var s=g.Simulation;var fps=s.Fps;var look=s.Look;
string before=g.SaveToJson();
try
{
    var chunk=s.World.Chunk;s.World.Enemies.Clear();
    chunk.FromBytes(new byte[Forgeborne.Core.VoxelChunk.Volume]);
    for(int x=0;x<32;x++)for(int z=0;z<32;z++)for(int y=0;y<4;y++)chunk.TrySet(x,y,z,Forgeborne.Core.BlockType.Stone);
    chunk.TrySet(5,5,5,Forgeborne.Core.BlockType.EmberOre);
    chunk.SurfaceProfile=Forgeborne.Core.SurfaceProfile.ContinuousTerrainV1;
    var oldScale=g.MeshRenderer.transform.localScale;bool badTransformRejected=false;
    try
    {
        g.MeshRenderer.transform.localScale=Vector3.one*1.1f;
        try {g.MeshRenderer.RefreshMesh();}
        catch(System.InvalidOperationException) {badTransformRejected=true;}
    }
    finally {g.MeshRenderer.transform.localScale=oldScale;}
    if(!badTransformRejected)throw new System.InvalidOperationException("A mismatched world transform was silently accepted.");
    var timer=System.Diagnostics.Stopwatch.StartNew();
    var geometry=chunk.Geometry;g.MeshRenderer.RefreshMesh();timer.Stop();
    var colliders=new System.Collections.Generic.List<MeshCollider>();
    int vertexCount=0,triangleCount=0;
    foreach(var collider in g.MeshRenderer.GetComponentsInChildren<MeshCollider>())
    {
        if(collider.sharedMesh==null)continue;
        colliders.Add(collider);
        int id=int.Parse(collider.name.Substring("Surface region ".Length));
        var expected=geometry.Regions[id].BuildMesh(s.World.PowerState);
        var mesh=collider.GetComponent<MeshFilter>().sharedMesh;var vertices=mesh.vertices;var indices=mesh.triangles;
        if(mesh!=collider.sharedMesh || vertices.Length!=expected.Vertices.Count || indices.Length!=expected.Indices.Count)
            throw new System.InvalidOperationException("Actual region render/collider buffer mismatch.");
        for(int i=0;i<vertices.Length;i++)
        {
            var v=expected.Vertices[i];
            if(vertices[i]!=new Vector3(v.Item1,v.Item2,v.Item3))throw new System.InvalidOperationException("Actual mesh vertex mismatch.");
        }
        for(int i=0;i<indices.Length;i++)if(indices[i]!=expected.Indices[i])throw new System.InvalidOperationException("Actual triangle index mismatch.");
        vertexCount+=vertices.Length;triangleCount+=indices.Length/3;
    }
    if(triangleCount!=geometry.TriangleCount)throw new System.InvalidOperationException("Actual regions do not cover the canonical surface.");
    Physics.SyncTransforms();
    var random=new System.Random(771);int compared=0;float maximumDifference=0;
    for(int i=0;i<200;i++)
    {
        var origin=new Vector3(2,4.8f+(float)random.NextDouble()*1.4f,4.8f+(float)random.NextDouble()*1.4f);
        var direction=new Vector3(1,(float)random.NextDouble()*0.08f-0.04f,(float)random.NextDouble()*0.08f-0.04f).normalized;
        Forgeborne.Core.SurfaceRayHit coreHit;RaycastHit unityHit;
        bool core=geometry.Raycast(new System.Numerics.Vector3(origin.x,origin.y,origin.z),
            new System.Numerics.Vector3(direction.x,direction.y,direction.z),20,out coreHit);
        bool unity=false;float unityDistance=20;
        foreach(var collider in colliders)
            if(collider.Raycast(new Ray(origin,direction),out unityHit,20) && unityHit.distance<=unityDistance)
            {unity=true;unityDistance=unityHit.distance;}
        if(core!=unity)throw new System.InvalidOperationException("Core/actual MeshCollider hit disagreement at ray "+i);
        if(core)
        {
            float difference=Mathf.Abs(coreHit.Distance-unityDistance);maximumDifference=Mathf.Max(maximumDifference,difference);
            if(difference>0.0005f)throw new System.InvalidOperationException("Core/actual MeshCollider distance disagreement.");
        }
        compared++;
    }
    s.Fps.Teleport(2,4,5.5f);s.AdvanceFrame(0,0,0,false);s.ApplyLook(90-s.Look.Yaw,-s.Look.Pitch);
    var hit=s.InspectAim(false);
    if(hit.Kind!=Forgeborne.Core.AimTargetKind.Solid || !hit.Distance.HasValue || hit.X!=5 || hit.Y!=5 || hit.Z!=5)
        throw new System.InvalidOperationException("Production aim did not resolve visible mineral ownership.");
    s.FireAimed(out var shot);g.FpsAdapter.Feedback.Shot(shot);
    if(!shot.HitDistance.HasValue || !g.FpsAdapter.Feedback.TraceVisible)
        throw new System.InvalidOperationException("Surface shot lost resolved distance/production feedback.");
    var inspect=s.InspectAim(false);
    if(s.GatherAhead()!=Forgeborne.Core.GatherResult.Success)throw new System.InvalidOperationException("Production surface gather failed.");
    var editTimer=System.Diagnostics.Stopwatch.StartNew();g.MeshRenderer.RefreshMesh();editTimer.Stop();
    int changedRegions=g.MeshRenderer.LastRegionsRebuilt;
    RaycastHit afterHit;
    if(chunk.Get(inspect.X,inspect.Y,inspect.Z)!=Forgeborne.Core.BlockType.Air)throw new System.InvalidOperationException("Gathered material remains.");
    foreach(var collider in g.MeshRenderer.GetComponentsInChildren<MeshCollider>())
        if(collider.sharedMesh!=null && collider.Raycast(new Ray(new Vector3(2,5.5f,5.5f),Vector3.right),out afterHit,10))
            throw new System.InvalidOperationException("Gathered mineral remains in actual physical mesh.");
    s.Fps.Teleport(16.5f,4,16.5f);s.AdvanceFrame(0,0,0,false);
    for(int i=0;i<60;i++)s.AdvanceFrame(1f/120,1,0,false);
    if(Mathf.Abs(s.Fps.Y-4)>0.0002f || !s.Fps.IsGrounded)
        throw new System.InvalidOperationException("Shared surface motion left the actual floor.");
    string continuous=g.SaveToJson();
    if(!g.TryLoadFromJson(continuous) || g.SaveToJson()!=continuous || !object.ReferenceEquals(s,g.Simulation))
        throw new System.InvalidOperationException("Continuous profile did not restore exact state in place.");
    return new{triangles=geometry.TriangleCount,vertices=vertexCount,regions=colliders.Count,firstMeshAndBvhMilliseconds=timer.Elapsed.TotalMilliseconds,
        gatherRebuildMilliseconds=editTimer.Elapsed.TotalMilliseconds,gatherRegionsRebuilt=changedRegions,
        actualMeshBufferIdentity=true,actualColliderRays=compared,maximumRayDifferenceMetres=maximumDifference,
        productionGatherRemovedGeometry=true,actualShotDistanceFeedback=true,sharedFloorMotion=true,exactSaveRecovery=true,
        badTransformRejected=badTransformRejected,
        sameSimulation=true,scope="Scripted paused fixture; final authored world/actors/construction/native/performance/quality remain open"};
}
finally
{
    if(!g.TryLoadFromJson(before) || !object.ReferenceEquals(s,g.Simulation) || !object.ReferenceEquals(fps,s.Fps)
        || !object.ReferenceEquals(look,s.Look) || g.SaveToJson()!=before)
        throw new System.InvalidOperationException("Surface probe did not restore the original exact state and owners.");
}
