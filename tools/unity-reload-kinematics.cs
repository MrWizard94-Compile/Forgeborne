var manager=UnityEngine.Object.FindFirstObjectByType<GameManager>();
var flow=UnityEngine.Object.FindFirstObjectByType<PlayerFlow>();
var rig=UnityEngine.Object.FindFirstObjectByType<OrbitalCameraRig>();
if(!Application.isPlaying || manager==null || manager.Simulation==null || flow==null || !flow.IsMenuOpen
    || Time.timeScale!=0 || rig==null || rig.IsOrbital)
    throw new System.InvalidOperationException("Actual paused FPS Main required.");
var view=rig.ViewModel;var camera=manager.FpsAdapter.GetComponent<Camera>();
var nodes=view.CameraAnchor.GetComponentsInChildren<Transform>(true);
var wrist=nodes.Single(t=>t.name=="LeftPalm");var elbow=nodes.Single(t=>t.name=="LeftForearm");
var marker=nodes.Single(t=>t.name=="LeftPalmContact");
var simulation=manager.Simulation;string snapshot=manager.SaveToJson();
try
{
    view.ResetFeedbackPose();var contact=wrist.InverseTransformPoint(marker.position);
    float duration=WeaponViewModel.ReloadPresentationSeconds;
    int intervals=Mathf.CeilToInt(duration*240);float step=duration/intervals;
    var positions=new Vector3[intervals+1];var wristPositions=new Vector3[intervals+1];
    float length=Vector3.Distance(wrist.position,elbow.position),lengthError=0;
    for(int i=0;i<=intervals;i++)
    {
        view.ResetFeedbackPose();view.ReactReload();view.AdvancePresentation(duration*i/intervals);
        positions[i]=camera.transform.InverseTransformPoint(wrist.TransformPoint(contact));
        wristPositions[i]=camera.transform.InverseTransformPoint(wrist.position);
        lengthError=Mathf.Max(lengthError,Mathf.Abs(Vector3.Distance(wrist.position,elbow.position)-length));
    }
    float speed=0,wristSpeed=0,acceleration=0,speedTime=0,accelerationTime=0;
    Vector3 previousVelocity=Vector3.zero;
    for(int i=1;i<=intervals;i++)
    {
        var velocity=(positions[i]-positions[i-1])/step;
        float value=velocity.magnitude;
        if(value>speed){speed=value;speedTime=(i-.5f)*step;}
        wristSpeed=Mathf.Max(wristSpeed,Vector3.Distance(wristPositions[i],wristPositions[i-1])/step);
        if(i>1)
        {
            value=(velocity-previousVelocity).magnitude/step;
            if(value>acceleration){acceleration=value;accelerationTime=(i-1)*step;}
        }
        previousVelocity=velocity;
    }
    return new{version=Application.version,durationSeconds=duration,sampleHz=240,intervals=intervals,actualStepSeconds=step,
        cameraSpacePalmPeakSpeedMetresPerSecond=speed,peakSpeedTimeSeconds=speedTime,
        cameraSpaceWristPeakSpeedMetresPerSecond=wristSpeed,
        cameraSpacePalmPeakAccelerationMetresPerSecondSquared=acceleration,peakAccelerationTimeSeconds=accelerationTime,
        maximumForearmLengthErrorMetres=lengthError,canonicalStateUnchanged=manager.SaveToJson()==snapshot,
        sameSimulation=object.ReferenceEquals(simulation,manager.Simulation),
        scope="Actual rig finite differences at fixed elapsed-time intervals. Camera-space contact/wrist references, not visible pixels, human biomechanics limits, native performance or natural-motion/fun acceptance."};
}
finally
{
    view.ResetFeedbackPose();
    if(manager.SaveToJson()!=snapshot || !object.ReferenceEquals(simulation,manager.Simulation) || Time.timeScale!=0)
        throw new System.InvalidOperationException("Kinematics probe changed canonical state, ownership or pause.");
}
