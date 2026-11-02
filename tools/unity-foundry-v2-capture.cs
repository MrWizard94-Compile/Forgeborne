var g=UnityEngine.Object.FindFirstObjectByType<GameManager>();var flow=UnityEngine.Object.FindFirstObjectByType<PlayerFlow>();
var rig=UnityEngine.Object.FindFirstObjectByType<OrbitalCameraRig>();
if(!Application.isPlaying || g==null || g.Simulation==null || flow==null || !flow.IsMenuOpen || Time.timeScale!=0 || rig==null || g.Simulation.World.Chunk.SurfaceProfile!=Forgeborne.Core.SurfaceProfile.AuthoredFoundryV2)
    throw new System.InvalidOperationException("Paused actual Main required.");
var s=g.Simulation;string before=g.SaveToJson();bool oldMode=rig.IsOrbital;
var camera=g.FpsAdapter.GetComponent<Camera>();var oldPosition=camera.transform.position;var oldRotation=camera.transform.rotation;
var active=RenderTexture.active;var target=new RenderTexture(1920,1080,24);var pixels=new Texture2D(1920,1080,TextureFormat.RGB24,false);
string directory=System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,"../../docs/evidence/2026-11-02"));
try
{
    var files=new System.Collections.Generic.List<string>();
    var poses=new[]{new Vector4(16.5f,7.5f,0,4),new Vector4(9.5f,13,-65,6),new Vector4(4,15.2f,90,2),new Vector4(11.5f,9.2f,0,-31)};
    for(int index=0;index<poses.Length;index++)
    {
        var p=poses[index];rig.SetOrbital(false);s.Fps.Teleport(p.x,4,p.y);s.AdvanceFrame(0,0,0,false);s.ApplyLook(p.z-s.Look.Yaw,p.w-s.Look.Pitch);
        if(index==3)
        {
            if(s.GatherAhead()!=Forgeborne.Core.GatherResult.Success)throw new System.InvalidOperationException("Actual mineral gather failed before fracture inspection.");
            g.MeshRenderer.RefreshMesh();
        }
        camera.transform.SetPositionAndRotation(new Vector3(s.Fps.X,s.Fps.Y+Forgeborne.Core.Simulation.EyeHeight,s.Fps.Z),Quaternion.Euler(-s.Look.Pitch,s.Look.Yaw,0));
        rig.ViewModel.SendMessage("LateUpdate");UnityEngine.Object.FindFirstObjectByType<WorldActorDisplay>().SendMessage("LateUpdate");
        var previous=camera.targetTexture;
        try
        {
            camera.targetTexture=target;camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,1920,1080),0,0);pixels.Apply();
            string path=System.IO.Path.Combine(directory,"foundry-0126-fps-"+index+".png");System.IO.File.WriteAllBytes(path,pixels.EncodeToPNG());files.Add(path);
        }
        finally{camera.targetTexture=previous;}
    }
    return new{files=files,actualProductionCamera=true,actualGatherFractureInspected=true,originalStateRestoredInFinally=true,overlayUiIncluded=false,scope="Material/lighting visual inspection; no native/input/fun claim."};
}
finally
{
    RenderTexture.active=active;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(pixels);
    rig.SetOrbital(oldMode);
    if(!g.TryLoadFromJson(before) || g.SaveToJson()!=before || !object.ReferenceEquals(s,g.Simulation))throw new System.InvalidOperationException("Surface capture failed exact recovery in the original Simulation.");
    camera.transform.SetPositionAndRotation(oldPosition,oldRotation);
    UnityEngine.Object.FindFirstObjectByType<WorldActorDisplay>().SendMessage("LateUpdate");
}
