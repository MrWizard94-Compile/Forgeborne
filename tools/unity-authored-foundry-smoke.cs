// Existing paused Main only. Actual render/collider consumer; no native/input/fun claim.
var g=UnityEngine.Object.FindFirstObjectByType<GameManager>();
var flow=UnityEngine.Object.FindFirstObjectByType<PlayerFlow>();
var rig=UnityEngine.Object.FindFirstObjectByType<OrbitalCameraRig>();
var atmosphere=UnityEngine.Object.FindFirstObjectByType<FoundryAtmosphere>();
if(!Application.isPlaying || g==null || g.Simulation==null || flow==null || !flow.IsMenuOpen || Time.timeScale!=0 || rig==null)
    throw new System.InvalidOperationException("Started paused Main required.");
var s=g.Simulation;var fps=s.Fps;var look=s.Look;string original=g.SaveToJson();
var camera=g.FpsAdapter.GetComponent<Camera>();var oldPosition=camera.transform.position;var oldRotation=camera.transform.rotation;bool oldMode=rig.IsOrbital;
var directory=System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,"../../docs/evidence/2026-11-01"));
try
{
    var timer=System.Diagnostics.Stopwatch.StartNew();
    bool entryAlreadyAuthored=s.World.Chunk.SurfaceProfile==Forgeborne.Core.SurfaceProfile.AuthoredFoundryV1;
    if(!entryAlreadyAuthored)Forgeborne.Core.AuthoredFoundry.Populate(s.World);
    timer.Stop();double contentLoad=timer.Elapsed.TotalMilliseconds;
    s.Fps.Teleport(16.5f,4,7.5f);s.AdvanceFrame(0,0,0,false);s.ApplyLook(-s.Look.Yaw,4-s.Look.Pitch);
    timer.Restart();var geometry=s.World.Chunk.Geometry;g.MeshRenderer.RefreshMesh();timer.Stop();double firstBuild=timer.Elapsed.TotalMilliseconds;
    if(atmosphere!=null)atmosphere.SendMessage("LateUpdate");
    UnityEngine.Object.FindFirstObjectByType<WorldActorDisplay>().SendMessage("LateUpdate");
    var regions=geometry.Regions.ToDictionary(r=>r.Id);
    var colliders=new System.Collections.Generic.List<MeshCollider>();int vertices=0,triangles=0,practicals=0;
    foreach(var collider in g.MeshRenderer.GetComponentsInChildren<MeshCollider>())
    {
        if(collider.sharedMesh==null)continue;
        string label=collider.name;int bracket=label.LastIndexOf('[');
        if(bracket<0)throw new System.InvalidOperationException("Legacy geometry remained active in the authored world.");
        int id=int.Parse(label.Substring(bracket+1,label.Length-bracket-2));var region=regions[id];var expected=region.BuildMesh();
        var mesh=collider.GetComponent<MeshFilter>().sharedMesh;var material=collider.GetComponent<MeshRenderer>().sharedMaterial;
        if(mesh!=collider.sharedMesh || mesh.vertexCount!=expected.Vertices.Count || mesh.triangles.Length!=expected.Indices.Count
            || material!=Resources.Load<Material>("Art/Materials/"+region.MaterialRole) || material.shader.name!="Forgeborne/IndustrialSurface")
            throw new System.InvalidOperationException("Authored mesh/collider/material consumer mismatch.");
        var actual=mesh.vertices;var indices=mesh.triangles;
        for(int i=0;i<actual.Length;i++)
        {
            var v=expected.Vertices[i];
            if(actual[i]!=new Vector3(v.Item1,v.Item2,v.Item3))throw new System.InvalidOperationException("Authored vertex mismatch.");
        }
        for(int i=0;i<indices.Length;i++)if(indices[i]!=expected.Indices[i])throw new System.InvalidOperationException("Authored index mismatch.");
        vertices+=actual.Length;triangles+=indices.Length/3;colliders.Add(collider);
        practicals+=collider.GetComponentsInChildren<Light>().Count(l=>l.enabled);
    }
    if(triangles!=geometry.TriangleCount || practicals!=14)throw new System.InvalidOperationException("Authored world or eight warm/six bay practical fixtures are incomplete.");
    Physics.SyncTransforms();
    var random=new System.Random(124);int rays=0;float maximumDifference=0;
    for(int i=0;i<300;i++)
    {
        Vector3 origin,direction;
        if(i<100){origin=new Vector3(4,4.5f+(float)random.NextDouble()*2,14.3f+(float)random.NextDouble()*1.8f);direction=Vector3.right;}
        else if(i<200){origin=new Vector3(9+(float)random.NextDouble()*14,8,8+(float)random.NextDouble()*16);direction=Vector3.down;}
        else{origin=new Vector3(16,6+(float)random.NextDouble()*3,7.5f);direction=new Vector3((float)random.NextDouble()-.5f,(float)random.NextDouble()*.2f,1).normalized;}
        Forgeborne.Core.SurfaceRayHit hit;RaycastHit physical;
        bool core=geometry.Raycast(new System.Numerics.Vector3(origin.x,origin.y,origin.z),new System.Numerics.Vector3(direction.x,direction.y,direction.z),50,out hit);
        bool unity=false;float nearest=50;
        foreach(var collider in colliders)
            if(collider.Raycast(new Ray(origin,direction),out physical,50) && physical.distance<=nearest){unity=true;nearest=physical.distance;}
        if(core!=unity)throw new System.InvalidOperationException("Actual authored collider hit disagreement at ray "+i);
        if(core)
        {
            float difference=Mathf.Abs(hit.Distance-nearest);maximumDifference=Mathf.Max(maximumDifference,difference);
            if(difference>0.0005f)throw new System.InvalidOperationException("Actual authored collider distance disagreement at ray "+i);
        }
        rays++;
    }
    // Capture the actual production cameras before disturbing the first room.
    var paths=new System.Collections.Generic.List<string>();var oldTarget=RenderTexture.active;
    var target=new RenderTexture(1920,1080,24);var pixels=new Texture2D(1920,1080,TextureFormat.RGB24,false);
    try
    {
        camera.transform.SetPositionAndRotation(new Vector3(s.Fps.X,s.Fps.Y+Forgeborne.Core.Simulation.EyeHeight,s.Fps.Z),Quaternion.Euler(-s.Look.Pitch,s.Look.Yaw,0));
        rig.ViewModel.SendMessage("LateUpdate");
        foreach(bool orbital in new[]{false,true})
        {
            rig.SetOrbital(orbital);rig.FocusPlayer();rig.SendMessage("LateUpdate");var view=orbital ? rig.GetComponent<Camera>() : camera;
            var previous=view.targetTexture;
            try
            {
                view.targetTexture=target;view.Render();RenderTexture.active=target;
                pixels.ReadPixels(new Rect(0,0,1920,1080),0,0);pixels.Apply();
                string path=System.IO.Path.Combine(directory,"authored-foundry-"+(orbital ? "orbital-" : "fps-")+Application.version+".png");
                System.IO.File.WriteAllBytes(path,pixels.EncodeToPNG());paths.Add(path);
            }
            finally{view.targetTexture=previous;}
        }
    }
    finally{RenderTexture.active=oldTarget;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(pixels);}
    rig.SetOrbital(false);
    s.Fps.Teleport(11.5f,4,9.2f);s.AdvanceFrame(0,0,0,false);s.ApplyLook(-s.Look.Yaw,-31-s.Look.Pitch);
    var aim=s.InspectAim(false);int oldOre=s.World.Player.Inventory.Get(Forgeborne.Core.BlockType.EmberOre);
    if(aim.X!=11 || aim.Y!=4 || aim.Z!=11 || s.GatherAhead()!=Forgeborne.Core.GatherResult.Success
        || s.World.Player.Inventory.Get(Forgeborne.Core.BlockType.EmberOre)!=oldOre+1)
        throw new System.InvalidOperationException("Authored production gather failed.");
    timer.Restart();g.MeshRenderer.RefreshMesh();timer.Stop();double gather=timer.Elapsed.TotalMilliseconds;
    int afterTriangles=0;
    foreach(var collider in g.MeshRenderer.GetComponentsInChildren<MeshCollider>())
        if(collider.sharedMesh!=null)afterTriangles+=collider.sharedMesh.triangles.Length/3;
    if(afterTriangles!=s.World.Chunk.Geometry.TriangleCount || afterTriangles>=triangles)
        throw new System.InvalidOperationException("Removed deposit retained orphan visual/physical geometry.");
    if(g.MeshRenderer.LastRegionsRebuilt!=0)throw new System.InvalidOperationException("Unaffected authored meshes were unnecessarily rebuilt.");
    s.World.Player.Inventory.Add(Forgeborne.Core.BlockType.Stone,50);s.World.Player.Inventory.Add(Forgeborne.Core.BlockType.EmberOre,50);
    if(s.World.Build(16,4,13,Forgeborne.Core.BlockType.Workstation)!=Forgeborne.Core.BuildResult.Success)
        throw new System.InvalidOperationException("Authored construction failed.");
    g.MeshRenderer.RefreshMesh();int created=g.MeshRenderer.LastRegionsRebuilt;
    if(created<3)throw new System.InvalidOperationException("Constructed authored machine components are missing.");
    string snapshot=g.SaveToJson();
    if(!g.TryLoadFromJson(snapshot) || g.SaveToJson()!=snapshot || !object.ReferenceEquals(s,g.Simulation))
        throw new System.InvalidOperationException("Authored exact in-place save recovery failed.");
    return new{version=Application.version,contentId=Forgeborne.Core.AuthoredFoundry.ContentId,contentSha256=Forgeborne.Core.AuthoredFoundry.ContentSha256,
        sourcePieces=Forgeborne.Core.AuthoredFoundry.SourcePartCount,activeRegions=colliders.Count,triangles=triangles,vertices=vertices,
        entryAlreadyAuthored=entryAlreadyAuthored,contentPopulationMilliseconds=contentLoad,compoundAndUploadMilliseconds=firstBuild,gatherRefreshMilliseconds=gather,
        gatherUnaffectedMeshesRebuilt=0,constructedMachinePieces=created,actualColliderRays=rays,maximumRayDifferenceMetres=maximumDifference,
        activePracticalLights=practicals,exactBuffersAndMaterialConsumers=true,productionGather=true,constructedOriginalMeshes=true,
        exactSaveRecovery=true,sameSimulation=true,cameraFiles=paths,
        limits="Actual Main entry followed by paused scripted interactions. Not native input, representative performance, final art, audio or fun acceptance. Continuous actor migration is open."};
}
finally
{
    rig.SetOrbital(oldMode);
    if(!g.TryLoadFromJson(original) || g.SaveToJson()!=original || !object.ReferenceEquals(s,g.Simulation)
        || !object.ReferenceEquals(fps,s.Fps) || !object.ReferenceEquals(look,s.Look))
        throw new System.InvalidOperationException("Authored fixture failed exact recovery of original state and owners.");
    camera.transform.SetPositionAndRotation(oldPosition,oldRotation);
    if(atmosphere!=null)atmosphere.SendMessage("LateUpdate");
    UnityEngine.Object.FindFirstObjectByType<WorldActorDisplay>().SendMessage("LateUpdate");
}
