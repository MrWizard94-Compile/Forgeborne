// Four elapsed seconds of the real Main frame driver in orbital mode.
// Background execution is temporary QA configuration, not native/focused-input acceptance.
var g=UnityEngine.Object.FindFirstObjectByType<GameManager>();
var flow=UnityEngine.Object.FindFirstObjectByType<PlayerFlow>();
var rig=UnityEngine.Object.FindFirstObjectByType<OrbitalCameraRig>();
if(!Application.isPlaying || g==null || g.Simulation==null || flow==null || rig==null
    || !flow.IsMenuOpen || Time.timeScale!=0)
    throw new System.InvalidOperationException("Fresh started paused Main required.");
var s=g.Simulation;
if(s.World.Tick!=0) throw new System.InvalidOperationException("Fresh zero-clock fixture required.");
string original=g.SaveToJson();
bool oldBackground=Application.runInBackground,oldOrbital=rig.IsOrbital;
var begunField=typeof(PlayerFlow).GetField("_begun",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
object oldBegun=begunField.GetValue(flow);
string output=System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,"../../docs/evidence/2026-11-01/orbital-background-cadence.json"));
if(System.IO.File.Exists(output)) throw new System.InvalidOperationException("Prior cadence evidence exists; preserve it before another run.");
int startFrame=Time.frameCount,startTick=s.World.Tick;
double gameStart=Time.timeAsDouble;
var stopwatch=System.Diagnostics.Stopwatch.StartNew();
UnityEditor.EditorApplication.CallbackFunction observe=null;
observe=delegate
{
    if(stopwatch.Elapsed.TotalSeconds<4 && Application.isPlaying) return;
    UnityEditor.EditorApplication.update-=observe;
    int frames=Time.frameCount-startFrame,ticks=s.World.Tick-startTick;
    double wall=stopwatch.Elapsed.TotalSeconds,game=Time.timeAsDouble-gameStart;
    bool live=Application.isPlaying && rig.IsOrbital && !flow.IsMenuOpen && Time.timeScale==1
        && g.FpsAdapter.enabled && object.ReferenceEquals(s,g.FpsAdapter.Simulation)
        && object.ReferenceEquals(s,g.Simulation) && !s.IsPlayerDead && !s.IsObjectiveComplete;
    bool cadence=live && frames>0 && game>=3.5 && System.Math.Abs(ticks-game*2)<=1.1;
    bool restored=false;
    string recoveryError="";
    try
    {
        flow.OpenMenu();
        restored=g.TryLoadFromJson(original) && object.ReferenceEquals(s,g.Simulation) && g.SaveToJson()==original;
        rig.SetOrbital(oldOrbital);
        begunField.SetValue(flow,oldBegun);
    }
    catch(System.Exception error) { recoveryError=error.GetType().Name; }
    finally { Application.runInBackground=oldBackground; }
    string json=string.Format(System.Globalization.CultureInfo.InvariantCulture,
        "{{\"unityVersion\":\"{0}\",\"productVersion\":\"{1}\",\"wallSeconds\":{2:R},\"gameSeconds\":{3:R},\"playerFrames\":{4},\"worldTicks\":{5},\"expectedTicksPerSecond\":2,\"liveOrbitalAndSingleDriver\":{6},\"cadencePass\":{7},\"exactStateRestoredInPlace\":{8},\"recoveryError\":\"{9}\",\"temporaryBackgroundExecution\":true,\"nativeInputPerformanceAndFunAcceptance\":false}}",
        Application.unityVersion,Application.version,wall,game,frames,ticks,live.ToString().ToLowerInvariant(),
        cadence.ToString().ToLowerInvariant(),restored.ToString().ToLowerInvariant(),recoveryError);
    System.IO.File.WriteAllText(output,json);
};
try
{
    Application.runInBackground=true;
    rig.SetOrbital(true);
    flow.Resume();
    UnityEditor.EditorApplication.update+=observe;
}
catch
{
    UnityEditor.EditorApplication.update-=observe;
    Application.runInBackground=oldBackground;
    flow.OpenMenu();
    g.TryLoadFromJson(original);
    rig.SetOrbital(oldOrbital);
    begunField.SetValue(flow,oldBegun);
    throw;
}
return new {scheduled=true,seconds=4,output=output,temporaryBackgroundExecution=true};
