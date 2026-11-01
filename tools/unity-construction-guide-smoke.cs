// Scripted production routing/presentation check; no actual-key claim.
var g=UnityEngine.Object.FindFirstObjectByType<GameManager>();
var flow=UnityEngine.Object.FindFirstObjectByType<PlayerFlow>();
var rig=UnityEngine.Object.FindFirstObjectByType<OrbitalCameraRig>();
if(!Application.isPlaying || g==null || g.Simulation==null || flow==null || !flow.IsMenuOpen || Time.timeScale!=0)
    throw new System.InvalidOperationException("Fresh started paused Main required.");
var adapter=g.FpsAdapter;
var guide=adapter.Construction;
if(guide==null || guide.Adapter!=adapter) throw new System.InvalidOperationException("Saved guide wiring required.");
var s=g.Simulation;
string original=g.SaveToJson();
bool oldSuppressed=adapter.InputSuppressed,oldOrbital=rig.IsOrbital;
var oldType=guide.SelectedType;
try
{
    s.Fps.Teleport(16.5f,4,16.5f);
    s.AdvanceFrame(0,0,0,false);
    float pitch=(float)(System.Math.Atan2(-2.1,2)*180/System.Math.PI);
    s.ApplyLook(90-s.Look.Yaw,pitch-s.Look.Pitch);
    adapter.InputSuppressed=false;
    guide.Select(Forgeborne.Core.BlockType.Wall);
    guide.Toggle();
    string before=g.SaveToJson();
    guide.RefreshPreview();
    var preview=s.InspectBuildAhead(Forgeborne.Core.BlockType.Wall);
    if(!guide.IsActive || !guide.OutlineVisible || !guide.Label.Contains("STONE 0/2") || !guide.Label.Contains("NEED RESOURCES")
        || !preview.HasCell || g.SaveToJson()!=before)
        throw new System.InvalidOperationException("Resource-short preview missing or mutated canonical state.");
    var line=guide.GetComponentsInChildren<LineRenderer>().Single(l=>l.name=="Construction cell outline");
    if(line.GetComponent<Collider>()!=null || line.GetPosition(0)!=new Vector3(preview.X,preview.Y,preview.Z)-Vector3.one*0.004f)
        throw new System.InvalidOperationException("Guide has a collider or differs from the canonical placement cell.");
    var choices=new System.Collections.Generic.HashSet<Forgeborne.Core.BlockType>();
    for(int i=0;i<9;i++){choices.Add(guide.SelectedType);guide.Cycle(1);}
    if(choices.Count!=9 || guide.SelectedType!=Forgeborne.Core.BlockType.Wall)
        throw new System.InvalidOperationException("Guide cycle did not cover all nine existing structures and wrap.");
    s.World.Player.Inventory.Add(Forgeborne.Core.BlockType.Stone,20);
    before=g.SaveToJson();
    guide.RefreshPreview();
    if(!guide.Label.Contains("READY") || g.SaveToJson()!=before)
        throw new System.InvalidOperationException("Funded preview missing or not read-only.");
    int ammo=s.Weapon.Mag,count=s.BlocksBuilt;
    typeof(FpsInputAdapter).GetMethod("PrimaryAction",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(adapter,null);
    if(s.BlocksBuilt!=count+1 || s.World.Player.Inventory.Get(Forgeborne.Core.BlockType.Stone)!=18 || s.Weapon.Mag!=ammo
        || s.World.Chunk.Get(preview.X,preview.Y,preview.Z)!=Forgeborne.Core.BlockType.Wall)
        throw new System.InvalidOperationException("Production primary action failed placement/payment or fired while constructing.");
    before=g.SaveToJson();
    flow.OpenMenu();
    if(guide.IsActive || guide.OutlineVisible || guide.Label!="" || g.SaveToJson()!=before)
        throw new System.InvalidOperationException("Pause retained the construction tool or changed state.");
    adapter.InputSuppressed=false;
    guide.Toggle();
    rig.SetOrbital(true);
    if(guide.IsActive || guide.OutlineVisible || g.SaveToJson()!=before)
        throw new System.InvalidOperationException("Orbital handoff retained the build tool or mutated canonical state.");
    return new {readOnlyPreview=true,resourceReason=true,exactCanonicalCell=true,noCollider=true,
        nineChoices=true,primaryBuildNotFire=true,unchangedCost=true,pauseAndViewCancel=true,sameSimulation=true};
}
finally
{
    adapter.InputSuppressed=true;
    rig.SetOrbital(oldOrbital);
    if(!g.TryLoadFromJson(original) || !object.ReferenceEquals(s,g.Simulation) || g.SaveToJson()!=original)
        throw new System.InvalidOperationException("Guide fixture did not restore exact state in place.");
    guide.Select(oldType);
    adapter.InputSuppressed=oldSuppressed;
}
