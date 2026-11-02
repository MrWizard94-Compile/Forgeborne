var g=UnityEngine.Object.FindFirstObjectByType<GameManager>();var flow=UnityEngine.Object.FindFirstObjectByType<PlayerFlow>();
if(!Application.isPlaying || g==null || g.Simulation==null || flow==null || !flow.IsMenuOpen || Time.timeScale!=0)
    throw new System.InvalidOperationException("Actual paused Main required.");
var s=g.Simulation;var fps=s.Fps;string original=g.SaveToJson();int scenarios=0;
var results=new System.Collections.Generic.List<object>();
try
{
    foreach(var profile in new[]{Forgeborne.Core.SurfaceProfile.AuthoredFoundryV1,Forgeborne.Core.SurfaceProfile.AuthoredFoundryV2})
    {
        Forgeborne.Core.AuthoredFoundry.Populate(s.World,profile);
        var starts=new[]{new Vector3(16.5f,4,16.5f),new Vector3(10,4,23),new Vector3(10,4,23)};
        var directions=new[]{Vector3.left,Vector3.left,Vector3.back};
        for(int scenario=0;scenario<(profile==Forgeborne.Core.SurfaceProfile.AuthoredFoundryV1 ? 1 : 3);scenario++)
        {
            var start=starts[scenario];var direction=directions[scenario];fps.Teleport(start.x,start.y,start.z);
            fps.UpdateWorld(1f/120,0,0,false,s.World.Chunk);bool contact=false;Vector3 at=Vector3.zero;
            for(int step=0;step<600;step++)
            {
                var before=new Vector3(fps.X,fps.Y,fps.Z);fps.UpdateWorld(1f/120,direction.x,direction.z,false,s.World.Chunk);
                at=new Vector3(fps.X,fps.Y,fps.Z);
                if(new Vector2(at.x-before.x,at.z-before.z).sqrMagnitude<.0000000001f){contact=true;break;}
            }
            for(int step=0;step<20;step++)fps.UpdateWorld(1f/120,-direction.x,-direction.z,false,s.World.Chunk);
            var end=new Vector3(fps.X,fps.Y,fps.Z);float retreat=Vector3.Dot(end-at,-direction);
            if(!contact || retreat<.8f || s.World.Chunk.Geometry.OverlapsBody(new System.Numerics.Vector3(fps.X,fps.Y+.002f,fps.Z)))
                throw new System.InvalidOperationException("Actual controller failed contact/release for "+profile+" scenario "+scenario);
            results.Add(new{profile=(int)profile,scenario=scenario,contact=new[]{at.x,at.y,at.z},retreatMetres=retreat});scenarios++;
        }
        fps.Teleport(6.0127454f,3.9999995f,16.5f);
        for(int step=0;step<20;step++)fps.UpdateWorld(1f/120,1,0,false,s.World.Chunk);
        if(fps.X<6.8f)throw new System.InvalidOperationException("Retained contact pose still locks "+profile);
        fps.Teleport(16.5f,4,4.5f);fps.UpdateWorld(1f/120,0,0,false,s.World.Chunk);
        for(int step=0;step<80;step++)fps.UpdateWorld(1f/120,1,-1,false,s.World.Chunk);
        if(fps.X<20.3f || fps.Z<3 || fps.Z>3.2f)
            throw new System.InvalidOperationException("Actual wall contact cancelled tangential motion for "+profile);
    }
    return new{oneSimulation=object.ReferenceEquals(s,g.Simulation),sameController=object.ReferenceEquals(fps,s.Fps),
        approachReleaseScenarios=scenarios,details=results,retainedContactRecovery=true,tangentMovement=true,noSlotWrites=true,
        scope="Existing production controller in actual paused Unity Main, both retained profiles. Scripted movement, not native keyboard input or a full playthrough."};
}
finally
{
    if(!g.TryLoadFromJson(original))throw new System.InvalidOperationException("Original Main snapshot recovery failed.");
    g.MeshRenderer.RefreshMesh();
    if(g.SaveToJson()!=original || !object.ReferenceEquals(fps,s.Fps))throw new System.InvalidOperationException("Movement probe changed original state or controller identity.");
}
