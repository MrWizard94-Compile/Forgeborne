// Scripted live production-action/presentation check in paused Main.
// No input, audible-mix, native or fun acceptance is implied.
if (!Application.isPlaying) throw new System.InvalidOperationException("Started Play required.");
var g = UnityEngine.Object.FindFirstObjectByType<GameManager>();
var f = UnityEngine.Object.FindFirstObjectByType<PlayerFlow>();
var feedback = UnityEngine.Object.FindFirstObjectByType<PlayerFeedback>();
if (g == null || g.Simulation == null || f == null || !f.IsMenuOpen || Time.timeScale != 0 || feedback == null)
    throw new System.InvalidOperationException("Started paused Main with feedback required.");
var s = g.Simulation;
string before = g.SaveToJson();
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
System.Action<float,float,float> aim = delegate(float x,float y,float z)
{
    float dx=x-s.Fps.X,dy=y-s.Fps.Y-Forgeborne.Core.Simulation.EyeHeight,dz=z-s.Fps.Z;
    float yaw=(float)(System.Math.Atan2(dx,dz)*180/System.Math.PI);
    float pitch=(float)(System.Math.Atan2(dy,System.Math.Sqrt(dx*dx+dz*dz))*180/System.Math.PI);
    s.ApplyLook(yaw-s.Look.Yaw,pitch-s.Look.Pitch);
};
try
{
    s.World.Enemies.Clear();
    var enemy = new Forgeborne.Core.Enemy(16,5,19);
    s.World.Enemies.Add(enemy);
    aim(16.5f,5.5f,19.5f);
    int ammo=s.Weapon.Mag;
    typeof(FpsInputAdapter).GetMethod("FireWithFeedback",flags).Invoke(g.FpsAdapter,null);
    var sound=feedback.GetComponents<AudioSource>().Single(a=>!a.loop);
    if(enemy.Health!=20 || s.Weapon.Mag!=ammo-1 || feedback.Status!="10 DAMAGE" || !feedback.TraceVisible || !sound.isPlaying)
        throw new System.InvalidOperationException("Resolved shot did not reach real damage/ammo/trace/UI/audio source.");
    string afterShot=g.SaveToJson();
    feedback.Gathered(Forgeborne.Core.GatherResult.Air,Forgeborne.Core.BlockType.Air,0,Vector3.zero);
    if(g.SaveToJson()!=afterShot) throw new System.InvalidOperationException("Presentation changed canonical state.");
    // Production reset is used by view swaps, menu/load recovery and restart.
    // It must clear visible/still-playing signals immediately, without a frame.
    g.FpsAdapter.ResetInteraction(false);
    var labels=feedback.GetComponentsInChildren<UnityEngine.UI.Text>(true);
    var hurt=feedback.GetComponentsInChildren<UnityEngine.UI.Image>(true).Single(i=>i.name=="Suit impact");
    if(feedback.TraceVisible || feedback.Status!="" || sound.isPlaying
        || labels.Single(t=>t.name=="Action result").enabled || hurt.color.a!=0
        || feedback.Reticle.color!=Color.white || feedback.Reticle.fontSize!=22
        || g.SaveToJson()!=afterShot)
        throw new System.InvalidOperationException("Production reset retained transient feedback or changed canonical state.");
    aim(12.5f,4.5f,19.5f);
    int gathered=s.BlocksGathered;
    typeof(FpsInputAdapter).GetMethod("GatherWithFeedback",flags).Invoke(g.FpsAdapter,null);
    if(s.BlocksGathered!=gathered+1 || !feedback.Status.Contains("STONE"))
        throw new System.InvalidOperationException("Production gather/feedback path failed.");
    aim(10.5f,4.5f,19.5f);
    var packedAim=s.InspectAim(false);
    if(packedAim.Kind!=Forgeborne.Core.AimTargetKind.Solid
        || s.World.Chunk.Get(packedAim.X,packedAim.Y,packedAim.Z)!=Forgeborne.Core.BlockType.Stone)
        throw new System.InvalidOperationException("Full-pack fixture must aim at real salvage.");
    s.World.Player.Inventory.Add(Forgeborne.Core.BlockType.Stone,Forgeborne.Core.Inventory.MaxStack);
    string packed=g.SaveToJson();
    typeof(FpsInputAdapter).GetMethod("GatherWithFeedback",flags).Invoke(g.FpsAdapter,null);
    if(g.SaveToJson()!=packed || feedback.Status!="PACK FULL · SURFACE KEPT")
        throw new System.InvalidOperationException("Full-pack gather lost state or reported the wrong reason.");
    s.World.Player.Inventory.Add(Forgeborne.Core.BlockType.Stone,20);
    aim(17.5f,3.5f,18.5f);
    int built=s.BlocksBuilt;
    typeof(FpsInputAdapter).GetMethod("BuildWithFeedback",flags).Invoke(g.FpsAdapter,
        new object[]{Forgeborne.Core.BlockType.Wall,new System.Func<Forgeborne.Core.BuildResult>(s.BuildWallAhead)});
    if(s.BlocksBuilt!=built+1 || feedback.Status!="WALL PLACED")
        throw new System.InvalidOperationException("Production build/feedback path failed.");
    int oldMag=s.Weapon.Mag;
    s.Weapon.Reload();
    feedback.Reloaded(oldMag!=s.Weapon.Mag,s.Weapon.Mag==s.Weapon.MagCapacity);
    if(feedback.Status!="MAGAZINE RELOADED") throw new System.InvalidOperationException("Reload facts did not reach presentation.");
    if(s.World.Tick!=0 || !object.ReferenceEquals(s,g.FpsAdapter.Simulation))
        throw new System.InvalidOperationException("Action feedback changed time/Simulation ownership.");
    return new {productionFire=true,traceAndMessage=true,audioSourcePlaying=true,
        productionGatherBuild=true,fullPackPreserved=true,reloadFeedback=true,immediateReset=true,presentationReadOnly=true,sameSimulation=true};
}
finally
{
    if(!g.TryLoadFromJson(before) || !object.ReferenceEquals(s,g.Simulation) || g.SaveToJson()!=before)
        throw new System.InvalidOperationException("Feedback fixture did not restore canonical state in place.");
}
