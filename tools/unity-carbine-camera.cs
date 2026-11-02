var g=UnityEngine.Object.FindFirstObjectByType<GameManager>();var flow=UnityEngine.Object.FindFirstObjectByType<PlayerFlow>();
var rig=UnityEngine.Object.FindFirstObjectByType<OrbitalCameraRig>();
if(!Application.isPlaying || g==null || g.Simulation==null || flow==null || !flow.IsMenuOpen || Time.timeScale!=0 || rig==null)
    throw new System.InvalidOperationException("Paused initialized actual Main required.");
var s=g.Simulation;string before=g.SaveToJson();bool oldMode=rig.IsOrbital;
var camera=g.FpsAdapter.GetComponent<Camera>();var oldPosition=camera.transform.position;var oldRotation=camera.transform.rotation;
var active=RenderTexture.active;var previous=camera.targetTexture;var target=new RenderTexture(1920,1080,24);
var pixels=new Texture2D(1920,1080,TextureFormat.RGB24,false);
string directory=System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,"../../docs/evidence/2026-11-02/carbine-0127"));
try
{
    rig.SetOrbital(false);rig.ViewModel.ResetFeedbackPose();
    s.Fps.Teleport(16.5f,4,7.5f);s.AdvanceFrame(0,0,0,false);s.ApplyLook(-s.Look.Yaw,4-s.Look.Pitch);
    camera.transform.SetPositionAndRotation(new Vector3(s.Fps.X,s.Fps.Y+Forgeborne.Core.Simulation.EyeHeight,s.Fps.Z),Quaternion.Euler(-s.Look.Pitch,s.Look.Yaw,0));
    camera.targetTexture=target;camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,1920,1080),0,0);pixels.Apply();
    string path=System.IO.Path.Combine(directory,"carbine-fps-rest.png");System.IO.File.WriteAllBytes(path,pixels.EncodeToPNG());
    return new{image=path,actualProductionCamera=true,restPose=true,overlayUiIncluded=false,scope="Original weapon/glove visual inspection, not native input or final art acceptance."};
}
finally
{
    camera.targetTexture=previous;RenderTexture.active=active;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(pixels);
    rig.SetOrbital(oldMode);rig.ViewModel.ResetFeedbackPose();
    if(!g.TryLoadFromJson(before) || g.SaveToJson()!=before || !object.ReferenceEquals(s,g.Simulation))throw new System.InvalidOperationException("Carbine camera inspection changed canonical state.");
    camera.transform.SetPositionAndRotation(oldPosition,oldRotation);
}
