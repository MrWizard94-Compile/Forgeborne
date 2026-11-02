var g=UnityEngine.Object.FindFirstObjectByType<GameManager>();var flow=UnityEngine.Object.FindFirstObjectByType<PlayerFlow>();
var guide=UnityEngine.Object.FindFirstObjectByType<ConstructionGuide>();
if(!Application.isPlaying || g==null || g.Simulation==null || flow==null || !flow.IsMenuOpen || Time.timeScale!=0
    || guide==null || guide.IsActive || g.Simulation.World.Chunk.SurfaceProfile!=Forgeborne.Core.SurfaceProfile.AuthoredFoundryV2)
    throw new System.InvalidOperationException("Actual paused v2 Main with inactive build guide required.");
var s=g.Simulation;string original=g.SaveToJson();bool suppressed=g.FpsAdapter.InputSuppressed;var selected=guide.SelectedType;
var field=typeof(ConstructionGuide).GetField("_ghostRoot",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
try
{
    guide.Select(Forgeborne.Core.BlockType.Wall);
    var counts=new System.Collections.Generic.List<int>();var profiles=new[]{Forgeborne.Core.SurfaceProfile.AuthoredFoundryV2,
        Forgeborne.Core.SurfaceProfile.AuthoredFoundryV1,Forgeborne.Core.SurfaceProfile.AuthoredFoundryV2};
    for(int step=0;step<profiles.Length;step++)
    {
        if(step==1)Forgeborne.Core.AuthoredFoundry.Populate(s.World,profiles[step]);
        if(step==2 && !s.LoadFromJson(original))throw new System.InvalidOperationException("V2 snapshot refused during catalog lifecycle check.");
        s.World.Player.Inventory.Add(Forgeborne.Core.BlockType.Stone,100);
        s.Fps.Teleport(16.5f,4,9.5f);s.ApplyLook(-s.Look.Yaw,-35-s.Look.Pitch);s.AdvanceFrame(0,0,0,false);
        g.MeshRenderer.RefreshMesh();g.FpsAdapter.InputSuppressed=false;
        if(step==0)guide.Toggle();guide.RefreshPreview();
        var root=field.GetValue(guide) as GameObject;
        if(root==null || !root.activeSelf)throw new System.InvalidOperationException("Catalog switch lost the actual model-shaped preview.");
        var actual=root.GetComponentsInChildren<MeshFilter>();
        var expected=Forgeborne.Core.AuthoredFoundry.ConstructionTemplate(Forgeborne.Core.BlockType.Wall,profiles[step]);
        if(actual.Length!=expected.Count)throw new System.InvalidOperationException("Preview selected the wrong retained template component count.");
        int vertices=0;
        for(int part=0;part<actual.Length;part++)
        {
            var mesh=actual[part].sharedMesh;var data=expected[part].BuildMesh();vertices+=mesh.vertexCount;
            if(mesh.vertexCount!=data.Vertices.Count || !mesh.triangles.SequenceEqual(data.Indices))
                throw new System.InvalidOperationException("Preview kept mesh indices from a different content profile.");
            var points=mesh.vertices;
            for(int i=0;i<points.Length;i++)
            {
                var p=data.Vertices[i];
                if((points[i]-new Vector3(p.Item1,p.Item2,p.Item3)).sqrMagnitude>.000000000001f)
                    throw new System.InvalidOperationException("Preview kept vertex data from a different content profile.");
            }
        }
        counts.Add(vertices);
    }
    if(counts[0]!=counts[2] || counts[0]==counts[1])throw new System.InvalidOperationException("Distinct v1/v2 defensive plate fixtures did not reach the actual preview consumer.");
    return new{oneSimulation=true,profilesExercised=new[]{3,2,3},actualPreviewVertexCounts=counts,
        differentRetainedTemplatesReachConsumer=true,noSlotWrites=true,originalRecoveredInFinally=true};
}
finally
{
    guide.Cancel();
    if(!g.TryLoadFromJson(original) || g.SaveToJson()!=original || !object.ReferenceEquals(s,g.Simulation))
        throw new System.InvalidOperationException("Catalog lifecycle probe failed exact original-state recovery.");
    g.FpsAdapter.InputSuppressed=suppressed;
    guide.Select(selected);
}
