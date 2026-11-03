// Executed by the connected Editor's CodeDom runner. One callback-owned job.
var manager=UnityEngine.Object.FindFirstObjectByType<GameManager>();
var flow=UnityEngine.Object.FindFirstObjectByType<PlayerFlow>();
var rig=UnityEngine.Object.FindFirstObjectByType<OrbitalCameraRig>();
if(!Application.isPlaying || manager==null || manager.Simulation==null || flow==null || !flow.IsMenuOpen
    || Time.timeScale!=0 || rig==null || rig.IsOrbital)
    throw new System.InvalidOperationException("Actual paused FPS Main required.");
const string key="Forgeborne.ReloadClip.";
if(SessionState.GetBool(key+"Active",false) || SessionState.GetBool("Forgeborne.Framing.Active",false))
    throw new System.InvalidOperationException("Another view capture owns the presentation.");
string directory=System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,"../../__OUTPUT_DIRECTORY__"));
string evidence=System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,"../../docs/evidence"))+System.IO.Path.DirectorySeparatorChar;
if(!directory.StartsWith(evidence,System.StringComparison.OrdinalIgnoreCase)
    || System.IO.Directory.Exists(directory) && System.IO.Directory.EnumerateFileSystemEntries(directory).Any())
    throw new System.InvalidOperationException("Use a new empty repository evidence directory.");
System.IO.Directory.CreateDirectory(directory);
var view=rig.ViewModel;var camera=manager.FpsAdapter.GetComponent<Camera>();
var simulation=manager.Simulation;string snapshot=manager.SaveToJson();
bool enabled=view.enabled,background=Application.runInBackground;
var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;
const int width=1280,height=720,fps=60;
float duration=WeaponViewModel.ReloadPresentationSeconds;
int intervals=Mathf.CeilToInt(duration*fps),index=0,setFrame=Time.frameCount;
string failure=null,started=System.DateTime.UtcNow.ToString("O");
var samples=new System.Collections.Generic.List<object>();
var target=new RenderTexture(width,height,24);
var pixels=new Texture2D(width,height,TextureFormat.RGBA32,false);
UnityEditor.Media.MediaEncoder encoder=null;
EditorApplication.CallbackFunction callback=null;
System.Action finish=()=>
{
    EditorApplication.update-=callback;
    try { if(encoder!=null)encoder.Dispose(); }
    catch(System.Exception e){failure=(failure??"")+" Encoder dispose: "+e;}
    if(camera!=null)camera.targetTexture=oldTarget;
    RenderTexture.active=oldActive;
    target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(pixels);
    if(view!=null){view.ResetFeedbackPose();view.enabled=enabled;}
    Application.runInBackground=background;
    bool state=manager!=null && manager.SaveToJson()==snapshot;
    bool same=manager!=null && object.ReferenceEquals(simulation,manager.Simulation);
    if(!state || !same || Time.timeScale!=0)failure=(failure??"")+" Canonical ownership/state/pause changed.";
    SessionState.SetBool(key+"Active",false);
    var report=new{success=failure==null,failure=failure,version=Application.version,unityVersion=Application.unityVersion,
        startedUtc=started,finishedUtc=System.DateTime.UtcNow.ToString("O"),width=width,height=height,framesPerSecond=fps,
        durationSeconds=duration,expectedFrames=intervals+1,actualFrames=samples.Count,frames=samples,
        actualSkins=view==null ? 0 : view.CameraAnchor.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length,
        canonicalStateUnchanged=state,sameSimulation=same,viewEnabledRestored=view!=null && view.enabled==enabled,
        runInBackgroundRestored=Application.runInBackground==background,cameraTargetRestored=camera!=null && camera.targetTexture==oldTarget,
        scope="Silent scripted actual Unity camera renders. Each pose crosses normal player frames before capture. Encoded 60fps playback is not native input, realtime performance, human motion or art/fun acceptance."};
    System.IO.File.WriteAllText(System.IO.Path.Combine(directory,"clip-report.json"),Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));
};
callback=()=>
{
    try
    {
        SessionState.SetFloat(key+"Heartbeat",(float)EditorApplication.timeSinceStartup);
        if(SessionState.GetBool(key+"Cancel",false))throw new System.InvalidOperationException("Capture cancelled.");
        if(!Application.isPlaying || flow==null || !flow.IsMenuOpen || Time.timeScale!=0
            || manager==null || !object.ReferenceEquals(simulation,manager.Simulation))
            throw new System.InvalidOperationException("Capture lost paused FPS ownership.");
        if(Time.frameCount<=setFrame+1){EditorApplication.QueuePlayerLoopUpdate();return;}
        try
        {
            camera.targetTexture=target;camera.Render();RenderTexture.active=target;
            pixels.ReadPixels(new Rect(0,0,width,height),0,0);pixels.Apply();
            if(!encoder.AddFrame(pixels))throw new System.InvalidOperationException("MediaEncoder rejected frame.");
            if(index==0 || index==intervals || index%Mathf.Max(1,intervals/8)==0)
                System.IO.File.WriteAllBytes(System.IO.Path.Combine(directory,"frame-"+index.ToString("D4")+".png"),pixels.EncodeToPNG());
        }
        finally {camera.targetTexture=oldTarget;RenderTexture.active=oldActive;}
        samples.Add(new{index=index,elapsedSeconds=duration*index/intervals,setFrame=setFrame,captureFrame=Time.frameCount,
            normalFramesCrossed=Time.frameCount-setFrame});
        SessionState.SetInt(key+"Frames",samples.Count);
        if(index++==intervals){finish();return;}
        view.ResetFeedbackPose();view.ReactReload();view.AdvancePresentation(duration*index/intervals);
        setFrame=Time.frameCount;EditorApplication.QueuePlayerLoopUpdate();
    }
    catch(System.Exception e){failure=e.ToString();finish();}
};
try
{
    encoder=new UnityEditor.Media.MediaEncoder(System.IO.Path.Combine(directory,"reload.mp4"),new UnityEditor.Media.VideoTrackAttributes{
        frameRate=new UnityEditor.Media.MediaRational(fps),width=width,height=height,includeAlpha=false,bitRateMode=UnityEditor.VideoBitrateMode.High});
    view.enabled=false;Application.runInBackground=true;
    view.ResetFeedbackPose();view.ReactReload();view.AdvancePresentation(0);
    SessionState.SetBool(key+"Cancel",false);SessionState.SetBool(key+"Active",true);SessionState.SetInt(key+"Frames",0);
    SessionState.SetString(key+"Directory",directory);
    EditorApplication.update+=callback;EditorApplication.QueuePlayerLoopUpdate();
}
catch(System.Exception e){failure=e.ToString();finish();throw;}
return new{queued=true,directory=directory,expectedFrames=intervals+1,framesPerSecond=fps};
