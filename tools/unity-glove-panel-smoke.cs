var manager=UnityEngine.Object.FindFirstObjectByType<GameManager>();
var flow=UnityEngine.Object.FindFirstObjectByType<PlayerFlow>();
var rig=UnityEngine.Object.FindFirstObjectByType<OrbitalCameraRig>();
if(!Application.isPlaying || manager==null || manager.Simulation==null || flow==null || !flow.IsMenuOpen
    || Time.timeScale!=0 || rig==null || rig.IsOrbital)
    throw new System.InvalidOperationException("Actual paused FPS Main required.");
var view=rig.ViewModel;var simulation=manager.Simulation;string snapshot=manager.SaveToJson();
var skins=view.CameraAnchor.GetComponentsInChildren<SkinnedMeshRenderer>(true);
var nodes=view.CameraAnchor.GetComponentsInChildren<Transform>(true);
var rows=new System.Collections.Generic.List<object>();
try
{
    foreach(string side in new[]{"Left","Right"})
    {
        view.ResetFeedbackPose();
        var skin=skins.Single(s=>s.name==side+"HandSkinned");
        var palm=nodes.Single(t=>t.name==side+"Palm");var elbow=nodes.Single(t=>t.name==side+"Forearm");
        var thumb=nodes.Single(t=>t.name==side+"Thumb0");var thumbRest=thumb.localRotation;
        var direction=(palm.position-elbow.position).normalized;
        int role=System.Array.FindIndex(skin.sharedMaterials,m=>m.name=="SleeveFabric");
        int palmIndex=System.Array.FindIndex(skin.bones,b=>b==palm);
        if(role<0 || palmIndex<0)throw new System.InvalidOperationException("Original panel/palm consumer missing.");
        // Select actual cloth above the wrist in world metres, independently of
        // its weights and the authoring recipe. The sleeve lies behind the wrist.
        var sourceVertices=skin.sharedMesh.vertices;
        var indices=skin.sharedMesh.GetTriangles(role).Distinct().Where(i=>
            Vector3.Dot(skin.transform.TransformPoint(sourceVertices[i])-palm.position,direction)>.008f).ToArray();
        if(indices.Length<50)throw new System.InvalidOperationException("Actual dorsal panel selection is empty or incomplete.");
        var weights=skin.sharedMesh.boneWeights;float nonPalm=0;
        foreach(int i in indices)
        {
            var w=weights[i];float other=0;
            if(w.boneIndex0!=palmIndex)other+=w.weight0;
            if(w.boneIndex1!=palmIndex)other+=w.weight1;
            if(w.boneIndex2!=palmIndex)other+=w.weight2;
            if(w.boneIndex3!=palmIndex)other+=w.weight3;
            nonPalm=Mathf.Max(nonPalm,other);
        }
        var mesh=new Mesh();var rest=new Vector3[indices.Length];float error=0,thumbAngle=0;
        try
        {
            skin.BakeMesh(mesh);var vertices=mesh.vertices;
            for(int i=0;i<indices.Length;i++)rest[i]=palm.InverseTransformPoint(skin.transform.TransformPoint(vertices[indices[i]]));
            foreach(float phase in new[]{.16f,.40f,.58f,.76f,.94f,1f})
            {
                view.ResetFeedbackPose();view.ReactReload();view.AdvancePresentation(WeaponViewModel.ReloadPresentationSeconds*phase);
                skin.BakeMesh(mesh);vertices=mesh.vertices;
                for(int i=0;i<indices.Length;i++)error=Mathf.Max(error,Vector3.Distance(rest[i],
                    palm.InverseTransformPoint(skin.transform.TransformPoint(vertices[indices[i]]))));
                thumbAngle=Mathf.Max(thumbAngle,Quaternion.Angle(thumbRest,thumb.localRotation));
            }
        }
        finally {UnityEngine.Object.DestroyImmediate(mesh);}
        if(nonPalm>.00001f || error>.00002f || side=="Left" && thumbAngle<50)
            throw new System.InvalidOperationException(side+" panel pinching/thumb articulation contract failed: "+nonPalm+" / "+error+" / "+thumbAngle);
        rows.Add(new{side=side,actualImportedPanelVertices=indices.Length,maximumNonPalmWeight=nonPalm,
            maximumPalmLocalBakedPanelDisplacementMetres=error,maximumThumb0LocalRotationDegrees=thumbAngle});
    }
    return new{version=Application.version,hands=rows,sameSimulation=object.ReferenceEquals(simulation,manager.Simulation),
        canonicalStateUnchanged=manager.SaveToJson()==snapshot,
        scope="Actual imported weights and CPU-baked skin across reload phases. Rigid palm panel stability prevents thumb-driven corner spikes; not dynamic cloth, final anatomy, realtime performance or director art/fun acceptance."};
}
finally
{
    view.ResetFeedbackPose();
    if(manager.SaveToJson()!=snapshot || !object.ReferenceEquals(simulation,manager.Simulation) || Time.timeScale!=0)
        throw new System.InvalidOperationException("Panel probe changed canonical state, ownership or pause.");
}
