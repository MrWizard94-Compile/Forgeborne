// Paused canonical shot fixture, camera-only render (screen-overlay HUD excluded).
// Rendering does not establish real input, audible output or fun.
var g=UnityEngine.Object.FindFirstObjectByType<GameManager>();
var flow=UnityEngine.Object.FindFirstObjectByType<PlayerFlow>();
if(!Application.isPlaying || g==null || g.Simulation==null || flow==null || !flow.IsMenuOpen || Time.timeScale!=0)
    throw new System.InvalidOperationException("Started paused Main required.");
var s=g.Simulation;
var camera=g.FpsAdapter.GetComponent<Camera>();
string before=g.SaveToJson();
var oldPosition=camera.transform.position;
var oldRotation=camera.transform.rotation;
var oldTarget=camera.targetTexture;
var oldActive=RenderTexture.active;
var target=new RenderTexture(1920,1080,24);
var pixels=new Texture2D(1920,1080,TextureFormat.RGB24,false);
string output=System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,"../../docs/evidence/2026-11-01/action-camera-prototype.png"));
try
{
    s.World.Enemies.Clear();
    s.World.Enemies.Add(new Forgeborne.Core.Enemy(16,5,19));
    s.Fps.Teleport(16.5f,4,16.5f);
    s.AdvanceFrame(0,0,0,false);
    float pitch=(float)(System.Math.Atan2(-0.1,3)*180/System.Math.PI);
    s.ApplyLook(-s.Look.Yaw,pitch-s.Look.Pitch);
    camera.transform.SetPositionAndRotation(new Vector3(s.Fps.X,s.Fps.Y+Forgeborne.Core.Simulation.EyeHeight,s.Fps.Z),Quaternion.Euler(-s.Look.Pitch,s.Look.Yaw,0));
    UnityEngine.Object.FindFirstObjectByType<WorldActorDisplay>().SendMessage("LateUpdate");
    typeof(FpsInputAdapter).GetMethod("FireWithFeedback",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(g.FpsAdapter,null);
    g.FpsAdapter.GetComponent<WeaponViewModel>().SendMessage("LateUpdate");
    if(s.World.Enemies[0].Health!=20 || !g.FpsAdapter.Feedback.TraceVisible)
        throw new System.InvalidOperationException("Canonical shot fixture failed.");
    camera.targetTexture=target;
    camera.Render();
    RenderTexture.active=target;
    pixels.ReadPixels(new Rect(0,0,1920,1080),0,0);
    pixels.Apply();
    System.IO.File.WriteAllBytes(output,pixels.EncodeToPNG());
    return new {file=output,width=1920,height=1080,canonicalDamage=10,overlayUiIncluded=false,pausedFixture=true};
}
finally
{
    camera.targetTexture=oldTarget;
    RenderTexture.active=oldActive;
    target.Release();
    UnityEngine.Object.DestroyImmediate(target);
    UnityEngine.Object.DestroyImmediate(pixels);
    if(!g.TryLoadFromJson(before) || !object.ReferenceEquals(s,g.Simulation) || g.SaveToJson()!=before)
        throw new System.InvalidOperationException("Action render did not restore exact canonical state.");
    camera.transform.SetPositionAndRotation(oldPosition,oldRotation);
    UnityEngine.Object.FindFirstObjectByType<WorldActorDisplay>().SendMessage("LateUpdate");
}
