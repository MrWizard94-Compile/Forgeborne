var manager=UnityEngine.Object.FindFirstObjectByType<GameManager>();
var flow=UnityEngine.Object.FindFirstObjectByType<PlayerFlow>();
var rig=UnityEngine.Object.FindFirstObjectByType<OrbitalCameraRig>();
if(!Application.isPlaying || manager==null || manager.Simulation==null || flow==null || !flow.IsMenuOpen
    || Time.timeScale!=0 || rig==null || rig.IsOrbital)
    throw new System.InvalidOperationException("Paused actual FPS Main required.");
var view=rig.ViewModel;var camera=manager.FpsAdapter.GetComponent<Camera>();
string snapshot=manager.SaveToJson();var simulation=manager.Simulation;
view.ResetFeedbackPose();view.AdvancePresentation(0);
var transforms=view.CameraAnchor.GetComponentsInChildren<Transform>(true);
var leftElbow=transforms.Single(t=>t.name=="LeftForearm");var leftWrist=transforms.Single(t=>t.name=="LeftPalm");
var rightElbow=transforms.Single(t=>t.name=="RightForearm");var rightWrist=transforms.Single(t=>t.name=="RightPalm");
var rows=new System.Collections.Generic.List<object>();
foreach(var bone in new[]{leftElbow,leftWrist,rightElbow,rightWrist})
{
    var local=camera.transform.InverseTransformPoint(bone.position);var viewport=camera.WorldToViewportPoint(bone.position);
    rows.Add(new{name=bone.name,cameraPosition=new[]{local.x,local.y,local.z},viewport=new[]{viewport.x,viewport.y,viewport.z}});
}
if(snapshot!=manager.SaveToJson() || !object.ReferenceEquals(simulation,manager.Simulation))
    throw new System.InvalidOperationException("Read-only arm observation changed canonical ownership/state.");
return new{version=Application.version,fieldOfView=camera.fieldOfView,viewOffset=view.ViewOffset.ToString(),
    viewAngles=view.ViewAngles.ToString(),bones=rows,leftForearmMetres=Vector3.Distance(leftElbow.position,leftWrist.position),
    rightForearmMetres=Vector3.Distance(rightElbow.position,rightWrist.position),sameSimulation=true,canonicalStateUnchanged=true,
    scope="Actual rest rig camera projections; visible sleeve silhouette and art quality require rendered review."};
