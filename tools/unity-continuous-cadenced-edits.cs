// One edit/rollback per elapsed interval and player frame, with actual Editor rendering.
// Temporary background execution is disclosed; no native/FPS/input/quality acceptance.
var g=UnityEngine.Object.FindFirstObjectByType<GameManager>();
var flow=UnityEngine.Object.FindFirstObjectByType<PlayerFlow>();
if(!Application.isPlaying || g==null || g.Simulation==null || flow==null || !flow.IsMenuOpen || Time.timeScale!=0)
    throw new System.InvalidOperationException("Started paused Main required.");
if(UnityEditor.SessionState.GetBool("ForgeborneSurfaceEditsRunning",false))
    throw new System.InvalidOperationException("A surface edit probe is already registered; inspect its callback before another run.");
var s=g.Simulation;string original=g.SaveToJson();bool oldBackground=Application.runInBackground;
var positions=new[]{new Vector3Int(5,5,5),new Vector3Int(3,3,3),new Vector3Int(4,4,4),new Vector3Int(15,3,15)};
string output=System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,"../../docs/evidence/2026-11-01/continuous-cadenced-edits-profile1-final.json"));
if(System.IO.File.Exists(output))throw new System.InvalidOperationException("Prior cadenced evidence exists; preserve it before rerunning.");
var times=new System.Collections.Generic.List<double>();var coreTimes=new System.Collections.Generic.List<double>();
var gcCollections=new System.Collections.Generic.List<int>();
int phase=0,lastFrame=Time.frameCount,startFrame=Time.frameCount,warmUntil=Time.frameCount+10,largestRegions=0;
Forgeborne.Core.BlockType restoreType=Forgeborne.Core.BlockType.Air;
double nextAt=0,largestFrame=0;string failure="";
var wall=System.Diagnostics.Stopwatch.StartNew();
UnityEditor.EditorApplication.CallbackFunction observe=null;
System.Action finish=delegate
{
    UnityEditor.EditorApplication.update-=observe;
    UnityEditor.SessionState.SetBool("ForgeborneSurfaceEditsRunning",false);
    bool restored=false;
    try { if(Application.isPlaying && g!=null)restored=g.TryLoadFromJson(original) && object.ReferenceEquals(s,g.Simulation) && g.SaveToJson()==original; }
    catch(System.Exception error){failure+=";restore:"+error.GetType().Name;}
    finally {Application.runInBackground=oldBackground;}
    var culture=System.Globalization.CultureInfo.InvariantCulture;
    var numbers=new System.Collections.Generic.List<string>();foreach(double value in times)numbers.Add(value.ToString("R",culture));
    var cores=new System.Collections.Generic.List<string>();foreach(double value in coreTimes)cores.Add(value.ToString("R",culture));
    string json="{\"unityVersion\":\""+Application.unityVersion+"\",\"productVersion\":\""+Application.version+"\",\"wallSeconds\":"+wall.Elapsed.TotalSeconds.ToString("R",culture)
        +",\"playerFrames\":"+(Time.frameCount-startFrame)+",\"editMilliseconds\":["+string.Join(",",numbers.ToArray())
        +"],\"geometryMilliseconds\":["+string.Join(",",cores.ToArray())+"],\"gcCollectionsDuringEdits\":["+string.Join(",",gcCollections.ToArray())
        +"],\"largestRegionsRebuilt\":"+largestRegions+",\"maximumObservedUnscaledFrameMilliseconds\":"+(largestFrame*1000).ToString("R",culture)
        +",\"exactStateRestoredInPlace\":"+restored.ToString().ToLowerInvariant()+",\"temporaryBackgroundExecution\":true,\"failure\":\""+failure
        +"\",\"nativeInputPerformanceAndQualityAcceptance\":false}";
    System.IO.File.WriteAllText(output,json);
};
observe=delegate
{
    try
    {
        if(!Application.isPlaying || g==null || !flow.IsMenuOpen || Time.timeScale!=0 || !object.ReferenceEquals(s,g.Simulation))
        {failure="interrupted";finish();return;}
        if(wall.Elapsed.TotalSeconds>15){failure="callback-timeout";finish();return;}
        if(Time.frameCount==lastFrame)return;
        lastFrame=Time.frameCount;if(lastFrame<warmUntil)return;
        largestFrame=System.Math.Max(largestFrame,Time.unscaledDeltaTime);
        if(wall.Elapsed.TotalSeconds<nextAt)return;
        int sample=phase/2;var p=positions[sample/4];
        if(phase%2==0)
        {
            restoreType=s.World.Chunk.Get(p.x,p.y,p.z);
            s.World.Chunk.TrySet(p.x,p.y,p.z,restoreType==Forgeborne.Core.BlockType.Air ? Forgeborne.Core.BlockType.EmberOre : Forgeborne.Core.BlockType.Air);
            int collections=System.GC.CollectionCount(0);
            var timer=System.Diagnostics.Stopwatch.StartNew();var geometry=s.World.Chunk.Geometry;
            double core=timer.Elapsed.TotalMilliseconds;g.MeshRenderer.RefreshMesh();timer.Stop();
            times.Add(timer.Elapsed.TotalMilliseconds);coreTimes.Add(core);gcCollections.Add(System.GC.CollectionCount(0)-collections);
            largestRegions=System.Math.Max(largestRegions,g.MeshRenderer.LastRegionsRebuilt);
        }
        else {s.World.Chunk.TrySet(p.x,p.y,p.z,restoreType);g.MeshRenderer.RefreshMesh();}
        phase++;nextAt=wall.Elapsed.TotalSeconds+0.10;
        if(phase>=32)finish();
    }
    catch(System.Exception error){failure=error.GetType().Name;finish();}
};
try
{
    var chunk=s.World.Chunk;chunk.FromBytes(new byte[Forgeborne.Core.VoxelChunk.Volume]);s.World.Enemies.Clear();
    for(int x=0;x<32;x++)for(int z=0;z<32;z++)for(int y=0;y<4;y++)chunk.TrySet(x,y,z,Forgeborne.Core.BlockType.Stone);
    chunk.SurfaceProfile=Forgeborne.Core.SurfaceProfile.ContinuousTerrainV1;g.MeshRenderer.RefreshMesh();
    Application.runInBackground=true;
    wall.Restart();startFrame=lastFrame=Time.frameCount;warmUntil=startFrame+10;
    UnityEditor.SessionState.SetBool("ForgeborneSurfaceEditsRunning",true);
    UnityEditor.SessionState.SetInt("ForgeborneSurfaceEditsCallbackHash",observe.GetHashCode());
    UnityEditor.EditorApplication.update+=observe;
}
catch
{
    UnityEditor.EditorApplication.update-=observe;UnityEditor.SessionState.SetBool("ForgeborneSurfaceEditsRunning",false);
    Application.runInBackground=oldBackground;g.TryLoadFromJson(original);throw;
}
return new{scheduled=true,edits=16,elapsedSpacingSeconds=0.10,output=output,temporaryBackgroundExecution=true};
