// Production adapter/Core boundary in the existing paused Main. No synthetic input claims.
if (!Application.isPlaying) throw new System.InvalidOperationException("Started Play required.");
var g=UnityEngine.Object.FindFirstObjectByType<GameManager>();
var flow=UnityEngine.Object.FindFirstObjectByType<PlayerFlow>();
if(g==null || g.Simulation==null || flow==null || !flow.IsMenuOpen || Time.timeScale!=0)
    throw new System.InvalidOperationException("Started paused Main required.");
var s=g.Simulation;
var feedback=g.FpsAdapter.Feedback;
if(feedback==null) throw new System.InvalidOperationException("Production feedback required.");
string original=g.SaveToJson();
var build=typeof(FpsInputAdapter).GetMethod("BuildWithFeedback",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
System.Action place=delegate {build.Invoke(g.FpsAdapter,new object[]{Forgeborne.Core.BlockType.Wall,new System.Func<Forgeborne.Core.BuildResult>(s.BuildWallAhead)});};
System.Action<float,float,float> aim=delegate(float x,float y,float z)
{
    float dx=x-s.Fps.X,dy=y-s.Fps.Y-Forgeborne.Core.Simulation.EyeHeight,dz=z-s.Fps.Z;
    float yaw=(float)(System.Math.Atan2(dx,dz)*180/System.Math.PI);
    float pitch=(float)(System.Math.Atan2(dy,System.Math.Sqrt(dx*dx+dz*dz))*180/System.Math.PI);
    s.ApplyLook(yaw-s.Look.Yaw,pitch-s.Look.Pitch);
};
try
{
    s.Fps.Teleport(16.5f,4,16.5f);
    s.AdvanceFrame(0,0,0,false);
    s.World.Player.Inventory.Add(Forgeborne.Core.BlockType.Stone,20);
    s.ApplyLook(-s.Look.Yaw,-89-s.Look.Pitch);
    string before=g.SaveToJson();
    place();
    if(g.SaveToJson()!=before || feedback.Status!="BUILD SPACE OCCUPIED · AIM ELSEWHERE")
        throw new System.InvalidOperationException("Suit-overlap build changed state or missed its real refusal message.");

    aim(18.5f,3.5f,16.5f);
    var target=s.InspectAim(false);
    if(target.Kind!=Forgeborne.Core.AimTargetKind.Solid || s.Fps.OverlapsCell(target.PrevX,target.PrevY,target.PrevZ))
        throw new System.InvalidOperationException("Valid-build fixture must target supported air beyond the suit.");
    int resources=s.World.Player.Inventory.Get(Forgeborne.Core.BlockType.Stone),count=s.BlocksBuilt;
    place();
    if(s.BlocksBuilt!=count+1 || s.World.Player.Inventory.Get(Forgeborne.Core.BlockType.Stone)!=resources-2
        || s.World.Chunk.Get(target.PrevX,target.PrevY,target.PrevZ)!=Forgeborne.Core.BlockType.Wall
        || feedback.Status!="WALL PLACED")
        throw new System.InvalidOperationException("Clear supported build lost its original placement/cost/feedback.");

    s.World.Chunk.TrySet(target.PrevX,target.PrevY,target.PrevZ,Forgeborne.Core.BlockType.Air);
    var enemy=new Forgeborne.Core.Enemy(target.PrevX,target.PrevY,target.PrevZ);
    s.World.Enemies.Add(enemy);
    before=g.SaveToJson();
    place();
    if(g.SaveToJson()!=before || feedback.Status!="BUILD SPACE OCCUPIED · AIM ELSEWHERE")
        throw new System.InvalidOperationException("Live-warden overlap changed state or missed refusal feedback.");
    if(s.World.Tick!=0 || !object.ReferenceEquals(s,g.FpsAdapter.Simulation))
        throw new System.InvalidOperationException("Construction changed time or Simulation ownership.");
    return new {suitRefused=true,wardenRefused=true,exactRefusalStatePreserved=true,
        validPlacementAndCost=true,productionFeedback=true,sameSimulation=true};
}
finally
{
    if(!g.TryLoadFromJson(original) || !object.ReferenceEquals(s,g.Simulation) || g.SaveToJson()!=original)
        throw new System.InvalidOperationException("Construction fixture did not restore exact state in place.");
}
