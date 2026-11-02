var g=UnityEngine.Object.FindFirstObjectByType<GameManager>();var rig=UnityEngine.Object.FindFirstObjectByType<OrbitalCameraRig>();
int setFrame=SessionState.GetInt("Forgeborne.CarbineCapture.SetFrame",-1);
float seconds=SessionState.GetFloat("Forgeborne.CarbineCapture.Time",-1);
string mode=SessionState.GetString("Forgeborne.CarbineCapture.Mode","reload");
if(!Application.isPlaying || g==null || g.Simulation==null || rig==null || Time.timeScale!=0 || setFrame<0 || Time.frameCount<=setFrame+1)
    throw new System.InvalidOperationException("Capture requires normal player/render frames after the actual pose setter.");
var view=rig.ViewModel;
var ageField=typeof(WeaponViewModel).GetField("_reloadAge",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
if(Mathf.Abs((float)ageField.GetValue(view)-seconds)>.00001f)
    throw new System.InvalidOperationException("A production pose owner changed the requested paused sample.");
if(g.SaveToJson()!=SessionState.GetString("Forgeborne.CarbineCapture.Snapshot",""))
    throw new System.InvalidOperationException("Canonical state changed between pose and rendered capture.");
var camera=g.FpsAdapter.GetComponent<Camera>();var previous=camera.targetTexture;var active=RenderTexture.active;
var target=new RenderTexture(1920,1080,24);var pixels=new Texture2D(1920,1080,TextureFormat.RGB24,false);
try
{
    camera.targetTexture=target;camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,1920,1080),0,0);pixels.Apply();
    string name=mode=="reload" ? "rendered-reload-"+seconds.ToString("0.00",System.Globalization.CultureInfo.InvariantCulture)+".png" : mode+".png";
    string path=System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,"../../docs/evidence/2026-11-02/carbine-0127",name));
    System.IO.File.WriteAllBytes(path,pixels.EncodeToPNG());
    return new{image=path,mode=mode,seconds=seconds,setFrame=setFrame,captureFrame=Time.frameCount,normalPlayerLoopFramesCrossed=Time.frameCount-setFrame,
        actualSkinnedRenderer=true,canonicalStateUnchanged=true,scope="Actual production pose retained across normal player/render frames before camera capture; no baked replacement mesh, native input or final-quality claim."};
}
finally
{
    camera.targetTexture=previous;RenderTexture.active=active;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(pixels);
    view.ResetFeedbackPose();
    view.enabled=SessionState.GetBool("Forgeborne.CarbineCapture.ViewEnabled",true);
    Application.runInBackground=SessionState.GetBool("Forgeborne.CarbineCapture.RunInBackground",false);
}
