var g=UnityEngine.Object.FindFirstObjectByType<GameManager>();
var flow=UnityEngine.Object.FindFirstObjectByType<PlayerFlow>();
if(!Application.isPlaying || g==null || g.Simulation==null || flow==null || !flow.IsMenuOpen || Time.timeScale!=0
    || g.Simulation.World.Chunk.SurfaceProfile!=Forgeborne.Core.SurfaceProfile.AuthoredFoundryV2)
    throw new System.InvalidOperationException("Started paused authored Main required.");
var s=g.Simulation;var adapter=g.FpsAdapter;var guide=adapter.Construction;
string original=g.SaveToJson();bool suppressed=adapter.InputSuppressed;var selected=guide.SelectedType;
try
{
    s.Fps.Teleport(16.5f,4,7.5f);s.AdvanceFrame(0,0,0,false);s.ApplyLook(-s.Look.Yaw,-40-s.Look.Pitch);
    adapter.InputSuppressed=false;guide.Cancel();guide.Toggle();
    int choices=0,pieces=0;
    foreach(var type in new[]{Forgeborne.Core.BlockType.Wall,Forgeborne.Core.BlockType.Workstation,Forgeborne.Core.BlockType.KineticMill,
        Forgeborne.Core.BlockType.Dynamo,Forgeborne.Core.BlockType.Transformer,Forgeborne.Core.BlockType.Cable,
        Forgeborne.Core.BlockType.Alternator,Forgeborne.Core.BlockType.ShockPylon,Forgeborne.Core.BlockType.Conduit})
    {
        guide.Select(type);string before=g.SaveToJson();guide.RefreshPreview();
        var preview=s.InspectBuildAhead(type);
        var ghost=g.MeshRenderer.GetComponentsInChildren<Transform>().FirstOrDefault(t=>t.name=="Authored construction preview");
        if(!preview.HasCell || !guide.OutlineVisible || ghost==null || g.SaveToJson()!=before
            || ghost.position!=new Vector3(preview.X,preview.Y,preview.Z) || ghost.rotation!=Quaternion.identity
            || ghost.GetComponentsInChildren<Collider>().Length!=0)
            throw new System.InvalidOperationException("Authored guide lost canonical placement, read-only state, world orientation or no-collider ownership.");
        var filters=ghost.GetComponentsInChildren<MeshFilter>();var expected=Forgeborne.Core.AuthoredFoundry.ConstructionTemplate(type,s.World.Chunk.SurfaceProfile);
        if(filters.Length!=expected.Count)throw new System.InvalidOperationException("Actual machine preview component count mismatch.");
        for(int i=0;i<filters.Length;i++)
        {
            var data=expected[i].BuildMesh();var mesh=filters[i].sharedMesh;var vertices=mesh.vertices;
            if(vertices.Length!=data.Vertices.Count || !mesh.triangles.SequenceEqual(data.Indices))throw new System.InvalidOperationException("Actual ghost triangle mismatch.");
            for(int j=0;j<vertices.Length;j++)
            {var v=data.Vertices[j];if(vertices[j]!=new Vector3(v.Item1,v.Item2,v.Item3))throw new System.InvalidOperationException("Actual ghost vertex mismatch.");}
            var material=filters[i].GetComponent<MeshRenderer>().sharedMaterial;
            if(material.GetFloat("_Surface")!=1 || material.GetFloat("_ZWrite")!=0 || Mathf.Abs(material.GetColor("_BaseColor").a-.30f)>.001f)
                throw new System.InvalidOperationException("Actual construction ghost is not the configured transparent consumer.");
        }
        if(guide.GetComponentsInChildren<LineRenderer>().Any(l=>l.name=="Construction cell outline" && l.enabled))
            throw new System.InvalidOperationException("Old cube outline is still visible in the authored world.");
        choices++;pieces+=filters.Length;
    }
    guide.Select(Forgeborne.Core.BlockType.Workstation);
    s.World.Player.Inventory.Add(Forgeborne.Core.BlockType.Stone,50);s.World.Player.Inventory.Add(Forgeborne.Core.BlockType.EmberOre,50);
    guide.RefreshPreview();var target=s.InspectBuildAhead(Forgeborne.Core.BlockType.Workstation);
    if(target.Result!=Forgeborne.Core.BuildResult.Success || !guide.Label.Contains("READY"))throw new System.InvalidOperationException("Funded machine preview refused valid floor placement.");
    int built=s.BlocksBuilt,ammo=s.Weapon.Mag;
    typeof(FpsInputAdapter).GetMethod("PrimaryAction",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(adapter,null);
    if(s.BlocksBuilt!=built+1 || s.Weapon.Mag!=ammo || s.World.Chunk.Get(target.X,target.Y,target.Z)!=Forgeborne.Core.BlockType.Workstation
        || s.World.Player.Inventory.Get(Forgeborne.Core.BlockType.Stone)!=40 || s.World.Player.Inventory.Get(Forgeborne.Core.BlockType.EmberOre)!=45)
        throw new System.InvalidOperationException("Actual primary action lost machine placement/payment or fired during construction.");
    string placed=g.SaveToJson();flow.OpenMenu();
    if(guide.OutlineVisible || guide.IsActive || g.SaveToJson()!=placed)throw new System.InvalidOperationException("Pause did not cancel the authored ghost without mutation.");
    return new{mainStartedAuthored=g.UseAuthoredFoundry && !g.UseCourtyardEncounter,nineOriginalChoices=choices,exactPreviewPieces=pieces,
        actualTransparentMaterial=true,previewMatchesPlacedSource=true,noCubeOutline=true,noExtraColliders=true,
        readOnlyPreview=true,productionPrimaryBuild=true,unchangedPrice=true,pauseCancelsGhost=true,sameSimulation=true,
        limits="Paused scripted production routing; actual keyboard/native/performance/fun remain open."};
}
finally
{
    guide.Cancel();adapter.InputSuppressed=true;
    if(!g.TryLoadFromJson(original) || g.SaveToJson()!=original || !object.ReferenceEquals(s,g.Simulation))
        throw new System.InvalidOperationException("Authored guide did not restore the exact original Main state in place.");
    guide.Select(selected);adapter.InputSuppressed=suppressed;
}
