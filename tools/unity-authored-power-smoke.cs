var g=UnityEngine.Object.FindFirstObjectByType<GameManager>();
var flow=UnityEngine.Object.FindFirstObjectByType<PlayerFlow>();
if(!Application.isPlaying || g==null || g.Simulation==null || flow==null || !flow.IsMenuOpen || Time.timeScale!=0
    || g.Simulation.World.Chunk.SurfaceProfile!=Forgeborne.Core.SurfaceProfile.AuthoredFoundryV1)
    throw new System.InvalidOperationException("Paused authored Main required.");
var s=g.Simulation;string original=g.SaveToJson();
try
{
    s.World.Player.Inventory.Add(Forgeborne.Core.BlockType.Stone,50);s.World.Player.Inventory.Add(Forgeborne.Core.BlockType.EmberOre,50);
    if(s.World.Build(16,4,13,Forgeborne.Core.BlockType.Workstation)!=Forgeborne.Core.BuildResult.Success)
        throw new System.InvalidOperationException("Workstation fixture placement failed.");
    g.MeshRenderer.RefreshMesh();var first=s.World.Chunk.Geometry;
    var region=first.Regions.First(r=>r.Label=="Console worktop");
    var renderer=g.MeshRenderer.GetComponentsInChildren<MeshRenderer>().Single(r=>r.name.EndsWith("["+region.Id+"]"));
    var properties=new MaterialPropertyBlock();renderer.GetPropertyBlock(properties);var before=properties.GetColor("_BaseColor");
    if(s.World.Build(17,4,13,Forgeborne.Core.BlockType.Dynamo)!=Forgeborne.Core.BuildResult.Success)
        throw new System.InvalidOperationException("Dynamo fixture placement failed.");
    g.MeshRenderer.RefreshMesh();var powered=s.World.Chunk.Geometry;
    if(!object.ReferenceEquals(region,powered.Regions.Single(r=>r.Id==region.Id)))
        throw new System.InvalidOperationException("Power-only change rebuilt the unchanged workstation geometry.");
    renderer.GetPropertyBlock(properties);var actual=properties.GetColor("_BaseColor");
    var tint=region.BuildColors(s.World.PowerState)[0];
    var expected=(renderer.sharedMaterial.GetColor("_BaseColor").linear*new Color(tint.Item1,tint.Item2,tint.Item3,tint.Item4)).gamma;
    if(actual==before || actual!=expected || !s.World.IsWorkstationPowered(16,4,13))
        throw new System.InvalidOperationException("Authored powered tint did not reach the actual industrial material property consumer.");
    s.World.Chunk.TrySet(17,4,13,Forgeborne.Core.BlockType.Air);g.MeshRenderer.RefreshMesh();renderer.GetPropertyBlock(properties);
    if(properties.GetColor("_BaseColor")!=before || s.World.IsWorkstationPowered(16,4,13))
        throw new System.InvalidOperationException("Removed power source left stale authored material state.");
    return new{actualIndustrialMaterialTint=true,unchangedMeshReused=true,powerSourceRemovalRestoresTint=true,sameSimulation=true};
}
finally
{
    if(!g.TryLoadFromJson(original) || g.SaveToJson()!=original || !object.ReferenceEquals(s,g.Simulation))
        throw new System.InvalidOperationException("Authored power fixture failed exact original-state recovery.");
}
