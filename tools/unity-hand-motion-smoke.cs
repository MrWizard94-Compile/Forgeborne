var g=UnityEngine.Object.FindFirstObjectByType<GameManager>();
var flow=UnityEngine.Object.FindFirstObjectByType<PlayerFlow>();
var rig=UnityEngine.Object.FindFirstObjectByType<OrbitalCameraRig>();
if(!Application.isPlaying || g==null || g.Simulation==null || flow==null || !flow.IsMenuOpen || Time.timeScale!=0 || rig==null || rig.IsOrbital)
    throw new System.InvalidOperationException("Actual paused FPS Main required.");
var view=rig.ViewModel;var simulation=g.Simulation;string snapshot=g.SaveToJson();
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var root=((GameObject)typeof(WeaponViewModel).GetField("_modelRoot",flags).GetValue(view)).transform;
var nodes=root.GetComponentsInChildren<Transform>(true);
var palm=nodes.Single(t=>t.name=="LeftPalm");var elbow=nodes.Single(t=>t.name=="LeftForearm");
var hook=nodes.Single(t=>t.name=="LeftIndex2");var contact=nodes.Single(t=>t.name=="LeftPalmContact");
var magazine=nodes.Single(t=>t.name=="Magazine");var slide=nodes.Single(t=>t.name=="ChargingHandle");
var bones=root.GetComponentsInChildren<SkinnedMeshRenderer>(true).SelectMany(s=>s.bones).Distinct().ToArray();
var skins=root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
var left=skins.Single(s=>s.name=="LeftHandSkinned");var right=skins.Single(s=>s.name=="RightHandSkinned");
System.Func<SkinnedMeshRenderer,Vector3[]> bake=skin=>{var mesh=new Mesh();try{skin.BakeMesh(mesh);return mesh.vertices;}finally{UnityEngine.Object.DestroyImmediate(mesh);}};
System.Func<Vector3[],Vector3[],float> error=(a,b)=>{float m=0;if(a.Length!=b.Length)throw new System.InvalidOperationException("Topology replaced.");for(int i=0;i<a.Length;i++)m=Mathf.Max(m,Vector3.Distance(a[i],b[i]));return m;};
try
{
    view.ResetFeedbackPose();var localContact=palm.InverseTransformPoint(contact.position);
    var restPositions=bones.Select(b=>b.localPosition).ToArray();var restRotations=bones.Select(b=>b.localRotation).ToArray();
    var leftRest=bake(left);var rightRest=bake(right);var magRest=magazine.localPosition;var slideRest=slide.localPosition;
    float forearmLength=Vector3.Distance(palm.position,elbow.position),duration=WeaponViewModel.ReloadPresentationSeconds;
    float magError=0,hookError=0,lengthError=0,rightError=0,maxStep=0,maxSlideTravel=0;
    int magSamples=0,hookSamples=0;var records=new System.Collections.Generic.List<object>();Vector3 previous=Vector3.zero;
    // A dense sweep checks imported consumers independently of the pose solver.
    for(int i=0;i<=240;i++)
    {
        float phase=i/240f;view.ResetFeedbackPose();view.ReactReload();view.AdvancePresentation(duration*phase);
        var p=root.InverseTransformPoint(palm.TransformPoint(localContact));
        var h=root.InverseTransformPoint(hook.position);
        var m=root.InverseTransformPoint(magazine.position);var c=root.InverseTransformPoint(slide.position);
        if(phase>=.18f && phase<=.50f){magSamples++;magError=Mathf.Max(magError,Vector3.Distance(p,m+new Vector3(-.047f,-.035f,.002f)));}
        if(phase>=.68f && phase<=.84f){hookSamples++;hookError=Mathf.Max(hookError,Vector3.Distance(h,c+new Vector3(-.025f,-.002f,.027f)));}
        lengthError=Mathf.Max(lengthError,Mathf.Abs(Vector3.Distance(palm.position,elbow.position)-forearmLength));
        maxSlideTravel=Mathf.Max(maxSlideTravel,Vector3.Distance(slide.localPosition,slideRest));
        if(i>0)maxStep=Mathf.Max(maxStep,Vector3.Distance(p,previous));previous=p;
        if(i%20==0)
        {
            rightError=Mathf.Max(rightError,error(rightRest,bake(right)));
            var frozen=bake(left);view.AdvancePresentation(0);
            if(error(frozen,bake(left))>.000002f)throw new System.InvalidOperationException("Paused hand moved.");
            records.Add(new{phase=phase,seconds=duration*phase,palm=p.ToString(),hook=h.ToString(),magazine=m.ToString(),slide=c.ToString()});
        }
    }
    if(magSamples<75 || hookSamples<38 || magError>.00002f || hookError>.00002f || lengthError>.00002f || rightError>.00002f
        || maxStep>.04f || Mathf.Abs(maxSlideTravel-.035f)>.00002f || error(leftRest,bake(left))>.00002f)
        throw new System.InvalidOperationException("Dense reload contact/continuity/skin contract failed: "+magError+" / "+hookError+" / "+maxStep);
    // Same elapsed moment from different update cadences, compared on actual skin.
    float cadenceError=0;
    foreach(float phase in new[]{.16f,.38f,.55f,.70f,.82f,.94f,1f})
    {
        view.ResetFeedbackPose();view.ReactReload();view.AdvancePresentation(duration*phase);var reference=bake(left);
        foreach(int hz in new[]{30,60,144})
        {
            view.ResetFeedbackPose();view.ReactReload();double elapsed=0,target=(double)(duration*phase),step=1.0/hz;
            while(elapsed<target){double delta=System.Math.Min(step,target-elapsed);view.AdvancePresentation((float)delta);elapsed+=delta;}
            cadenceError=Mathf.Max(cadenceError,error(reference,bake(left)));
        }
    }
    if(cadenceError>.00002f)throw new System.InvalidOperationException("Elapsed-time cadence changed the skin pose: "+cadenceError);
    view.ResetFeedbackPose();view.ReactReload();view.AdvancePresentation(duration*.84f);view.ReactShot();view.AdvancePresentation(0);
    if(Vector3.Distance(magazine.localPosition,magRest)>.000002f || Vector3.Distance(slide.localPosition,slideRest)>.000002f
        || error(leftRest,bake(left))>.00002f)throw new System.InvalidOperationException("Shot interruption stranded the support hand or an assembly.");
    view.ResetFeedbackPose();
    for(int i=0;i<bones.Length;i++)if(!bones[i].localPosition.Equals(restPositions[i]) || !bones[i].localRotation.Equals(restRotations[i]))
        throw new System.InvalidOperationException("Exact imported bone rest was lost.");
    if(!magazine.localPosition.Equals(magRest) || !slide.localPosition.Equals(slideRest))throw new System.InvalidOperationException("Exact local assembly reset was lost.");
    return new{sameSimulation=object.ReferenceEquals(simulation,g.Simulation),reloadPresentationSeconds=duration,denseSamples=241,
        magazineContactSamples=magSamples,maximumPalmMagazineErrorMetres=magError,chargingContactSamples=hookSamples,
        maximumHookSlideErrorMetres=hookError,maximumForearmLengthErrorMetres=lengthError,maximumOtherHandSkinErrorMetres=rightError,
        maximumPalmStepAt240IntervalsMetres=maxStep,chargingSlideTravelMetres=maxSlideTravel,
        cadenceHz=new[]{30,60,144},maximumCadenceSkinErrorMetres=cadenceError,pausedStable=true,shotInterruptionReturnsSupportHand=true,
        exactImportedRestReset=true,canonicalStateUnchanged=g.SaveToJson()==snapshot,samples=records,
        limits="Scripted actual rig/CPU skin. Joint/palm references do not prove visible surface contact or final animation quality. Separate rendered/native and director review required. Cosmetic reload only; Core transfer remains immediate."};
}
finally
{
    view.ResetFeedbackPose();
    if(g.SaveToJson()!=snapshot || !object.ReferenceEquals(simulation,g.Simulation) || Time.timeScale!=0)
        throw new System.InvalidOperationException("Reload motion probe changed canonical state, ownership or pause.");
}
