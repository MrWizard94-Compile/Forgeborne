// Internal camera diagnostic of the existing rejected layout under the new surface.
// Actual production camera/skins; paused, no UI, no native-input/final-art claim.
var g=UnityEngine.Object.FindFirstObjectByType<GameManager>();
var flow=UnityEngine.Object.FindFirstObjectByType<PlayerFlow>();
var rig=UnityEngine.Object.FindFirstObjectByType<OrbitalCameraRig>();
if(!Application.isPlaying || g==null || g.Simulation==null || flow==null || !flow.IsMenuOpen || Time.timeScale!=0 || rig==null)
    throw new System.InvalidOperationException("Started paused Main required.");
var s=g.Simulation;string before=g.SaveToJson();bool oldMode=rig.IsOrbital;
var fpsCamera=g.FpsAdapter.GetComponent<Camera>();var oldPosition=fpsCamera.transform.position;var oldRotation=fpsCamera.transform.rotation;
var active=RenderTexture.active;var target=new RenderTexture(1920,1080,24);var pixels=new Texture2D(1920,1080,TextureFormat.RGB24,false);
var directory=System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,"../../docs/evidence/2026-11-01"));
try
{
    s.World.Chunk.SurfaceProfile=Forgeborne.Core.SurfaceProfile.ContinuousTerrainV1;g.MeshRenderer.RefreshMesh();
    s.Fps.Teleport(16.5f,4,16.5f);s.AdvanceFrame(0,0,0,false);s.ApplyLook(-s.Look.Yaw,7-s.Look.Pitch);
    fpsCamera.transform.SetPositionAndRotation(new Vector3(s.Fps.X,s.Fps.Y+Forgeborne.Core.Simulation.EyeHeight,s.Fps.Z),Quaternion.Euler(-s.Look.Pitch,s.Look.Yaw,0));
    UnityEngine.Object.FindFirstObjectByType<WorldActorDisplay>().SendMessage("LateUpdate");
    rig.ViewModel.SendMessage("LateUpdate");
    var paths=new System.Collections.Generic.List<string>();
    foreach(bool orbital in new[]{false,true})
    {
        rig.SetOrbital(orbital);var camera=orbital ? rig.GetComponent<Camera>() : fpsCamera;
        var previous=camera.targetTexture;
        try
        {
            camera.targetTexture=target;camera.Render();RenderTexture.active=target;
            pixels.ReadPixels(new Rect(0,0,1920,1080),0,0);pixels.Apply();
            string path=System.IO.Path.Combine(directory,orbital ? "continuous-orbital-diagnostic.png" : "continuous-fps-diagnostic.png");
            System.IO.File.WriteAllBytes(path,pixels.EncodeToPNG());paths.Add(path);
        }
        finally {camera.targetTexture=previous;}
    }
    return new{files=paths,actualCameras=true,overlayUiIncluded=false,pausedFixture=true,
        acceptance="Rejected layout compatibility diagnostic; no final-art/native/input/fun acceptance"};
}
finally
{
    RenderTexture.active=active;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(pixels);
    rig.SetOrbital(oldMode);
    if(!g.TryLoadFromJson(before) || !object.ReferenceEquals(s,g.Simulation) || g.SaveToJson()!=before)
        throw new System.InvalidOperationException("Surface render did not restore exact state in the original Simulation.");
    fpsCamera.transform.SetPositionAndRotation(oldPosition,oldRotation);
    UnityEngine.Object.FindFirstObjectByType<WorldActorDisplay>().SendMessage("LateUpdate");
}
