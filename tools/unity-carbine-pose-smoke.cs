var g=UnityEngine.Object.FindFirstObjectByType<GameManager>();var flow=UnityEngine.Object.FindFirstObjectByType<PlayerFlow>();
var rig=UnityEngine.Object.FindFirstObjectByType<OrbitalCameraRig>();
if(!Application.isPlaying || g==null || g.Simulation==null || flow==null || !flow.IsMenuOpen || Time.timeScale!=0 || rig==null)
    throw new System.InvalidOperationException("Paused initialized actual Main required.");
var simulation=g.Simulation;string before=g.SaveToJson();bool oldMode=rig.IsOrbital;
var view=rig.ViewModel;
var field=typeof(WeaponViewModel).GetField("_modelRoot",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
var root=(GameObject)field.GetValue(view);
var skins=root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
var left=skins.Single(s=>s.name=="LeftHandSkinned");var right=skins.Single(s=>s.name=="RightHandSkinned");
var transforms=root.GetComponentsInChildren<Transform>(true);
var wrist=transforms.Single(t=>t.name=="LeftPalm");var elbow=transforms.Single(t=>t.name=="LeftForearm");
var magazine=transforms.Single(t=>t.name=="Magazine");
System.Func<SkinnedMeshRenderer,Vector3[]> bake=skin=>{
    var mesh=new Mesh();try{skin.BakeMesh(mesh);return mesh.vertices;}finally{UnityEngine.Object.DestroyImmediate(mesh);}
};
System.Func<Vector3[],Vector3[],float> difference=(a,b)=>{
    if(a.Length!=b.Length)throw new System.InvalidOperationException("Presentation replaced original skin topology.");
    float maximum=0;for(int i=0;i<a.Length;i++)maximum=Mathf.Max(maximum,Vector3.Distance(a[i],b[i]));return maximum;
};
try
{
    rig.SetOrbital(false);view.ResetFeedbackPose();
    var leftRest=bake(left);var rightRest=bake(right);float originalLength=Vector3.Distance(wrist.position,elbow.position);
    float rightLength=Vector3.Distance(transforms.Single(t=>t.name=="RightPalm").position,transforms.Single(t=>t.name=="RightForearm").position);
    var fill=view.CameraAnchor.GetComponentsInChildren<Light>(true).Single(l=>l.name=="Weapon diffuse fill");
    if(originalLength<.24f || originalLength>.32f || rightLength<.24f || rightLength>.32f
        || fill.cullingMask!=(1<<30) || fill.shadows!=LightShadows.None || !fill.enabled)
        throw new System.InvalidOperationException("Arm proportions or view-only light isolation failed.");
    var magazineRest=magazine.localPosition;var bones=skins.SelectMany(s=>s.bones).Distinct().ToArray();
    var bonePositions=bones.Select(b=>b.localPosition).ToArray();var boneRotations=bones.Select(b=>b.localRotation).ToArray();
    var samples=new System.Collections.Generic.List<object>();float maxLeft=0,maxRight=0,maxLengthError=0;
    float duration=WeaponViewModel.ReloadPresentationSeconds;
    var times=new[]{0f,.15f*duration,.35f*duration,.50f*duration,.70f*duration,.84f*duration,duration};
    foreach(float time in times)
    {
        view.ResetFeedbackPose();view.ReactReload();view.AdvancePresentation(time);
        float leftChange=difference(leftRest,bake(left)),rightChange=difference(rightRest,bake(right));
        float lengthError=Mathf.Abs(Vector3.Distance(wrist.position,elbow.position)-originalLength);
        maxLeft=Mathf.Max(maxLeft,leftChange);maxRight=Mathf.Max(maxRight,rightChange);maxLengthError=Mathf.Max(maxLengthError,lengthError);
        if(rightChange>.00002f || lengthError>.00002f)throw new System.InvalidOperationException("Left-hand presentation changed the other glove or stretched its forearm.");
        var frozen=bake(left);view.AdvancePresentation(0);
        if(difference(frozen,bake(left))>.000001f)throw new System.InvalidOperationException("Zero presentation time moved the paused glove.");
        samples.Add(new{seconds=time,leftVertexDisplacementMetres=leftChange,rightVertexDisplacementMetres=rightChange,
            forearmLengthErrorMetres=lengthError,magazineRootPosition=root.transform.InverseTransformPoint(magazine.position).ToString()});
    }
    if(maxLeft<.08f || maxRight>.00002f || difference(leftRest,bake(left))>.00002f)
        throw new System.InvalidOperationException("Actual reload skin deformation or final rest recovery failed.");
    view.ResetFeedbackPose();
    for(int i=0;i<bones.Length;i++)if(!bones[i].localPosition.Equals(bonePositions[i]) || !bones[i].localRotation.Equals(boneRotations[i]))
        throw new System.InvalidOperationException("Reset did not restore exact authored bone rest transforms.");
    if(!magazine.localPosition.Equals(magazineRest))throw new System.InvalidOperationException("Reset lost the magazine's exact local rest pivot.");
    view.ReactShot();view.AdvancePresentation(0);
    if(Vector3.Distance(view.MuzzlePosition,transforms.Single(t=>t.name=="MuzzleSocket").position)>.000001f)
        throw new System.InvalidOperationException("Recoil disconnected the actual muzzle landmark.");
    float triggerMovement=difference(rightRest,bake(right));
    if(triggerMovement<.0001f)throw new System.InvalidOperationException("Actual trigger finger did not deform under recoil feedback.");
    view.ResetFeedbackPose();
    for(int i=0;i<20;i++)view.AdvancePresentation(1f/120);
    var timer=new System.Diagnostics.Stopwatch();long allocated=System.GC.GetAllocatedBytesForCurrentThread();timer.Start();
    int reloadInterval=Mathf.CeilToInt(WeaponViewModel.ReloadPresentationSeconds*120)+24;
    for(int i=0;i<600;i++){if(i%reloadInterval==0)view.ReactReload();view.AdvancePresentation(1f/120);}
    timer.Stop();allocated=System.GC.GetAllocatedBytesForCurrentThread()-allocated;
    if(allocated>256)throw new System.InvalidOperationException("Weapon presentation allocated during warm frame updates: "+allocated);
    view.ResetFeedbackPose();rig.SetOrbital(true);
    if(root.activeSelf || fill.enabled)throw new System.InvalidOperationException("Orbital handoff left the camera-local rig/light active.");
    rig.SetOrbital(false);
    return new{oneSimulation=object.ReferenceEquals(simulation,g.Simulation),originalBoneCount=bones.Length,skins=skins.Length,
        actualLeftHandDeformation=true,noCrossHandBinding=true,forearmLengthPreserved=true,pausedPoseStable=true,exactRestReset=true,
        triggerFingerDisplacementMetres=triggerMovement,orbitalVisibility=true,maximumForearmLengthError=maxLengthError,samples=samples,
        leftForearmLengthMetres=originalLength,rightForearmLengthMetres=rightLength,fillExcludesWorldLayers=true,
        warmPresentationUpdates=600,warmReloadIntervalUpdates=reloadInterval,warmPresentationMilliseconds=timer.Elapsed.TotalMilliseconds,warmPresentationAllocatedBytes=allocated,
        scope="Actual production rig and CPU-baked skin consumers in paused Main. GPU deformation requires the separate frame-boundary captures. Warm presentation-only timing excludes rendering/skinning/platform overhead and is not representative native performance or fun acceptance."};
}
finally
{
    view.ResetFeedbackPose();rig.SetOrbital(oldMode);
    if(g.SaveToJson()!=before || !object.ReferenceEquals(simulation,g.Simulation) || Time.timeScale!=0)
        throw new System.InvalidOperationException("Carbine presentation probe changed canonical state, ownership or pause.");
}
