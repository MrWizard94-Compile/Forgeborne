var g=UnityEngine.Object.FindFirstObjectByType<GameManager>();
var view=UnityEngine.Object.FindFirstObjectByType<WeaponViewModel>();
int start=SessionState.GetInt("Forgeborne.Framing.Frame",-1);
if(!Application.isPlaying || g==null || g.Simulation==null || view==null || Time.timeScale!=0
    || !SessionState.GetBool("Forgeborne.Framing.Active",false) || Time.frameCount<=start+1)
    throw new System.InvalidOperationException("Framing capture requires later actual player/render frames.");
var simulation=g.Simulation;
if(g.SaveToJson()!=SessionState.GetString("Forgeborne.Framing.Snapshot",""))
    throw new System.InvalidOperationException("Framing study changed canonical state.");
var camera=g.FpsAdapter.GetComponent<Camera>();
var previous=camera.targetTexture;var active=RenderTexture.active;
var target=new RenderTexture(1920,1080,24);var pixels=new Texture2D(1920,1080,TextureFormat.RGB24,false);
try
{
    camera.targetTexture=target;camera.Render();RenderTexture.active=target;
    pixels.ReadPixels(new Rect(0,0,1920,1080),0,0);pixels.Apply();
    string label=SessionState.GetString("Forgeborne.Framing.Label","");
    string directory=System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,"../../docs/evidence/2026-11-02/framing-0128/candidates"));
    System.IO.Directory.CreateDirectory(directory);
    string path=System.IO.Path.Combine(directory,label+".png");System.IO.File.WriteAllBytes(path,pixels.EncodeToPNG());
    var transforms=view.CameraAnchor.GetComponentsInChildren<Transform>(true);
    var right=camera.WorldToViewportPoint(transforms.Single(t=>t.name=="RightPalmContact").position);
    var left=camera.WorldToViewportPoint(transforms.Single(t=>t.name=="LeftPalmContact").position);
    return new{image=path,offset=view.ViewOffset.ToString(),fieldOfView=camera.fieldOfView,
        rightPalmRestLandmarkViewport=right.ToString(),leftPalmRestLandmarkViewport=left.ToString(),
        normalFramesCrossed=Time.frameCount-start,actualSkinnedRenderer=true,canonicalStateUnchanged=true,
        scope="Actual FPS camera and original geometry. Rest landmarks are projection diagnostics, not visible-pixel, native-input, final-art or fun acceptance."};
}
finally
{
    camera.targetTexture=previous;RenderTexture.active=active;target.Release();
    UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(pixels);
    view.ViewOffset=new Vector3(SessionState.GetFloat("Forgeborne.Framing.X",0),
        SessionState.GetFloat("Forgeborne.Framing.Y",0),SessionState.GetFloat("Forgeborne.Framing.Z",0));
    view.ViewAngles=new Vector3(SessionState.GetFloat("Forgeborne.Framing.AngleX",0),
        SessionState.GetFloat("Forgeborne.Framing.AngleY",0),SessionState.GetFloat("Forgeborne.Framing.AngleZ",0));
    view.ResetFeedbackPose();Application.runInBackground=SessionState.GetBool("Forgeborne.Framing.Background",false);
    view.enabled=SessionState.GetBool("Forgeborne.Framing.Enabled",true);
    SessionState.SetBool("Forgeborne.Framing.Active",false);
    if(g.SaveToJson()!=SessionState.GetString("Forgeborne.Framing.Snapshot","") || !object.ReferenceEquals(simulation,g.Simulation))
        throw new System.InvalidOperationException("Framing capture did not preserve canonical state/ownership.");
}
